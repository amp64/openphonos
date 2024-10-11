using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace OpenPhonos.UPnP
{
    public class DeviceWatcher
    {
        public delegate Task NotifyCallback(IDictionary<string, string> headers);

        public event NotifyCallback OnDeviceUpdate;

        private string WantedUrn;
        private List<Socket> ListeningSockets;

        public DeviceWatcher(string urn)
        {
            WantedUrn = urn;
            ListeningSockets = new List<Socket>();
        }

        // inspired by: https://stackoverflow.com/questions/12794761/upnp-multicast-missing-answers-from-m-search-discovery

        public void StartWatching()
        {
            var multicastIP = IPAddress.Parse("239.255.255.250");

            IPEndPoint LocalEndPoint = new IPEndPoint(IPAddress.Any, 1900);
            IPEndPoint MulticastEndPoint = new IPEndPoint(multicastIP, 1900);

            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            socket.Bind(LocalEndPoint);
            socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(MulticastEndPoint.Address, IPAddress.Any));
            socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, 1);
            socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastLoopback, true);

            ListeningSockets.Add(socket);

            Task.Run(() =>
            {
                byte[] ReceiveBuffer = new byte[64000];

                try
                {
                    while (true)
                    {
                        int ReceivedBytes = socket.Receive(ReceiveBuffer, SocketFlags.None);

                        if (ReceivedBytes > 0)
                        {
                            string all = Encoding.UTF8.GetString(ReceiveBuffer, 0, ReceivedBytes);
                            var headers = Finder.ParseSSDPResponse(all);
                            if (headers == null)
                                continue;
                            if (!headers.TryGetValue(string.Empty, out string first))
                                continue;
                            if (first != "NOTIFY * HTTP/1.1")
                                continue;
                            if (!headers.TryGetValue("nt", out string nt))
                                continue;
                            if (nt != WantedUrn)
                                continue;

                            try
                            {
                                Platform.Instance.OnUIThread(async () =>
                                {
                                    await OnDeviceUpdate?.Invoke(headers);
                                });
                            }
                            catch (Exception)
                            { }
                        }
                        else
                        {
                            Debug.WriteLine("listenthread exiting");
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Exception {0} during device listen thread", (object)ex.Message);
                }
            });
        }

        public void StopWatching()
        {
            foreach (var socket in ListeningSockets)
            {
                try
                {
                    socket.Dispose();
                }
                catch (Exception)
                { }
            }
            ListeningSockets.Clear();
        }
    }
}
