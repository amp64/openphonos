using OpenPhonos.UPnP;
using System.Runtime.InteropServices;
using Xunit.Abstractions;

namespace UPnP.Test
{
    public class FinderTests
    {
        private const string TestUrn = "urn:schemas-upnp-org:device:ZonePlayer:1";

        private readonly ITestOutputHelper output;

        public FinderTests(ITestOutputHelper output)
        {
            this.output = output;

            // OpenPhonos.UPnP.Platform.Instance not needed to be set as the default is sufficient for these tests
            UPnPConfig.ListenerName = "MyTestApp";                   // this is used on NOTIFY events
            UPnPConfig.UserAgent = "Test UPnP/1.0 TestApp TestOS";       // this is used on all calls to the devices        }
        }

        [Fact]
        public async Task BasicNetworkScan()
        {
            var finder = new Finder();
            var devices = new List<string>();
            await finder.ByURNAsync(TestUrn, async (location, network, headers) =>
            {
                lock (devices)
                {
                    output.WriteLine("Found Device at {0}", location);
                    devices.Add(location);
                }
                return await Task.FromResult(true);
            },
            3);

            output.WriteLine("Found {0} devices", devices.Count);
        }

        [Fact]
        public async Task ScanAndMakeDevices()
        {
            var finder = new Finder();
            var devices = new List<Device>();
            await finder.ByURNAsync(TestUrn, async (location, network, headers) =>
            {
                var newdevice = await Device.CreateAsync(location);
                Assert.NotNull(newdevice);
                Assert.False(newdevice.IsMissing);
                output.WriteLine("Device {0} {1} {2}", newdevice.ModelName, newdevice.ModelNumber, newdevice.FriendlyName);

                lock (devices)
                {
                    devices.Add(newdevice);
                }
                return await Task.FromResult(true);
            },
            3);

            output.WriteLine("Found {0} devices", devices.Count);
        }

        [Fact]
        public async Task ScanForMediaServers()
        {
            var finder = new Finder();
            var devices = new List<Device>();
            await finder.ByURNAsync("urn:schemas-upnp-org:device:MediaServer:1", async (location, network, headers) =>
            {
                var newdevice = await Device.CreateAsync(location);
                Assert.NotNull(newdevice);
                Assert.False(newdevice.IsMissing);
                output.WriteLine("Device {0},{1},{2}", newdevice.ModelName, newdevice.ModelNumber, newdevice.FriendlyName);

                lock (devices)
                {
                    devices.Add(newdevice);
                }
                return await Task.FromResult(true);
            },
            3);

            output.WriteLine("Found {0} media servers", devices.Count);
        }

        // This test requires a Sonos device with a ContentDirectory service
        [Fact]
        public async Task ScanAndMakeContentDirectory()
        {
            var finder = new Finder();
            var devices = new List<Device>();
            await finder.ByURNAsync(TestUrn, async (location, network, headers) =>
            {
                var newdevice = await Device.CreateAsync(location);
                Assert.NotNull(newdevice);
                Assert.False(newdevice.IsMissing);
                output.WriteLine("Device {0} {1} {2}", newdevice.ModelName, newdevice.ModelNumber, newdevice.FriendlyName);
                var info = newdevice.FindServiceInfo(TestUrn, "urn:upnp-org:serviceId:ContentDirectory", throwIfMissing: false);
                if (info != null)
                { 
                    var content = new SonosServices.ContentDirectory1(info);
                    var id = await content.GetSystemUpdateID().Required();

                    lock (devices)
                    {
                        devices.Add(newdevice);
                    }
                }
                return await Task.FromResult(true);
            },
            3);

            output.WriteLine("Found {0} devices with the ContentDirectory service", devices.Count);
        }
    }
}