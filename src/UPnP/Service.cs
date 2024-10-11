using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Diagnostics;
using System.Xml.Linq;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Security;

namespace OpenPhonos.UPnP
{
    public class Service
    {
        private string ControlAddress;
        private string ServiceType;
        private string EventAddress;
        private const string NSCONTROL = "urn:schemas-upnp-org:control-1-0";
        public const int RecommendedTimeout = 500;          // in milliseconds
        public const int MaximumTimeout = 30000;
        public const int ImageLoaderTimeout = 10000;
        private const int TimeoutUnsubscribe = 1000;         // in milliseconds
        private const int TimeoutSubscribe = 1000;         // in milliseconds

        public Uri BaseUri
        {
            get;
            private set;
        }

        public static bool Verbose { get; set; }

        // Every Action needs to have an instance of this information, which encapsulates all the data
        // necessary to make a call
        public struct ActionInfo
        {
            public string name;
            public string[] argnames;
            public int outargs;
        };

        // Every Action that has any OUT arguments needs to have a type inherited from this one, containing
        // each OUT argument as a member
        public class ActionResult
        {
            public Exception Error;             // Stores last error returned

            // Override this to convert an array of strings into the desired type
            public virtual void Fill(string[] rawdata)
            {
                Debug.Assert(rawdata.Length == 0);      // will fire if failed to override for non-empty results
            }


            // Helper methods
            //
            protected bool ParseBool(string item)
            {
                return item == "1";
            }

            public void ThrowIfFailed()
            {
                if (Error != null)
                {
                    Debug.WriteLine("ERROR getting {0}", (object)(this.GetType().ToString()));
                    throw Error;
                }
            }
        };

        protected Service(ServiceInfo info)
        {
            this.BaseUri = info.BaseUri;
            this.ControlAddress = info.BaseUri.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped) + info.ControlURL;
            this.EventAddress = info.BaseUri.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped) + info.EventSubURL;
            this.ServiceType = info.ServiceType;
        }

        protected Service(Service original)
        {
            this.BaseUri = original.BaseUri;
            this.ControlAddress = original.ControlAddress;
            this.EventAddress = original.EventAddress;
            this.ServiceType = original.ServiceType;
        }

        private static HttpClient _Client;

        private static HttpClient CreateHttpClientOnce()
        {
            // For some reason Lazy doesn't work on its own here?? Need the extra static
            if (_Client == null)
            {
                _Client = CreateClient(UPnPConfig.UserAgent);
            }

            return _Client;
        }

        protected async Task<ActionResult> Action_Async(ActionInfo actionInfo, object[] args, ActionResult actionResult)
        {
            actionResult.Error = null;

            try
            {
                StringBuilder soap = new StringBuilder("<u:" + actionInfo.name + " xmlns:u=\"" + ServiceType + "\">");
                int currentarg = 0;

                if (args != null)
                {
                    foreach (var arg in args)
                    {
                        soap.AppendFormat("<{0}>{1}</{0}>", actionInfo.argnames[currentarg++], FormatArg(arg));
                    }
                }
                soap.AppendFormat("</u:{0}>", actionInfo.name);

                if (Verbose)
                {
                    NetLogger.WriteLine("UPnP-Start " + soap.ToString());
                }

                var result = await SOAPRequest_Async(new Uri(ControlAddress), soap.ToString(), actionInfo.name);

                if (Verbose)
                {
                    NetLogger.WriteLine("UPnP-End " + actionInfo.name);
                }

                var error = result.Descendants(XName.Get("UPnPError", NSCONTROL)).FirstOrDefault();
                if (error != null)
                {
                    var code = error.Descendants(XName.Get("errorCode", NSCONTROL)).FirstOrDefault();
                    Debug.WriteLine("UPNPError {0} on {1}", code, actionInfo.name);
                    throw new UPnPException(code != null ? code.Value : null, actionInfo.name, ControlAddress);
                }

                if (actionInfo.outargs != 0)
                {
                    // Children of u:<VERB>Response are the result we want
                    XName xn = XName.Get(actionInfo.name + "Response", ServiceType);

                    var items = from item in result.Descendants(xn).Descendants()
                                select (string)item.Value;

                    // Check that we have at least the expected number of results
                    if (items.Count() < actionInfo.outargs)
                        throw new ArgumentException();

                    actionResult.Fill(items.ToArray());
                }
            }
            catch (Exception ex)
            {
                actionResult.Error = ex;
            }

            return actionResult;
        }

        public static HttpClient CreateClient(string useragent, bool allowRedirects = true, int timeout = MaximumTimeout)
        {
            var handler = new HttpClientHandler();
            handler.AllowAutoRedirect = allowRedirects;
            handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            NetLogger.WriteLine(NetLogger.LogType.LogVerbose, $"Creating HttpClient with handler: {UPnPConfig.HttpMessageHandler}");

            var client = new HttpClient(UPnPConfig.HttpMessageHandler ?? handler);
            client.MaxResponseContentBufferSize = 256 * 1024;
            if (useragent != null)
                client.DefaultRequestHeaders.Add("user-agent", useragent);
            client.Timeout = TimeSpan.FromMilliseconds(timeout);                    // 100 second default is way too long
            return client;
        }

        private async Task<XElement> SOAPRequest_Async(Uri target, string soap, string action)
        {
            string req = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
            "<s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\" s:encodingStyle=\"http://schemas.xmlsoap.org/soap/encoding/\">" +
            "<s:Body>" +
            soap +
            "</s:Body>" +
            "</s:Envelope>";

            var request = new HttpRequestMessage();
            request.Method = HttpMethod.Post;
            request.Content = new StringContent(req, Encoding.UTF8, "text/xml");
            request.Content.Headers.Add("SOAPACTION", "\"" + ServiceType + "#" + action + "\"");
            // "Content-Length" is set for us
            request.RequestUri = target;

            var resp = await CreateHttpClientOnce().SendAsync(request);
            // resp.Content.ReadAsStringAsync() will fail here for some mysterious reason so read as a stream instead
            var st = await resp.Content.ReadAsStreamAsync();
            return XElement.Load(st);
        }

        private Listener eventListener;

        public async Task SubscribeAsync(int seconds, string info, Listener.EventHandler callback)
        {
            if (seconds < Listener.MinimumTimeout)
                throw new InvalidDataException("Subscription");

            if (eventListener == null)
            {
                eventListener = Listener.Create(seconds, info, callback, this);
            }
            else if (callback != null)
            {
                eventListener.AddSubscription(callback);
            }

            if (!IsSubscribed)
            {
                try
                {
                    var resp = await SendSubscription_Async(seconds, new Uri(EventAddress), null, info, Listener.SubscribeAfterFailureCount);

                    if (resp.IsSuccessStatusCode)
                    {
                        eventListener.Sid = resp.Headers.GetValues("SID").First();
                        eventListener.SetTimeout(resp.Headers.GetValues("TIMEOUT").First());

                        Debug.WriteLine("Subscription SID={0} for {1} ({2})", eventListener.Sid, info, eventListener.ListenerId);
                    }
                    else
                    {
                        NetLogger.WriteCRITICAL("Failed to subscribe {0} due to {1}", info, resp.StatusCode);
                    }
                }
                catch (Exception)
                {
                    NetLogger.WriteCRITICAL("Failed to subscribe {0} on {1}", info, BaseUri.Host);
                }
            }
        }

        public bool IsSubscribed
        {
            get
            {
                return eventListener != null && eventListener.IsSubscribed;
            }
        }

        public async Task<bool> RenewSubscriptionAsync(int seconds)
        {
            Uri host = new Uri(EventAddress);
            Exception last = null;
            Listener listener = eventListener;

            for (int i = 0; i < Listener.RenewAfterFailureCount; i++)
            {
                try
                {
                    var resp = await SendSubscription_Async(seconds, host, listener.Sid, listener.InternalName, Listener.RenewAfterFailureCount);

                    // In case made Unwanted during this function
                    if (eventListener == null)
                    {
                        Debug.WriteLine("Renewal cancelled for {0}", (object)listener.InternalName);
                        return false;
                    }

                    IEnumerable<string> values;
                    if (resp.Headers.TryGetValues("TIMEOUT", out values))
                    {
                        listener.SetTimeout(values.First());
                        Debug.WriteLine("Renewal ok for {0}", (object)listener.InternalName);
                        return true;
                    }
                    else
                    {
                        NetLogger.WriteLine("Renewal for {0} weird {1}", listener.InternalName, resp.StatusCode);
                        last = new Exception(resp.ReasonPhrase);
                    }
                }
                catch (Exception ex)
                {
                    NetLogger.WriteLine("{0} during Renewal on {1} {2}", ex.Message, listener.InternalName, EventAddress);
                    last = ex;
                }
            }

            Listener.CriticalError(string.Format("Renewal failed on {0} {1} due to {2}", listener.InternalName, listener.ListenerId, last == null ? "gave-up" : last.Message));
            return false;
        }

        private static HttpClient SubscriberClient = CreateSubscriberClient(TimeoutSubscribe);

        private static HttpClient CreateSubscriberClient(int timeout)
        {
            HttpClient r = new HttpClient();
            r.MaxResponseContentBufferSize = 65536;
            r.Timeout = TimeSpan.FromMilliseconds(timeout);

            r.DefaultRequestHeaders.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
            r.DefaultRequestHeaders.CacheControl.NoCache = true;

            r.DefaultRequestHeaders.ConnectionClose = true;
            r.DefaultRequestHeaders.Add("useragent", UPnPConfig.UserAgent);
            return r;
        }

        private async Task<HttpResponseMessage> SendSubscription_Async(int seconds, Uri host, string sid, string name, int retrycount)
        {
            Exception last = null;

            for (int i = 0; i < retrycount; i++)
            {
                try
                {
                    var resp = await SendSubscriptionOnce_Async(seconds, host, sid);
                    if (i == 0)
                    {
                        NetLogger.WriteLine(NetLogger.LogType.LogVerbose, "Subscribed first time {0}", name);
                    }
                    else
                    {
                        NetLogger.WriteLine(NetLogger.LogType.LogVerbose, "Subscribed after {0} tries {1}", i, name);
                    }
                    return resp;
                }
                catch (Exception ex)
                {
                    last = ex;
                }

                // Wait a bit before trying again
                await Task.Delay(Listener.RenewAfterFailureTimeout);
            }

            NetLogger.WriteLine("Subsciption failed {0} after {2} due to {1}", name, last, retrycount);
            throw last;
        }

        // This isnt terribly reliable, for some reason, so always use retry semantics
        private async Task<HttpResponseMessage> SendSubscriptionOnce_Async(int seconds, Uri host, string sid)
        {
            var msgreq = new HttpRequestMessage(new HttpMethod("SUBSCRIBE"), host);
            if (sid == null)
            {
                msgreq.Headers.Add("NT", "upnp:event");
                msgreq.Headers.Add("CALLBACK", "<" + eventListener.Uri + ">");
            }
            else
            {
                msgreq.Headers.Add("SID", sid);
            }
            msgreq.Headers.Add("TIMEOUT", "Second-" + seconds.ToString());
            return await SubscriberClient.SendAsync(msgreq);
        }

        private static HttpClient UnsubscriberClient = CreateSubscriberClient(TimeoutUnsubscribe);

        // tells Service to unsubscribe, but keeps all callbacks intact
        internal async Task DisconnectSubscribeAsync()
        {
            var ok = await UnsubscribeInternal_Async();
            if (!ok)
            {
                ok = await UnsubscribeInternal_Async();
                if (!ok)
                {
                    Debug.WriteLine("FAILED Unsubscribe on {0}", (object)EventAddress);
                }
            }
            Debug.WriteLine("Disconnected on {0} ({1})", eventListener.InternalName, eventListener.ListenerId);
        }

        public async Task UnsubscribeAsync(Listener.EventHandler callback)
        {
            Debug.Assert(callback != null);

            if (eventListener != null)
            {
                eventListener.RemoveSubscription(callback);

                // If we are the last subscription then we don't need the listener any more
                bool cleanuplistener = !eventListener.HasSubscriptions;
                if (cleanuplistener)
                {
                    await DisconnectSubscribeAsync();

                    eventListener?.Unwanted();
                    eventListener = null;
                }
                else
                {
                    Debug.WriteLine("Unsubscribe on {0} ({1}) leaving {2} callbacks", eventListener.InternalName, eventListener.ListenerId, eventListener.SubscriptionCount);
                }
            }
        }

        private async Task<bool> UnsubscribeInternal_Async()
        {
            //r.DefaultRequestHeaders.Add("HOST", eventListener.Uri);
            var msgreq = new HttpRequestMessage(new HttpMethod("UNSUBSCRIBE"), new Uri(EventAddress));
            msgreq.Headers.Add("SID", eventListener.Sid);

            try
            {
                var resp = await UnsubscriberClient.SendAsync(msgreq);
                return true;
            }
            catch (Exception ex)
            {
                NetLogger.WriteLine("{0} during Unsubscribe on {1}", ex.Message, EventAddress);
                return false;
            }
        }

        private string FormatArg(object obj)
        {
            string result;

            if (obj == null)
                result = "";
            else if (obj is string)
                result = SecurityElement.Escape(obj.ToString());
            else if (obj is bool)
                result = (bool)(obj) ? "1" : "0";
            else
                result = Convert.ChangeType(obj, obj.GetType(), CultureInfo.InvariantCulture).ToString();

            return result;
        }
    }

    static public class ServiceExtensions
    {
        /// <summary>
        /// This is a helper to add to calls so you don't have to explicitly call ThrowIfFailed on the result of a Service call
        /// So to replace:
        /// var foo = await service.Function(args);
        /// foo.ThrowIfFailed();
        /// with:
        /// var foo = await service.Function(args).Required();
        /// </summary>
        /// <typeparam name="T">Anything that inherits from ActionResult</typeparam>
        /// <param name="result"></param>
        /// <returns>The original ActionResult, unless the call failed when it will throw</returns>
        static public async Task<T> Required<T>(this Task<T> result) where T : Service.ActionResult
        {
            var finished = await result;
            finished.ThrowIfFailed();
            return finished;
        }

        /// <summary>
        /// This is the opposite of Required, this doesn't have any affect other than to make it clear in the code that the service call doesn't have to succeed
        /// </summary>
        /// <typeparam name="T">Anything that inherits from ActionResult</typeparam>
        /// <param name="result"></param>
        /// <returns>The original ActionResult</returns>
        static public Task<T> Optional<T>(this Task<T> result) where T : Service.ActionResult
        {
            return result;
        }
    }
}

