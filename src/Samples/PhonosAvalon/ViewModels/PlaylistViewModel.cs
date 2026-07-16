using OpenPhonos.Sonos;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PhonosAvalon.ViewModels
{
    public class PlaylistViewModel : ViewModelBase
    {
        public ObservableCollection<MusicItem> Items { get; }

        private uint PlaylistId;
        private PlayerCallbacks Callbacks;

        private MusicItem? _ItemToAdd;
        public MusicItem? ItemToAdd
        {
            get => _ItemToAdd;
            set => SetProperty(ref _ItemToAdd, value);
        }

        private bool _Busy;
        public bool Busy
        {
            get => _Busy;
            set => SetProperty(ref _Busy, value);
        }

        private string _StatusMessage;
        public string StatusMessage
        {
            get => _StatusMessage;
            set => SetProperty(ref _StatusMessage, value);
        }

        public PlaylistViewModel(PlayerCallbacks callbacks)
        {
            Items = new();
            PlaylistId = uint.MaxValue - 1;         // unlikely to be a "real" value
            Callbacks = callbacks;
        }

        public async Task RefreshAsync()
        {
            bool firstLoop = true;

            var enumerator = new SonosPlaylistEnumerator();
            Busy = true;
            StatusMessage = "Loading...";

            for (; ; )
            {
                var playlists = await enumerator.GetNextItemsAsync(Callbacks);
                if (playlists == null || playlists.Count() == 0)
                {
                    if (firstLoop)
                    {
                        Items.Clear();
                    }
                    break;
                }

                if (firstLoop)
                {
                    firstLoop = false;

                    // Same playlists as last time, do nothing
                    if (enumerator.UpdateID == PlaylistId)
                        break;

                    // Changed (or first time)
                    PlaylistId = enumerator.UpdateID;
                    Items.Clear();
                }

                foreach (var playlist in playlists)
                {
                    Items.Add(playlist);
                }
            }

            StatusMessage = string.Empty;
            Busy = false;
        }

        internal async Task SaveToPlaylistAsync(MusicItem item)
        {
            var playlist = (UPnPMusicItem)item;
            uint id = await Callbacks.Player.AddToPlaylistAsync(playlist.Id, PlaylistId, _ItemToAdd);
            PlaylistId = id;
            ItemToAdd = null;
        }
    }
}
