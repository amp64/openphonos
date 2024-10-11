using Avalonia;
using Avalonia.iOS;

using Foundation;
using OpenPhonos.UPnP;
using PhonosAvalon;
using System.Net.Http;

namespace PhonosAvalon.iOS;

// The UIApplicationDelegate for the application. This class is responsible for launching the 
// User Interface of the application, as well as listening (and optionally responding) to 
// application events from iOS.
[Register("AppDelegate")]
public partial class AppDelegate : AvaloniaAppDelegate<App>
{
    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        UPnPConfig.HttpMessageHandler = null; // new CFNetworkHandler(); // new NSUrlSessionHandler();

        return base.CustomizeAppBuilder(builder)
            .LogToTrace()
            .WithInterFont()
            .UseiOS(this);
    }
}
