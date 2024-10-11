using OpenPhonos.Sonos;
using OpenPhonos.UPnP;
using System.Collections.ObjectModel;
using Xunit.Abstractions;

namespace Sonos.Tests
{
    public class HouseholdTests
    {
        private readonly ITestOutputHelper output;

        public HouseholdTests(ITestOutputHelper output)
        {
            this.output = output;

            UPnPConfig.ListenerName = "SonosTests";
            UPnPConfig.UserAgent = "SonosTests UPnP/1.0 TestApp TestOS";
        }

        [Fact]
        public async Task TestHouseholdsAndPlayers()
        {
            var all = await Household.FindAllHouseholdsAndPlayersAsync();
            foreach(var household in all.Households)
            {
                output.WriteLine($"HouseholdId {household.HouseholdId}");
                foreach (var player in household.AllPlayerList)
                {
                    output.WriteLine($"  Player {player.RoomName} ({player.ModelName})");
                }
                output.WriteLine(string.Empty);
            }
        }

        [Fact]
        public async Task TestHouseholdsAndGroups()
        {
            var all = await Household.FindAllHouseholdsAndPlayersAsync();
            foreach (var household in all.Households)
            {
                output.WriteLine($"HouseholdId {household.HouseholdId}");
                await household.BuildGroupsAsync();
                foreach (var group in household.Groups)
                {
                    output.WriteLine($"  Group {group.Summary()}");
                }
            }
        }
    }
}
