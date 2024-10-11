using OpenPhonos.UPnP;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;

namespace OpenPhonos.Sonos
{
    public class DidlData
    {
        public string Title;
        public string Id;
        public string Res;
        public string AlbumArtURI;
        public string Class;
        public string Creator;
        public string Album;
        public string Stream;
        public string RadioShow;
        public string Metadata;
        public string TrackNumber;      // TODO add code (maybe also ProtocolInfo?)
        public bool IsContainer;
        public bool IsPlayable;
        public bool IsQueueable;
        public bool IsDeletable;
        public bool IsFavoritable;
        public bool IsAlarmable;

        const string DC = "http://purl.org/dc/elements/1.1/";
        const string NS = "urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/";
        const string UPNP = "urn:schemas-upnp-org:metadata-1-0/upnp/";
        const string R = "urn:schemas-rinconnetworks-com:metadata-1-0/";

        // ref: http://blogs.msdn.com/b/ericwhite/archive/2009/05/14/working-with-optional-elements-and-attributes-in-linq-to-xml-queries.aspx

        public static IEnumerable<DidlData> Parse(string xml)
        {
            try
            {
                XElement xelem = XElement.Parse(xml);

                var elems = from item in xelem.Descendants()
                            where item.Name == XName.Get("container", NS) || item.Name == XName.Get("item", NS)
                            select new DidlData(item);

                return elems;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception {0} during DidlParse", ex.Message);
                Debug.Assert(false);
                return new DidlData[1];
            }
        }

        public DidlData()
        {
        }

        private DidlData(XElement item)
        {
            Id = item.Attribute(XName.Get("id")).Value;
            Title = (string)item.Element(XName.Get("title", DC));
            Res = (string)item.Element(XName.Get("res", NS));
            AlbumArtURI = (string)item.Element(XName.Get("albumArtURI", UPNP));
            Class = (string)item.Element(XName.Get("class", UPNP));
            Creator = (string)item.Element(XName.Get("creator", DC));
            Album = (string)item.Element(XName.Get("album", UPNP));
            Stream = ZPConvert((string)item.Element(XName.Get("streamContent", R)));
            RadioShow = RadioShowName((string)item.Element(XName.Get("radioShowMd", R)));
            IsContainer = item.Name == XName.Get("container", NS);
            IsFavoritable = true;
            IsAlarmable = false;

            if (Class.StartsWith("object.item.audioItem.audioBroadcast"))
            {
                Metadata = item.ToString();
                IsAlarmable = true;
            }
            else if (Class == "object.itemobject.item.sonos-favorite")
            {
                // Favorites are very different
                Album = (string)item.Element(XName.Get("description", R));
                Metadata = item.Element(XName.Get("resMD", R)).Value;

                // Recurse to get "real" MD
                var innerdidl = Parse(Metadata).First();
                Class = innerdidl.Class;
                IsContainer = Class.StartsWith("object.container");
                IsFavoritable = false;
                IsDeletable = true;
            }
            else if (Class == "object.container.playlistContainer")
            {
                IsDeletable = true;
                IsAlarmable = true;
            }

            if (Id == "spdif-input")
            {
                Title = StringResource.Get("NPTV");
            }

            IsPlayable = _IsPlayable;
            IsQueueable = _IsQueueable;
            IsFavoritable &= IsQueueable;
        }

        public static string ZPConvert(string p)
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
                int trunc = name.IndexOf(",p");
                return name.Substring(0, trunc);
            }
            return name;
        }

        // is the item playable? True if an audioitem or a playlist or a genre or an album, UNLESS it is the root Tracks item
        private bool _IsPlayable
        {
            get
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
        }

        // is the item queueable? True if we can play it, unless its a radio stream or LineIn
        private bool _IsQueueable
        {
            get
            {
                return IsPlayable &&
                    (!Class.StartsWith("object.item.audioItem.audioBroadcast")) &&
                    (Class != "object.item.audioItem");
            }
        }

        internal static DidlData MakeTV(Player device)
        {
            DidlData tv = new DidlData();
            tv.Id = "spdif-input";
            tv.Class = "object.item.audioItem";
            tv.Res = "x-sonos-htastream:" + device.UniqueName.Substring(5) + ":spdif";
            tv.Title = StringResource.Get("NPTV");
            return tv;
        }

        internal static string RadioMetadata(string id, string parent, string title)
        {
            return "<DIDL-Lite xmlns:dc=\"http://purl.org/dc/elements/1.1/\" xmlns:upnp=\"urn:schemas-upnp-org:metadata-1-0/upnp/\" xmlns:r=\"urn:schemas-rinconnetworks-com:metadata-1-0/\" xmlns=\"urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/\">" +
                "<item id=\"" + id.Replace(":", "%3a") + "\" parentID=\"" + parent.Replace(":", "%3a") + "\" restricted=\"true\">" +
                "<dc:title>" + SecurityElement.Escape(title) + "</dc:title>" +
                "<upnp:class>object.item.audioItem.audioBroadcast</upnp:class>" +
                "<desc id=\"cdudn\" nameSpace=\"urn:schemas-rinconnetworks-com:metadata-1-0/\">SA_RINCON65031_</desc>" +
                "</item></DIDL-Lite>";
        }

        /*
            <DIDL-Lite xmlns:dc="http://purl.org/dc/elements/1.1/"    xmlns:upnp="urn:schemas-upnp-org:metadata-1-0/upnp/"    xmlns:r="urn:schemas-rinconnetworks-com:metadata-1-0/"    xmlns="urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/">    
                <item>
                    <dc:title>''40''</dc:title>
                    <r:type>instantPlay</r:type>
                    <upnp:albumArtURI>/getaa?u=x-file-cifs%3a%2f%2fmontecarlo%2fmusic%2fU2%2fLive%2520-%2520Under%2520A%2520Blood%2520Red%2520Sky%2f08%2520-%2520''40''.mp3&amp;v=446</upnp:albumArtURI>
                    <res protocolInfo="x-file-cifs:*:audio/mpeg:*">x-file-cifs://montecarlo/music/U2/Live%20-%20Under%20A%20Blood%20Red%20Sky/08%20-%20''40'&amp;apos;.mp3</res>
                    <r:description>By U2</r:description>
                    <r:resMD>&lt;DIDL-Lite xmlns:dc="http://purl.org/dc/elements/1.1/" xmlns:upnp="urn:schemas-upnp-org:metadata-1-0/upnp/" xmlns:r="urn:schemas-rinconnetworks-com:metadata-1-0/" xmlns="urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/"&gt;&lt;item id="S://montecarlo/music/U2/Live%20-%20Under%20A%20Blood%20Red%20Sky/08%20-%20&amp;apos;&amp;apos;40&amp;apos;&amp;apos;.mp3" parentID="A:TRACKS" restricted="true"&gt;&lt;dc:title&gt;&amp;apos;&amp;apos;40&amp;apos;&amp;apos;&lt;/dc:title&gt;&lt;upnp:class&gt;object.item.audioItem.musicTrack&lt;/upnp:class&gt;&lt;desc id="cdudn" nameSpace="urn:schemas-rinconnetworks-com:metadata-1-0/"&gt;RINCON_AssociatedZPUDN&lt;/desc&gt;&lt;/item&gt;&lt;/DIDL-Lite&gt;</r:resMD>    
                </item>
            </DIDL-Lite>
         * */
        internal static string CreateFavorite(string title, string description, string art, string res, string protocolInfo, string fullmd)
        {
            string resel = protocolInfo != null ? ("<res protocolInfo=\"" + protocolInfo + "\">" + SecurityElement.Escape(res) + "</res>") : "<res>" + SecurityElement.Escape(res) + "</res>";

            return "<DIDL-Lite xmlns:dc=\"http://purl.org/dc/elements/1.1/\" xmlns:upnp=\"urn:schemas-upnp-org:metadata-1-0/upnp/\" xmlns:r=\"urn:schemas-rinconnetworks-com:metadata-1-0/\" xmlns=\"urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/\">" +
                "<item>" +
                "<dc:title>" + SecurityElement.Escape(title) + "</dc:title>" +
                "<r:type>instantPlay</r:type>" +
                Element("upnp:albumArtURI", art) +
                resel +
                Element("r:description", description) +
                "<r:resMD>" + SecurityElement.Escape(fullmd) + "</r:resMD>" +
                "</item></DIDL-Lite>";
        }

        private static string Element(string name, string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            return string.Format("<{0}>{1}</{0}>", name, SecurityElement.Escape(value));
        }

        internal static string CreateItem(string id, string parent, string title, string classname)
        {
            string item = id != null ? "<item id=\"" + SecurityElement.Escape(id) + "\" parentID=\"" + SecurityElement.Escape(parent) + "\" restricted=\"true\">" : "<item>";

            return "<DIDL-Lite xmlns:dc=\"http://purl.org/dc/elements/1.1/\" xmlns:upnp=\"urn:schemas-upnp-org:metadata-1-0/upnp/\" xmlns:r=\"urn:schemas-rinconnetworks-com:metadata-1-0/\" xmlns=\"urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/\">" +
                item +
                "<dc:title>" + SecurityElement.Escape(title) + "</dc:title>" +
                Element("upnp:class", classname) +
                "<desc id=\"cdudn\" nameSpace=\"urn:schemas-rinconnetworks-com:metadata-1-0/\">RINCON_AssociatedZPUDN</desc>" +
                "</item></DIDL-Lite>";
        }

        internal static string ClassToName(string uclass)
        {
            switch (uclass)
            {
                case "object.container.person.musicArtist":
                    return StringResource.Get("NPArtist");
                case "object.container.album.musicAlbum":
                    return StringResource.Get("NPArtist");
                case "object.container.genre.musicGenre":
                    return StringResource.Get("TypeGenre");
                case "object.container":
                    return StringResource.Get("FromMusicLibrary");
                case "object.container.playlistContainer":
                    return StringResource.Get("TypePlaylist");
                case "object.container.playlistContainer.sameArtist":
                    return StringResource.Get("TypeComposer");
                case null:
                    return string.Empty;
                default:
                    Debug.Assert(false);
                    return string.Empty;
            }
        }

        public static string ArtConverter(string art, Uri devicebaseuri, MusicItemDetail extendedMetadata)
        {
            if (string.IsNullOrEmpty(art))
            {
                art = null;
            }
            else
            {
                if (extendedMetadata != null && extendedMetadata.Source != null)
                {
                    if (extendedMetadata.Art != null)
                    {
                        art = ReplaceArtResolution(extendedMetadata.Art, extendedMetadata.Source);
                    }
                    else if (art.StartsWith("http"))
                    {
                        // eg Sonos Radio which returns no art in extended metadata but returns a real url 
                        art = ReplaceArtResolution(art, extendedMetadata.Source);
                    }
                }

                if (art.StartsWith("/getaa?"))
                {
                    // Local Library most likely
                    art = new Uri(devicebaseuri, art).AbsoluteUri;
                }
            }

            return art;
        }

        private static string ReplaceArtResolution(string uri, MusicService source)
        {
            var subst = source.GetArtSubstitions();
            if (subst != null && subst.Count > 0)
            {
                string newval = subst.Last().Value;       // HACK choose last ie biggest

                // Find a substition in the existing url
                foreach (string sub in subst.Values)
                {
                    if (uri.IndexOf(sub) >= 0)
                    {
                        uri = uri.Replace(sub, newval);
                        Debug.WriteLine($"Art({source.RawName}) upgraded to {newval}");
                        break;
                    }
                }
            }

            return uri;
        }
    }
}
