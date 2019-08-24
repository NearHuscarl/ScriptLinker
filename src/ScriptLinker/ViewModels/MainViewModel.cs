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
using System.Timers;
using PropertyChanged;

namespace ScriptLinker.ViewModels
{
    class MainViewModel : ViewModelBase
    {
        protected readonly IEventAggregator _eventAggregator;
        private ScriptService _scriptService;
        private AutomationService _automationService;
        private DialogService _dialogService;
        private WinService _winService;
        private SettingsAccess _settingsAccess;
        private ScriptAccess _scriptAccess;
        private Settings _settings;

        public ICommand OpenCreateNewScriptCommand { get; private set; }
        public ICommand SaveScriptInfoCommand { get; private set; }
        public ICommand DeleteScriptInfoCommand { get; private set; }
        public ICommand AddTemplateToEntryPointCommand { get; private set; }
        public ICommand OpenOptionWindowCommand { get; private set; }
        public ICommand OpenScriptFolderCommand { get; private set; }
        public ICommand CopyToClipboardCommand { get; private set; }
        public ICommand CompileCommand { get; private set; }
        public ICommand CompileAndRunCommand { get; private set; }
        public ICommand ViewReadMeCommand { get; private set; }
        public ICommand OpenAboutWindowCommand { get; private set; }
        public ICommand ExpandLinkedFilesWindowCommand { get; private set; }
        public ICommand OpenFileCommand { get; private set; }
        public ICommand PendingGlobalCommand { get; private set; } = null;

        public List<string> ScriptNames { get; set; }

        public string ScriptName { get; set; } = "";
        private void OnScriptNameChanged()
        {
            _eventAggregator.GetEvent<ScriptInfoSelectedEvent>().Publish(ScriptName);
        }

        [DoNotNotify]
        public ProjectInfo ProjectInfo { get; private set; }

        public ScriptInfo ScriptInfo { get; set; } = new ScriptInfo();
        private void OnScriptInfoChanged()
        {
            ProjectInfo = _scriptService.GetProjectInfo(ScriptInfo);
        }

        public bool GenerateExtensionScript { get; set; }

        public HotKey CopyToClipboardHotkey { get; set; }
        public string CopyToClipboardHotkeyName => CopyToClipboardHotkey.ToString() + " (Global)";

        public HotKey CompileHotkey { get; set; }
        public string CompileHotkeyName => CompileHotkey.ToString() + " (Global)";

        public HotKey CompileAndRunHotkey { get; set; }
        public string CompileAndRunHotkeyName => CompileAndRunHotkey.ToString() + " (Global)";

        public string ResultInfo { get; set; }
        public string ResultInfoColor { get; set; }

        public bool IsLinkedFileWindowExpanded { get; set; }
        public string ExpandIcon => IsLinkedFileWindowExpanded ? "▲" : "▼";

        public HashSet<string> LinkedFiles { get; set; }

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
            _eventAggregator = eventAggregator;
            _eventAggregator.GetEvent<ScriptInfoAddedEvent>().Subscribe(OnScriptInfoAdded);
            _eventAggregator.GetEvent<ScriptInfoChangedEvent>().Subscribe((scriptInfo) => ScriptInfo = scriptInfo);
            _eventAggregator.GetEvent<SettingsChangedEvent>().Subscribe((settings) => LoadSettings(settings));
            
            // Disable global hotkeys while user is changing hotkey
            _eventAggregator.GetEvent<SettingsWindowOpenEvent>().Subscribe(
                () => _winService.GlobalKeyDown -= OnGlobalHotkeyDown);
            _eventAggregator.GetEvent<SettingsWindowClosedEvent>().Subscribe(
                () => _winService.GlobalKeyDown += OnGlobalHotkeyDown);

            _settingsAccess = new SettingsAccess();
            _scriptAccess = new ScriptAccess();

            _dialogService = new DialogService();
            _scriptService = new ScriptService();

            _winService = new WinService();
            _winService.GlobalKeyDown += OnGlobalHotkeyDown;
            _winService.GlobalKeyUp += OnGlobalHotkeyUp;

            _automationService = new AutomationService();
            _automationService.RemoveFileModificationDetectedDialogOnCreated();

            _settings = _settingsAccess.LoadSettings();

            LoadCommands();
            LoadSettings(_settings);
            ScriptNames = _scriptAccess.GetScriptNames();
        }
        private void LoadCommands()
        {
            DeleteScriptInfoCommand = new DelegateCommand(DeleteScriptInfo);
            AddTemplateToEntryPointCommand = new DelegateCommand(AddTemplateToEntryPoint);
            OpenScriptFolderCommand = new DelegateCommand(() => Process.Start(ApplicationPath.ScriptFolder));
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

            _winService.ClearGlobalHookedKey();
            _winService.AddGlobalHookedKey(
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

            _settings = settings;
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
                TimerUtil.SetTimeOut(() =>
                {
                    // The automation command will probably send some keystrokes (e.g. Ctrl-V to paste text or something)
                    // We dont want the global hook receives and processes those events, thus run this command again
                    _winService.GlobalKeyUp -= OnGlobalHotkeyUp;
                    PendingGlobalCommand.Execute(null);
                    PendingGlobalCommand = null;
                    _winService.GlobalKeyUp += OnGlobalHotkeyUp;
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
                        _scriptAccess.UpdateScriptInfo(scriptInfo);
                        ScriptInfo = scriptInfo;
                        ShowInlineMessage("Save successfully", 1200);
                    }
                };
            }
        }

        private void OnScriptInfoAdded(ScriptInfo scriptInfo)
        {
            ScriptNames = _scriptAccess.GetScriptNames();
            ScriptName = scriptInfo.Name;
        }

        private void DeleteScriptInfo()
        {
            _dialogService.ShowConfirmDialog($"Are you sure you want to delete {ScriptName} script", () =>
            {
                _scriptAccess.RemoveScriptInfo(ScriptName);
                ScriptNames = _scriptAccess.GetScriptNames();
                ScriptName = ScriptNames.FirstOrDefault();
            });
        }

        private void AddTemplateToEntryPoint()
        {
            if (!ScriptInfo.IsEmpty())
            {
                var entryPointFile = Path.GetFileName(ScriptInfo.EntryPoint);

                _scriptService.AddTemplate(ProjectInfo, ScriptInfo.EntryPoint);
                ShowInlineMessage($"Init template to {entryPointFile}", 1500);
            }
            else
            {
                ShowInlineMessage("Script info is empty", 1500, TextColor.Failed);
            }
        }

        private void Compile(bool runAfterCompiling)
        {
            var result = _automationService.Compile(ProjectInfo, ScriptInfo, runAfterCompiling);
            var sourceCode = result.Content;

            if (result.Error == AutomationError.SfdNotOpen)
            {
                _dialogService.ShowWarningDialog("SFD is not opened");
            }
            else if (result.Error == AutomationError.ScriptEditorNotOpen)
            {
                _dialogService.ShowWarningDialog("Script editor tab is not opened");
            }
            else
            {
                ReportResult(result);
                GenerateOutputFile(sourceCode);
            }
        }

        private void CopyToClipboard()
        {
            var result = _automationService.CopyToClipboard(ProjectInfo, ScriptInfo);
            ReportResult(result);
        }

        private void ReportResult(LinkResult result)
        {
            LinkedFiles = result.LinkedFiles;
#if DEBUG
            var message = $"Successfully linked {result.LinkedFiles.Count} file(s)! ({result.Elapsed} ms)";
#else
            var message = $"Successfully linked {result.LinkedFiles.Count} file(s)!)";
#endif
            ShowInlineMessage(message, 5000);
        }

        private Timer _successMessageTimer = new Timer();
        private void ShowInlineMessage(string message, int timeoutMs, string textColor = TextColor.Success)
        {
            ResultInfoColor = textColor;
            ResultInfo = message;

            _successMessageTimer.Stop();
            _successMessageTimer.Elapsed -= (sender, e) => ResultInfo = "";
            _successMessageTimer.Elapsed += (sender, e) => ResultInfo = "";
            _successMessageTimer.AutoReset = false; // run once
            _successMessageTimer.Interval = timeoutMs;
            _successMessageTimer.Start();
        }

        private async void GenerateOutputFile(string sourceCode)
        {
            try
            {
                var outputPath = Path.ChangeExtension(ScriptInfo.EntryPoint, "txt");
                await FileUtil.WriteTextAsync(outputPath, sourceCode);

                if (GenerateExtensionScript)
                {
                    _scriptService.CreateExtensionScript(ScriptInfo.EntryPoint, sourceCode);
                }
            }
            catch (IOException)
            {
                _dialogService.ShowInfoDialog("Don't spam the button bruh");
            }
        }

        private void ViewReadMe()
        {
            Process.Start((string)Application.Current.Properties[Constants.SourceCodeUrl] + "/blob/master/README.md");
        }

        private void ExpandLinkedFilesWindow()
        {
            IsLinkedFileWindowExpanded = !IsLinkedFileWindowExpanded;
        }

        public override void OnWindowClosing(object sender, CancelEventArgs e)
        {
            _settings.GenerateExtensionScript = GenerateExtensionScript;
            _settings.IsLinkedFileWindowExpanded = IsLinkedFileWindowExpanded;
            _settings.LastOpenedScript = ScriptName;

            _settingsAccess.SaveSettings(_settings);
            _scriptAccess.RemoveNotFoundScriptInfo();
            _scriptAccess.UpdateScriptInfo(ScriptInfo);
            _scriptService.CheckRemoveBackupFiles();
        }

        public override void OnWindowClosed(object sender, EventArgs e)
        {
            _automationService.Dispose();
            _winService.Dispose();
        }
    }
}
