using OpenPhonos.Sonos;
using OpenPhonos.UPnP;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhonosAvalon.ViewModels
{
    public class NamedGroup
    {
        public string Name { get; private set; }
        public bool All { get => PlayerIds == null; }

        private List<string>? PlayerIds;

        public NamedGroup(string name)
        {
            Name = name;
        }

        public NamedGroup(string name, IList<string> ids) : this(name)
        {
            PlayerIds = ids.ToList();
        }

        internal bool Contains(Player p)
        {
            return PlayerIds == null || PlayerIds.Contains(p.UniqueName);
        }

        internal bool Matches(IList<Player> selectedPlayers, IList<Player> allPlayers)
        {
            if (this.All)
            {
                return selectedPlayers.Count() == allPlayers.Count();
            }

            if (selectedPlayers.Count() != PlayerIds.Count())
            {
                return false;
            }

            return selectedPlayers.All(p => PlayerIds.Contains(p.UniqueName));
        }
    }

    public class GroupEditorViewModel : ViewModelBase
    {
        public string Title { get => "Group Rooms"; }

        private string _StatusMessage;
        public string StatusMessage { get => _StatusMessage; set => SetProperty(ref _StatusMessage, value); }

        public ReadOnlyCollection<Player> Players { get; private set; }
        public ObservableCollection<Player> SelectedPlayers { get; private set; }
        public List<NamedGroup> NamedGroups { get; private set; }
        public NamedGroup? SelectedNamedGroup
        {
            get => _SelectedNamedGroup;
            set
            {
                if (SetProperty(ref _SelectedNamedGroup, value))
                {
                    OnSelectedNamedGroupChanged();
                }
            }
        }

        private NamedGroup? _SelectedNamedGroup;
        private bool _ChangingPlayers;
        private GroupViewModel _Group;
        private Household _Household;

        internal GroupEditorViewModel(Household h, GroupViewModel viewModel)
        {
            _Group = viewModel;
            _Household = h;

            var players = h.VisiblePlayerList.ToList();
            players.Sort(SortByPlayerName);

            // HACK make the list bigger for testing purposes
            // foreach (var p in h.VisiblePlayerList) players.Add(p); 

            Players = new ReadOnlyCollection<Player>(players);
            SelectedPlayers = new ObservableCollection<Player>(players.Where(p=>viewModel.ContainsPlayer(p)));
            SelectedPlayers.CollectionChanged += OnSelectedPlayersChanged;

            NamedGroups = new List<NamedGroup>()
            {
                new NamedGroup("Everywhere")
            };

#if DEBUG
            NamedGroups.Add(
                new NamedGroup("Debug First", new List<string>()
                {
                    Players.First().UniqueName
                }));
#endif

            SelectedNamedGroup = null;
        }

        private void OnSelectedPlayersChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (!_ChangingPlayers)
            {
                // Any change to selecte players unselects the named group
                SelectedNamedGroup = null;
            }
        }

        private NamedGroup? FindNamedGroup()
        {
            // If the current selection matches a NamedGroup, return it
            foreach(var g in NamedGroups)
            {
                if (g.Matches(SelectedPlayers, Players))
                {
                    return g;
                }
            }

            return null;
        }

        internal void OnSelectedNamedGroupChanged()
        {
            if (SelectedNamedGroup == null)
            {
                return;
            }

            var newSelection = Players.Where(p => SelectedNamedGroup.Contains(p));
            {
                _ChangingPlayers = true;
                SelectedPlayers.Clear();
                foreach (var p in newSelection)
                {
                    SelectedPlayers.Add(p);
                }
                _ChangingPlayers = false;
            }
        }

        private int SortByPlayerName(Player x, Player y)
        {
            return string.Compare(x.RoomName, y.RoomName, true);
        }

        public async Task<bool> OnOKAsync()
        {
            StatusMessage = "Changing Group";

            var ToAdd = new List<Player>();
            var ToRemove = new List<Player>();

            // Calculate what we need to do before we start, as the Zone is likely to change
            // as we edit it
            foreach (var player in Players)
            {
                bool playerSelected = SelectedPlayers.Contains(player);

                if (playerSelected && !_Group.ContainsPlayer(player))
                    ToAdd.Add(player);
                else if (!playerSelected && _Group.ContainsPlayer(player))
                    ToRemove.Add(player);
            }

            if (ToAdd.Count == 0 && ToRemove.Count == 0)
            {
                return true;
            }

            if (!Players.First().IsRealPlayer)
            {
                return true;
            }

            try
            {
                var zone = _Household.Groups.First(g => _Group.IsThisGroup(g));

                // Start by adding all the players to the zone
                foreach (var player in ToAdd)
                {
                    var oldzone = _Household.Groups.First(g => g.VisiblePlayers.Contains(player));
                    await oldzone.MovePlayerToZoneAsync(player, zone);
                }

                // then Remove any
                foreach (var player in ToRemove)
                {
                    zone = await zone.RemovePlayerFromZoneAsync(player, _Household);
                    if (zone == null)
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{ex.Message} during GroupEdit");
                throw;
            }

            return true;
        }
    }
}
