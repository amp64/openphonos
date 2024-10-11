using CommunityToolkit.Mvvm.ComponentModel;
using OpenPhonos.Sonos;
using OpenPhonos.UPnP;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PhonosAvalon.ViewModels
{
    public class NowPlayingViewModel : ViewModelBase
    {
        public enum PlayStateType
        {
            STOPPED,
            PLAYING,
            PAUSED_PLAYBACK,
            TRANSITIONING
        }

        public enum BuiltinArt
        {
            Nothing,
            TV,
            LineIn,
            Track,
            Book,
            Radio
        };

        #region public bindable properties

        private string _ErrorMessage;
        public string ErrorMessage
        {
            get { return _ErrorMessage; }
            set { SetProperty(ref _ErrorMessage, value); }
        }

        private bool _Error;
        public bool Error
        {
            get
            {
                return _Error;
            }
            private set
            {
                SetProperty(ref _Error, value);
            }
        }

        private bool _AlarmPlaying;
        public bool AlarmPlaying
        {
            get
            {
                // When Snoozing, Alarm is disabled (as it is implied)
                return _AlarmPlaying && !_SnoozeRunning;
            }
            private set
            {
                if (SetProperty(ref _AlarmPlaying, value))
                {
                    OnPropertyChanged(nameof(SnoozeRunning));
                }
            }
        }

        private bool _SnoozeRunning;
        public bool SnoozeRunning
        {
            get
            {
                return _SnoozeRunning;
            }
            private set
            {
                if (SetProperty(ref _SnoozeRunning, value))
                {
                    OnPropertyChanged(nameof(AlarmPlaying));
                }
            }
        }

        private bool _SeekVisible;
        public bool SeekVisible
        {
            get
            {
                return _SeekVisible;
            }
            private set
            {
                SetProperty(ref _SeekVisible, value);
            }
        }

        private string _Artist;
        public string Artist
        {
            get
            {
                return _Artist;
            }
            protected set
            {
                SetProperty(ref _Artist, value);
            }
        }

        private uint _NumberOfTracks;
        public uint NumberOfTracks
        {
            get
            {
                return _NumberOfTracks;
            }
            protected set
            {
                SetProperty(ref _NumberOfTracks, value);
            }
        }

        public string AVTransportUri { get; private set; }

        private uint _TrackNumber;
        public uint TrackNumber
        {
            get
            {
                return _TrackNumber;
            }
            protected set
            {
                SetProperty(ref _TrackNumber, value);
            }
        }

        private string _Album;
        public string Album
        {
            get
            {
                return _Album;
            }
            protected set
            {
                SetProperty(ref _Album, value);
            }
        }

        private string _TrackTitle;
        public string TrackTitle
        {
            get
            {
                return _TrackTitle;
            }
            protected set
            {
                SetProperty(ref _TrackTitle, value);
            }
        }

        private string _Thumbnail;
        public string Thumbnail
        {
            get
            {
                return _Thumbnail;
            }
            protected set
            {
                SetProperty(ref _Thumbnail, value);
            }
        }

        private string _Art;
        public string Art
        {
            get
            {
                return _Art;
            }
            protected set
            {
                if (SetProperty(ref _Art, value))
                {
                    //_ArtSource = null;
                    //NotifyPropertyChanged("ArtSource");
                }
            }
        }

        private string _Info;
        public string Info
        {
            get
            {
                return _Info;
            }
            protected set
            {
                SetProperty(ref _Info, value);
            }
        }

        private string _InfoDescription;
        public string InfoDescription
        {
            get
            {
                return _InfoDescription;
            }
            protected set
            {
                SetProperty(ref _InfoDescription, value);
            }
        }

        private double _Position;
        public double Position
        {
            get
            {
                return _Position;
            }
            set
            {
                if (_Position != value)
                {
                    if (CurrentTrackLength == 0)
                    {
                        _Position = 0;
                        OnPropertyChanged(nameof(Position));
                        return;
                    }
                    _Position = value;
                    OnPropertyChanged(nameof(Position));
                    Debug.WriteLine("Position set to {0} ({1})", _Position, _EditingPosition);
                    if (!EditingPosition)
                    {
                        // TODO var later = SetPositionAsync(value);
                    }
                }
            }
        }

        private bool _EditingPosition;
        public bool EditingPosition
        {
            get
            {
                return _EditingPosition;
            }
            set
            {
                if (SetProperty(ref _EditingPosition, value))
                {
                    Debug.WriteLine("EditingPosition set to {0}", _EditingPosition);
                    if (_EditingPosition == false)
                    {
                        // TODO var later = SetPositionAsync(_Position);
                    }
                }
            }
        }

        private string _PositionText;
        public string PositionText
        {
            get
            {
                return _PositionText;
            }
            protected set
            {
                SetProperty(ref _PositionText, value);
            }
        }

        private string _PositionLeft;
        public string PositionLeft
        {
            get
            {
                return _PositionLeft;
            }
            protected set
            {
                SetProperty(ref _PositionLeft, value);
            }
        }

        private bool _NightMode;
        public bool NightMode
        {
            get
            {
                return _NightMode;
            }
            set
            {
                if (SetProperty(ref _NightMode, value))
                {
                    // TODO var later = ActualPlayer.RenderingControl.SetEQ(0, "NightMode", (short)(value ? 1 : 0));
                }
            }
        }

        private bool _SpeechEnhancement;
        public bool SpeechEnhancement
        {
            get
            {
                return _SpeechEnhancement;
            }
            set
            {
                if (SetProperty(ref _SpeechEnhancement, value))
                {
                    // TODO var later = ActualPlayer.RenderingControl.SetEQ(0, "DialogLevel", (short)(value ? 1 : 0));
                }
            }
        }

        private string _TrackDescription;
        public string TrackDescription
        {
            get
            {
                return _TrackDescription;
            }
            protected set
            {
                SetProperty(ref _TrackDescription, value);
            }
        }

        private string _ArtistDescription;
        public string ArtistDescription
        {
            get
            {
                return _ArtistDescription;
            }
            protected set
            {
                SetProperty(ref _ArtistDescription, value);
            }
        }

        private string _AlbumDescription;
        public string AlbumDescription
        {
            get
            {
                return _AlbumDescription;
            }
            protected set
            {
                SetProperty(ref _AlbumDescription, value);
            }
        }

        private string _NextDescription;
        public string NextDescription
        {
            get
            {
                return _NextDescription;
            }
            protected set
            {
                if (SetProperty(ref _NextDescription, value))
                    OnPropertyChanged(nameof(NextSeparator));
            }
        }

        public string NextSeparator
        {
            get
            {
                return string.IsNullOrEmpty(_NextDescription) ? string.Empty : ":";
            }
        }

        private PlayStateType _PlayState;
        public PlayStateType PlayState
        {
            get
            {
                return _PlayState;
            }
            protected set
            {
                SetProperty(ref _PlayState, value);
            }
        }

        private string _NextTrack;
        public string NextTrack
        {
            get
            {
                return _NextTrack;
            }
            protected set
            {
                SetProperty(ref _NextTrack, value);
            }
        }

        private bool _SleepTimerRunning;
        public bool SleepTimerRunning
        {
            get
            {
                return _SleepTimerRunning;
            }
            protected set
            {
                SetProperty(ref _SleepTimerRunning, value);
            }
        }

        private bool _Explicit;
        public bool Explicit
        {
            get => _Explicit;
            protected set
            {
                if (SetProperty(ref _Explicit, value))
                {
                    OnPropertyChanged(nameof(ExplicitText));
                }
            }
        }

        public string ExplicitText
        {
            get => _Explicit ? "E" : string.Empty;
        }
        #endregion

        #region private members

        // In Seconds, or 0 if doesnt have one
        private int CurrentTrackLength;

        private string TrackMetaData;

        private readonly Player ActualPlayer;

        private bool HaveSubscribed;

        private readonly MusicServiceProvider MusicServices;

        private readonly Func<string, string?> PlayerFinder;

        #endregion

        public NowPlayingViewModel(Player player, MusicServiceProvider musicServices, Func<string, string?> playerFinder)
        {
            ActualPlayer = player;
            Artist = player.IsMissing ? "Network problem" : StringResource.Get("StreamConnecting");
            MusicServices = musicServices;
            TrackRatings = new RatingsViewModel[2] 
            { 
                new RatingsViewModel(RatingsCommand, 0), 
                new RatingsViewModel(RatingsCommand, 1) 
            };
            PlayerFinder = playerFinder;
        }

        protected NowPlayingViewModel()
        {
        }

        public NowPlayingViewModel WithSubscription()
        {
            if (ActualPlayer != null && !HaveSubscribed && ActualPlayer.AVTransport != null)
            {
                HaveSubscribed = true;
                var later = ActualPlayer.AVTransport.SubscribeAsync(OpenPhonos.UPnP.Listener.MinimumTimeout, "AVT", AVTransportCallback);
            }

            return this;
        }

        private async Task AVTransportCallback(Service sender, EventSubscriptionArgs args)
        {
            if (!args.Items.TryGetValue("LastChange", out string? lastchange) || ActualPlayer.Invisible)
            {
                return;
            }

            Debug.WriteLine("AVTLastChange " + ActualPlayer.RoomName);

            var xml = XElement.Parse(lastchange);
            await RefreshAVTransportAsync(xml);
        }

        private async Task RefreshAVTransportAsync(XElement xml)
        {
            const string NS = "urn:schemas-upnp-org:metadata-1-0/AVT/";
            const string R = "urn:schemas-rinconnetworks-com:metadata-1-0/";
            xml = xml.Descendants(XName.Get("InstanceID", NS)).FirstOrDefault();

            Func<string, string, string> get = delegate (string name, string ns)
            {
                var element = xml.Descendants(XName.Get(name, ns)).FirstOrDefault();
                if (element == null)
                    return null;
                return (string)element.Attribute(XName.Get("val")).Value;
            };

            ErrorMessage = string.Empty;

            var sleep = get("SleepTimerGeneration", R);
            if (sleep != null)
                this.SleepTimerRunning = sleep != "0";

            var alarm = get("AlarmRunning", R);
            if (alarm != null)
                this.AlarmPlaying = alarm != "0";

            var snoozing = get("SnoozeRunning", R);
            if (snoozing != null)
                this.SnoozeRunning = snoozing != "0";

            var status = get("TransportStatus", NS);
            if (status != null && status.StartsWith("ERROR_"))
            {
                var descr = get("TransportErrorDescription", NS);
                string track = string.Empty;
                string source = string.Empty;
                string serviceId = string.Empty;
                if (descr != null)
                {
                    var parts = descr.Split(',');       // seems to be <?>,ServiceId,Track,ServiceName,Uri
                    if (parts.Length > 3)
                    {
                        serviceId = parts[1];
                        track = parts[2];
                        source = parts[3];
                    }
                }
                switch (status)
                {
                    case "ERROR_BUFFERING":
                        status = StringResource.Get("ErrorBuffering");
                        break;
                    case "ERROR_LOST_CONNECTION":
                        status = StringResource.Get("ErrorLostConnection");
                        break;
                    case "ERROR_CANT_RESOLVE_NAME":
                        status = StringResource.Get("ErrorCantResolveName");
                        break;
                    case "ERROR_CANT_REACH_SERVER":
                        status = StringResource.Get("ErrorCantReachServer");
                        break;

                    default:
                        if (status.StartsWith("ERROR_SONOSAPI_"))
                        {
                            var err = await this.MusicServices.GetErrorMessageAsync(serviceId, "Error" + status.Substring(15) + "Message");
                            if (err != null)
                                status = err;
                        }
                        status = StringResource.Get("ErrorUnableToPlay") + status;
                        break;
                }
                var msg = string.Format(status, track, source);
                NetLogger.WriteLine(msg);
                ErrorMessage = msg;
            }

            var pm = get("CurrentPlayMode", NS);
            if (pm == null)
                return;             // eg Sleep only changed

            string state = get("TransportState", NS);
            if (state != null)
            {
                PlayState = Enum.Parse<PlayStateType>(state);
            }

            var actions = get("CurrentTransportActions", NS);
            if (actions == null)
            {
                var moreactions = await ActualPlayer.AVTransport.GetCurrentTransportActions(0);
                if (moreactions.Error == null)
                    actions = moreactions.Actions;
            }
            UpdateActions(actions);

#if false
            var validplaymodes = get("CurrentValidPlayModes", R);
            if (validplaymodes != null)
            {
                this.ShuffleVisible = validplaymodes.Contains("SHUFFLE");
                this.RepeatVisible = validplaymodes.Contains("REPEAT");
                this.CrossFadeVisible = validplaymodes.Contains("CROSSFADE");
            }

            var crossf = get("CurrentCrossfadeMode", NS);
            if (crossf != null)
            {
                this._CrossFadeSuppress = true;
                this.CrossFade = crossf != "0";
                this._CrossFadeSuppress = false;
            }
#endif

            var trackmd = get("CurrentTrackMetaData", NS);

            NumberOfTracks = uint.Parse(get("NumberOfTracks", NS));

            // if the track hasn't changed then we're done
            if (trackmd == TrackMetaData)
            {
                return;
            }

            // Remember the last value of this, used in Queue and Pandora calculations
            var avturi = get("AVTransportURI", NS);
            if (avturi == null)
            {
                // 7.1 doesnt seem to include this in the event any more, so go get it
                var info = await ActualPlayer.AVTransport.GetMediaInfo(0);
                if (info.Error == null)
                    avturi = info.CurrentURI;
            }

            if (!string.IsNullOrEmpty(avturi))
            {
                // Important to set this BEFORE TrackNumber/TrackMetadata, for eventing to the Queue to work correctly
                this.AVTransportUri = avturi;
            }

            this.NextDescription = this.NextTrack = string.Empty;

            TrackNumber = uint.Parse(get("CurrentTrack", NS));
            TrackMetaData = get("CurrentTrackMetaData", NS);

            await CalculateTrackDisplay_Async(get("EnqueuedTransportURIMetaData", R));

            var nexttrackmd = get("NextTrackMetaData", R);
            if (!string.IsNullOrEmpty(nexttrackmd))
            {
                SimpleDidl next = new SimpleDidl(XElement.Parse(nexttrackmd), false);
                this.NextDescription = StringResource.Get("NPNext");
                this.NextTrack = next.title + " - " + next.creator;
            }
        }

        private void UpdateActions(string actions)
        {
            if (actions != null)
            {
                CanPrevious = actions.Contains("Previous");
                CanNext = actions.Contains("Next");

                // this.SeekVisible = this.Actions.Contains("X_DLNA_SeekTime");
            }
        }

        // This code is designed to replicate the PC controller display as closely as possible
        private async Task CalculateTrackDisplay_Async(string enqueuedtransportmd)
        {
            Explicit = false;
            CurrentRatings.Clear();
            TrackRatings[0].Reset();
            TrackRatings[1].Reset();

            ExtendedMetadata = null;

            if (string.IsNullOrEmpty(TrackMetaData))
            {
                TrackDescription = StringResource.Get("NPTrack");
                TrackTitle = StringResource.Get("NoMusic");
                ArtistDescription = Artist = AlbumDescription = Album = InfoDescription = Info = string.Empty;
                Art = GetArtUri(BuiltinArt.Nothing);
                Thumbnail = Art;
                //HideAllButtons();
                return;
            }

            var xml = XElement.Parse(TrackMetaData);
            var track = new SimpleDidl(xml, false);

            if (track.res.StartsWith("x-sonos-htastream:"))
            {
                // TV
                TrackDescription = StringResource.Get("NPSource");
                TrackTitle = StringResource.Get("NPTV");
                ArtistDescription = StringResource.Get("NPAudioIn");
                Artist = Player.ConvertDigitalFormat(uint.Parse(track.streamInfo));
                AlbumDescription = Album = InfoDescription = Info = string.Empty;
                Art = GetArtUri(BuiltinArt.TV);
                Thumbnail = Art;
                return;
            }
            else if (track.res.StartsWith("x-rincon-stream:"))
            {
                // Line-in
                TrackDescription = StringResource.Get("NPSource");
                TrackTitle = track.title;
                ArtistDescription = StringResource.Get("NPRoom"); ;
                Artist = track.res.Substring(16);           // RINCON_xxx
                var room = PlayerFinder(Artist);
                if (room != null)
                {
                    Artist = room;
                }
                AlbumDescription = Album = InfoDescription = Info = string.Empty;
                Art = GetArtUri(BuiltinArt.LineIn);
                Thumbnail = Art;
                return;
            }
            else if (track.res.StartsWith("x-rincon-buzzer:"))
            {
                // Alarm chime
                TrackDescription = "Alarm";     // loc
                TrackTitle = "Sonos Chime";
                ArtistDescription = string.Empty;
                Artist = string.Empty;
                AlbumDescription = string.Empty;
                Album = string.Empty;
                Art = GetArtUri(BuiltinArt.Track);
                Thumbnail = Art;
                return;
            }

            string source = string.Empty;
            SimpleDidl enqueueTransportMd = default(SimpleDidl);

            if (!string.IsNullOrEmpty(enqueuedtransportmd))         // can be null when playing from a File (and idle)
            {
                try
                {
                    XElement exml;
                    try
                    {
                        exml = XElement.Parse(enqueuedtransportmd);
                    }
                    catch (System.Xml.XmlException)
                    {
                        if (enqueuedtransportmd.Contains("&mt="))
                        {
                            // Bad encoding sometimes on Apple urls
                            enqueuedtransportmd = enqueuedtransportmd.Replace("&mt=", "&amp;mt=");
                            exml = XElement.Parse(enqueuedtransportmd);
                        }
                        else
                        {
                            throw;
                        }
                    }

                    enqueueTransportMd = new SimpleDidl(exml, true);
                    source = enqueueTransportMd.desc;

                    if (source != null && source.StartsWith("SA_RINCON") && MusicServices != null)
                    {
                        source = MusicServices.GetServiceName(source);
                        var streaminfo = Player.DecodeStreamInfo(source, track.streamInfo);
                        if (streaminfo != null)
                        {
                            source = source + " " + string.Join(" ", streaminfo);
#if DEBUG
                            source = source + " " + track.streamInfo;
#endif
                        }
                    }
                    else
                        source = string.Empty;
                }
#if false // TODO json
                catch (System.Xml.XmlException)
                {
                    try
                    {
                        // Spotify-connect
                        var json = JsonValue.Parse(enqueuedtransportmd);
                        source = json.GetObject().GetNamedObject("service").GetNamedString("name");
                    }
                    catch (Exception)
                    { }
            }
#endif
                catch (Exception)
                {
                    source = string.Empty;
                }
            }

            var defaultArt = BuiltinArt.Track;

            if (track.res.StartsWith("x-sonosapi-hls:"))
            {
                // Sirius (which we do better than the Desktop)
                TrackDescription = StringResource.Get("NPChannel");
                TrackTitle = enqueueTransportMd.title;
                InfoDescription = source;
                Info = !string.IsNullOrEmpty(track.radioShowMd) ? track.radioShowMd.TrimEnd(new char[] { ',' }) : string.Empty;
                ArtistDescription = Artist = Album = AlbumDescription = string.Empty;
                if (!string.IsNullOrEmpty(track.streamContent))
                {
                    // "TYPE=SNG|TITLE goosebumps|ARTIST Travis Scott|ALBUM Birds in the Trap Sing McKnight"
                    var parts = track.streamContent.Split('|');
                    if (parts.Length > 3)
                    {
                        // Apple Music Beats 1 does this
                        Info = TrackTitle;
                        InfoDescription = TrackDescription;

                        ArtistDescription = StringResource.Get("NPArtist");
                        Artist = parts.First(p => p.StartsWith("ARTIST ")).Substring(7);

                        AlbumDescription = StringResource.Get("NPAlbum");
                        Album = parts.First(p => p.StartsWith("ALBUM ")).Substring(6);

                        TrackDescription = StringResource.Get("NPTrack");
                        TrackTitle = parts.First(p => p.StartsWith("TITLE ")).Substring(6);
                    }
                }
            }
            else if (track.uclass == "object.item.audioItem.audioBook.chapter")
            {
                // AudioBook
                TrackDescription = "Chapter";
                TrackTitle = track.title;
                ArtistDescription = "Author";
                Artist = track.creator;
                AlbumDescription = "Narrator";
                Album = track.narrator;
                InfoDescription = "Book";
                Info = enqueueTransportMd.title;
                defaultArt = BuiltinArt.Book;
            }
            else if (track.uclass == "object.item.audioItem.podcast")
            {
                // Podcast
                TrackDescription = "Episode";
                TrackTitle = track.title;
                ArtistDescription = "Podcast";
                Artist = string.Empty;
                if (!string.IsNullOrEmpty(track.releaseDate))
                {
                    AlbumDescription = "Release Date";
                }
                Album = track.releaseDate;
                InfoDescription = source;
                Info = string.Empty;
            }
            else if (track.uclass.StartsWith("object.item.audioItem.musicTrack"))
            {
                // Pandora (which has source and, strangely, tracks) and MP3s (which has no ms source)
                if ((NumberOfTracks == 0) || !string.IsNullOrEmpty(source))
                    TrackDescription = StringResource.Get("NPTrack");
                else
                    TrackDescription = string.Format(StringResource.Get("NP_Track_Counter"), TrackNumber, NumberOfTracks);
                TrackTitle = track.title;
                ArtistDescription = StringResource.Get("NPArtist");
                Artist = track.creator;
                AlbumDescription = string.IsNullOrEmpty(track.album) ? string.Empty : StringResource.Get("NPAlbum");
                Album = track.album;
                InfoDescription = source;
                Info = string.IsNullOrEmpty(source) ? string.Empty : enqueueTransportMd.title;
            }
            else if (enqueueTransportMd.uclass != null && enqueueTransportMd.uclass.StartsWith("object.item.audioItem.audioBroadcast"))
            {
                // Radio station
                TrackDescription = StringResource.Get("NPStation");
                TrackTitle = enqueueTransportMd.title;

                var radio = track.radioShowMd;
                if (!string.IsNullOrEmpty(radio))
                {
                    var comma = radio.IndexOf(",p");
                    if (comma != -1)
                        radio = radio.Substring(0, comma);
                }

                ArtistDescription = StringResource.Get("NPOnNow");
                Artist = radio;

                Album = string.Empty;
                AlbumDescription = string.Empty;

                if (string.IsNullOrEmpty(track.streamContent) || track.streamContent == "  - ")
                {
                    InfoDescription = string.Empty;
                    Info = string.Empty;
                }
                else
                {
                    InfoDescription = StringResource.Get("NPInformation");
                    Info = DidlData.ZPConvert(track.streamContent);
                }

                defaultArt = BuiltinArt.Radio;
            }
            else if (track.uclass == "object.item")
            {
                // MP3 Files
                if ((NumberOfTracks == 0) || !string.IsNullOrEmpty(source))
                    TrackDescription = StringResource.Get("NPTrack");
                else
                    TrackDescription = string.Format(StringResource.Get("NP_Track_Counter"), TrackNumber, NumberOfTracks);
                TrackTitle = track.title;
                ArtistDescription = string.IsNullOrEmpty(track.creator) ? string.Empty : StringResource.Get("NPArtist");
                Artist = track.creator;
                AlbumDescription = string.IsNullOrEmpty(track.album) ? string.Empty : StringResource.Get("NPAlbum");
                Album = track.album;
                InfoDescription = source;
                Info = string.IsNullOrEmpty(source) ? string.Empty : enqueueTransportMd.title;
            }

            ExtendedMetadata = await MusicServices.GetExtendedMetadataFromResAsync(track.res, ActualPlayer);
            UpdateTrackRating();

            if (!string.IsNullOrEmpty(track.albumArtURI))
            {
                Art = DidlData.ArtConverter(track.albumArtURI, ActualPlayer.AVTransport?.BaseUri, ExtendedMetadata);
                if (Art == null)
                {
                    Art = GetArtUri(defaultArt);
                }
                Thumbnail = DidlData.ArtConverter(track.albumArtURI, ActualPlayer.AVTransport?.BaseUri, null);
                if (Thumbnail == null)
                {
                    Thumbnail = GetArtUri(defaultArt);
                }
            }
            else
            {
                Art = GetArtUri(defaultArt);
                Thumbnail = Art;
            }
        }

        #region Ratings

        List<MusicService.Rating> CurrentRatings = new List<MusicService.Rating>();
        MusicItemDetail? ExtendedMetadata;

        private void UpdateTrackRating()
        {
            if (ExtendedMetadata != null)
            {
                Explicit = ExtendedMetadata.Explicit;
                var ratings = ExtendedMetadata.Source.FindRating(ExtendedMetadata.Properties);
                if (ratings != null)
                {
                    CurrentRatings = ratings.ToList();
                    // If 1 rating, put it into [1] so its on the right, if 2 ratings, put them into [0] and [1]
                    if (CurrentRatings.Count == 1) 
                    {
                        CopyRating(0, 1);
                        TrackRatings[0].Enabled = false;
                    }
                    else
                    {
                        CopyRating(0, 0);
                        CopyRating(1, 1);
                    }
                }
            }

            void CopyRating(int source, int dest)
            {
                TrackRatings[dest].Enabled = true;
                TrackRatings[dest].BeforeMessage = CurrentRatings[source].StringId;
                TrackRatings[dest].Image = CurrentRatings[source].Uri;
            }
        }

        public RatingsViewModel [] TrackRatings { get; private set; }

        private async Task RatingsCommand(int index)
        {
            int ratingindex = CurrentRatings.Count == 1 ? 1 - index : index;

            // Get a reference to this in case it goes away in the middle
            var ext = ExtendedMetadata;
            if (ActualPlayer == null || CurrentRatings == null || CurrentRatings.Count <= ratingindex || ext == null)
            {
                return;
            }

            TrackRatings[index].AfterMessage = "Sending...";

            var before = await ext.Source.GetLastUpdateFavoriteAsync(ActualPlayer);

            string rating = CurrentRatings[ratingindex].Id;
            string id = ext.SmapiId;
            var rateResult = await ext.Source.RateItemAsync(id, rating, ActualPlayer);
            if (!rateResult.success)
            {
                TrackRatings[index].AfterMessage = string.Empty;
                return;
            }
            TrackRatings[index].AfterMessage = CurrentRatings[ratingindex].OnSuccessStringId;
            for (int i = 0; i < 10; i++)
            {
                var after = await ext.Source.GetLastUpdateFavoriteAsync(ActualPlayer);
                if (after == null)
                    break;
                if (before != after)
                {
                    var newmd = await ext.Source.GetExtendedMetadataAsync(ext.SmapiId, ActualPlayer);
                    if (ExtendedMetadata != null && ExtendedMetadata.SmapiId == newmd?.SmapiId)
                    {
                        // Only update this if the track is still current
                        ExtendedMetadata = newmd;
                        UpdateTrackRating();
                    }
                    break;
                }
                await Task.Delay(200);
            }
        }
        #endregion

        protected static string GetArtUri(BuiltinArt what)
        {
            string name;

            switch (what)
            {
                case BuiltinArt.Book:
                    name = "book";
                    break;
                case BuiltinArt.LineIn:
                    name = "linein";
                    break;
                case BuiltinArt.Nothing:
                    name = "nothing";
                    break;
                case BuiltinArt.Radio:
                    name = "radio";
                    break;
                case BuiltinArt.TV:
                    name = "tv";
                    break;
                case BuiltinArt.Track:
                default:
                    name = "defaultartwork";
                    break;
            }

            return "avares://PhonosAvalon/Assets/NowPlaying/" + name + ".png";
        }

        public async Task PlayPauseFromQueue(QueueMusicItem music, uint which /* 0 means Pause */, bool justResume)
        {
            if (ActualPlayer?.AVTransport == null)
            {
                return;
            }

            if (which == 0)
            {
                await ActualPlayer.AVTransport.Pause(0);

            }
            else
            {
                if (justResume)
                {
                    await ActualPlayer.AVTransport.Play(0, "1");
                }
                else
                {
                    await ActualPlayer.StartPlaybackFromQueue(which);
                }
            }
        }

        #region Commands
        public async Task PlayPauseCommand(string msg)
        {
            if (ActualPlayer == null || ActualPlayer.AVTransport == null)
            {
                return;
            }

            if (PlayState==PlayStateType.PLAYING)
            {
                await ActualPlayer.AVTransport.Pause(0);
            }
            else
            {
                await ActualPlayer.AVTransport.Play(0, "1");
            }
        }

        public async Task NextCommand(string _)
        {
            if (ActualPlayer == null)
            {
                return;
            }

            await ActualPlayer.AVTransport.Next(0);
        }

        private bool _CanNext;
        public bool CanNext
        {
            get => _CanNext;
            set => SetProperty(ref _CanNext, value);
        }

        public async Task PreviousCommand(string _)
        {
            if (ActualPlayer == null)
            {
                return;
            }

            await ActualPlayer.AVTransport.Previous(0);
        }

        private bool _CanPrevious;
        public bool CanPrevious
        {
            get => _CanPrevious;
            set => SetProperty(ref _CanPrevious, value);
        }

        #endregion

        #region static Getter
        private static Dictionary<Player, NowPlayingViewModel> ViewModels = new Dictionary<Player, NowPlayingViewModel>();

        /// <summary>
        /// We maintain a list of VMs for each Player as we don't want to over-subscribe
        /// To get one for a given player, call this
        /// </summary>
        public static NowPlayingViewModel Get(Group group, MusicServiceProvider musicServices)
        {
            var player = group.Coordinator;

            if (!ViewModels.TryGetValue(player, out NowPlayingViewModel? model))
            {
                string? playerFinder (string id)
                {
                    var p = group.FindPlayerById(id);
                    return p?.RoomName;
                };

                model = new NowPlayingViewModel(player, musicServices, playerFinder);
                ViewModels[player] = model;
            }

            return model;
        }
        #endregion
    }

    #region Fake
    public class FakeNowPlayingViewModel : NowPlayingViewModel
    {
        public FakeNowPlayingViewModel()
        {
            ArtistDescription = "Artist";
            Artist = "OpenPhonos";

            TrackDescription = "Track";
            TrackTitle = "First Release";

            AlbumDescription = "Album";
            Album = "Main Branch";

            InfoDescription = "Generic Music Service";
            Info = "44KHz Stereo";

            Art = GetArtUri(BuiltinArt.Track);
            Thumbnail = Art;
        }
    }
    #endregion
}
