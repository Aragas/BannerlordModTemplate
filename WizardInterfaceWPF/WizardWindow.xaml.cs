using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Forms;

namespace WizardInterfaceWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class WizardWindow : MetroWindow
    {
        // TODO: Two strings for directory? One escaped, one non-escaped?
        public string BannerlordDirectory => PathTextBox.Text.Replace("&", "&amp;");
        public bool IncludeSubModule => IncludeSubModuleCheckBox.IsChecked ?? false;
        public bool IncludeReadme => IncludeReadmeCheckBox.IsChecked ?? false;
        public bool IncludeHarmony => IncludeHarmonyCheckBox.IsChecked ?? false;
        public bool UseLauncherMods => UseLauncherModulesCheckBox.IsChecked ?? false;
        public List<string> LauncherMods => GameFinder.GetLauncherModules().ToList();


        public WizardWindow()
        {
            InitializeComponent();
        }

        private void ButtonClick_Manager(object sender, RoutedEventArgs e)
        {
            if (ReferenceEquals(sender, BrowsePathButton))
            {
                using var folderBrowser = new FolderBrowserDialog
                {
                    RootFolder = Environment.SpecialFolder.MyComputer,
                    ShowNewFolderButton = false,
                    Description = "Browse to M&B 2: Bannerlord Root Folder"
                };

                if (folderBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    if (GameFinder.VerifyInstallPath(folderBrowser.SelectedPath))
                    {
                        ConfirmButton.IsEnabled = true;
                        PathTextBox.Text = folderBrowser.SelectedPath;
                    }
                    else
                    {
                        BannerlordPathFailed(1);
                    }
                }
                else
                {
                    // User cancelled or closed the dialog
                }
            }

            if (ReferenceEquals(sender, ConfirmButton))
                DialogResult = true;

            if (ReferenceEquals(sender, CancelButton))
                DialogResult = false;

            if (ReferenceEquals(sender, GitHubButton))
                System.Diagnostics.Process.Start("https://github.com/Dealman/BannerlordModTemplate");

            if (ReferenceEquals(sender, ForumButton))
                System.Diagnostics.Process.Start("https://forums.taleworlds.com/index.php?threads/release-mod-template-for-visual-studio-automatically-configs-adds-references-and-more.413981/");
        }

        private void MetroWindow_ContentRendered(object sender, EventArgs e)
        {
            var installPath = GameFinder.GetLocation();
            if (installPath != null && GameFinder.VerifyInstallPath(installPath))
            {
                ConfirmButton.IsEnabled = true;
                PathTextBox.Text = installPath;
                return;
            }


            BannerlordPathFailed(0);
        }

        private async void BannerlordPathFailed(int reason)
        {
            switch (reason)
            {
                case 0:
                    await this.ShowMessageAsync("Warning",
                        "Unable to automatically locate M&B 2: Bannerlord.\n\nPlease, try and locate it manually instead.");
                    break;
                case 1:
                    await this.ShowMessageAsync("Warning",
                        "Selected folder failed to verify! Are you sure this is the root folder for Bannerlord? Try again.\n\nExample:\n" +
                        @"C:\Program Files (x86)\Steam\steamapps\common\Mount & Blade II Bannerlord");
                    break;
            }
        }
    }
}
