using System;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace OpenPhonos.UPnP
{
    public interface IPlatform
    {
        /// <summary>
        /// Does the device only support WiFi networks (eg a Phone)
        /// </summary>
        /// <returns></returns>
        bool IsAlwaysWiFi();

        /// <summary>
        /// The name of the device
        /// </summary>
        /// <returns></returns>
        string DeviceName();

        /// <summary>
        /// The name of the Platform
        /// </summary>
        /// <returns></returns>
        string Name();

        /// <summary>
        /// The name of the Platform including version etc
        /// </summary>
        /// <returns></returns>
        string FullPlatformName();

        /// <summary>
        /// Extract a "nice" name given a network adaptor
        /// </summary>
        /// <param name="network"></param>
        /// <returns></returns>
        string FindNetworkName(NetworkInterface network);

        /// <summary>
        /// Run an Action on the UI thread (if there is one)
        /// </summary>
        /// <param name="uiAction"></param>
        void OnUIThread(Func<Task> uiAction);

        /// <summary>
        /// Is this network going to work? If not, return true and an error message, if maybe, return false and a warning message
        /// </summary>
        bool IsUsableNetwork(NetworkInterface network, out string error);
    }
}
