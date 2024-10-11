using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace OpenPhonos.Sonos
{
    public class MusicServiceItem : MusicItem
    {
        internal string Id { get; }
        internal MusicService Provider { get; }
        internal string SearchItem { get; }

        internal MusicServiceItem(string id, string title, string subtitle, bool playable, bool queueable, bool container, bool canfave, bool alarmable, string metadata, string res, string art, MusicService source)
        {
            Id = id;
            Title = title;
            Subtitle = subtitle;
            IsPlayable = playable;
            IsQueueable = queueable;
            IsContainer = container;
            IsFavoritable = canfave;
            IsAlarmable = alarmable;
            Metadata = metadata;
            Res = res;
            ArtUri = art;
            Provider = source;
        }

        // This is used for search-result containers
        internal MusicServiceItem(string id, string title, string subtitle, string search, MusicService source)
        {
            Id = id;
            Title = title;
            Subtitle = subtitle;
            Provider = source;
            IsContainer = true;
            SearchItem = search;
        }
    }

    public class MusicItemDetail
    {
        public readonly string Art;
        public readonly bool Explicit;
        public readonly Dictionary<string, string> Properties;
        public readonly MusicService Source;
        public readonly string SmapiId;

        public MusicItemDetail(string art, bool @explicit, MusicService source, string smapiId)
        {
            Art = art;
            Explicit = @explicit;
            Properties = new Dictionary<string, string>();
            Source = source;
            SmapiId = smapiId;
        }
    }
}
