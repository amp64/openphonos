using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace OpenPhonos.Sonos
{
    public class QueueMusicItem : UPnPMusicItem, INotifyPropertyChanged
    {
        internal static new IEnumerable<QueueMusicItem> Parse(XElement dom, Uri baseUrl)
        {
            var elems = from item in dom.Descendants()
                        where item.Name == XName.Get("container", NS) || item.Name == XName.Get("item", NS)
                        select new QueueMusicItem(item, baseUrl);
            return elems;
        }

        private QueueMusicItem(XElement element, Uri baseUrl) : base(element, baseUrl)
        {
        }

        private QueueMusicItem(string title) : base(title, false, string.Empty)
        {
        }

        private bool _PauseVisible;
        public bool PauseVisible
        {
            get => _PauseVisible;
            set => SetProperty(ref _PauseVisible, value);
        }

        private bool _PlayVisible;
        public bool PlayVisible
        {
            get => _PlayVisible;
            set => SetProperty(ref _PlayVisible, value);
        }

        private void SetProperty(ref bool member, bool value, [CallerMemberName] string propname = null)
        {
            if (member != value)
            {
                member = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        static public MusicItem GetEmptyItem()
        {
            return new QueueMusicItem("The queue is empty.");
        }
    }

    public class QueueItemEnumerator : IMusicItemEnumeratorObservable, INotifyPropertyChanged
    {
        public string DisplayName => "Queue";

        public MusicItem EmptyItem => QueueMusicItem.GetEmptyItem();

        public string ArtUri => null;

        public DisplayData ParentDisplayMode => null;

        public bool CanExport(ExportType exportType)
        {
            throw new NotImplementedException();
        }

        public Task<int> CountAsync(PlayerCallbacks callbacks)
        {
            throw new NotImplementedException();
        }

        public Task<string> ExportAsync(PlayerCallbacks player, ExportType exportType)
        {
            throw new NotImplementedException();
        }

        public string QueueName { get; private set; }

        private uint _QueueSize;
        /// <summary>
        /// This is how many items are in the Queue, which can be larger than the number of items
        /// </summary>
        public uint QueueSize
        {
            get => _QueueSize;
            set
            {
                if (value != _QueueSize)
                {
                    _QueueSize = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(QueueSize)));
                }
            }
        }

        // Always contains QueueMusicItems
        private ObservableCollection<MusicItem> _Items;

        const uint _ChunkSize = 100;
        private uint _CurrentIndex;
        private Player _Source;
        private uint _UpdateId;

        public QueueItemEnumerator()
        {
            _Items = new ObservableCollection<MusicItem>();
        }

        public ReadOnlyObservableCollection<MusicItem> GetAsObservable(Player source)
        {
            _Source = source;
            return new ReadOnlyObservableCollection<MusicItem>(_Items);
        }

        public IMusicItemEnumerator GetChildrenEnumerator(MusicItem parent)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<MusicItem>> GetNextItemsAsync(PlayerCallbacks callbacks)
        {
            throw new NotImplementedException();
        }

        public void PauseRead()
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
        }

        public bool ResumeRead()
        {
            throw new NotImplementedException();
        }

        public async Task StartReadAsync(Player source)
        {
            _CurrentIndex = 0;
            _QueueSize = 0;

            if (source?.ContentDirectory != null)
            {
                var name = await source.ContentDirectory.Browse("Q:0", "BrowseMetadata", "*", 0, 1, "");
                if (name.Error == null)
                {
                    this.QueueName = DidlData.Parse(name.Result).First().Res;
                }
            }

            for(; ; )
            {
                if (source?.ContentDirectory != null)
                {
                    var items = await source.ContentDirectory.Browse("Q:0", "BrowseDirectChildren", "*", _CurrentIndex, _ChunkSize, string.Empty);
                    items.ThrowIfFailed();
                    QueueSize = items.TotalMatches;
                    var chunk = QueueMusicItem.Parse(XElement.Parse(items.Result), source.AVTransport.BaseUri);
                    foreach(var item in chunk)
                    {
                        _Items.Add(item);
                    }
                    _CurrentIndex += items.NumberReturned;
                    _UpdateId = items.UpdateID;
                    if (items.NumberReturned == 0)
                        break;
                }
                else
                {
                    QueueSize = 0;
                    break;
                }
            }
        }

        public void StopRead()
        {
            throw new NotImplementedException();
        }

        public async Task<bool> UpdateAsync(Player source)
        {
            bool changed = false;

            if (source != _Source)
            {
                _Items.Clear();
                _Source = source;
                var later = StartReadAsync(source);
                changed = true;
            }
            return await Task.FromResult(changed);
        }

        public void OnQueueChanged(Player.QueueChangedEventArgs args)
        {
            if (args.UpdateId == _UpdateId)
            {
                return;
            }

            var later = RefillAsync();
        }

        private async Task RefillAsync()
        {
            _Items.Clear();
            await StartReadAsync(_Source);
        }

        public QueueMusicItem TryGetItem(int index)
        {
            if (index < 0 || index >= _Items.Count)
            {
                return null;
            }
            return _Items[index] as QueueMusicItem;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
