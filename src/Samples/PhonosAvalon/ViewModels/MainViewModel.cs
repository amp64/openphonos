using OpenPhonos.Sonos;
using OpenPhonos.UPnP;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Windows.Input;
using Avalonia.Threading;
using System.Web;
using System.Xml;
using System.IO;
using Avalonia;
using Avalonia.Platform;
using System.Collections.Generic;
using System.Reflection;

namespace PhonosAvalon.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    partial void GetAppDetails();
    partial void AfterNetworkScan(bool success);

    public MainViewModel()
    {
        ArtSize = 200;

        ZoneList = new ObservableCollection<GroupViewModel>();

        VolumePopupCommand = new ActionCommand((o) => VolumePopupOpen = true);
        GroupEditorCommand = new ActionCommand((o) => GroupEditorOpen = true);
        QueuePopupCommand = new ActionCommand((o) => QueuePopupOpen = true);
        StartupFailedCommand = new ActionCommand(OnStartupFailed);
        AboutCommand = new ActionCommand((o) => AboutPopupOpen = true);
        FeedbackCommand = new ActionCommand(OnFeedback);

        AppVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        AppName = "Open Phonos";
        Copyright = "Copyright";
        HelpEmailAddress = "openphonos@example.com";

        GetAppDetails();

        var later = StartupAsync();
    }

    private void OnFeedback(object? obj)
    {
        try
        {
            var info = new ProcessStartInfo()
            {
                FileName = $"mailto:{HelpEmailAddress}?subject={Uri.EscapeDataString(AppName + " Feedback")}",
                UseShellExecute = true,
            };
            Process.Start(info);
        }
        catch
        {
        }
    }

    private double _ArtSize;
    public double ArtSize
    {
        get => _ArtSize;
        set => SetProperty(ref _ArtSize, value);
    }

    private Household.HouseholdResult? AllHouseholds;

    public ObservableCollection<GroupViewModel> ZoneList { get; private set; }

    private GroupViewModel? _CurrentZone;
    public GroupViewModel? CurrentZone
    {
        get => _CurrentZone;
        set
        {
            if (SetProperty(ref _CurrentZone, value) && value is not FakeGroupViewModel)
            {
                Settings.Instance.CurrentZoneId = value?.Coordinator?.UniqueName;
            }
        }
    }

    public ICommand VolumePopupCommand { get; private set; }

    private bool _VolumePopupOpen;
    public bool VolumePopupOpen
    {
        get => _VolumePopupOpen;
        set => SetProperty(ref _VolumePopupOpen, value);
    }

    public ICommand GroupEditorCommand { get; private set; }

    private bool _GroupEditorOpen;
    public bool GroupEditorOpen
    {
        get => _GroupEditorOpen;
        set => SetProperty(ref _GroupEditorOpen, value);
    }

    public ICommand QueuePopupCommand { get; private set; }

    private bool _QueuePopupOpen;
    public bool QueuePopupOpen
    {
        get => _QueuePopupOpen;
        set => SetProperty(ref _QueuePopupOpen, value);
    }

    public ICommand StartupFailedCommand { get; private set; }

    public ICommand AboutCommand { get; private set; }
    public ICommand FeedbackCommand { get; private set; }

    private bool _AboutPopupOpen;
    public bool AboutPopupOpen
    {
        get => _AboutPopupOpen;
        set => SetProperty(ref _AboutPopupOpen, value);
    }

    private bool _StartupPopupOpen;
    public bool StartupPopupOpen
    {
        get => _StartupPopupOpen;
        set => SetProperty(ref _StartupPopupOpen, value);
    }

    private string? _StartupMessage;
    public string? StartupMessage
    {
        get => _StartupMessage;
        set => SetProperty(ref _StartupMessage, value);
    }

    private bool _StartupSearching;
    public bool StartupSearching
    {
        get => _StartupSearching;
        set => SetProperty(ref _StartupSearching, value);
    }

    public Household GetHouseholdFromZone(GroupViewModel zone)
    {
        var h = AllHouseholds.Households.FirstOrDefault(h => h.Groups.FirstOrDefault(g => zone.IsThisGroup(g)) != null);
        return h;
    }

    virtual protected async Task StartupAsync()
    {
        if (OpenPhonos.UPnP.Platform.Instance is Platform)
        {
            OpenPhonos.UPnP.Platform.Instance = new AvaloniaPlatform();
        }

        UPnPConfig.ListenerName = "OpenPhonos.2023";
        UPnPConfig.UserAgent = "Linux UPnP/1.0 Sonos/80.1-55014 (WDCR:Microsoft Windows NT 10.0.19045 64-bit)";

        CurrentZone = new FakeGroupViewModel("Starting up...");

        await FindAndConfigureAsync();
    }

    private async Task<bool> FindAndConfigureAsync()
    {
        StartupPopupOpen = true;
        StartupSearching = true;
        int foundSoFar = 0;

        AllHouseholds = await Household.FindAllHouseholdsAndPlayersAsync((player, other) =>
        {
            // Status update here as players are found
            foundSoFar++;
            string msg = $"Found: {foundSoFar}...";
            Dispatcher.UIThread.Post(() =>
            {
                if (foundSoFar > 0)
                {
                    // Don't update this if we have already exited the FindAllHouseholdsAndPlayersAsync
                    StartupMessage = msg;
                }
            });
        });

        foundSoFar = -1;
        StartupMessage = "Calculating Groups...";

        if (string.IsNullOrEmpty(Settings.Instance.ControllerId))
        {
            Settings.Instance.ControllerId = Guid.NewGuid().ToString();
        }

        foreach (var h in AllHouseholds.Households)
        {
            if (h.AssociatedZP != null)
            {
                Debug.WriteLine($"Building HH {h.HouseholdId}");

                await h.BuildGroupsAsync();

                Debug.WriteLine($"Initializing MS via {h.AssociatedZP.DisplayName}");
                h.MusicServices = new MusicServiceProvider();
                await h.MusicServices.InitializeAsync(h.AssociatedZP);
                MusicService.ControllerId = Settings.Instance.ControllerId;

                Debug.WriteLine($"Adding {h.Groups.Count} groups");

                foreach (var g in h.Groups)
                {
                    var vm = new GroupViewModel(g, h);
                    await vm.SubscribeAsync();
                    ZoneList.Add(vm);
                }

                h.Groups.CollectionChanged += OnGroupListChanged;
                await h.SubscribeAsync();
                await InitMusicServicesAsync(h.AssociatedZP, h.MusicServices);
            }
        }

        Debug.WriteLine($"All HH done, setting Zone from {ZoneList.Count}");
        StartupSearching = false;

        AfterNetworkScan(ZoneList.Count > 0);

        if (ZoneList.Count > 0)
        {
            StartupPopupOpen = false;
            var z = ZoneList.FirstOrDefault(z => z.Coordinator?.UniqueName == Settings.Instance.CurrentZoneId);
            CurrentZone = z ?? ZoneList[0];

            string? previousVersion = Settings.Instance.LastVersion;
            if (previousVersion != AppVersion)
            {
                Settings.Instance.LastVersion = AppVersion;
                AboutPopupOpen = true;
            }

            return true;
        }
        else
        {
            StartupMessage = "No devices found. " + NetLogger.LikelyProblem ?? string.Empty;
            NetLogger.WriteLine("BAD: No zones found");
            return false;
        }
    }

    private async void OnGroupListChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        Household h = AllHouseholds.Households.First(hh => sender.Equals(hh.Groups));

        Debug.WriteLine($"Group changed: {e.Action} in {h.HouseholdId}");

        if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null && e.NewItems.Count != 0)
        {
            foreach (var item in e.NewItems)
            {
                var g = item as Group;
                var vm = new GroupViewModel(g, h);
                await vm.SubscribeAsync();
                ZoneList.Add(vm);
            }
        }
        else if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems != null && e.OldItems.Count != 0)
        {
            bool currentGone = false;

            foreach (var item in e.OldItems)
            {
                var g = item as Group;
                var vm = ZoneList.First(v => v.IsThisGroup(g));
                currentGone |= vm == CurrentZone;
                ZoneList.Remove(vm);
                await vm.UnsubscribeAsync();
            }

            if (currentGone)
            {
                // TODO Would be better to switch to the zone that contains the old current zone coordinator
                CurrentZone = ZoneList[0];
            }
        }
        else
        {
            int x = 4;
        }
    }

    private async Task InitMusicServicesAsync(Player player, MusicServiceProvider provider)
    {
        WaitableEvent credentialsEvent = new WaitableEvent();
        string credentials = null;
        var which = new PlayerEventSelector() { Zone = true };

        player.OnThirdPartyMediaServers = (uri, servers) =>
        {
            if (credentials == null)
            {
                credentials = servers;
            }
            credentialsEvent.Set();
            return Task.FromResult(true);
        };

        await player.SubscribeToEventsAsync(OpenPhonos.UPnP.Listener.MinimumTimeout, which);
        await credentialsEvent.WaitAsync(Timeout.Infinite);                                 // TODO should not wait forever
        await player.UnsubscribeToEventsAsync(which);
        await provider.RefreshAsync(credentials);
    }

    private void OnStartupFailed(object? arg)
    {
        bool quit = true;
        StartupPopupOpen = false;

        switch (arg as string)
        {
            case "Q":
                break;
            case "R":
                quit = false;
                var later = FindAndConfigureAsync();
                break;
            case "H":
                try
                {
                    var info = new ProcessStartInfo()
                    {
                        FileName = $"mailto:{HelpEmailAddress}?subject={Uri.EscapeDataString(AppName + " Help")}&body={Uri.EscapeDataString(HelpBody())}",
                        UseShellExecute = true,
                    };
                    Process.Start(info);
                }
                catch
                {
                    quit = false;
                }
                break;
            case "D":
                _ = CreateDemoSystemAsync();
                quit = false;
                break;
        }

        if (quit)
        {
            (App.Current as App).TidyExit();
        }

        string HelpBody()
        {
            var items = new List<string>()
            {
                "Version: " + AppVersion,
                "Platform: " + OpenPhonos.UPnP.Platform.Instance.FullPlatformName(),
                "Diagnostic: " + NetLogger.ScopeId,
                NetLogger.LikelyProblem ?? string.Empty,
            };

            return string.Join(Environment.NewLine, items);
        }
    }

    public string AppVersion { get; private set; }
    public string AppName { get; private set; }
    public string Copyright { get; private set; }
    public string HelpEmailAddress { get; private set; }

    private async Task CreateDemoSystemAsync()
    {
        using StreamReader streamReader = new(AssetLoader.Open(new Uri("avares://PhonosAvalon/Assets/demozone.xml")));
        var xml = streamReader.ReadToEnd();
        AllHouseholds = await Household.CreateTestHouseholdAsync(xml, (p, q) => { });

        if (string.IsNullOrEmpty(Settings.Instance.ControllerId))
        {
            Settings.Instance.ControllerId = Guid.NewGuid().ToString();
        }

        var household = AllHouseholds.Households[0];
        household.MusicServices = new MusicServiceProvider();
        MusicService.ControllerId = Settings.Instance.ControllerId;

        foreach (var g in household.Groups)
        {
            var gvm = new GroupViewModel(g, household);
            ZoneList.Add(gvm);
            gvm.FillWithFakes();
        }

        CurrentZone = ZoneList[0];
    }
}

public class FakeMainViewModel : MainViewModel
{
    public FakeMainViewModel()
    {
    }

    protected override Task StartupAsync()
    {
        ZoneList.Add(new FakeGroupViewModel());

        CurrentZone = ZoneList[0];

        return Task.FromResult(0);
    }
}

internal class ActionCommand : ICommand
{
    public event EventHandler? CanExecuteChanged;

    private Action<object?> Action;

    public ActionCommand(Action<object?> action)
    {
        Action = action;
    }

    public bool CanExecute(object? parameter)
    {
        return true;
    }

    public void Execute(object? parameter)
    {
        Action.Invoke(parameter);
    }
}
