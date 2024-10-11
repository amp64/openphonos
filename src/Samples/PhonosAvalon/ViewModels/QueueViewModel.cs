using OpenPhonos.Sonos;
using System;
using System.Collections.Generic;
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

        private uint _QueueSize;
        public uint QueueSize
        {
            get => _QueueSize;
            private set => SetProperty(ref _QueueSize, value);
        }

        public QueueViewModel(GroupViewModel g) : base(g?.Coordinator, null)
        {
            CurrentIndex = -1;
            InUse = false;
            QueueSize = 0;
            PlayPauseCommand = new PlayorPauseCommand(g);
        }

        public ICommand PlayPauseCommand { get; }

        protected override IMusicItemEnumerator RootEnumerator(MusicServiceProvider m)
        {
            _QueueEnumerator = new QueueItemEnumerator();
            _QueueEnumerator.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(QueueItemEnumerator.QueueSize))
                {
                    QueueSize = (uint)_QueueEnumerator.QueueSize;
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
        public void QueueChanged(object sender, Player.QueueChangedEventArgs args)
        {
            _QueueEnumerator?.OnQueueChanged(args);
        }

        /// <summary>
        /// Called when the current track or playstate has changed
        /// </summary>
        /// <param name="track">0 means none, 1 means first track</param>
        /// <param name="playstate">The player state</param>
        public void TrackChanged(string uri, uint track, PlayStateType playstate)
        {
            if (_QueueEnumerator?.TryGetItem(CurrentIndex) is QueueMusicItem item)
            {
                item.PlayVisible = false;
                item.PauseVisible = false;
            }

            InUse = uri != null && uri.StartsWith("x-rincon-queue:");
            CurrentIndex = (int)track - 1;
            Heading = InUse ? "Queue" : "Queue (not in use)";

            if (_QueueEnumerator?.TryGetItem(CurrentIndex) is QueueMusicItem newItem)
            {
                switch (playstate)
                {
                    case PlayStateType.PLAYING:
                        newItem.PauseVisible = InUse;
                        break;
                    case PlayStateType.PAUSED_PLAYBACK:
                    case PlayStateType.STOPPED:
                        newItem.PlayVisible = true;
                        break;
                }
            }
        }

        public void QueueItemClicked(QueueMusicItem music, int which)
        {
            if (_QueueEnumerator?.TryGetItem(CurrentIndex) is QueueMusicItem item && CurrentIndex != which)
            {
                item.PlayVisible = false;
                item.PauseVisible = false;
            }

            music.PauseVisible = false;
            CurrentIndex = which;
            PlayPauseCommand.Execute(music);
        }

        private class PlayorPauseCommand : ICommand
        {
            public event EventHandler? CanExecuteChanged;

            public bool CanExecute(object? parameter)
            {
                return true;
            }

            private GroupViewModel ParentGroup;

            public PlayorPauseCommand(GroupViewModel g)
            {
                ParentGroup = g;
            }

            public void Execute(object? parameter)
            {
                if (parameter is QueueMusicItem music)
                {
                    uint which = music.PauseVisible ? 0 : (uint)ParentGroup.Queue.CurrentIndex + 1;
                    bool resume = ParentGroup.Queue.InUse && music.PlayVisible;
                    var later = ParentGroup.NowPlaying.PlayPauseFromQueue(music, which, resume);
                }
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
