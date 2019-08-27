﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using XCOM2Launcher.XCOM;

namespace XCOM2Launcher.Forms
{
    public partial class SettingsDialog : Form
    {
        protected Settings Settings { get; set; }

        public bool IsRestartRequired { get; private set; }

        public SettingsDialog(Settings settings)
        {
            InitializeComponent();

            Settings = settings;

            // Restore states
            gamePathTextBox.Text = settings.GamePath;

            closeAfterLaunchCheckBox.Checked = settings.CloseAfterLaunch;
            searchForUpdatesCheckBox.Checked = settings.CheckForUpdates;
            showHiddenEntriesCheckBox.Checked = settings.ShowHiddenElements;
            autoNumberModIndexesCheckBox.Checked = settings.AutoNumberIndexes;
            useModSpecifiedCategoriesCheckBox.Checked = settings.UseSpecifiedCategories;
            neverAdoptTagsAndCatFromprofile.Checked = settings.NeverImportTags;
            ShowQuickLaunchArgumentsToggle.Checked = settings.ShowQuickLaunchArguments;
            checkForPreReleaseUpdates.Checked = settings.CheckForPreReleaseUpdates;
            useSentry.Checked = Properties.Settings.Default.IsSentryEnabled;
            allowMutipleInstances.Checked = settings.AllowMultipleInstances;

            checkForPreReleaseUpdates.Enabled = searchForUpdatesCheckBox.Checked;

            foreach (var modPath in settings.ModPaths)
                modPathsListbox.Items.Add(modPath);

            argumentsTextBox.Text = string.Join(" ", settings.ArgumentList);

            // Create autofill values for arguments box
            List<string> arguments = new List<string>();
            foreach (var propertyInfo in typeof(Arguments).GetProperties())
            {
                var attrs = propertyInfo.GetCustomAttributes(true);
                arguments.AddRange(
                                   from attrName in attrs.OfType<DisplayNameAttribute>()
                                   where !propertyInfo.Name.Equals("Custom")
                                   select attrName.DisplayName);
            }

            argumentsTextBox.Values = arguments.ToArray();



        }

        private void BrowseGamePathButtonOnClick(object sender, EventArgs eventArgs)
        {
            var dialog = new OpenFileDialog
            {
                FileName = "XCom2.exe",
                Filter = @"XCOM 2 Executable|XCom2.exe",
                RestoreDirectory = true,
                InitialDirectory = gamePathTextBox.Text
            };

            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            var path = Path.GetFullPath(Path.Combine(dialog.FileName, "../../.."));
            gamePathTextBox.Text = path;
            Settings.GamePath = path;
        }

        private void RemoveModPathButtonOnClick(object sender, EventArgs e)
        {
            if (modPathsListbox.SelectedItem == null)
                return;

            var path = (string) modPathsListbox.SelectedItem;
            modPathsListbox.Items.Remove(path);
            Settings.ModPaths.Remove(path);
        }

        private void AddModPathButtonOnClick(object sender, EventArgs eventArgs)
        {
            var dialog = new FolderBrowserDialog
            {
                ShowNewFolderButton = true,
                RootFolder = Environment.SpecialFolder.MyComputer,
                Description = "Add a new mod path. Note: This should be the directory that contains the mod directories."
            };

            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            Settings.ModPaths.Add(dialog.SelectedPath + "\\");
            modPathsListbox.Items.Add(dialog.SelectedPath + "\\");
        }

        private void SettingsDialog_Shown(object sender, EventArgs e)
        {
            // if (Settings.Windows.ContainsKey("settings"))
            //     Bounds = Settings.Windows["settings"].Bounds;
        }

        private void bOK_Click(object sender, EventArgs e)
        {
            // indicate if some changes require an application restart
            IsRestartRequired = useSentry.Checked != Properties.Settings.Default.IsSentryEnabled;

            // Apply changes
            Settings.GamePath = Path.GetFullPath(gamePathTextBox.Text);
            Settings.CloseAfterLaunch = closeAfterLaunchCheckBox.Checked;
            Settings.CheckForUpdates = searchForUpdatesCheckBox.Checked;
            Settings.ShowHiddenElements = showHiddenEntriesCheckBox.Checked;
            Settings.AutoNumberIndexes = autoNumberModIndexesCheckBox.Checked;
            Settings.UseSpecifiedCategories = useModSpecifiedCategoriesCheckBox.Checked;
            Settings.NeverImportTags = neverAdoptTagsAndCatFromprofile.Checked;
            Settings.ShowQuickLaunchArguments = ShowQuickLaunchArgumentsToggle.Checked;
            Settings.CheckForPreReleaseUpdates = checkForPreReleaseUpdates.Checked;
            Properties.Settings.Default.IsSentryEnabled = useSentry.Checked;
            Settings.AllowMultipleInstances = allowMutipleInstances.Checked;

            var newArguments = argumentsTextBox.Text.Split(' ').Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();
            Settings.ArgumentList = newArguments.AsReadOnly();

            // Save dimensions
            Settings.Windows["settings"] = new WindowSettings(this);

            Properties.Settings.Default.Save();
            DialogResult = DialogResult.OK;
            Close();
        }

        private void searchForUpdatesCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            checkForPreReleaseUpdates.Enabled = searchForUpdatesCheckBox.Checked;
        }
    }
}