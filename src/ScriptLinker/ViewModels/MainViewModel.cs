using ScriptLinker.Utilities;
using System.Windows.Input;
using ScriptLinker.Models;
using System.IO;
using System.Windows;
using System;
using System.ComponentModel;
using System.Linq;

// https://stackoverflow.com/a/41511598/9449426
// Install-Package WindowsAPICodePack-Shell
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Collections.Generic;
using ScriptLinker.DataLogic;
using System.Diagnostics;
using System.Timers;
using ScriptLinker.Access;

namespace ScriptLinker.ViewModels
{
    class MainViewModel : ViewModelBase
    {
        private Linker m_linker;
        private ScheduledTask m_scheduledTask;
        private readonly GlobalKeyboardHook m_keyboardHook;
        private SettingsAccess m_settingsAccess;
        private ScriptAccess m_scriptAccess;
        private VisualSln m_visualSln;
        private FileSystemWatcher m_fileWatcher;

        public ICommand BrowseEntryPointCommand { get; private set; }
        public ICommand OpenEntryPointCommand { get; private set; }
        public ICommand BrowseProjectDirCommand { get; private set; }
        public ICommand OpenProjectDirCommand { get; private set; }
        public ICommand CopyToClipboardCommand { get; private set; }
        public ICommand CompileCommand { get; private set; }
        public ICommand ExpandLinkedFilesWindowCommand { get; private set; }
        public ICommand OpenFileCommand { get; private set; }

        private string entryPoint;
        public string EntryPoint
        {
            get { return entryPoint; }
            set
            {
                SetPropertyAndNotify(ref entryPoint, value);
                ResetFileWatcher();
            }
        }

        private string projectDir;
        public string ProjectDir
        {
            get { return projectDir; }
            set
            {
                SetPropertyAndNotify(ref projectDir, value);

                RootNamespace = ProjectUtil.GetRootNamespace(ProjectDir);
                ResetFileWatcher();

                if (RootNamespace != "")
                {
                    var slnPath = ProjectUtil.GetSlnPath(projectDir);

                    if (slnPath != null)
                    {
                        m_visualSln = new VisualSln(slnPath);
                    }
                }
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
            get {
                return new ProjectInfo()
                {
                    ProjectDir = projectDir,
                    EntryPoint = entryPoint,
                    RootNamespace = rootNamespace,
                    Breakpoints = m_visualSln != null ? m_visualSln.GetBreakpoints() : new List<Breakpoint>(),
                };
            }
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
            get {
                return new ScriptInfo()
                {
                    EntryPoint = EntryPoint,
                    ProjectDirectory = ProjectDir,
                    Author = author,
                    Description = description,
                    MapModes = mapModes,
                };
            }
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

        public string EntryPointTooltip
        {
            get { return EntryPoint + "\nDouble click to open"; }
        }

        public string ProjectDirTooltip
        {
            get { return ProjectDir + "\nDouble click to open in explorer"; }
        }

        public MainViewModel()
        {
            m_settingsAccess = new SettingsAccess();
            m_scriptAccess = new ScriptAccess();

            m_keyboardHook = new GlobalKeyboardHook();
            m_keyboardHook.HookedKeys.Add(System.Windows.Forms.Keys.F6);
            m_keyboardHook.KeyUp += (sender, e) => Compile(null);

            m_linker = new Linker();
            m_scheduledTask = new ScheduledTask();

            BrowseEntryPointCommand = new DelegateCommand(BrowseEntryPoint);
            OpenEntryPointCommand = new DelegateCommand(OpenEntryPoint);
            BrowseProjectDirCommand = new DelegateCommand(BrowseProjectDir);
            OpenProjectDirCommand = new DelegateCommand(OpenProjectDir);
            CopyToClipboardCommand = new DelegateCommand(CopyToClipboard);
            CompileCommand = new DelegateCommand(Compile);
            ExpandLinkedFilesWindowCommand = new DelegateCommand(ExpandLinkedFilesWindow);
            OpenFileCommand = new DelegateCommand(OpenFile);

            LoadData();
        }

        private void ResetFileWatcher()
        {
            if (!Directory.Exists(ProjectDir) || !File.Exists(EntryPoint)) return;

            m_fileWatcher = new FileSystemWatcher
            {
                Path = ProjectDir,
                NotifyFilter = NotifyFilters.LastWrite |
                         NotifyFilters.FileName |
                         NotifyFilters.DirectoryName,
                Filter = "*.cs",
                IncludeSubdirectories = true,
            };

            m_fileWatcher.Created += (sender, e) =>
            {
                if (!File.Exists(EntryPoint) && Path.GetFileName(EntryPoint) == Path.GetFileName(e.Name))
                {
                    EntryPoint = e.FullPath; // A naive solution to detect entry point file moved
                }
            };
            m_fileWatcher.Renamed += (sender, e) =>
            {
                EntryPoint = e.FullPath;
            };

            // Begin watching.
            m_fileWatcher.EnableRaisingEvents = true;
        }

        private void Compile(object param)
        {
            var sfdProcess = Process.GetProcessesByName("Superfighters Deluxe").FirstOrDefault();

            if (sfdProcess != null)
            {
                CopyToClipboard(null);

                // Switch to the Script Editor window
                WinUtil.BringWindowToFront("Script Editor");

                // Wait until the window is switching back
                while (WinUtil.GetActiveWindowTitle() == null)
                {
                }

                if (WinUtil.GetActiveWindowTitle() == "Script Editor")
                {
                    // Tab to focus in the editor's text area if not already
                    WinUtil.Simulate(sfdProcess, "{TAB}");
                    // CTRL-A Select all text in editor
                    WinUtil.Simulate(sfdProcess, "^(a)");
                    // CTRL-V Paste clipboard content
                    WinUtil.Simulate(sfdProcess, "^(v)");
                    // Compile newly pasted code
                    WinUtil.Simulate(sfdProcess, "{F5}");
                }
            }
        }

        private void LoadData()
        {
            var settings = m_settingsAccess.LoadSettings();

            EntryPoint = settings.EntryPoint;
            ProjectDir = settings.ProjectDirectory;
            IsStandaloneScript = settings.StandaloneScript;
            IsLinkedFileWindowExpanded = settings.IsLinkedFileWindowExpanded;

            var scriptInfo = m_scriptAccess.LoadScriptInfo(ProjectDir, EntryPoint);

            Author = scriptInfo.Author;
            Description = scriptInfo.Description;
            MapModes = scriptInfo.MapModes;
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

        private void OpenEntryPoint(object param)
        {
            if (File.Exists(EntryPoint))
            {
                Process.Start(EntryPoint);
            }
            else
            {
                MessageBox.Show($"File not found: {EntryPoint}", "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
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

        private void OpenProjectDir(object param)
        {
            if (Directory.Exists(ProjectDir))
            {
                Process.Start(ProjectDir);
            }
            else
            {
                MessageBox.Show($"Directory not found: {ProjectDir}", "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
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
            var message = $"Successfully linked {result.LinkedFiles.Count} file(s)! ({stopwatch.ElapsedMilliseconds} ms)";
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
            m_settingsAccess.SaveSettings(new Settings()
            {
                EntryPoint = EntryPoint,
                ProjectDirectory = ProjectDir,
                StandaloneScript = IsStandaloneScript,
                IsLinkedFileWindowExpanded = IsLinkedFileWindowExpanded,
            });
            m_scriptAccess.UpdateScriptInfo(ScriptInfo);
        }
    }
}
