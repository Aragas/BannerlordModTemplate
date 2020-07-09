using EnvDTE;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TemplateWizard;
using NuGet.VisualStudio;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using WizardInterfaceWPF;

namespace BannerlordModVSX
{
    public class TemplateWizard : IWizard
    {
        // TODO: This can probably be cleaned up a bit
        private WizardWindow _wizardWindow;
        private string _bannerlordDirectory;
        private string _bannerlordExe;
        private bool _createSubModule;
        private bool _createReadme;
        private bool _addHarmony;
        private bool _useLauncherMods;
        private Dictionary<string, string> _replacementsDictionary;

        public void BeforeOpeningFile(ProjectItem projectItem)
        {

        }

        public void ProjectFinishedGenerating(Project project)
        {
            // Visual Studio wants this or it gets very sad
            ThreadHelper.ThrowIfNotOnUIThread();

            var componentModel = (IComponentModel) Package.GetGlobalService(typeof(SComponentModel));
            var installer = componentModel.GetService<IVsPackageInstaller2>();

            #region Installation of NuGet Packages

            if (_addHarmony)
                installer.InstallLatestPackage(null, project, "Lib.Harmony", false, false);

            #endregion

            #region Specific Item Removal (SubModule, Readme...)

            var _dte = project.DTE;

            if (!_createSubModule)
                _dte.Solution.FindProjectItem("SubModule.cs").Remove();

            if (!_createReadme)
                _dte.Solution.FindProjectItem("Readme.txt").Remove();

            #endregion
        }

        public void ProjectItemFinishedGenerating(ProjectItem projectItem)
        {
            // Executed after an ItemTemplate is finished generating
        }

        public void RunFinished()
        {
            // Executed after the project is finished generating
        }

        public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary,
            WizardRunKind runKind, object[] customParams)
        {
            // Store the reference of the replacements dictionary, used later for deleting the project if it was cancelled
            _replacementsDictionary = replacementsDictionary;
            var projectName = replacementsDictionary["$projectname$"];
            var destinationDirectory = replacementsDictionary["$destinationdirectory$"];

            try
            {
                // Create a new instance of the WizardWindow
                _wizardWindow = new WizardWindow();

                if (_wizardWindow.ShowDialog() == true)
                {
                    // Set up the paths we need for the configuration. & should automatically be escaped with &amp;
                    _bannerlordDirectory = _wizardWindow.BannerlordDirectory;
                    _bannerlordExe = _bannerlordDirectory + @"\bin\Win64_Shipping_Client\Bannerlord.exe";
                    _createSubModule = _wizardWindow.IncludeSubModule;
                    _createReadme = _wizardWindow.IncludeReadme;
                    _addHarmony = _wizardWindow.IncludeHarmony;
                    _useLauncherMods = _wizardWindow.UseLauncherMods;

                    // Parse the Bannerlord Launcher to find what modules were last used
                    var argumentString = new StringBuilder();
                    var moduleList = _wizardWindow.LauncherMods;

                    if (moduleList != null && moduleList.Count >= 1 && _useLauncherMods)
                    {
                        argumentString.Append("/singleplayer _MODULES_*");
                        foreach (var module in moduleList)
                        {
                            argumentString.Append($"{module}*");
                        }

                        argumentString.Append($"{replacementsDictionary["$safeprojectname$"]}*_MODULES_");
                    }
                    else
                    {
                        argumentString.Append($"/singleplayer _MODULES_*Native*SandBoxCore*SandBox*StoryMode*CustomBattle*{replacementsDictionary["$safeprojectname$"]}*_MODULES_");
                    }

                    // Add our custom replacements to the dictionary
                    replacementsDictionary.Add("$BannerlordDirectory$", _bannerlordDirectory);
                    replacementsDictionary.Add("$BannerlordExecutable$", _bannerlordExe);
                    replacementsDictionary.Add("$BannerlordDebugArgs$", argumentString.ToString());

                    // We should be done now, close the window
                    _wizardWindow.Close();
                }
                else
                {
                    // The user clicked cancel or closed the window, throw the exception
                    _wizardWindow.Close();
                    throw new WizardBackoutException();
                }
            }
            catch (Exception ex)
            {

                if (ex.GetType().IsAssignableFrom(typeof(WizardBackoutException)) ||
                    ex.GetType().IsAssignableFrom(typeof(WizardCancelledException)))
                {
                    // Project folder would still have been created, clean it up if the user decided to back out
                    var projectFolder = Path.GetFullPath(Path.Combine(destinationDirectory, @"..\"));

                    Directory.Delete(Directory.Exists(projectFolder) ? projectFolder : destinationDirectory, true);
                    throw;
                }
                else
                {
                    MessageBox.Show($"An error has occurred!\n\nError Message:\n{ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public bool ShouldAddProjectItem(string filePath) => true;
    }
}