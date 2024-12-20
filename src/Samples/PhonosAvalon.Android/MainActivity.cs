using Android.App;
using Android.Content.PM;
using Android.OS;

using Avalonia;
using Avalonia.Android;

namespace PhonosAvalon.Android;

[Activity(
    Label = "PhonosAvalon.Android",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity<App>
{
    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        var context = Application?.ApplicationContext;
        App.DisplayVersion = context?.PackageManager?.GetPackageInfo(context.PackageName, 0).VersionName;
        App.OSDisplayVersion = Build.VERSION.Release;

        return base.CustomizeAppBuilder(builder)
            .WithInterFont();
    }
}
