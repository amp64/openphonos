using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Text.Json;
using Avalonia.Styling;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Extensions.Logging;

namespace PhonosAvalon
{
    // Best discussion of why ConfigurationProvider can't Save anything:
    // https://stackoverflow.com/questions/57978535/save-changes-of-iconfigurationroot-sections-to-its-json-file-in-net-core-2-2

    internal class Settings
    {
        private static Settings? _Instance;

        internal static Settings Instance
        {
            get
            {
                _Instance ??= new();
                return _Instance;
            }
        }

        private struct SettingsInFile
        {
            public string? controllerId { get; set; }
            public string? currentZoneId { get; set; }
            public string? lastVersion { get; set; }
            public string? lastMessage { get; set; }
            public string? hosts { get; set; }
            public int theme { get; set; }
            public string? windowPos { get; set; }
        }

        private string SettingsFilename;

        private Settings()
        {
            SettingsFilename = Path.Combine((App.Current as App).DataFilePath, "settings.json");
            Load();
        }

        private void Load()
        {
            if (!File.Exists(SettingsFilename))
            {
                return;
            }

            try
            {
                var all = File.ReadAllText(SettingsFilename);
                var json = System.Text.Json.JsonDocument.Parse(all);
                var settings = json.Deserialize<SettingsInFile>(Options());

                _controllerId = settings.controllerId;
                _currentZoneId = settings.currentZoneId;
                _lastVersion = settings.lastVersion;
                _lastMessage = settings.lastMessage;
                _hosts = settings.hosts;
                _theme = settings.theme;
                _windowPos = settings.windowPos;
            }
            catch (Exception ex)
            {
                var log = App.AppLoggerFactory.CreateLogger("App");
                log.LogError("Error reading settings file: {message} {file}", ex.Message, SettingsFilename);
            }
        }

        private void Save()
        {
            var settings = new SettingsInFile()
            {
                controllerId = _controllerId,
                currentZoneId = _currentZoneId,
                lastVersion = _lastVersion,
                lastMessage = _lastMessage,
                hosts = _hosts,
                theme = _theme,
                windowPos = _windowPos
            };

            try
            {
                var json = JsonSerializer.Serialize(settings, Options());
                File.WriteAllText(SettingsFilename, json);
            }
            catch (Exception ex)
            {
                var log = App.AppLoggerFactory.CreateLogger("App");
                log.LogError("Error writing settings file: {message} {file}", ex.Message, SettingsFilename);
            }
        }

        internal void Reset()
        {
            try
            {
                File.Delete(SettingsFilename);
            }
            catch (Exception ex)
            {
                var log = App.AppLoggerFactory.CreateLogger("App");
                log.LogError("Error deleting settings file: {message} {file}", ex.Message, SettingsFilename);
            }
        }

        private JsonSerializerOptions Options()
        {
            return new JsonSerializerOptions()
            {
                TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
                WriteIndented = true
            };
        }
        
        private string? _controllerId;
        internal string? ControllerId
        {
            get => _controllerId; 
            set => Setter(value, ref _controllerId);
        }

        private string? _currentZoneId;
        internal string? CurrentZoneId
        {
            get => _currentZoneId;
            set => Setter(value, ref _currentZoneId);
        }

        private string? _lastVersion;
        internal string? LastVersion
        {
            get => _lastVersion;
            set => Setter(value, ref _lastVersion);
        }

        private string? _lastMessage;
        internal string? LastMessage
        {
            get => _lastMessage;
            set => Setter(value, ref _lastMessage);
        }

        private string? _hosts;
        internal string? Hosts
        {
            get => _hosts;
            set => Setter(value, ref _hosts);
        }

        // 0=default system theme
        // 1=dark
        // 2=light
        private int _theme;
        internal int Theme
        {
            get => _theme;
            set => Setter(value, ref _theme);
        }

        private string? _windowPos;
        internal string? WindowPos
        {
            get => _windowPos;
            set => Setter(value, ref _windowPos);
        }

        private static void Setter<T>(T value, ref T member, [CallerMemberName] string? name = null)
        {
            if (value != null)
            {
                if (value.Equals(member))
                {
                    return;
                }
            }
            else if (member == null)
            {
                return;
            }

            member = value;
            Instance.Save();
        }
    }
}
