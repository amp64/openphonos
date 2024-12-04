using CommunityToolkit.Mvvm.ComponentModel;
using OpenPhonos.Sonos;
using OpenPhonos.UPnP;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PhonosAvalon.ViewModels
{
    public class GroupViewModel : ObservableObject
    {
        public static bool WatchDeviceEvents = true;

        private readonly Group ActualGroup;
        private ICommand _PlayNowCommand;
        private ICommand _PlayNextCommand;
        private ICommand _AddToQueueCommand;
        private ICommand _ReplaceQueueCommand;
        private readonly Household _Household;
        protected NowPlayingViewModel _NowPlaying;
        protected MusicPickerViewModel _MusicSource;
        protected QueueViewModel _Queue;
        protected IVolume _GroupVolume;
        protected VolumeHandler _VolumeHandler;

        // We only want one MusicPickerViewModel per household
        private static Dictionary<MusicServiceProvider, MusicPickerViewModel> _MusicPickers = new Dictionary<MusicServiceProvider, MusicPickerViewModel>();

        public NowPlayingViewModel NowPlaying => _NowPlaying.WithSubscription();

        public MusicPickerViewModel MusicSource => _MusicSource;

        public QueueViewModel Queue => _Queue;

        public IVolume GroupVolume => _GroupVolume;

        public Player? Coordinator => ActualGroup?.Coordinator;

        public ICommand PlayNowCommand => _PlayNowCommand;
        public ICommand PlayNextCommand => _PlayNextCommand;
        public ICommand AddToQueueCommand => _AddToQueueCommand;
        public ICommand ReplaceQueueCommand => _ReplaceQueueCommand;

        private bool _GroupEditorInvalid;
        private GroupEditorViewModel? _GroupEditor;
        public GroupEditorViewModel? GroupEditor { get { UpdateGroupEditor(false); return _GroupEditor; } set => SetProperty(ref _GroupEditor, value); }

        public ObservableCollection<PlayerViewModel> Players { get; }

        public GroupViewModel(Group group, Household h)
        {
            ActualGroup = group;
            ActualGroup.VisiblePlayers.CollectionChanged += VisiblePlayers_CollectionChanged;

            _Household = h;
            _NowPlaying = NowPlayingViewModel.Get(group, h.MusicServices);
            _GroupVolume = new GroupVolumeViewModel(this);
            _VolumeHandler = new VolumeHandler(GroupVolumeHandlerAsync);
            _Queue = new QueueViewModel(this);
            if (!_MusicPickers.TryGetValue(h.MusicServices, out _MusicSource))
            {
                _MusicSource = new MusicPickerViewModel(group.Coordinator, h.MusicServices);
                _MusicPickers[h.MusicServices] = _MusicSource;
            }
            _NowPlaying.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(NowPlayingViewModel.TrackNumber) || e.PropertyName==nameof(NowPlayingViewModel.PlayState))
                {
                    _Queue.TrackChanged(_NowPlaying.AVTransportUri, _NowPlaying.TrackNumber, _NowPlaying.PlayState);
                }
            };

            _PlayNowCommand = new PlayCommand(Coordinator, PlayType.PlayNow);
            _PlayNextCommand = new PlayCommand(Coordinator, PlayType.PlayNext);
            _AddToQueueCommand = new PlayCommand(Coordinator, PlayType.AddToQueue);
            _ReplaceQueueCommand = new PlayCommand(Coordinator, PlayType.ReplaceQueue);

            Players = new ObservableCollection<PlayerViewModel>(ActualGroup.VisiblePlayers.Select(p => new PlayerViewModel(p)));
        }

        private void VisiblePlayers_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    var vm = new PlayerViewModel(e.NewItems[0] as Player);
                    Players.Add(vm);
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    var existing = FindPlayerVM(e.OldItems[0] as Player);
                    if (existing != null)
                    {
                        Players.Remove(existing);
                    }
                    break;
                default:
                    int x = 42;
                    break;
            }

            _GroupEditorInvalid = true;
            OnPropertyChanged(nameof(Summary));
        }

        public async Task SubscribeAsync()
        {
            {
                var what = new PlayerEventSelector()
                {
                    GroupRenderer = ActualGroup.Coordinator.HasVolume,
                    ContentDirectory = true,
                };
                ActualGroup.Coordinator.PropertyChanged += Coordinator_PropertyChanged;
                ActualGroup.Coordinator.QueueChangedEvent += Queue.QueueChanged;
                await ActualGroup.Coordinator.SubscribeToEventsAsync(Listener.MinimumTimeout, what);
            }

            if (WatchDeviceEvents)
            {
                foreach (var p in ActualGroup.VisiblePlayers)
                {
                    var what = new PlayerEventSelector() { Renderer = true, Device = true };
                    p.PropertyChanged += Player_PropertyChanged;
                    await p.SubscribeToEventsAsync(Listener.MinimumTimeout, what);
                }
            }
        }

        public async Task UnsubscribeAsync()
        {
            {
                var what = new PlayerEventSelector()
                {
                    GroupRenderer = ActualGroup.Coordinator.HasVolume,
                    ContentDirectory = true,
                };
                ActualGroup.Coordinator.PropertyChanged -= Coordinator_PropertyChanged;
                ActualGroup.Coordinator.QueueChangedEvent -= Queue.QueueChanged;
                await ActualGroup.Coordinator.UnsubscribeToEventsAsync(what);
            }

            if (WatchDeviceEvents)
            {
                foreach (var p in ActualGroup.VisiblePlayers)
                {
                    var what = new PlayerEventSelector() { Renderer = true };
                    p.PropertyChanged -= Player_PropertyChanged;
                    await p.UnsubscribeToEventsAsync(what);
                }
            }
        }

        public bool IsThisGroup(Group g) => ActualGroup.Equals(g);

        private PlayerViewModel FindPlayerVM(Player? player)
        {
            var vm = Players.FirstOrDefault(p => p.IsPlayer(player));
            return vm;
        }

        private void Player_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            var player = sender as Player;
            var vm = FindPlayerVM(player);
            if (vm == null)
            {
                return;
            }

            switch (e.PropertyName)
            {
                case nameof(Player.DeviceVolume):
                    vm.VolumeHasChanged(player.DeviceVolume);
                    break;
                case nameof(Player.IsMuted):
                    vm.MutedHasChanged(player.IsMuted);
                    break;
            }
        }

        private void Coordinator_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            var player = sender as Player;
            if (player == null)
            {
                return;
            }

            if (player != ActualGroup.Coordinator)
            {
                return;
            }

            if (false)      // TODO figure out how
            {
                // If this is the CurrentZone then update the MusicSource (to keep "TV" presence correct)
                MusicSource.SourcePlayer = player;
            }

            switch (e.PropertyName)
            {
                case "GroupVolumeEvent":
                    if (_VolumeHandler.EventReceived(player.GroupVolume))
                    {
                        _GroupVolume.Volume = player.GroupVolume;
                    }
                    break;

                case nameof(Player.GroupMuted):
                    _GroupVolume.Muted = player.GroupMuted;
                    break;

                case nameof(Player.GroupFixedVolume):
                    _GroupVolume.Fixed = player.GroupFixedVolume;
                    break;
            }
        }

        private async Task<ushort> GroupVolumeHandlerAsync(ushort oldVolume, ushort newVolume)
        {
            int adjustment = (int)newVolume - oldVolume;
            if (adjustment == 0)
            {
                return newVolume;
            }
            var result = await ActualGroup.Coordinator.SetRelativeGroupVolumeAsync(adjustment);
            if (result >= 0)
            {
                Debug.WriteLine("adjusted {0} to {1} got {2}", oldVolume, newVolume, result);
                Debug.Assert(newVolume == result);
                return (ushort)result;
            }
            else
            {
                throw new Exception("Failed to set GroupVolume");
            }
        }

        internal void UpdateGroupEditor(bool force)
        {
            // Get a fresh one if anyone has invalidated the old one
            if (_GroupEditor == null || _GroupEditorInvalid || force)
            {
                _GroupEditorInvalid = false;
                GroupEditor = new GroupEditorViewModel(_Household, this);
            }
        }

        protected GroupViewModel()
        {
        }

        virtual public string Summary
        {
            get => ActualGroup.Summary();
        }

        private bool _Muted;

        internal bool GetMuted()
        {
            return _Muted;
        }

        internal bool SetMuted(bool newMute)
        {
            if (newMute != _Muted)
            {
                _Muted = newMute;
                var later = ActualGroup.Coordinator.SetGroupMuted(newMute);
                return true;
            }

            return false;
        }

        private int _Volume;

        internal int GetVolume()
        {
            return _Volume;
        }

        internal bool SetVolume(int newVolume)
        {
            if (newVolume != _Volume)
            {
                ushort newVol = (ushort)newVolume;
                _Volume = newVol;
                _VolumeHandler.PropSet(newVol);
                var later = _VolumeHandler.SetAsync(newVol);
                return true;
            }

            return false;
        }

        internal bool ContainsPlayer(Player p)
        {
            return ActualGroup.FindPlayerById(p.UniqueName) != null;
        }

        internal void FillWithFakes()
        {
            _NowPlaying = new FakeNowPlayingViewModel();
            _GroupVolume = new FakeGroupVolumeViewModel();
            _Queue = new FakeQueueViewModel();
        }
    }

    public class FakeGroupViewModel : GroupViewModel
    {
        public FakeGroupViewModel() : this("Family Room")
        {
        }

        internal FakeGroupViewModel(string name)
        {
            _Summary = name;
            _NowPlaying = new FakeNowPlayingViewModel();
            _GroupVolume = new FakeGroupVolumeViewModel();
            _Queue = new FakeQueueViewModel();
        }

        private string _Summary;
        public override string Summary => _Summary;
    }

    public class FakeGroupVolumeViewModel : ViewModelBase, IVolume
    {
        public FakeGroupVolumeViewModel()
        {
            _Volume = 50;
        }

        private int _Volume;
        private bool _Muted;
        private bool _Fixed;
        private NowPlayingViewModel _NowPlaying;

        public int Volume { get => _Volume; set => SetProperty(ref _Volume, value); }

        public bool Muted { get => _Muted; set => SetProperty(ref _Muted, value); }
        public bool Fixed { get => _Fixed; set => SetProperty(ref _Fixed, value); }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
