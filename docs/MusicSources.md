# OpenPhonos Music Sources

## Introduction
One of the great strengths of Sonos is the variety of different music sources. Its also one of the hardest things to cover completely. To the user, music sources are presented as lists of lists. To the developer, each of those lists can come from a different place and from a very different location.
I have written and rewritten this area more than any other, and right now it has two different ways of enumerating things. The goals are to enumerate items efficiently and asynchronously, and to handle dynamic lists (eg the *TV* item appearing when you select an HT device, then disappearing when you select something else).

## Base class objects
Every item in the object tree is a MusicItem, and music items are obtained from an IMusicItemEnumerator. MusicItem has a few properties that are common to all items, such as Title, Subtitle, IsPlayable, IsQueueable, Metadata and Res. There are a few inherited classes that add additional properties, such as MusicServiceItem.

If a MusicItem returns true for IsContainer, then you can enumerate its children by calling GetChildrenEnumerator on enumerator that created it, and passing the item.
This is a recursive process, and you can walk the entire tree this way.

Given an IMusicItemEnumerator, you can get its children in two ways: the 'old' way, which just keeps giving you items until there are no more, and the 'new' way, which uses an ObservableCollection.

## Enumeration - the old way
All of the UPnP sources do it this way, and its pretty straightforward. You call GetNextItemsAsync on the enumerator, and it returns a list of MusicItems. If there are more items, you call it again. If there are no more items, it returns null.
```csharp
    IMusicItemEnumerator enumerator = new SonosPlaylistEnumerator();
    for (; ; )
    {
        // callback is a PlayerCallback, which is a simple structure to abstract the 'current player' (and, optionally, a list of players)
        IEnumerable<MusicItem> items = await enumerator.GetNextItemsAsync(callback);
        if (items == null)
        {
            break;
        }
        // do something with the items, eg add them to some UX
    }
```

## Enumeration - the new way
Most lists of music items will never change (or so rarely I don't care), but some can change (eg when 'TV' comes and goes depending on the current Group), so the 'new' way uses an ObservableCollection<MusicItem> designed to be used with data binding controls.
```csharp
    IMusicItemEnumerator baseEnumerator = new QueueItemEnumerator();
    var enumerator = baseEnumerator as IMusicItemEnumeratorObservable;      // will be null for 'old style' enumerators
    ReadOnlyObservableCollection<MusicItem> items = enumerator.GetAsObservable(callback.Player);
    await enumerator.StartReadAsync(callback.Player);
    // 'items' can now be bound to a ListView Source or whatever
```

## Enumeration - both ways
Having come up with two different ways of doing this, and not having the time or inclination to switch the 'old' enumerators to 'new' ones, I did the next best thing: I wrote a wrapper class that handles both. If you want to use something similar, take a look at MusicItemReader. This also has a clearer Start/Pause/Resume semantics, necessary for a reasonable UX with interruptible enumeration/back actions.

## Enumeration - futures
Even though I've come up with two (or, arguably, three) ways of doing this, its still not really enough. There are certain music lists that are gigantic ('Tracks' is usually the single largest list, maybe 'Queue') and really that needs a sparse enumerator, so that users can jump to, say, artists starting with T without having to enumerating all the artists A-S. If/when Avalonia shows up with a decent virtualizing ListView control, I'll consider giving it a shot.

## Supported Sources
The following top-level classes enable music source enumeration (and search) in this code:
- AllMusicEnumerator - duplicates the top-level of the Sonos Desktop app's music sources list, with the 'TV' entry being dynamic as well as the list of music services which shows up once it is determined
- FavoritesEnumerator
- MusicLibraryEnumerator - for local libraries
- RadioEnumerator - for TuneIn classic
- SonosPlaylistEnumerator - for Sonos playlists
- LineInEnumerator - for line-in sources

and then there are lower-level enumerators:
- UPnPItemEnumerator
- MusicLibraryEnumerator - for items within the local library
- RadioStationsEnumerator
- RadioShowsEnumerator
- MusicServiceItemEnumerator - for everything music service related
