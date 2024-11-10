using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace OpenPhonos.Sonos
{
    [DebuggerDisplay("{Title}/{Subtitle}")]
    public class MusicItem
    {
        public string Title { get; protected set; }
        public string Subtitle { get; protected set; }

        /// <summary>
        /// Get the raw uri for artwork: not directly usable
        /// </summary>
        private string _ArtUri;
        public string ArtUri { get => _ArtUri; protected set => AssignArtUri(value); }

        public bool IsPlayable { get; protected set; }
        public bool IsContainer { get; protected set; }
        public bool IsFavoritable { get; protected set; }
        public bool IsAlarmable { get; protected set; }
        public bool IsDeletable { get; protected set; }
        public bool IsQueueable { get; protected set; }
        public bool IsError { get; protected set; }
        public bool IsExplicit { get; protected set; }

        public DisplayData DisplayData { get; set; }
        public DisplayData ParentDisplayData { get; set; }
        public string TrackNumber { get; set; }
        public Dictionary<string, string> Attributes { get; set; }

        public string Res { get; protected set; }
        public string Metadata { get; protected set; }

        public string Line0 => DisplayData.LineItem(this, 0);
        public string Line1 => DisplayData.LineItem(this, 1);
        public string Line2 => DisplayData.LineItem(this, 2);

        public IList<string> InlineChildren { get; set; }

        /// <summary>
        /// Get a usable art uri
        /// </summary>
        public Task<string> ArtUriAsync => GetArtUriAsync();

        /// <summary>
        /// Derived classes can override as required
        /// Base implementation is the same as ArtUri
        /// </summary>
        protected virtual Task<string> GetArtUriAsync()
        {
            return Task.FromResult(ArtUri);
        }

        public void UpdateMetadata(string md)
        {
            Metadata = md;
        }

        public void UpdateRes(string res)
        {
            Res = res;
        }

        public void UpdateSubtitle(string title)
        {
            Subtitle = title;
        }

        internal static MusicItem FromError(Exception ex)
        {
            return new MusicItem()
            {
                Title = ex.Message,
                IsError = true,
            };
        }

        public static MusicItem FromTitle(string title)
        {
            return new MusicItem()
            {
                Title = title
            };
        }

        // null is ok for a uri, but empty string is not
        private void AssignArtUri(string uri)
        {
            if (uri == string.Empty)
            {
                uri = null;
            }
            _ArtUri = uri;
        }
    }
}
