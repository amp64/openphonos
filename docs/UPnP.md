# OpenPhonos UPnP

## Introduction
This UPnP stack has only ever been used with Sonos devices. There's no known reason why it won't work on anything else, but I've never tried it.

## Overview
To use UPnP you need a device on your network. First you need to find your devices, using SSDP to scan for them. Having found your devices, you can then query them for their services. This codebase uses pre-built types and methods, and you can find the Sonos ones in that project's *Services* folder. You can create your own types and methods using the included *UPNPServiceCreator*.

## Initialization
This codebase has been run on many different platforms and UX frameworks. To handle the differences, you need to provide an implementation of the IPlatform interface and set the class up before using it. The sample app includes the AvaloniaPlatform class. It's not complicated.
```csharp
        OpenPhonos.UPnP.Platform.Instance = new MyPlatform();     // MyPlatform must derive from IPlatform
        UPnPConfig.ListenerName = "MyGreatApp";                   // this is used on NOTIFY events
        UPnPConfig.UserAgent = "Me UPnP/1.0 MyApp MyOS";       // this is used on all calls to the devices
```

## Device Discovery
```csharp
    string TestUrn = "urn:schemas-upnp-org:device:ZonePlayer:1";
    var finder = new Finder();
    var devices = new List<Device>();
    await finder.ByURNAsync(TestUrn, async (location, network, headers) =>
    {
        // This delegate can be called on any thread, don't block it
	    var device = await Device.CreateAsync(location);
        lock (devices)
        {
            Console.WriteLine("Found Device at {0}", location);
            devices.Add(device);
        }
        return await Task.FromResult(true);         // carry on searching
    });
```

## Services
A **Device** isn't much good on its own, you need to get the services from it. Here's how you can get the services from a device (using one of the Sonos pre-built classes):
```csharp	
    var info = device.FindServiceInfo(TestUrn, "urn:upnp-org:serviceId:ContentDirectory", throwIfMissing: false);
    if (info != null)
    {
	    var service = new SonosServices.ContentDirectory1(info);
```
## Methods (aka UPnP Actions)
Now you have a service, you can make calls on it. All calls are async, but I did not append that to the method names, contrary to general guidelines and the rest of this project.
Here's an example of calling a method on a service:
```csharp
    // assume service is a ContentDirectory1
    var response = await service.GetSystemUpdateID();
```
The return value for all of these methods is derived from the *OpenPhonos.UPnP.Service.ActionResult* class. Note that these calls **do not throw exceptions** by default. Over the years I have found that I often don't care if many of these calls fail, they are 'fire and forget', so I chose an unusual calling pattern.
The return value includes an Error property, which is null if the call was successful. If it is not null, it will contain a UPnPException which you can throw if you want to.
```csharp
    var response = await service.GetSystemUpdateID();
    if (response.Error != null)
    {
	    throw response.Error;
    }
    // alternate pattern:
    response.ThrowIfFailed();
```
In more recent code I have been using some extension methods, to make it clearer if the call will throw, or not.
```csharp
    // assume service is an AVTransport1
    var info = await service.GetMediaInfo(0).Optional();	      // failure is ok to ignore
    var response = await service.Play(0, "1").Required();         // failure is not ok
```
You can see real examples of all of this in the UPnP.Test project.

## UPnP Events
You can subscribe to UPnP events to receive change notifications from the services. When you are finished you should unsubscribe from them. The library looks after renewing the event, until you unsubscribe.
For mobile devices, you should also unsubscribe when your app is about to be suspended, else the UPnP device will likely stop sending you events as you cannot respond to them when in a suspended state. The Listener class can also handle the bulk-unsubscription/resubscription during suspend/resume, but you need to call it at the right moment.
```csharp
    // assume service is an AVTransport1, pass a timeout, a debug-string, and a handler
    await service.SubscribeAsync(Listener.MinimumTimeout, "AVT", AVTransportHandler);
    ...
    private async Task AVTransportHandler(Service sender, EventSubscriptionArgs args)
    {
        // this is called on the UI thread, as determined by IPlatform.OnUIThread
        if (!args.Items.TryGetValue("LastChange", out string? lastchange))
        {
            return;
        }

        Debug.WriteLine("AVT-LastChange");
        var xml = XElement.Parse(lastchange);
        await RefreshAVTransportAsync(xml);
    }

    // when you are done
    await service.UnsubscribeAsync();

    // before you exit, or your app is suspended
    await Listener.OnSuspendAllAsync();

    // when you come back after being suspended
    Listener.OnResumeAll();

```

## UPnPServiceGenerator
To use with your own UPnP devices, or perhaps to update the service descriptions in the provided Sonos project, you can run this tool. Give it the url of one of your devices, and it will create the types and methods for you.