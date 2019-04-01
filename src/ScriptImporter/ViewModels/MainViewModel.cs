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
using System.Diagnostics;
using System.Threading.Tasks;
using System.Timers;

namespace ScriptImporter.ViewModels
{
    class MainViewModel : ViewModelBase
    {
        private CodeMerger m_merger;
        private ScheduledTask m_scheduledTask;
        private readonly string SettingsPath = Path.Combine(Directory.GetCurrentDirectory(), "settings.xml");

        public ICommand BrowseScriptPathCommand { get; private set; }
        public ICommand BrowseProjectDirCommand { get; private set; }
        public ICommand CopyToClipboardCommand { get; private set; }
        public ICommand ExpandMergedFilesWindowCommand { get; private set; }
        public ICommand OpenFileCommand { get; private set; }

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

        private string successInfo;
        public string SuccessInfo
        {
            get { return successInfo; }
            set { SetPropertyAndNotify(ref successInfo, value); }
        }

        private bool isMergedFileWindowExpanded = false;
        public bool IsMergedFileWindowExpanded
        {
            get { return isMergedFileWindowExpanded; }
            set { SetPropertyAndNotify(ref isMergedFileWindowExpanded, value); }
        }

        private string expandIcon = "▼"; //▲
        public string ExpandIcon
        {
            get { return expandIcon; }
            set { SetPropertyAndNotify(ref expandIcon, value); }
        }

        private HashSet<string> mergedFiles;
        public HashSet<string> MergedFiles
        {
            get { return mergedFiles; }
            set { SetPropertyAndNotify(ref mergedFiles, value); }
        }

        public MainViewModel()
        {
            m_merger = new CodeMerger();
            m_scheduledTask = new ScheduledTask();

            BrowseScriptPathCommand = new DelegateCommand(BrowseScriptPath);
            BrowseProjectDirCommand = new DelegateCommand(BrowseProjectDir);
            CopyToClipboardCommand = new DelegateCommand(CopyToClipboard);
            ExpandMergedFilesWindowCommand = new DelegateCommand(ExpandMergedFilesWindow);
            OpenFileCommand = new DelegateCommand(OpenFile);

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

            ScriptInfo scriptInfo = null;
            if (!string.IsNullOrEmpty(scriptPath))
            {
                var outputPath = Path.ChangeExtension(scriptPath, "txt");
                scriptInfo = FileUtil.ReadOutputScriptInfo(outputPath);
            }

            if (scriptInfo != null)
            {
                author = scriptInfo.Author;
                description = scriptInfo.Description;
                mapModes = scriptInfo.MapModes;
            }
            else
            {
                author = settingsElement.Descendants("Author").Select(e => e.Value).FirstOrDefault();
                description = settingsElement.Descendants("Description").Select(e => e.Value).FirstOrDefault();
                mapModes = settingsElement.Descendants("MapModes").Select(e => e.Value).FirstOrDefault();
            }
            var isStandaloneScript = settingsElement.Descendants("StandaloneScript").Select(e => e.Value).FirstOrDefault();
            var isMergedFileWindowExpanded = settingsElement.Descendants("IsMergedFileWindowExpanded").Select(e => e.Value).FirstOrDefault();

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
            IsMergedFileWindowExpanded = isMergedFileWindowExpanded == "true" ? true : false;
        }

        private void BrowseScriptPath(object param)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = ".cs",
                Filter = "C# Files (*.cs)|*.cs",
                InitialDirectory = ProjectDir,
            };

            if (dialog.ShowDialog() == true)
            {
                ScriptPath = dialog.FileName;
                UpdateScriptInfo();
            }
        }

        private void BrowseProjectDir(object param)
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

        private void CopyToClipboard(object param)
        {
            var stopwatch = new Stopwatch();
            stopwatch.StartPrinting();

            var info = new ScriptInfo()
            {
                Author = Author,
                Description = Description,
                MapModes = MapModes,
            };

            //var sourceCode = await Task.Run(() => m_merger.Merge(ProjectDir, ScriptPath, RootNamespace, info));
            var mergeResult = m_merger.Merge(ProjectDir, ScriptPath, RootNamespace, info);
            var sourceCode = mergeResult.Content;
            MergedFiles = mergeResult.MergedFiles;
            stopwatch.PrintTime("CopyToClipboard() Merge");

            Clipboard.SetText(sourceCode);
            stopwatch.PrintTime("CopyToClipboard() ToClipboard");
            GenerateOutputFile(sourceCode);
            var message = $"Successfully merged {mergeResult.MergedFiles.Count} files! ({stopwatch.ElapsedMilliseconds} ms)";
            ShowSuccessMessage(message, 5000);
            stopwatch.PrintTime("CopyToClipboard() Gen output file");
        }

        private Timer m_successMessageTimer = new Timer();
        private void ShowSuccessMessage(string message, int timeoutMs)
        {
            SuccessInfo = message;

            m_successMessageTimer.Stop();
            m_successMessageTimer.Elapsed += (sender, e) => SuccessInfo = "";
            m_successMessageTimer.AutoReset = false; // run once
            m_successMessageTimer.Interval = timeoutMs;
            m_successMessageTimer.Start();
        }

        private async void GenerateOutputFile(string sourceCode)
        {
            var outputPath = Path.ChangeExtension(ScriptPath, "txt");
            await FileUtil.WriteTextAsync(outputPath, sourceCode);

            if (IsStandaloneScript)
            {
                CopyToScriptFolder(sourceCode);
            }
        }

        private async void CopyToScriptFolder(string sourceCode)
        {
            var sourcePath = Path.ChangeExtension(ScriptPath, "txt");
            var myDocument = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var sfdScriptPath = Path.Combine(myDocument, @"Superfighters Deluxe\Scripts");
            var scriptName = Path.GetFileNameWithoutExtension(ScriptPath) + ".txt";
            var destinationPath = Path.Combine(sfdScriptPath, scriptName);

            await FileUtil.CopyFileAsync(sourcePath, destinationPath);
        }

        private void ExpandMergedFilesWindow(object param)
        {
            IsMergedFileWindowExpanded = !IsMergedFileWindowExpanded;

            if (IsMergedFileWindowExpanded)
                ExpandIcon = "▲";
            else
                ExpandIcon = "▼";
    }

    public void OpenFile(object filePath)
        {
            Process.Start((string)filePath);
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
                new XElement("StandaloneScript", IsStandaloneScript),
                new XElement("IsMergedFileWindowExpanded", IsMergedFileWindowExpanded)));

            doc.Save(SettingsPath);
        }
    }
}
