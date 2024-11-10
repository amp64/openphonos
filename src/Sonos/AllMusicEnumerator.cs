
using OpenPhonos.UPnP;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace OpenPhonos.Sonos
{

    public class AllMusicEnumerator : IMusicItemEnumeratorObservable
    {
        // This is the root item of a Music Service, which Enumerates the top level
        public class MusicEnumeratorItem : MusicItem
        {
            public IMusicItemEnumerator ActualEnum { get; }

            public MusicEnumeratorItem(IMusicItemEnumerator actualEnum)
            {
                ActualEnum = actualEnum;
                this.Title = ActualEnum.DisplayName;
                this.IsContainer = true;
                this.ArtUri = ActualEnum.ArtUri;
                this.DisplayData = ActualEnum.ParentDisplayMode;
            }
        }

        public AllMusicEnumerator(MusicServiceProvider provider)
        {
            // This emulates the (old Tune-In era) Desktop list
            _All = new ObservableCollection<MusicItem>();
            _AllReadOnly = new ReadOnlyObservableCollection<MusicItem>(_All);
            _All.Add(new MusicEnumeratorItem(new FavoritesEnumerator(provider)));

            // TV will be added when UpdateTV is called (via GetAsObservable)
            _TVLocation = _All.Count;

            _All.Add(new MusicEnumeratorItem(new MusicLibraryEnumerator()));
            _All.Add(new MusicEnumeratorItem(new RadioEnumerator(provider)));

            _NextMusicServiceIndex = _All.Count;
            _MusicServiceProvider = provider;

            _All.Add(new MusicEnumeratorItem(new SonosPlaylistEnumerator()));
            _All.Add(new MusicEnumeratorItem(new LineInEnumerator()));

            if (provider.GetMusicServices?.Count > 0)
            {
                AddMusicServices();
            }
            else
            {
                provider.OnServicesLoaded += () =>
                {
                    AddMusicServices();
                };
            }
        }

        private void AddMusicServices()
        {
            var display = new DisplayData(id: "rootgrid", "GRID");

            foreach (var service in _MusicServiceProvider.GetMusicServices)
            {
                // NOTE: We do NOT Init the actual music services here as too slow
                _All.Insert(_NextMusicServiceIndex, new MusicEnumeratorItem(new MusicServiceItemEnumerator(service, "root", service.DisplayName, 200, display)));
                _NextMusicServiceIndex++;
            }
        }

        private ObservableCollection<MusicItem> _All;
        private ReadOnlyObservableCollection<MusicItem> _AllReadOnly;
        private int _NextMusicServiceIndex;
        private MusicServiceProvider _MusicServiceProvider;
        private int _TVIndex = -1;          // -1 = none, otherwise its the current location
        private int _TVLocation;            // Where it should go

        public string DisplayName => StringResource.Get("MusicMusicSource");

        public MusicItem EmptyItem => null;

        public string ArtUri => null;

        public DisplayData ParentDisplayMode => null;

        public ReadOnlyObservableCollection<MusicItem> GetAsObservable(Player source)
        {
            UpdateTV(source);
            return _AllReadOnly;
        }

        public Task StartReadAsync(Player source)
        {
            return Task.FromResult(0);
        }

        public Task<bool> UpdateAsync(Player source)
        {
            return Task.FromResult(UpdateTV(source));
        }

        private bool UpdateTV(Player source)
        {
            bool changed = false;

            if (_TVLocation != 0)
            {
                if (source.HTCapable && _TVIndex == -1)
                {
                    // Need to Add the TV item
                    _All.Insert(_TVLocation, MusicItem.FromTitle(StringResource.Get("MusicTV")));
                    _TVIndex = _TVLocation;
                }
                else if (!source.HTCapable && _TVIndex != -1)
                {
                    // Need to remove the TV item
                    _All.RemoveAt(_TVIndex); 
                    _TVIndex = -1;
                }
            }
            return changed;
        }

        public async Task<int> CountAsync(PlayerCallbacks callbacks)
        {
            return await Task.FromResult(-1);
        }

        public void Reset()
        {
        }

        public Task<IEnumerable<MusicItem>> GetNextItemsAsync(PlayerCallbacks callbacks)
        {
            throw new NotImplementedException();
        }

        public IMusicItemEnumerator GetChildrenEnumerator(MusicItem parent)
        {
            var item = parent as MusicEnumeratorItem;
            return item.ActualEnum;
        }

        public bool CanExport(ExportType exportType)
        {
            return false;
        }

        public Task<string> ExportAsync(PlayerCallbacks player, ExportType exportType)
        {
            throw new NotImplementedException();
        }

        public void PauseRead()
        {
        }

        public bool ResumeRead()
        {
            return false;
        }

        public void StopRead()
        {
        }
    }
}
