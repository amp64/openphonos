using OpenPhonos.Sonos;
using OpenPhonos.UPnP;
using System.Collections.ObjectModel;
using Xunit.Abstractions;

namespace Sonos.Tests
{
    public class EnumerateItemsTests
    {
        private readonly ITestOutputHelper output;

        public EnumerateItemsTests(ITestOutputHelper output)
        {
            this.output = output;

            // OpenPhonos.UPnP.Platform.Instance not needed to be set as the default is sufficient for these tests
            UPnPConfig.ListenerName = "SonosTests";
            UPnPConfig.UserAgent = "SonosTests UPnP/1.0 TestApp TestOS";
        }

        private async Task<Player> FindPlayerAsync()
        {
            var finder = new Finder();
            var devices = new List<string>();

            await finder.ByURNAsync("urn:schemas-upnp-org:device:ZonePlayer:1", async (location, network, headers) =>
            {
                lock (devices)
                {
                    devices.Add(location);
                }
                return await Task.FromResult(true);
            },
            3);

            foreach (var address in devices)
            {
                var device = await Device.CreateAsync(address);
                if (device == null)
                {
                    output.WriteLine($"Ignoring {address}: not found");
                    continue;
                }

                var player = await Player.CreatePlayerAsync(device);
                if (player.IsMissing)
                {
                    output.WriteLine($"Ignoring {address}: missing");
                    continue;
                }
                if (player.ContentDirectory == null)
                {
                    output.WriteLine($"Ignoring {address}: no ContentDirectory");
                    continue;
                }

                output.WriteLine($"Using player {player.RoomName} ({player.ModelName})");
                return player;
            }

            Assert.Fail("Didn't find any players");
            return default(Player);
        }

        private async Task<PlayerCallbacks> GetPlayerCallback()
        {
            var player = await FindPlayerAsync();
            var callbacks = new PlayerCallbacks(() => player);
            return callbacks;
        }

        [Fact]
        public async Task EnumeratePlaylistsTest()
        {
            var callback = await GetPlayerCallback();
            IMusicItemEnumerator enumerator = new SonosPlaylistEnumerator();
            int total = 0;
            for (; ; )
            {
                // This is the 'old' enumeration method
                IEnumerable<MusicItem> items = await enumerator.GetNextItemsAsync(callback);
                if (items == null)
                {
                    break;
                }
                var list = items.ToList();
                string summary = string.Join(", ", list.Select(x => x.Title));
                output.WriteLine($"Found {list.Count}: items {summary}");
                total += list.Count;
            }
            output.WriteLine($"Total items: {total}");
        }

        [Fact]
        public async Task EnumerateQueueTest()
        {
            var callback = await GetPlayerCallback();
            IMusicItemEnumerator baseEnumerator = new QueueItemEnumerator();

            // This is the 'new' enumeration method, which is designed for data binding so uses an ObservableCollection
            var enumerator = baseEnumerator as IMusicItemEnumeratorObservable;
            Assert.NotNull(enumerator);
            ReadOnlyObservableCollection<MusicItem> items = enumerator.GetAsObservable(callback.Player);
            Assert.NotNull(items);

            // Normally you would bind 'items' to some UX, but that won't work here, instead we'll just allow for 2 seconds worth of items to show up
            await enumerator.StartReadAsync(callback.Player);
            await Task.Delay(TimeSpan.FromSeconds(2));

            var list = items.ToList();

            output.WriteLine($"Found {list.Count} items in the queue:");
            foreach (var item in list)
            {
                output.WriteLine($"{item.Title} - {item.Subtitle}");
            }
        }
    }
}
