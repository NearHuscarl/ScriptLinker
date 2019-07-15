using ScriptLinker.Utilities;
using System.Windows.Input;
using ScriptLinker.Models;
using System.IO;
using System.Windows;
using System;
using System.ComponentModel;
using System.Xml.Linq;
using System.Linq;

// https://stackoverflow.com/a/41511598/9449426
// Install-Package WindowsAPICodePack-Shell
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Collections.Generic;
using ScriptLinker.DataLogic;
using System.Diagnostics;
using System.Timers;
using System.Xml;

namespace ScriptLinker.ViewModels
{
    class MainViewModel : ViewModelBase
    {
        private Linker m_linker;
        private ScheduledTask m_scheduledTask;
        private readonly string SettingsPath = Path.Combine(Directory.GetCurrentDirectory(), "settings.xml");
        //private readonly HotKeyHook m_hotKeyHook;
        private readonly GlobalKeyboardHook m_keyboardHook;

        public ICommand BrowseEntryPointCommand { get; private set; }
        public ICommand BrowseProjectDirCommand { get; private set; }
        public ICommand CopyToClipboardCommand { get; private set; }
        public ICommand CompileCommand { get; private set; }
        public ICommand ExpandLinkedFilesWindowCommand { get; private set; }
        public ICommand OpenFileCommand { get; private set; }

        private string entryPoint;
        public string EntryPoint
        {
            get { return entryPoint; }
            set { SetPropertyAndNotify(ref entryPoint, value); }
        }

        private string projectDir;
        public string ProjectDir
        {
            get { return projectDir; }
            set
            {
                projectDir = value;
                var csProjDoc = VSProject.GetProjectInfo(ProjectDir);

                if (csProjDoc != null)
                {
                    RootNamespace = csProjDoc
                        .Element(VSProject.Msbuild + "Project")
                        .Elements(VSProject.Msbuild + "PropertyGroup").First()
                        .Elements(VSProject.Msbuild + "RootNamespace")
                        .Select(e => e.Value).FirstOrDefault();
                }
                else
                {
                    RootNamespace = "";
                }

                SetPropertyAndNotify(ref projectDir, projectDir);
            }
        }

        private string rootNamespace;
        public string RootNamespace
        {
            get { return rootNamespace; }
            set { SetPropertyAndNotify(ref rootNamespace, value); }
        }

        private ProjectInfo ProjectInfo
        {
            get { return new ProjectInfo(projectDir, entryPoint, rootNamespace); }
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

        private ScriptInfo ScriptInfo
        {
            get { return new ScriptInfo(author, description, mapModes); }
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

        private bool isLinkedFileWindowExpanded = false;
        public bool IsLinkedFileWindowExpanded
        {
            get { return isLinkedFileWindowExpanded; }
            set { SetPropertyAndNotify(ref isLinkedFileWindowExpanded, value); }
        }

        private string expandIcon = "▼"; //▲
        public string ExpandIcon
        {
            get { return expandIcon; }
            set { SetPropertyAndNotify(ref expandIcon, value); }
        }

        private HashSet<string> linkedFiles;
        public HashSet<string> LinkedFiles
        {
            get { return linkedFiles; }
            set { SetPropertyAndNotify(ref linkedFiles, value); }
        }

        public MainViewModel()
        {
            m_keyboardHook = new GlobalKeyboardHook();
            m_keyboardHook.HookedKeys.Add(System.Windows.Forms.Keys.F4);
            m_keyboardHook.KeyUp += (sender, e) => Compile(null);

            m_linker = new Linker();
            m_scheduledTask = new ScheduledTask();

            BrowseEntryPointCommand = new DelegateCommand(BrowseEntryPoint);
            BrowseProjectDirCommand = new DelegateCommand(BrowseProjectDir);
            CopyToClipboardCommand = new DelegateCommand(CopyToClipboard);
            CompileCommand = new DelegateCommand(Compile);
            ExpandLinkedFilesWindowCommand = new DelegateCommand(ExpandLinkedFilesWindow);
            OpenFileCommand = new DelegateCommand(OpenFile);

            LoadSettings();
        }

        private void Compile(object param)
        {
            var scriptEditorProcess = Process.GetProcessesByName("Superfighters Deluxe").FirstOrDefault();
            // Focus on Script Editor window
            if (scriptEditorProcess != null)
            {
                CopyToClipboard(null);
                
                WinUtil.BringMainWindowToFront(scriptEditorProcess);
                // Tab to focus in the editor's text area if not already
                WinUtil.Simulate(scriptEditorProcess, "{TAB}");
                // CTRL-A Select all text in editor
                WinUtil.Simulate(scriptEditorProcess, "^(a)");
                // CTRL-V Paste clipboard content
                WinUtil.Simulate(scriptEditorProcess, "^(v)");
                // Compile newly pasted code
                WinUtil.Simulate(scriptEditorProcess, "{F5}");
            }
        }

        private void LoadSettings()
        {
            if (!File.Exists(SettingsPath))
                return;

            var doc = XDocument.Load(SettingsPath);
            var settingsElement = doc.Descendants("settings");
            var entryPoint = settingsElement.Descendants("EntryPoint").Select(e => e.Value).FirstOrDefault();
            var projectDir = settingsElement.Descendants("ProjectDirectory").Select(e => e.Value).FirstOrDefault();

            var scriptInfo = ScriptInfo.Empty;
            if (!string.IsNullOrEmpty(entryPoint))
            {
                var outputPath = Path.ChangeExtension(entryPoint, "txt");
                scriptInfo = FileUtil.ReadOutputScriptInfo(outputPath);
            }

            if (!scriptInfo.IsEmpty)
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
            var isLinkedFileWindowExpanded = settingsElement.Descendants("IsLinkedFileWindowExpanded").Select(e => e.Value).FirstOrDefault();

            EntryPoint = entryPoint;
            ProjectDir = projectDir;
            Author = author;
            Description = description;
            MapModes = mapModes;
            IsStandaloneScript = XmlConvert.ToBoolean(isStandaloneScript);
            IsLinkedFileWindowExpanded = XmlConvert.ToBoolean(isLinkedFileWindowExpanded);
        }

        private void BrowseEntryPoint(object param)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = ".cs",
                Filter = "C# Files (*.cs)|*.cs",
                InitialDirectory = ProjectDir,
            };

            if (dialog.ShowDialog() == true)
            {
                EntryPoint = dialog.FileName;
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
            var outputPath = Path.ChangeExtension(EntryPoint, "txt");
            var scriptInfo = FileUtil.ReadOutputScriptInfo(outputPath);

            Author = scriptInfo.Author;
            Description = scriptInfo.Description;
            MapModes = scriptInfo.MapModes;
        }

        private void CopyToClipboard(object param)
        {
            var stopwatch = new Stopwatch();
            stopwatch.StartPrinting();

            //var sourceCode = await Task.Run(() => m_linker.Link(ProjectInfo, ScriptInfo));
            var result = m_linker.Link(ProjectInfo, ScriptInfo);
            var sourceCode = result.Content;
            LinkedFiles = result.LinkedFiles;
            stopwatch.PrintTime("CopyToClipboard() Link");

            Clipboard.SetText(sourceCode);
            stopwatch.PrintTime("CopyToClipboard() ToClipboard");
            GenerateOutputFile(sourceCode);
            var message = $"Successfully linked {result.LinkedFiles.Count} files! ({stopwatch.ElapsedMilliseconds} ms)";
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
            var outputPath = Path.ChangeExtension(EntryPoint, "txt");
            await FileUtil.WriteTextAsync(outputPath, sourceCode);

            if (IsStandaloneScript)
            {
                CopyToScriptFolder(sourceCode);
            }
        }

        private async void CopyToScriptFolder(string sourceCode)
        {
            var sourcePath = Path.ChangeExtension(EntryPoint, "txt");
            var myDocument = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var sfdScriptPath = Path.Combine(myDocument, @"Superfighters Deluxe\Scripts");
            var scriptName = Path.GetFileNameWithoutExtension(EntryPoint) + ".txt";
            var destinationPath = Path.Combine(sfdScriptPath, scriptName);

            await FileUtil.CopyFileAsync(sourcePath, destinationPath);
        }

        private void ExpandLinkedFilesWindow(object param)
        {
            IsLinkedFileWindowExpanded = !IsLinkedFileWindowExpanded;

            if (IsLinkedFileWindowExpanded)
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
                new XElement("EntryPoint", EntryPoint),
                new XElement("ProjectDirectory", ProjectDir),
                new XElement("Author", Author),
                new XElement("Description", Description),
                new XElement("MapModes", MapModes),
                new XElement("StandaloneScript", IsStandaloneScript),
                new XElement("IsLinkedFileWindowExpanded", IsLinkedFileWindowExpanded)));

            doc.Save(SettingsPath);
        }
    }
}
