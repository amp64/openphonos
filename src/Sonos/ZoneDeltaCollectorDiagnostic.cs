using System.Collections.Generic;
using System.Diagnostics;

namespace OpenPhonos.Sonos
{
    public class ZoneDeltaCollectorDiagnostic : ZoneGroupState.IZoneDelta
    {
        readonly private ZoneGroupState Before, After;

        public List<string> Results { get; }

        public ZoneDeltaCollectorDiagnostic(ZoneGroupState oldState, ZoneGroupState newState)
        {
            Results = new List<string>();
            Before = oldState;
            After = newState;
        }

        private void Log(string message)
        {
            Results.Add(message);
            Debug.WriteLine(message);
        }

        public void AddGroup(string coord)
        {
            Log(string.Format("Group {0} added", After.GroupName(coord)));
        }

        public void AddRoom(string coord, string player)
        {
            Log(string.Format("{0} added (to {1})", After.PlayerName(player), After.GroupName(coord)));
        }

        public void RemoveGroup(string coord)
        {
            Log(string.Format("Group {0} removed", Before.GroupName(coord)));
        }

        public void RemoveRoom(string coord, string player)
        {
            Log(string.Format("{0} removed (from {1})", Before.PlayerName(player), Before.GroupName(coord)));
        }

        public void RoomMoved(string player, string oldGroup, string newGroup)
        {
            Log(string.Format("{0} moved from {1} to {2}", Before.PlayerNameWithVis(player), Before.GroupName(oldGroup), After.GroupName(newGroup)));
        }

        public void CreatePlayer(string player)
        {
            Log(string.Format("{0} new player", After.PlayerName(player)));
        }

        public void DeletePlayer(string player)
        {
            Log(string.Format("{0} deleted player", Before.PlayerName(player)));
        }

        public void RenameRoom(string puid, string oldname, string newname, bool invisible)
        {
            Log(string.Format("{0} renamed to {1}{2}", Before.PlayerNameWithVis(puid), After.PlayerNameWithVis(puid), invisible ? " Invisible" : string.Empty));
        }
    };
}
