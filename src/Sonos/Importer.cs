using OpenPhonos.UPnP;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace OpenPhonos.Sonos
{
    public interface IImporter
    {
        // This is the name of the Importer (eg "Playlists")
        string Name { get; set; }
        // This is the name of the thing being imported (eg "Sting Faves.xml")
        string DisplayName { get; set; }

        bool RestrictedMode { get; set; }

        Task DoImport(PlayerCallbacks callbacks);
    }

    public class ImporterBase : IImporter
    {
        public string Name { get; set; }

        public string DisplayName { get; set; }

        public bool RestrictedMode { get; set; }

        protected XElement Document;

        public ImporterBase(string name, XElement xml)
        {
            Name = name;
            Document = xml;
        }

        public static IImporter FindImporter(string raw, string displayName)
        {
            IImporter importer = null;
            var xml = XElement.Parse(raw);
            string rootname = xml.Name.LocalName;
            switch (rootname)
            {
                case "Alarms":
                    importer = new AlarmImporter(xml);
                    break;
                case "DIDL-Lite":
                    {
                        var first = xml.Descendants().First();
                        string parent = (string)first.Attribute(XName.Get("parentID"));
                        if (first.Name.LocalName == "item")
                        {
                            switch (parent)
                            {
                                case "R:0/0":
                                    importer = new RadioStationsImporter(xml);
                                    break;
                                case "FV:2":
                                    importer = new FavoritesImporter(xml);
                                    break;
                            }
                            if (importer == null && parent.StartsWith("SQ:"))
                            {
                                importer = new PlaylistImporter(xml);
                            }
                        }
                        else if (first.Name.LocalName=="container")
                        {
                            switch(parent)
                            {
                                case "R:0/1":
                                    importer = new RadioShowsImporter(xml);
                                    break;
                            }
                        }
                    }
                    break;
                case "playlist":
                    throw new Exception("Cannot import Soundiiz playlists, only Sonos playlists");
            }

            if (importer != null)
            {
                importer.DisplayName = displayName;
            }

            return importer;
        }

        public virtual Task DoImport(PlayerCallbacks callbacks)
        {
            throw new Exception("Sorry, cannot import that file");
        }
    }

    public class GenericFileImporter : IImporter
    {
        private IImporter ActualImporter;

        public GenericFileImporter()
        {
            Name = "Generic";
            DisplayName = "";
        }

        public string Name { get; set; }

        public string DisplayName { get; set; }

        public bool RestrictedMode { get; set; }

        public Task DoImport(PlayerCallbacks callbacks)
        {
            return ActualImporter.DoImport(callbacks);
        }

        public void UseImporterFromFile(string name, string content)
        {
            ActualImporter = ImporterBase.FindImporter(content, name);
            if (ActualImporter == null)
                throw new Exception("Cannot import that kind of file");
        }
    }

    public class AlarmImporter : ImporterBase
    {
        public AlarmImporter(XElement xml) : base(StringResource.Get("AlarmsMenuItem"), xml)
        {
        }
    }

    public class FavoritesImporter : ImporterBase
    {
        public FavoritesImporter(XElement xml) : base(StringResource.Get("MusicSonosFavorites"), xml)
        {
        }
    }

    public class PlaylistImporter : ImporterBase, IMusicItemEnumerator
    {
        public PlaylistImporter(XElement xml) : base(StringResource.Get("MusicSonosPlaylists"), xml)
        {
        }

        public MusicItem EmptyItem => null;
        public string ArtUri => null;
        public DisplayData ParentDisplayMode => null;



        public bool CanExport(ExportType exportType)
        {
            return false;
        }

        public Task<int> CountAsync(PlayerCallbacks callbacks)
        {
            return Task.FromResult(-1);
        }

        public Task<string> ExportAsync(PlayerCallbacks player, ExportType exportType)
        {
            throw new NotImplementedException();
        }

        public IMusicItemEnumerator GetChildrenEnumerator(MusicItem parent)
        {
            throw new NotImplementedException();
        }

        int CurrentItem = -1;
        List<UPnPMusicItem> Items;

        public async Task<IEnumerable<MusicItem>> GetNextItemsAsync(PlayerCallbacks callbacks)
        {
            if (CurrentItem == -1)
            {
                Items = UPnPMusicItem.Parse(this.Document, null).ToList();
            }

            CurrentItem++;
            if (CurrentItem >= Items.Count)
                return null;

            return await Task.FromResult(new List<UPnPMusicItem>() { Items[CurrentItem] });
        }

        public void Reset()
        {
            CurrentItem = -1;
        }

        public override async Task DoImport(PlayerCallbacks callbacks)
        {
            var player = callbacks.Player;

            await player.ClearQueue();
            uint index = await player.QueueSize() + 1;
            List<string> md = new List<string>();
            List<string> res = new List<string>();
            uint updateID = 0;
            for (; ; )
            {
                // Do it in chunks as a lot quicker
                for (int i = 0; i < 16; i++)
                {
                    var item = await GetNextItemsAsync(callbacks) as UPnPMusicItem;
                    if (RestrictedMode && i == 5)
                        item = null;
                    if (item == null)
                        break;

                    // If somehow there's a playlist in the playlist, ignore it
                    if (item.Class == "object.item")
                        continue;

                    var detail = await player.ContentDirectory.Browse(item.Id, "BrowseMetadata", "*", 0, 1, "");
                    detail.ThrowIfFailed();

                    md.Add(detail.Result);
                    res.Add(string.IsNullOrEmpty(item.Res) ? "res:" : item.Res);
                }

                if (md.Count == 0)
                    break;

                var added = await player.AVTransport.AddMultipleURIsToQueue(0, updateID, (uint)md.Count, string.Join(" ", res), string.Join(" ", md), string.Empty, string.Empty, index, false);
                added.ThrowIfFailed();
                index += added.NumTracksAdded;
                updateID = added.NewUpdateID;

                md.Clear();
                res.Clear();

                if (RestrictedMode)
                    break;
            }
        }
    }

    public class RadioStationsImporter : ImporterBase
    {
        public RadioStationsImporter(XElement xml) : base(StringResource.Get("MyRadioStations"), xml)
        {
        }
    }
    public class RadioShowsImporter : ImporterBase
    {
        public RadioShowsImporter(XElement xml) : base(StringResource.Get("MyRadioShows"), xml)
        {
        }
    }
}
