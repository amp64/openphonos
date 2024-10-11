# OpenPhonos Sample App aka PhonosAvalon

## Introduction
To call this a sample app is somewhat disingenuous, it's a full featured Sonos controller. Thanks to the Avalonia framework it runs on pretty much anything. Lets look at its features:
- Transport controls Play/Pause/Next/Previous
- Volume control: Group and Device volumes
- Groups: fully understands Sonos Groups (aka *Zones*) and lets you add and remove devices to/from them
- Compatible with all S1 and S2 devices, all the way back to Sonos 8.4, the last version to support the trusty CR100
- Handles multiple Sonos Households on the same network simultaneously (its not magic though, you can't group players from different households together)
- Playback options include Play Now, Play Next, Add to Queue, Replace Queue, and Clear Queue
- Shows the current track (including high-res artwork), and the next track, including the music service it came from
- Stream display - shows you the details of the current music service stream (bitrate & sample rate) - one of the few S2-only features
- Understands all known types of content, including everything UPnP (playlists, local libraries, Favorites, line-in), TV, and every Sonos supported music service[^1] (there's over a hundred now!).
- Search works on all of the above types of content
- Browse all [Sonos supported music sources](MusicSources.md)
- Handles Like/Dislike ratings for tracks
- Understands light and dark modes automatically
- Displays content using the layout information provided by the music services, including Hero, List and Grid layouts
- Queue management: incomplete, but there's a popup window that shows you the queue and lets you change the playing track

Note that my UX skills have never been called great, by anyone, including me. I am a back-end developer, and it shows. However, as my wife is customer #1 for the iOS version I have put some effort into making it not terrible to look at.

Most of the ViewModels in this sample could likely be ported to other UX frameworks if you like. That's on you though, I am a big fan of Avalonia.

## Dependencies

I have tried to keep external dependencies to a minimum, but there are a few to make up for a few inexplicable-to-me deficiencies in Avalonia itself. These are:
- [AsyncImageLoader.Avalonia](https://github.com/AvaloniaUtils/AsyncImageLoader.Avalonia) - a image loader that works with Avalonia's Image control
- [DialogHost.Avalonia](https://github.com/AvaloniaUtils/DialogHost.Avalonia) - for modal dialogs
- [FluentIcons.Avalonia](https://github.com/davidxuang/FluentIcons) - for the icons to make the app not look truly terrible

## The App In Action
Here are a few screenshots showing key pieces of UX:

![Mobile](/docs/images/mobile.png)
![Desktop](/docs/images/desktop.png)
![Search](/docs/images/search.png)
![Groups](/docs/images/groups.png)
![Queue](/docs/images/queue.png)
![Group Edit](/docs/images/groupedit.png)
![Group Volume](/docs/images/groupvolume.png)]
![Display](/docs/images/display.png)]


[^1]: Seamless music service support requires you to bring your own decryption function.
