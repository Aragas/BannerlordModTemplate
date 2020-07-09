using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace WizardInterfaceWPF
{
    internal static class GameFinder
    {
        public static string GetLocationViaRegistry()
        {
            var steamInstallPath = Registry.GetValue(
                @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 261550",
                "InstallLocation", null);

            if (steamInstallPath is string str && !string.IsNullOrWhiteSpace(str))
                return str;


            return null;
        }

        public static bool VerifyInstallPath(string path) =>
            !string.IsNullOrWhiteSpace(path) && File.Exists(path + @"\bin\Win64_Shipping_Client\Bannerlord.exe");

        public static List<string> GetLauncherModules()
        {
            var moduleList = new List<string>();
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
                    {
                        moduleList.Add(moduleName);
                    }

                    return moduleList;
                }

                return null;
            }
            else
            {
                return null;
            }
        }
    }
}