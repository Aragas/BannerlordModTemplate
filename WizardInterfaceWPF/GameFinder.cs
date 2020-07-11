#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace WizardInterfaceWPF
{
    internal static class GameFinder
    {
        public static string? GetLocation()
        {
            // TODO: Epic Game Launcher
            var steamSore = new SteamStore();
            return steamSore.FindByAppId("261550")?.GamePath;
        }

        public static bool VerifyInstallPath(string path) => File.Exists(path + @"\bin\Win64_Shipping_Client\Bannerlord.exe");

        public static IEnumerable<string> GetLauncherModules()
        {
            var documentsDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var launcherConfig = documentsDirectory + @"\Mount and Blade II Bannerlord\Configs\LauncherData.xml";

            if (File.Exists(launcherConfig))
            {
                var config = XDocument.Load(launcherConfig);
                if (config?.Root != null)
                {
                    var xmlQuery = config.Root.Descendants("UserModData")
                        .Where(x => ((bool) x.Element("IsSelected")) && (x.Parent?.Parent?.Name != "MultiplayerData"))
                        .Select(x => x.Element("Id"));

                    foreach (string moduleName in xmlQuery)
                        yield return moduleName;
                }
            }
        }
    }
}