using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenPhonos.Sonos
{
    public class EmptyEnumerator : IMusicItemEnumerator
    {
        public string DisplayName { get; private set; }

        public MusicItem EmptyItem => null;

        public string ArtUri => null;

        public DisplayData ParentDisplayMode => null;

        public EmptyEnumerator(string name)
        {
            DisplayName = name;
        }

        public bool CanExport(ExportType exportType)
        {
            return false;
        }

        public Task<int> CountAsync(PlayerCallbacks callbacks)
        {
            return Task.FromResult(0);
        }

        public Task<string> ExportAsync(PlayerCallbacks player, ExportType exportType)
        {
            throw new NotImplementedException();
        }

        public IMusicItemEnumerator GetChildrenEnumerator(MusicItem parent)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<MusicItem>> GetNextItemsAsync(PlayerCallbacks callbacks)
        {
            return Task.FromResult((IEnumerable<MusicItem>)null);
        }

        public void Reset()
        {
        }
    }
}
