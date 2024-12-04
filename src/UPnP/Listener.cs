#define FIXED_EVENT_LOCATION
using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Net.Sockets;
using System.Net.Http;
using System.Net;
using System.Text;

namespace OpenPhonos.UPnP
{
    public class EventSubscriptionArgs : EventArgs
    {
        public uint EventNumber;
        public Dictionary<string, string> Items;        // never null
        public string Body;
    }

    public class Listener
    {
        static private Socket listenerSocket;
        static public int DefaultTimeout = 5 * 60;        // seconds, should be 60*60
        static public int LongTimeout = 60 * 60;
        public const int MinimumTimeout = 4 * 60;

        static public int RenewAfterFailureTimeout = 1 * 1000; // 1 second
        static public int RenewAfterFailureCount = 5;
        static public int SubscribeAfterFailureCount = 5;

        static public event Action<string> CriticalErrorHandler;

        private static void CreateListener()
        {
            if (listenerSocket == null)
            {
                listenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                Exception last = null;

                // Always let the OS choose the port (critical for Xbox)
                int port = 0;

                try
                {
                    listenerSocket.Bind(new IPEndPoint(IPAddress.Any, port));
                    listenerSocket.Listen(0);

                    var result = listenerSocket.LocalEndPoint as IPEndPoint;
                    NetLogger.WriteLine($"Listener setup on {Me}:{result.Port}");

                    ListenAndRespond(listenerSocket);
                    return;
                }
                catch (Exception ex)
                {
                    last = ex;
                }

                CriticalError(string.Format("Network setup problem during bind ({0})", last.Message));
            }
        }

        class SocketInfo
        {
            public Socket Socket;
        }

        private static void ListenAndRespond(Socket socket)
        {
            var item = new SocketInfo() { Socket = socket };

            try
            {
                // add this arg to avoid https://stackoverflow.com/questions/70556555/socket-endaccept-throwing-argumentexception
                const int maxsize = 65536;
                var result = socket.BeginAccept(maxsize, AcceptCallback, item);
            }
            catch (ObjectDisposedException)
            {
                // This can happen during shutdown, its ok
                Debug.WriteLine("ListendAndRespond caught disposed exception");
            }
        }

        // this assumes Sonos-specific format, which hopefully won't change
        static byte[] SingleCRPattern = Encoding.UTF8.GetBytes("\r\n");
        static byte[] DoubleCRPattern = Encoding.UTF8.GetBytes("\r\n\r\n");
        static byte[] ContentLengthPattern = Encoding.UTF8.GetBytes("CONTENT-LENGTH:");

        private static int FindByteMatch(byte[] source, int sourceLen, byte[] find, int start)
        {
            int where = start;
            int findLen = find.Length;
            sourceLen -= findLen;

            while (where < sourceLen)
            {
                int f = Array.IndexOf(source, find[0], where, sourceLen);
                if (f == -1)
                    break;

                // First byte matches, check rest
                int i;
                for (i = 1; i < findLen; i++)
                {
                    if (source[f + i] != find[i])
                        break;
                }

                if (i == findLen)
                    return f;

                where = f + 1;
            }

            return -1;
        }

        private static void AcceptCallback(IAsyncResult ar)
        {
            var item = ar.AsyncState as SocketInfo;
            string all;
            Socket respondingSocket;

            try
            {
                respondingSocket = item.Socket.EndAccept(out byte[] buffer, out int offset, ar);
                byte[] readBuffer = new byte[65536];
                if (offset != 0)
                    Array.Copy(buffer, readBuffer, offset);                            // copy anything we have already (usually nothing though)
                int expectedContentLength = 0;

                for (; ; )
                {
                    if (offset != 0 && expectedContentLength == 0)
                    {
                        // Try and guess how large the packet will be, so we can read it efficiently
                        int headerEnd = FindByteMatch(readBuffer, offset, DoubleCRPattern, 0);
                        if (headerEnd != -1)
                        {
                            int header = FindByteMatch(readBuffer, offset, ContentLengthPattern, 0);
                            if (header != -1)
                            {
                                header += ContentLengthPattern.Length;
                                int contentEnd = FindByteMatch(readBuffer, offset, SingleCRPattern, header);
                                if (contentEnd != -1)
                                {
                                    string number = Encoding.UTF8.GetString(readBuffer, header, contentEnd - header);
                                    if (int.TryParse(number, out expectedContentLength))
                                        expectedContentLength += (headerEnd + 4);
                                }
                            }
                        }
                    }

                    if (expectedContentLength == 0)
                    {
                        if (offset != 0)
                            NetLogger.WriteLine("WARNING: slow eventing");

                        // There is a 5-second timeout on the final zero-length buffer, so try and avoid that
                        if (!respondingSocket.Poll(3500, SelectMode.SelectRead))
                            break;
                        int got = respondingSocket.Receive(readBuffer, offset, 65536 - offset, SocketFlags.None);
                        if (got == 0)
                            break;
                        offset += got;
                    }
                    else if (offset < expectedContentLength)
                    {
                        int got = respondingSocket.Receive(readBuffer, offset, expectedContentLength - offset, SocketFlags.None);
                        if (got == 0)
                            break;
                        offset += got;
                    }
                    else
                    {
                        break;
                    }
                }

                all = Encoding.UTF8.GetString(readBuffer, 0, offset);
            }
            catch
            {
                // Socket was likely closed, ok to stop now
                return;
            }

            var sreader = new StringReader(all);
            string firstline = sreader.ReadLine();
            List<string> headers = new List<string>();

            for (; ; )
            {
                var header = sreader.ReadLine();
                if (string.IsNullOrWhiteSpace(header))
                    break;
                headers.Add(header);
            }
            var httpheaders = ParseHeaders(headers);

            string response;
            Listener listener = null;

            if (ParseNotify(firstline, out int id))
            {
                lock (ListenActions)
                {
                    listener = ListenActions.FirstOrDefault((a) => a.ListenerId == id);
                }
            }

            if (listener != null)
            { 
                string contentType = "text/plain";
                int contentSize = 0;

                response = String.Format("HTTP/1.1 200 OK\r\n" +
                                     "Content-Type: {0}\r\n" +
                                     "Content-Length: {1}\r\n" +
                                     "Connection: close\r\n" +
                                     "\r\n",
                                     contentType,
                                     contentSize);
            }
            else
            {
                response = "HTTP/1.1 412 Precondition failed\r\n\r\n";

                Debug.WriteLine("Event {0} has no listener so returning 412", id);
            }

            var respbuf = Encoding.UTF8.GetBytes(response);
            var src = respondingSocket.RemoteEndPoint as IPEndPoint;

            respondingSocket.NoDelay = true;                           // disable NAGLE
            int sent = respondingSocket.Send(respbuf);
            Debug.Assert(sent == respbuf.Length);

            var body = sreader.ReadToEnd();

            if (listener != null)
            {
                try
                {
                    listener.DispatchEvent(src.Address, body, httpheaders);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("{0} during listener DispatchEvent", (object)ex.Message);
                }
            }

            // Get ready for the next result
            ListenAndRespond(item.Socket);
        }

        public static void CriticalError(string msg)
        {
            NetLogger.WriteCRITICAL(msg);

            if (CriticalErrorHandler != null)
            {
                CriticalErrorHandler.Invoke(msg);
            }
        }

        private static Dictionary<string, string> ParseHeaders(List<string> headers)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();

            foreach (var line in headers)
            {
                if ((line != null) && (line != ""))
                {
                    int colon = line.IndexOf(':');
                    if (colon < 1)
                    {
                        throw new InvalidDataException("Colon missing");
                    }
                    string name = line.Substring(0, colon).Trim();
                    string value = line.Substring(colon + 1).Trim();
                    if (string.IsNullOrEmpty(name))
                    {
                        throw new InvalidDataException("Bad value after colon");
                    }
                    result[name.ToLowerInvariant()] = value;
                }
                else
                {
                    break;
                }
            }

            return result;
        }

        public static async Task<string> EventFailureMessage(Uri player)
        {
            string msg;
            string name = "default";         // should try and get the WiFi name but needs a Host
            bool playerProblem = false;
            if (player != null)
            {
                // Try and read the topo directly to see if VPN is blocking
                try
                {
                    using (var client = new HttpClient())
                    {
                        var uri = new UriBuilder(player);
                        uri.Path = "/status/topology";
                        var topo = await client.GetStringAsync(uri.Uri);
                        NetLogger.WriteLine(string.Format("Player {0} read {1}", player.Host, topo.Length));
                        playerProblem = string.IsNullOrEmpty(topo);
                    }
                }
                catch (Exception ex)
                {
                    NetLogger.WriteLine(string.Format("Player {0} error {1}", player.Host, ex.Message));
                    playerProblem = true;
                }
            }
            string badplayer = listenerSocket != null && playerProblem ? $", or the player at {player.Host} is not responding" : string.Empty;

            msg = string.Format(StringResource.Get("Firewall_Blocking"), name);
            msg += badplayer;

            return msg;
        }

        public delegate Task EventHandler(Service sender, EventSubscriptionArgs args);

        private void DispatchEvent(IPAddress host, string result, Dictionary<string, string> headers)
        {
            EventSubscriptionArgs args;

            lock (this)
            {
                uint eventnumber;
                if (!uint.TryParse(headers["seq"], out eventnumber))
                {
                    Debug.WriteLine("{0}: Bad SEQ value", InternalName);
                    return;
                }

                int expectedlength = -1;
                if (headers["content-length"] != null)
                    int.TryParse(headers["content-length"], out expectedlength);

                if (eventnumber < NextEventNumber)
                {
                    if (eventnumber + 1 == NextEventNumber)
                    {
                        Debug.WriteLine("{0}: Ignoring duplicate event {1}", InternalName, eventnumber);
                    }
                    else
                    {
                        Debug.WriteLine("{0}: Ignoring old event {1}", InternalName, eventnumber);
                    }
                    return;
                }
                else if (eventnumber > NextEventNumber)
                {
                    Debug.WriteLine("{0}: EVENT(S) LOST", InternalName);
                }
                else
                {
                    Debug.WriteLine("{0}: got SEQ {1} {2}", InternalName, eventnumber, DateTime.Now.TimeOfDay);
                }

                NextEventNumber = eventnumber + 1;
                if (NextEventNumber == 0)
                    NextEventNumber++;                 // roll around correctly

                // Debug.WriteLine("{0}({1}/{3}):{2}", InternalName, result.Length, result, expectedlength);
                Dictionary<string, string> items;

                try
                {
                    const string NS = "urn:schemas-upnp-org:event-1-0";
                    var xml = XElement.Parse(result);
                    var props = from prop in xml.Descendants(XName.Get("property", NS))
                                select new
                                {
                                    Name = prop.Descendants().First().Name.LocalName,
                                    Value = prop.Descendants().First().Value
                                };

                    items = new Dictionary<string, string>(props.Count());

                    foreach (var prop in props)
                    {
                        items[prop.Name] = prop.Value;
                    }
                }
                catch (Exception)
                {
                    // Didnt get valid XML, so pass everything (without a dictionary) and hope it makes sense
                    items = new Dictionary<string, string>();
                }

                args = new EventSubscriptionArgs() { Body = result, EventNumber = eventnumber, Items = items };
            }

            // Call the event handler on the UI thread (but not under the lock)
            Platform.Instance.OnUIThread(async () =>
            {
                try
                {
                    var evt = this.EventActions;
                    if (evt != null)
                    {
                        await evt.Invoke(this.Parent, args);
                    }
                }
                catch (Exception ex)
                {
                    string err = string.Format("{0} during {1} event handler ({2})", ex.Message, InternalName, ListenerId);
                    NetLogger.WriteLine(err);
                }
            });
        }

        private Listener(int seconds, EventHandler callback, string info, Service parentService)
        {
            // Each one has a monotonic increasing id used to identify it in the URL
            this.ListenerId = ++ListenerCounter;

            AddSubscription(callback);

            // append the last byte of the IP address to the internal name
            var dot = parentService.BaseUri.Host.LastIndexOf('.');
            if (dot != -1)
            {
                this.InternalName = info + "/" + parentService.BaseUri.Host.Substring(dot + 1);
            }
            else
            {
                this.InternalName = info;
            }
            this.Parent = parentService;
            this.RequestedTimeout = seconds;
            this.AutoRenew = true;
        }

        static private Random Random = new Random();

        // When the SUBSCRIBE call returns it says how long it will be until it expires, we will need
        // to auto-renew before then
        public void SetTimeout(string timeout)
        {
            if (!timeout.ToLower().StartsWith("second-"))
                throw new InvalidDataException();

            this.Lifetime = int.Parse(timeout.Substring(7));

            // We will renew between 1 and 2 minutes before expiry (assumes its at least 3 minutes)
            // Its random so we dont renew everything at once
            int seconds = Lifetime - 60 - Random.Next(0, 60);
            if (seconds < 60)
                throw new InvalidDataException();

            RenewTimer = new System.Threading.Timer(RenewHandler, null, seconds * 1000, seconds * 1000);
        }

        public void AddSubscription(EventHandler callback)
        {
            // Ideally we would lock EventActions, but it can be null when empty
            lock (this)
            {
                EventActions += callback;
            }
        }

        public void RemoveSubscription(EventHandler callback)
        {
            lock (this)
            {
                EventActions -= callback;
            }
        }

        public bool HasSubscriptions
        {
            get
            {
                return EventActions != null;
            }
        }

        public int SubscriptionCount
        {
            get
            {
                lock (this)
                {
                    return EventActions != null ? EventActions.GetInvocationList().Length : 0;
                }
            }
        }

        private void RenewHandler(object state)
        {
            RenewHandlerAsync().Later();
        }

        private async Task RenewHandlerAsync()
        {
            if (AutoRenew)
            {
                Debug.Assert(IsSubscribed);

                NetLogger.WriteLine(NetLogger.LogType.LogVerbose, "Time to renew {0}", InternalName);

                RenewTimer?.Dispose();
                RenewTimer = null;
                bool ok = await this.Parent.RenewSubscriptionAsync(Lifetime);

                // TODO if this fails, set a timer and try later?
            }
        }

        public bool IsSubscribed
        {
            get { return RenewTimer != null; }
        }

        public void Unwanted()
        {
            Debug.WriteLine("{0} now unwanted ({1})", (object)InternalName, ListenerId);

            if (this.RenewTimer != null)
            {
                this.RenewTimer.Dispose();
                this.RenewTimer = null;
            }

            lock (ListenActions)
            {
                ListenActions.Remove(this);
            }

            lock (this)
            {
                // TODO is this the same as this.EventActions = null;
                var all = this.EventActions?.GetInvocationList();
                all?.All(a =>
                {
                    this.EventActions -= (EventHandler)a;
                    return true;
                }
                );
            }
        }

        static bool ParseNotify(string notify, out int id)
        {
            if (notify != null && notify.StartsWith("NOTIFY /" + UPnPConfig.ListenerName + "/"))
            {
                string ids = notify.Substring(UPnPConfig.ListenerName.Length + 9);
                int trail = ids.IndexOf(' ');
                if (trail != -1)
                    return int.TryParse(ids.Substring(0, trail), out id);
            }
            id = -1;
            return false;
        }

        public string Sid { get; set; }
        public string Uri
        {
            get
            {
                if (string.IsNullOrEmpty(UPnPConfig.ListenerName))
                    throw new InvalidDataException("ListenerName not set");

                // Always return a "live" value to handle changing IP address/ListenerSocket
                return string.Format("http://{0}:{1}/{2}/{3}", Me.ToString(), (listenerSocket.LocalEndPoint as IPEndPoint).Port, UPnPConfig.ListenerName, ListenerId);
            }
        }

        public static IPAddress Me { get; internal set; }
        public static int SuspendCount { get; private set; }

        public int ListenerId { get; private set; }
        public string InternalName { get; private set; }
        public bool AutoRenew { get; set; }
        public bool Background { get { return false; } }

        private uint NextEventNumber;
        private System.Threading.Timer RenewTimer;
        private Service Parent;
        private int RequestedTimeout;               // time originally requested, in seconds
        private int Lifetime;                       // time the event was given, in seconds

        private event EventHandler EventActions;

        static private int ListenerCounter;
        static List<Listener> ListenActions = new List<Listener>();

        static public Listener Create(int seconds, string info, EventHandler callback, Service parent)
        {
            if (listenerSocket == null)
            {
                CreateListener();
            }

            // Must get the port into ListenActions before trying to Subscribe so we don't miss it
            var listener = new Listener(seconds, callback, info, parent);
            lock (ListenActions)
            {
                ListenActions.Add(listener);
            }
            return listener;
        }

        //
        // Re-use an unsubscribed event to subscribe again (when Resuming)
        private async Task ReSubscribe_Async()
        {
            this.NextEventNumber = 0;
            await this.Parent.SubscribeAsync(this.RequestedTimeout, this.InternalName, null);       // leave the callbacks alone
        }

        // Called when the app is being suspended, to stop all the subscriptions
        // But we have to leave the data intact, in case OnResume is later called
        static async public Task OnSuspendAllAsync(Action OnCompletion)
        {
            Debug.WriteLine("Listener.OnSuspend starting");

            List<Task> tasklist = new List<Task>();
            SuspendCount++;

            lock (ListenActions)
            {
                foreach (var listener in ListenActions)
                {
                    if (listener.Parent != null && !listener.Background)
                    {
                        listener.AutoRenew = false;
                        if (listener.RenewTimer != null)
                        {
                            listener.RenewTimer.Dispose();
                            listener.RenewTimer = null;
                        }
                        try
                        {
                            var task = listener.Parent.DisconnectSubscribeAsync();  // do not clean up the listener, we might need it in OnResume
                            tasklist.Add(task);
                        }
                        catch (Exception ex)
                        {
                            NetLogger.WriteLine("ERROR {0} during Listener.OnSuspendAll on {1}", ex.Message, listener.InternalName);
                        }
                    }
                }
            }

            try
            {
                await Task.WhenAll(tasklist);
            }
            catch (Exception ex)
            {
                NetLogger.WriteLine("ERROR {0} during Listener.OnSuspendAll WhenAll", ex.Message);
            }

            // Throw away the listenerSocket else we wont be able to Bind on the same socket when we resume
            try
            {
                if (listenerSocket != null)
                {
                    listenerSocket.Dispose();
                }
            }
            catch (Exception ex)
            {
                NetLogger.WriteLine("ERROR {0} during disposal of listenersocket", ex.Message);
            }
            listenerSocket = null;

            Debug.WriteLine("Listener.OnSuspend ended");
            OnCompletion();
        }

        static public void OnResumeAll()
        {
            OnResumeAllAsync().Later();
        }

        static public void Reset()
        {
            ListenActions.Clear();
        }

        static public void Update(IPAddress newaddress)
        {
            if (newaddress == null)
            {
                // Find out address while avoiding GetAllNetworkAddresses - requires internet connection though
                using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
                {
                    socket.Connect("8.8.8.8", 65530);
                    IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                    newaddress = endPoint.Address;
                    NetLogger.WriteLine($"Listener: {newaddress}");
                }
            }

            Me = newaddress;
        }

        static async private Task OnResumeAllAsync()
        {
            Debug.WriteLine("Listener.OnResume start");

            CreateListener();

            lock (ListenActions)
            {
                foreach (var listener in ListenActions)
                {
                    if (listener.Parent != null && !listener.Background)
                    {
                        listener.AutoRenew = true;
                        listener.ReSubscribe_Async().Later();
                    }
                }
            }
            Debug.WriteLine("Listener.OnResume end");

            await Task.FromResult(true);
        }
    }
}
