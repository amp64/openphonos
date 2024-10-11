using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace OpenPhonos.Sonos
{
    public class MultiMusicContainer : MusicItem
    {
        public IMusicItemEnumerator Enumerator { get; }

        internal MultiMusicContainer(string name, IMusicItemEnumerator enumerator)
        {
            Title = name;
            IsContainer = true;
            Enumerator = enumerator;
            ArtUri = enumerator.ArtUri;
        }
    }

    // This wraps multiple other enumerators/items in-sequence
    public class MultiMusicItemEnumerator : IMusicItemEnumerator, IMusicItemSearchable
    {
        public string DisplayName { get; }
        public MusicItem EmptyItem => null;

        public string ArtUri { get; }
        public DisplayData ParentDisplayMode => null;

        public virtual bool CanSearch => false;

        public virtual bool CanExport(ExportType exportType)
        {
            return false;
        }

        private List<object> Items;         // contains a MusicItem (not a container) or IMusicItemEnumerator
        private int CurrentItem;
        protected bool NeedsInitializing;

        public MultiMusicItemEnumerator(string name, string art)
        {
            DisplayName = name;
            ArtUri = art;
            Items = new List<object>();
            Reset();
        }

        public void AddEnumerator(IMusicItemEnumerator enumerator)
        {
            Items.Add(enumerator);
            Reset();
        }

        public Task<int> CountAsync(PlayerCallbacks callbacks)
        {
            return Task.FromResult(Items.Count);
        }

        public IMusicItemEnumerator GetChildrenEnumerator(MusicItem parent)
        {
            var enumerator = parent as MultiMusicContainer;
            if (enumerator != null)
            {
                return enumerator.Enumerator;
            }
            else
            {
                return null;
            }
        }

        public async Task<IEnumerable<MusicItem>> GetNextItemsAsync(PlayerCallbacks callbacks)
        {
            if (NeedsInitializing)
            {
                NeedsInitializing = false;
                await InitializeAsync(callbacks);
            }

            if (CurrentItem >= Items.Count)
            {
                return await Task.FromResult((IEnumerable<MusicItem>)(null));
            }

            CurrentItem += Items.Count;
            return Items.Select(x =>
            {
                if (x is IMusicItemEnumerator e)
                {

                    return new MultiMusicContainer(e.DisplayName, e);
                }
                else
                {
                    return x as MusicItem;
                }
            });
        }

        protected virtual Task InitializeAsync(PlayerCallbacks callbacks)
        {
            return Task.FromResult(false);
        }

        public void Reset()
        {
            CurrentItem = 0;
        }

        public Task<string> ExportAsync(PlayerCallbacks callbacks, ExportType exportType)
        {
            throw new NotImplementedException();
        }

        public virtual IMusicItemEnumerator GetSearchEnumerator(string search)
        {
            throw new NotImplementedException();
        }
    }
}
