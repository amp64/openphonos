using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Net.NetworkInformation;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace OpenPhonos.UPnP
{
    /// <summary>
    /// Find UPnP Devices on a subnet using SSDP
    /// </summary>
    public class Finder
    {
        /// <summary>
        /// Callback used for every device found during a search
        /// IMPORTANT: This is called on a random thread, possibly at the same time as another thread in the same callback
        /// </summary>
        /// <param name="location">Address of device</param>
        /// <param name="network">NetworkInterface it was found via</param>
        /// <param name="headers">Headers on the device</param>
        /// <returns>true to carry on searching, false to stop as soon as possible</returns>
        public delegate Task<bool> FinderCallbackAsync(string location, AdapterInfo network, IDictionary<string, string> headers);

        public bool FoundVPN { get; private set; }
        private FinderCallbackAsync CallbackAsync;
        private string WantedUrn;
        private HashSet<string> FoundDevices = new HashSet<string>();
        private ArraySegment<byte> PacketBytes;

        public class AdapterInfo
        {
            public readonly IPAddress IPV4Address;
            public readonly string Description;
            public readonly string NetworkName;
            public readonly NetworkInterfaceType NetworkInterfaceType;

            public AdapterInfo(IPAddress ipv4Address)
            {
                IPV4Address = ipv4Address;
                Description = "localhost";
                NetworkName = "localhost";
                NetworkInterfaceType = NetworkInterfaceType.Wireless80211;
            }

            public AdapterInfo(NetworkInterface network)
            {
                IPV4Address = GetIPV4Address(network);
                Description = network.Description;
                NetworkName = network.Name;
                NetworkInterfaceType = network.NetworkInterfaceType;
            }
        }

        public static List<AdapterInfo> FindAdapters(out string problem)
        {
            TestInterfaces();

            var possible = new List<AdapterInfo>();
            problem = null;
            
            // Quickmode just uses the local IP but requires an internet connection
            // Slowmode uses as many network interfaces as it can find, but can run into 
            bool quickmode = false;

            try
            {
                if (!quickmode)
                {
                    var hosts = NetworkInterface.GetAllNetworkInterfaces();
                    NetLogger.WriteLine($"Total network interfaces: {hosts.Length}");

                    foreach (var host in hosts)
                    {
                        if (HostInteresting(host, ref problem))
                        {
                            possible.Add(new AdapterInfo(host));
                        }
                    }
                }
                else
                {
                    // Lets try and find our own IP address at least
                    using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
                    {
                        try
                        {
                            socket.Connect("8.8.8.8", 65530);
                            IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                            possible.Add(new AdapterInfo(endPoint.Address));
                        }
                        catch
                        { }
                    }
                }
            }
            catch (Exception ex)
            {
                NetLogger.WriteLine(string.Format("Exception {0} in GetHostNames", ex.Message));
                problem = StringResource.Get("InternalNetworkFailure");
            }

            if (possible.Count == 0 && problem == null)
                problem = StringResource.Get(Platform.Instance.IsAlwaysWiFi() ? "NotConnectedToWiFi" : "NoSuitableNetworksFound");

            return possible;
        }

        private static void TestInterfaces()
        {
            foreach (NetworkInterface netiface in NetworkInterface.GetAllNetworkInterfaces())
            {
                Debug.WriteLine($"interface name : {netiface.Description}, {netiface.NetworkInterfaceType}");
            }
            Debug.WriteLine("");
        }

        private static async Task TestBroadcastAsync()
        {
            try
            {
                IPAddress localIP;
                using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
                {
                    socket.Connect("8.8.8.8", 65530);
                    IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                    localIP = endPoint.Address;
                }

                bool multicast = false;

                using (var socket = new Socket(localIP.AddressFamily, SocketType.Dgram, ProtocolType.Udp))
                {
                    if (!multicast)
                    {
                        socket.EnableBroadcast = true;
                    }
                    socket.ExclusiveAddressUse = false;

                    var dest = new IPEndPoint(IPAddress.Broadcast, 1900);
                    socket.Bind(new IPEndPoint(localIP, 0));
                    Debug.WriteLine($"Test-Bound ok to {localIP}");
                    
                    var bytes = new ArraySegment<byte>(new byte[1]);
                    await socket.SendToAsync(bytes, SocketFlags.None, dest);
                }
            } 
            catch (Exception ex)
            {
               Debug.WriteLine("Test-Exception: " + ex.Message);
            }
        }

        private List<AdapterInfo> WorkingHosts;

        /// <summary>
        /// Find devices via their URN
        /// </summary>
        /// <param name="urn">URN to search for</param>
        /// <param name="found">Callback for every device found</param>
        /// <param name="ScanTime">How long to search for (in seconds)</param>
        /// <returns></returns>
        public async Task ByURNAsync(string urn, FinderCallbackAsync found, int ScanTime)
        {
            NetLogger.WriteLine("Device search Start on " + Platform.Instance.DeviceName() + " (" + Platform.Instance.FullPlatformName() + ")");

            FoundVPN = false;
            WantedUrn = urn;
            CallbackAsync = found;
            string conclusion = string.Empty;

            string find = "M-SEARCH * HTTP/1.1\r\n" +
                "HOST:239.255.255.250:1900\r\n" +
                "MAN:\"ssdp:discover\"\r\n" +
                "MX:" + ScanTime.ToString() + "\r\n" +
                "ST:" + urn + "\r\n" +
                "\r\n";

            PacketBytes = new ArraySegment<byte>(Encoding.UTF8.GetBytes(find));

            await TestBroadcastAsync();

            WorkingHosts = FindAdapters(out string problem);

            if (WorkingHosts.Count == 0)
            {
                NetLogger.LikelyProblem = problem;
            }
            else
            {
                List<Task> SearchTasks = new List<Task>();
                var multicastIP = IPAddress.Parse("239.255.255.250");

                foreach (var host in WorkingHosts)
                {
                    var addr = host.IPV4Address;

                    NetLogger.WriteLine("Trying host {0}", addr);

                    var t = SearchOnHostAsync(host, multicastIP, true);
                    SearchTasks.Add(t);

                    t = SearchOnHostAsync(host, IPAddress.Broadcast, false);
                    SearchTasks.Add(t);
                }

                Task all = Task.WhenAll(SearchTasks).TimeoutAfter(ScanTime * 1000);

                try
                {
                    await all;
                    await Task.Delay(500);          // allow a little more time to catch the laggards
                }
                catch (TimeoutException)
                { }
                catch (Exception ex)
                {
                    NetLogger.WriteLine("Exception {0} during SSDP", ex.Message);
                }

                conclusion = all.Status.ToString();
            }

            NetLogger.WriteLine("Device search complete, found {0} ({1})", FoundDevices.Count, conclusion);
        }

        private async Task SearchOnHostAsync(AdapterInfo host, IPAddress addr, bool multicast)
        {
            try
            {
                NetLogger.WriteLine("Searching on {0} {1} ({2}) {3} {4}", host.Description, host.NetworkName, host.NetworkInterfaceType, host.IPV4Address?.ToString(), multicast);

                await BroadcastSSDPAsync(host, addr, PacketBytes, multicast);
            }
            catch (Exception ex)
            {
                NetLogger.WriteLine("Search.Exception: " + ex.ToString());
            }
        }

        private static bool HostInteresting(NetworkInterface host, ref string why)
        {
            var name = host.Description;

            if (host.OperationalStatus != OperationalStatus.Up)
            {
                NetLogger.WriteLine("Ignoring {0} as {1}", name, host.OperationalStatus.ToString());
                return false;
            }

            if (host.NetworkInterfaceType == NetworkInterfaceType.Loopback)
            {
                NetLogger.WriteLine("Ignoring {0} as {1}", name, host.NetworkInterfaceType.ToString());
                return false;
            }

            var addr = GetIPV4Address(host);
            if (addr == null)
            {
                // Don't check this earlier as in the background it reports No, and yet gives up an address when asked
                if (!host.Supports(NetworkInterfaceComponent.IPv4))
                {
                    NetLogger.WriteLine("Ignoring {0} as not IPv4", name);
                }
                else
                {
                    NetLogger.WriteLine("Ignoring {0} as no IPv4 address", name);
                }
                return false;
            }

            var raw = addr.GetAddressBytes();
            if (raw[0] == 169 && raw[1] == 254)
            {
                NetLogger.WriteLine("Ignoring {0} as link-local", name);
                return false;
            }

            NetLogger.WriteLine($"Found {name} looks promising", name);
            return true;
        }

        public void NetworkStatusChanged()
        {
            NetLogger.WriteLine("Warning: Network Status Changed");
        }

        private async Task BroadcastSSDPAsync(AdapterInfo local, IPAddress remoteIP, ArraySegment<byte> reqBuff, bool multicast)
        {
            var localIP = local.IPV4Address;
            var socket = new Socket(localIP.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            if (!multicast)
            {
                socket.EnableBroadcast = true;
            }
            socket.ExclusiveAddressUse = false;

            var dest = new IPEndPoint(remoteIP, 1900);
            string x = "bind";

            try
            {
                socket.Bind(new IPEndPoint(localIP, 0));

                for (int i = 0; i < 3; i++)
                {
                    x = "send" + i.ToString();
                    await socket.SendToAsync(reqBuff, SocketFlags.None, dest);
                }

                x = "read";
                await ReadEverything(local, socket);           // will likely never return
            }
            catch (TimeoutException)
            {
            }
            catch (SocketException ex)
            {
                NetLogger.WriteLine($"Network broadcast error: {ex.Message} on {localIP} {x}");
            }
            catch (Exception ex)
            {
                NetLogger.WriteLine("Network search error: {0} on {1}", ex.Message, localIP.ToString());
            }

            socket.Dispose();
        }

        private async Task ReadEverything(AdapterInfo host, Socket socket)
        {
            var response = new ArraySegment<byte>(new byte[8192]);

            for (; ; )
            {
                var size = await socket.ReceiveAsync(response, SocketFlags.None);
                if (size == 0)
                    break;

                string raw = Encoding.UTF8.GetString(response.Take(size).ToArray());

                var headers = ParseSSDPResponse(raw);
                if (headers == null)
                    continue;
                if (!headers.TryGetValue("", out string first) || first != "HTTP/1.1 200 OK")
                    continue;
                if (!headers.TryGetValue("st", out string st) || st != WantedUrn)
                    continue;
                if (!headers.TryGetValue("location", out string location))
                    continue;
                lock (FoundDevices)
                {
                    if (FoundDevices.Contains(location))
                        continue;
                    FoundDevices.Add(location);
                }
                NetLogger.WriteLine("Found {0}", location);

                if (Listener.Me == null)
                {
                    // We will remember the first network adapter that finds a device
                    Listener.Me = host.IPV4Address;
                    NetLogger.WriteLine("Listener using IP address {0}", Listener.Me);
                }

                bool more = await CallbackAsync(location, host, headers);
                if (!more)
                {
                    NetLogger.WriteLine("Stopping search per callback");
                    break;
                }
            }
        }

        public static IPAddress GetIPV4Address(NetworkInterface net)
        {
            var addr = net.GetIPProperties().UnicastAddresses.FirstOrDefault(a => a.Address.AddressFamily == AddressFamily.InterNetwork);
            return addr?.Address;
        }

        // Probably not exactly compliant with RFC 2616 but good enough for Sonos
        internal static Dictionary<string, string> ParseSSDPResponse(string response)
        {
            StringReader reader = new StringReader(response);

            Dictionary<string, string> result = null;

            for (; ; )
            {
                string line = reader.ReadLine();
                if (line == null)
                    break;
                if (result==null)
                {
                    // First line add with empty key
                    result = new Dictionary<string, string>
                    {
                        { string.Empty, line }
                    };
                }
                else if (line != "")
                {
                    int colon = line.IndexOf(':');
                    if (colon < 1)
                    {
                        return null;
                    }
                    string name = line.Substring(0, colon).Trim();
                    string value = line.Substring(colon + 1).Trim();
                    if (string.IsNullOrEmpty(name))
                    {
                        return null;
                    }
                    result[name.ToLowerInvariant()] = value;
                }
            }

            return result;
        }
    }
}
