using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace OpenPhonos.UPnP
{
    public static class UPnPConfig
    {
        public static string ListenerName { get; set; } = "OpenPhonos";

        /// <summary>
        /// This is the UserAgent string passed on every Service/UPnP call
        /// Default is "NetStandard/2.0 UPnP/1.1 OpenPhonos/1.0"
        /// </summary>
        public static string UserAgent { get; set; } = "NetStandard/2.0 UPnP/1.1 OpenPhonos/1.0";

        /// <summary>
        /// This is so platforms can override this if necessary
        /// </summary>
        public static HttpMessageHandler HttpMessageHandler { get; set; }
    }
}
