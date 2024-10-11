using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace OpenPhonos.Sonos
{
    /// <summary>
    /// This is an in-memory representation of the critical parts of a ZoneGroupState DOM with helper functions
    /// </summary>
    public class ZoneGroupState
    {
        public class ZoneGroupMemberData
        {
            public string Location;
            public string Uuid;
            public string Name;
            public string Invisible;
            public string MoreInfo;
            public string ActiveZoneID;

            public override string ToString()
            {
                return string.Format("{0} {1} {2}", Name, Uuid, Location);
            }
        }

        public class ZoneGroupMemberDataVanished : ZoneGroupMemberData
        {
            public string Mac;
            public string LastSeen;
            public string Reason;
            public string ModelInfo;
        }

        public class ZoneGroupData
        {
            readonly public string Coordinator;
            readonly public string ID;
            readonly public List<ZoneGroupMemberData> Members;

            public ZoneGroupData(string coord, string id, IEnumerable<ZoneGroupMemberData> members)
            {
                Members = members.ToList();
                ID = id;
                Coordinator = coord;

                if (string.IsNullOrEmpty(coord))
                {
                    // Occasionally the Coordinator is empty, so assume the first[/only] member is the one
                    Coordinator = Members[0].Uuid;
                }
            }

            public bool Contains(string uuid)
            {
                return Members.FirstOrDefault(m => m.Uuid == uuid) != null;
            }

            public override string ToString()
            {
                var player = Members.FirstOrDefault(m => m.Uuid == Coordinator);
                if (player != null)
                {
                    return string.Format("{0}-{1}", player.Name, Coordinator);
                }
                else
                {
                    return Coordinator;
                }
            }
        }

        /// <summary>
        /// This interface is told what changes occur between once instance of ZoneGroupState and another
        /// Only uuids are passed as arguments, the caller has to turn those into more useful items
        /// </summary>
        public interface IZoneDelta
        {
            void AddGroup(string coord);
            void RemoveGroup(string coord);
            void RoomMoved(string puid, string oldGroup, string newGroup);
            void AddRoom(string coord, string puid);
            void RemoveRoom(string coord, string puid);
            void RenameRoom(string puid, string oldname, string newname, bool invisibility);
            void CreatePlayer(string puid);
            void DeletePlayer(string puid);
        }

        private static IEnumerable<ZoneGroupData> ParseZoneGroupState(XElement elements)
        {
            var groups = from item in elements.Descendants(XName.Get("ZoneGroup"))
                         select new ZoneGroupData(
                             ((string)item.Attribute(XName.Get("Coordinator"))),
                             ((string)item.Attribute(XName.Get("ID"))),
                             ParseZoneGroupMembers(item.Descendants(XName.Get("ZoneGroupMember")).Concat(item.Descendants(XName.Get("Satellite"))))
                             );
            return groups;
        }

        private static IEnumerable<ZoneGroupMemberDataVanished> ParseVanishedDevices(XElement elements)
        {
            var devices = from device in elements.Descendants(XName.Get("VanishedDevices")).Descendants(XName.Get("Device"))
                          select new ZoneGroupMemberDataVanished()
                          {
                              Name = (string)device.Attribute("ZoneName"),
                              Location = (string)device.Attribute("LastKnownIP"),
                              Uuid = (string)device.Attribute("UUID"),
                              MoreInfo = (string)device.Attribute("MoreInfo"),
                              LastSeen = (string)device.Attribute("LastSeenUTC"),
                              Mac = (string)device.Attribute("Mac"),
                              Reason = (string)device.Attribute("Reason"),
                              ModelInfo = (string)device.Attribute("ModelInfo"),
                              ActiveZoneID = (string)device.Attribute("ActiveZoneID")
                          };
            return devices;
        }

        private static List<ZoneGroupMemberData> ParseZoneGroupMembers(IEnumerable<XElement> descendants)
        {
            var members = from item in descendants
                          select new ZoneGroupMemberData()
                          {
                              Location = (string)item.Attribute(XName.Get("Location")),
                              Uuid = (string)item.Attribute(XName.Get("UUID")),
                              Name = (string)item.Attribute(XName.Get("ZoneName")),
                              Invisible = (string)item.Attribute(XName.Get("Invisible")),
                              MoreInfo = (string)item.Attribute(XName.Get("MoreInfo")),
                              ActiveZoneID = (string)item.Attribute(XName.Get("ActiveZoneID"))
                          };

            return members.ToList();
        }

        readonly private List<ZoneGroupData> Zones;
        private List<ZoneGroupMemberDataVanished> VanishedPlayers;

        public IReadOnlyList<ZoneGroupData> Groups { get => Zones; }
        public IReadOnlyList<ZoneGroupMemberDataVanished> Vanished { get => VanishedPlayers; }

        public ZoneGroupState(string zonestate)
        {
            var xml = XElement.Parse(zonestate);
            Zones = ParseZoneGroupState(xml).ToList();
            VanishedPlayers = ParseVanishedDevices(xml).ToList();
        }

        public ZoneGroupState() : this("<ZoneGroupState></ZoneGroupState>")
        {
        }

        /// <summary>
        /// Returns a map of Player-uuid and Coordinator
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, string> BuildPlayerCoordList()
        {
            var result = new Dictionary<string, string>();
            Zones.ForEach(group =>
            {
                group.Members.ForEach(member => result[member.Uuid] = group.Coordinator);
            });

            return result;
        }

        public IEnumerable<ZoneGroupMemberData> GetAllPlayers()
        {
            var result = new List<ZoneGroupMemberData>();
            Zones.ForEach(group =>
            {
                result.AddRange(group.Members);
            });
            return result;
        }

        /// <summary>
        /// Find the group based on the coordinator
        /// </summary>
        /// <param name="coord">coordinator uuid</param>
        /// <returns>Group or null</returns>
        public ZoneGroupData FindGroup(string coord)
        {
            return Zones.FirstOrDefault(g => g.Coordinator == coord);
        }

        /// <summary>
        /// Find the name of a player, which must exist
        /// </summary>
        /// <param name="uuid">player uuid</param>
        /// <returns>Name</returns>
        public string PlayerName(string uuid)
        {
            var group = Zones.First(z => z.Contains(uuid));
            return group.Members.First(m => m.Uuid == uuid).Name;
        }

        /// <summary>
        /// Find the name of a player, which must exist, wrap with brackets if invisible
        /// </summary>
        /// <param name="uuid">player uuid</param>
        /// <returns>Name</returns>
        public string PlayerNameWithVis(string uuid)
        {
            var group = Zones.First(z => z.Contains(uuid));
            var player = group.Members.First(m => m.Uuid == uuid);
            string result = player.Name;
            if (player.Invisible == "1")
                result = "(" + result + ")";
            return result;
        }

        /// <summary>
        /// Find the invisibility of a player, which must exist
        /// </summary>
        /// <param name="uuid">player uuid</param>
        /// <returns>Name</returns>
        public bool PlayerVisibility(string uuid)
        {
            var group = Zones.First(z => z.Contains(uuid));
            return group.Members.First(m => m.Uuid == uuid).Invisible == "1";
        }

        /// <summary>
        /// Find the name of a group, which must exist
        /// </summary>
        /// <param name="coord">Coordinator uuid</param>
        /// <returns></returns>
        public string GroupName(string coord)
        {
            var group = Zones.First(z => z.Coordinator == coord);
            return group.Members.First(m => m.Uuid == coord).Name;
        }

        /// <summary>
        /// Remove a list of players from the state
        /// </summary>
        /// <param name="puids"></param>
        public void RemoveUnwantedPlayers(IEnumerable<string> puids)
        {
            foreach (var puid in puids)
            {
                var group = Zones.First(z => z.Contains(puid));
                var member = group.Members.First(m => m.Uuid == puid);
                group.Members.Remove(member);
                if (group.Members.Count == 0)
                {
                    Zones.Remove(group);
                }
            }
        }

        public void RemoveUnwantedPlayers(IEnumerable<ZoneGroupMemberDataVanished> vanished)
        {
            VanishedPlayers = Vanished.Where(v => !vanished.Contains(v)).ToList();
        }

        /// <summary>
        /// Compare two states and return a list of players that are in the new state but not in the old state
        /// </summary>
        /// <param name="oldState"></param>
        /// <param name="newState"></param>
        /// <returns>List of puids</returns>
        public static IList<ZoneGroupMemberData> FindNewPlayers(ZoneGroupState oldState, ZoneGroupState newState)
        {
            var oldPlayers = oldState.GetAllPlayers();
            var newPlayers = newState.GetAllPlayers();
            var brandNew = new List<ZoneGroupMemberData>();

            foreach (var player in newPlayers)
            {
                if (oldPlayers.FirstOrDefault(p => p.Uuid == player.Uuid) == null)
                {
                    brandNew.Add(player);
                }
            }

            return brandNew;
        }

        /// <summary>
        /// Compare two instances and describe how they differ
        /// </summary>
        /// <param name="oldState">The original zone state</param>
        /// <param name="newState">The updated zone state</param>
        /// <param name="handler">The interface by which all changes will be described</param>
        public static void CalculateDifferences(ZoneGroupState oldState, ZoneGroupState newState, IZoneDelta handler)
        {
            // Find new groups
            // Find players that moved between groups
            // Find new players (never seen before)
            // Find removed groups
            // Find vanishing players (never seen again)

            var newPlayers = newState.BuildPlayerCoordList();
            var oldPlayers = oldState.BuildPlayerCoordList();

            // Find new groups
            foreach (var newG in newState.Groups)
            {
                var oldG = oldState.FindGroup(newG.Coordinator);
                if (oldG == null)
                {
                    // Ignore groups containing single invisible members (eg a BOOST)
                    if ((newG.Members.Count > 1) || (newG.Members[0].Invisible != "1"))
                    {
                        handler.AddGroup(newG.Coordinator);
                    }
                }
            };

            foreach (var oldPlayer in oldPlayers)
            {
                if (newPlayers.TryGetValue(oldPlayer.Key, out string newCoord))
                {
                    if (oldPlayer.Value != newCoord)
                    {
                        handler.RoomMoved(oldPlayer.Key, oldPlayer.Value, newCoord);
                    }
                }
                else
                {
                    handler.RemoveRoom(oldPlayer.Value, oldPlayer.Key);
                }
            }

            foreach (var newPlayer in newPlayers)
            {
                if (!oldPlayers.ContainsKey(newPlayer.Key))
                {
                    handler.AddRoom(newPlayer.Value, newPlayer.Key);
                }
            }

            // Find lost groups
            foreach (var oldG in oldState.Groups)
            {
                var newG = newState.FindGroup(oldG.Coordinator);
                if (newG == null)
                {
                    handler.RemoveGroup(oldG.Coordinator);
                }
            }

            // Find players that have gone away
            foreach (var oldPlayer in oldPlayers)
            {
                if (!newPlayers.ContainsKey(oldPlayer.Key))
                {
                    handler.DeletePlayer(oldPlayer.Key);
                }
            }

            // Find players that have been renamed
            foreach (var player in newPlayers)
            {
                if (oldPlayers.ContainsKey(player.Key))
                {
                    if (
                        (newState.PlayerName(player.Key) != oldState.PlayerName(player.Key)) ||
                        (newState.PlayerVisibility(player.Key) != oldState.PlayerVisibility(player.Key))
                       )
                    {
                        handler.RenameRoom(player.Key, oldState.PlayerName(player.Key), newState.PlayerName(player.Key), newState.PlayerVisibility(player.Key));
                    }
                }
            }
        }
    }
}
