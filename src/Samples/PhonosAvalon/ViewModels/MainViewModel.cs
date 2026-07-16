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
using Microsoft.Extensions.Logging;

namespace PhonosAvalon.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    partial void GetAppDetails();
    partial void BeforeNetworkScan(ref Task? task);
    partial void AfterNetworkScan(bool success, string type, TimeSpan duration, ref Task? task);

    public MainViewModel()
    {
        ArtSize = 200;

        ZoneList = new SortedObservableCollection<GroupViewModel>();

        VolumePopupCommand = new ActionCommand((o) => VolumePopupOpen = true);
        GroupEditorCommand = new ActionCommand((o) => GroupEditorOpen = true);
        QueuePopupCommand = new ActionCommand((o) => QueuePopupOpen = true);
        StartupFailedCommand = new ActionCommand(OnStartupFailed);
        AboutCommand = new ActionCommand((o) => AboutPopupOpen = true);
        FeedbackCommand = new ActionCommand(OnFeedback);
        DevicesPopupCommand = new ActionCommand((o) => DevicesPopupOpen = true);
        RadioPopupCommand = new ActionCommand((o) => RadioPopupOpen = true);
        SettingsPopupCommand = new ActionCommand((o) => SettingsPopupOpen = true);
        Message = new MessageViewModel();

        AppVersion = App.DisplayVersion ?? Assembly.GetExecutingAssembly().GetName().Version.ToString();
        AppName = "Open Phonos";
        Copyright = "Copyright";
        HelpEmailAddress = "openphonos@example.com";

        GetAppDetails();

        // It is critical to call StartupAsync later to get things started
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

    public bool OnBackPressed()
    {
        if (CloseAllPopups())
        {
        }
        else if (CurrentZone?.MusicSource != null)
        {
            if (CurrentZone.MusicSource.BackCommand.CanExecute(null))
            {
                CurrentZone.MusicSource.BackCommand.Execute(null);
            }
        }
        else
        {
            return false;
        }

        return true;
    }

    internal bool CloseAllPopups()
    {
        if (AboutPopupOpen)
        {
            AboutPopupOpen = false;
        }
        else if (VolumePopupOpen)
        {
            VolumePopupOpen = false;
        }
        else if (GroupEditorOpen)
        {
            GroupEditorOpen = false;
        }
        else if (QueuePopupOpen)
        {
            QueuePopupOpen = false;
        }
        else if (DevicesPopupOpen)
        {
            DevicesPopupOpen = false;
        }
        else if (MessagePopupOpen)
        {
            MessagePopupOpen = false;
        }
        else if (RadioPopupOpen)
        {
            RadioPopupOpen = false;
        }
        else if (SettingsPopupOpen)
        {
            SettingsPopupOpen = false;
        }
        else if (CurrentZone != null && CurrentZone.PlaylistPopupOpen)
        {
            CurrentZone.PlaylistPopupOpen = false;
        }
        else
        {
            return false;
        }

        return true;
    }

    private double _ArtSize;
    public double ArtSize
    {
        get => _ArtSize;
        set => SetProperty(ref _ArtSize, value);
    }

    private Household.HouseholdResult? AllHouseholds;
    public IReadOnlyList<Household> GetHouseholds() => AllHouseholds.Households;

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

    private AddRadioViewModel? _RadioViewModel;
    public AddRadioViewModel? RadioViewModel
    {
        get => _RadioViewModel;
        set => SetProperty(ref _RadioViewModel, value);
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

    public ICommand RadioPopupCommand { get; private set; }

    private bool _RadioPopupOpen;
    public bool RadioPopupOpen
    {
        get => _RadioPopupOpen;
        set => SetProperty(ref _RadioPopupOpen, value);
    }

    public ICommand DevicesPopupCommand { get; private set; }
    
    private bool _DevicesPopupOpen;
    public bool DevicesPopupOpen
    {
        get => _DevicesPopupOpen;
        set => SetProperty(ref _DevicesPopupOpen, value);
    }

    private bool _MessagePopupOpen;
    public bool MessagePopupOpen
    {
        get => _MessagePopupOpen;
        set => SetProperty(ref _MessagePopupOpen, value);
    }

    public MessageViewModel Message { get; private set; }

    public ICommand StartupFailedCommand { get; private set; }

    public ICommand AboutCommand { get; private set; }
    public ICommand FeedbackCommand { get; private set; }
    public ICommand SettingsPopupCommand { get; private set; }

    private bool _AboutPopupOpen;
    public bool AboutPopupOpen
    {
        get => _AboutPopupOpen;
        set => SetProperty(ref _AboutPopupOpen, value);
    }

    private bool _SettingsPopupOpen;
    public bool SettingsPopupOpen
    {
        get => _SettingsPopupOpen;
        set => SetProperty(ref _SettingsPopupOpen, value);
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
    /// <summary>
    /// True when actively searching the network
    /// </summary>
    public bool StartupSearching
    {
        get => _StartupSearching;
        set => SetProperty(ref _StartupSearching, value);
    }

    private bool _StartupFailed;
    public bool StartupFailed
    {
        get => _StartupFailed;
        set => SetProperty(ref _StartupFailed, value);
    }

    public int Theme
    {
        get => Settings.Instance.Theme;
        set
        {
            int t = Settings.Instance.Theme;
            if (SetProperty(ref t, value))
            {
                Settings.Instance.Theme = value;
                (App.Current as App).UpdateTheme();
            }
        }
    }

    private bool _large;
    public bool LargeIcons
    {
        get => _large;
        set
        {
            if (SetProperty(ref _large, value))
            {
                (App.Current as App).UpdateLargeIcons(value);
            }
        }
    }

    public Household GetHouseholdFromZone(GroupViewModel zone)
    {
        var h = AllHouseholds.Households.FirstOrDefault(h => h.Groups.FirstOrDefault(g => zone.IsThisGroup(g)) != null);
        return h;
    }

    virtual public async Task StartupAsync()
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
        Task? before = null;
        BeforeNetworkScan(ref before);
        if (before !=null)
        {
            await before; 
        }

        StartupPopupOpen = true;
        StartupSearching = true;
        int foundSoFar = 0;

        string [] hosts = (Settings.Instance.Hosts ?? string.Empty).Split(',');
        bool faster = hosts[0].Length != 0;
        var stopWatch = Stopwatch.StartNew();

        if (faster)
        {
            NetLogger.WriteLine("Trying HHosts: {0}", Settings.Instance.Hosts);

            StartupMessage = "Accelerated network scan...";
            AllHouseholds = await Household.FindHouseholdFromPlayers(hosts);
            if (AllHouseholds == null)
            {
                faster = false;
            }
        }

        if (!faster)
        {
            Settings.Instance.Hosts = string.Empty;
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
        }

        StartupMessage = "Calculating Groups...";
        foundSoFar = -1;
        var logger = App.AppLoggerFactory!.CreateLogger("App");

        if (string.IsNullOrEmpty(Settings.Instance.ControllerId))
        {
            Settings.Instance.ControllerId = Guid.NewGuid().ToString();
        }
        MusicService.ControllerId = Settings.Instance.ControllerId;

        var htasks = new List<Task>();
        int hhIndex = 0;

        foreach (var h in AllHouseholds.Households)
        {
            var htask = BuildHouseholdAsync(hhIndex++, h, faster);
            htasks.Add(htask);
        }

        try
        {
            await Task.WhenAll(htasks);
        }
        catch (Exception ex)
        {
            logger.LogCritical("BuildHouseholdAsync2 {exception}", ex.Message);
            NetLogger.LikelyProblem = ex.Message;
        }

        Debug.WriteLine($"All HH done, setting Zone from {ZoneList.Count}");
        StartupSearching = false;
        stopWatch.Stop();

        Task? work = null;
        AfterNetworkScan(ZoneList.Count > 0, faster ? "fast" : "normal", stopWatch.Elapsed, ref work);
        if (work != null)
        {
            await work;
        }

        if (ZoneList.Count > 0)
        {
            StartupPopupOpen = false;
            var z = ZoneList.FirstOrDefault(z => z.Coordinator?.UniqueName == Settings.Instance.CurrentZoneId);
            CurrentZone = z ?? ZoneList[0];

            if (AllHouseholds.CacheResults)
            {
                var updatedHosts = AllHouseholds.Households.Select(hh => hh.AssociatedZP.IPAddress);
                Settings.Instance.Hosts = string.Join(',', updatedHosts);
                NetLogger.WriteLine("Saving hosts: {0}", Settings.Instance.Hosts);
            }
            else
            {
                NetLogger.WriteLine("Clearing hosts");
                Settings.Instance.Hosts = string.Empty;
            }

            if (faster)
            {
                // Do the current zone first as its the one on screen when we start
                NetLogger.WriteLine($"Group Subscribing {CurrentZone.Summary}");
                var first = CurrentZone;

                // This kicks off the AVT subscription, which we need to populate the main screen
                var refill = first.NowPlaying;
                await first.SubscribeAsync();

                // Do the other Group subscriptions on other another thread else it severely slows down startup
                var vms = ZoneList.ToList();
                vms.Remove(first);

                var later = Task.Run(async () =>
                {
                    // Give the main Zone a chance
                    await Task.Delay(500);
                    foreach (var vm in vms)
                    {
                        NetLogger.WriteLine($"Group Subscribing {vm.Summary} ({Environment.CurrentManagedThreadId})");
                        await vm.SubscribeAsync();
                    }
                });
            }

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
            NetLogger.WriteLine("BAD: No zones found, clearing hosts");
            Settings.Instance.Hosts = string.Empty;

            return false;
        }
    }

    private async Task BuildHouseholdAsync(int hhIndex, Household h, bool faster)
    {
        if (h.AssociatedZP != null)
        {
            NetLogger.WriteLine($"Building household #{hhIndex}");

            await h.BuildGroupsAsync();

            NetLogger.WriteLine($"Initializing MS via {h.AssociatedZP.RoomName}");
            h.MusicServices = new MusicServiceProvider();
            try
            {
                await h.MusicServices.InitializeAsync(h.AssociatedZP);
            }
            catch (Exception ex)
            {
                NetLogger.WriteLine($"BuildHousehold problem: {ex.Message}");
            }

            NetLogger.WriteLine($"Adding {h.Groups.Count} groups");
            var vms = new List<GroupViewModel>();

            foreach (var g in h.Groups)
            {
                var vm = new GroupViewModel(g, h, hhIndex);
                if (!faster)
                {
                    NetLogger.WriteLine($"Group Subscribing {vm.Summary}");
                    var later = vm.SubscribeAsync();
                }
                vms.Add(vm);
            }

            h.Groups.CollectionChanged += OnGroupListChanged;
            h.NotifyHousehold += Household_Notify;

            NetLogger.WriteLine($"Household #{hhIndex} Subscribing");
            await h.SubscribeAsync();
            await InitMusicServicesAsync(h.AssociatedZP, h.MusicServices);

            // Now it is ok to add to the ZoneList
            foreach(var vm in vms)
            {
                ZoneList.Add(vm);
            }

            NetLogger.WriteLine($"Building household #{hhIndex} done");
        }
    }

    private void Household_Notify(object sender, Household.HouseholdEventArgs args)
    {
        OnHouseholdNotify.Invoke(sender, args);
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
                var vm = new GroupViewModel(g, h, GetIndex(h));
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

    private int GetIndex(Household h)
    {
        int i = 0;
        foreach(var hh in AllHouseholds.Households)
        {
            if (hh == h)
            {
                break;
            }
            i++;
        }
        return i;
    }

    private HardwareItem _HouseholdDisplayList;
    internal HardwareItem HouseholdDisplayList
    {
        get => _HouseholdDisplayList;
        set
        {
            SetProperty(ref _HouseholdDisplayList, value);
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
        bool got = await credentialsEvent.WaitAsync(TimeSpan.FromSeconds(10));
        await player.UnsubscribeToEventsAsync(which);
        if (!got)
        {
            string msg = await Listener.EventFailureMessage(player.ZoneGroupTopology.BaseUri);
            throw new Exception(msg);
        }
        await provider.RefreshAsync(credentials);
    }

    private void OnStartupFailed(object? arg)
    {
        bool quit = true;
        bool close = true;

        switch (arg as string)
        {
            case "Q":
                break;
            case "R":
                quit = false;
                close = false;
                StartupPopupOpen = false;
                StartupMessage = string.Empty;
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
                    StartupFailed = true;
                    StartupMessage = $"Email {HelpEmailAddress} for assistance and include diagnostic={App.SessionId}";
                    quit = false;
                    close = false;
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
        else if (close)
        {
            StartupPopupOpen = false;
        }

        string HelpBody()
        {
            var items = new List<string>()
            {
                "Version: " + AppVersion,
                "Platform: " + OpenPhonos.UPnP.Platform.Instance.FullPlatformName(),
                "Diagnostic: " + App.SessionId,
                NetLogger.LikelyProblem ?? string.Empty,
            };

            return string.Join(Environment.NewLine, items);
        }
    }

    public string AppVersion { get; private set; }
    public string AppName { get; private set; }
    public string Copyright { get; private set; }
    public string HelpEmailAddress { get; private set; }
    public string DiagnosticCode { get => App.SessionId; }
    public Action<object, Household.HouseholdEventArgs> OnHouseholdNotify { get; internal set; }

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
            var gvm = new GroupViewModel(g, household, 0);
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

    public override Task StartupAsync()
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

public class SortedObservableCollection<T> : ObservableCollection<T> where T : IComparable<T>
{
    protected override void InsertItem(int index, T item)
    {
        int sortedIndex = 0;
        while (sortedIndex < this.Count && this[sortedIndex].CompareTo(item) < 0)
        {
            sortedIndex++;
        }
        base.InsertItem(sortedIndex, item);
    }
}
