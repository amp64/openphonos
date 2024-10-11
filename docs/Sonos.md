# OpenPhonos Sonos

## Introduction
There are ally two ways to use the OpenPhonos.Sonos collection of classes: pick and choose a few, starting with Player, or go whole-hog and grab everything. Which path you take depends on what you want to do.

## Player
This is the easiest to use on its own, all you need is is a UPnP.Device:
```csharp
    Device device = // see [UPnUP](UPnP.md) for how to get a device
    Player player = await Player.CreatePlayerAsync(device);     // this will throw on failure
```
You can do all kinds of things with a Player, like setting its volume (which will only work if it is the Coordinator in a group), getting its *RoomName*, *HasBattery* and all manner of other things.

If you want to make direct UPnP calls on it you can pick one of the SonosServices on it:
```csharp
    // enable shuffle mod
    await player.AVTransport1.SetPlayMode(0, "SHUFFLE");
```

## Player Events
You can subscripe to events from a Player: to subscribe you pass a list of which Service events you want to subscribe, and handlers for those will be set up.
```csharp
    PlayerEventSelector what = new PlayerEventSelector(){ Device = true, Zone = true };
    await player.SubscribeToEventsAsync(Listener.DefaultTimeout, what);
```
Take a look at the event handlers to see what benefits each of them give you. Many are required for a full app experience.

## Household
If you want to handle Groups and the big picture stuff, you'll need to know about *Households*. A Household is a collection of Players that can be grouped together. There's a helper function that finds all the Players and puts them into Groups for you, *Household.FindAllHouseholdsAndPlayersAsync*. Note that this helper will find devices on every Household on your network.
```csharp
    var all = await Household.FindAllHouseholdsAndPlayersAsync();
    foreach(var household in all.Households)
    {
        Debug.WriteLine($"HouseholdId {household.HouseholdId}");
        foreach (var player in household.AllPlayerList)
        {
            Debug.WriteLine($"  Player {player.RoomName} ({player.ModelName})");
        }
    }
```
Households contain Groups, and you can explicitly get the current groups with a call to *BuildGroupsAsync*, or you can call *SubscribeAsync* to get them automatically updated.
```csharp
    var all = await Household.FindAllHouseholdsAndPlayersAsync();
    foreach (var household in all.Households)
    {
        Debug.WriteLine($"HouseholdId {household.HouseholdId}");
        await household.BuildGroupsAsync();
        foreach (var group in household.Groups)
        {
            Debug.WriteLine($"  Group {group.Summary()}");
        }
    }
```