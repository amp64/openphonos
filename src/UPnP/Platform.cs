using System;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace OpenPhonos.UPnP
{
    /// <summary>
    /// This is the default implementation
    /// </summary>
    public class Platform : IPlatform
    {
        public static IPlatform Instance
        {
            get;
            set;
        } = new Platform();

        public bool IsAlwaysWiFi()
        {
            return false;
        }

        public string DeviceName()
        {
            return "DEFAULTDEVICENAME";
        }

        public string Name()
        {
            return "DEFAULTPLATFORMNAME";
        }

        public string FullPlatformName()
        {
            return "FULLPLATFORMNAME";
        }

        public string FindNetworkName(NetworkInterface network)
        {
            return network.Description;
        }

        /// <summary>
        /// IMPORTANT: The default implementation of this does NOT run on another thread
        /// </summary>
        /// <param name="uiAction"></param>
        public void OnUIThread(Func<Task> uiAction)
        {
            uiAction();
        }
    }
}
