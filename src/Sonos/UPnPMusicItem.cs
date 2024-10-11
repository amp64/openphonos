using OpenPhonos.UPnP;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace OpenPhonos.Sonos
{
    public class UPnPMusicItem : MusicItem
    {
        public string Id { get; private set; }
        public string Class { get; private set; }
        public string Creator { get; private set; }
        public string Album { get; private set; }
        public string StreamContent { get; private set; }
        public string RadioShowMd { get; private set; }

        const string DC = "http://purl.org/dc/elements/1.1/";
        public const string NS = "urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/";
        public const string UPNP = "urn:schemas-upnp-org:metadata-1-0/upnp/";
        const string R = "urn:schemas-rinconnetworks-com:metadata-1-0/";

        protected Uri BaseUrl;

        internal static IEnumerable<UPnPMusicItem> Parse(string xml, Uri baseUrl)
        {
            return Parse(XElement.Parse(xml), baseUrl);
        }

        internal static IEnumerable<UPnPMusicItem> Parse(XElement dom, Uri baseUrl)
        {
            var elems = from item in dom.Descendants()
                        where item.Name == XName.Get("container", NS) || item.Name == XName.Get("item", NS)
                        select new UPnPMusicItem(item, baseUrl);
            return elems;
        }

        public UPnPMusicItem(string title, bool container, string id)
        {
            Title = title;
            IsContainer = container;
            Id = id;
        }

        internal UPnPMusicItem(string title, bool container, string id, string md, string res, bool playable, bool favable)
        {
            Title = title;
            IsContainer = container;
            Id = id;
            Metadata = md;
            Res = res;
            IsPlayable = playable;
            IsFavoritable = favable;
        }

        internal UPnPMusicItem(XElement element, Uri baseUrl) : base()
        {
            Id = (string)element.Attribute(XName.Get("id"));
            Title = (string)element.Element(XName.Get("title", DC));
            Res = (string)element.Element(XName.Get("res", NS));
            ArtUri = (string)element.Element(XName.Get("albumArtURI", UPNP));           // will likely need conversion before can be used
            Class = (string)element.Element(XName.Get("class", UPNP));
            Creator = (string)element.Element(XName.Get("creator", DC));
            Album = (string)element.Element(XName.Get("album", UPNP));
            StreamContent = ZPConvert((string)element.Element(XName.Get("streamContent", R)));
            RadioShowMd = RadioShowName((string)element.Element(XName.Get("radioShowMd", R)));
            BaseUrl = baseUrl;

            IsContainer = element.Name == XName.Get("container", NS);
            IsFavoritable = true;
            IsAlarmable = false;

            if (Class.StartsWith("object.item.audioItem.audioBroadcast"))
            {
                Metadata = element.ToString();
                IsAlarmable = true;
            }
            else if (Class == "object.itemobject.item.sonos-favorite")
            {
                // Favorites are very different
                Album = (string)element.Element(XName.Get("description", R));
                Metadata = element.Element(XName.Get("resMD", R)).Value;

                // Recurse to get "real" MD
                var innerdidl = Parse(Metadata, null).First();
                Class = innerdidl.Class;
                IsContainer = Class.StartsWith("object.container");
                IsFavoritable = false;
                IsDeletable = true;

                Subtitle = Album;
            }
            else if (Class == "object.container.playlistContainer")
            {
                IsDeletable = true;
                IsAlarmable = true;
            }
            else if (Class == "object.item.audioItem.musicTrack")
            {
                Metadata = element.ToString();
            }

            if (Id == "spdif-input")
            {
                Title = StringResource.Get("NPTV");
            }

            IsPlayable = CalcIsPlayable();
            IsQueueable = CalcIsQueueable();
            IsFavoritable &= IsQueueable;
        }

        protected override Task<string> GetArtUriAsync()
        {
            string uri = this.ArtUri;
            if (!string.IsNullOrEmpty(uri) && BaseUrl != null)
            {
                uri = ConvertArtUri(uri);
            }
            return Task.FromResult(uri);
        }

        private string ConvertArtUri(string original)
        {
            if (original == null)
            {
                return null;
            }
            if (original.StartsWith("/getaa?"))
            {
                return new Uri(BaseUrl, original).AbsoluteUri;
            }

            return original;
        }

        // is the item playable? True if an audioitem or a playlist or a genre or an album, UNLESS it is the root Tracks item
        private bool CalcIsPlayable()
        {
            return Class.StartsWith("object.item.audioItem") ||                 // any audioItem
                Class == "object.container.genre.musicGenre" ||                   // a Genre
                Class == "object.container.album.musicAlbum" ||                   // any Album
                Class == "object.container" ||                                    // any container (eg from Folder view)
                Class == "object.container.person.musicArtist" ||                 // Artist on Rhapsody
                Class == "object.container.playlistContainer.sameArtist" ||       // any fake-up "All" album
                (
                    (Class == "object.container.playlistContainer") &&
                    !Id.EndsWith(":TRACKS")
                    );
        }

        // is the item queueable? True if we can play it, unless its a radio stream or LineIn
        private bool CalcIsQueueable()
        {
            return IsPlayable &&
                (!Class.StartsWith("object.item.audioItem.audioBroadcast")) &&
                (Class != "object.item.audioItem");
        }

        private static string ZPConvert(string p)
        {
            switch (p)
            {
                case "ZPSTR_CONNECTING":
                    return StringResource.Get("StreamConnecting");
                case "ZPSTR_BUFFERING":
                    return StringResource.Get("StreamStarting");
                default:
                    return p;
            }
        }

        private static string RadioShowName(string name)
        {
            if ((name != null) && name.Contains(",p"))
            {
                // For some reason radio show names sometimes end with ",p<digits>" so strip that (crudely)
                int trunc = name.LastIndexOf(",p");
                return name.Substring(0, trunc);
            }
            return name;
        }

    }

    public class UPnPMusicItemEnumerator : IMusicItemEnumerator, IMusicItemSearchable
    {
        private const uint ChunkSize = 10;
        private uint CurrentIndex;
        private uint StartIndexOfCache;
        private uint TotalSize;
        protected readonly string BaseObjectID;
        private IEnumerable<UPnPMusicItem> CachedItems;

        public UPnPMusicItemEnumerator(string displayName, string path)
        {
            DisplayName = displayName;
            BaseObjectID = path;
            Reset();
        }

        public string DisplayName { get; }

        public MusicItem EmptyItem => null;

        public string ArtUri { get; }

        public DisplayData ParentDisplayMode => null;

        public bool CanSearch => true;

        public virtual bool CanExport(ExportType exportType)
        {
            return false;
        }

        public void Reset()
        {
            CurrentIndex = 0;
            StartIndexOfCache = 0;
            TotalSize = 0;
        }

        public async Task<IEnumerable<MusicItem>> GetNextItemsAsync(PlayerCallbacks callbacks)
        {
            if (
                (CurrentIndex == 0) ||
                (CurrentIndex >= (StartIndexOfCache + ChunkSize))
               )
            {
                // Need to read the next chunk
                StartIndexOfCache = CurrentIndex;
                if (callbacks.Player.ContentDirectory != null)
                {
                    var items = await callbacks.Player.ContentDirectory.Browse(BaseObjectID, "BrowseDirectChildren", "*", CurrentIndex, ChunkSize, string.Empty);
                    items.ThrowIfFailed();
                    TotalSize = items.TotalMatches;
                    CachedItems = UPnPMusicItem.Parse(items.Result, callbacks.Player.AVTransport.BaseUri);
                }
                else
                {
                    TotalSize = 0;
                }
            }

            if (CurrentIndex >= TotalSize)
            {
                return null;
            }

            CurrentIndex += (uint)CachedItems.Count();
            return CachedItems;
        }

        public async Task<int> CountAsync(PlayerCallbacks callbacks)
        {
            if (TotalSize == 0)
            {
                var items = await callbacks.Player.ContentDirectory.Browse(BaseObjectID, "BrowseDirectChildren", string.Empty, 0, 1, string.Empty);
                items.ThrowIfFailed();
                TotalSize = items.TotalMatches;
            }

            return (int)TotalSize;
        }

        public IMusicItemEnumerator GetChildrenEnumerator(MusicItem parent)
        {
            if (!parent.IsContainer)
            {
                throw new ArgumentException("not a container", "parent");
            }
            
            var music = parent as UPnPMusicItem;
            if (music == null)
            {
                throw new ArgumentException("not UPnP music", "parent");
            }

            return GetEnumerator(music);
        }

        protected virtual IMusicItemEnumerator GetEnumerator(UPnPMusicItem music)
        {
            return new UPnPMusicItemEnumerator(music.Title, music.Id);
        }

        public virtual async Task<string> ExportAsync(PlayerCallbacks callbacks, ExportType exportType)
        {
            const uint blocksize = 200;
            uint start = 0;

            if (!CanExport(exportType) || exportType != ExportType.Xml)
                throw new NotImplementedException();

            var items = await callbacks.Player.ContentDirectory.Browse(BaseObjectID, "BrowseDirectChildren", "*", start, blocksize, "");
            items.ThrowIfFailed();
            var xml = XElement.Parse(items.Result);
            start += items.NumberReturned;
            uint total = items.TotalMatches;

            while (start < total)
            {
                items = await callbacks.Player.ContentDirectory.Browse(BaseObjectID, "BrowseDirectChildren", "*", start, blocksize, "");
                items.ThrowIfFailed();
                var more = XElement.Parse(items.Result);
                start += items.NumberReturned;
                if (items.NumberReturned == 0)
                    break;                      // in case changes during enumeration
                xml.LastNode.AddAfterSelf(more.Nodes());
            }

            return xml.ToString();
        }

        protected async Task<string> BaseExportAsync(PlayerCallbacks callbacks, Func<MusicItem,string> formatter)
        {
            Reset();

            var sb = new StringBuilder();
            for(; ;)
            {
                var items = await GetNextItemsAsync(callbacks);
                if (items == null)
                    break;
                foreach (var item in items)
                {
                    sb.Append(formatter(item));
                    sb.AppendLine();
                }
            }
            return sb.ToString();
        }

        public IMusicItemEnumerator GetSearchEnumerator(string search)
        {
            return new UPnPMusicItemFinder(search);
        }
    }


    public class UPnPMusicItemFinder : IMusicItemEnumerator, IMusicItemSearchable
    {
        private struct SearchInfo
        {
            public SearchInfo(string id, string display)
            {
                Id = id;
                Display = display;
            }
            public string Id;
            public string Display;
        }

        private static SearchInfo[] SearchableThings = new SearchInfo[] {
                new SearchInfo("A:ALBUMARTIST", StringResource.Get("Artists")),
                new SearchInfo("A:ALBUM", StringResource.Get("Albums")),
                new SearchInfo("A:COMPOSER", StringResource.Get("Composers")),
                new SearchInfo("A:TRACKS", StringResource.Get("Tracks")),
                new SearchInfo("A:GENRE", StringResource.Get("Genres")),
            };

        // This is the number of individual items we show in a Search category result, more than this and we just list an Enumerator
        internal const int SearchMax = 5;
        
        private string SearchItem;
        private bool Finished;

        public bool CanSearch => true;

        public string DisplayName { get; private set; }

        public string ArtUri => null;

        public DisplayData ParentDisplayMode => null;

        public MusicItem EmptyItem => null;

        public UPnPMusicItemFinder(string what)
        {
            DisplayName = StringResource.Get("MusicLibraryTitle") + ": " + what;
            SearchItem = what;
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

            var service = callbacks.Player.ContentDirectory;
            var result = new List<MusicItem>();

            foreach (var category in SearchableThings)
            {
                var things = await service.Browse(category.Id + ":" + SearchItem, "BrowseDirectChildren", "*", 0, SearchMax, "");
                if (things.TotalMatches > SearchMax)
                {
                    // Lots of items in that category, so just add an enumerator
                    var item = new UPnPMusicItem(string.Format("{0} ({1})", category.Display, things.TotalMatches), true, category.Id + ":" + SearchItem);
                    result.Add(item);
                }
                else if (things.TotalMatches > 0)
                {
                    // add the actual items
                    var results = UPnPMusicItem.Parse(things.Result, service.BaseUri);
                    result.AddRange(results);
                }
            }

            Finished = true;
            return result;
        }

        public async Task<int> CountAsync(PlayerCallbacks callbacks)
        {
            return await Task.FromResult(-1);
        }

        public IMusicItemEnumerator GetSearchEnumerator(string search)
        {
            return new UPnPMusicItemFinder(search);
        }

        public IMusicItemEnumerator GetChildrenEnumerator(MusicItem parent)
        {
            if (!parent.IsContainer)
            {
                throw new ArgumentException("not a container", "parent");
            }

            var music = parent as UPnPMusicItem;
            if (music == null)
            {
                throw new ArgumentException("not UPnP music", "parent");
            }

            return new UPnPMusicItemEnumerator(music.Title, music.Id);
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

    public class SonosPlaylistEnumerator : UPnPMusicItemEnumerator
    {
        public SonosPlaylistEnumerator() : base(StringResource.Get("MusicSonosPlaylists"), "SQ:")
        {
        }

        public SonosPlaylistEnumerator(string title, string id) : base(title, id)
        {
        }

        public override bool CanExport(ExportType exportType)
        {
            return BaseObjectID != "SQ:";
        }

        protected override IMusicItemEnumerator GetEnumerator(UPnPMusicItem music)
        {
            return new SonosPlaylistEnumerator(music.Title, music.Id);
        }

        public override async Task<string> ExportAsync(PlayerCallbacks callbacks, ExportType exportType)
        {
            if (!CanExport(exportType))
                throw new NotImplementedException();

            if (exportType == ExportType.Xml)
            {
                return await base.ExportAsync(callbacks, exportType);
            }
            else if (exportType == ExportType.SoundIIZ)
            {
                Reset();
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(@"<?xml version=""1.0""?>");
                sb.AppendLine("<playlist>");
                for (; ; )
                {
                    var item = await GetNextItemsAsync(callbacks) as UPnPMusicItem;
                    if (item == null)
                        break;
                    sb.AppendLine(" <track>");
                    sb.AppendFormat("  <title>{0}</title>", SecurityElement.Escape(item.Title));
                    sb.AppendFormat("  <artist>{0}</artist>", SecurityElement.Escape(item.Creator));
                    sb.AppendFormat("  <album>{0}</album>", SecurityElement.Escape(item.Album));
                    sb.AppendLine(" </track>");
                }
                sb.AppendLine("</playlist>");
                return sb.ToString();
            }
            else
            {
                return await BaseExportAsync(callbacks, (item) =>
                {
                    var music = item as UPnPMusicItem;
                    return string.Format("{0},{1},{2}", music.Title, music.Album, music.Creator);
                });
            }
        }
    }

    public class MusicLibraryEnumerator : MultiMusicItemEnumerator
    {
        public MusicLibraryEnumerator() : base(StringResource.Get("MusicLibraryTitle"), null)
        {
            AddEnumerator(new UPnPMusicItemEnumerator(StringResource.Get("Artists"), "A:ALBUMARTIST"));
            AddEnumerator(new UPnPMusicItemEnumerator(StringResource.Get("Albums"), "A:ALBUM"));
            AddEnumerator(new UPnPMusicItemEnumerator(StringResource.Get("Composers"), "A:COMPOSER"));
            AddEnumerator(new UPnPMusicItemEnumerator(StringResource.Get("Genres"), "A:GENRE"));
            AddEnumerator(new UPnPMusicItemEnumerator(StringResource.Get("Tracks"), "A:TRACKS"));
            AddEnumerator(new UPnPMusicItemEnumerator(StringResource.Get("ImportedPlaylists"), "A:PLAYLISTS"));
            AddEnumerator(new UPnPMusicItemEnumerator(StringResource.Get("Folders"), "S:"));
            AddEnumerator(new LineInEnumerator());
        }

        public override bool CanSearch => true;

        public override IMusicItemEnumerator GetSearchEnumerator(string search)
        {
            return new UPnPMusicItemFinder(search);
        }
    }

    public class FavoritesEnumerator : UPnPMusicItemEnumerator
    {
        private readonly MusicServiceProvider MusicServiceProvider;

        public FavoritesEnumerator(MusicServiceProvider provider) : base(StringResource.Get("MusicSonosFavorites"), "FV:2")
        {
            MusicServiceProvider = provider;
        }

        protected override IMusicItemEnumerator GetEnumerator(UPnPMusicItem music)
        {
            // Children of Favorites are special
            if (IsMusicServiceEnumerator(music, out var enumerator))
            {
                return enumerator;
            }
            else
            {
                string id = UPnPMusicItem.Parse(music.Metadata, null).First().Id;
                return new UPnPMusicItemEnumerator(music.Title, id);
            }
        }

        private bool IsMusicServiceEnumerator(UPnPMusicItem music, out IMusicItemEnumerator enumerator)
        {
            string desc, id, classname;

            enumerator = null;

            if (music.Res == null || !music.Res.StartsWith("x-rincon-cpcontainer:") || music.Metadata == null)
            {
                return false;
            }

            try
            {
                var xml = XElement.Parse(music.Metadata);
                classname = xml.Descendants(XName.Get("class", UPnPMusicItem.UPNP)).First().Value;
                if (!classname.StartsWith("object.container"))
                {
                    return false;
                }
                id = xml.Descendants(XName.Get("item", UPnPMusicItem.NS)).First().Attribute("id").Value;
                desc = (string)xml.Descendants(XName.Get("desc", UPnPMusicItem.NS)).First();
            }
            catch (InvalidOperationException)
            {
                // Iffy xml
                return false;
            }
            catch (XmlException)
            {
                return false;
            }

            if (desc == "RINCON_AssociatedZPUDN")
            {
                // some UPNP do this eg playlists
                return false;
            }

            if (id.Length < 9 || MusicServiceProvider == null)
            {
                // isnt a hex id (or no music services)
                return false;
            }

            DisplayData dd = null;
            var svc = MusicServiceProvider.GetServiceFromUdnMaybe(desc);
            if (svc == null)
            {
                return false;
            }

            if (classname.Contains("#"))
            {
                string dtype = classname.Split('#')[1];
                svc.DisplayModesByType.TryGetValue(dtype, out dd);
                music.DisplayData = dd;
            }

            // id is Url encoded with a hex prefix, and we need it raw
            id = Uri.UnescapeDataString(id.Substring(8));

            enumerator = new MusicServiceItemEnumerator(svc, id, music.Title, 10, dd, music.ArtUri);
            return true;
        }

        public override bool CanExport(ExportType exportType)
        {
            return exportType == ExportType.Xml || exportType == ExportType.Text;
        }

        public override async Task<string> ExportAsync(PlayerCallbacks callbacks, ExportType exportType)
        {
            if (exportType == ExportType.Xml)
            {
                return await base.ExportAsync(callbacks, ExportType.Xml);
            }

            return await BaseExportAsync(callbacks, (item) => string.Format("{0} - {1}", item.Title, item.Subtitle));
        }
    }

    public class ArtistEnumerator : UPnPMusicItemEnumerator
    {
        public ArtistEnumerator() : base(StringResource.Get("Artists"), "A:ALBUMARTIST")
        {
        }
    }

    public class RadioStationsEnumerator : UPnPMusicItemEnumerator
    {
        public RadioStationsEnumerator() : base(StringResource.Get("MyRadioStations"), "R:0/0")
        {
        }

        public override bool CanExport(ExportType exportType)
        {
            return exportType == ExportType.Xml || exportType == ExportType.Text;
        }

        public override async Task<string> ExportAsync(PlayerCallbacks callbacks, ExportType exportType)
        {
            if (exportType == ExportType.Xml)
            {
                return await base.ExportAsync(callbacks, ExportType.Xml);
            }

            return await BaseExportAsync(callbacks, (item) => item.Title);
        }
    }

    public class RadioShowsEnumerator : UPnPMusicItemEnumerator
    {
        public RadioShowsEnumerator() : base(StringResource.Get("MyRadioShows"), "R:0/1")
        {
        }

        public override bool CanExport(ExportType exportType)
        {
            return exportType == ExportType.Xml || exportType == ExportType.Text;
        }

        public override async Task<string> ExportAsync(PlayerCallbacks callbacks, ExportType exportType)
        {
            if (exportType == ExportType.Xml)
            {
                return await base.ExportAsync(callbacks, ExportType.Xml);
            }

            return await BaseExportAsync(callbacks, (item) => item.Title);
        }
    }

    public class LineInEnumerator : IMusicItemEnumerator
    {
        public string DisplayName => StringResource.Get("MusicLineIn");

        public MusicItem EmptyItem => null;

        public string ArtUri => null;
        public DisplayData ParentDisplayMode => null;



        public bool CanExport(ExportType exportType)
        {
            return false;
        }

        private List<MusicItem> Cache;
        private int Current;

        public Task<int> CountAsync(PlayerCallbacks callbacks)
        {
            return Task.FromResult(-1);
        }

        public IMusicItemEnumerator GetChildrenEnumerator(MusicItem parent)
        {
            throw new NotSupportedException();
        }

        public async Task<IEnumerable<MusicItem>> GetNextItemsAsync(PlayerCallbacks callbacks)
        {
            if (Cache == null)
            {
                Cache = new List<MusicItem>();

                foreach(var player in callbacks.AllPlayers)
                {
                    await TryAddLineIn(player);
                }
            }

            if (Current >= Cache.Count)
            {
                return null;
            }

            Current += Cache.Count;
            return Cache;
        }

        private async Task TryAddLineIn(Player player)
        {
            if (player.ContentDirectory == null)
            {
                // eg Surround rears
                return;
            }

            var items = await player.ContentDirectory.Browse("AI:", "BrowseDirectChildren", "*", 0, 10, "");                    // assume max of 10 per device!
            if ((items.Error != null) || (items.NumberReturned == 0))
                return;

            var list = DidlData.Parse(items.Result);
            foreach (var linein in list)
            {
                // We need to get the Metadata now, as at Playback time it wont be available (as it will be on a different device)
                var md = await player.ContentDirectory.Browse(linein.Id, "BrowseMetadata", "*", 0, 1, "");
                md.ThrowIfFailed();
                
                var item = new UPnPMusicItem(
                    string.Format("{0}: {1}", linein.Title, player.RoomName),
                    false,
                    linein.Id,
                    md.Result,
                    linein.Res,
                    playable: true,
                    favable: true);
    
                Cache.Add(item);
            }
        }

        public void Reset()
        {
            Cache = null;
            Current = 0;
        }

        public Task<string> ExportAsync(PlayerCallbacks callbacks, ExportType exportType)
        {
            throw new NotImplementedException();
        }
    }

    public class RadioEnumerator : MultiMusicItemEnumerator
    {
        private readonly MusicService RadioService;
        private const int ChunkSize = 100;              // required for TuneIn paging to work correctly

        public RadioEnumerator(MusicServiceProvider provider) : base(StringResource.Get("RadioTitle"), null)
        {
            NeedsInitializing = true;
            RadioService = provider.CreateRadioService();

            AddEnumerator(new RadioStationsEnumerator());
            AddEnumerator(new RadioShowsEnumerator());
        }

        protected override async Task InitializeAsync(PlayerCallbacks callbacks)
        {
            var radio = await callbacks.Player.SystemProperties.GetString("R_RadioLocation");

            if (radio.Error == null)
            {
                // <9 chars><id>,<display>
                int comma = radio.StringValue.IndexOf(',');
                if (comma > 9)
                {
                    string localId = radio.StringValue.Substring(9, comma - 9);
                    string location = radio.StringValue.Substring(comma + 1);

                    AddEnumerator(new MusicServiceItemEnumerator(RadioService, localId, string.Format(StringResource.Get("Local_Radio"), location), ChunkSize));
                }
            }

            var tuneIn = new MusicServiceItemEnumerator(RadioService, "root", "TuneIn", ChunkSize);
            var tuneInItems = tuneIn.GetAsObservable(callbacks.Player);
            await tuneIn.StartReadAsync(callbacks.Player);
            foreach (var r in tuneInItems)
            {
                var radioItem = r as MusicServiceItem;
                var radioEnum = new MusicServiceItemEnumerator(RadioService, radioItem.Id, radioItem.Title, ChunkSize, null, radioItem.ArtUri);
                AddEnumerator(radioEnum);
            }
        }
    }

    // Only suitable for Export, not generate enumeration
    public class AlarmsEnumerator : IMusicItemEnumerator
    {
        public string DisplayName => "Alarms";

        public MusicItem EmptyItem => null;

        public string ArtUri => throw new NotImplementedException();

        public DisplayData ParentDisplayMode => null;

        public bool CanExport(ExportType exportType)
        {
            return exportType == ExportType.Xml || exportType == ExportType.Text;
        }

        public Task<int> CountAsync(PlayerCallbacks callbacks)
        {
            throw new NotImplementedException();
        }

        public async Task<string> ExportAsync(PlayerCallbacks callbacks, ExportType exportType)
        {
            var data = await callbacks.Player.AlarmClock.ListAlarms();
            data.ThrowIfFailed();
            string xml = data.CurrentAlarmList;
            if (exportType == ExportType.Xml)
            {
                return xml;
            }
            else
            {
                var result = XElement.Parse(xml);
                var alarms = from alarm in result.Descendants(XName.Get("Alarm"))
                             select new AlarmData(alarm, callbacks.AllPlayers);
                alarms = alarms.OrderBy(a => a.RoomName);
                return string.Join("\n", alarms);
            }
        }

        public IMusicItemEnumerator GetChildrenEnumerator(MusicItem parent)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<MusicItem>> GetNextItemsAsync(PlayerCallbacks callbacks)
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }
    }
}
