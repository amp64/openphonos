using OpenPhonos.UPnP;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace OpenPhonos.Sonos
{
    public class PlayerEventSelector
    {
        public bool Device;
        public bool Zone;
        public bool Renderer;
        public bool GroupRenderer;
        public bool ContentDirectory;
    }

    public enum PlayType
    {
        PlayNow,        // End of queue (if queueable), Seek then Play it, else just Play
        PlayNext,       // Add to one after current queue (must be queueable)
        AddToQueue,     // Add to end of queue (must be queueable)
        ReplaceQueue,   // Delete queue and play (must be queueable)
        ReplaceQueueAndShuffle,
        Delete,         // Delete the item
        Favorite,       // Add to Favorites
    };

    /// <summary>
    /// These wrap actual Speaker devices (and non-Speaker Sonos devices)
    /// Equivalent to a Room in the Sonos controller
    /// </summary>
    [DebuggerDisplay("{RoomName,nq} ({FriendlyName,nq}) Invis={Invisible} {UniqueName,nq}")]
    public class Player
    {
        public enum Family
        {
            Unknown,
            Deactivated,
            Obsolete,
            Legacy,
            Modern,
            ModernOnly
        };

        [Flags]
        public enum Feature3Flags
        {
            Battery = 0x80
        };

        /// <summary>
        /// This is the name set by the user eg "Kitchen"
        /// </summary>
        public string RoomName { get; private set; }

        /// <summary>
        /// This is the unique name of the form RINCON_xxxxxxxxxxxxxx
        /// </summary>
        public string UniqueName { get; }

        /// <summary>
        /// This is the UPnP FriendlyName such as "192.168.1.2 Sonos One"
        /// </summary>
        public string FriendlyName { get; }

        /// <summary>
        /// This determines whether the Player is hidden from the user
        /// </summary>
        public bool Invisible { get; private set; }

        /// <summary>
        /// Is the player deactivated or not
        /// Can be null or "DEACTIVATED" or "PENDING"
        /// </summary>
        public string Deactivated { get; private set; }

        /// <summary>
        /// This is the relative path to the device's icon
        /// </summary>
        public string Icon { get; }

        /// <summary>
        /// The player's Model Name eg "Sonos One"
        /// </summary>
        public string ModelName { get; }

        /// <summary>
        /// The player's Model Number eg "S13"
        /// </summary>
        public string ModelNumber { get; }

        /// <summary>
        /// Is the device Home Theater-capable ie has a digital TV input
        /// </summary>
        public bool HTCapable { get; private set; }

        /// <summary>
        /// Serial number of the device
        /// </summary>
        public string SerialNumber { get; }

        /// <summary>
        /// Software version
        /// </summary>
        public string SoftwareVersion { get; }

        /// <summary>
        /// Hardware version
        /// </summary>
        public string HardwareVersion { get; }

        /// <summary>
        /// Series id which allows for different versions of the same model
        /// </summary>
        public string SeriesID { get; }

        /// <summary>
        /// Just the IP address, use for display purposes only
        /// </summary>
        public string IPAddress { get; }

        /// <summary>
        /// Memory of the device (in MB)
        /// </summary>
        public string Memory { get; }

        /// <summary>
        /// Flash storage size of the device (in MB)
        /// </summary>
        public string Flash { get; }

        /// <summary>
        /// Model name, eg "Play:1" or "Sonos One"
        /// </summary>
        public string DisplayName { get; }

        public string SoftwareGeneration { get; }
        public string MinCompatibleVersion { get; }
        public string WirelessMode { get; private set; }
        public string BatteryState { get; private set; }
        public int DeviceVolume { get; private set; }
        public bool HasVolume { get; private set; }
        public bool FixedVolume { get; private set; }
        public bool IsMuted { get; private set; }
        public bool IsMissing { get; private set; }
        public Feature3Flags Feature3 { get; }
        public bool HasBattery { get => (Feature3 & Feature3Flags.Battery) != 0; }

        public int GroupVolume { get; private set; }
        public bool GroupMuted { get; private set; }
        public bool GroupFixedVolume { get; private set; }

        /// <summary>
        /// Create a new Sonos Player device from a UPnP device
        /// Will throw if the device is not capable
        /// </summary>
        /// <param name="device">UPnP device</param>
        /// <returns>New Player object</returns>
        public static async Task<Player> CreatePlayerAsync(Device device)
        {
            // The UDN has a "uuid:" prefix which we never need
            var player = new Player(device, device.Attribute("roomName"), device.UDN.Substring(5), invisible: false);
            await player.InitializeAsync(device.IsRealDevice, true);
            return player;
        }

        public async Task SetVolumeAsync(int newVolume)
        {
            if (this.RenderingControl != null)
            {
                await this.RenderingControl.SetVolume(0, "Master", (ushort)newVolume);
            }
        }

        public async Task SetGroupVolumeAsync(int newVolume)
        {
            if (GroupRenderingControl != null)
            {
                await this.GroupRenderingControl.SetGroupVolume(0, (ushort)newVolume);
            }
        }

        public async Task<int> SetRelativeGroupVolumeAsync(int deltaVolume)
        {
            if (GroupRenderingControl != null)
            {
                var result = await this.GroupRenderingControl.SetRelativeGroupVolume(0, deltaVolume);
                result.ThrowIfFailed();
                return result.NewVolume;
            }

            return -1;      // indicates failure
        }

        public async Task SetMutedAsync(bool muted)
        {
            if (this.RenderingControl != null)
            {
                await this.RenderingControl.SetMute(0, "Master", muted);
            }
        }

        public async Task SetGroupMuted(bool muted)
        {
            if (this.GroupRenderingControl != null)
            {
                await this.GroupRenderingControl.SetGroupMute(0, muted);
            }
        }

        public void Rename(string newname, bool invisible)
        {
            // This sets the in-memory name only, does not physically change the name
            RoomName = newname;
            Invisible = invisible;
        }

        /// <summary>
        /// Create a new Sonos Player device from a UPnP device and a few other items
        /// Will throw if the device is not capable
        /// </summary>
        /// <param name="device">UPnP device</param>
        /// <param name="roomName">Room name</param>
        /// <param name="uniqueName">Unique name (RINCON_xxx)</param>
        /// <param name="invisible">Whether the player is invisible to the user</param>
        /// <returns>New Player object</returns>
        public static async Task<Player> CreatePlayerAsync(Device device, string roomName, string uniqueName, bool invisible)
        {
            var player = new Player(device, roomName, uniqueName, invisible);
            await player.InitializeAsync(device.IsRealDevice, false);
            return player;
        }

        public async Task<uint> QueueSize()
        {
            var queue = await ContentDirectory.Browse("Q:0", "BrowseDirectChildren", "*", 0, 1, "").Required();
            return queue.TotalMatches;
        }

        public async Task ClearQueue()
        {
            await AVTransport.RemoveAllTracksFromQueue(0).Required();
        }

        #region Play music item functions

        /// <summary>
        /// Play an Item (this Player must be a Coordinator for this to work)
        /// </summary>
        /// <param name="item">Must be UPnPMusicItem or MusicServiceItem</param>
        public async Task PlayItemAsync(MusicItem item, PlayType action)
        {
            string md = item.Metadata;

            if (string.IsNullOrEmpty(md) && item is UPnPMusicItem upnp)
            {
                // We have no metadata, so go get some
                if (upnp.Id != "spdif-input")
                {
                    var freshmd = await ContentDirectory.Browse(upnp.Id, "BrowseMetadata", "*", 0, 1, "").Required();
                    item.UpdateMetadata(freshmd.Result);
                }
                else
                {
                    item.UpdateRes(DidlData.MakeTV(this).Res);
                }
                md = item.Metadata;
            }

            if (!item.IsQueueable)
            {
                if (action != PlayType.PlayNow)
                    throw new ArgumentException(nameof(action), "Only Play Now is allowed for non-queueable items");

                // Play immediately, no queue actions required
                await AVTransport.SetAVTransportURI(0, item.Res, md).Required();
                await AVTransport.Play(0, "1").Required();
            }
            else
            {
                await PlayWithQueueAsync(item, action);
            }
        }

        private async Task PlayWithQueueAsync(MusicItem item, PlayType action)
        {
            uint whereToAdd = 0;
            bool queueAsNext;
            bool clearQueue = false;
            bool seekThenPlay = false;
            bool shuffle = false;

            switch (action)
            {
                case PlayType.PlayNow:
                    whereToAdd = 1;
                    queueAsNext = true;
                    seekThenPlay = true;
                    break;
                case PlayType.PlayNext:
                    {
                        var current = await AVTransport.GetPositionInfo(0);
                        current.ThrowIfFailed();
                        whereToAdd = current.Track + 1;
                        queueAsNext = true;
                    }
                    break;
                case PlayType.AddToQueue:
                    {
                        var current = await AVTransport.GetMediaInfo(0);
                        current.ThrowIfFailed();
                        queueAsNext = false;
                        whereToAdd = current.NrTracks + 1;
                    }
                    break;
                case PlayType.ReplaceQueue:
                case PlayType.ReplaceQueueAndShuffle:
                    whereToAdd = 1;
                    queueAsNext = true;
                    clearQueue = true;
                    seekThenPlay = true;
                    shuffle = action == PlayType.ReplaceQueueAndShuffle;
                    break;
                default:
                    Debug.Assert(false);
                    queueAsNext = false;
                    break;
            }

            if (clearQueue)
            {
                await AVTransport.RemoveAllTracksFromQueue(0).Optional();
            }

            // Failure or not we add then play
            await AVTransport.AddURIToQueue(0, item.Res, item.Metadata, whereToAdd, queueAsNext).Required();

            if (seekThenPlay)
            {
                await PlayFromQueueAsync(whereToAdd, shuffle);
            }
        }

        private async Task PlayFromQueueAsync(uint tracknum, bool shuffle)
        {
            var current = await AVTransport.GetMediaInfo(0).Required();
            var name = await ContentDirectory.Browse("Q:0", "BrowseMetadata", "*", 0, 1, "").Required();
            var queueName = DidlData.Parse(name.Result).First().Res;

            if (current.CurrentURI != queueName)
            {
                // Playing something not from a queue (eg Radio) so switch first
                await AVTransport.SetAVTransportURI(0, queueName, "").Required();
            }

            if (shuffle)
            {
                await AVTransport.SetPlayMode(0, "SHUFFLE").Required();
            }

            await AVTransport.Seek(0, "TRACK_NR", tracknum.ToString()).Required();
            await AVTransport.Play(0, "1").Required();
        }
#endregion

        /// <summary>
        /// Gets ZoneGroupState
        /// </summary>
        /// <returns>ZoneGroupState as an xml string</returns>
        public async Task<string> GetZoneGroupStateAsync()
        {
            var result = await this.ZoneGroupTopology.GetZoneGroupState();
            result.ThrowIfFailed();
            return result.ZoneGroupState;
        }

        public async Task<string> GetHTAudioInAsync()
        {
            var result = await this.DeviceProperties.GetZoneInfo();
            result.ThrowIfFailed();
            return ConvertDigitalFormat(result.HTAudioIn);
        }

        public async Task SubscribeToEventsAsync(int seconds, PlayerEventSelector which)
        {
            if (which.Renderer && this.RenderingControl != null)
            {
                await this.RenderingControl.SubscribeAsync(seconds, "Renderer", RendererSubscriptionHandler);
            }

            if (which.Device && this.DeviceProperties != null)
            {
                await this.DeviceProperties.SubscribeAsync(seconds, "Device", DeviceSubscriptionHandler);
            }
            
            if (which.Zone && this.ZoneGroupTopology != null)
            {
                await this.ZoneGroupTopology.SubscribeAsync(seconds, "ZoneGroup", ZoneGroupSubscriptionHandler);
            }

            if (which.GroupRenderer && this.GroupRenderingControl != null)
            {
                await this.GroupRenderingControl.SubscribeAsync(seconds, "GroupRenderer", GroupRenderingCallback);
            }

            if (which.ContentDirectory && this.ContentDirectory != null)
            {
                await this.ContentDirectory.SubscribeAsync(seconds, "ContentDirectory", ContentDirectoryCallback);
            }
        }

        public async Task UnsubscribeToEventsAsync(PlayerEventSelector which)
        {
            if (which.Renderer && this.RenderingControl != null)
            {
                await this.RenderingControl.UnsubscribeAsync(RendererSubscriptionHandler);
            }

            if (which.Device && this.DeviceProperties != null)
            {
                await this.DeviceProperties.UnsubscribeAsync(DeviceSubscriptionHandler);
            }

            if (which.Zone && this.ZoneGroupTopology != null)
            {
                await this.ZoneGroupTopology.UnsubscribeAsync(ZoneGroupSubscriptionHandler);
            }

            if (which.GroupRenderer && this.GroupRenderingControl != null)
            {
                await this.GroupRenderingControl.UnsubscribeAsync(GroupRenderingCallback);
            }

            if (which.ContentDirectory && this.ContentDirectory != null)
            {
                await this.ContentDirectory.UnsubscribeAsync(ContentDirectoryCallback);
            }
        }

        public async Task StartPlaybackFromQueue(uint trackNumber)
        {
            var name = await ContentDirectory.Browse("Q:0", "BrowseMetadata", "*", 0, 1, "");
            name.ThrowIfFailed();
            string queueName = DidlData.Parse(name.Result).First().Res;
            var switchq = await AVTransport.SetAVTransportURI(0, queueName, "");
            switchq.ThrowIfFailed();
            var seek = await AVTransport.Seek(0, "TRACK_NR", trackNumber.ToString());
            seek.ThrowIfFailed();
            var play = await AVTransport.Play(0, "1");
            play.ThrowIfFailed();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private async Task DeviceSubscriptionHandler(Service sender, EventSubscriptionArgs args)
        {
            if (args.Items.TryGetValue("WirelessMode", out string mode))
            {
                WirelessMode = mode;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("WirelessMode"));
            }
            if (args.Items.TryGetValue("MoreInfo", out string moreinfo) && !string.IsNullOrEmpty(moreinfo))
            {
                try
                {
                    var items = moreinfo.Split(',');
                    var battery = new Dictionary<string, string>(items.Length);
                    foreach (var item in items)
                    {
                        var parts = item.Split(':');
                        battery[parts[0]] = parts[1];
                    }
                    string charging = battery["BattChg"].ToLower().Replace('_', ' ');
                    int rawPercent = int.Parse(battery["RawBattPct"]);
                    int percent = int.Parse(battery["BattPct"]);
                    int temp = int.Parse(battery["BattTmp"]);

                    BatteryState = string.Format("{0}% {1}°C ({2}) ", percent, temp, charging);
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BatteryState)));
                }
                catch (Exception)
                { }
            }
            
            await Task.FromResult(true);
        }

        private async Task RendererSubscriptionHandler(Service sender, EventSubscriptionArgs args)
        {
            if (args.Items.TryGetValue("LastChange", out string last))
            {
                const string NS = "urn:schemas-upnp-org:metadata-1-0/RCS/";
                var xml = XElement.Parse(last);

                var volumes = from volume in xml.Descendants(XName.Get("Volume", NS))
                              where volume.Attribute(XName.Get("channel")).Value == "Master"
                              select volume.Attribute(XName.Get("val")).Value;
                var master = volumes.FirstOrDefault();

                if (master != null)
                {
                    this.DeviceVolume = ushort.Parse(master);
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("DeviceVolume"));
                }

                var mutes = from mute in xml.Descendants(XName.Get("Mute", NS))
                            where mute.Attribute(XName.Get("channel")).Value == "Master"
                            select mute.Attribute(XName.Get("val")).Value;
                var muted = mutes.FirstOrDefault();
                if (muted != null)
                {
                    this.IsMuted = muted == "1";
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsMuted"));
                }

                var op = xml.Descendants(XName.Get("OutputFixed", NS)).FirstOrDefault();
                if (op != null)
                {
                    var fixd = op.Attribute("val").Value;
                    this.FixedVolume = fixd == "1";
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FixedVolume"));
                }
            }

            await Task.FromResult(true);
        }

        public class QueueChangedEventArgs : EventArgs
        {
            public uint UpdateId;
        }

        public delegate void QueueChangedEventHandler(object sender, QueueChangedEventArgs e);
        public event QueueChangedEventHandler QueueChangedEvent;

        private Task ContentDirectoryCallback(Service sender, EventSubscriptionArgs args)
        {
            if (args.Items.Count == 0 || this.Invisible)
            {
                return Task.CompletedTask;
            }

            if (args.Items.TryGetValue("ContainerUpdateIDs", out string ids))
            {
                // this can be a list such as: R:0,21,FV:2,21,FV:3,21
                if (ids.StartsWith("Q:0,"))
                {
                    // Queue contents have changed
                    var values = ids.Split(',');
                    var update = uint.Parse(values[1]);
                    QueueChangedEvent.Invoke(this, new QueueChangedEventArgs()
                    {
                        UpdateId = update
                    }
                    );
                }
                else if (ids.StartsWith("SQ:"))
                {
                    // TODO handle Playlist updates
                }
            }

            return Task.CompletedTask;
        }

        private async Task GroupRenderingCallback(Service sender, EventSubscriptionArgs args)
        {
            if (args.Items.Count == 0 || this.Invisible)
                return;

            string v;
            if (args.Items.TryGetValue("GroupVolume", out v))
            {
                this.GroupVolume = ushort.Parse(v);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("GroupVolumeEvent"));        // NOTE: Event name is different
            }
            if (args.Items.TryGetValue("GroupMute", out v))
            {
                this.GroupMuted = v == "1";
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("GroupMuted"));
            }
            if (args.Items.TryGetValue("GroupVolumeChangeable", out v))
            {
                this.GroupFixedVolume = v == "0";
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("GroupFixedVolume"));
            }
            await Task.FromResult(false);
        }

        public delegate Task OnThirdPartyMediaServersDelegate(Uri uri, string state);
        public OnThirdPartyMediaServersDelegate OnThirdPartyMediaServers;

        private async Task ZoneGroupSubscriptionHandler(Service sender, EventSubscriptionArgs args)
        {
            if (args.Items.TryGetValue("ThirdPartyMediaServersX", out string servers))
            {
                if (OnThirdPartyMediaServers != null)
                {
                    await OnThirdPartyMediaServers.Invoke(sender.BaseUri, servers);
                }
            }
        }

        private Player(Device device, string displayName, string uniqueName, bool invisible) : this(device)
        {
            this.RoomName = displayName;
            this.UniqueName = uniqueName;
            this.Invisible = invisible;
            this.IsMissing = device.IsMissing;
        }

        // This is used for missing players
        private Player(string location, string displayName, string uniqueName, bool invisible)
        {
            if (Uri.TryCreate(location, UriKind.Absolute, out Uri uri))
            {
                this.IPAddress = uri.Host;
            }
            else
            {
                this.IPAddress = location;
            }
            this.RoomName = displayName;
            this.UniqueName = uniqueName;
            this.Invisible = invisible;
        }

        private Player(Device device)
        {
            if (device.IsRealDevice)
            {
                // Required
                this.DeviceProperties = new SonosServices.DeviceProperties1(device.FindServiceInfo("urn:schemas-upnp-org:device:ZonePlayer:1", "urn:upnp-org:serviceId:DeviceProperties", throwIfMissing: true));
                this.ZoneGroupTopology = new SonosServices.ZoneGroupTopology1(device.FindServiceInfo("urn:schemas-upnp-org:device:ZonePlayer:1", "urn:upnp-org:serviceId:ZoneGroupTopology", throwIfMissing: true));
                this.SystemProperties = new SonosServices.SystemProperties1(device.FindServiceInfo("urn:schemas-upnp-org:device:ZonePlayer:1", "urn:upnp-org:serviceId:SystemProperties", throwIfMissing: true));

                // Optional
                var service = device.FindServiceInfo("urn:schemas-upnp-org:device:MediaRenderer:1", "urn:upnp-org:serviceId:AVTransport", throwIfMissing: false);
                if (service != null)
                {
                    this.AVTransport = new SonosServices.AVTransport1(service);
                }

                service = device.FindServiceInfo("urn:schemas-upnp-org:device:MediaServer:1", "urn:upnp-org:serviceId:ContentDirectory", throwIfMissing: false);
                if (service != null)
                {
                    this.ContentDirectory = new SonosServices.ContentDirectory1(service);
                }

                service = device.FindServiceInfo("urn:schemas-upnp-org:device:ZonePlayer:1", "urn:upnp-org:serviceId:MusicServices", throwIfMissing: false);
                if (service != null)
                {
                    this.MusicServices = new SonosServices.MusicServices1(service);
                }

                service = device.FindServiceInfo("urn:schemas-upnp-org:device:ZonePlayer:1", "urn:upnp-org:serviceId:AlarmClock", throwIfMissing: false);
                if (service != null)
                {
                    this.AlarmClock = new SonosServices.AlarmClock1(service);
                }

                service = device.FindServiceInfo("urn:schemas-upnp-org:device:MediaRenderer:1", "urn:upnp-org:serviceId:RenderingControl", throwIfMissing: false);
                if (service != null)
                {
                    this.RenderingControl = new SonosServices.RenderingControl1(service);
                    this.HasVolume = true;
                }

                service = device.FindServiceInfo("urn:schemas-upnp-org:device:MediaRenderer:1", "urn:upnp-org:serviceId:GroupRenderingControl", throwIfMissing: false);
                if (service != null)
                {
                    this.GroupRenderingControl = new SonosServices.GroupRenderingControl1(service);
                }
            }

            this.ModelName = device.ModelName;
            this.ModelNumber = device.ModelNumber;
            this.FriendlyName = device.FriendlyName;
            this.Icon = device.Icon ?? device.Attribute("icon");
            this.SerialNumber = device.Attribute("serialNum");
            this.SoftwareVersion = device.Attribute("softwareVersion");
            this.HardwareVersion = device.Attribute("hardwareVersion");
            this.SeriesID = device.Attribute("seriesid");
            this.DisplayName = device.Attribute("displayName");
            this.IPAddress = device.BaseUri.GetComponents(UriComponents.Host, UriFormat.Unescaped);
            this.Memory = device.Attribute("memory");
            this.Flash = device.Attribute("flash");
            this.Deactivated = device.Attribute("DeactivationState");
            this.SoftwareGeneration = "S" + (device.Attribute("swGen") ?? "1");
            this.MinCompatibleVersion = device.Attribute("minCompatibleVersion");
            try
            {
                Feature3 = (Feature3Flags)Convert.ToUInt32(device.Attribute("feature3"), 16);
            }
            catch { }
        }

        private async Task InitializeAsync(bool realDevice, bool calcInvisible)
        {
            if (realDevice)
            {
                var result = await DeviceProperties.GetZoneInfo();
                result.ThrowIfFailed();
                this.HTCapable = result.HTAudioIn != 0;

                if (calcInvisible)
                {
                    var zoneinfo = await ZoneGroupTopology.GetZoneGroupAttributes();
                    zoneinfo.ThrowIfFailed();
                    this.Invisible = string.IsNullOrEmpty(zoneinfo.CurrentZoneGroupID);
                }

                if (this.Invisible)
                {
                    // If this Player is invisible, could be a Satellite device so lets find out (could also be a Boost)
                    this.HasVolume = false;
                    var group = await ZoneGroupTopology.GetZoneGroupState();
                    group.ThrowIfFailed();
                    var topo = XElement.Parse(group.ZoneGroupState);
                    var me = topo.Descendants(XName.Get("Satellite")).FirstOrDefault(s => (string)s.Attribute("UUID") == this.UniqueName);
                    if (me != null)
                    {
                        CalculateSurroundName((string)me.Attribute("HTSatChanMapSet"));
                    }
                }
            }
            else
            {
                this.HasVolume = true;
            }
        }

        public string FriendlyModelName()
        {
            string[] gen1 = { "S5", "S13" };
            string[] gen2 = { "S6", "S15", "S18" };
            string[] gen3 = { "S26" };

            string model = this.ModelNumber;
            int suffix = 0;

            if (gen1.Contains(model))
                suffix = 1;
            else if (gen2.Contains(model))
                suffix = 2;
            else if (gen3.Contains(model))
                suffix = 3;
            else if (model == "Sub")
            {
                if (this.Memory == "64")
                    suffix = 1;
                else
                    suffix = 2;
            }
            else if ((model == "ZP120") || (model == "ZP90"))
            {
                if (this.Memory == "32")
                    suffix = 1;
                else
                    suffix = 2;
            }
            else if (model == "TEST")
            {
                return "Sonos Speaker";
            }
            else
                return this.ModelName;

            return string.Format("{0} (Gen {1})", this.ModelName, suffix);
        }

        public Family CalculateFamily(bool newMeansUnknown)
        {
            string[] obsolete = { "WD100", "ZB100" };
            string[] s1 = { "ZP80", "ZP100", "S5" };
            string[] s2 = { "S3", "Sub", "S9", "S1", "S12", "S6", "S11", "S15", "S13", "S14", "S16", "S18", "S20", "S21", "S23", "S17", "S22", "BR200", "TEST" };
            string[] s2only = { "S19", "S24", "S26", "S27", "S29", "S30", "S31", "S33", "S34", "S35", "S38" };

            string model = ModelNumber;
            int minver = 0;
            if (MinCompatibleVersion != null)
            {
                var ver = MinCompatibleVersion.Split('.');
                if (ver.Length != 0)
                    int.TryParse(ver[0], out minver);
            }

            if (!string.IsNullOrEmpty(this.Deactivated))
                return Family.Deactivated;

            if (s2only.Contains(model))
                return Family.ModernOnly;

            if (s2.Contains(model))
                return Family.Modern;

            if (s1.Contains(model))
                return Family.Legacy;

            if (obsolete.Contains(model))
                return Family.Obsolete;

            if (model == "ZP120" || model == "ZP90")
                return Memory == "32" ? Family.Legacy : Family.Modern;

            // Unknown model number

            if (newMeansUnknown)
                return Family.Unknown;

            if (minver >= 58)           // v12.0
                return Family.ModernOnly;
            else
                return Family.Unknown;
        }

        private void CalculateSurroundName(string htmap)
        {
            if (string.IsNullOrEmpty(htmap))
                return;

            string toFind = this.UniqueName + ":";
            int me = htmap.IndexOf(toFind);
            if (me < 0)
                return;

            string what = htmap.Substring(me + toFind.Length, 2);
            switch (what)
            {
                case "LR":
                    RoomName += " (LS)";
                    break;
                case "RR":
                    RoomName += " (RS)";
                    break;
                case "SW":
                    RoomName += " (Sub)";
                    break;
            }
        }

        public static string ConvertDigitalFormat(uint fmt)
        {
            switch (fmt)
            {
                case 0:
                    return StringResource.Get("FormatNoSignal");
                case 1:
                    return StringResource.Get("FormatUnsupported");
                case 2:
                    return StringResource.Get("FormatStereo");
                case 7:
                    return StringResource.Get("FormatDolby2");
                case 18:
                    return StringResource.Get("FormatDolby");
                case 19:
                    return "Bug-19-email-us-please";
                case 20:
                    return StringResource.Get("FormatPauseBurst");
                case 21:
                    return string.Empty;
                case 22:
                    return StringResource.Get("FormatSilence");

                case 0x02000002:
                    return "Stereo PCM";
                case 0x05100002:
                    return "Multichannel PCM 5.1";
                case 0x07100002:
                    return "Multichannel PCM 7.1";

                case 0x02000016:
                    return "Silence 2.0";
                case 0x05100016:
                    return "Silence 5.1";
                case 0x07100016:
                    return "Silence 7.1";

                case 38:    // 0x26
                    return "Bug-38-email-us-please";

                case 56:    // 0x38
                    return "Dolby Digital";
                case 0x02000038:
                    return "Dolby Digital 2.0";

                case 0x05100039:
                    return "Dolby Digital 5.1";

                case 0x0200003a:        // 33554490 DD+ 2.0?
                    return "Dolby Digital Plus 2.0";
                case 0x0510003a:        // 84934714
                    return "Dolby Digital Plus 5.1";

                case 59:    // 0x3B
                    return "Dolby Atmos (DD+)";

                case 0x0510003C:
                    return "Dolby TrueHD 5.1";
                case 0x0710003C:
                    return "Dolby TrueHD 7.1";

                case 61:    // 0x3D
                    return "Dolby Atmos (TrueHD)";

                case 0x0200003e:
                    return "Dolby Multichannel PCM 2.0";
                case 0x0510003e:
                    return "Dolby Multichannel PCM 5.1";
                case 0x0710003E:
                    return "Dolby Multichannel PCM 7.1";

                case 63:    // 0x3F
                    return "Dolby Atmos";

                case 0x05100041:
                    return "DTS Surround 5.1";

                default:
                    return string.Format(StringResource.Get("Format_Unknown"), fmt);
            }
        }

        public static List<string> DecodeStreamInfo(string service, string streamInfo)
        {
            if (string.IsNullOrEmpty(streamInfo) || streamInfo.StartsWith("bd:0"))
                return null;

            var parts = streamInfo.Split(',');
            if (parts.Length == 1)
                return null;

            Dictionary<string, string> items = new Dictionary<string, string>();
            foreach (var p in parts)
            {
                var split = p.Split(':');
                if (split.Length == 2)
                {
                    items[split[0]] = split[1];
                }
            }
            List<string> result = new List<string>();
            int bd = 0;
            float sr = 0;
            bool lossless = false;
            bool dolby = false;
            if (!items.TryGetValue("bd", out string v) || !int.TryParse(v, out bd))
                return null;
            if (!items.TryGetValue("sr", out v) || !float.TryParse(v, out float f))
                return null;
            sr = f / 1000;

            if (items.TryGetValue("d", out v))
                dolby = v == "1";

            if (items.TryGetValue("l", out v))
                lossless = v == "1";

            if (dolby)
            {
                result.Add("Dolby Atmos");
            }
            else if (lossless && service.Contains("Amazon Music"))
            {
                if (bd == 16)
                    result.Add("HD");
                else if (bd == 24)
                    result.Add("Ultra HD");
            }

            result.Add(sr.ToString() + "kHz");
            result.Add(bd.ToString() + "-bit");

            return result;
        }

        public List<Tuple<string, string>> DiagnosticList(string hhid)
        {
            var result = new List<Tuple<string, string>>();
            result.Add(new Tuple<string, string>("Room", RoomName));
            result.Add(new Tuple<string, string>("Model", ModelName));
            result.Add(new Tuple<string, string>("Model Number", ModelNumber));
            result.Add(new Tuple<string, string>("Family", CalculateFamily(false).ToString()));
            result.Add(new Tuple<string, string>("IP address", IPAddress));
            result.Add(new Tuple<string, string>("Memory", Memory));
            result.Add(new Tuple<string, string>("Storage", Flash)); ;
            result.Add(new Tuple<string, string>("Sonos OS", SoftwareGeneration));
            result.Add(new Tuple<string, string>("Software Version", SoftwareVersion));
            result.Add(new Tuple<string, string>("Hardware Version", HardwareVersion));
            result.Add(new Tuple<string, string>("Series ID", SeriesID));
            // NO as needs event: result.Add(new Tuple<string, string>("Wireless Mode", WirelessMode));
            result.Add(new Tuple<string, string>("Status", Deactivated ?? "Working"));
            result.Add(new Tuple<string, string>("Serial Number", SerialNumber));
            result.Add(new Tuple<string, string>("Device ID", UniqueName));
            result.Add(new Tuple<string, string>("Household", hhid));

            return result;
        }

        internal static Player CreateMissingPlayer(ZoneGroupState.ZoneGroupMemberData info)
        {
            var player = new Player(info.Location, info.Name + " MISSING", info.Uuid, true);
            return player;
        }

        // Every Sonos device has these four:
        public SonosServices.DeviceProperties1 DeviceProperties { get; }
        public SonosServices.SystemProperties1 SystemProperties { get; }
        public SonosServices.ZoneGroupTopology1 ZoneGroupTopology { get; }

        // Only Players can have these:
        public SonosServices.AVTransport1 AVTransport { get; }
        private SonosServices.RenderingControl1 RenderingControl { get; }
        public SonosServices.ContentDirectory1 ContentDirectory { get; }
        public SonosServices.MusicServices1 MusicServices { get; }
        public SonosServices.AlarmClock1 AlarmClock { get; }
        public SonosServices.GroupRenderingControl1 GroupRenderingControl { get; }
    }
}
