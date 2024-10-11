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

namespace PhonosAvalon.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public MainViewModel()
    {
        ArtSize = 200;

        ZoneList = new ObservableCollection<GroupViewModel>();

        VolumePopupCommand = new ActionCommand((o) => VolumePopupOpen = true);
        GroupEditorCommand = new ActionCommand((o) => GroupEditorOpen = true);
        QueuePopupCommand = new ActionCommand((o) => QueuePopupOpen = true);

        var later = StartupAsync();
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

    public Household GetHouseholdFromZone(GroupViewModel zone)
    {
        var h = AllHouseholds.Households.FirstOrDefault(h => h.Groups.FirstOrDefault(g => zone.IsThisGroup(g)) != null);
        return h;
    }

    virtual protected async Task StartupAsync()
    {
        OpenPhonos.UPnP.Platform.Instance = new AvaloniaPlatform();
        UPnPConfig.ListenerName = "OpenPhonos.2023";
        UPnPConfig.UserAgent = "Linux UPnP/1.0 Sonos/80.1-55014 (WDCR:Microsoft Windows NT 10.0.19045 64-bit)";

        CurrentZone = new FakeGroupViewModel("Starting up...");

        AllHouseholds = await Household.FindAllHouseholdsAndPlayersAsync((player, other) =>
        {
            // Status update here as players are found
        });

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

        if (ZoneList.Count > 0)
        {
            var z = ZoneList.FirstOrDefault(z => z.Coordinator?.UniqueName == Settings.Instance.CurrentZoneId);
            CurrentZone = z ?? ZoneList[0];
        }
        else
        {
            // TODO tell the user
            NetLogger.WriteLine("BAD: No zones found");
        }
    }

    private async void OnGroupListChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        Household h = AllHouseholds.Households.First(hh => sender.Equals(hh.Groups));

        Debug.WriteLine($"Group changed: {e.Action} in {h.HouseholdId}");

        if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null && e.NewItems.Count != 0)
        {
            foreach(var item in e.NewItems)
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

            foreach(var item in e.OldItems)
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
