using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;

using PhonosAvalon.ViewModels;
using PhonosAvalon.Views;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using System.Diagnostics;
using OpenPhonos.UPnP;
using System.Collections.Generic;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Reflection;

namespace PhonosAvalon;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

        // This is where we can store app data
        string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        string name = Process.GetCurrentProcess().ProcessName;
        DataFilePath = System.IO.Path.Combine(path, name);
        Directory.CreateDirectory(DataFilePath);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Line below is needed to remove Avalonia data validation.
        // Without this line you will get duplicate validations from both Avalonia and CT
        BindingPlugins.DataValidators.RemoveAt(0);

        InitializeLogging();

        if (TryGetFeature(typeof(IActivatableLifetime)) is IActivatableLifetime activatableLifetime)
        {
            activatableLifetime.Activated += OnActivated;
            activatableLifetime.Deactivated += OnDeactivated;
        }
        else if (Platform.Instance is IActivatableLifetimeX activatableLifetimeX)
        {
            activatableLifetimeX.Activated += OnActivated;
            activatableLifetimeX.Deactivated += OnDeactivated;
        }

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainViewModel()
            };
            desktop.ShutdownRequested += Desktop_ShutdownRequested;
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MobileMainView
            {
                DataContext = new MainViewModel()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    public static ILoggerFactory? AppLoggerFactory { get; private set; }
    public string DataFilePath { get; set; }

    /// <summary>
    /// This is the version that is displayed to the user. Can be null.
    /// </summary>
    public static string DisplayVersion { get; set; }

    /// <summary>
    /// Implement this if you want to use ApplicationInsights
    /// </summary>
    /// <param name="connection"></param>
    partial void GetAppInsightsConnectionString(ref string connection);

    private void InitializeLogging()
    {
        bool debugAppInsights = false;
        string connection = string.Empty;

        GetAppInsightsConnectionString(ref connection);
        bool useAppInsights = connection != string.Empty;

        // Ref: https://learn.microsoft.com/en-us/azure/azure-monitor/app/ilogger?tabs=dotnet6#console-application

        AppLoggerFactory = LoggerFactory.Create(builder =>
        {
            var b = builder.AddDebug();

            if (useAppInsights)
            {
                b.AddApplicationInsights(
                configureTelemetryConfiguration: (config) =>
                    config.ConnectionString = connection,
                configureApplicationInsightsLoggerOptions: (options) =>
                    options.FlushOnDispose = true
                );

                Microsoft.ApplicationInsights.Extensibility.Implementation.TelemetryDebugWriter.IsTracingDisabled = !debugAppInsights;
            }
        });


        OpenPhonos.UPnP.NetLogger.LoggerFactory = AppLoggerFactory;
        var applog = AppLoggerFactory.CreateLogger("App");
        string os;
        if (OperatingSystem.IsAndroid())
        {
            os = "Android";
        }
        else if (OperatingSystem.IsIOS())
        {
            os = "iOS";
        }
        else if (OperatingSystem.IsWindows())
        {
            os = "Windows";
        }
        else if (OperatingSystem.IsMacOS())
        {
            os = "MacOS";
        }
        else if (OperatingSystem.IsLinux())
        {
            os = "Linux";
        }
        else
        {
            os = "Unknown";
        }

        var ver = App.DisplayVersion ?? Assembly.GetExecutingAssembly().GetName().Version.ToString();

        var scope = new Dictionary<string, object> {
            { "OS", os },
            { "Version", ver },
#if DEBUG
            { "Debug", true },
#endif
        };

        applog.BeginScope(scope);

        applog.LogInformation("Logging initialized");
    }

    public static void DeinitializeLogging()
    {
        if (AppLoggerFactory != null)
        {
            AppLoggerFactory.CreateLogger("App").LogInformation("Shutting down");
            AppLoggerFactory.Dispose();
            AppLoggerFactory = null;
        }
    }

    private void OnActivated(object? sender, ActivatedEventArgs e)
    {
        Debug.WriteLine($"OnActivated {e.Kind}");
        if (e.Kind == ActivationKind.Background)
        {
            AppLoggerFactory?.CreateLogger("App").LogInformation("Activated");
            Listener.OnResumeAll();
        }
    }

    private void OnDeactivated(object? sender, ActivatedEventArgs e)
    {
        Debug.WriteLine($"OnDectivated {e.Kind}");

        if (e.Kind == ActivationKind.Background)
        {
            AppLoggerFactory?.CreateLogger("App").LogInformation("Deactivated");
            var later = Listener.OnSuspendAllAsync(() =>
            {
            });
        }
    }

    private bool ShutdownRetry;

    private void Desktop_ShutdownRequested(object? sender, ShutdownRequestedEventArgs e)
    {
        if (!ShutdownRetry)
        {
            // Ideally we need a deferral, so we'll do our best by aborting this shutdown then firing another when we are done
            e.Cancel = true;
            var later = ShutdownAsync();
        }
    }

    private async Task ShutdownAsync()
    {
        await OpenPhonos.UPnP.Listener.OnSuspendAllAsync(() =>
        {
            ShutdownRetry = true;
            DeinitializeLogging();
        });

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Shutdown(0);
        }
        else
        {
            Environment.Exit(0);
        }
    }

    public void TidyExit()
    {
        _ = ShutdownAsync();
    }
}

// IActivatableLifetime is NotClientImplementable, which is a PITA so lets define a similar subset
public interface IActivatableLifetimeX
{
    //
    // Summary:
    //     An event that is raised when the application is Activated for various reasons
    //     as described by the Avalonia.Controls.ApplicationLifetimes.ActivationKind enumeration.
    event EventHandler<ActivatedEventArgs>? Activated;

    //
    // Summary:
    //     An event that is raised when the application is Deactivated for various reasons
    //     as described by the Avalonia.Controls.ApplicationLifetimes.ActivationKind enumeration.
    event EventHandler<ActivatedEventArgs>? Deactivated;
}
