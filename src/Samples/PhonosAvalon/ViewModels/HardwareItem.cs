using OpenPhonos.Sonos;
using OpenPhonos.UPnP;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhonosAvalon.ViewModels;

/// <summary>
/// This is a static ViewModel, once created the items in it never change, so no INotify stuff required
/// </summary>
public class HardwareItem : ViewModelBase
{
    public string DisplayName { get; private set; }
    public string FriendlyModelName { get; protected set; }
    public string? Icon { get; protected set; }
    public FluentIcons.Common.Symbol? Symbol { get; protected set; }
    public string? Channel { get; protected set; }
    public bool DefaultExpand { get; set; }
    public IList<KeyValuePair<string,string>>? Data { get => GetData(); }
    public string? Problem { get; set; }

    public List<HardwareItem> Children { get; set; }

    private IList<KeyValuePair<string, string>>? _Data;

    protected virtual IList<KeyValuePair<string, string>>? GetData()
    {
        return _Data;
    }

    public HardwareItem(string name, FluentIcons.Common.Symbol? symbol = null)
    {
        DisplayName = name;
        Children = new List<HardwareItem>();
        FriendlyModelName = string.Empty;
        DefaultExpand = true;
        Symbol = symbol;
    }

    public HardwareItem(string name, IList<KeyValuePair<string, string>> data, FluentIcons.Common.Symbol? symbol = null)
    {
        DisplayName = name;
        Children = new List<HardwareItem>();
        FriendlyModelName = string.Empty;
        DefaultExpand = true;
        _Data = data;
        Symbol = symbol;
    }

    private enum ConnectionType
    {
        Wired,
        SonosNet,
        WiredOrSonosNet,
        WiFi,
        Quarantined,
        Unknown
    };

    private static string[] ConnectionTitles = new string[]
    {
        "Wired", "SonosNet", "Wired / SonosNet", "Wi-Fi", "Quarantined", "Unknown"
    };

    private static FluentIcons.Common.Symbol[] ConnectionSymbols = new FluentIcons.Common.Symbol[]
    {
        FluentIcons.Common.Symbol.VirtualNetwork, 
        FluentIcons.Common.Symbol.WifiLock, 
        FluentIcons.Common.Symbol.WifiSettings,
        FluentIcons.Common.Symbol.Wifi1, 
        FluentIcons.Common.Symbol.ErrorCircle,
        FluentIcons.Common.Symbol.QuestionCircle
    };

    static internal async Task<HardwareItem> CreateHouseholdDisplayAsync(IReadOnlyList<Household> households)
    {
        var root = new HardwareItem("Devices");
        int total = 0;
        foreach (var h in households)
        {
            var hid = new List<KeyValuePair<string, string>>() { new KeyValuePair<string, string>("ID", h.HouseholdId) };
            var household = new HardwareItem($"{h.AssociatedZP?.SoftwareGeneration} Household", hid, FluentIcons.Common.Symbol.HomeMore);

            // Dictionary of connections
            var connections = new Dictionary<ConnectionType, List<PlayerHardwareItem>>();
            var boundPlayers = new List<Player>();

            total += h.AllPlayerList.Count;

            // Add players that are not Bound to another
            foreach (var player in h.AllPlayerList)
            {
                // Skip bound items to later
                if (player.WirelessMode == null && player.ParentId != null)
                {
                    boundPlayers.Add(player);
                    continue;
                }

                var device = new PlayerHardwareItem(player);
                device.Problem = h.FindPlayerProblem(player.UniqueName);

                ConnectionType connectionType;

                switch (player.WirelessMode)
                {
                    case "0":
                        {
                            switch (player.EthLink)
                            {
                                case "1":
                                    connectionType = ConnectionType.Wired;
                                    break;
                                case "0":
                                    connectionType = ConnectionType.SonosNet;
                                    break;
                                default:
                                    connectionType = ConnectionType.WiredOrSonosNet;
                                    break;
                            }
                        }
                        break;
                    case "1":
                    case null:
                        connectionType = ConnectionType.WiFi;
                        break;
                    default:
                        connectionType = ConnectionType.Unknown;
                        break;
                }

                if (!connections.ContainsKey(connectionType))
                {
                    connections.Add(connectionType, new List<PlayerHardwareItem>());
                }
                connections[connectionType].Add(device);
            }

            foreach(var quarantined in h.GroupData.Quarantined)
            {
                try
                {
                    var d = await Device.CreateAsync(quarantined.Location);
                    var player = await Player.CreatePlayerAsync(d, quarantined);
                    var device = new PlayerHardwareItem(player);
                    device.Problem = "Quarantined: " + quarantined.QuarantineReason;
                    if (!connections.ContainsKey(ConnectionType.Quarantined))
                    {
                        connections.Add(ConnectionType.Quarantined, new List<PlayerHardwareItem>());
                    }
                    connections[ConnectionType.Quarantined].Add(device);
                    total++;
                }
                catch
                {}
            }

            foreach (var connection in connections)
            {
                string title = ConnectionTitles[(int)connection.Key];
                FluentIcons.Common.Symbol symbol = ConnectionSymbols[(int)connection.Key];

                if (connection.Key == ConnectionType.SonosNet || connection.Key == ConnectionType.WiredOrSonosNet)
                {
                    // Try and figure the frequency out
                    var net = connection.Value.FirstOrDefault(p => !string.IsNullOrEmpty(p.Channel));
                    if (net != null)
                    {
                        title += $" {net.Channel}";
                    }
                }
                var connectionRoot = new HardwareItem(title, symbol);
                AddPlayers(connectionRoot, connection.Value);
                household.Children.Add(connectionRoot);
            }

            root.Children.Add(household);

            void AddPlayers(HardwareItem root, List<PlayerHardwareItem> players)
            {
                foreach (var item in players)
                {
                    foreach (var bound in boundPlayers)
                    {
                        if (bound.ParentId == item.UniqueName)
                        {
                            var boundItem = new PlayerHardwareItem(bound);
                            item.Children.Add(boundItem);
                            item.DefaultExpand = false;
                        }
                    }
                    root.Children.Add(item);
                }
            }
        }
        root.DisplayName = $"Devices ({total})";
        return root;
    }
}

public class PlayerHardwareItem : HardwareItem
{
    private Player Player { get; set; }

    public string UniqueName => Player.UniqueName;

    internal PlayerHardwareItem(Player player) : base(player.RoomNameWithSuffix)
    {
        Children = new();
        FriendlyModelName = player.FriendlyModelName();
        Icon = player.Icon;
        Channel = ConvertChannel(player.ChannelFreq);
        if (player.Invisible && !string.IsNullOrEmpty(Channel))
        {
            Channel += " (Private)";
        }
        DefaultExpand = true;
        Player = player;
    }

    protected override IList<KeyValuePair<string, string>>? GetData()
    {
        var items = new List<KeyValuePair<string, string>>();
        items.Add(new KeyValuePair<string, string>("Model", FriendlyModelName));
        if (Player.SerialNumber != null)
        {
            items.Add(new KeyValuePair<string, string>("Serial Number", Player.SerialNumber));
        }
        if (Player!.IPAddress != null)
        {
            items.Add(new KeyValuePair<string, string>("IP Address", Player.IPAddress));
        }
        if (Channel != null)
        {
            items.Add(new KeyValuePair<string, string>("Channel", Channel));
        }
        items.Add(new KeyValuePair<string, string>("Version", $"{Player.DisplayVersion} (build {Player!.SoftwareVersion})"));
        items.Add(new KeyValuePair<string, string>("Hardware Version", Player!.HardwareVersion));
        if (Player.HasBattery && Player.BatteryState != null)
        {
            items.Add(new KeyValuePair<string, string>("Battery", Player.BatteryState.ToString()));
        }
        if (Problem != null)
        {
            items.Add(new KeyValuePair<string, string>("Problem", Problem));
        }
        return items;
    }

    internal static string ConvertChannel(string freq)
    {
        if (string.IsNullOrEmpty(freq))
            return string.Empty;

        if (int.TryParse(freq, out int mhz))
        {
            // based on https://en.wikipedia.org/wiki/List_of_WLAN_channels
            int f = mhz / 1000;
            int channel;
            if (f == 2)
            {
                channel = (mhz - 2407) / 5;
                if (channel > 14)
                    channel = 14;
                return $"2.4GHz channel {channel}";
            }
            else if (f == 5)
            {
                if (mhz < 5735)
                {
                    channel = (mhz - 5160) / 20 * 4 + 32;
                }
                else
                {
                    channel = (mhz - 5745) / 20 * 4 + 149;
                }
                return $"5GHz channel {channel}";
            }
        }

        return freq + "MHz";
    }
}
