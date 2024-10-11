using Avalonia.Controls;
using Avalonia.Interactivity;
using OpenPhonos.Sonos;
using OpenPhonos.UPnP;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PhonosAvalon.ViewModels
{
    public class MusicItemWithStrings : MusicItem
    {
        public MusicItem Item { get; private set; }

        public string ItemThumbnails { get => Item.DisplayData != null ? Item.DisplayData.ItemThumbnails : null; }
        public bool HeaderVisibility { get => Item.DisplayData != null && Item.DisplayData.HeaderVisible; }

        public bool CanPlayShuffle { get; private set; }

        public string Header0 { get => DisplayData.HeaderItem(Item, 0); }
        public string Header1 { get => DisplayData.HeaderItem(Item, 1); }
        public string Header2 { get => DisplayData.HeaderItem(Item, 2); }

        public MusicItemWithStrings(MusicItem item)
        {
            Item = item;
            DisplayData = item.DisplayData;
            ParentDisplayData = item.ParentDisplayData;
            CanPlayShuffle = item.IsPlayable && item.IsContainer;
        }
    }

    public class MusicPickerViewModel : ViewModelBase
    {
        public IReadOnlyCollection<MusicItem> Items 
        {
            get;
            private set;
        }

        private bool _SearchControlVisible;
        public bool SearchControlVisible { get => _SearchControlVisible; set => SetProperty(ref _SearchControlVisible, value); }

        private bool _CanSearch;
        public bool CanSearch { get => _CanSearch; set => SetProperty(ref _CanSearch, value); }

        public Player SourcePlayer
        {
            get => _SourcePlayer;
            set
            {
                var old = _SourcePlayer;
                if (SetProperty(ref _SourcePlayer, value))
                {
                    OnSourcePlayerChanged(old);
                }
            }
        }

        public string Heading
        {
            get => _Heading;
            protected set => SetProperty(ref _Heading, value);
        }

        public bool Busy
        {
            get => _Busy;
            private set => SetProperty(ref _Busy, value);
        }

        public string ViewType
        {
            get => _ViewType;
            private set => SetProperty(ref _ViewType, value);
        }

        public MusicItemWithStrings? ParentItem
        {
            get
            {
                return _ParentItem;
            }
            set
            {
                SetProperty(ref _ParentItem, value);
            }
        }

        public GoBackCommand BackCommand { get; private set; }
        public GoHomeCommand HomeCommand { get; private set; }
        public GoPlayCommand PlayCommand { get; private set; }
        public GoPlayCommand ShuffleCommand { get; private set; }
        public ToggleSearchCommand SearchToggleCommand { get; private set; }
        public GoSearchCommand SearchCommand { get; private set; }

        private MusicItemReader _MusicReader;
        private string _Heading;
        private bool _Busy;
        private PlayerCallbacks _PlayerCallbacks;
        private Stack<MusicViewState> _ItemsStack;
        private MusicItemWithStrings? _ParentItem;
        private string _ViewType;
        private Player _SourcePlayer;
        private Task _ReadingItemTask;
        private static ReadOnlyObservableCollection<MusicItem> _EmptyItems = new ReadOnlyObservableCollection<MusicItem>(new ObservableCollection<MusicItem>());

        public MusicPickerViewModel(Player p, MusicServiceProvider m)
        {
            _Heading = string.Empty;
            _ItemsStack = new Stack<MusicViewState>();
            _ViewType = "Default";
            _SourcePlayer = p;
            _PlayerCallbacks = new PlayerCallbacks(() => _SourcePlayer, null);
            BackCommand = new GoBackCommand(this);
            HomeCommand = new GoHomeCommand(this);
            PlayCommand = new GoPlayCommand(this, PlayType.ReplaceQueue);
            ShuffleCommand = new GoPlayCommand(this, PlayType.ReplaceQueueAndShuffle);
            SearchToggleCommand = new ToggleSearchCommand(this);
            SearchCommand = new GoSearchCommand(this);
            _ReadingItemTask = StartAsync(RootEnumerator(m), null, false);
        }

        /// <summary>
        /// This is overridable so different roots can use used
        /// </summary>
        /// <returns>The desired root enumerator</returns>
        protected virtual IMusicItemEnumerator RootEnumerator(MusicServiceProvider m)
        {
            return new AllMusicEnumerator(m);
        }

        private async Task StartAsync(IMusicItemEnumerator music, MusicViewState? state, bool resuming)
        {
            // Clear the UX
            SetItems(_EmptyItems);

            // Stop reading the current data
            if (_MusicReader != null)
            {
                _MusicReader.Pause();

                // And wait for it to actually stop
                await _ReadingItemTask;
            }

            if (resuming)
            {
                _MusicReader = state.Source;
            }
            else
            {
                // Setup the new data source
                _MusicReader = new MusicItemReader(music, _SourcePlayer, _PlayerCallbacks);
            }

            SetItems(_MusicReader.Items);
            CanSearch = (music is IMusicItemSearchable finder && finder.CanSearch);

            if (ParentItem == null || ParentItem.DisplayData == null)
            {
                ViewType = "Default";
            }
            else
            {
                switch (ParentItem.DisplayData.DisplayMode)
                {
                    case "GRID": ViewType = "Grid"; break;
                    default: ViewType = "Default"; break;
                }
            }

            Heading = music.DisplayName;

            // Start reading the new data
            Busy = true;

            if (!resuming)
            {
                // TODO why??
                music.Reset();
            }

            // Start/continue reading
            _ReadingItemTask = _MusicReader.ReadItemsAsync(resuming)
                .ContinueWith((t) =>
                {
                    Busy = false;
                    if (_MusicReader.JustAnError)
                    {
                        SetItems(_MusicReader.Items);
                        ViewType = "Default";               // so that errors don't show up in Grid form
                    }
                });

            if (state != null && state.Control != null)
            {
                state.Control.Scroll.Offset = new Avalonia.Vector(0, state.YPos);
            }
        }

        private void SetItems(ReadOnlyObservableCollection<MusicItem> items)
        {
            Items = items;
            OnPropertyChanged(nameof(Items));
        }

        private void OnSourcePlayerChanged(Player previous)
        {
            if (_MusicReader.MusicEnumerator is IMusicItemEnumeratorObservable m)
            {
                var later = m.UpdateAsync(_SourcePlayer);
            }
        }

        public void MusicItemClicked(MusicItem item, ListBox listbox)
        {
            if (item.IsContainer)
            {
                _MusicReader.StopReading(true);
                Busy = false;

                var next = _MusicReader.MusicEnumerator.GetChildrenEnumerator(item);

                SetItems(_EmptyItems);

                if (next != null)
                {
                    _ItemsStack.Push(new MusicViewState(_MusicReader, listbox, listbox.Scroll.Offset.Y, ParentItem));
                    BackCommand.ExecuteHasChanged();
                    HomeCommand.ExecuteHasChanged();
                    ParentItem = new MusicItemWithStrings(item);
                    _ReadingItemTask = StartAsync(next, null, false);
                }
            }
        }

        private void DoSearch(string what)
        {
            _MusicReader.StopReading(true);
            Busy = false;
            SearchControlVisible = false;

            var finder = _MusicReader.MusicEnumerator as IMusicItemSearchable;
            var next = finder.GetSearchEnumerator(what);

            _ItemsStack.Push(new MusicViewState(_MusicReader, null, 0, ParentItem));
            BackCommand.ExecuteHasChanged();
            HomeCommand.ExecuteHasChanged();
            ParentItem = null;
            _ReadingItemTask = StartAsync(next, null, false);
        }

        internal bool CanBack =>_ItemsStack.Count != 0;

        private void GoBack()
        {
            var previous = _ItemsStack.Pop();
            BackCommand.ExecuteHasChanged();
            HomeCommand.ExecuteHasChanged();
            SearchControlVisible = false;

            _MusicReader.StopReading(false);

            var next = previous.Source;
            ParentItem = previous.SourceItem;
            _ReadingItemTask = StartAsync(next.MusicEnumerator, previous, true);        // TODO pass it the Reader directly?
        }

        private void GoHome()
        {
            MusicViewState previous;
            do
            {
                previous = _ItemsStack.Pop();
            } while (_ItemsStack.Count != 0);

            BackCommand.ExecuteHasChanged();
            HomeCommand.ExecuteHasChanged();
            SearchControlVisible = false;

            _MusicReader.StopReading(false);

            var next = previous.Source;
            ParentItem = previous.SourceItem;
            _ReadingItemTask = StartAsync(next.MusicEnumerator, previous, true);        // TODO pass it the Reader directly?
        }

#if false
        private void FillTestData()
        {
            Heading = "Music";
            _Items.Add(MusicItem.FromTitle("First"));
            for (int i = 1; i < 20; i++)
            {
                _Items.Add(MusicItem.FromTitle($"Music Item {i}"));
            }
            _Items.Add(MusicItem.FromTitle("Last"));
        }
#endif
        private class MusicViewState
        {
            public MusicItemReader Source;
            public ListBox? Control;
            public double YPos;
            public MusicItemWithStrings SourceItem;
        
            public MusicViewState(MusicItemReader source, ListBox? control, double ypos, MusicItemWithStrings display)
            {
                Source = source;
                Control = control;
                YPos = ypos;
                SourceItem = display;
            }
        }

        public class GoBackCommand : ICommand
        {
            public event EventHandler? CanExecuteChanged;

            private readonly MusicPickerViewModel _Parent;

            public GoBackCommand(MusicPickerViewModel parent)
            {
                _Parent = parent;
            }

            internal void ExecuteHasChanged()
            {
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            }

            public bool CanExecute(object? parameter)
            {
                return _Parent.CanBack;
            }

            public void Execute(object? parameter)
            {
                _Parent.GoBack();
            }
        }

        public class GoSearchCommand : ICommand
        {
            public event EventHandler? CanExecuteChanged;

            private readonly MusicPickerViewModel _Parent;

            public GoSearchCommand(MusicPickerViewModel parent)
            {
                _Parent = parent;
            }

            public bool CanExecute(object? parameter)
            {
                return true;
            }

            public void Execute(object? parameter)
            {
                _Parent.DoSearch(parameter as string);
            }
        }

        public class ToggleSearchCommand : ICommand
        {
            public event EventHandler? CanExecuteChanged;

            private readonly MusicPickerViewModel _Parent;

            public bool CanExecute(object? parameter)
            {
                return true;
            }

            public void Execute(object? parameter)
            {
                _Parent.SearchControlVisible = !_Parent.SearchControlVisible;
            }

            public ToggleSearchCommand(MusicPickerViewModel parent)
            {
                _Parent = parent;
            }
        }

        public class GoPlayCommand : ICommand
        {
            public event EventHandler? CanExecuteChanged;

            private readonly MusicPickerViewModel _Parent;
            private readonly PlayType _Action;

            public GoPlayCommand(MusicPickerViewModel parent, PlayType action)
            {
                _Parent = parent;
                _Action = action;
            }

            public bool CanExecute(object? parameter)
            {
                return true;
            }

            public void Execute(object? parameter)
            {
                _Parent.DoPlayAction(parameter as MusicItem, _Action);
            }
        }

        private void DoPlayAction(MusicItem? musicItem, PlayType action)
        {
            var later = SourcePlayer.PlayItemAsync(musicItem, action);
            // TODO report errors?
        }

        public class GoHomeCommand : ICommand
        {
            public event EventHandler? CanExecuteChanged;

            private readonly MusicPickerViewModel _Parent;

            public GoHomeCommand(MusicPickerViewModel parent)
            {
                _Parent = parent;
            }

            internal void ExecuteHasChanged()
            {
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            }

            public bool CanExecute(object? parameter)
            {
                return _Parent.CanBack;
            }

            public void Execute(object? parameter)
            {
                _Parent.GoHome();
            }
        }
    }
}
