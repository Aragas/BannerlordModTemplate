#nullable enable
using System;

namespace WizardInterfaceWPF
{
    public class SteamStoreEntry
    {
        public string AppId { get; }
        public string Name { get; }
        public string GamePath { get; }
        public DateTimeOffset? LastUpdated { get; }

        public SteamStoreEntry(string appId, string name, string gamePath, DateTimeOffset? lastUpdated = default)
        {
            AppId = appId;
            Name = name;
            GamePath = gamePath;
            LastUpdated = lastUpdated;
        }

        public override string ToString() => $"[{AppId}] {Name}";
    }
}