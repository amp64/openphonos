using OpenPhonos.UPnP;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Linq;
using System.Threading.Tasks;

namespace OpenPhonos.Sonos
{
    // This is used to contain the data for a page of data (which might be incomplete)
    // It also abstracts away the difference between the two kinds of Enumerators
    public class MusicItemReader
    {
        public IMusicItemEnumerator MusicEnumerator { get; private set; }

        public ReadOnlyObservableCollection<MusicItem> Items { get; private set; }

        public bool JustAnError { get; private set; }

        // This is used for non-Dynamic collections, else null
        private ObservableCollection<MusicItem> _Items;
        // This is used to stop the reading of items (non-Dynamic only)
        volatile bool _Continue;

        private Player _SourcePlayer;
        private PlayerCallbacks _PlayerCallbacks;

        public MusicItemReader(IMusicItemEnumerator music, Player source, PlayerCallbacks callbacks)
        {
            MusicEnumerator = music;
            if (music is IMusicItemEnumeratorObservable m)
            {
                Items = m.GetAsObservable(source);
            }
            else
            {
                _Items = new ObservableCollection<MusicItem>();
                Items = new ReadOnlyObservableCollection<MusicItem>(_Items);
            }
            _SourcePlayer = source;
            _PlayerCallbacks = callbacks;
        }

        public async Task ReadItemsAsync(bool resuming)
        {
            if (MusicEnumerator is IMusicItemEnumeratorObservable src)
            {
                if (resuming)
                {
                    bool more = src.ResumeRead();
                    if (more)
                    {
                        return;
                    }
                }
                else
                {
                    await src.StartReadAsync(_SourcePlayer);
                }
            }
            else
            {
                _Continue = true;
                while (_Continue)
                {
                    var items = await MusicEnumerator.GetNextItemsAsync(_PlayerCallbacks);
                    if (items == null)
                        break;
                    foreach (var item in items)
                    {
                        _Items.Add(item);       // TODO inefficient
                    }
                }
            }

            JustAnError = false;

            if (Items.Count() == 0)
            {
                JustAnError = true;

                var list = new ObservableCollection<MusicItem>();
                if (!(MusicEnumerator is EmptyEnumerator))
                {
                    // Enumerators can override what their empty item is
                    var empty = MusicEnumerator?.EmptyItem ?? MusicItem.FromTitle(StringResource.Get("MusicNoItemsFound"));
                    list.Add(empty);
                }

                Items = new ReadOnlyObservableCollection<MusicItem>(list);
            }
            else if (Items.Count() == 1 && Items.First().IsError)
            {
                JustAnError = true;
            }
        }

        public void Pause()
        {
            if (MusicEnumerator is IMusicItemEnumeratorObservable m)
            {
                m.PauseRead();
            }
            else
            {
                _Continue = false;
            }
        }

        public void StopReading(bool pause)
        {
            if (MusicEnumerator is IMusicItemEnumeratorObservable src)
            {
                if (pause)
                {
                    src.PauseRead();
                }
                else
                {
                    src.StopRead();
                }
            }
            else
            {
                // stop StartAsync looping
                _Continue = false;
            }
        }
    }
}
