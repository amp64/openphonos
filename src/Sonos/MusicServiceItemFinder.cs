using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenPhonos.Sonos
{
    class MusicServiceFinder : IMusicItemEnumerator, IMusicItemSearchable
    {
        public bool CanSearch => true;

        public string DisplayName => Service.DisplayName;

        public string ArtUri => null;

        public DisplayData ParentDisplayMode => null;

        public MusicItem EmptyItem => null;

        public MusicServiceFinder(MusicService svc, string search)
        {
            Service = svc;
            SearchItem = search;
        }

        private MusicService Service;
        private string SearchItem;
        private bool Finished;

        public IMusicItemEnumerator GetSearchEnumerator(string search)
        {
            return new MusicServiceFinder(Service, search);
        }

        public Task<int> CountAsync(PlayerCallbacks callbacks)
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            Finished = false;
        }

        public async Task<IEnumerable<MusicItem>> GetNextItemsAsync(PlayerCallbacks callbacks)
        {
            if (Finished)
            {
                return null;
            }
                
            var result = new List<MusicServiceItem>();

            // Get the top level of searching a service, ie something from each Category
            // Top-level Service search
            var list = await Service.GetSearchCategoriesAsync(callbacks.Player);

            var all = list.Items.FirstOrDefault(i => i.Id.ToLower() == "all");
            if (all != null)
            {
                // Just do the All search if the service supports it
                var chunk = await Service.SearchItemsAsync(callbacks.Player, all.Id, SearchItem, 0, MusicService.GulpSize);
                result = chunk.Items.ToList();
            }
            else
            {

                // TODO get all categories simultaneously?
                foreach (var category in list.Items)
                {
                    // TODO more reliable check for > SearchMax as SoundCloud returns bad MaximumItems
                    var chunk = await Service.SearchItemsAsync(callbacks.Player, category.Id, SearchItem, 0, UPnPMusicItemFinder.SearchMax);
                    if (chunk.MaximumItems > UPnPMusicItemFinder.SearchMax)
                    {
                        // just provide an enumerator that does a Search
                        result.Add(new MusicServiceItem(category.Id, string.Format("{0} ({1})", category.Title, chunk.MaximumItems), category.Subtitle, SearchItem, Service));
                    }
                    else
                    {
                        // add the (small) result, excluding dupes (eg Sonos Radio, Apple)

                        // if the item has no Subtitle, need to use the Category name
                        foreach (var item in chunk.Items)
                        {
                            if (string.IsNullOrEmpty(item.Subtitle))
                            {
                                item.UpdateSubtitle(category.Title);
                            }
                        }
                        var filtered = chunk.Items.Where(i => result.FirstOrDefault(j => j.Id == i.Id) == null);
                        result.AddRange(filtered);
                    }
                }
            }

            Finished = true;
            return result;
        }

        public IMusicItemEnumerator GetChildrenEnumerator(MusicItem parent)
        {
            if (parent is MusicServiceItem music)
            {
                return new MusicServiceItemEnumerator(Service, music.Id, parent.Title, MusicService.GulpSize, parent.DisplayData, null, music.SearchItem);
            }

            throw new InvalidOperationException();
        }

        public bool CanExport(ExportType exportType)
        {
            return false;
        }

        public Task<string> ExportAsync(PlayerCallbacks player, ExportType exportType)
        {
            throw new NotImplementedException();
        }
    }
}
