# OpenPhonos Music Services

## Introduction
One of Sonos' great strengths is its extensive support for music services, at over a hundred and counting. This huge scope was achieved by Sonos designing a REST interface that their apps consumed, and moving the onus on every music service provider to implement those interfaces for their music.
## History
When Sonos was released it supported local libraries only, via file shares. The first music service it supported was Rhapsody, and it used the Rhapsody API. (The Rhapsody API was one of the worst APIs I have ever seen: my oldest Sonos apps used it).
The next service Sonos supported was Pandora, and again they used the Pandora API directly in the app (it was a much nicer API). They realized that this way of doing things, adding in each music service's API, was simply not scalable, so they created SMAPI - *the Sonos Music API*. After this the work in service integration moved almost entirely to the providers themselves. Eventually Sonos persuaded Pandora and Rhapsody to move to SMAPI, and here we are.
## The Security Problem
Sonos used to store the credentials for each music service in their graph (the database that is duplicated on every speaker), and you could extract the usernames and passwords with a simple UPnP call. Ooops. Seems ridiculous now, but it wasn't so crazy back then, its just music service credentials, not your banking login. Well, unless you used the same credentials with both your bank and Sonos...

Around 2014 Sonos stepped up the security of their music service support: they added the ability to store tokens, instead of actual usernames/passwords, so to add a music service you would login to the music service web site (or, later, their mobile apps) and the service would return an account key and a token, which Sonos would store, securely.

This was a much more secure system, as a token leak would only allow an attacker to play your music, not empty your bank account. However, for third party app developers, like me, it was kind of a gigantic problem, as once music services moved to this option (which was eventually all of the paid services), they could not be used by third parties.
## How SMAPI Works
When a Sonos app starts, it gets a list of available services, which includes the uris and other attributes of every service available to Sonos. (See *MusicServiceProvider.InitializeAsync*). Meanwhile someone has subscribed to events from a Player, and when the *Player.OnThirdPartyMediaServices* event fires, an encrypted list of credentials is delivered.
Once decrypted, that list can be parsed for every subscribed service, giving the key and token of each service to later use for authorizition (see *MusicServiceProvider.RefreshAsync*).

Once you can authorize yourself to a SMAPI endpoint you can then ask it for all kinds of good things, and you can read about the details on [the Sonos developer site](https://docs.sonos.com/docs/content-service-get-started). Sadly these docs are not as good as the older version, which you can find on the [wayback machine](http://web.archive.org/web/20230402002238/https://developer.sonos.com/reference/sonos-music-api/). Something you won't find anywhere is the json version of SMAPI which appeared along with the original Sonos Radio service. Fortunately I have figured that all out for you. Look at the MusicService class to see how all of that works.

## What about that decryption?
You won't find the credential decryption algorithm here. If you know what it is, just implement *MusicServiceProvier.DecryptMusicServiceData* (its a partial method, so the code builds and runs without it, but you get no music services).
I'm not including it here because I didn't do the hard work of figuring it out, so it's not my code to share.

All is not lost though: others have figured out service-specific ways of using those service's APIs (eg Spotify) to get a key and token which then work on SMAPI calls to that service. It's not as clean as supporting every music service, but it's better than nothing. Once you have the key and the token you should be able to fake up enough additional data to construct a MusicService instance (look at *MusicServiceProvider.CreateRadioService* to see how TuneIn (Classic) is created as an example).
