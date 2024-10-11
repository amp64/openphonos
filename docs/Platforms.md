# OpenPhonos Platforms

## Introduction
In theory the back-end components of *UPnP* and *Sonos* will run on any platform that runs .NET. I have successfully run them on the following platforms:
- Windows
- Android
- iOS
- MacOS
- TvOS

The back-end is built against .NET Standard 2.0 so is pretty flexible and should work on .Net FX, UWP, and all of the newer .NET Core releases.

As Avalonia requires .NET8 for iOS, that means the back-end also runs against it in the sample app, which does create some challenges (listed below).

## Important - Releasing on Retail
This specific version of the codebase has not been shipped as retail code on any of these platforms at the time of writing, though predecessors have shipped on Windows and iOS. They all work via F5 in the debugger, but getting them to build release and package suitable to submit to a managed store is a whole ball of fun I have not completed. Yet.

## Windows
This is the easiest and quickest to develop on, and needs nothing special to run. If you are hosting things in a UWP app you'll need to add privateNetworkClientServer and internetClientServer capabilities to the manifest.

## Android
This will not work on the emulator, due to the way the emulator handles network traffic. You need to run it on a real device. You also need to add the INTERNET permission to the manifest and also enable usesCleartextTraffic so that http calls will work to devices and to some artwork providers (eg the Sonos local library).

## iOS
This will work on the emulator and is the easiest way to get going on iOS. To work on a device you will need to flagellate yourself by getting an developer account, creating an app, some certs, and a provisioning profile. Probably some other painful stuff too I have forgotten due to PTSD. Oh yes, you'll need a Macintosh, you can't build iOS apps without one, and a relatively recent one. Your developer account will also need to be granted the [multicast entitlement](https://developer.apple.com/documentation/bundleresources/entitlements/com_apple_developer_networking_multicast) to run on any device.

Note that I have never tried the sample app on a iPad - it will use the phone layout I am sure, feel free to fix that so it uses the Desktop layout instead.
### iOS Critical Issues
- As of this writing the Xamarin infrastructure used to remote build, deploy and debug iOS apps from Visual Studio only works on XCode 15.4 If your Mac runs too new a MacOS version and forces you to XCode 16.x then that's a problem.
- Also note that .NET 8 on iOS >= 16 [cannot create broadcastable sockets](https://github.com/dotnet/runtime/issues/106588) which is a major problem for device discovery. Older iOS versions are fine, as is the emulator.
- Your app's *info.plist* will need to include a bunch of network-related NS-permissions, in order to listen to broadcasts, do http calls, and other essentials. See the sample app for details.
# MacOS
To run this on MacOS you will need install *Visual Studio for Macintosh*, then load the project and F5. Sadly Visual Studio for Mac is no longer supported. If you figure out another way to do this (e.g. VS Code) please share the magic.
