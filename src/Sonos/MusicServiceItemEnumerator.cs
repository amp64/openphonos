using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenPhonos.Sonos
{
    public class MusicServiceItemEnumerator : IMusicItemEnumeratorObservable, IMusicItemSearchable
    {
        public string DisplayName { get; }
        public string ArtUri { get; }
        public MusicItem EmptyItem => null;

        public bool CanSearch { get; }

        // This is how the children of this enumerator should be displayed
        public DisplayData ParentDisplayMode { get; private set; }

        public virtual bool CanExport(ExportType exportType)
        {
            return false;
        }

        private string Id;
        private string SearchId;
        private MusicService Service;
        private int CurrentItem;
        private int ChunkSize;
        private ObservableCollection<MusicItem> Items;
        private ReadOnlyObservableCollection<MusicItem> ItemsReadOnly;
        private PauseTokenSource PauseController;
        private bool Stopped;

        public MusicServiceItemEnumerator(MusicService service, string id, string name, int chunksize, DisplayData display = null, string art = null, string search = null)
        {
            DisplayName = name;
            Id = id;
            Service = service;
            ChunkSize = chunksize;
            ArtUri = art ?? Service.LogoUri;
            ParentDisplayMode = display;
            Items = new ObservableCollection<MusicItem>();
            ItemsReadOnly = new ReadOnlyObservableCollection<MusicItem>(Items);
            PauseController = new PauseTokenSource();
            CanSearch = service.IsSearchable;
            SearchId = search;
        }

        public async Task<int> CountAsync(PlayerCallbacks callbacks)
        {
            throw new NotImplementedException();
        }

        public IMusicItemEnumerator GetChildrenEnumerator(MusicItem parent)
        {
            var item = parent as MusicServiceItem;

            if (!parent.IsContainer || item == null)
                throw new Exception("No children here");

            return new MusicServiceItemEnumerator(Service, item.Id, item.Title, 100, item.DisplayData);
        }

        public async Task<IEnumerable<MusicItem>> GetNextItemsAsync(PlayerCallbacks callbacks)
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
        }

        public Task<string> ExportAsync(PlayerCallbacks callbacks, ExportType exportType)
        {
            throw new NotImplementedException();
        }

        public ReadOnlyObservableCollection<MusicItem> GetAsObservable(Player source)
        {
            return ItemsReadOnly;
        }

        public async Task StartReadAsync(Player source)
        {
            CurrentItem = 0;
            await Service.InitializeAsync();
            var pauseToken = PauseController.Token;

            while (!Stopped)
            {
                try
                {
                    MusicServiceResult results;
                    if (SearchId == null)
                    { 
                        results = await Service.GetItemsAsync(source, Id, CurrentItem, ChunkSize, false);
                    }
                    else
                    {
                        results = await Service.SearchItemsAsync(source, Id, SearchId, CurrentItem, ChunkSize);
                    }

                    if (results.Items.Count() == 0)
                    {
                        break;
                    }
                    foreach (var item in results.Items)
                    {
                        item.ParentDisplayData = ParentDisplayMode;
                        Items.Add(item);
                        CurrentItem++;
                    }
                    if (CurrentItem >= results.MaximumItems)
                    {
                        break;
                    }
                    await pauseToken.WaitWhilePausedAsync();
                }
                catch (Exception ex)
                {
                    Items.Add(MusicItem.FromError(ex));
                    break;
                }
            }

            Stopped = true;
        }

        public void PauseRead()
        {
            PauseController.IsPaused = true;
        }

        public bool ResumeRead()
        {
            bool more = !Stopped;
            PauseController.IsPaused = false;
            return more;
        }

        public Task<bool> UpdateAsync(Player source)
        {
            return Task.FromResult(false);
        }

        public void StopRead()
        {
            Stopped = true;
            PauseController.IsPaused = false;
        }

        public IMusicItemEnumerator GetSearchEnumerator(string search)
        {
            return new MusicServiceFinder(Service, search);
        }
    }
}
