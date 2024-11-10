using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Linq;
using System.Text;
using System.Net.Http;
using System.Security;
using System.Diagnostics;
using System.Linq;
using System.Globalization;
using OpenPhonos.UPnP;
using System.Text.Json;
using System.Xml;

namespace OpenPhonos.Sonos
{

    // (This exists because async methods cannot have out args)
    public class MusicServiceResult
    {
        internal int MaximumItems { get; private set; }
        public IEnumerable<MusicServiceItem> Items { get; private set; }

        internal MusicServiceResult(IEnumerable<MusicServiceItem> list, int max)
        {
            MaximumItems = max;
            Items = list;
        }
    }

    /// <summary>
    /// Represents the SMAPI xml data
    /// </summary>
    class SMAPIItem
    {
        public string id;
        public string title;
        public string itemtype;
        public DisplayData displayType;
        public bool canPlay;
        public bool cannotAlarm;
        public string subtitle;
        public bool canEnumerate;
        public bool canFavorite;
        public bool @explicit;
        public string art;
        public string mimetype;
        public string trackNumber;
        public Dictionary<string, string> attributes;
    }

    [DebuggerDisplay("{Description.Name,nq} ({Nickname,nq})")]
    public class MusicService
    {
        public string DisplayName
        {
            get
            {
                if (string.IsNullOrEmpty(Nickname))
                    return Description.Name;
                else
                    return string.Format("{0} ({1})", Description.Name, Nickname);
            }
        }

        public string LogoUri { get; }
        public string Udn { get; }
        public string SerialNumber { get; }
        public string RawName
        {
            get
            {
                return Description.Name;
            }
        }

        public string Nickname { get; }                        // can be null or empty

        public string Tier { get; private set; }
        public Dictionary<string, string> QualityBadges { get; private set; }
        public Dictionary<string, DisplayData> DisplayModesById { get; private set; }
        public Dictionary<string, DisplayData> DisplayModesByType { get; private set; }

        private readonly MusicServiceProvider Provider;
        private MusicServiceDescription Description;
        private string UserName;
        private string Token;
        private string SessionId;
        private string AccountKey;

        private bool NeedFreshToken;
        private bool GoogleApi;
        private string GoogleApiKey;
        private string SonosApiKey;

        private const string ServiceNamespace = "http://www.sonos.com/Services/1.1";
        //public const int GulpSize = 25;              // we can't read more than this (Google limits results to 1000, XBM 25-30)
        public const int GulpSize = 100;         // services can request different using Presentation BrowseOptions.PageSize

        public const int YTMXml = 284;
        public const int GoogleXml = 151;

        public static string ControllerId { get; set; }

        internal MusicService(MusicServiceProvider provider, MusicServiceDescription available, string udn, string user, string key, string token, string serial, string nickname)
        {
            if (string.IsNullOrEmpty(ControllerId))
            {
                throw new ArgumentNullException("ControllerId");
            }

            this.Provider = provider;
            this.Description = available;
            this.Udn = udn;
            this.UserName = user;
            this.AccountKey = key;
            this.Token = token;
            this.SerialNumber = serial;
            this.Nickname = nickname;
            this.LogoUri = available.LogoUri;

            if (Description.XmlId == YTMXml)
            {
                this.GoogleApi = true;
                this.NeedFreshToken = true;
            }

            this.QualityBadges = new Dictionary<string, string>();
            this.DisplayModesById = new Dictionary<string, DisplayData>();
            this.DisplayModesByType = new Dictionary<string, DisplayData>();

            this.EnsureInitialized = new Lazy<Task>(() => InitializeOnceAsync(), true);
        }

        private Lazy<Task> EnsureInitialized;

        private async Task InitializeOnceAsync()
        {
            await InitializeManifestAsync();
            await InitializePresentationMapAsync();
        }

        public async Task InitializeAsync()
        {
            await EnsureInitialized.Value;
        }

        public bool IsInitialized => EnsureInitialized.IsValueCreated;

        private bool IsAnonymous { get { return this.Description.Auth == "Anonymous"; } }
        private bool IsUserId { get { return this.Description.Auth == "UserId"; } }
        private bool IsDeviceLink { get { return this.Description.Auth == "DeviceLink" || this.Description.Auth == "AppLink"; } }
        private bool IsRadio { get { return this.Description.Auth == "Radio"; } }

        [Flags]
        enum CapabilityFlag
        {
            Search = 1,
            FavTracks = 2,
            FavAlbums = 0x10,
            LogTrackEnd = 0x40,
            ExtendedMedatata = 0x200,
            DisableAlarms = 0x400,
            UserContent = 0x800,
            LogTrackPlaying = 0x1000,
            AccountLogging = 0x2000,
            DisableMultipleAccounts = 0x4000,
            Actions = 0x8000,
            Context = 0x10000,
            DeviceCert = 0x20000,
            ZonePlayer = 0x40000,
            PlayContext = 0x80000,
            EnableUserInfo = 0x100000,
            ContentFiltering = 0x200000,
            SupportManifestFile = 0x400000,
            ExtendedMetadataForRadio = 0x800000,
        }

        public bool IsSearchable
        {
            get
            {
                return (Description.Capabilities & 1) != 0;
            }
        }

        private bool NeedsContext
        {
            get
            {
                return (Description.Capabilities & (1 << 16)) != 0;      // new for 5.5
            }
        }

        public async Task<MusicServiceResult> GetItemsAsync(Player player, string id, int startIndex, int maxcount, bool recurse)
        {
            await InitializeAsync();

            // Accuradio cannot handle recursive, so only send it if set
            string rec = recurse ? "<recursive>" + recurse.ToString().ToLower() + "</recursive>" : string.Empty;

            string soap = "<getMetadata xmlns=\"" + ServiceNamespace + "\">" +
            "<id>" + SecurityElement.Escape(id) + "</id>" +
            "<index>" + startIndex.ToString() + "</index>" +
            "<count>" + maxcount.ToString() + "</count>" +
            rec +
            "</getMetadata>";

            MusicServiceResult items = null;
            Exception caught = null;
            bool first = id == "root" && startIndex == 0;

            // Always try SMAPI first in order to update tokens etc
            try
            {
                items = await GeneralItemsAsync(player, soap, "getMetadata", id);
            }
            catch (MusicServiceException ex)
            {
                // Don't throw this unless json fails
                caught = ex;
            }

            // Try JSON afterwards if it makes sense to
            if (first)
            {
                var json = await GetRootItemsByJsonAsync(player);
                if (json != null)
                {
                    items = json;
                    caught = null;
                }
            }

            if (caught != null)
                throw caught;

            return items;
        }

        private async Task<MusicServiceResult> GeneralItemsAsync(Player player, string soap, string verb, string name)
        {
            XElement result = null;

            for (int i = 0; i < 3; i++)
            {
                await VerifyAvailableAsync(player);

                try
                {
                    result = await DoRequestAsync(false, verb, CredentialHeader(player, !GoogleApi), soap, ContextHeader());
                    break;
                }
                catch (RetryException)
                {
                    Debug.WriteLine("Will retry on {0}", (object)this.DisplayName);
                    continue;
                }
            }

            if (result == null)
                throw new FileNotFoundException();

            var results = new List<MusicServiceItem>();

            int max = 0;
            Debug.WriteLine("index={0} count={1} total={2}",
                (string)result.Descendants(XName.Get("index", ServiceNamespace)).FirstOrDefault(),
                (string)result.Descendants(XName.Get("count", ServiceNamespace)).FirstOrDefault(),
                (string)result.Descendants(XName.Get("total", ServiceNamespace)).FirstOrDefault()
                );

            int.TryParse((string)result.Descendants(XName.Get("total", ServiceNamespace)).FirstOrDefault(), out max);      // as Murfie returns "undefined" for total

            foreach (var item in result.Descendants())
            {
                SMAPIItem music = null;

                // (XBM Top Songs returns a mixture of mediaCollection and mediaMatadata)
                if (item.Name == XName.Get("mediaCollection", ServiceNamespace))
                {
                    bool playable = BoolParse((string)item.Element(XName.Get("canPlay", ServiceNamespace)), false);
                    bool enumerable = BoolParse((string)item.Element(XName.Get("canEnumerate", ServiceNamespace)), true);

                    // Containers come as mediaCollection items (which can be playable)
                    music = new SMAPIItem()
                    {
                        id = (string)item.Element(XName.Get("id", ServiceNamespace)),
                        title = (string)item.Element(XName.Get("title", ServiceNamespace)) ?? "",
                        itemtype = (string)item.Element(XName.Get("itemType", ServiceNamespace)),
                        displayType = ExtractDisplayType(item),
                        canPlay = playable,
                        canFavorite = BoolParse((string)item.Element(XName.Get("canAddToFavorites", ServiceNamespace)), true),
                        subtitle = (string)item.Element(XName.Get("artist", ServiceNamespace)),
                        canEnumerate = enumerable,
                        art = (string)item.Element(XName.Get("albumArtURI", ServiceNamespace)),
                        attributes = ExtractAttributes(item),
                        @explicit = ExtractExplicit(item),
                    };
                }
                else if (item.Name == XName.Get("mediaMetadata", ServiceNamespace))
                {
                    var trackMd = item.Descendants(XName.Get("trackMetadata", ServiceNamespace));

                    // Actual tracks come as mediaMetadata items
                    music = new SMAPIItem()
                    {
                        id = (string)item.Element(XName.Get("id", ServiceNamespace)),
                        title = (string)item.Element(XName.Get("title", ServiceNamespace)),
                        itemtype = (string)item.Element(XName.Get("itemType", ServiceNamespace)),
                        trackNumber = (string)trackMd.Elements(XName.Get("trackNumber", ServiceNamespace)).FirstOrDefault(),
                        canPlay = BoolParse((string)trackMd.Elements(XName.Get("canPlay", ServiceNamespace)).FirstOrDefault(), true),
                        canFavorite = BoolParse((string)trackMd.Elements(XName.Get("canAddToFavorites", ServiceNamespace)).FirstOrDefault(), true),
                        subtitle = ExtractSubtitle(item),
                        art = ExtractArt(item),
                        canEnumerate = false,
                        mimetype = (string)item.Element(XName.Get("mimeType", ServiceNamespace)),
                        attributes = ExtractAttributes(item, trackMd.FirstOrDefault()),
                        @explicit = ExtractExplicit(item),
                    };

                    var stream = item.Descendants(XName.Get("streamMetadata", ServiceNamespace)).FirstOrDefault();
                    if (stream != null)
                    {
                        music.cannotAlarm = BoolParse((string)stream.Elements(XName.Get("isEphemeral", ServiceNamespace)).FirstOrDefault(), false);
                    }
                }
                else
                    continue;

                // Make sure the args passed have the colons removed
                var md = CreateDidlData(music, EscapeUrl(music.id), EscapeUrl(name), music.art);
                if (md == null)
                {
                    // Item was "bad" (eg non-playable Slacker streams) so ignore it
                    continue;
                }
                if (!string.IsNullOrEmpty(music.art))
                {
                    // If its https artwork, it will take ages to return an error, so force the url to be "bad" so it fails quickly
                    if (music.art.StartsWith("https://cloudplayer.ws.sonos.com/VirtualAssets"))
                        music.art = "x-sonos-auth-" + music.art;
                    md.AlbumArtURI = music.art;
                }
                else
                {
                    music.art = null;
                    md.AlbumArtURI = null;
                }

                string title = music.title;
#if DEBUG
                if (music.displayType != null)
                {
                    title += " (" + music.displayType.Summary() + ")";
                }
#endif

                results.Add(new MusicServiceItem(
                    music.id,             // this is used to drill down, must remain "true"
                    title,
                    music.subtitle,
                    md.IsPlayable,
                    md.IsQueueable,
                    md.IsContainer,
                    music.canFavorite,
                    md.IsPlayable && !music.cannotAlarm,
                    music.@explicit,
                    md.Metadata,
                    md.Res.Replace("/", "%2f").Replace("#", "%23"),         // & critical for Radio (removed), / for Prime stations
                    music.art,
                    this
                )
                {
                    DisplayData = music.displayType,
                    TrackNumber = music.trackNumber,
                    Attributes = music.attributes,
                });
            }

            return new MusicServiceResult(results, max);
        }

        private static string EscapeUrl(string url)
        {
            return url.Replace(":", "%3a").Replace("/", "%2f").Replace("?", "%3f").Replace("=", "%3d").Replace("&", "%26").Replace("#", "%23");
        }

        private static string GetTimeZone()
        {
            // eg "-08:00" for PST
            var offset = TimeZoneInfo.Local.BaseUtcOffset;
            return string.Format("{0:D2}:{1:D2}", offset.Hours, offset.Minutes);
        }

        private string ContextHeader()
        {
            if (!NeedsContext)
                return null;

            // eg "-08:00" for PST
            var offset = TimeZoneInfo.Local.BaseUtcOffset;
            var tz = string.Format("{0:D2}:{1:D2}", offset.Hours, offset.Minutes);

            return "<context xmlns=\"http://www.sonos.com/Services/1.1\">" +
                "<timeZone>" + tz + "</timeZone>" +
                "</context>";
        }

        protected string CredentialHeader(Player player, bool tokkey)
        {
            if (IsAnonymous || IsRadio)
            {
                // We miss some radio stations by NOT passing credentials (eg KXNT?)
                return "<credentials xmlns=\"" + ServiceNamespace + "\">" +
                    DeviceIdTagged(player) +
                    "<deviceProvider>Sonos</deviceProvider>" +
                    "</credentials>";
            }
            else if (AccountKey != null)
            {
                string creds = "<credentials xmlns=\"" + ServiceNamespace + "\">" +
                    DeviceIdTagged(player) +
                    "<deviceProvider>Sonos</deviceProvider>" +
                    "<loginToken>";
                if (tokkey)
                    creds += "<token>" + SecurityElement.Escape(Token) + "</token>" +
                    "<key>" + SecurityElement.Escape(AccountKey) + "</key>";

                creds += "<householdId>" + SecurityElement.Escape(HouseholdIdX()) + "</householdId>" +
                    "</loginToken></credentials>";
                return creds;
            }
            else
                return "<credentials xmlns=\"" + ServiceNamespace + "\">" +
                    DeviceIdTagged(player) +
                "<deviceProvider>Sonos</deviceProvider>" +
                "<sessionId>" + SecurityElement.Escape(SessionId) + "</sessionId>" +
                "</credentials>";
        }

        private string HouseholdIdX()
        {
            // If the username includes an id, and that id is non-zero then return a special hh, that matches the OADevID column on the account page
            string hh = Provider.HouseholdId;
            if (this.UserName != null)
            {
                string[] split = this.UserName.Split('-');
                if ((split.Length == 3) && (split[1] != "0"))
                    hh += "_" + split[1];
            }
            return hh;
        }

        private string DeviceIdTagged(Player player)
        {
            if (Provider.DeviceId != null)
                return "<deviceId>" + SecurityElement.Escape(Provider.DeviceId) + "</deviceId>"; // +*/ "<zonePlayerId>" + SecurityElement.Escape(player.UniqueName) + "</zonePlayerId>";
            else
            {
                //Telemetry.ShipAssert("NoDeviceId", "");
                return string.Empty;
            }
        }

        private string ConvertContainerClass(string type, out string prefix, out string resbase)
        {
            // for Sonos Favorites we likely need correct values here, but for enum it doesnt matter
            // and for playback, well I guess not...
            string uclass = "object.container.album.musicAlbum";
            resbase = "x-rincon-cpcontainer:";
            int what = 8;

            switch (type)
            {
                case "artist":
                    what = 5;
                    break;
                case "album":
                    what = 4;
                    break;
                case "genre":
                    what = 7;
                    break;
                case "playlist":
                    what = 6;
                    break;
                case "favorites":
                    what = 0xA;
                    break;
                case "albumList":
                    what = 0xD;
                    break;
                case "trackList":
                    what = 0xE;
                    uclass = "object.container.playlistContainer";
                    break;
                case "artistTrackList":
                    what = 0xF;
                    break;
                case "search":          // TODO better
                case "container":
                case "collection":      // TODO figure out real value (MOG)
                case null:              // TODO figure out real value (Songza root)
                    what = 8;
                    break;
                case "":
                    Debug.Assert(false);
                    what = 8;       // TODO better one day?
                    break;
                    // missing values: 0?,1,2,b
            }
            prefix = what.ToString("d4");
            return uclass;
        }

        private DidlData CreateDidlData(SMAPIItem item, string id, string parent, string art)
        {
            bool playable = item.canPlay;
            bool enumerable = item.canEnumerate;
            bool queueable = playable;
            bool faveable = item.canFavorite;

            DidlData data = new DidlData();
            data.Id = id;
            data.Title = item.title;
            data.IsContainer = enumerable;
            data.TrackNumber = item.trackNumber;

            string uclass = string.Empty;
            string res = string.Empty;
            string me = string.Empty;
            string parentId = string.Empty;
            string problem = null;
            string problemmore = null;
            string addserial = string.IsNullOrEmpty(this.SerialNumber) ? string.Empty : "&sn=" + SerialNumber;
            string mime = item.mimetype;
            if (mime != null)
                mime = mime.ToLower();

            switch (item.itemtype)
            {
                case "artist":
                case "album":
                case "genre":
                case "playlist":
                case "favorites":
                case "search":
                case "albumList":
                case "trackList":
                case "artistTrackList":
                case "container":
                case "collection":
                case "":
                case null:
                    {
                        string index;
                        string resbase;
                        uclass = ConvertContainerClass(item.itemtype, out index, out resbase);
                        res = resbase + index + "008c" + id;
                        me = index + "008c";
                        parentId = "000d0084";
                    }
                    break;

                case "track":       // always 3
                    switch (mime)
                    {
                        case "audio/x-spotify":
                            uclass = "object.item.audioItem.musicTrack";
                            res = "x-sonos-spotify:" + id + "?sid=" + Description.XmlId.ToString() + "&flags=0" + addserial;
                            me = "00030000";
                            parentId = "00040008";
                            break;
                        case "audio/mpeg":  // MOG
                        case "audio/mp3": // Rdio
                        case "audio/mpeg3":  // Murfie
                        case "audio/aac":   // WiMP
                        case "audio/mp4":   // Unknown (in the spec)
                        case "application/x-mpegurl":   // (in the spec)
                        case "application/vnd.apple.mpegurl":   // Amazon 
                        case "audio/mpegurl":                   // SiriusXM
                        case null:
                            uclass = "object.item.audioItem.musicTrack";
                            res = "x-sonos-http:" + id + ".mp3?sid=" + Description.XmlId.ToString() + "&flags=32" + addserial;
                            me = "10030020";
                            parentId = "1004006c";
                            break;
                        case "audio/x-ms-wma":  // Juke
                            uclass = "object.item.audioItem.musicTrack";
                            res = "x-sonos-mms:" + id;
                            me = "00030030";
                            parentId = "00050064";        // made this up!
                            break;

                        case "audio/vnd.radiotime":             // NPR Hours News (Recent Episodes) [not in the spec]
                            uclass = "object.item.audioItem.musicTrack.recentShow";
                            res = "x-sonosapi-rtrecent:" + id + "?sid=" + Description.XmlId.ToString() + "&flags=8224" + addserial;
                            me = "F00032020";
                            parentId = "F00082064";
                            break;

                        // no known users but treat it like the others, Just In Case it works anyway!
                        case "audio/wma":           // MusicBay-Interpreten
                        case "audio/ogg":           // QQ?? uses this
                        case "application/ogg":     // (in the spec)
                            uclass = "object.item.audioItem.musicTrack";
                            res = "x-sonos-http:" + id + ".mp3?sid=" + Description.XmlId.ToString() + "&flags=32" + addserial;
                            me = "10030020";
                            parentId = "1004006c";
                            break;

                        case "audio/flac":          // Amazon HD
                            uclass = "object.item.audioItem.musicTrack";
                            res = "x-sonos-http:" + id + ".flac?sid=" + Description.XmlId.ToString() + "&flags=0" + addserial;
                            me = "10030020";
                            parentId = "1004006c";

                            break;

                        default:
                            // Amazon does this sometimes
                            problem = "XMusicTrack";
                            problemmore = item.mimetype;
                            break;
                    }
                    queueable = item.canPlay;           // assume Tracks are always queuable (assuming they are playable)
                    break;

                case "stream":  // always 9
                    switch (item.mimetype)
                    {
                        case "audio/mpeg":      // iheartradio
                        case "audio/mp3":       // "Radio Play"?
                        case "mp3":             // PowerApp
                        case null:              // TuneIn
                            uclass = "object.item.audioItem.audioBroadcast";
                            res = "x-sonosapi-stream:" + id + "?sid=" + Description.XmlId.ToString() + "&flags=32" + addserial;
                            // me?
                            break;
                        case "audio/aac":      // iheartradio (local stations)
                            uclass = "object.item.audioItem.audioBroadcast";
                            res = "x-sonosapi-stream:" + id + "?sid=" + Description.XmlId.ToString() + "&flags=8224" + addserial;
                            break;
                        case "application/x-mpegURL":  // SiriusXM
                        case "application/x-mpegurl":
                        case "audio/x-mpegurl":        // Live365, Radionomy
                        case "audio/x-scpls":          // RadioPup
                            uclass = "object.item.audioItem.audioBroadcast";
                            res = "x-sonosapi-hls:" + id + "?sid=" + Description.XmlId.ToString() + "&flags=288" + addserial;
                            break;
                        default:
                            problem = "XMusicStream";
                            problemmore = item.mimetype;
                            break;
                    }
                    queueable = false;
                    break;

                case "program": // always c
                    // MOG artist radio, no mimetype as got by enumerating later
                    // also Rdio
                    uclass = "object.item.audioItem.audioBroadcast";
                    res = "x-sonosapi-radio:" + id + "?sid=" + Description.XmlId.ToString() + "&flags=104" + addserial;
                    me = "000c0068";
                    parentId = "00050064";
                    playable = true;
                    queueable = false;

                    // may be specified as Enumerable but only to allow "Program" playback, not in the UI
                    data.IsContainer = false;
                    enumerable = false;
                    break;

                case "show":
                    uclass = "object.item.audioItem.podcast";
                    res = "x-sonosapi-hls-static:" + id + "?sid=" + Description.XmlId.ToString() + "&flags=24616" + addserial;
                    me = "10156128";
                    parentId = "10140064";
                    playable = item.canPlay;
                    break;

                case "audiobook":
                    uclass = "object.item.audioItem.audioBook";
                    res = "x-rincon-cpcontainer:101340c8" + id + "?sid=" + Description.XmlId.ToString() + "&flags=16584" + addserial;
                    me = "101340c8";
                    parentId = "100d0c4";
                    playable = item.canPlay;
                    // TODO more faveable = true;
                    break;

                default:
                    if (enumerable)
                    {
                        // Metadata seems to allow any old type eg Wolfgang's Vault 
                        // so as long as it is enumerable, we'll handle it
                        string index;
                        string resbase;
                        uclass = ConvertContainerClass("container", out index, out resbase);
                        res = resbase + index + "008c" + id;
                        me = index + "008c";
                        parentId = "000d0084";
                    }
                    else if (item.itemtype == "other")
                    {
                        // eg items in DAR.fm which are not playable or enumerable
                        data.Class = "object.nothing";
                        data.Res = string.Empty;
                        data.Metadata = string.Empty;
                        data.IsContainer = false;
                        playable = false;
                    }
                    else
                    {
                        problem = "XmusicItem";
                        problemmore = item.itemtype;
                    }
                    break;
            }

            if (problem != null)
            {
                Debug.Assert(false);
                data.Class = "object.nothing";
                data.Res = string.Empty;
                data.Metadata = string.Empty;
                data.IsContainer = false;
                playable = false;
                item.title = item.title + "(??)";
            }
            else if (playable)
            {
                data.Class = uclass;
                data.Res = res;
                data.Metadata = StandardMetadata(id, parent, item.title, data.Class, me, parentId, art);
                data.IsPlayable = playable;
                data.IsQueueable = queueable;
                data.IsFavoritable = faveable;
            }
            else
            {
                data.Class = "object.nothing";
                data.Res = string.Empty;
                data.Metadata = string.Empty;
            }

            Debug.Assert(data.IsPlayable == playable);

            return data;
        }

        private static string XmlWrap(string name, string value)
        {
            return $"<{name}>{SecurityElement.Escape(value)}</{name}>";
        }

        private string StandardMetadata(string id, string parent, string title, string uclass, string idNum, string parentNum, string art)
        {
            if (art != null)
            {
                art = XmlWrap("upnp:albumArtURI", art);
            }
            else
            {
                art = string.Empty;
            }

            return $@"<DIDL-Lite 
xmlns:dc=""http://purl.org/dc/elements/1.1/"" 
xmlns:upnp=""urn:schemas-upnp-org:metadata-1-0/upnp/"" 
xmlns:r=""urn:schemas-rinconnetworks-com:metadata-1-0/"" 
xmlns=""urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/"">
<item id=""{idNum}{SecurityElement.Escape(id)}"" parentID=""{parentNum}{SecurityElement.Escape(parent)}"" restricted=""true"">
<dc:title>{SecurityElement.Escape(title)}</dc:title>
<upnp:class>{uclass}</upnp:class>{art}
<desc id=""cdudn"" nameSpace=""urn:schemas-rinconnetworks-com:metadata-1-0/"">{this.Udn}</desc>
</item></DIDL-Lite>";
        }

        private static async Task<XElement> ReadXmlFileAsync(string uri)
        {
            if (string.IsNullOrEmpty(uri))
                return null;

            try
            {
                var client = UPnP.Service.CreateClient(MusicServiceProvider.SonosUserAgent, true, Service.ImageLoaderTimeout);
                var pmap = await client.GetStringAsync(uri);
                var xml = XElement.Parse(FixXmlVersion(pmap));
                return xml;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public class Rating
        {
            public string AutoSkip;
            public string Id;
            public string StringId;
            public string OnSuccessStringId;
            public string LastModified;
            public string Uri;

            internal Rating(XElement rating, string controllerType, Func<string, string> translator)
            {
                Id = (string)rating.Attribute("Id");
                AutoSkip = (string)rating.Attribute("AutoSkip");
                OnSuccessStringId = translator((string)rating.Attribute("OnSuccessStringId"));
                StringId = translator((string)rating.Attribute("StringId"));
                var icon = rating.Descendants("Icon").Where(s => (string)s.Attribute("Controller") == controllerType).FirstOrDefault();
                if (icon != null)
                {
                    LastModified = (string)icon.Attribute("LastModified");
                    Uri = (string)icon.Attribute("Uri");
                }

            }
        }

        private class RatingsMatch
        {
            public string PropName;
            public string Value;
            public Rating [] Ratings;       // 1 or 2 of these
        }

        private List<RatingsMatch> Ratings = new List<RatingsMatch>();

        public Rating[] FindRating(Dictionary<string, string> itemProps)
        {
            foreach(var rating in Ratings)
            {
                if (itemProps.TryGetValue(rating.PropName, out var v) && v == rating.Value)
                {
                    return rating.Ratings;
                }
            }

            return null;
        }

        public async Task<string> GetLastUpdateFavoriteAsync(Player player)
        {
            var xml = await GetLastUpdateAsync(player);
            if (xml == null)
            {
                return null;
            }
            return (string)xml.Descendants(XName.Get("favorites", ServiceNamespace)).FirstOrDefault();
        }

        public async Task<XElement> GetLastUpdateAsync(Player player)
        {
            string soap = "<getLastUpdate xmlns=\"" + ServiceNamespace + "\">" +
            "</getLastUpdate>";

            for (int i = 0; i < 2; i++)
            {
                try
                {
                    var result = await DoRequestAsync(false, "getLastUpdate", CredentialHeader(player, true), soap, null);
                    return result;
                }
                catch (RetryException)
                { }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Exception {ex.Message} during getLastUpdate for {this.RawName}");
                    break;
                }
            }

            return null;
        }

        public class RateItemResponse
        {
            public bool success;
            public bool? shouldSkip;
            public string messageString;
        }

        public async Task<RateItemResponse> RateItemAsync(string id, string rating, Player player)
        {
            string soap = "<rateItem xmlns=\"" + ServiceNamespace + "\">" +
            "<id>" + SecurityElement.Escape(id) + "</id>" +
            "<rating>" + SecurityElement.Escape(rating) + "</rating>" +
            "</rateItem>";

            for (int i = 0; i < 2; i++)
            {
                try
                {
                    var result = await DoRequestAsync(true, "rateItem", CredentialHeader(player, true), soap, null);

                    string msg = (string)result.Descendants(XName.Get("messageStringId", ServiceNamespace)).FirstOrDefault();
                    if (!string.IsNullOrEmpty(msg))
                    {
                        msg = LookupLocalizedString(msg);
                    }
                    return new RateItemResponse()
                    {
                        success = true,
                        shouldSkip = (bool?)result.Descendants(XName.Get("shouldSkip", ServiceNamespace)).FirstOrDefault(),
                        messageString = msg
                    };
                }
                catch (RetryException)
                { }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Exception {ex.Message} during rateItem for {this.RawName} on {id},{rating}");
                    break;
                }
            }

            return new RateItemResponse()
            {
                success = false
            };
        }

        private async Task InitializePresentationMapAsync()
        {
            // Tokens can be localized so get that first
            await InitializeLocalizedStringsAsync();

            if (Description.PresentationMap == null)
            {
                Description.PresentationMap = await ReadXmlFileAsync(Description.PresentationUri);
                if (Description.PresentationMap != null)
                {
                    try
                    {
                        var badges = Description.PresentationMap.Descendants(XName.Get("PresentationMap"))
                            .Where(s => (string)s.Attribute(XName.Get("type")) == "StreamQualityBadgeDictionary")
                            .Descendants(XName.Get("QualityBadgeMap"));

                        foreach (var badge in badges)
                        {
                            string id = (string)badge.Attributes(XName.Get("id")).FirstOrDefault();
                            string text = (string)badge.Attributes(XName.Get("text")).FirstOrDefault();
                            QualityBadges[id] = text;
                        }

                        var rootnodedisplaytype = Description.PresentationMap.Descendants(XName.Get("PresentationMap"))
                            .Where(s => (string)s.Attribute(XName.Get("type")) == "DisplayType")
                            .Descendants(XName.Get("RootNodeDisplayType"))
                            .Descendants(XName.Get("DisplayMode"))
                            .FirstOrDefault();
                        string rootnode = rootnodedisplaytype != null ? rootnodedisplaytype.Value : null;

                        var displayTypes = Description.PresentationMap.Descendants(XName.Get("PresentationMap"))
                            .Where(s => (string)s.Attribute(XName.Get("type")) == "DisplayType")
                            .Descendants(XName.Get("DisplayType"));

                        foreach (var displayType in displayTypes)
                        {
                            // if there is one for the type, use that in preference
                            string type = (string)displayType.Attributes(XName.Get("type")).FirstOrDefault();
                            if (type != null)
                            {
                                DisplayModesByType[type] = DisplayData.FromXml(type, displayType, LookupLocalizedString);
                            }
                            else
                            {
                                string id = (string)displayType.Attributes(XName.Get("id")).FirstOrDefault();
                                if (id != null)
                                {
                                    DisplayModesById[id] = DisplayData.FromXml(id, displayType, LookupLocalizedString);
                                }
                            }
                        }

                        DisplayData.AddMusicServiceDefaults(DisplayModesByType);

                        string controllerType = "pcdcr";

                        bool typematch(string s) => s != null && s.StartsWith("NowPlayingRatings");     // as Pandora uses "_v2"

                        var matches = Description.PresentationMap.Descendants(XName.Get("PresentationMap"))
                            .Where(s => typematch((string)s.Attribute(XName.Get("type"))))
                            .Descendants(XName.Get("Match"));

                        // TODO trackEnabled/programEnabled
                        foreach(var match in matches)
                        {
                            var ratingsMatch = new RatingsMatch();
                            ratingsMatch.PropName = (string)match.Attribute("propname");
                            ratingsMatch.Value = (string)match.Attribute("value");

                            var ratings = match.Descendants(XName.Get("Rating"));
                            ratingsMatch.Ratings = ratings.Select(r => new Rating(r, controllerType, LookupLocalizedString)).ToArray();
                            Ratings.Add(ratingsMatch);
                        }

                        Debug.WriteLine($"{this.RawName} has ratings {string.Join(", ", Ratings.Select(r=>r.PropName + "=" + r.Value))}");

                    }
                    catch (Exception ex)
                    {
                        /*Analytics.TrackEvent("PresentationMap.Exception", new Dictionary<string, string>()
                        {
                            ["uri"] = Description.PresentationUri,
                            ["message"] = ex.Message,
                        });
                        */
                    }
                }
            }
        }

        private Dictionary<string, string> LocalizedStrings;

        private async Task InitializeLocalizedStringsAsync()
        {
            if (LocalizedStrings == null)
            {
                var localizedStrings = new Dictionary<string, string>();

                if (string.IsNullOrEmpty(Description.StringsUri))
                {
                    LocalizedStrings = localizedStrings;
                    return;
                }

                XElement stringsXml;

                try
                {
                    var client = new HttpClient();
                    var pmap = await client.GetStringAsync(Description.StringsUri);
                    stringsXml = XElement.Parse(FixXmlVersion(pmap));
                }
                catch (Exception ex)
                {
                    /* TODO Analytics.TrackEvent("MusicService.StringsLoad", new Dictionary<string, string>()
                    {
                        ["url"] = Description.StringsUri,
                        ["message"] = ex.Message
                    });  */
                    LocalizedStrings = localizedStrings;
                    return;
                }

                const string NS = "http://sonos.com/sonosapi";

                var lang = GetSupportedLocale();

                // Namespace seems random
                var stringXName = XName.Get("string", NS);
                var st = stringsXml.Descendants(XName.Get("stringtable", NS));
                if (st.Count() == 0)
                {
                    st = stringsXml.Descendants(XName.Get("stringtable"));
                    stringXName = XName.Get("string");
                }
                var stringtable = st.Where(x => x.Attribute(XName.Get("lang", "http://www.w3.org/XML/1998/namespace")).Value == lang);
                if (!stringtable.Any())
                {
                    // eg Pandora only has en-US
                    stringtable = st.Where(x => x.Attribute(XName.Get("lang", "http://www.w3.org/XML/1998/namespace")).Value == DefaultLanguage);
                }

                var strings = stringtable.Descendants(stringXName).Where(t => t.Attribute(XName.Get("stringId")) != null);
                foreach (var item in strings)
                {
                    string id = (string)item.Attributes(XName.Get("stringId")).First();
                    localizedStrings[id] = item.Value.Trim(new char[] { '\n', ' ' });
                }

                LocalizedStrings = localizedStrings;                // handle when this is called simultaneously on startup, last one wins
            }
        }

        internal string LookupLocalizedString(string which)
        {
            if (LocalizedStrings.TryGetValue(which, out string result))
            {
                return result;
            }

            return which;
        }

        internal string LookupLocalizedString(string which, string defaultResult)
        {
            if (LocalizedStrings.TryGetValue(which, out string result))
            {
                return result;
            }

            return defaultResult;
        }

        public string GetProtocolInfo(MusicItem item)
        {
            throw new NotImplementedException();
        }

        private MusicServiceResult CachedSearchCategories;

        public async Task<MusicServiceResult> GetSearchCategoriesAsync(Player player)
        {
            if (CachedSearchCategories == null)
            {
                if (Description.PresentationMap != null)
                {
                    var searches = from s
                                   in Description.PresentationMap.Descendants(XName.Get("PresentationMap"))
                                   where (string)s.Attribute(XName.Get("type")) == "Search"
                                   select s;

                    var categories = from c in searches.Descendants(XName.Get("SearchCategories"))
                                     from d in c.Descendants(XName.Get("Category"))
                                     select new
                                     {
                                         stringId = (string)c.Attribute(XName.Get("stringId")),
                                         id = (string)d.Attribute(XName.Get("id")),
                                         mappedId = (string)d.Attribute(XName.Get("mappedId")),
                                     };

                    var list = categories.Select(s =>
                    {
                        string id = s.mappedId;
                        if (id == null)
                            id = s.id;              // mappedId is optional

                        string subtitle = s.stringId; // Needs localization
                        string raw = s.id;

                        return new { id, raw, subtitle };
                    });

                    // eg Stitcher
                    var custom = from c in searches.Descendants(XName.Get("SearchCategories"))
                                 from d in c.Descendants(XName.Get("CustomCategory"))
                                 select new
                                 {
                                     stringId = (string)d.Attribute(XName.Get("stringId")),
                                     mappedId = (string)d.Attribute(XName.Get("mappedId")),
                                 };

                    var newlist = new List<MusicServiceItem>();
                    foreach (var item in list.ToList())
                    {
                        string sub = item.subtitle != null ? LookupLocalizedString(item.subtitle) : null;
                        newlist.Add(new MusicServiceItem(item.id, TranslateId(item.raw), sub, playable: false, queueable: false, container: true, canfave: false, alarmable: false, expl:false, metadata: null, res: null, art: null, this));
                    }

                    foreach (var item in custom.ToList())
                    {
                        string sub = LookupLocalizedString(item.stringId);
                        newlist.Add(new MusicServiceItem(item.mappedId, sub, string.Empty, false, false, true, false, false, false, null, null, null, this));
                    }

                    CachedSearchCategories = new MusicServiceResult(newlist, newlist.Count());
                }
                else
                {
                    // Old API (eg iHeartRadio)
                    var result = await GetItemsAsync(player, "search", 0, 20, false);
                    var newlist = new List<MusicServiceItem>();
                    if (result != null && result.Items?.Count() != 0)
                    {
                        foreach (var item in result.Items)
                        {
                            newlist.Add(new MusicServiceItem(item.Id, item.Title, string.Empty, false, false, true, false, false, false, null, null, null, this));
                        }
                    }

                    CachedSearchCategories = new MusicServiceResult(newlist, newlist.Count());
                }
            }

            return CachedSearchCategories;
        }

        private static string TranslateId(string id)
        {
            string[] idlist = new string[] { "artists", "albums", "composers", "genres", "hosts", "podcasts", "people", "playlists", "stations", "tags", "tracks" };
            string[] reslist = new string[] { "Artists", "Albums", "Composers", "Genres", "SearchHosts", "SearchPodcasts", "SearchPeople", "SearchPlaylists", "SearchStations", "SearchTags", "Tracks" };

            for (int i = 0; i < idlist.Length; i++)
            {
                if (idlist[i] == id)
                {
                    string loc = reslist[i];
                    return StringResource.Get(loc);
                }
            }

            return id;
        }

        public async Task GetSecureStreamAsync(Stream stream, string uri)
        {
            await InitializeAsync();
            throw new NotImplementedException();
        }

        public async Task<MusicServiceResult> SearchItemsAsync(Player player, string category, string what, int startIndex, int maxcount)
        {
            string soap = "<search xmlns=\"" + ServiceNamespace + "\">" +
            "<id>" + SecurityElement.Escape(category) + "</id>" +
            "<term>" + SecurityElement.Escape(what) + "</term>" +
            "<index>" + startIndex.ToString() + "</index>" +
            "<count>" + maxcount.ToString() + "</count>" +
            "</search>";

            return await GeneralItemsAsync(player, soap, "search", what);
        }

        public Task<string> SendFeedback(Player player, string station, string track, bool like)
        {
            throw new NotImplementedException();
        }

        internal static void Update(MusicService existing, object user, object key, object token, object serial, string newlogo)
        {
            throw new NotImplementedException();
        }

        private async Task<XElement> SOAPRequestAsync(Uri url, IEnumerable<string> header, string body, string function, string servicetype, string languages, string userAgent, string name, Action<HttpClient> clientAdder)
        {
            StringBuilder sb = new StringBuilder("<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                "<s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\">"
            );
            if (header != null)
            {
                // The way one header tag wraps all the headers is contrary to the spec, but required for Pandora to parse it!
                sb.Append("<s:Header>");
                foreach (var h in header)
                {
                    sb.Append(h);
                }
                sb.Append("</s:Header>");
            }
            sb.Append(
            "<s:Body>" +
            body +
            "</s:Body>" +
            "</s:Envelope>");

            // A few services (eg Deezer) require a certain version, so lets pretend to be Sonos
            HttpClient client = UPnP.Service.CreateClient(userAgent);
            client.BaseAddress = url;
            // This is CRITICAL for Rhapsody 2.0
            client.DefaultRequestHeaders.ExpectContinue = false;
            var r = new HttpRequestMessage();
            r.Method = HttpMethod.Post;
            byte[] b = Encoding.UTF8.GetBytes(sb.ToString());
            r.Content = new StringContent(sb.ToString(), Encoding.UTF8, "text/xml");
            r.Content.Headers.Add("SOAPACTION", "\"" + servicetype + "#" + function + "\"");
            // "Content-Length" is set for us
            r.RequestUri = url;
            if (languages != null)
            {
                // r.Content.Headers.Add cannot be used for this (?)
                client.DefaultRequestHeaders.Add("ACCEPT-LANGUAGE", languages);
            }

            client.DefaultRequestHeaders.Add("X-Sonos-Corr-Id", Guid.NewGuid().ToString());
            client.DefaultRequestHeaders.Add("X-Sonos-Controller-ID", ControllerId);
            clientAdder?.Invoke(client);

            var resp = await client.SendAsync(r);
            if (resp.Content.Headers.ContentType?.MediaType == "application/json")
            {
                // Use this for debugging and bad responses (eg YouTube music) only
                // var raw = await resp.Content.ReadAsStringAsync();
                throw new Exception("Music service not currently compatible");
            }
            else
            {
                var raw = await resp.Content.ReadAsStreamAsync();
                return SafeParseXml(raw);
            }
        }

        // This is to handle the "xsi:" namespace in Sonos Radio responses
        private static XElement SafeParseXml(Stream stream)
        {
            var settings = new XmlReaderSettings() { NameTable = new NameTable(), CloseInput = true };
            var xmlns = new XmlNamespaceManager(settings.NameTable);
            xmlns.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");
            XmlParserContext context = new XmlParserContext(null, xmlns, "", XmlSpace.Default);
            XmlReader reader = XmlReader.Create(stream, settings, context);
            return XElement.Load(reader);
        }

        private async Task VerifyAvailableAsync(Player player)
        {
            if (IsAnonymous || IsRadio)
                return;

            if (IsUserId)
            {
                if (SessionId != null)
                    return;
                var result = await player.MusicServices.GetSessionId((uint)Description.XmlId, UserName);
                result.ThrowIfFailed();
                SessionId = result.SessionId;
                if (SessionId == null)
                    throw result.Error;
                return;                                 // we have a new session id
            }

            if (NeedFreshToken)
            {
                // Get some fresh creds
                // We need both X-headers and credentialheader with tok/key
                string soap = "<refreshAuthToken xmlns=\"" + ServiceNamespace + "\">" + "</refreshAuthToken>";

                var result = await DoRequestAsync(true, "refreshAuthToken", CredentialHeader(player, true), soap, ContextHeader());

                var newtok = (string)result.Descendants(XName.Get("authToken")).FirstOrDefault();
                if (newtok == null)
                    newtok = (string)result.Descendants(XName.Get("authToken", ServiceNamespace)).FirstOrDefault();
                var newkey = (string)result.Descendants(XName.Get("privateKey")).FirstOrDefault();
                if (newkey == null)
                    newkey = (string)result.Descendants(XName.Get("privateKey", ServiceNamespace)).FirstOrDefault();
                if (newtok != null && newkey != null)
                {
                    Token = newtok;
                    AccountKey = newkey;
                    NeedFreshToken = false;
                }
            }

            if (IsDeviceLink)
            {
                // We normally rely on the Token from the Event
                if (AccountKey != null)
                    return;

                throw new Exception(string.Format(StringResource.Get("Reauth_Official"), this.DisplayName));
            }

            throw new Exception("Unknown authorization method");
        }

        private JsonDocument ItemsAsJson;
        private bool ManifestLoaded;
        private string RootJsonUri;
        private string ApiKey;
        private string RadioUrl;

        private async Task InitializeManifestAsync()
        {
            if (
                this.Description.ManifestUri == null ||
                (((CapabilityFlag)Description.Capabilities & CapabilityFlag.SupportManifestFile) == 0) ||
                ManifestLoaded)
            {
                return;
            }

            using (var manifest = await ReadJsonFileAsync(this.Description.ManifestUri, authenticate: false))
            {
                if (manifest == null)
                {
                    return;
                }
                var doc = manifest.RootElement;

                if (Description.StringsUri == null)
                {
                    var strings = doc.GetProperty("strings");
                    Description.StringsUri = strings.GetProperty("uri").GetString();
                }

                if (Description.PresentationUri == null)
                {
                    if (doc.TryGetProperty("presentationMap", out var pmap))
                    {
                        Description.PresentationUri = pmap.GetProperty("uri").GetString();
                    }
                }

                if (doc.TryGetProperty("endpoints", out var endpoints))
                {
                    foreach (var endpoint in endpoints.EnumerateArray())
                    {
                        if (endpoint.TryGetProperty("type", out var prop))
                        {
                            if (prop.GetString() == "radio")
                            {
                                RadioUrl = endpoint.GetProperty("uri").GetString();
                            }
                            else if (prop.GetString() == "browse")
                            {
                                RootJsonUri = endpoint.GetProperty("uri").GetString();
                            }
                        }
                    }
                }

                if (ApiKey == null && doc.TryGetProperty("apiKey", out var apiKey) && apiKey.TryGetProperty("cr", out var key))
                {
                    ApiKey = key.GetString();
                }

                ManifestLoaded = true;
            }
        }

        private async Task<MusicServiceResult> GetRootItemsByJsonAsync(Player player)
        {
            await InitializeAsync();

            if (ManifestLoaded && ItemsAsJson == null && RootJsonUri != null)
            {
                for (int i = 0; i < 2; i++)
                {
                    await VerifyAvailableAsync(player);

                    try
                    {
                        ItemsAsJson = await ReadJsonFileAsync(RootJsonUri, authenticate: true);
                        if (ItemsAsJson != null)
                            break;
                    }
                    catch (RetryException)
                    {
                        NeedFreshToken = true;
                        Debug.WriteLine("Will retry json on {0}", (object)this.DisplayName);
                    }
                }
            }

            if (ItemsAsJson == null)
            {
                return null;
            }

            DisplayData rootDisplayType = null;
            int max = (int)ItemsAsJson.RootElement.GetProperty("total").GetInt32();
            var views = ItemsAsJson.RootElement.GetProperty("views");
            var converted = new List<MusicServiceItem>();
            if (ItemsAsJson.RootElement.TryGetProperty("displayType", out var dp))
            {
                rootDisplayType = new DisplayData(id: null, dp.GetString());
            }
            foreach (var view in views.EnumerateArray())
            {
                var v = view;
                string id = v.GetProperty("id").GetProperty("objectId").GetString();
                string name = v.GetProperty("content").GetProperty("container").GetProperty("name").GetString();
                bool canEnum = v.GetProperty("content").GetProperty("container").GetProperty("type").GetString() == "container";
                if (!canEnum && v.TryGetProperty("items", out var vitems) && vitems.GetArrayLength() != 0)
                {
                    // for Apple Music / Library
                    canEnum = true;
                }
                var data = new MusicServiceItem(id, name, string.Empty, false, false, canEnum, false, false, false, string.Empty, string.Empty, string.Empty, this);
                data.ParentDisplayData = rootDisplayType;

                if (v.TryGetProperty("displayType", out var d))
                {
                    if (DisplayModesById.TryGetValue(d.GetString(), out DisplayData dd))
                    {
                        data.DisplayData = dd;
#if DEBUG
                        if (canEnum && !string.IsNullOrEmpty(data.Title))
                        {
                            // TODO data.Title += " (" + dd.Summary() + ")";
                        }
#endif
                    }
                }
                if (v.TryGetProperty("items", out var items))
                {
                    try
                    {
                        var itemArray = items.EnumerateArray();
                        List<string> uris = new List<string>();
                        for (int i = 0; i < itemArray.Count(); i++)
                        {
                            var x = itemArray.ElementAt(i);
                            var content = x.GetProperty("content");
                            string uri;
                            if (content.TryGetProperty("container", out var container) || content.TryGetProperty("track", out container))
                            {
                                uri = container.GetProperty("imageUrl").GetString();
                            }
                            else
                            {
                                uri = content.GetProperty("imageUrl").GetString();
                            }
                            uris.Add(uri);
                        }
                        
                        data.InlineChildren = uris;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("{0} reading json for {1}", ex.Message, this.DisplayName);
                    }
                }
                converted.Add(data);
            }
            var result = new MusicServiceResult(converted, max);
            return result;
        }

        // from http://musicpartners.sonos.com/node/111
        private static string[] SupportedLocales = { "en-US", "da-DK", "de-DE", "es-ES", "fr-FR", "it-IT", "ja-JP", "nb-NO", "nl-NL", "pt-BR", "sv-SE", "zh-CN" };
        private static string DefaultLanguage = "en-US";

        private static string GetSupportedLocale()
        {
            string lang = CultureInfo.CurrentCulture.IetfLanguageTag;
            if (SupportedLocales.Contains(lang))
            {
                return lang;
            }

            if (lang.Length > 3 && lang[2] == '-')
            {
                string first = lang.Substring(0, 2);
                string similar = SupportedLocales.FirstOrDefault(c => c.Substring(0, 2) == first);
                if (similar != null)
                {
                    return similar;
                }
            }

            return DefaultLanguage;
        }

        private static string GetLanguage()
        {
            // (Amazon/SoundCloud choke on unsupported values, so force US)
            string lang = GetSupportedLocale();

            if (lang != DefaultLanguage)
            {
                // If not USA ensure that is always there
                lang = lang + ", en-US;q=0.9";
            }

            return lang;
        }

        protected async Task<XElement> DoRequestAsync(bool secure, string action, string header, string soap, string secondHeader)
        {
            // this is for Radio Paradise (possibly an S2 requirement now?)
            if (RadioUrl != null)
                secure = true;

            Uri uri = new Uri(secure ? this.Description.SecureUri : this.Description.Uri);

            var lang = GetLanguage();
            var headers = new List<string>();
            headers.Add(header);
            if (secondHeader != null)
                headers.Add(secondHeader);

            string userAgent = MusicServiceProvider.SonosUserAgent;

            var xml = await SOAPRequestAsync(uri, headers, soap, action, ServiceNamespace, lang, userAgent, RawName, (client) =>
            {
                if (GoogleApi && GoogleApiKey != null && SonosApiKey != null)
                {
                    client.DefaultRequestHeaders.Add("X-Goog-Api-Key", GoogleApiKey);
                    client.DefaultRequestHeaders.Add("X-Sonos-Api-Key", SonosApiKey);
                    if (!NeedFreshToken)
                    {
                        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token);
                    }
                }
            }
            );

            var faultcode = xml.Descendants(XName.Get("faultcode")).FirstOrDefault();
            if (faultcode != null)
            {
#if false // TODO
                NetLogger.Empty();
                NetLogger.WriteLine(uri.ToString());
                NetLogger.WriteLine(soap);
                NetLogger.WriteLine(xml.ToString());
#endif
                await HandleFault(faultcode.Value, xml);
            }

            return xml;
        }

        private async Task HandleFault(string faultcode, XElement fault)
        {
            // Faultcode often has bogus "s:" or similar on the front
            if (faultcode.EndsWith("Client.SessionIdInvalid"))
            {
                SessionId = null;
                throw new RetryException(this.DisplayName, "SessionIdInvalid");
            }
            else if (faultcode.EndsWith("Client.TokenRefreshRequired"))
            {
                var newtok = fault.Descendants(XName.Get("authToken")).FirstOrDefault();
                if (newtok == null)
                    newtok = fault.Descendants(XName.Get("authToken", ServiceNamespace)).FirstOrDefault();
                var newkey = fault.Descendants(XName.Get("privateKey")).FirstOrDefault();
                if (newkey == null)
                    newkey = fault.Descendants(XName.Get("privateKey", ServiceNamespace)).FirstOrDefault();
                if (newtok != null && newkey != null && newtok.Value != null && newkey.Value != null)
                {
                    Token = newtok.Value;
                    AccountKey = newkey.Value;
                    throw new RetryException(this.DisplayName, "TokenRefreshSuccessful");
                }
            }
            else if (faultcode.EndsWith("Client.AuthTokenExpired"))
            {
                throw new Exception(string.Format(StringResource.Get("Reauth_Official"), this.DisplayName));
            }
            else if (faultcode.EndsWith("Server.ServiceUnknownError"))
            {
                string err = (string)fault.Descendants(XName.Get("SonosError")).FirstOrDefault();
                if (err == null)
                {
                    err = (string)fault.Descendants(XName.Get("faultstring")).FirstOrDefault();
                    if (err != null)
                        throw new Exception(err);
                    err = "1";
                }
                // If we can't localize it, use the default (eg for Google Music)
                var error = LookupLocalizedString("Error" + err + "Message", null);
                if (error != null)
                    throw new Exception(error);
            }
            else if (faultcode == "s:Server")
            {
                if (this.Description.Name == "Spotify")
                {
                    // Handle weird Spotify issue
                    string err = (string)fault.Descendants(XName.Get("SonosError")).FirstOrDefault();
                    if (err == "25")
                    {
                        // "Update your Sonos system"
                        throw new RetryException(this.DisplayName, "BogusUpgrade");
                    }
                }
            }
            else if (faultcode == "soap:Client" && this.Description.Name == "Google Play Music")
            {
                // Google isnt compatible (client-cert?) so say so
                throw new Exception(string.Format("{0} is not currently compatible", this.Description.Name));
            }

            // Nothing we recognized to report it directly
            var faultstring = fault.Descendants(XName.Get("ExceptionInfo")).FirstOrDefault();
            if (faultstring == null || string.IsNullOrEmpty(faultstring.Value))
                faultstring = fault.Descendants(XName.Get("faultstring")).FirstOrDefault();
            var errorCode = fault.Descendants(XName.Get("SonosError")).FirstOrDefault();
            int code = -1;
            if (errorCode != null)
            {
                int.TryParse(errorCode.Value, out code);
            }

            if (faultstring != null)
                throw new MusicServiceException(faultstring.Value, code);
            else
                throw new MusicServiceException(faultcode, code);
        }

        private class MusicServiceException : Exception
        {
            public int SonosError { get; }
            public MusicServiceException(string msg, int code) : base(msg)
            {
                SonosError = code;
            }
        }

        private static string FixXmlVersion(string xml)
        {
            // Handle bizarre Amazon presentation map version
            if (xml.StartsWith("<?xml version=\"2.0\""))
                xml = "<?xml version=\"1.0\"" + xml.Substring(19);

            return xml;
        }

        private async Task<JsonDocument> ReadJsonFileAsync(string uri, bool authenticate)
        {
            var client = UPnP.Service.CreateClient(MusicServiceProvider.SonosUserAgent131, allowRedirects: true, timeout: Service.ImageLoaderTimeout * 2);

            try
            {
                if (NeedsContext)
                {
                    client.DefaultRequestHeaders.Add("X-Sonos-Context-TimeZone", GetTimeZone());
                }
                if (authenticate && AccountKey != null)
                {
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token);
                }
                client.DefaultRequestHeaders.Add("X-Sonos-Corr-Id", Guid.NewGuid().ToString());
                client.DefaultRequestHeaders.Add("X-Sonos-Controller-ID", ControllerId);
                client.DefaultRequestHeaders.Add("X-Sonos-Device-Id", HouseholdIdX());       // should include the _stuff if available
                client.DefaultRequestHeaders.Add("Accept-Language", GetLanguage());
                client.DefaultRequestHeaders.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue() { NoCache = true, NoStore = true };
                client.DefaultRequestHeaders.ConnectionClose = false;
                var content = await client.GetStringAsync(uri);
                return JsonDocument.Parse(content);
            }
            catch (HttpRequestException ex)
            {
                // Terrible way of detecting auth errors but as good as .NET FX permits
                if (authenticate && ex.Message.Contains("401"))
                {
                    NeedFreshToken = true;
                    throw new RetryException(this.DisplayName, "Json401");
                }
                else
                {
                    Report(ex);
                    return null;
                }
            }
            catch (Exception ex)
            {
                Report(ex);
                return null;
            }

            void Report(Exception ex)
            {
                var diags = new Dictionary<string, string>();
                diags["service"] = this.Description.Name;
                diags["url"] = uri;
                diags["exception"] = ex.Message;
                diags["type"] = ex.GetType().Name;
                NetLogger.WriteLine("Json error: {0} {1}", uri, ex.Message);
            }
        }

        private static string ExtractArt(XElement item)
        {
            var art = (string)item.Descendants(XName.Get("trackMetadata", ServiceNamespace)).Elements(XName.Get("albumArtURI", ServiceNamespace)).FirstOrDefault();
            if (art == null)
                art = (string)item.Descendants(XName.Get("streamMetadata", ServiceNamespace)).Elements(XName.Get("logo", ServiceNamespace)).FirstOrDefault();
            return art;
        }

        private bool ExtractExplicit(XElement item)
        {
            var e = (string)item.Descendants(XName.Get("tags", ServiceNamespace)).Descendants(XName.Get("explicit", ServiceNamespace)).FirstOrDefault();
            return e == "1";
        }

        // from https://developer.sonos.com/build/content-service-add-features/customize-display/configure-display-types/ plus podcast-specifics
        static string[] InterestingAttributes = { "title", "artist", "album", "summary", "genre", "itemType", "releaseDate", "duration", "semanticType" };

        private static Dictionary<string, string> ExtractAttributes(XElement item, XElement trackMD = null)
        {
            Dictionary<string, string> results = null;

            var itemAttrs = item.Elements().Where(e => InterestingAttributes.Contains(e.Name.LocalName));
            if (trackMD != null)
            {
                itemAttrs = itemAttrs.Concat(trackMD.Elements().Where(e => InterestingAttributes.Contains(e.Name.LocalName)));
            }

            if (itemAttrs.Count() != 0)
            {
                results = itemAttrs.ToDictionary(i => i.Name.LocalName, j => j.Value);
            }

            if (trackMD != null)
            {
                if (results == null)
                {
                    results = new Dictionary<string, string>();
                }
                results.Add("playshuffle", "true");
            }

            return results;
        }
        private DisplayData ExtractDisplayType(XElement item)
        {
            string type;
            DisplayData t;

            type = (string)item.Element(XName.Get("displayType", ServiceNamespace));
            if (type != null && DisplayModesById.TryGetValue(type, out t))
            {
                return t;
            }

            type = (string)item.Element(XName.Get("itemType", ServiceNamespace));
            if (type != null && DisplayModesByType.TryGetValue(type, out t))
            {
                return t;
            }

            return null;
        }

        private static string ExtractSubtitle(XElement item)
        {
            if ((string)item.Element(XName.Get("itemType", ServiceNamespace)) == "show")
            {
                string date = (string)item.Element(XName.Get("releaseDate", ServiceNamespace));
                if (DateTime.TryParse(date, out DateTime utc))
                {
                    return utc.ToShortDateString();
                }
                else
                {
                    return string.Empty;
                }
            }
            else
            {
                return (string)item.Descendants(XName.Get("trackMetadata", ServiceNamespace)).Elements(XName.Get("artist", ServiceNamespace)).FirstOrDefault();
            }
        }

        static private bool BoolParse(string b, bool def)
        {
            bool result = def;
            if (b != null)
                bool.TryParse(b, out result);
            return result;
        }

        private Dictionary<int, string> ArtSubstitutions;

        public Dictionary<int, string> GetArtSubstitions()
        {
            if (ArtSubstitutions == null && this.Description.PresentationMap != null)
            {
                var map = from s
                               in this.Description.PresentationMap.Descendants(XName.Get("PresentationMap"))
                          where (string)s.Attribute(XName.Get("type")) == "ArtWorkSizeMap"
                          select s;

                var items = from i in map.Descendants(XName.Get("sizeEntry"))
                            select new
                            {
                                org = int.Parse((string)i.Attribute(XName.Get("size"))),
                                sub = (string)i.Attribute(XName.Get("substitution"))
                            };

                ArtSubstitutions = new Dictionary<int, string>();
                foreach (var i in items)
                {
                    ArtSubstitutions[i.org] = i.sub;
                }
            }

            return ArtSubstitutions;
        }

        public async Task<MusicItemDetail> GetExtendedMetadataAsync(string id, Player player)
        {
            string soap = "<getExtendedMetadata xmlns=\"" + ServiceNamespace + "\">" +
            "<id>" + SecurityElement.Escape(id) + "</id>" +
            "</getExtendedMetadata>";

            for (int i = 0; i < 2; i++)
            {
                try
                {
                    var result = await DoRequestAsync(false, "getExtendedMetadata", CredentialHeader(player, true), soap, null);
                    string art = (string)result.Descendants(XName.Get("albumArtURI", ServiceNamespace)).FirstOrDefault();
                    var @explicit = result.Descendants(XName.Get("tags", ServiceNamespace)).Descendants(XName.Get("explicit", ServiceNamespace)).FirstOrDefault();
                    var md = new MusicItemDetail(art, ((string)@explicit) == "1", this, id);
                    var dyn = result.Descendants(XName.Get("dynamic", ServiceNamespace)).FirstOrDefault();
                    if (dyn != null)
                    {
                        var props = dyn.Descendants(XName.Get("property", ServiceNamespace)).
                            Select(p => new KeyValuePair<string, string>((string)p.Descendants(XName.Get("name", ServiceNamespace)).FirstOrDefault(),
                                (string)p.Descendants(XName.Get("value", ServiceNamespace)).FirstOrDefault()));
                        foreach (var prop in props)
                        {
                            md.Properties[prop.Key] = prop.Value;
                        }
                    }
                    return md;
                }
                catch (RetryException)
                {
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{ex.Message} getting ExtendedMetadata for {id} from {this.RawName}");
                    break;
                }
            }

            return null;
        }
    }
}
