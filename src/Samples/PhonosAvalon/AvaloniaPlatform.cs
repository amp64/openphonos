
using Avalonia.Threading;
using OpenPhonos.UPnP;
using System;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace PhonosAvalon
{
    internal class AvaloniaPlatform : IPlatform
    {
        public string DeviceName()
        {
            return "Avalonia";
        }

        public string FindNetworkName(NetworkInterface network)
        {
            return network.Name;
        }

        public string FullPlatformName()
        {
            return "Avanonia.Real";
        }

        public bool IsAlwaysWiFi()
        {
            return false;
        }

        public string Name()
        {
            return "Name";
        }

        public void OnUIThread(Func<Task> uiAction)
        {
            Dispatcher.UIThread.Post(async () => await uiAction.Invoke());
        }
    }
}
