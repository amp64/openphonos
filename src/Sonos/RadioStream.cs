using OpenPhonos.UPnP;
using System;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace OpenPhonos.Sonos
{
    /// <summary>
    /// Utilities to assist in direct radio streams
    /// </summary>
    public class RadioStream
    {
        public const string TestStreamName = "Radio Stream Test";

        public string FailureReason { get; private set; }

        private readonly string Stream;
        private readonly WaitableEvent CompletedEvent;

        public RadioStream(string stream)
        {
            Stream = stream;
            CompletedEvent = new WaitableEvent();
        }

        private const string StreamPrefix = @"x-rincon-mp3radio://";

        private static string GetRadioMd(string name, string url)
        {
            return @"<DIDL-Lite  xmlns:dc=""http://purl.org/dc/elements/1.1/"" xmlns:upnp=""urn:schemas-upnp-org:metadata-1-0/upnp/"" xmlns=""urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/"">
<item id="""" restricted=""false"">
<dc:title>" + SecurityElement.Escape(name) + @"</dc:title>
<res>" + SecurityElement.Escape(url) + @"</res>
</item>
</DIDL-Lite>";
        }

        /// <summary>
        /// See if we can use a stream
        /// </summary>
        /// <param name="player">Player to test with, should be a Coordinator</param>
        /// <returns>True if worked, False if failed</returns>
        public async Task<bool> TestStreamAsync(Player player)
        {
            bool succeeded = false;
            FailureReason = null;
            string stream =  StreamPrefix + Stream;

            try
            {
                if (!player.IsRealPlayer)
                    throw new NotSupportedException();

                var stop = await player.AVTransport.Stop(0);
                stop.ThrowIfFailed();
                await player.AVTransport.SubscribeAsync(OpenPhonos.UPnP.Listener.MinimumTimeout, "AVTTest", AVTEventHandler);
                var result = await player.AVTransport.SetAVTransportURI(0, stream, GetRadioMd(TestStreamName, stream));
                result.ThrowIfFailed();
                var play = await player.AVTransport.Play(0, "1");
                play.ThrowIfFailed();
                // Takes at least 5 seconds to report bad streams
                await CompletedEvent.WaitAsync(20000);
                succeeded = FailureReason == null;
            }
            catch (Exception ex)
            {
                FailureReason = ex.Message;
            }
            finally
            {
                await player.AVTransport.UnsubscribeAsync(AVTEventHandler);
            }

            if (player.IsRealPlayer)
            {
                await player.AVTransport.Stop(0).Optional();
            }

            return succeeded;
        }

        public async Task AddStreamAsync(Player player, string name)
        {
            if (!player.IsRealPlayer)
                throw new NotSupportedException();

            var create = await player.ContentDirectory.CreateObject("R:0/0", GetRadioMd(name, StreamPrefix + Stream));
            create.ThrowIfFailed();
        }

        /// <summary>
        /// For a successful stream, TransportState goes TRANSITIONING->PLAYING
        /// For a bad stream content, TransportState goes TRANSITIONING->STOPPED with TransportStatus=ERROR_
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private Task AVTEventHandler(Service sender, EventSubscriptionArgs args)
        {
            const string NS = "urn:schemas-upnp-org:metadata-1-0/AVT/";

            if (args.Items.TryGetValue("LastChange", out string status))
            {
                var xml = XElement.Parse(status);

                Func<string, string, string> get = delegate (string name, string ns)
                {
                    var element = xml.Descendants(XName.Get(name, ns)).FirstOrDefault();
                    if (element == null)
                        return null;
                    return (string)element.Attribute(XName.Get("val")).Value;
                };

                string transportState = get("TransportState", NS);
                string transportStatus = get("TransportStatus", NS);

                System.Diagnostics.Debug.WriteLine("TransportState = {0}", (object)transportState);
                System.Diagnostics.Debug.WriteLine("TransportStatus = {0}", (object)transportStatus);

                if (transportState == "PLAYING")
                {
                    // Succeeded
                    CompletedEvent.Set();
                }
                else if (transportState == "STOPPED")
                {
                    if (transportStatus != null && transportStatus.StartsWith("ERROR_"))
                    {
                        FailureReason = transportStatus;
                        CompletedEvent.Set();
                    }
                }
            }

            return Task.FromResult(false);
        }
    }
}
