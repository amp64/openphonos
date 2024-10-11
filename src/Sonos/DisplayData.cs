using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text.Json;

//#define DISPLAY_DEBUG

namespace OpenPhonos.Sonos
{
    /// <summary>
    /// Immutable object containing everything needed to display collections
    /// </summary>
    public class DisplayData
    {
        public string DisplayMode { get; set; }

        private readonly string[] Lines;
        private readonly string[] Header;
        private readonly string Id;             // debugging purposes only
        private readonly string Thumbnails;

        private static string[] DefaultHeaderItems = { "title", "artist", "summary" };

        // Docs says the default is title/artist but seems to really be title only
        private static DisplayData DefaultForAlbum = new DisplayData(id: "ADefault", "HERO", "trackNumber", new string[] { "title" });

        // "albumList" needs a default (not in the docs)
        private static DisplayData DefaultForAlbumList = new DisplayData(id: "ALDefault", "GRID");

        public DisplayData(string id, string mode, string thumbs = null, IEnumerable<string> lines = null, IEnumerable<string> headers = null)
        {
            Id = id;
            DisplayMode = mode;
            Thumbnails = thumbs;

            if (thumbs == null && mode == "HERO_EDITORIAL")
            {
                Thumbnails = "none";
            }

            if (lines != null && lines.Count() > 0)
            {
                Lines = lines.ToArray();
            }
            if (headers != null && headers.Count() > 0)
            {
                Header = headers.ToArray();
            }
        }

        public bool HeaderVisible
        {
            get
            {
                bool header = DisplayMode == "HERO" || DisplayMode == "HERO_EDITORIAL" || (DisplayMode == null && Lines != null);
                return header;
            }
        }

        public string ItemThumbnails { get => Thumbnails; }

        public static string HeaderItem(MusicItem item, int index)
        {
            if (item == null)
                return null;

            string name;

            if (item.DisplayData != null && item.DisplayData.Header != null)
            {
                if (item.DisplayData.Header.Count() > index)
                    name = item.DisplayData.Header[index];
                else
                    name = null;
            }
            else
            {
                name = DefaultHeaderItems[index];
            }

            if (name != null)
            {
                name = FormatLine(item, name);
            }

            return name;
        }

        public static string LineItem(MusicItem item, int index)
        {
            if (item == null)
                return null;

            string name = null;
            var display = item.DisplayData;
            // dont use default if parent is specified
            if (display == null || (display == DefaultForAlbum && item.ParentDisplayData != null))
            {
                display = item.ParentDisplayData;
            }

            if (display?.Lines != null)
            {
                if (display.Lines?.Count() > index)
                {
                    name = display.Lines[index];
                    name = FormatLine(item, name);
#if DISPLAY_DEBUG
                    if (!string.IsNullOrEmpty(name))
                    {
                        name += "-(" + display.Lines[index] + ")";
                    }
#endif
                }
                else
                {
                    name = null;
                }
            }
            else
            {
                // no Lines specified, use the default of title/artist
                switch (index)
                {
                    case 0:
                        if (item.Attributes == null || !item.Attributes.TryGetValue("title", out name))
                            name = item.Title;
                        break;
                    case 1:
                        if (item.Attributes == null || !item.Attributes.TryGetValue("artist", out name))
                            name = item.Subtitle;
                        break;
                    default:
                        name = null;
                        break;
                }
            }

            return name;
        }

        private static string SafeAttribute(MusicItem item, string name)
        {
            if (item.Attributes == null || !item.Attributes.TryGetValue(name, out string value))
            {
                return string.Empty;
            }
            else
            {
                return value;
            }
        }

        private static string FormatLine(MusicItem item, string name)
        {
            if (item.Attributes != null && name != null)
            {
                if (item.Attributes.TryGetValue(name, out string result))
                {
                    return result;
                }

                if (name == "artist" && SafeAttribute(item, "itemType") == "show")
                {
                    // its a podcast with "artist" in the template (eg BBC Sounds), which we have to make up out of parts
                    try
                    {
                        name = DateTime.Parse(item.Attributes["releaseDate"]).ToShortDateString();
                        if (item.Attributes.ContainsKey("duration"))
                        {
                            name = string.Format("{0} - {1} min", name, uint.Parse(item.Attributes["duration"]) / 60);
                        }
                        return name;
                    }
                    catch (Exception ex)
                    {
                        /*Analytics.TrackEvent("DisplayData.ShowEx", item.Attributes);*/
                        return null;
                    }
                }

                if (name.Contains('{') && name.Contains('}'))
                {
                    name = name.Replace("{artist}", "{0}").Replace("{title}", "{1}").Replace("{summary}", "{2}".Replace("{album}", "{3}").Replace("{genre}", "{4}"));
                    try
                    {
                        return string.Format(name, SafeAttribute(item, "artist"), SafeAttribute(item, "title"), SafeAttribute(item, "summary"), SafeAttribute(item, "album"), SafeAttribute(item, "genre"));
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("{0} while parsing {1}", ex.Message, name);
                        return "BUG!";
                    }
                }

                return null;
            }

            switch (name)
            {
                case "title":
                    return item.Title;
                case "artist":
                    return item.Subtitle;
                case null:
                    return null;
                case "summary":
                    return SafeAttribute(item, "summary");
                default:
                    if (name.Contains('{') && name.Contains('}'))
                    {
                        name = name.Replace("{artist}", "{0}").Replace("{title}", "{1}").Replace("{summary}", "{2}");
                        try
                        {
                            return string.Format(name, item.Subtitle, item.Title, "summary");
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine("{0} while parsing {1}", ex.Message, name);
                            return "BUG!";
                        }
                    }
                    else
                    {
                        return "BUG:" + name;
                    }
            }
        }

        public override string ToString()
        {
            return Summary();
        }

        private class DisplayDataJson
        {
            public string d { get; set; }
            public string t { get; set; }
            public List<string> l { get; set; }
            public List<string> h { get; set; }
        }

        public string AsString()
        {
            var o = new DisplayDataJson();
            o.d = DisplayMode ?? string.Empty;
            o.d = Thumbnails ?? string.Empty;
            if (Lines?.Count() > 0)
            {
                o.l = Lines.ToList();
            }
            if (Header?.Count() > 0)
            {
                o.h = Header.ToList();
            }

            return JsonSerializer.Serialize<DisplayDataJson>(o);
        }

        public static DisplayData FromString(string val)
        {
            var json = JsonSerializer.Deserialize<DisplayDataJson>(val);
            return new DisplayData(id: null, json.d, json.t, json.l, json.h);
        }

        public string Summary()
        {
            List<string> result = new List<string>();
            if (!string.IsNullOrEmpty(Id))
                result.Add(Id);

            if (!string.IsNullOrEmpty(DisplayMode))
                result.Add(DisplayMode);
            else
                result.Add("default");

            if (!string.IsNullOrEmpty(ItemThumbnails))
                result.Add(ItemThumbnails);

            if (Lines != null)
            {
                result.Add("L" + Lines.Count().ToString());
            }
            if (Header != null)
            {
                result.Add("H" + Header.Count().ToString());
            }

            return string.Join(",", result);
        }

        internal static DisplayData FromXml(string id, XElement displayType, Func<string,string> translator)
        {
            string mode = (string)displayType.Descendants(XName.Get("DisplayMode")).FirstOrDefault();

            var lines = displayType.Descendants(XName.Get("Lines")).Descendants(XName.Get("Line"));
            var content = LinesFromXml(lines, translator);

            var header = displayType.Descendants(XName.Get("Header")).Descendants(XName.Get("Line"));
            var headers = LinesFromXml(header, translator);

            var thumbs = (string)displayType.Descendants(XName.Get("ItemThumbnails")).Attributes(XName.Get("source")).FirstOrDefault();

            return new DisplayData(id, mode, thumbs, content, headers);
        }

        private static IEnumerable<string> LinesFromXml(IEnumerable<XElement> lines, Func<string, string> translator)
        {
            return lines.Select(l =>
            {
                string val = (string)l.Attribute("token");
                if (val == null)
                {
                    val = (string)l.Attribute("stringId");
                    if (val != null)
                    {
                        val = translator(val);
                    }
                }

                return val;
            });
        }

        internal static void AddMusicServiceDefaults(Dictionary<string, DisplayData> displayModesByType)
        {
            if (displayModesByType.TryGetValue("album", out var albumDisplay))
            {
                displayModesByType["album"] = FixupAlbumData(albumDisplay);
            }
            else
            {
                displayModesByType["album"] = DefaultForAlbum;
            }

            if (!displayModesByType.ContainsKey("albumList"))
            {
                displayModesByType["albumList"] = DefaultForAlbumList;
            }
        }

        private static DisplayData FixupAlbumData(DisplayData original)
        {
            bool changed = false;

            string thumbs = original.ItemThumbnails;
            if (thumbs == null)
            {
                thumbs = "trackNumber";
                changed = true;
            }

            string mode = original.DisplayMode;
            if (mode == null)
            {
                mode = "HERO";
                changed = true;
            }

            if (changed)
            {
                return new DisplayData(original.Id, mode, thumbs, original.Lines, original.Header);
            }
            else
            {
                return original;
            }
        }

        // Note: UPNP Albums sometimes have no track numbers
        private static DisplayData DefaultForUPNPAlbum = new DisplayData(id: "UDefault", "HERO", "trackNumber", null, new string[] { "title", "artist" });

        // Tracks from UPNP Albums can skip the Artist name
        private static DisplayData DefaultForUPNPAlbumTracks = new DisplayData(id: "UTDefault", "LIST", "trackNumber", new string[] { "title" });

        // Tracks from Artists show art, title and album name (which is summary)
        private static DisplayData DefaultForUPNPArtistTracks = new DisplayData(id: "UPDefault", "LIST", "albumArtUri", new string[] { "title", "summary" }, null);

        private static int CountOf(string val, char what)
        {
            int found = val.IndexOf(what);
            if (found < 0)
            {
                return 0;
            }
            
            int count = 1;
            for (int i = found + 1; i < val.Length; i++)
            {
                if (val[i] == what)
                {
                    count++;
                }
            }

            return count;
        }

        // How should childen of this item be displayed?
        internal static DisplayData ParentModeFromUPnPPath(string dataPath)
        {
            int slashes = CountOf(dataPath, '/');

            if (dataPath.StartsWith("A:ALBUM/"))
                return DefaultForUPNPAlbum;

            if (
                (dataPath.StartsWith("A:ALBUMARTIST/") && slashes >= 2) ||
                (dataPath.StartsWith("A:COMPOSER/") && slashes >= 2) ||
                (dataPath.StartsWith("A:GENRE/") && slashes >= 3)
               )
            {
                if (dataPath.EndsWith("/"))
                {
                    // Artist / All
                    return DefaultForUPNPArtistTracks;
                }
                else
                {
                    // Artist / Album
                    return DefaultForUPNPAlbum;
                }
            }
            else if (
                dataPath == "A:ALBUM" || 
                dataPath == "FV:2" || 
                dataPath.StartsWith("A:ALBUMARTIST/") || 
                (dataPath.StartsWith("A:GENRE/") && slashes == 2) ||
                (dataPath.StartsWith("A:COMPOSER") && slashes == 1)
               )
                return DefaultForAlbumList;

            return null;
        }

        // Does this parent item deliver tracks from the same artist?
        private static bool DeliversTracksFromSingleArtist(string dataPath)
        {
            return
                dataPath.StartsWith("A:ALBUM/") ||
                (dataPath.StartsWith("A:ALBUMARTIST/") && !dataPath.EndsWith("/"));
        }

        // Does this parent item allow play/shuffle on its items? (Only required for items without individual DisplayData)
        private static bool AllowUPNPPlayShuffle(UPnPMusicItem item)
        {
            string id = item.Id;

            int slashes = CountOf(id, '/');

            // Artist / All needs this
            return
                id.StartsWith("SQ:") ||
                id.StartsWith("S:") ||
                (id.StartsWith("A:ALBUMARTIST/") && id.EndsWith("/") && slashes == 2) ||
                (id.StartsWith("A:COMPOSER/") && id.EndsWith("/") && slashes == 2) ||
                (id.StartsWith("A:GENRE/") && id.EndsWith("/") && slashes == 3);
        }

        private static DisplayData FromUPnPClass(string name)
        {
            if (name == "object.container.album.musicAlbum")
                return DefaultForUPNPAlbum;

            return null;
        }

        // Return true if this item should make its parent potentially be shown with Play/Shuffle buttons
        internal static bool SetUPNP(string parentPath, DidlData item, MusicItem genericitem)
        {
            genericitem.TrackNumber = item.TrackNumber;
            genericitem.DisplayData = DisplayData.FromUPnPClass(item.Class);

            bool isTrack = item.Class == "object.item.audioItem.musicTrack";

            if (genericitem.DisplayData == null && isTrack)
            {
                if (DeliversTracksFromSingleArtist(parentPath))
                {
                    // these are album tracks from the same artist so just display the name, not the artist
                    genericitem.DisplayData = DefaultForUPNPAlbumTracks;
                }
            }

            // If we have some DisplayData (or we are a track), we will need some Attributes as well
            if (genericitem.DisplayData != null || isTrack)
            {
                genericitem.Attributes = new Dictionary<string, string>()
                {
                    ["class"] = item.Class,
                    ["title"] = genericitem.Title,
                    ["artist"] = item.Creator,
                    ["summary"] = item.Album,
                };
            }

            return isTrack;
        }
    }
}
