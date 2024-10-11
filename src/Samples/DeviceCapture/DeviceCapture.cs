using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using OpenPhonos.UPnP;

namespace DeviceCapture
{
    /// <summary>
    /// Scans the network for Sonos devices and captures their service descriptions
    /// Writes to directory passed as argument
    /// Writes device files and all services files to a subdirectory whose name is the version of the APIs
    /// Removes all identifying information from the files (IP address, display names, serial numbers, MAC addresses etc
    /// </summary>
    public class DocComparer : Comparer<XmlDocument>
    {
        public override int Compare(XmlDocument x, XmlDocument y)
        {
            var a = x.GetElementsByTagName("serialNum")[0].InnerText;
            var b = y.GetElementsByTagName("serialNum")[0].InnerText;
            return a.CompareTo(b);
        }
    }

    class DeviceCapture
    {
        public const string SonosUrn = "urn:schemas-upnp-org:device:ZonePlayer:1";
        public const string NS = "urn:schemas-upnp-org:device-1-0";

        static void Main(string[] args)
        {
            string basedir = args[0];
            if (!Directory.Exists(basedir))
            {
                Console.WriteLine("Usage: DeviceCapture <directory>");
                return;
            }
            FindAllDevicesAsync(basedir).Wait();
        }

        private static async Task FindAllDevicesAsync(string baseDirectory)
        {
            Console.WriteLine("Searching for devices");

            var finder = new Finder();
            List<string> locations = new List<string>();

            await finder.ByURNAsync(SonosUrn, async (location, network, headers) =>
            {
                if (headers.TryGetValue("st", out string st) && st == SonosUrn)
                {
                    lock (locations)
                    {
                        if (!locations.Contains(location))
                        {
                            locations.Add(location);
                        }
                    }
                }

                return await Task.FromResult(true);
            }, 3);

            Console.WriteLine("Found {0} devices", locations.Count);

            // For consistent results, the order is always based on the serial number of each device
            var documents = new SortedList<XmlDocument, string>(new DocComparer());
            var client = new HttpClient();

            foreach (string loc in locations)
            {
                string raw = await client.GetStringAsync(loc);
                var doc = new XmlDocument();
                doc.PreserveWhitespace = true;
                doc.LoadXml(raw);
                documents.Add(doc, loc);
            }

            foreach (var player in documents)
            {
                string loc = player.Value;
                var doc = player.Key;

                var root = doc.DocumentElement;
                var version = root.GetElementsByTagName("softwareVersion", NS)[0].InnerText;
                var display = doc.GetElementsByTagName("displayVersion", NS)[0].InnerText;
                var model = doc.GetElementsByTagName("modelNumber", NS)[0].InnerText;

                string dirname = Path.Combine(baseDirectory, string.Format("{0} ({1})", version, display));
                Directory.CreateDirectory(dirname);

                var svc = root.GetElementsByTagName("SCPDURL", NS);

                string modelname = model + ".xml";
                if (svc.Count == 6)
                    modelname = model + "_b.xml";           // Bound devices listed differently

                string filename = Path.Combine(dirname, modelname);

                var room = doc.GetElementsByTagName("roomName")[0].InnerText;
                Console.WriteLine("{0} is {1} with {2} services", room, model, svc.Count);

                if (!File.Exists(filename))
                {
                    Console.WriteLine("Updating {0} from {1}", modelname, room);
                    MakeAnonymous(doc);
                    doc.Save(filename);

                    for (int i = 0; i < svc.Count; i++)
                    {
                        string name = svc[i].InnerText;
                        filename = Path.Combine(dirname, Path.GetFileName(name));
                        if (!File.Exists(filename))
                        {
                            var uri = new Uri(loc);
                            string all = await client.GetStringAsync(uri.GetLeftPart(UriPartial.Authority) + name);
                            File.WriteAllText(filename, all);
                        }
                    }
                }
            }
        }

        private static void MakeAnonymous(XmlDocument doc)
        {
            var room = doc.GetElementsByTagName("roomName")[0];
            string originalRoom = room.InnerText;
            room.InnerText = "Room";

            var friendly = doc.GetElementsByTagName("friendlyName");
            for (int i = 0; i < friendly.Count; i++)
            {
                var parts = friendly[i].InnerText.Split('-');
                if (parts[0].Trim() == originalRoom)
                    friendly[i].InnerText = room.InnerText + " -" + parts[1];
                else
                    friendly[i].InnerText = "1.2.3.4 -" + parts[1];
            }

            var serial = doc.GetElementsByTagName("serialNum")[0];
            serial.InnerText = "00-11-22-33-44-55:1";

            var mac = doc.GetElementsByTagName("MACAddress");
            if (mac.Count > 0)
            {
                mac[0].InnerText = "00:11:22:33:44:55";
            }

            var udn = doc.GetElementsByTagName("UDN");
            for (int i = 0; i < udn.Count; i++)
            {
                var parts = udn[i].InnerText.Split('_');
                udn[i].InnerText = parts[0] + "_00112233445501400";
                if (parts.Length > 2)
                    udn[i].InnerText += "_" + parts[2];
            }

            var deviceId = doc.GetElementsByTagName("deviceID");
            for(int i=0;i<deviceId.Count; i++)
            {
                int r = deviceId[i].InnerText.IndexOf("RINCON_");
                if (r > 0)
                {
                    deviceId[i].InnerText = deviceId[i].InnerText.Substring(0, r) + "RINCON_00112233445501400";
                }
            }
        }
    }
}
