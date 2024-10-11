using System;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Diagnostics;
using System.Net.Http;
using System.Collections.Generic;

namespace OpenPhonos.UPnP
{
    public class ServiceInfo
    {
        public string ServiceType;
        public string ServiceId;
        public string SCP;
        public string ControlURL;
        public string EventSubURL;
        public Uri BaseUri;
    }

    public class Device
    {
        readonly XElement Dom;

        public Uri BaseUri { get; private set; }
        public bool IsRealDevice { get; }
        public bool IsMissing { get; private set; }

        const string NS = "urn:schemas-upnp-org:device-1-0";

        // This is used by Test code to be the subnet of fake devices
        public const string ImposterSubnet = "1.1.1.";

        // This is used by Test code to create fake devices
        public static Func<string, XElement> ImposterDeviceCreator { get; set; }

        private Device(string uri, XElement dom, bool real)
        {
            this.Dom = dom;
            this.BaseUri = new Uri(uri, UriKind.Absolute);
            this.IsRealDevice = real;
        }

        public string FriendlyName
        {
            get
            {
                return Attribute("friendlyName");
            }
        }

        public string UDN
        {
            get
            {
                return Attribute("UDN");
            }
        }

        public string Icon
        {
            get
            {
                var urls = from icon in Dom.Descendants(XName.Get("icon", NS))
                           select (string)icon.Element(XName.Get("url", NS));
                string url = urls.FirstOrDefault();
                if (!string.IsNullOrEmpty(url))
                {
                    url = new Uri(this.BaseUri, url).AbsoluteUri;
                }
                return url;
            }
        }

        public string ModelName
        {
            get
            {
                return Attribute("modelName");
            }
        }

        public string ModelNumber
        {
            get
            {
                return Attribute("modelNumber");
            }
        }

        /// <summary>
        /// This allows access to any non-standard attributes in the device.xml file
        /// </summary>
        /// <param name="name">The attribute name</param>
        /// <returns>The value of that attribute, or null if not found</returns>
        public string Attribute(string name)
        {
            return (string)Dom.Descendants(XName.Get(name, NS)).FirstOrDefault();
        }

        public string FindService(string urn)
        {
            var service = from item in Dom.Descendants(XName.Get("service", NS))
                          where (string)item.Element(XName.Get("serviceId", NS)) == urn
                          select new
                          {
                              ServiceType = (string)item.Element(XName.Get("serviceType", NS)),
                              ServiceId = (string)item.Element(XName.Get("serviceId", NS)),
                              SCP = (string)item.Element(XName.Get("SCPDURL", NS)),
                              ControlURL = (string)item.Element(XName.Get("controlURL", NS)),
                              EventSubURL = (string)item.Element(XName.Get("eventSubURL", NS))
                          };

            // The SCP is relative to the base uri
            return service != null ? BaseUri.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped) + service.First().ControlURL : null;
        }

        private static HttpClient _Client;

        private static HttpClient DeviceClient()
        {
            if (_Client == null)
            {
                var c = Service.CreateClient(UPnPConfig.UserAgent, false, 2000);
                c.DefaultRequestHeaders.Add("Accept", "*/*");
                _Client = c;
            }
            return _Client;
        }

        public static async Task<Device> CreateAsync(string uri)
        {
            if (ImposterDeviceCreator != null && uri.StartsWith("http://" + ImposterSubnet))
            {
                return new Device(uri, ImposterDeviceCreator.Invoke(uri), real: false);
            }

#if DEBUG && false
            if (uri.Contains(".112"))
                return Device.CreateMissingDevice(uri.Replace(".112",".300"));
#endif
            try
            {
                var resp = await DeviceClient().GetAsync(uri);
                var content = await resp.Content.ReadAsStreamAsync();
                var dom = XElement.Load(content);
                if (dom.GetDefaultNamespace() != NS)
                {
                    NetLogger.WriteLine("Bad default namespace {0} on {1}", dom.GetDefaultNamespace(), uri);
                    return null;
                }

                Device device = new Device(uri, dom, real: true);
                return device;
            }
            catch (Exception ex)
            {
                NetLogger.WriteLine("Error {0} during Device.Create_Async on {1}", ex.Message, uri);
                Debug.WriteLine(ex.StackTrace);
                return Device.CreateMissingDevice(uri.Replace(".112", ".300"));
            }
        }

        private static Device CreateMissingDevice(string uri)
        {
            var dom = new XElement(XName.Get("root", NS));
            var dev = new XElement(XName.Get("device", NS));
            dom.Add(dev);
            dev.Add(new XElement(XName.Get("UDN", NS), "uuid:RINCON_1234567"));
            var device = new Device(uri, dom, false);
            device.IsMissing = true;
            return device;
        }

        public ServiceInfo FindServiceInfo(string devicetype, string serviceid, bool throwIfMissing)
        {
            Debug.Assert(serviceid.Contains(":serviceId:"));
            Debug.Assert(devicetype.Contains(":device:"));

            var device = from item in Dom.Descendants(XName.Get("device", NS))
                         where (string)item.Element(XName.Get("deviceType", NS)) == devicetype
                         select item;

            var service = from item in device.Descendants(XName.Get("service", NS))
                          where (string)item.Element(XName.Get("serviceId", NS)) == serviceid
                          select new ServiceInfo()
                          {
                              ServiceType = (string)item.Element(XName.Get("serviceType", NS)),
                              ServiceId = (string)item.Element(XName.Get("serviceId", NS)),
                              SCP = (string)item.Element(XName.Get("SCPDURL", NS)),
                              ControlURL = (string)item.Element(XName.Get("controlURL", NS)),
                              EventSubURL = (string)item.Element(XName.Get("eventSubURL", NS)),
                              BaseUri = this.BaseUri
                          };

            var result = service.FirstOrDefault();

            if (throwIfMissing && result == null)
                throw new Exception(string.Format("UPnP Service {0} missing from {1}", serviceid, devicetype));

            return result;
        }

        public string Diagnostic()
        {
            return Dom.ToString();
        }
    }
}
