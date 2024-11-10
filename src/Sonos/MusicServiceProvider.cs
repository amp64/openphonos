using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Net.Http;
using System.Web;
using System.Collections.Specialized;

namespace OpenPhonos.Sonos
{
    internal class MusicServiceDescription
    {
        internal int ServiceId
        {
            get
            {
                // Convert the XML ID into the ServiceID
                return (XmlId << 8) ^ 7;
            }
        }
        internal Int16 XmlId;
        internal string Name;
        internal string Version;
        internal string Uri;
        internal string SecureUri;
        internal string ContainerType;
        internal int Capabilities;
        internal string Auth;
        internal string LogoUri;              // can be null
        internal string StringsUri;           // can be null
        internal string PresentationUri;      // can be null
        internal string ManifestUri;          // can be null
        internal XElement PresentationMap;    // can be null

        public override string ToString()
        {
            return string.Format("{0} {1} ({2})", Name, ServiceId, XmlId);
        }
    }

    public partial class MusicServiceProvider
    {
        internal const string SonosUserAgent131 = "Linux UPnP/1.0 Sonos/63.2-78090 (WDCR:Microsoft Windows NT 10.0.18363)";
        internal const string SonosUserAgent1414 = "Linux UPnP/1.0 Sonos/69.1-32100 (WDCR:Microsoft Windows NT 10.0.19043)";

        internal const string SonosUserAgent = SonosUserAgent1414;

        /// <summary>
        /// Implementers need to provide an implementation of this for full Music Service support
        /// </summary>
        /// <param name="data">The encrypted data, which is return decrypted (or left alone if fails)</param>
        /// 
        partial void DecryptMusicServiceData(ref string data);

        internal string HouseholdId;
        internal string DeviceId;
        private IEnumerable<MusicServiceDescription> MusicServiceList;
        private List<MusicService> AvailableServices;

        // Caller MUST call InitializeAsync on each result before making any network calls
        public IReadOnlyList<MusicService> GetMusicServices { get { return AvailableServices; } }

        public event Action OnServicesLoaded;

        /// <summary>
        /// Only call this once per instance
        /// </summary>
        /// <param name="player">A player from the household</param>
        /// <returns></returns>
        public async Task InitializeAsync(Player player)
        {
            try
            {
                var hh = await player.DeviceProperties.GetHouseholdID();
                hh.ThrowIfFailed();
                HouseholdId = hh.CurrentHouseholdID;

                var servicelist = await player.MusicServices.ListAvailableServices();
                servicelist.ThrowIfFailed();

                var ids = servicelist.AvailableServiceTypeList.Split(',');
                var availableIds = ids.Select(id => int.Parse(id));

                var dev = await player.SystemProperties.GetString("R_TrialZPSerial");
                DeviceId = dev.Error == null ? dev.StringValue : null;

                var xml = XElement.Parse(servicelist.AvailableServiceDescriptorList);
                MusicServiceList =
                    from svc in xml.Descendants(XName.Get("Service"))
                    select new MusicServiceDescription()
                    {
                        XmlId = Int16.Parse(svc.Attribute(XName.Get("Id")).Value),
                        Name = svc.Attribute(XName.Get("Name")).Value,
                        Version = svc.Attribute(XName.Get("Version")).Value,
                        Uri = svc.Attribute(XName.Get("Uri")).Value,
                        SecureUri = svc.Attribute(XName.Get("SecureUri")).Value,
                        ContainerType = svc.Attribute(XName.Get("ContainerType")).Value,
                        Capabilities = int.Parse((string)svc.Attribute(XName.Get("Capabilities"))),
                        Auth = svc.Descendants(XName.Get("Policy")).Attributes(XName.Get("Auth")).FirstOrDefault().Value,
                        StringsUri = CalcPresentation(svc, "Strings"),
                        PresentationUri = CalcPresentation(svc, "PresentationMap"),
                        ManifestUri = (string)svc.Descendants(XName.Get("Manifest")).FirstOrDefault()?.Attribute(XName.Get("Uri")),
                    };

#if DEBUG
                var debug = DumpMusicList(MusicServiceList);
#endif
                AvailableServices = new List<MusicService>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("{0} during MusicServicesInit", (object)(ex.Message));
            }
        }

        private static string CalcPresentation(XElement svc, string itemName)
        {
            var p = svc.Descendants(XName.Get("Presentation"));
            if (p != null)
            {
                var s = p.Descendants(XName.Get(itemName)).FirstOrDefault();
                if (s != null)
                    return s.Attribute(XName.Get("Uri")).Value;
            }
            return null;
        }

#if DEBUG
        private static string DumpMusicList(IEnumerable<MusicServiceDescription> StandardServiceList)
        {
            var list = StandardServiceList.ToList();
            list.Sort((a, b) => string.Compare(a.Name, b.Name));
            var sb = new StringBuilder();
            foreach (var s in list)
            {
                sb.AppendFormat("{0},{1},{2},{3},{4},{5:x},{6},{7}\n", s.Name, s.Version, s.XmlId, s.Auth, s.ContainerType, s.Capabilities, s.Uri, s.SecureUri);
            }
            return sb.ToString();
        }
#endif

        private static string FindLogo(IEnumerable<XElement> artlist, int svcid, string which)
        {
            if (artlist == null)
                return null;
            string serviceid = svcid.ToString();
            var x = from art in artlist
                    where art.Attribute(XName.Get("id")).Value == serviceid
                    select art.Descendants(XName.Get("image"));
            if (x.FirstOrDefault() == null)
                return null;
            var y = from art in x.FirstOrDefault() where art.Attribute(XName.Get("placement")).Value == which select art.Value;
            var logo = y.FirstOrDefault();
            if (logo != null)
            {
                logo = logo.Trim();
                if(logo.StartsWith("http://"))
                {
                    // Force https for the logos 
                    logo = "https://" + logo.Substring(7);
                }
            }
            return logo;
        }

#if false
        internal MusicService GetServiceByUDNAsync(string id)
        {
            return AvailableServices.FirstOrDefault(s => s.Udn == id);
        }
#endif

        public string GetServiceName(string udn)
        {
            var service = AvailableServices.FirstOrDefault(s => s.Udn == udn);
            if (service != null)
            {
                return service.DisplayName;
            }
            int prefix = "SA_RINCON".Length;
            string idstr = udn.Substring(prefix, udn.IndexOf('_', prefix) - prefix);
            int id = int.Parse(idstr);
            return GetUnsupportedServiceName(id);
        }

        internal MusicServiceDescription FindMusicService(int id)
        {
            if (MusicServiceList != null)
                return MusicServiceList.FirstOrDefault(s => s.ServiceId == id);
            else
                return null;
        }

        public async Task<MusicService> GetServiceBySnAsync(string sn)
        {
            var services = AvailableServices;
            if (services != null)
            {
                var svc = services.FirstOrDefault(s => s.SerialNumber.ToString() == sn);
                if (svc != null)
                {
                    await svc.InitializeAsync();
                }
                return svc;
            }
            else
            {
                return null;
            }
        }

        public async Task<string> GetErrorMessageAsync(string serviceId, string which)
        {
            var std = await GetServiceBySnAsync(serviceId);
            if (std == null)
                return null;
            return std.LookupLocalizedString(which);
        }


        private string GetUnsupportedServiceName(int id)
        {
            if (MusicServiceList != null)
            {
                var svc = MusicServiceList.FirstOrDefault((s) => s.ServiceId == id);
                if (svc != null)
                {
                    return svc.Name;
                }
            }
            return string.Format("Music Service {0}", id);
        }

        /// <summary>
        /// Call this after InitializeAsync, once the encrypted service list has arrived
        /// </summary>
        /// <param name="encryptedServices"></param>
        /// <returns></returns>
        public async Task RefreshAsync(string encryptedServices)
        {
            // Go find the artwork (optional)
            IEnumerable<XElement> artlist = null;
            var beforeArt = DateTime.Now;
            try
            {
                /*
                    cr-200: 48x48
                    icr: 36x36
                    acr: 36x36
                    acr-hdpi: 54x54
                    size: 
                     small 18x18
                     medium: 24x24
                     large: 48x48
                     x-large: 72x72
                     square:x-small: 20x20 SVG
                     square:small: 40x40 SVG
                     square: 112x112 SVG
                */
                // Important to set these to get the old circular pngs instead of the new square SVGs
                var client = UPnP.Service.CreateClient(SonosUserAgent, true);
                client.DefaultRequestHeaders.Add("X-Sonos-SWGen", "1");
                client.MaxResponseContentBufferSize = 512 * 1024;
                string artfile = await client.GetStringAsync("https://update-services.sonos.com/services/mslogo.xml");
                var artxml = XElement.Parse(artfile);
                artlist = artxml.Descendants(XName.Get("sized")).Descendants(XName.Get("service"));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception {ex.Message} getting music service art");
            }

            bool refresh = false;
            string arttype = "x-large";

            // Get the credentials
            string credentials = encryptedServices;
            DecryptMusicServiceData(ref credentials);
            if (encryptedServices != credentials)
            {
                if (AvailableServices.Count != 0)
                {
                    Debug.WriteLine("SERVICE REFRESH {0}", DateTime.Now.ToLocalTime());
                    refresh = true;
                }

                var xml = XElement.Parse(credentials);

                var unsupportedServices = new List<string>();

                foreach (var service in xml.Descendants(XName.Get("Service")))
                {
                    var numaccounts = service.Attribute(XName.Get("NumAccounts")).Value;

                    string udn;
                    int id = ExtractUdnAndService(service, out udn);
                    var available = MusicServiceList.FirstOrDefault(s => s.ServiceId == id);

                    // These are new for 5.2
                    var nickname = (string)service.Attribute(XName.Get("Nickname0"));
                    var serial = (string)service.Attribute(XName.Get("SerialNum0"));

                    // The availability of these depends on the auth type
                    var user = (string)service.Attribute(XName.Get("Username0"));
                    var password = (string)service.Attribute(XName.Get("Password0"));
                    var token = (string)service.Attribute(XName.Get("Token0"));
                    var key = (string)service.Attribute(XName.Get("Key0"));

                    if (IsServiceHidden(service))
                        continue;

                    if (refresh)
                    {
                        if (available != null)
                        {
                            var existing = AvailableServices.FirstOrDefault(s => s.Udn == udn);
                            var newlogo = FindLogo(artlist, id, arttype);
                            if (existing != null)
                            {
                                MusicService.Update(existing, user, key, token, serial, newlogo);
                            }
                            else
                            {
                                // Ignore new services
                                Debug.WriteLine("New Music Service!");
                            }
                        }
                        continue;
                    }

                    string ignored;
                    var count = xml.Descendants(XName.Get("Service")).Count(x => ExtractUdnAndService(x, out ignored) == id && !IsServiceHidden(x));
                    if (count < 2)
                        nickname = null;

                    if (available != null)
                    {
                        available.LogoUri = FindLogo(artlist, id, arttype);
                        AvailableServices.Add(new MusicService(this, available, udn, user, key, token, serial, nickname));
                    }
                    else
                    {
                        unsupportedServices.Add(GetUnsupportedServiceName(id));
                    }
                }
            }
            else
            {
                Debug.WriteLine("Warning: no DecryptMusicServiceData override so no music services");
            }

            AvailableServices.Sort((a, b) => a.DisplayName.CompareTo(b.DisplayName));
            OnServicesLoaded?.Invoke();
        }

        private static bool IsServiceHidden(XElement service)
        {
            var flags = (string)service.Attribute(XName.Get("Flags0"));

            // Flag bit 0 appears to be a "hidden" bit for Spotify-connect accounts
            // Flag bit 1 is used for Tidal, for reasons unknown
            return (flags != null && int.TryParse(flags, out int flag) && ((flag & 1) != 0));
        }

        private static int ExtractUdnAndService(XElement service, out string udn)
        {
            udn = service.Attribute(XName.Get("UDN")).Value;

            int prefix = "SA_RINCON".Length;
            string idstr = udn.Substring(prefix, udn.IndexOf('_', prefix) - prefix);
            return int.Parse(idstr);
        }

        internal MusicService GetServiceFromUdnMaybe(string udn)
        {
            var list = AvailableServices;
            if (list != null)
            {
                var svc = list.FirstOrDefault(s => s.Udn == udn);
                if (svc != null && svc.IsInitialized)
                {
                    return svc;
                }
            }
            return null;
        }

        internal async Task<string> SendFeedback(Player player, string udn, string station, string track, bool liked)
        {
            var service = AvailableServices.FirstOrDefault(s => s.Udn == udn);
            if (service == null)
                throw new Exception("Unknown music service");

            return await service.SendFeedback(player, station, track, liked);
        }

        // Need to get some artwork from a Service that is secured
        // data can be a DataItemBase (of the item) or a string (which is a /getaa-type uri)
        internal async Task GetStreamFromService(System.IO.Stream stream, Uri uri, object data)
        {
            if (data is MusicItem)
            {
                /* TODO
                if (((MusicItem)data).Provider is IMusicService)
                {
                    var provider = ((DataItemBase)data).Provider as IMusicService;
                    await provider.GetSecureStreamAsync(stream, uri.AbsoluteUri);
                    return;
                }
                else */
                if (uri.Scheme == "x-sonos-auth-https")
                {
                    // Amazon art in Faves list? Ends with "-<serialnumber>"
                    int dash = uri.OriginalString.LastIndexOf('-');
                    if (dash != -1)
                    {
                        var sn = uri.OriginalString.Substring(dash + 1);
                        var provider = AvailableServices.FirstOrDefault(s => s.SerialNumber.ToString() == sn);
                        if (provider != null)
                        {
                            await provider.GetSecureStreamAsync(stream, uri.AbsoluteUri);
                            return;
                        }
                    }
                }
            }
            else if (data is string)
            {
                string u = data as string;
                var sn = HttpUtility.ParseQueryString(u)["sn"];         // serial numbers new for 5.2
                var provider = AvailableServices.FirstOrDefault(s => s.SerialNumber.ToString() == sn);
                if (provider != null)
                {
                    await provider.GetSecureStreamAsync(stream, u);
                    return;
                }
            }

            throw new System.IO.FileNotFoundException("Failed to find music service for art");
        }

        internal MusicService CreateRadioService()
        {
            const int TuneInId = 65031;

            var description = FindMusicService(TuneInId);
            if (description != null)
            {
                description.Auth = "Radio";
            }
            else
            {
                description = new MusicServiceDescription()
                {
                    Auth = "Radio",
                    Capabilities = 0,
                    Name = "Radio",
                    Uri = "https://legato.radiotime.com/Radio.asmx",
                    XmlId = ((TuneInId ^ 7) >> 8),
                };
            }

            return new MusicService(this, description, "SA_RINCON65031_", null, null, null, null, null);
        }

        // Attempts to extract extended metadata from the res path, if it is a music service path
        // If not, returns null
        public async Task<MusicItemDetail> GetExtendedMetadataFromResAsync(string res, Player player)
        {
            if (string.IsNullOrEmpty(res))
            {
                return null;
            }

            // We expect something:id-string-escaped?args&sn=123
            int question = res.IndexOf('?');
            if (question < 0)
            {
                return null;
            }

            var args = HttpUtility.ParseQueryString(res.Substring(question + 1));
            string sn = args["sn"];
            if (string.IsNullOrEmpty(sn))
            {
                return null;
            }

            var source = await GetServiceBySnAsync(sn);
            if (source == null)
            {
                return null;
            }

            var parts = res.Substring(0, question).Split(':');
            if (parts.Length < 2)
            {
                return null;
            }

            string id = parts[1];
            int dot = id.LastIndexOf('.');
            if (dot > 0)
                id = id.Substring(0, dot);        // drop .mp4 but keep if in id (eg Apple)
            id = Uri.UnescapeDataString(id);
            var result = await source.GetExtendedMetadataAsync(id, player);
            return result;
        }
    }

    // When this is thrown the caller should retry the original call
    internal class RetryException : Exception
    {
        internal RetryException(string item, string value)
        {
        }
    }
}
