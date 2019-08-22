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
using System.Diagnostics;
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
        private Settings m_settings;

        public ICommand OpenCreateNewScriptCommand { get; private set; }
        public ICommand SaveScriptInfoCommand { get; private set; }
        public ICommand DeleteScriptInfoCommand { get; private set; }
        public ICommand AddTemplateToEntryPointCommand { get; private set; }
        public ICommand OpenOptionWindowCommand { get; private set; }
        public ICommand CopyToClipboardCommand { get; private set; }
        public ICommand CompileCommand { get; private set; }
        public ICommand CompileAndRunCommand { get; private set; }
        public ICommand ViewReadMeCommand { get; private set; }
        public ICommand OpenAboutWindowCommand { get; private set; }
        public ICommand ExpandLinkedFilesWindowCommand { get; private set; }
        public ICommand OpenFileCommand { get; private set; }
        public ICommand PendingGlobalCommand { get; private set; } = null;

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
            get { return scriptInfo; }
            set
            {
                SetPropertyAndNotify(ref scriptInfo, value);
                ProjectInfo = m_scriptService.GetProjectInfo(scriptInfo);
            }
        }

        private bool generateExtensionScript;
        public bool GenerateExtensionScript
        {
            get { return generateExtensionScript; }
            set { SetPropertyAndNotify(ref generateExtensionScript, value); }
        }

        private HotKey copyToClipboardHotkey;
        public HotKey CopyToClipboardHotkey
        {
            get { return copyToClipboardHotkey; }
            set
            {
                copyToClipboardHotkey = value;
                CopyToClipboardHotkeyName = copyToClipboardHotkey.ToString() + " (Global)";
            }
        }

        private string copyToClipboardHotkeyName;
        public string CopyToClipboardHotkeyName
        {
            get { return copyToClipboardHotkeyName; }
            set { SetPropertyAndNotify(ref copyToClipboardHotkeyName, value); }
        }

        private HotKey compileHotkey;
        public HotKey CompileHotkey
        {
            get { return compileHotkey; }
            set
            {
                compileHotkey = value;
                CompileHotkeyName = compileHotkey.ToString() + " (Global)";
            }
        }

        private string compileHotkeyName;
        public string CompileHotkeyName
        {
            get { return compileHotkeyName; }
            set { SetPropertyAndNotify(ref compileHotkeyName, value); }
        }

        private HotKey compileAndRunHotkey;
        public HotKey CompileAndRunHotkey
        {
            get { return compileAndRunHotkey; }
            set
            {
                compileAndRunHotkey = value;
                CompileAndRunHotkeyName = compileAndRunHotkey.ToString() + " (Global)";
            }
        }

        private string compileAndRunHotkeyName;
        public string CompileAndRunHotkeyName
        {
            get { return compileAndRunHotkeyName; }
            set { SetPropertyAndNotify(ref compileAndRunHotkeyName, value); }
        }

        private string resultInfo;
        public string ResultInfo
        {
            get { return resultInfo; }
            set { SetPropertyAndNotify(ref resultInfo, value); }
        }

        private string resultInfoColor;
        public string ResultInfoColor
        {
            get { return resultInfoColor; }
            set { SetPropertyAndNotify(ref resultInfoColor, value); }
        }

        private bool isLinkedFileWindowExpanded = false;
        public bool IsLinkedFileWindowExpanded
        {
            get { return isLinkedFileWindowExpanded; }
            set
            {
                SetPropertyAndNotify(ref isLinkedFileWindowExpanded, value);
                if (IsLinkedFileWindowExpanded)
                    ExpandIcon = "▲";
                else
                    ExpandIcon = "▼";
            }
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

        private Action openOptionWindow;
        public Action OpenOptionWindow
        {
            get { return openOptionWindow; }
            set
            {
                openOptionWindow = value;
                OpenOptionWindowCommand = new DelegateCommand(openOptionWindow);
            }
        }

        private Action openAboutWindow;
        public Action OpenAboutWindow
        {
            get { return openAboutWindow; }
            set
            {
                openAboutWindow = value;
                OpenAboutWindowCommand = new DelegateCommand(openAboutWindow);
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
            m_eventAggregator.GetEvent<SettingsChangedEvent>().Subscribe((settings) => LoadSettings(settings));
            
            // Disable global hotkeys while user changing hotkey
            m_eventAggregator.GetEvent<SettingsWindowOpenEvent>().Subscribe(
                () => m_winService.GlobalKeyDown -= OnGlobalHotkeyDown);
            m_eventAggregator.GetEvent<SettingsWindowClosedEvent>().Subscribe(
                () => m_winService.GlobalKeyDown += OnGlobalHotkeyDown);

            m_settingsAccess = new SettingsAccess();
            m_scriptAccess = new ScriptAccess();

            m_winService = new WinService();
            m_winService.GlobalKeyDown += OnGlobalHotkeyDown;
            m_winService.GlobalKeyUp += OnGlobalHotkeyUp;
            m_winService.ForegroundChanged += RemoveFileModificationDetectedDialog;

            m_scriptService = new ScriptService();
            m_scheduledTask = new ScheduledTask();
            m_settings = m_settingsAccess.LoadSettings();

            LoadCommands();
            LoadSettings(m_settings);
            ScriptNames = m_scriptAccess.GetScriptNames();
        }
        private void LoadCommands()
        {
            DeleteScriptInfoCommand = new DelegateCommand(DeleteScriptInfo);
            AddTemplateToEntryPointCommand = new DelegateCommand(AddTemplateToEntryPoint);
            CopyToClipboardCommand = new DelegateCommand(CopyToClipboard);
            CompileCommand = new DelegateCommand(() => Compile(false));
            CompileAndRunCommand = new DelegateCommand(() => Compile(true));
            ViewReadMeCommand = new DelegateCommand(ViewReadMe);
            ExpandLinkedFilesWindowCommand = new DelegateCommand(ExpandLinkedFilesWindow);
            OpenFileCommand = new DelegateCommand<string>(FileUtil.OpenFile);
        }

        private void LoadSettings(Settings settings)
        {
            GenerateExtensionScript = settings.GenerateExtensionScript;
            IsLinkedFileWindowExpanded = settings.IsLinkedFileWindowExpanded;
            ScriptName = settings.LastOpenedScript;

            CopyToClipboardHotkey = InputUtil.Parse(settings.CopyToClipboardHotkey);
            CompileHotkey = InputUtil.Parse(settings.CompileHotkey);
            CompileAndRunHotkey = InputUtil.Parse(settings.CompileAndRunHotkey);

            m_winService.ClearGlobalHookedKey();
            m_winService.AddGlobalHookedKey(
                Key.LeftShift,
                Key.RightShift,
                Key.LeftCtrl,
                Key.RightCtrl,
                Key.LeftAlt,
                Key.RightAlt,
                CopyToClipboardHotkey.Key,
                CompileHotkey.Key,
                CompileAndRunHotkey.Key
            );

            m_settings = settings;
        }

        private void OnGlobalHotkeyDown(object sender, GlobalKeyEventArgs e)
        {
            var hotKey = e.HotKey;

            if (hotKey.Equals(CopyToClipboardHotkey))
            {
                PendingGlobalCommand = CopyToClipboardCommand;
            }
            else if (hotKey.Equals(CompileHotkey))
            {
                PendingGlobalCommand = CompileCommand;
            }
            else if (hotKey.Equals(CompileAndRunHotkey))
            {
                PendingGlobalCommand = CompileAndRunCommand;
            }
        }

        private void OnGlobalHotkeyUp(object sender, GlobalKeyEventArgs e)
        {
            if (PendingGlobalCommand == null) return;

            var hotKey = e.HotKey;

            // Execute the pending command only if no modifier key is pressed or it will mess up the keystrokes
            // sent from the command
            if (InputUtil.IsSame(hotKey.Key, hotKey.Modifiers) || !InputUtil.IsModifierKey(hotKey.Key))
            {
                // If hotKey.Key is a modifier key, that modifier key state is still down at the time this event
                // is fired, which may interfere with the keystrokes sent from the automation command below, so we
                // will have to wait a bit more
                TimerUtil.RunOnce(() =>
                {
                    // The automation command will probably send some keystrokes (e.g. Ctrl-V to paste text or something)
                    // We dont want the global hook receives and processes those events, thus run this command again
                    m_winService.GlobalKeyUp -= OnGlobalHotkeyUp;
                    PendingGlobalCommand.Execute(null);
                    PendingGlobalCommand = null;
                    m_winService.GlobalKeyUp += OnGlobalHotkeyUp;
                }, 1);
            }
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
            if (!ScriptInfo.IsEmpty())
            {
                var entryPointFile = Path.GetFileName(ScriptInfo.EntryPoint);

                m_scriptService.AddTemplate(ProjectInfo, ScriptInfo.EntryPoint);
                ShowInlineMessage($"Init template to {entryPointFile}", 1500);
            }
            else
            {
                ShowInlineMessage("Script info is empty", 1500, TextColor.Failed);
            }
        }

        private void Compile(bool runAfterCompiling)
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

                    if (runAfterCompiling)
                        RunMapEditorTestIfSuccess();
                }
            }
        }

        private void RunMapEditorTestIfSuccess()
        {
            m_winService.ForegroundChanged += OnCompileResultDialogOpened;
        }

        private void OnCompileResultDialogOpened(object sender, WinEventArgs args)
        {
            var windowTitle = WinUtil.GetWindowTitle(args.HWnd);

            if (windowTitle == "Success" || windowTitle == "Error")
            {
                // Enter to close the result dialog after compiling
                WinUtil.SimulateKey("{ENTER}");

                if (windowTitle == "Success")
                {
                    WinUtil.BringWindowToFront("Superfighters Deluxe Map Editor");
                    WinUtil.SimulateKey("{F5}"); // Run map editor test
                }

                m_winService.ForegroundChanged -= OnCompileResultDialogOpened;
            }
        }

        private readonly WinEventHandler RemoveFileModificationDetectedDialog = (sender, args) =>
        {
            if (WinUtil.GetWindowTitle(args.HWnd) == "Microsoft Visual Studio")
            {
                // Focus and enter to accept external changes to Visual Studio files
                WinUtil.BringWindowToFront("Microsoft Visual Studio");
                WinUtil.SimulateKey("{ENTER}");
            }
        };

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

        private void ShowInlineMessage(string message, int timeoutMs, string textColor = TextColor.Success)
        {
            ResultInfoColor = textColor;
            ResultInfo = message;
            TimerUtil.RunOnce(() => ResultInfo = "", timeoutMs);
        }

        private async void GenerateOutputFile(string sourceCode)
        {
            try
            {
                var outputPath = Path.ChangeExtension(ScriptInfo.EntryPoint, "txt");
                await FileUtil.WriteTextAsync(outputPath, sourceCode);

                if (GenerateExtensionScript)
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
            var scriptName = Path.GetFileName(sourcePath);
            var destinationPath = Path.Combine(sfdScriptPath, scriptName);

            await FileUtil.CopyFileAsync(sourcePath, destinationPath);
        }

        private void ViewReadMe()
        {
            Process.Start((string)Application.Current.Properties[Constants.SourceCodeUrl] + "/blob/master/README.md");
        }

        private void ExpandLinkedFilesWindow()
        {
            IsLinkedFileWindowExpanded = !IsLinkedFileWindowExpanded;
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
            m_settings.GenerateExtensionScript = GenerateExtensionScript;
            m_settings.IsLinkedFileWindowExpanded = IsLinkedFileWindowExpanded;
            m_settings.LastOpenedScript = ScriptName;

            m_settingsAccess.SaveSettings(m_settings);
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
