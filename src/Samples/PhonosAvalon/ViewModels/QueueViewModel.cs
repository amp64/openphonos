using OpenPhonos.Sonos;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using static PhonosAvalon.ViewModels.NowPlayingViewModel;

namespace PhonosAvalon.ViewModels
{
    public class QueueViewModel : MusicPickerViewModel
    {
        private int _CurrentIndex;
        private QueueItemEnumerator? _QueueEnumerator;

        private bool _InUse;
        public bool InUse
        {
            get => _InUse;
            private set => SetProperty(ref _InUse, value);
        }

        // -1 means nothing, else 0->total-1
        public int CurrentIndex
        {
            get => _CurrentIndex;
            set => SetProperty(ref _CurrentIndex, value);
        }

        private int _PlaylistIndex;
        public int PlaylistIndex
        {
            get => _PlaylistIndex;
            set => SetProperty(ref _PlaylistIndex, value);
        }

        private uint _QueueSize;
        public uint QueueSize
        {
            get => _QueueSize;
            private set => SetProperty(ref _QueueSize, value);
        }

        private string? _StateMessage;
        public string? StateMessage
        {
            get => _StateMessage;
            set => SetProperty(ref _StateMessage, value);
        }

        internal const string NewId = "NEW";

        // This is the state machine for the UX
        public enum EState
        {
            View,
            PreClear,
            PostClear,
            PreSave,
            PreSaveNew,
            PostSave,
            Message,
            Error
        }

        private EState _EditState;
        public EState EditState
        {
            get => _EditState;
            set => SetProperty(ref _EditState, value);
        }

        public ObservableCollection<UPnPMusicItem> Playlists { get; private set; }

        private GroupViewModel ParentGroup;

        public QueueViewModel(GroupViewModel g) : base(g?.Coordinator, null)
        {
            CurrentIndex = -1;
            InUse = false;
            QueueSize = 0;
            PlayPauseCommand = new PlayorPauseCommand(g);
            ParentGroup = g;

            PreClearCommand = new ActionCommand((o) => EditState = EState.PreClear);
            PostClearCommand = new ClearCommand(g);
            SaveQueueCommand = new SaveCommand(g);

            NoCommand = new ActionCommand((o)=>
            {
                ResetEditState();
            });

            ResetEditState();
            Playlists = [new UPnPMusicItem("New...", false, NewId)];
        }

        public void ResetEditState()
        {
            EditState = EState.View;
            StateMessage = "";
            PlaylistIndex = -1;
        }

        public ICommand PlayPauseCommand { get; }
        public ICommand SaveQueueCommand { get; }
        public ICommand PreClearCommand { get; }
        public ICommand PostClearCommand { get; }
        public ICommand NoCommand { get; }

        protected override IMusicItemEnumerator RootEnumerator(MusicServiceProvider m)
        {
            _QueueEnumerator = new QueueItemEnumerator();
            _QueueEnumerator.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(QueueItemEnumerator.QueueSize))
                {
                    var qsize = (uint)_QueueEnumerator.QueueSize;
                    if (qsize == 1 && (_QueueEnumerator.TryGetItem(0) as QueueMusicItem).IsError)
                    {
                        // an empty queue contains one Error item, but size is zero
                        qsize = 0;
                    }
                    QueueSize = qsize;
                }
                else if (e.PropertyName == nameof(QueueItemEnumerator.DoUpdateCurrentTrack))
                {
                    var later = UpdateCurrentTrackAsync();
                }
            };
            return _QueueEnumerator;
        }

        /// <summary>
        /// Use this when subscribing to the player's QueueChanged event
        /// Updates the viewmodel to suit
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void QueueChanged(object sender, Player.ContentChangedEventArgs args)
        {
            _QueueEnumerator?.OnQueueChanged(args);
        }

        /// <summary>
        /// Called when the current track has changed
        /// </summary>
        /// <param name="track">0 means none, 1 means first track</param>
        public void TrackChanged(string uri, uint track)
        {
            bool isPauseVis = false;
            bool isPlayingVis = false;

            if (_QueueEnumerator?.TryGetItem(CurrentIndex) is QueueMusicItem item)
            {
                isPauseVis = item.PauseVisible;
                isPlayingVis = item.PlayVisible;

                item.PlayVisible = false;
                item.PauseVisible = false;
            }

            InUse = uri != null && uri.StartsWith("x-rincon-queue:");
            CurrentIndex = (int)track - 1;
            Heading = InUse ? "Queue" : "Queue (not in use)";

            if (_QueueEnumerator?.TryGetItem(CurrentIndex) is QueueMusicItem newItem)
            {
                newItem.PauseVisible = isPauseVis;
                newItem.PlayVisible = isPlayingVis;
            }
        }

        /// <summary>
        /// Called when the current playstate has changed
        /// </summary>
        /// <param name="track">0 means none, 1 means first track</param>
        /// <param name="playstate">The player state</param>
        public void PlayStateChanged(string uri, PlayStateType playstate)
        {
            InUse = uri != null && uri.StartsWith("x-rincon-queue:");
            Heading = InUse ? "Queue" : "Queue (not in use)";

            if (_QueueEnumerator?.TryGetItem(CurrentIndex) is QueueMusicItem newItem)
            {
                switch (playstate)
                {
                    case PlayStateType.PLAYING:
                        newItem.PauseVisible = true;        // should be InUse, but uri is null on startup
                        newItem.PlayVisible = false;
                        break;
                    case PlayStateType.PAUSED_PLAYBACK:
                    case PlayStateType.STOPPED:
                        newItem.PlayVisible = true;
                        newItem.PauseVisible = false;
                        break;
                }
            }
        }

        // It is assumed the previous current track has been cleared already
        private async Task UpdateCurrentTrackAsync()
        {
            if (ParentGroup == null)
                return;

            CurrentIndex = await ParentGroup.NowPlaying.GetCurrentTrackAsync() - 1;

            if (_QueueEnumerator?.TryGetItem(CurrentIndex) is QueueMusicItem item)
            {
                switch (ParentGroup.NowPlaying.PlayState)
                {
                    case PlayStateType.PLAYING:
                        item.PauseVisible = InUse;
                        item.PlayVisible = false;
                        break;
                    case PlayStateType.PAUSED_PLAYBACK:
                    case PlayStateType.STOPPED:
                        item.PlayVisible = true;
                        item.PauseVisible = false;
                        break;
                }
            }
        }

        public void QueueItemClicked(QueueMusicItem music, int which)
        {
            if (_QueueEnumerator?.TryGetItem(CurrentIndex) is QueueMusicItem item && CurrentIndex != which)
            {
                // Going to change the current one, so clear the old one's buttons
                item.PlayVisible = false;
                item.PauseVisible = false;
            }

            // this is the item we are about to play/pause
            CurrentIndex = which;
            PlayPauseCommand.Execute(music);
        }

        public void DeleteQueueItem(QueueMusicItem music, int index)
        {
            _QueueEnumerator.SetUpdateExpectation(index);
            var later = ParentGroup.NowPlaying.DeleteFromQueue(music, _QueueEnumerator.UpdateId);
        }

        private async Task<IEnumerable<UPnPMusicItem>> GetPlaylistsAsync()
        {
            try
            {
                if (ParentGroup?.Coordinator != null)
                {
                    return await ParentGroup.Coordinator.GetPlaylistsAsync();
                }
            }
            catch (Exception ex)
            {
                StateMessage = ex.Message;
                EditState = EState.Error;
            }

            return null;
        }

        private async Task ClearQueueAsync(Player coordinator)
        {
            try
            {
                await coordinator.ClearQueueAsync();
                EditState = EState.View;
            }
            catch (Exception ex)
            {
                StateMessage = ex.Message;
                EditState = EState.Error;
            }
        }

        private void DoPreSave()
        {
            PlaylistIndex = -1;

            // Clear out the old list (leaving the first one there)
            while (Playlists.Count > 1)
            {
                Playlists.RemoveAt(1);
            }

            EditState = EState.PreSave;

            _ = GetPlaylistsAsync().ContinueWith((t) =>
            {
                if (t.Result != null)
                {
                    foreach (var item in t.Result)
                    {
                        Playlists.Add(item);
                    }
                }
            });
        }

        private void DoRealSave(object? parameter)
        {
            string name;

            if (parameter is UPnPMusicItem music)
            {
                if (music.Id != NewId)
                {
                    // This is a pre-existing Playlist so will need overwriting
                    name = music.Title;
                    ParentGroup.Queue.StateMessage = $"Updating '{name}'...";
                    ParentGroup.Queue.EditState = EState.Message;
                    _ = ParentGroup.Queue.SaveQueueAsync(ParentGroup.Coordinator!, music);
                }
                else
                {
                    ParentGroup.Queue.EditState = EState.PreSaveNew;
                }
            }
            else
            {
                name = parameter.ToString();
                ParentGroup.Queue.StateMessage = $"Saving '{name}'...";
                ParentGroup.Queue.EditState = EState.Message;
                _ = ParentGroup.Queue.SaveQueueAsync(ParentGroup.Coordinator!, name);
            }
        }

        private void DoSave()
        {
            switch (EditState)
            {
                case EState.PreSave:
                    if (PlaylistIndex >= 0)
                    {
                        DoRealSave(Playlists[PlaylistIndex]);
                    }
                    break;

                case EState.PreSaveNew:
                    DoRealSave(NewPlaylistName);
                    break;

                default:
                    DoPreSave();
                    break;
            }
        }

        private abstract class QueueCommandBase : ICommand
        {
            public event EventHandler? CanExecuteChanged;

            protected GroupViewModel ParentGroup;

            public QueueCommandBase(GroupViewModel g)
            {
                ParentGroup = g;
            }

            public virtual bool CanExecute(object? parameter)
            {
                var b = ParentGroup?.Coordinator?.IsRealPlayer;
                return b.HasValue && b.Value;
            }

            public abstract void Execute(object? parameter);
        }

        private class PlayorPauseCommand : QueueCommandBase
        {
            public PlayorPauseCommand(GroupViewModel g) : base(g)
            {
            }

            override public bool CanExecute(object? parameter)
            {
                return true;
            }

            override public void Execute(object? parameter)
            {
                if (parameter is QueueMusicItem music)
                {
                    uint which = music.PauseVisible ? 0 : (uint)ParentGroup.Queue.CurrentIndex + 1;
                    bool resume = ParentGroup.Queue.InUse && music.PlayVisible;
                    var later = ParentGroup.NowPlaying.PlayPauseFromQueue(music, which, resume);
                }
            }
        }

        private class ClearCommand : QueueCommandBase
        {
            public ClearCommand(GroupViewModel g) : base(g)
            {
            }

            override public void Execute(object? parameter)
            {
                _ = ParentGroup.Queue.ClearQueueAsync(ParentGroup.Coordinator!);
            }
        }

        private string _NewPlaylistName;
        public string NewPlaylistName
        {
            get => _NewPlaylistName;
            set => SetProperty(ref _NewPlaylistName, value);    
        }

        private class SaveCommand : QueueCommandBase
        {
            public SaveCommand(GroupViewModel g) : base(g)
            {
            }

            override public void Execute(object? parameter)
            {
                ParentGroup.Queue.DoSave();
            }
        }

        private async Task SaveQueueAsync(Player player, object what)
        {
            try
            {
                if (what is string name)
                {
                    if (Playlists.Any(p => p.Title == name))
                    {
                        throw new ArgumentException("A playlist with that name already exists, to replace it, select it from the list instead.");
                    }
                    else if (string.IsNullOrEmpty(name))
                    {
                        throw new ArgumentException("Playlist name cannot be empty");
                    }
                    await player.SaveQueueAsNewPlaylistAsync(name);
                }
                else if (what is UPnPMusicItem music)
                {
                    await player.SaveQueueReplacePlaylistAsync(music);
                }
                else
                {
                    throw new ArgumentException(nameof(what));
                }
                EditState = EState.View;
            }
            catch (Exception ex)
            {
                StateMessage = $"Error: {ex.Message}";
                EditState = EState.Error;
            }
        }
    }

    public class FakeQueueViewModel : QueueViewModel
    {
        public FakeQueueViewModel() : base(null)
        {
        }

        protected override IMusicItemEnumerator RootEnumerator(MusicServiceProvider m)
        {
            return new EmptyEnumerator("Queue");
        }
    }
}
