using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace OpenPhonos.Sonos
{
    public struct SimpleDidl
    {
        const string NS = "urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/";
        const string R = "urn:schemas-rinconnetworks-com:metadata-1-0/";
        const string UPNP = "urn:schemas-upnp-org:metadata-1-0/upnp/";
        const string DC = "http://purl.org/dc/elements/1.1/";

        public string res;
        public string streamContent;
        public string radioShowMd;
        public string albumArtURI;
        public string title;
        public string uclass;
        public string creator;
        public string album;
        public string desc;
        public string streamInfo;
        public string parent;
        public string id;
        public string protocolInfo;
        public string narrator;
        public string releaseDate;

        public SimpleDidl(XElement xml, bool tiny)
        {
            title = (string)xml.Descendants(XName.Get("title", DC)).FirstOrDefault();
            uclass = (string)xml.Descendants(XName.Get("class", UPNP)).FirstOrDefault();
            parent = null;
            id = null;
            protocolInfo = null;
            releaseDate = null;
            if (!tiny)
            {
                res = (string)xml.Descendants(XName.Get("res", NS)).FirstOrDefault();
                streamContent = (string)xml.Descendants(XName.Get("streamContent", R)).FirstOrDefault();
                streamInfo = (string)xml.Descendants(XName.Get("streamInfo", R)).FirstOrDefault();
                radioShowMd = (string)xml.Descendants(XName.Get("radioShowMd", R)).FirstOrDefault();
                albumArtURI = (string)xml.Descendants(XName.Get("albumArtURI", UPNP)).FirstOrDefault();
                creator = (string)xml.Descendants(XName.Get("creator", DC)).FirstOrDefault();
                album = (string)xml.Descendants(XName.Get("album", UPNP)).FirstOrDefault();
                narrator = (string)xml.Descendants(XName.Get("narrator", R)).FirstOrDefault();
                desc = string.Empty;
                string release = (string)xml.Descendants(XName.Get("releaseDate", R)).FirstOrDefault();
                if (release != null && DateTime.TryParse(release, out var dt))
                {
                    releaseDate = dt.ToShortDateString();
                }
                parent = xml.Descendants(XName.Get("container", NS)).Select(s => (string)s.Attribute(XName.Get("parentID"))).FirstOrDefault();
                id = xml.Descendants(XName.Get("container", NS)).Select(s => (string)s.Attribute(XName.Get("id"))).FirstOrDefault();
                protocolInfo = xml.Descendants(XName.Get("res", NS)).Select(s => (string)s.Attribute(XName.Get("protocolInfo"))).FirstOrDefault();
            }
            else
            {
                res = streamContent = radioShowMd = albumArtURI = creator = album = streamInfo = string.Empty;
                desc = (string)xml.Descendants(XName.Get("desc", NS)).FirstOrDefault();
                narrator = null;
            }
        }
    }
}
