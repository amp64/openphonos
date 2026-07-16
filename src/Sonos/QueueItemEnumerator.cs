using OpenPhonos.UPnP;
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
            return new QueueMusicItem("The queue is empty.") { IsError = true }; 
        }

        public void AdjustId(int delta)
        {
            // eg change Q:0/7 to Q:0/6 (if -1 passed in)
            if (int.TryParse(this.Id.Substring(4), out int idnum))
            {
                idnum += delta;
                this.Id = "Q:0/" + idnum.ToString();
            }
        }
    }

    public class QueueItemEnumerator : IMusicItemEnumeratorObservable, INotifyPropertyChanged
    {
        public string DisplayName => "Queue";

        public MusicItem EmptyItem => QueueMusicItem.GetEmptyItem();

        public string ArtUri => null;

        public DisplayData ParentDisplayMode => null;

        internal class UpdateInfo
        {
            public int UpdateIndex;
            public uint UpdateId;
        }

        public object DoUpdateCurrentTrack;

        // This is the UpdateInfo we expect next time, set after a Delete operation, null most of the time
        private UpdateInfo _ExpectedUpdateInfo;

        public bool CanExport(ExportType exportType)
        {
            throw new NotImplementedException();
        }

        public Task<int> CountAsync(PlayerCallbacks callbacks)
        {
            return Task.FromResult(-1);
        }

        public Task<string> ExportAsync(PlayerCallbacks player, ExportType exportType)
        {
            throw new NotImplementedException();
        }

        public string QueueName { get; private set; }

        public uint UpdateId { get => _UpdateId; }

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
        private uint _CurrentChunkIndex;
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
            _CurrentChunkIndex = 0;
            _QueueSize = 0;
            _ExpectedUpdateInfo = null;

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
                    var items = await source.ContentDirectory.Browse("Q:0", "BrowseDirectChildren", "*", _CurrentChunkIndex, _ChunkSize, string.Empty);
                    items.ThrowIfFailed();
                    var chunk = QueueMusicItem.Parse(XElement.Parse(items.Result), source.AVTransport.BaseUri);
                    foreach(var item in chunk)
                    {
                        _Items.Add(item);
                    }
                    if (items.TotalMatches != 0)
                    {
                        QueueSize = items.TotalMatches;
                    }
                    _CurrentChunkIndex += items.NumberReturned;
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

            if (_Items.Count == 0)
            {
                _Items.Add(QueueMusicItem.GetEmptyItem());
                // do this AFTER adding the empty item as the QueueViewModel depends on it
                QueueSize = 1;
            }

            // Tell the attached ViewModel to update its CurrentIndex, but we have a property value for that
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DoUpdateCurrentTrack)));
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

        public void SetUpdateExpectation(int deletedIndex)
        {
            _ExpectedUpdateInfo = new UpdateInfo()
            {
                UpdateIndex = deletedIndex,
                UpdateId = _UpdateId + 1
            };
        }

        public void OnQueueChanged(Player.ContentChangedEventArgs args)
        {
            if (args.UpdateId == _UpdateId)
            {
                return;
            }

            var later = RefillAsync();
        }

        private async Task RefillAsync()
        {
            // Lets special-case the single-item deleted case to avoid a full reload (except when deleting the last one)
            if (_ExpectedUpdateInfo != null && QueueSize == _Items.Count && QueueSize != 1)
            {
                // We have loaded the entire queue previously, so get the size of the new queue and see if it's smaller by 1
                var newq = await _Source.ContentDirectory.Browse("Q:0", "BrowseDirectChildren", "*", 0, 1, "").Required();
                if (newq.TotalMatches + 1 == QueueSize && newq.UpdateID == _ExpectedUpdateInfo.UpdateId)
                {
                    // One item deleted as expected so just remove it
                    _Items.RemoveAt(_ExpectedUpdateInfo.UpdateIndex);

                    // and need to adjust the IDs of every subsequent item
                    for(int i = _ExpectedUpdateInfo.UpdateIndex; i < _Items.Count; i++)
                    {
                        if (_Items[i] is QueueMusicItem qmi)
                        {
                            qmi.AdjustId(-1);
                        }
                    }

                    QueueSize = newq.TotalMatches;
                    _UpdateId = newq.UpdateID;
                    _ExpectedUpdateInfo = null;

                    // Tell the attached ViewModel to update its CurrentIndex, but we have a property value for that
                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(DoUpdateCurrentTrack)));
                    return;
                }
            }

            _ExpectedUpdateInfo = null;
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
