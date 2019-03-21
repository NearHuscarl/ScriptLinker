using ScriptImporter.Utilities;
using System.Windows.Input;
using ScriptImporter.Models;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows;
using System;
using System.ComponentModel;
using System.Xml.Linq;
using System.Linq;

// https://stackoverflow.com/a/41511598/9449426
// Install-Package WindowsAPICodePack-Shell
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Collections.Generic;
using ScriptImporter.DataLogic;

namespace ScriptImporter.ViewModels
{
    class MainViewModel : ViewModelBase
    {
        private CodeMerger m_merger;
        private readonly string SettingsPath = Path.Combine(Directory.GetCurrentDirectory(), "settings.xml");

        public ICommand BrowseScriptPathCommand { get; private set; }
        public ICommand BrowseProjectDirCommand { get; private set; }
        public ICommand CopyToClipboardCommand { get; private set; }

        private string scriptPath;
        public string ScriptPath
        {
            get { return scriptPath; }
            set { SetPropertyAndNotify(ref scriptPath, value); }
        }

        private string projectDir;
        public string ProjectDir
        {
            get { return projectDir; }
            set
            {
                VSProject.ProjectDirectory = value;
                SetPropertyAndNotify(ref projectDir, value);
            }
        }

        private string rootNamespace;
        public string RootNamespace
        {
            get { return rootNamespace; }
            set { SetPropertyAndNotify(ref rootNamespace, value); }
        }

        private string author;
        public string Author
        {
            get { return author; }
            set { SetPropertyAndNotify(ref author, value); }
        }

        private string description;
        public string Description
        {
            get { return description; }
            set { SetPropertyAndNotify(ref description, value); }
        }

        private string mapModes;
        public string MapModes
        {
            get { return mapModes; }
            set { SetPropertyAndNotify(ref mapModes, value); }
        }

        private bool isStandaloneScript;
        public bool IsStandaloneScript
        {
            get { return isStandaloneScript; }
            set { SetPropertyAndNotify(ref isStandaloneScript, value); }
        }

        public MainViewModel()
        {
            m_merger = new CodeMerger();

            BrowseScriptPathCommand = new DelegateCommand(BrowseScriptPath);
            BrowseProjectDirCommand = new DelegateCommand(BrowseProjectDir);
            CopyToClipboardCommand = new DelegateCommand(CopyToClipboard);

            LoadSettings();
        }

        private void LoadSettings()
        {
            if (!File.Exists(SettingsPath))
                return;

            var doc = XDocument.Load(SettingsPath);
            var settingsElement = doc.Descendants("settings");
            var scriptPath = settingsElement.Descendants("ScriptPath").Select(e => e.Value).FirstOrDefault();
            var projectDir = settingsElement.Descendants("ProjectDir").Select(e => e.Value).FirstOrDefault();
            var author = settingsElement.Descendants("Author").Select(e => e.Value).FirstOrDefault();
            var description = settingsElement.Descendants("Description").Select(e => e.Value).FirstOrDefault();
            var mapModes = settingsElement.Descendants("MapModes").Select(e => e.Value).FirstOrDefault();
            var isStandaloneScript = settingsElement.Descendants("StandaloneScript").Select(e => e.Value).FirstOrDefault();

            ScriptPath = scriptPath;
            ProjectDir = projectDir;
            RootNamespace = VSProject.GetProjectInfo(ProjectDir)
                .Element(VSProject.Msbuild + "Project")
                .Elements(VSProject.Msbuild + "PropertyGroup").First()
                .Elements(VSProject.Msbuild + "RootNamespace")
                .Select(e => e.Value).FirstOrDefault();
            Author = author;
            Description = description;
            MapModes = mapModes;
            IsStandaloneScript = isStandaloneScript == "true" ? true : false;
        }

        private void BrowseScriptPath()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = ".cs",
                Filter = "C# Files (*.cs)|*.cs"
            };

            if (dialog.ShowDialog() == true)
            {
                ScriptPath = dialog.FileName;
                UpdateScriptInfo();
            }
        }

        private void BrowseProjectDir()
        {
            var dialog = new CommonOpenFileDialog
            {
                InitialDirectory = string.IsNullOrEmpty(ProjectDir) ? "C:\\Users" : ProjectDir,
                IsFolderPicker = true
            };

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                ProjectDir = dialog.FileName;
            }
        }

        private void UpdateScriptInfo()
        {
            var outputPath = Path.ChangeExtension(ScriptPath, "txt");
            var scriptInfo = FileUtil.ReadOutputScriptInfo(outputPath);

            Author = scriptInfo.Author;
            Description = scriptInfo.Description;
            MapModes = scriptInfo.MapModes;
        }

        private void CopyToClipboard()
        {
            var info = new ScriptInfo()
            {
                Author = Author,
                Description = Description,
                MapModes = MapModes,
            };
            var sourceCode = m_merger.Merge(ProjectDir, ScriptPath, RootNamespace, info);

            Clipboard.SetText(sourceCode);
            GenerateOutputFile(sourceCode);

            if (IsStandaloneScript)
            {
                CopyToScriptFolder(sourceCode);
            }
        }

        private void GenerateOutputFile(string sourceCode)
        {
            var outputPath = Path.ChangeExtension(ScriptPath, "txt");
            File.WriteAllText(outputPath, sourceCode);
        }

        private void CopyToScriptFolder(string sourceCode)
        {
            var myDocument = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var sfdScriptPath = Path.Combine(myDocument, @"Superfighters Deluxe\Scripts");
            var scriptName = Path.GetFileNameWithoutExtension(ScriptPath) + ".txt";
            var fullPath = Path.Combine(sfdScriptPath, scriptName);

            File.WriteAllText(fullPath, sourceCode);
        }

        public override void OnWindowClosing(object sender, CancelEventArgs e)
        {
            // Save current settings
            var doc = new XDocument(new XElement("settings",
                new XElement("ScriptPath", ScriptPath),
                new XElement("ProjectDir", ProjectDir),
                new XElement("Author", Author),
                new XElement("Description", Description),
                new XElement("MapModes", MapModes),
                new XElement("StandaloneScript", IsStandaloneScript)));

            doc.Save(SettingsPath);
        }
    }
}
