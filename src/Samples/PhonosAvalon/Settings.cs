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
        }

        private string SettingsFilename;

        private Settings()
        {
            SettingsFilename = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.Create), "settings.json");
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
                var settings = json.Deserialize<SettingsInFile>();

                _controllerId = settings.controllerId;
                _currentZoneId = settings.currentZoneId;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error reading settings file: {ex.Message}");
            }
        }

        private void Save()
        {
            var settings = new SettingsInFile()
            {
                controllerId = _controllerId,
                currentZoneId = _currentZoneId,
            };

            try
            {
                var json = JsonSerializer.Serialize(settings);
                File.WriteAllText(SettingsFilename, json);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error writing settings file: {ex.Message}");
            }
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
