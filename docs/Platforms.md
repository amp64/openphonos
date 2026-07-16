# OpenPhonos Platforms

## Introduction
In theory the back-end components of *UPnP* and *Sonos* will run on any platform that runs .NET. I have successfully run them on the following platforms:
- Windows
- Android
- iOS
- MacOS
- TvOS

The back-end is built against .NET Standard 2.0 so is pretty flexible and should work on .Net FX, UWP, and all of the newer .NET Core releases.

As Avalonia requires .NET9 for MacOS, that means the back-end also runs against it in the sample app, which does create some challenges (listed below).

## Important - Releasing on Retail
Versions of this codebase have been shipped as retail code on Windows, MacOS and Android at the time of writing, and predecessors have shipped on Windows and iOS. They all work via F5 in the debugger, but getting them to build release and package suitable for submission to a managed store is a whole ball of fun left as an exercise to the reader.

Links are provided to the retail store versions (which include full music service support).

## Windows
This is the easiest and quickest to develop on, and needs nothing special to run. If you are hosting things in a UWP app you'll need to add privateNetworkClientServer and internetClientServer capabilities to the manifest.

[Phonos Ultimate on the Windows Store](https://apps.microsoft.com/detail/9pj1nl64bqqr?cid=gh)

## Android
This will not work on the emulator, due to the way the emulator handles network traffic. You need to run it on a real device. You also need to add the INTERNET permission to the manifest and also enable usesCleartextTraffic so that http calls will work to devices and to some artwork providers (eg the Sonos local library).

[Phonos Ultimate on the Play Store](https://play.google.com/store/apps/details?id=com.controlav.phonosultimate)

## iOS
This will work on the emulator and is the easiest way to get going on iOS. 
To work on a device you will need to flagellate yourself by getting an developer account, 
creating an app, some certs, and a provisioning profile. Probably some other painful stuff too I have forgotten due to PTSD. 
Oh yes, you'll need a Macintosh, you can't build iOS apps without one, and a relatively recent one. 
Your developer account will also need to be granted the [multicast entitlement](https://developer.apple.com/documentation/bundleresources/entitlements/com_apple_developer_networking_multicast) to run on any device.

I have not attempted to run this on iOS in a long time for multiple reasons:
 - Deployment from Visual Studio on PC was never reliable and went away entirely with the demise of Xamarin
 - The pain of certificates and provisioning profiles is just too much for me
- Also note that .NET 8 on iOS >= 16 [cannot create broadcastable sockets](https://github.com/dotnet/runtime/issues/106588) which is a major problem for device discovery. Older iOS versions are fine, as is the emulator.
- Your app's *info.plist* will need to include a bunch of network-related NS-permissions, in order to listen to broadcasts, do http calls, and other essentials. See the sample app for details.

# MacOS
On MacOS I recommend JetBrains Rider on your Mac, it can build and run it just fine.

[Phonos Ultimate on the App Store](https://apps.apple.com/us/app/phonos-ultimate-for-sonos/id6740011689)

# Others
I did briefly get something working on TvOS, but it broke one day and was too painful to attempt to fix.

A friend did get this running on Ubuntu Linux by installing the .NET SDK.
