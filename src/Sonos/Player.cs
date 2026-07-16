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
            Balance = 0x20,
            Battery = 0x80
        };

        /// <summary>
        /// This is the name set by the user eg "Kitchen"
        /// </summary>
        public string RoomName { get; private set; }

        /// <summary>
        /// This is the name set by the user with the bound suffix eg "Media (LR)"
        /// </summary>
        public string RoomNameWithSuffix { get; private set; }

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
        /// Display version
        /// </summary>
        public string DisplayVersion { get; }

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
        public BatteryStatus BatteryState { get; private set; }
        public int DeviceVolume { get; private set; }
        public bool HasVolume { get; private set; }
        public bool FixedVolume { get; private set; }
        public bool IsMuted { get; private set; }
        public bool IsMissing { get; private set; }
        public bool IsRealPlayer { get; protected set; }
        public string ParentId { get; protected set; }
        public short Bass { get; protected set; }
        public short Treble { get; protected set; }
        public short Loudness { get; protected set; }
        public short Balance { get; protected set; }
        public bool NightMode { get; protected set; }
        public short DialogLevel { get; protected set; }

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
            await player.InitializeAsync(true);
            return player;
        }

        public static async Task<Player> CreatePlayerAsync(Device device, ZoneGroupState.ZoneGroupMemberData info)
        {
            var player = new Player(device, info.Name, info.Uuid, info.Invisible=="1");
            await player.InitializeAsync(true);
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

        public async Task SetBassAsync(short newBass)
        {
            this.Bass = newBass;
            await this.RenderingControl.SetBass(0, newBass);
        }

        public async Task SetTrebleAsync(short newTreble)
        {
            this.Treble = newTreble;
            await this.RenderingControl.SetTreble(0, newTreble);
        }

        public async Task SetLoudnessAsync(bool loud)
        {
            this.Loudness = loud ? (short)1 : (short)0;
            await this.RenderingControl.SetLoudness(0, "Master", loud);
        }

        public async Task SetBalanceAsync(short newBalance)
        {
            ushort lf = (ushort)((newBalance <= 0) ? 100 : 100 - newBalance);
            ushort rf = (ushort)((newBalance >= 0) ? 100 : 100 + newBalance);

            await this.RenderingControl.SetVolume(0, "LF", lf);
            await this.RenderingControl.SetVolume(0, "RF", rf);
        }

        public async Task ResetEQAsync()
        {
            // This is the old API, newer ones have more args, but this works on everything
            await this.RenderingControl.ResetBasicEQ(0).Optional();
        }

        public async Task SetNightModeAsync(bool nightMode)
        {
            await this.RenderingControl.SetEQ(0, "NightMode", (short)(nightMode ? 1 : 0)).Optional();
        }

        public async Task SetDialogLevelAsync(short level)
        {
            await this.RenderingControl.SetEQ(0, "DialogLevel", level).Optional();
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

        public void Update(ZoneGroupState.ZoneGroupMemberData info)
        {
            EthLink = info.EthLink;
            ChannelFreq = info.ChannelFreq;
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
            await player.InitializeAsync(false);
            return player;
        }

        public async Task<uint> QueueSize()
        {
            var queue = await ContentDirectory.Browse("Q:0", "BrowseDirectChildren", "*", 0, 1, "").Required();
            return queue.TotalMatches;
        }

        public async Task ClearQueueAsync()
        {
            await AVTransport.RemoveAllTracksFromQueue(0).Required();
        }

        public async Task SaveQueueAsNewPlaylistAsync(string name)
        {
            // This is assumed to be a new playlist, so no Replace required
            await AVTransport.SaveQueue(0, name, string.Empty).Required();
        }

        public async Task SaveQueueReplacePlaylistAsync(UPnPMusicItem item)
        {
            // Must remove the old version first
            await ContentDirectory.DestroyObject(item.Id).Required();
            await AVTransport.SaveQueue(0, item.Title, string.Empty).Required();
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

            var (res, md) = await GetResAndMdAsync(item);

            await AVTransport.AddURIToQueue(0, res, md, whereToAdd, queueAsNext).Required();

            if (seekThenPlay)
            {
                await PlayFromQueueAsync(whereToAdd, shuffle);
            }
        }

        private async Task<(string res, string md)> GetResAndMdAsync(MusicItem item)
        {
            string res = item.Res;
            string md = item.Metadata;

            // Go get some fresh md if UPnP (unless a Favorite)
            if (item is UPnPMusicItem upnp && upnp.ParentId != "FV:2")
            {
                if (upnp.Id != "spdif-input")
                {
                    var freshmd = await ContentDirectory.Browse(upnp.Id, "BrowseMetadata", "*", 0, 1, "");
                    if (freshmd.Error == null)
                    {
                        md = freshmd.Result;
                    }
                }
                else
                {
                    res = DidlData.MakeTV(this).Res;
                }
            }
            else if (md != null)
            {
                md = md.Replace("\r\n", string.Empty);
            }

            return (res, md);
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

        public async Task Skip(int delta)
        {
            var diff = TimeSpan.FromSeconds(delta).ToString();
            await AVTransport.Seek(0, "TIME_DELTA", diff).Required();
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
                    bool foundBattery = false;
                    foreach (var item in items)
                    {
                        var parts = item.Split(':');
                        battery[parts[0]] = parts[1];
                        foundBattery |= parts[0] == "BattChg";
                    }

                    if (foundBattery)
                    {
                        bool charging = battery["BattChg"] == "CHARGING";         // or "NOT_CHARGING"
                        uint rawPercent = uint.Parse(battery["RawBattPct"]);
                        uint percent = uint.Parse(battery["BattPct"]);
                        int temp = int.Parse(battery["BattTmp"]);

                        if (BatteryState == null)
                        {
                            BatteryState = new BatteryStatus();
                        }

                        BatteryState.Charging = charging;
                        BatteryState.Percent = percent;
                        BatteryState.RawPercent = rawPercent;
                        BatteryState.Temperature = temp;
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BatteryState)));
                    }
                }
                catch (Exception)
                { }
            }
            
            await Task.FromResult(true);
        }

        public class BatteryStatus
        {
            public bool Charging;
            public uint Percent;
            public uint RawPercent;
            public int Temperature;

            public override string ToString()
            {
                return string.Format("{0}% {1}°C ({2}) ", Percent, Temperature, Charging ? "Charging" : "Not charging");
            }
        }

        private async Task RendererSubscriptionHandler(Service sender, EventSubscriptionArgs args)
        {
            if (args.Items.TryGetValue("LastChange", out string last))
            {
                const string NS = "urn:schemas-upnp-org:metadata-1-0/RCS/";
                var xml = XElement.Parse(last);

                var v = xml.Descendants(XName.Get("Volume", NS));

                var lf = v.FirstOrDefault(ch => (string)ch.Attribute(XName.Get("channel")) == "LF");
                if (lf != null)
                {
                    var lfv = (string)(lf.Attribute(XName.Get("val")));
                    var rf = v.FirstOrDefault(ch => (string)ch.Attribute(XName.Get("channel")) == "RF");
                    if (rf != null)
                    {
                        // This assumes LF and RF are always sent together
                        var rfv = (string)(rf.Attribute(XName.Get("val")));
                        if (ushort.TryParse(lfv, out var left) && ushort.TryParse(rfv, out var right))
                        {
                            var newBalance = CalculateBalance(left, right);
                            if (newBalance != this.Balance)
                            {
                                this.Balance = newBalance;
                                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Balance)));
                            }
                        }
                    }
                }

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

                var bass = (string)xml.Descendants(XName.Get("Bass", NS)).FirstOrDefault()?.Attribute("val");
                if (short.TryParse(bass, out short b) && b != this.Bass)
                {
                    this.Bass = b;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Bass)));
                }

                var treble = (string)xml.Descendants(XName.Get("Treble", NS)).FirstOrDefault()?.Attribute("val");
                if (short.TryParse(treble, out short t) && t != this.Treble)
                {
                    this.Treble = t;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Treble)));
                }

                var loudness = xml.Descendants(XName.Get("Loudness", NS)).FirstOrDefault();
                if (((string)loudness?.Attribute(XName.Get("channel")) == "Master"))
                {
                    string l = (string)loudness.Attribute(XName.Get("val"));
                    if (short.TryParse(l, out short loud) && loud != this.Loudness)
                    {
                        this.Loudness = loud;
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Loudness)));
                    }
                }

                var night = (string)xml.Descendants(XName.Get("NightMode", NS)).FirstOrDefault()?.Attribute("val");
                if (night != null)
                {
                    bool n = night == "1";
                    if (n != this.NightMode)
                    {
                        this.NightMode = n;
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NightMode)));
                    }
                }

                var dialog = (string)xml.Descendants(XName.Get("DialogLevel", NS)).FirstOrDefault()?.Attribute("val");
                if (dialog != null)
                {
                    if (short.TryParse(dialog, out short d) && d != this.DialogLevel)
                    {
                        this.DialogLevel = d;
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DialogLevel)));
                    }
                }
            }

            await Task.FromResult(true);
        }

        // LF is 100 if Balance<=0, else =Balance
        // RF is 100 if Balance>=0, else =-Balance
        internal static short CalculateBalance(ushort lf, ushort rf)
        {
            return (short)((lf < 100) ? 100 - lf : (short)rf - 100);
        }


        public class ContentChangedEventArgs : EventArgs
        {
            public string What;
            public string IDs;
            public uint UpdateId;
        }

        public delegate void QueueChangedEventHandler(object sender, ContentChangedEventArgs e);
        public event QueueChangedEventHandler QueueChangedEvent;

        public delegate void ContentChangedEventHandler(object sender, ContentChangedEventArgs e);
        public event ContentChangedEventHandler ContentChangedEvent;

        private Task ContentDirectoryCallback(Service sender, EventSubscriptionArgs args)
        {
            if (args.Items.Count == 0 || this.Invisible)
            {
                return Task.CompletedTask;
            }

            if (args.Items.TryGetValue("ContainerUpdateIDs", out string ids))
            {
                var values = ids.Split(',');
                if (values.Length < 2)
                    return Task.CompletedTask;

                var update = uint.Parse(values[1]);

                // this can be a list such as: R:0,21,FV:2,21,FV:3,21
                if (ids.StartsWith("Q:0,"))
                {
                    // Queue contents have changed
                    QueueChangedEvent.Invoke(this, new ContentChangedEventArgs()
                    {
                        UpdateId = update
                    }
                    );
                }
                else if (ids.StartsWith("SQ:"))
                {
                    // eg "SQ:,25"
                    // The list of playlists changed (or the contents of a playlist)
                    ContentChangedEvent.Invoke(this, new ContentChangedEventArgs()
                    {
                        What = values[0],
                        IDs = ids,
                        UpdateId = update
                    });
                }
                else if (ids.StartsWith("FV:2"))
                {
                    // List of Favorites changed eg "FV2:,100" (or a Favorite)
                    ContentChangedEvent.Invoke(this, new ContentChangedEventArgs()
                    {
                        What = values[0],
                        IDs = ids,
                        UpdateId = update
                    });

                }
                else if (ids.StartsWith("R:0"))
                {
                    // List of Radio Stations changed
                    ContentChangedEvent.Invoke(this, new ContentChangedEventArgs()
                    {
                        What = values[0],
                        IDs = ids,
                        UpdateId = update
                    });

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

        public string EthLink { get; private set; }
        public string ChannelFreq { get; private set; }

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
                this.IsRealPlayer = true;

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
            this.DisplayVersion = device.Attribute("displayVersion");
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

        private async Task InitializeAsync(bool calcInvisible)
        {
            if (this.IsRealPlayer)
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

                RoomNameWithSuffix = RoomName;          // default

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
                        ParentId = (string)me.Parent.Attribute("UUID");
                    }
                    else
                    {
                        // For stereo pairs
                        me = topo.Descendants(XName.Get("ZoneGroupMember")).FirstOrDefault(s => (string)s.Attribute("UUID") == this.UniqueName);
                        if (me != null)
                        {
                            ParentId = (string)me.Parent.Attribute("Coordinator");
                            if (ParentId == UniqueName)
                            {
                                // A BOOST is in a group of 1
                                ParentId = null;
                            }
                            if (ParentId != null)
                            {
                                // Coordinator is always Left, so invisible one must be R
                                RoomNameWithSuffix = RoomName + " (R)";
                            }
                        }
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
            string[] gen2 = { "S6", "S15", "S18", "S31", "S38", "S54", "S57" };
            string[] gen3 = { "S26", "S43" };
            string[] gen4 = { "S55" };

            string model = this.ModelNumber;
            int suffix = 0;

            if (gen1.Contains(model))
                suffix = 1;
            else if (gen2.Contains(model))
                suffix = 2;
            else if (gen3.Contains(model))
                suffix = 3;
            else if (gen4.Contains(model))
                suffix = 4;
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

            var items = htmap.Split(';');
            var mine = items.FirstOrDefault(i => i.StartsWith(this.UniqueName));
            if (mine == null)
                return;

            string what = mine.Substring(this.UniqueName.Length + 1);
            string suffix;

            switch (what)
            {
                case "LR":
                    suffix = "LS";
                    break;
                case "RR":
                    suffix = "RS";
                    break;
                case "SW":
                    suffix = "Sub";
                    break;
                case "LR,RR":
                    suffix = "LS+RS";
                    break;
                default:
                    return;
            }

            RoomNameWithSuffix = $"{RoomName} ({suffix})";
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

        public static List<string> DecodeStreamInfo(MusicService service, string streamInfo)
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

            bool showbits = true;

            if (dolby)
            {
                result.Add("Dolby Atmos");
            }
            else if (lossless)
            {
                string name;

                if (bd == 16 && service != null && service.QualityBadges.TryGetValue("16bit", out name))
                {
                    result.Add(name);
                    showbits = false;
                }
                else if (bd == 24 && service != null && service.QualityBadges.TryGetValue("24bit", out name))
                {
                    result.Add(name);
                    showbits = false;
                }
                else
                {
                    result.Add("Lossless");
                }
            }

            result.Add(sr.ToString() + "kHz");

            if (showbits)
            {
                result.Add(bd.ToString() + "-bit");
            }

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

        public async Task<IEnumerable<UPnPMusicItem>> GetPlaylistsAsync()
        {
            var all = await ContentDirectory.Browse("SQ:", "BrowseDirectChildren", "*", 0, 0, "");
            return UPnPMusicItem.Parse(all.Result, ContentDirectory.BaseUri);
        }

        public async Task<uint> AddToPlaylistAsync(string playlistId, uint updateId, MusicItem item)
        {
            uint where = unchecked((uint)-1);
            var (res, md) = await GetResAndMdAsync(item);

            var state = await ContentDirectory.Browse(playlistId, "BrowseMetadata", string.Empty, 0, 1, string.Empty).Required();
            var added = await AVTransport.AddURIToSavedQueue(0, playlistId, state.UpdateID, res, md, where);
            added.ThrowIfFailed();
            return added.NewUpdateID;
        }

        public async Task AddToFavoritesAsync(MusicItem item)
        {
            string md = item.Metadata;
            string subtitle = item.Subtitle;
            string art = item.ArtUri;
            if (art != null && art.StartsWith("ms-appx://"))
            {
                art = null;
            }
            string protocolInfo = null;

            if (string.IsNullOrEmpty(md) && item is UPnPMusicItem upnp)
            {
                // We have no metadata (i.e. real UPnP source), so go get some
                var freshmd = await ContentDirectory.Browse(upnp.Id, "BrowseMetadata", "*", 0, 1, "").Required();
                md = freshmd != null ? freshmd.Result : string.Empty;
            }

            if (item is MusicServiceItem svcItem)
            {
                // Make a Fave from a Music Service item
                protocolInfo = svcItem.Provider.GetProtocolInfo(item);                // Necessary so that Tracks from Services are playable
                if (subtitle == null)
                {
                    subtitle = svcItem.Provider.DisplayName;
                }
                // Get rid of the art from the md as it can be enormous and cause the resMD to be >1024 which will get truncated by CreateObject
                md = DidlData.RemoveElement(md, "upnp:albumArtURI");
            }
            else
            {
                // UPnP Source
                var originalMD = new SimpleDidl(XElement.Parse(md), false);
                art = originalMD.albumArtURI;
                if (string.IsNullOrEmpty(subtitle))
                {
                    subtitle = DidlData.ClassToName(originalMD.uclass);
                }
                protocolInfo = originalMD.protocolInfo;

                if (item.IsContainer)
                {
                    // The metadata we have for this is a Container, but we need to manually build an Item instead
                    md = DidlData.CreateItem(originalMD.id, originalMD.parent, item.Title, originalMD.uclass);
                }
            }

            // Make sure the md isn't going to be corrupted by CreateObject
            Debug.Assert(md.Length < 1024);

            var didl = DidlData.CreateFavorite(item.Title, subtitle, art, item.Res, protocolInfo, md);
            var create = await ContentDirectory.CreateObject("FV:2", didl).Required();
        }

        public async Task RemoveQueueItemAsync(QueueMusicItem item, uint updateId)
        {
            await AVTransport.RemoveTrackFromQueue(0, item.Id, updateId).Required();
        }

        public async Task RemoveItemAsync(UPnPMusicItem item)
        {
            // This is based on IsDeletable, which is true for:
            // Favorites, Playlists, and Tracks in Playlists

            if (item.ParentId == "FV:2" || item.ParentId == "R:0/0")
            {
                // Delete a Favorite or Radio Station
                await ContentDirectory.DestroyObject(item.Id).Required();
                return;
            }

            if (!item.ParentId.StartsWith("SQ:"))
                throw new NotImplementedException("Cannot delete from " + item.ParentId);

            if (item.ParentId == "SQ:")
            {
                // Delete a Playlist
                await ContentDirectory.DestroyObject(item.Id).Required();
                return;
            }

            const uint chunksize = 50;
            uint start = 0;
            SonosServices.ContentDirectory1.Browse_Result browse = null;
            bool found = false;

            // Need to determine the index of the item in the playlist so have to enum it!
            while (!found)
            {
                browse = await ContentDirectory.Browse(item.ParentId, "BrowseDirectChildren", string.Empty, start, chunksize, string.Empty).Required();
                var items = UPnPMusicItem.Parse(browse.Result, ContentDirectory.BaseUri);
                foreach(var track in items)
                {
                    if (track.Id == item.Id)
                    {
                        found = true;
                        break;
                    }
                    start++;
                }
                if (browse.TotalMatches == start)
                {
                    throw new Exception("Cannot find track in playlist");
                }
            };

            // This is a truly weird API, see the details at https://github.com/SoCo/SoCo/blob/master/soco/core.py#L2628
            string tracklist = start.ToString();
            string newpos = string.Empty;
            var moved = await AVTransport.ReorderTracksInSavedQueue(0, item.ParentId, browse.UpdateID, tracklist, newpos);
            moved.ThrowIfFailed();
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
