#nullable enable
using Gameloop.Vdf;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace WizardInterfaceWPF
{
    public class SteamStore
    {
        private readonly string? _baseFolder;
        private SteamStoreEntry[]? _cache;

        public SteamStore()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\Valve\Steam", "SteamPath", null) is string path)
                    _baseFolder = path;
                else
                    _baseFolder = null;
            }
            else
                _baseFolder = Path.Combine(Environment.GetEnvironmentVariable("HOME") ?? "/home", ".steam", "stream");
        }

        public SteamStoreEntry[] AllGames() => _cache ??= ParseManifests().ToArray();

        public SteamStoreEntry? FindByAppId(string appId) => AllGames().FirstOrDefault(x => x.AppId == appId);

        public SteamStoreEntry? FindByName(string namePattern)
        {
            var regex = new Regex(namePattern);
            return AllGames().FirstOrDefault(x => regex.IsMatch(x.Name));
        }

        public IEnumerable<SteamStoreEntry> ParseManifests()
        {
            if (_baseFolder == null)
                yield break;

            var steamPaths = new List<string> { _baseFolder };
            
            var config = VdfConvert.Deserialize(File.ReadAllText(Path.Combine(_baseFolder, "config", "config.vdf")));
            for (var counter = 1; ; counter++)
            {
                if (config.Value["Software"]?["Valve"]?["Steam"]?[$"BaseInstallFolder_{counter}"] is { } baseInstallFolder)
                    steamPaths.Add(baseInstallFolder.ToString());
                else
                    break;
            }

            foreach (var steamPath in steamPaths)
            {
                var path = Path.Combine(steamPath, "steamapps");
                var files = Directory.GetFiles(path, "appmanifest_*.acf", SearchOption.TopDirectoryOnly);
                foreach (var file in files)
                {
                    var appManifest = VdfConvert.Deserialize(File.ReadAllText(file));
                    if (appManifest.Key == "AppState")
                        yield return new SteamStoreEntry(
                            appManifest.Value["appid"]?.ToString() ?? "",
                            appManifest.Value["name"]?.ToString() ?? "",
                            Path.Combine(path, "common", appManifest.Value["installdir"]?.ToString() ?? ""),
                            DateTimeOffset.FromUnixTimeSeconds(long.TryParse(appManifest.Value["LastUpdated"]?.ToString() ?? "0", out var l) ? l : 0));
                }
            }
        }
    }
}