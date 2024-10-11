using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using OpenPhonos.UPnP;

namespace OpenPhonos.Sonos
{
    /// <summary>
    /// A Group is a collection of Players that act as one and play together
    /// Every Group has a Coordinator, which is the Player that gets the audio from the source
    /// The coordinator then sends it on to all of the other Players in the group
    /// Almost all control commands (Play/Pause etc) go to the Coordinator
    /// A Group can include invisible devices (eg Paired surrounds, Subs, etc)
    /// </summary>
    public class Group
    {
        private readonly List<Player> PlayerList;        // includes invisible devices

        // This cannot be null
        public Player Coordinator { get; }

        /// <summary>
        /// This is always sorted by Room Name (except Coord first)
        /// </summary>
        public ObservableCollection<Player> VisiblePlayers { get; }

        public Group(string coordinatorId, List<Player> players)
        {
            Coordinator = players.FirstOrDefault(p => p.UniqueName == coordinatorId);
            if (Coordinator == null)
                throw new ArgumentException(nameof(coordinatorId));

            PlayerList = new List<Player>() { Coordinator };
            VisiblePlayers = new ObservableCollection<Player>() { Coordinator };
            VisiblePlayers.CollectionChanged += (s, e) => SendPlayersChangedEvent(e.Action.ToString());

            players.ForEach(p => AddPlayer(p));
        }

        public override string ToString()
        {
            return Coordinator.UniqueName + " " + Summary();
        }

        /// <summary>
        /// Find a player given its unique name
        /// </summary>
        /// <param name="uid">UniqueName (includes RINCON_ prefix)</param>
        /// <returns>Player object or null</returns>
        public Player FindPlayerById(string uid)
        {
            return PlayerList.FirstOrDefault(p => p.UniqueName == uid);
        }

        public IEnumerable<Player> GetEveryPlayer()
        {
            return PlayerList;
        }

        /// <summary>
        /// Produces a short summary of the Group, eg "RoomName + 3"
        /// </summary>
        /// <returns></returns>
        public string Summary()
        {
            int count = VisiblePlayers.Count();
            if (count == 1)
                return Coordinator.RoomName;
            else
                return string.Format("{0} + {1}", Coordinator.RoomName, count - 1);
        }

        internal void AddPlayer(Player player)
        {
            if (player == null)
                throw new ArgumentNullException("player");
            if (PlayerList.Contains(player))
            {
                // Its ok if we try to add the coordinator as it was added when the group was created, so we ignore it now
                if (player == Coordinator)
                    return;
                throw new ArgumentException("player duplicated");
            }
            if (VisiblePlayers.Contains(player))
                throw new ArgumentException("visible player duplicate");

            PlayerList.Add(player);
            if (!player.Invisible)
            {
                int where;
                // The VisiblePlayers list is alphabetic, with the coordinator first
                for (where = 0; where < VisiblePlayers.Count; where++)
                {
                    int cmp = player.RoomName.CompareTo(VisiblePlayers[where].RoomName);
                    if ((cmp < 0) && (where != 0))
                        break;
                }
                VisiblePlayers.Insert(where, player);
            }
        }

        /// <summary>
        /// Note this does NOT change the coordinator as we need that to identify Groups
        /// </summary>
        /// <param name="player"></param>
        internal void RemovePlayer(Player player)
        {
            if (player == null)
                throw new ArgumentNullException("player");

            PlayerList.Remove(player);
            VisiblePlayers.Remove(player);              // failure (eg invisible) is fine
        }

        public static async Task<List<Group>> CreateGroupsAsync(string groupstate)
        {
            var results = new ZoneGroupState(groupstate);

            var groups = new List<Group>();
            foreach(var item in results.Groups)
            {
                List<Player> players = new List<Player>();

                foreach(var data in item.Members)
                {
                    Player player = null;
                    try
                    {
                        var device = await Device.CreateAsync(data.Location);
                        player = await Player.CreatePlayerAsync(device, data.Name, data.Uuid, data.Invisible == "1");
                    }
                    catch (Exception)
                    {
                        // TODO this is NOT tested yet
                        player = Player.CreateMissingPlayer(data);
                    }

                    if (player != null)
                    {
                        players.Add(player);
                    }
                }

                // Find if some Vanished players were in this group once
                if (results.Vanished.Count != 0)
                {
                    string zoneid = item.Members.FirstOrDefault(p => p.ActiveZoneID != null)?.ActiveZoneID;
                    if (zoneid != null)
                    {
                        var vanished = results.Vanished.Where(p => p.ActiveZoneID == zoneid).ToList();
                        foreach (var v in vanished)
                        {
                            var player = Player.CreateMissingPlayer(v);
                            players.Add(player);
                        }
                        results.RemoveUnwantedPlayers(vanished);
                    }
                }

                // Ignore groups with a single, invisible player
                if ((players.Count != 0) && !players[0].Invisible)
                {
                    var group = new Group(item.Coordinator, players);
                    groups.Add(group);
                }
            }

            foreach (var vanished in results.Vanished)
            {
                // For each remaining Vanished player that we can't find the original Group for, we need to create a Group with just that player in it
                // TODO Untested
                var player = Player.CreateMissingPlayer(vanished);
                var group = new Group(vanished.Uuid, new List<Player>() { player });
                groups.Add(group);
            }

            return groups;
        }

        /// <summary>
        /// Given an existing Household that already has Players in it, create Groups in it
        /// </summary>
        /// <param name="household">Household to be filled-in</param>
        /// <param name="groupstate">xml for the zone state</param>
        public static async Task CreateGroupsForHouseholdAsync(Household household, string groupstate)
        {
            household.GroupData = new ZoneGroupState(groupstate);

            foreach(var group in household.GroupData.Groups)
            {
                // Ignore Groups that just contain a single invisible player as it is probably a BOOST
                if ((group.Members.Count == 1) && (group.Members[0].Invisible == "1"))
                    continue;

                var players = new List<Player>();

                foreach(var playerInfo in group.Members)
                {
                    if (playerInfo.Invisible == "1")
                        continue;

                    var player = household.FindPlayerById(playerInfo.Uuid);
                    if (player == null)
                    {
                        // If the network scan did not find the player with SSDP it may not be in the Household, so we'd better create one
                        try
                        {
                            var device = await Device.CreateAsync(playerInfo.Location);
                            player = await Player.CreatePlayerAsync(device);
                            household.AddMissingPlayer(player);
                        }
                        catch (Exception)
                        {
                            // Something bad happened with a Player, so bail on this whole group
                            continue;
                        }
                    }
                    players.Add(player);
                }

                var newGroup = new Group(group.Coordinator, players);
                household.AddGroup(newGroup);
            }
        }

        private readonly AutoResetEvent PlayersChangedEvent = new AutoResetEvent(false);

        private void SendPlayersChangedEvent(string diag)
        {
            Debug.WriteLine($"SendingPlayersChangedEvent {diag}");
            PlayersChangedEvent.Set();
        }

        // Wait for the PlayersChangedEvent (which indicates this Zone has changed) AND a user-provided condition
        private async Task<bool> WaitForPlayersChange(int timeout, Func<bool> condition)
        {
            bool ok;
            int tries = 1;

            for (; ; )
            {
                ok = condition();
                if (ok)
                    break;

                tries++;

                await Task.Run(() =>
                {
                    ok = PlayersChangedEvent.WaitOne(timeout);
                });
                Debug.Assert(ok);
            }

            Debug.WriteLine("PlayersChangedEvent {0} (after {1} tries)", ok, tries);
            return ok;
        }

        // This physically moves a player from this Zone to another, and waits until UPnP-completion
        // Can throw exceptions
        private async Task MovePlayerBetweenZones(Player player, Group newzone)
        {
            if (newzone == this)
            {
                SendPlayersChangedEvent("MovePlayers-nop");              // in case someone is waiting
                return;
            }

            string uri = "x-rincon:" + newzone.Coordinator.UniqueName;

            var moved = await player.AVTransport.SetAVTransportURI(0, uri, null);
            moved.ThrowIfFailed();
        }

        private const int ZoneChangeTimeout = 15 * 1000;

        // As above but waits for the Zone to be updated
        // Will throw on error
        public async Task MovePlayerToZoneAsync(Player player, Group newzone)
        {
            newzone.PlayersChangedEvent.Reset();
            await MovePlayerBetweenZones(player, newzone);
            await newzone.WaitForPlayersChange(ZoneChangeTimeout, () => newzone.VisiblePlayers.Contains(player));
        }


        // This physically removes a Player from a Zone (moving it to its own, exclusive Zone)
        // Can throw exceptions
        private async Task RemovePlayerFromZone(Player player)
        {
            // If this zone only has a single player, then do nothing
            if (this.PlayerList.Count == 1)
            {
                SendPlayersChangedEvent("RemovePlayer-nop");                  // in case someone is waiting
                return;
            }

            string uri = "x-rincon-queue:" + player.UniqueName + "#0";

            var orphan = await player.AVTransport.BecomeCoordinatorOfStandaloneGroup(0);
            orphan.ThrowIfFailed();

            var setavt = await player.AVTransport.SetAVTransportURI(0, uri, null);
            setavt.ThrowIfFailed();
        }

        // Removes player from Zone, waits for Zone event
        // Returns the (possibly changed) Zone, can return null if tried to remove the last player from a Zone and its not orphaned
        public async Task<Group> RemovePlayerFromZoneAsync(Player player, Household household)
        {
            PlayersChangedEvent.Reset();

            if (this.Coordinator != player)
            {
                await RemovePlayerFromZone(player);
                await WaitForPlayersChange(ZoneChangeTimeout, () =>
                {
                    return !PlayerList.Contains(player);
                });

                return this;
            }
            else
            {
                // Removing the coordinator from the group, which will create another group
                var others = this.PlayerList.Where(p => p != Coordinator).ToList();

                household.Groups.CollectionChanged += SendEventOnZoneChange;
                int groupcount = household.Groups.Count();

                try
                {
                    await RemovePlayerFromZone(player);
                    await WaitForPlayersChange(ZoneChangeTimeout, () =>
                    {
                        int change = household.Groups.Count() - groupcount;
                        return change > 0;
                    });

                    if (others.Count() == 0)
                        return null;

                    // We removed the coordinator, so the old Zone ('this') is toast, need to find the new Zone
                    // We do this by looking for any of the original Other players as the coord
                    foreach (var z in household.Groups)
                    {
                        if (others.Contains(z.Coordinator))
                            return z;
                    }

                    Debug.Assert(false, "Cannot find new Zone after RemovePlayer");
                    return null;
                }
                finally
                {
                    household.Groups.CollectionChanged -= SendEventOnZoneChange;
                }
            }
        }

        private void SendEventOnZoneChange(object sender, NotifyCollectionChangedEventArgs e)
        {
            SendPlayersChangedEvent("ZoneChange");
        }
    }
}
