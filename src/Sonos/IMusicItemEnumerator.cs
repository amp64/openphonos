using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;

namespace OpenPhonos.Sonos
{
    public enum ExportType
    {
        Xml,
        Text,
        Playlist,
        SoundIIZ,
    };

    public interface IMusicItemEnumerator
    {
        // Can return -1 if cannot compute
        Task<int> CountAsync(PlayerCallbacks callbacks);
        string DisplayName { get; }
        string ArtUri { get; }
        void Reset();
        DisplayData ParentDisplayMode { get; }
        Task<IEnumerable<MusicItem>> GetNextItemsAsync(PlayerCallbacks callbacks);
        IMusicItemEnumerator GetChildrenEnumerator(MusicItem parent);
        bool CanExport(ExportType exportType);
        Task<string> ExportAsync(PlayerCallbacks player, ExportType exportType);
        MusicItem EmptyItem { get; }
    }

    public interface IMusicItemSearchable
    {
        bool CanSearch { get; }
        IMusicItemEnumerator GetSearchEnumerator(string search);
    }

    public interface IMusicItemEnumeratorObservable : IMusicItemEnumerator
    {
        /// <param name="source">The coordinator player</param>
        /// <returns>ObservableCOllection that contains/will contain the items</returns>
        ReadOnlyObservableCollection<MusicItem> GetAsObservable(Player source);

        /// <summary>
        /// Start reading the items. Call only once per instance.
        /// </summary>
        /// <param name="source">The coordinator player</param>
        Task StartReadAsync(Player source);

        /// <summary>
        /// Pause the reading of the items
        /// </summary>
        void PauseRead();

        /// <summary>
        /// Resumes a paused Read
        /// </summary>
        /// <returns>true if there are more, false if already finished</returns>
        bool ResumeRead();

        /// <summary>
        /// Stop the reading of items. The eumerator should be considered finished
        /// </summary>
        void StopRead();

        /// <summary>
        /// The coordinator device has changed, update the items if necessary
        /// </summary>
        /// <param name="source">Coordinator</param>
        /// <returns>true if something changed</returns>
        Task<bool> UpdateAsync(Player source);
    }

    /// <summary>
    /// This class is used whenever a enumerator needs to use a Player or Players
    /// </summary>
    public class PlayerCallbacks
    {
        // Returns the Player to use, cannot be null
        public Player Player => player();

        // Returns a list of all players (including invisible ones)
        // Will never return null, but can return an empty list if the caller never set it
        public IEnumerable<Player> AllPlayers
        {
            get
            {
                if (allPlayers != null)
                    return allPlayers();
                Debug.WriteLine("Warning: PlayerCallbacks.AllPlayers not set");
                return new List<Player>();
            }
        }

        private Func<Player> player;
        private Func<IEnumerable<Player>> allPlayers;

        public PlayerCallbacks(Func<Player> getplayer, Func<IEnumerable<Player>> getallplayers = null)
        {
            player = getplayer;
            allPlayers = getallplayers;
        }
    }
}
