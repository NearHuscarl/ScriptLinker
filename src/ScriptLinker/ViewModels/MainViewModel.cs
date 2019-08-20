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
using System.Collections.Generic;
using ScriptLinker.DataLogic;
using System.Diagnostics;
using System.Timers;
using ScriptLinker.Access;
using Prism.Events;
using ScriptLinker.Events;
using ScriptLinker.Services;

namespace ScriptLinker.ViewModels
{
    class MainViewModel : ViewModelBase
    {
        protected readonly IEventAggregator m_eventAggregator;
        private ScriptService m_scriptService;
        private ScheduledTask m_scheduledTask;
        private WinService m_winService;
        private SettingsAccess m_settingsAccess;
        private ScriptAccess m_scriptAccess;

        public ICommand OpenCreateNewScriptCommand { get; private set; }
        public ICommand SaveScriptInfoCommand { get; private set; }
        public ICommand DeleteScriptInfoCommand { get; private set; }
        public ICommand AddTemplateToEntryPointCommand { get; private set; }
        public ICommand CopyToClipboardCommand { get; private set; }
        public ICommand CompileCommand { get; private set; }
        public ICommand ExpandLinkedFilesWindowCommand { get; private set; }
        public ICommand OpenFileCommand { get; private set; }

        private List<string> scriptNames;
        public List<string> ScriptNames
        {
            get { return scriptNames; }
            set { SetPropertyAndNotify(ref scriptNames, value); }
        }

        private string scriptName = "";
        public string ScriptName
        {
            get { return scriptName; }
            set
            {
                SetPropertyAndNotify(ref scriptName, value);
                m_eventAggregator.GetEvent<ScriptInfoSelectedEvent>().Publish(scriptName);
            }
        }

        public ProjectInfo ProjectInfo { get; private set; }

        private ScriptInfo scriptInfo = new ScriptInfo();
        public ScriptInfo ScriptInfo
        {
            set
            {
                SetPropertyAndNotify(ref scriptInfo, value);
                ProjectInfo = m_scriptService.GetProjectInfo(scriptInfo);
            }
            get { return scriptInfo; }
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

        private Action openNewScriptWindow;
        public Action OpenNewScriptWindow
        {
            get { return openNewScriptWindow; }
            set
            {
                openNewScriptWindow = value;
                OpenCreateNewScriptCommand = new DelegateCommand(openNewScriptWindow);
            }
        }

        private Action save;
        public Action Save
        {
            get { return save; }
            set
            {
                save = value;
                SaveScriptInfoCommand = new DelegateCommand(save);
            }
        }

        public MainViewModel(IEventAggregator eventAggregator)
        {
            m_eventAggregator = eventAggregator;
            m_eventAggregator.GetEvent<ScriptInfoAddedEvent>().Subscribe(OnScriptInfoAdded);
            m_eventAggregator.GetEvent<ScriptInfoChangedEvent>().Subscribe(OnScriptInfoChanged);

            m_settingsAccess = new SettingsAccess();
            m_scriptAccess = new ScriptAccess();

            m_winService = new WinService();
            m_winService.AddGlobalHookedKey(System.Windows.Forms.Keys.F6);
            m_winService.GlobalKeyUp += (sender, e) => Compile();
            m_winService.InitKillFileModificationDetectedDialog();

            m_scriptService = new ScriptService();
            m_scheduledTask = new ScheduledTask();

            LoadCommands();
            LoadData();
        }

        private void LoadCommands()
        {
            DeleteScriptInfoCommand = new DelegateCommand(DeleteScriptInfo);
            AddTemplateToEntryPointCommand = new DelegateCommand(AddTemplateToEntryPoint);
            CopyToClipboardCommand = new DelegateCommand(CopyToClipboard);
            CompileCommand = new DelegateCommand(Compile);
            ExpandLinkedFilesWindowCommand = new DelegateCommand(ExpandLinkedFilesWindow);
            OpenFileCommand = new DelegateCommand<string>(FileUtil.OpenFile);
        }

        private void LoadData()
        {
            var settings = m_settingsAccess.LoadSettings();

            IsStandaloneScript = settings.StandaloneScript;
            IsLinkedFileWindowExpanded = settings.IsLinkedFileWindowExpanded;
            ScriptName = settings.LastOpenedScript;

            ScriptNames = m_scriptAccess.GetScriptNames();
        }

        public Action<ScriptInfo> SaveScriptInfo
        {
            get
            {
                return (scriptInfo) =>
                {
                    if (!ScriptInfo.Equals(scriptInfo))
                    {
                        m_scriptAccess.UpdateScriptInfo(scriptInfo);
                        ScriptInfo = scriptInfo;
                        ShowInlineMessage("Save successfully", 1200);
                    }
                };
            }
        }

        private void OnScriptInfoAdded(ScriptInfo scriptInfo)
        {
            ScriptNames = m_scriptAccess.GetScriptNames();
            ScriptName = scriptInfo.Name;
        }

        private void OnScriptInfoChanged(ScriptInfo scriptInfo)
        {
            ScriptInfo = scriptInfo;
        }

        private void DeleteScriptInfo()
        {
            var result = MessageBox.Show($"Are you sure you want to delete {ScriptName} script", "Warning",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                m_scriptAccess.RemoveScriptInfo(ScriptName);
                ScriptNames = m_scriptAccess.GetScriptNames();
                ScriptName = ScriptNames.FirstOrDefault();
            }
        }

        private void AddTemplateToEntryPoint()
        {
            var entryPointFile = Path.GetFileName(ScriptInfo.EntryPoint);

            m_scriptService.AddTemplate(ProjectInfo, ScriptInfo.EntryPoint);
            ShowInlineMessage($"Init template to {entryPointFile}", 1500);
        }

        private void Compile()
        {
            var sfdProcess = Process.GetProcessesByName("Superfighters Deluxe").FirstOrDefault();

            if (sfdProcess != null)
            {
                CopyToClipboard();

                // Switch to the Script Editor window
                WinUtil.BringWindowToFront("Script Editor");

                // Wait until the window is switching back
                while (WinUtil.GetActiveWindowTitle() == null)
                {
                }

                if (WinUtil.GetActiveWindowTitle() == "Script Editor")
                {
                    // Tab to focus in the editor's text area if not already
                    WinUtil.SimulateKey("{TAB}");
                    // CTRL-A Select all text in editor
                    WinUtil.SimulateKey("^(a)");
                    // CTRL-V Paste clipboard content
                    WinUtil.SimulateKey("^(v)");
                    // Compile newly pasted code
                    WinUtil.SimulateKey("{F5}");
                }
            }
        }

        private void CopyToClipboard()
        {
            var stopwatch = new Stopwatch();
            stopwatch.StartPrinting();

            //var sourceCode = await Task.Run(() => m_linker.Link(ProjectInfo, ScriptInfo));
            var result = m_scriptService.Link(ProjectInfo, ScriptInfo);
            var sourceCode = result.Content;
            LinkedFiles = result.LinkedFiles;
            stopwatch.PrintTime("CopyToClipboard() Link");

            Clipboard.SetText(sourceCode);
            stopwatch.PrintTime("CopyToClipboard() ToClipboard");
            GenerateOutputFile(sourceCode);
            var message = $"Successfully linked {result.LinkedFiles.Count} file(s)! ({stopwatch.ElapsedMilliseconds} ms)";
            ShowInlineMessage(message, 5000);
            stopwatch.PrintTime("CopyToClipboard() Gen output file");
        }

        private Timer m_successMessageTimer = new Timer();
        private void ShowInlineMessage(string message, int timeoutMs)
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
            try
            {
                var outputPath = Path.ChangeExtension(ScriptInfo.EntryPoint, "txt");
                await FileUtil.WriteTextAsync(outputPath, sourceCode);

                if (IsStandaloneScript)
                {
                    CopyToScriptFolder(sourceCode);
                }
            }
            catch (IOException)
            {
                MessageBox.Show($"Don't spam the button bruh", "Info",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        private async void CopyToScriptFolder(string sourceCode)
        {
            var entryPoint = ScriptInfo.EntryPoint;
            var sourcePath = Path.ChangeExtension(entryPoint, "txt");
            var myDocument = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var sfdScriptPath = Path.Combine(myDocument, @"Superfighters Deluxe\Scripts");
            var scriptName = Path.GetFileNameWithoutExtension(entryPoint) + ".txt";
            var destinationPath = Path.Combine(sfdScriptPath, scriptName);

            await FileUtil.CopyFileAsync(sourcePath, destinationPath);
        }

        private void ExpandLinkedFilesWindow()
        {
            IsLinkedFileWindowExpanded = !IsLinkedFileWindowExpanded;

            if (IsLinkedFileWindowExpanded)
                ExpandIcon = "▲";
            else
                ExpandIcon = "▼";
        }

        private void CheckRemoveBackupFiles()
        {
            var entryPoint = ScriptInfo.EntryPoint;
            var backupFolder = new DirectoryInfo(m_scriptService.GetBackupFolder());

            foreach (var backupFile in backupFolder.GetFiles("~*"))
            {
                var creationTime = backupFile.CreationTimeUtc;
                var timeNow = DateTime.UtcNow;

                if (timeNow.Subtract(creationTime).Hours >= 72)
                {
                    backupFile.Delete();
                }
            }
        }

        public override void OnWindowClosing(object sender, CancelEventArgs e)
        {
            m_settingsAccess.SaveSettings(new Settings()
            {
                LastOpenedScript = ScriptName,
                StandaloneScript = IsStandaloneScript,
                IsLinkedFileWindowExpanded = IsLinkedFileWindowExpanded,
            });
            m_scriptAccess.RemoveNotFoundScriptInfo();
            m_scriptAccess.UpdateScriptInfo(ScriptInfo);
            CheckRemoveBackupFiles();
        }

        public override void OnWindowClosed(object sender, EventArgs e)
        {
            m_winService.Dispose();
        }
    }
}
