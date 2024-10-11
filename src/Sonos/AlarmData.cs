using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Xml.Linq;
using OpenPhonos.UPnP;
using System.Globalization;

namespace OpenPhonos.Sonos
{
    public class AlarmData
    {
        public class Strings
        {
            public const string Once = "ONCE";
            public const string Daily = "DAILY";
            public const string OnPrefix = "ON_";
            public const string Weekdays = "WEEKDAYS";
            public const string Weekends = "WEEKENDS";
            public const string ShuffleOn = "SHUFFLE";
            public const string ShuffleOff = "REPEAT_ALL";
            public const string ChimeUri = "x-rincon-buzzer:0";
        }

        private uint Id;
        private string ProgramUri { get; set; }
        private string ProgramMetadata { get; set; }
        private string PlayMode { get; set; }
        private DidlData Metadata { get; set; }
        public TimeSpan StartTime { get; set; }
        public string Duration { get; set; }
        public string Recurrence { get; set; }
        public bool Enabled { get; set; }
        public string RoomUUID { get; set; }
        public int Volume { get; set; }
        public bool IncludeLinkedZones { get; set; }
        public string Description { get; set; }
        public string RoomName { get; set; }

        public AlarmData(XElement alarm, IEnumerable<Player> players)
        {
            this.Id = uint.Parse((string)alarm.Attribute(XName.Get("ID")));
            this.StartTime = TimeSpan.Parse((string)alarm.Attribute(XName.Get("StartTime")));
            this.Duration = (string)alarm.Attribute(XName.Get("Duration"));
            this.Recurrence = (string)alarm.Attribute(XName.Get("Recurrence"));
            this.Enabled = int.Parse((string)alarm.Attribute(XName.Get("Enabled"))) != 0;
            this.RoomUUID = (string)alarm.Attribute(XName.Get("RoomUUID"));
            this.ProgramUri = (string)alarm.Attribute(XName.Get("ProgramURI"));
            this.ProgramMetadata = (string)alarm.Attribute(XName.Get("ProgramMetaData"));
            this.PlayMode = (string)alarm.Attribute(XName.Get("PlayMode"));
            this.Volume = int.Parse((string)alarm.Attribute(XName.Get("Volume")));
            this.IncludeLinkedZones = int.Parse((string)alarm.Attribute(XName.Get("IncludeLinkedZones"))) != 0;

            // Calculated values
            if (string.IsNullOrEmpty(this.ProgramMetadata))
            {
                this.Description = StringResource.Get("SonosChime");
            }
            else
            {
                this.Metadata = DidlData.Parse(this.ProgramMetadata).First();
                this.Description = this.Metadata.Title;
            }

            var room = players.FirstOrDefault(p => p.UniqueName == RoomUUID);
            this.RoomName = room != null ? room.RoomName : "Unknown Room";
        }

        public string DisplayStartTime
        {
            get
            {
                // Time is displayed in current culture format
                DateTime dt = new DateTime(2000, 1, 1, StartTime.Hours, StartTime.Minutes, StartTime.Seconds, DateTimeKind.Local);
                return dt.ToString(CultureInfo.CurrentUICulture.DateTimeFormat.ShortTimePattern);
            }
        }

        public string Summary()
        {
            var culture = CultureInfo.CurrentUICulture;
            StringBuilder sb = new StringBuilder(RoomName);
            sb.AppendFormat(" : {0} - ", DisplayStartTime);
            if (Recurrence == Strings.Once)
                sb.Append(StringResource.Get("AlarmSummaryOnce"));
            else if (Recurrence == Strings.Daily)
                sb.Append(StringResource.Get("AlarmSummaryDaily"));
            else if (Recurrence == Strings.Weekdays)
                sb.Append(culture.DateTimeFormat.AbbreviatedDayNames[1] + "-" + culture.DateTimeFormat.AbbreviatedDayNames[5]);
            else if (Recurrence == Strings.Weekends)
                sb.Append(culture.DateTimeFormat.AbbreviatedDayNames[6] + "," + culture.DateTimeFormat.AbbreviatedDayNames[0]);
            else
            {
                for (int i = Strings.OnPrefix.Length; i < Recurrence.Length; i++)
                {
                    int day = int.Parse(Recurrence.Substring(i, 1));
                    sb.Append(culture.DateTimeFormat.AbbreviatedDayNames[day]);
                    if ((Recurrence.Length - i) > 1)
                        sb.Append(", ");
                }
            }
            return sb.ToString();
        }

        // This is used when exporting as readable
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0} {1}% - {2}", Summary(), this.Volume, this.Description);
            if (this.IncludeLinkedZones)
                sb.Append(" (inc. grouped rooms)");
            if (this.PlayMode != Strings.ShuffleOff)
                sb.Append(" (Shuffle)");
            if (!this.Enabled)
                sb.Append(" (Off)");
            return sb.ToString();
        }
    }
}
