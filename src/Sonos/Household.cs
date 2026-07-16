using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using OpenPhonos.UPnP;
using SonosServices;

namespace OpenPhonos.Sonos
{
    public class Household : ZoneGroupState.IZoneDelta
    {
        public const string SonosUrn = "urn:schemas-upnp-org:device:ZonePlayer:1";
        private const int SSDP_TIMEOUT = 3;

        // List of IPaddresses to try when no devices are found
        public static IList<string> LastDitchDeviceList;

        public class HouseholdResult
        {
            public readonly IReadOnlyList<Household> Households;
            public readonly string FailureReason;
            public readonly bool CacheResults;

            internal HouseholdResult(List<Household> households, string reason, bool cache)
            {
                Households = households;
                FailureReason = reason;
                CacheResults = cache;
            }
        }

        // Given the IP address of devices, build a Household from each, or null if cannot
        public static async Task<HouseholdResult> FindHouseholdFromPlayers(IList<string> hosts, Action<Player, string> OnNewPlayer = null)
        {
            var households = new List<Household>();
            bool nocache = false;

            foreach (var host in hosts)
            {
                NetLogger.WriteLine("Trying specific household from: {0}", host);

                var newdevice = await Device.CreateAsync($"http://{host}:1400/xml/device_description.xml");
                if (newdevice.IsMissing)
                    continue;

                var svcinfo = newdevice.FindServiceInfo("urn:schemas-upnp-org:device:ZonePlayer:1", "urn:upnp-org:serviceId:DeviceProperties", false);
                if (svcinfo == null)
                    continue;

                var devinfo = new DeviceProperties1(svcinfo);
                var hh = await devinfo.GetHouseholdID();
                if (hh.Error is UPnPForbiddenException)
                {
                    NetLogger.LikelyProblem = hh.Error.Message;
                    nocache = true;
                    continue;
                }
                if (hh.Error != null || hh.CurrentHouseholdID == string.Empty)
                    continue;

                string hhid = hh.CurrentHouseholdID;
                if (households.Any(t => t.HouseholdId == hhid))
                    continue;

                var h = new Household(hhid);
                households.Add(h);
                var devlist = new List<Tuple<Device, string>>() { new Tuple<Device, string>(newdevice, hhid) };
                var quickResults = await AddDevicesToHousehold(OnNewPlayer, households, devlist);
                nocache |= !quickResults.CacheResults;
            }

            if (households.Any())
            {
                Listener.Update(households.First().VisiblePlayerList.FirstOrDefault()?.IPAddress, 1400);
                if (nocache && households.Count != 0)
                {
                    var later = NotifyMissingSystemAsync(households.First(), $"A Sonos system could not be found due to: {NetLogger.LikelyProblem}");
                }
                return new HouseholdResult(households, string.Empty, !nocache);
            }
            else
            {
                return null;
            }
        }

        public static async Task<HouseholdResult> FindAllHouseholdsAndPlayersAsync(Action<Player, string> OnNewPlayer = null)
        {
            var households = new List<Household>();
            var finder = new Finder();
            var devices = new List<Tuple<Device, string>>();
            bool carryon = true;
            await finder.ByURNAsync(SonosUrn, async (location, network, headers) =>
            {
                if (headers.TryGetValue("st", out string st) && st == SonosUrn)
                {
                    headers.TryGetValue("x-rincon-household", out string hhid);

                    var newdevice = await Device.CreateAsync(location);
                    {
                        Household owner = null;
                        lock (households)
                        {
                            owner = households.Find(h => h.HouseholdId == hhid);
                            if (owner == null)
                            {
                                owner = new Household(hhid);
                                households.Add(owner);
                            }
                        }

                        lock (devices)
                        {
                            devices.Add(Tuple.Create(newdevice, hhid));
                        }
                    }
                }
                return await Task.FromResult(carryon);
            },
            SSDP_TIMEOUT);

            carryon = false;

            // Try the last ditch list
            if (LastDitchDeviceList?.Count > 0)
            {
                bool more = false;
                foreach (var host in LastDitchDeviceList)
                {
                    NetLogger.WriteLine("Trying last ditch device: {0}", host);

                    var newdevice = await Device.CreateAsync($"http://{host}:1400/xml/device_description.xml");
                    if (newdevice.IsMissing)
                        continue;
                    var svcinfo = newdevice.FindServiceInfo("urn:schemas-upnp-org:device:ZonePlayer:1", "urn:upnp-org:serviceId:DeviceProperties", false);
                    if (svcinfo == null)
                        continue;
                    var devinfo = new DeviceProperties1(svcinfo);
                    var hh = await devinfo.GetHouseholdID();
                    if (hh.Error != null || hh.CurrentHouseholdID == string.Empty)
                        continue;
                    string hhid = hh.CurrentHouseholdID;

                    lock (households)
                    {
                        var owner = households.Find(h => h.HouseholdId == hhid);
                        if (owner == null)
                        {
                            owner = new Household(hhid);
                            households.Add(owner);
                        }
                    }

                    // If we've already seen it, don't add it again
                    if (devices.Any(d => d.Item1.BaseUri == newdevice.BaseUri))
                        continue;

                    devices.Add(Tuple.Create(newdevice, hhid));
                    more = true;
                    NetLogger.WriteLine("Using last ditch device: {0}", host);
                }

                // We have to set the Listener before we start subscribing to events
                if (more)
                {
                    Listener.Update(null, 0);
                }
            }

            var finaldevices = new List<Tuple<Device, string>>();
            lock (devices)
            {
                finaldevices = devices.ToList();
            }

            return await AddDevicesToHousehold(OnNewPlayer, households, finaldevices);
        }

        private static async Task<HouseholdResult> AddDevicesToHousehold(Action<Player, string> OnNewPlayer, List<Household> households, List<Tuple<Device, string>> finaldevices)
        {
            bool nocache = false;

            foreach (var d in finaldevices)
            {
                try
                {
                    var player = await Player.CreatePlayerAsync(d.Item1);
                    lock (households)
                    {
                        var household = households.Find(h => h.HouseholdId == d.Item2);
                        household.AddPlayer(player);
                    }
                    OnNewPlayer?.Invoke(player, d.Item2);
                }
                catch (Exception ex)
                {
                    NetLogger.WriteLine("Device {0} skipped due to {1}", d.Item1.BaseUri, ex.Message);
                    if (ex is UPnPForbiddenException)
                    {
                        NetLogger.LikelyProblem = ex.Message;
                    }
                    nocache = true;
                }
            }

            int removed = households.RemoveAll(h => h.AllPlayerList.Count == 0);
            if (removed != 0)
            {
                NetLogger.WriteLine($"Removed {removed} empty household(s)");
                if (households.Count != 0)
                {
                    var later = NotifyMissingSystemAsync(households.First(), $"A Sonos system could not be found due to: {NetLogger.LikelyProblem}");
                }
            }

            households.Sort(CompareHouseholds);
            return new HouseholdResult(households, NetLogger.LikelyProblem, !nocache);
        }

        private static async Task NotifyMissingSystemAsync(Household household, string msg)
        {
            // Tell the user, but wait a bit until the UX has installed a handler
            await Task.Delay(TimeSpan.FromSeconds(3));
            household.NotifyUser(new HouseholdEventArgs(msg));
        }

        public static async Task<HouseholdResult> CreateTestHouseholdAsync(string xml, Action<Player, string> onNewPlayer)
        {
            var household = new Household("TestHousehold");
            xml = xml.Replace("192.168.1.", OpenPhonos.UPnP.Device.ImposterSubnet);
            CreateImposterPlayers(xml);
            await household.UpdateFromZoneStateAsync(xml, "Test");
            foreach (var player in household.VisiblePlayerList)
            {
                onNewPlayer.Invoke(player, household.HouseholdId);
            }
            household.AssociatedZP = household.VisiblePlayerList.First();
            var hlist = new List<Household>() { household };
            return new HouseholdResult(hlist, string.Empty, false);
        }

        private static void CreateImposterPlayers(string xml)
        {
            var dom = XElement.Parse(xml);
            var imposters = new Dictionary<string, XElement>();
            var players = dom.Descendants(XName.Get("ZoneGroupMember")).ToList();
            players.AddRange(dom.Descendants(XName.Get("Satellite")));
            XNamespace ns = "urn:schemas-upnp-org:device-1-0";
            var rnd = new Random(42);
            string icon = string.Empty;     // TODO better default

            foreach (var player in players)
            {
                var root = new XElement(ns + "root",
                    new XElement(ns + "device",
                        new XElement(ns + "friendlyName", (string)player.Attribute("ZoneName")),
                        new XElement(ns + "roomName", (string)player.Attribute("ZoneName")),
                        new XElement(ns + "UDN", "uuid:" + (string)player.Attribute("UUID")),
                        new XElement(ns + "serialNum", rnd.Next(0, int.MaxValue).ToString("X")),
                        new XElement(ns + "modelNumber", "TEST"),
                        new XElement(ns + "memory", "128"),
                        new XElement(ns + "flash", "64"),
                        new XElement(ns + "swGen", "1"),
                        new XElement(ns + "hardwareVersion", "1.23"),
                        new XElement(ns + "softwareVersion", "42.01"),
                        new XElement(ns + "seriesid", "A100"),
                        new XElement(ns + "icon", icon)
                        )
                    );

                imposters[(string)player.Attribute("Location")] = root;
            }

            OpenPhonos.UPnP.Device.ImposterDeviceCreator = (uri) =>
            {
                return imposters[uri];
            };
        }

        // Sort by id, except that empty goes last
        private static int CompareHouseholds(Household a, Household b)
        {
            if (a == b)
                return 0;
            if (a.HouseholdId == null)
                return 1;
            if (b.HouseholdId == null)
                return -1;
            return a.HouseholdId.CompareTo(b.HouseholdId);
        }

        public string HouseholdId { get; }
        public Player AssociatedZP { get; private set; }
        public MusicServiceProvider MusicServices { get; set; }

        /// <summary>
        /// This action is called after a Zone update has completed with a description and a list of actions taken
        /// </summary>
        public Action<string, IEnumerable<string>> ZoneDiagnostic { get; set; }
        public ZoneGroupState GroupData { get; internal set; }

        public ObservableCollection<Group> Groups { get; }

        public delegate void HouseholdEventHandler(object sender, HouseholdEventArgs args);
        public event HouseholdEventHandler NotifyHousehold;

        public class HouseholdEventArgs : EventArgs
        {
            public string Message { get; set; }

            public HouseholdEventArgs(string msg)
            {
                Message = msg;
            }
        }

        public Household(string id)
        {
            HouseholdId = id;
            playerList = new List<Player>();
            Groups = new ObservableCollection<Group>();
        }

        public void NotifyUser(HouseholdEventArgs args)
        {
            NotifyHousehold?.Invoke(this, args);
        }

        /// <summary>
        /// Call this on a fresh Household to fill in all of the Group information
        /// </summary>
        /// <returns></returns>
        public async Task BuildGroupsAsync()
        {
            if (Groups.Count != 0)
                throw new InvalidOperationException("Can only call BuildGroupsAsync once");

            var zoneinfo = await AssociatedZP.GetZoneGroupStateAsync();
            await UpdateFromZoneStateAsync(zoneinfo, "Start");
        }

        public async Task SubscribeAsync()
        {
            if (AssociatedZP.ZoneGroupTopology == null)
                return;

            await AssociatedZP.ZoneGroupTopology.SubscribeAsync(OpenPhonos.UPnP.Listener.MinimumTimeout, "HH", ZoneGroupHandler);

            foreach(var more in AllPlayerList)
            {
                // We need to get the Wireless attributes of Boosts/Bridges, this is the sole subscription they have
                if (more.Invisible && (more.ModelNumber == "BR100" || more.ModelNumber == "BR200"))
                {
                    var what = new PlayerEventSelector() { Device = true };
                    await more.SubscribeToEventsAsync(OpenPhonos.UPnP.Listener.MinimumTimeout, what);
                }
            }
        }

        private async Task ZoneGroupHandler(object sender, EventSubscriptionArgs args)
        {
            if (args.Items.TryGetValue("ZoneGroupState", out string zoneState))
            {
                await UpdateFromZoneStateAsync(zoneState, args.EventNumber.ToString());
            }
        }

        public async Task UnsubscribeAsync()
        {
            if (AssociatedZP.ZoneGroupTopology == null)
                return;

            await AssociatedZP.ZoneGroupTopology.UnsubscribeAsync(ZoneGroupHandler);
        }

        ZoneDeltaCollectorDiagnostic ZoneLogger;

        public async Task UpdateFromZoneStateAsync(string zoneinfo, string description)
        {
            if (this.GroupData == null)
                this.GroupData = new ZoneGroupState();

            var newData = new ZoneGroupState(zoneinfo);
#if DEBUG
            ZoneLogger = new ZoneDeltaCollectorDiagnostic(GroupData, newData);
#else
            ZoneLogger = null;
#endif
            await EnsurePlayersExist(newData);
            ZoneGroupState.CalculateDifferences(this.GroupData, newData, this);

            VerifyGroupState();
            this.GroupData = newData;

            ZoneDiagnostic?.Invoke(description, ZoneLogger.Results);
        }

        private async Task EnsurePlayersExist(ZoneGroupState newData)
        {
            foreach(var info in newData.GetAllPlayers())
            {
                Player player = FindPlayerById(info.Uuid);
                if (player == null)
                {
                    try
                    {
                        var device = await Device.CreateAsync(info.Location);
                        {
                            player = await Player.CreatePlayerAsync(device, info);
                        }
                    }
                    catch (Exception)
                    {
                        Debug.WriteLine($"Problem finding {info.Name}");
                    }

                    if (player != null)
                    {
                        (this as ZoneGroupState.IZoneDelta).CreatePlayer(info.Uuid);
                        AddPlayer(player);
                    }
                }

                player?.Update(info);
            }
        }

        private readonly List<Player> playerList;

        public IReadOnlyList<Player> AllPlayerList
        {
            get
            {
                return playerList;
            }
        }

        public IReadOnlyList<Player> VisiblePlayerList
        {
            get
            {
                return playerList.Where(p => !p.Invisible).ToList();
            }
        }

        public bool VerifyGroupState()
        {
            // Verify that each group has a Coordinator, that the Coordinators are in the player list for the group, and that no Coordinator is in more than one group
            bool ok = Groups.All(g => g.Coordinator != null);
            ok &= Groups.All(g => g.FindPlayerById(g.Coordinator.UniqueName) != null);
            var coords = Groups.Select(g => g.Coordinator);
            ok &= coords.Count() == Groups.Count;
            return ok;
        }

        public Player ChooseAssociatedZP()
        {
            var first = VisiblePlayerList.FirstOrDefault(p => !p.HasBattery);
            if (first == null)
            {
                first = VisiblePlayerList.First();
            }
            AssociatedZP = first;
            return first;
        }

        private void AddPlayer(Player player)
        {
            // We will choose the first player as the AssociatedZP, UNLESS we picked a battery one first and a non-battery one came after
            if (player.ContentDirectory != null)
            {
                if (AssociatedZP == null)
                {
                    AssociatedZP = player;
                }
                else if (AssociatedZP.HasBattery && !player.HasBattery)
                {
                    AssociatedZP = player;
                }
            }

            playerList.Add(player);
        }

        internal void AddMissingPlayer(Player player)
        {
            // Don't ever add the same Player twice
            if (FindPlayerById(player.UniqueName) != null)
                throw new ArgumentException("player");

            AddPlayer(player);
        }

        public Player FindPlayerById(string uid)
        {
            return playerList.FirstOrDefault(p => p.UniqueName == uid);
        }

        public string FindPlayerProblem(string uid)
        {
            var q = GroupData?.Quarantined.FirstOrDefault(p=>p.Uuid == uid);
            if (q != null)
            {
                return $"Quarantined: {q.QuarantineReason}";
            }
            return null;
        }

        private Group FindGroupById(string uid)
        {
            return Groups.First(g => g.Coordinator.UniqueName == uid);
        }

        private Group FindGroupByIdOptional(string uid)
        {
            return Groups.FirstOrDefault(g => g.Coordinator.UniqueName == uid);
        }

        internal void AddGroup(Group group)
        {
            Groups.Add(group);
        }

        internal void RemoveGroup(Group group)
        {
            Groups.Remove(group);
        }

        public List<string> Summary()
        {
            var singles = new List<string>();
            var multiples = new List<string>();

            foreach (var g in Groups)
            {
                if (g.VisiblePlayers.Count != 1)
                {
                    string members = string.Join(",", g.VisiblePlayers.Select(p => p.RoomName));
                    multiples.Add(string.Format("Group {0}: {1}", g.Coordinator.RoomName, members));
                }
                else
                {
                    singles.Add(g.Coordinator.RoomName);
                }
            }

            multiples.Add("Groups: " + string.Join(",", singles));
            return multiples;
        }

        void ZoneGroupState.IZoneDelta.AddGroup(string coord)
        {
            ZoneLogger?.AddGroup(coord);

            var player = FindPlayerById(coord);
            var group = new Group(coord, new List<Player>() { player });
            Groups.Add(group);
        }

        void ZoneGroupState.IZoneDelta.RemoveGroup(string coord)
        {
            ZoneLogger?.RemoveGroup(coord);

            var group = FindGroupById(coord);
            Groups.Remove(group);
        }

        void ZoneGroupState.IZoneDelta.RoomMoved(string puid, string oldGroup, string newGroup)
        {
            ZoneLogger?.RoomMoved(puid, oldGroup, newGroup);

            var old = FindGroupById(oldGroup);
            var _new = FindGroupById(newGroup);
            var player = FindPlayerById(puid);
            old.RemovePlayer(player);
            _new.AddPlayer(player);
        }

        void ZoneGroupState.IZoneDelta.AddRoom(string coord, string puid)
        {
            ZoneLogger?.AddRoom(coord, puid);

            // We add the coord at Creation time, so don't do it again
            if (coord != puid)
            {
                var player = FindPlayerById(puid);
                var group = FindGroupById(coord);
                if (player != null)
                {
                    group.AddPlayer(player);
                }
                else
                {
                    Debug.WriteLine($"Player {puid} not found, cannot add to group {coord}");
                }
            }
        }

        void ZoneGroupState.IZoneDelta.RemoveRoom(string coord, string puid)
        {
            ZoneLogger?.RemoveRoom(coord, puid);

            var player = FindPlayerById(puid);
            var group = FindGroupByIdOptional(coord);
            if (group != null)
            {
                group.RemovePlayer(player);
            }
            else
            {
                // The Player was on its own, so its group is gone
                int todo = 1;
            }
        }

        void ZoneGroupState.IZoneDelta.CreatePlayer(string puid)
        {
            ZoneLogger?.CreatePlayer(puid);
        }

        void ZoneGroupState.IZoneDelta.DeletePlayer(string puid)
        {
            ZoneLogger?.DeletePlayer(puid);

            playerList.Remove(FindPlayerById(puid));
        }

        void ZoneGroupState.IZoneDelta.RenameRoom(string puid, string oldname, string newname, bool invisible)
        {
            ZoneLogger?.RenameRoom(puid, oldname, newname, invisible);

            var player = FindPlayerById(puid);
            player.Rename(newname, invisible);
        }
    }
}
