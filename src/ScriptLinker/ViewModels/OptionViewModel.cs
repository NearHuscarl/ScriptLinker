using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Prism.Events;
using ScriptLinker.Access;
using ScriptLinker.Events;
using ScriptLinker.Models;
using ScriptLinker.Utilities;
using ScriptLinker.Infrastructure.Utilities;

namespace ScriptLinker.ViewModels
{
    class OptionViewModel : ViewModelBase
    {
        protected readonly IEventAggregator _eventAggregator;
        private SettingsAccess _settingsAccess;
        private Settings _settings;
        private Dictionary<string, int> _hotkeys = new Dictionary<string, int>();

        public ICommand SaveSettingsCommand { get; private set; }
        public ICommand CloseCommand { get; private set; }

        private string copyToClipboardHotkey;
        public string CopyToClipboardHotkey
        {
            get { return copyToClipboardHotkey; }
            set
            {
                if (string.IsNullOrEmpty(value)) return;
                ChangeHotKey(copyToClipboardHotkey, value);
                copyToClipboardHotkey = value;
            }
        }

        private string compileHotkey;
        public string CompileHotkey
        {
            get { return compileHotkey; }
            set
            {
                if (string.IsNullOrEmpty(value)) return;
                ChangeHotKey(compileHotkey, value);
                compileHotkey = value;
            }
        }

        private string compileAndRunHotkey;
        public string CompileAndRunHotkey
        {
            get { return compileAndRunHotkey; }
            set
            {
                if (string.IsNullOrEmpty(value)) return;
                ChangeHotKey(compileAndRunHotkey, value);
                compileAndRunHotkey = value;
            }
        }

        public bool GenerateExtensionScript { get; set; }

        public string CopyToClipboardHotkeyError { get; set; }
        public string CompileHotkeyError { get; set; }
        public string CompileAndRunHotkeyError { get; set; }

        public OptionViewModel(IEventAggregator eventAggregator, Action closeAction)
        {
            _eventAggregator = eventAggregator;
            _eventAggregator.GetEvent<SettingsWindowOpenEvent>().Publish();
            _settingsAccess = new SettingsAccess();

            _settings = _settingsAccess.LoadSettings();

            CopyToClipboardHotkey = _settings.CopyToClipboardHotkey;
            CompileHotkey = _settings.CompileHotkey;
            CompileAndRunHotkey = _settings.CompileAndRunHotkey;
            GenerateExtensionScript = _settings.GenerateExtensionScript;

            Close = closeAction;

            SaveSettingsCommand = new DelegateCommand(SaveSettings);
            CloseCommand = new DelegateCommand(() => Close());
        }

        private string ReadKeyEvent(KeyEventArgs e)
        {
            // Fetch the actual shortcut key
            var key = (e.Key == Key.System ? e.SystemKey : e.Key);

            if (InputUtil.IsModifierKey(key)) return "";

            var sb = new StringBuilder();

            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                sb.Append("Ctrl+");
            }
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                sb.Append("Shift+");
            }
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Alt))
            {
                sb.Append("Alt+");
            }

            return sb
                .Append(key.ToString())
                .ToString();
        }

        public void OnChangeCopyToClipboardHotkey(object sender, KeyEventArgs e)
        {
            // The text box should grab all input.
            e.Handled = true;
            CopyToClipboardHotkey = ReadKeyEvent(e);
        }

        public void OnChangeCompileHotkey(object sender, KeyEventArgs e)
        {
            e.Handled = true;
            CompileHotkey = ReadKeyEvent(e);
        }

        public void OnChangeCompileAndRunHotkey(object sender, KeyEventArgs e)
        {
            e.Handled = true;
            CompileAndRunHotkey = ReadKeyEvent(e);
        }

        protected override void OnPropsChanged(string propertyName, object before, object after)
        {
            if (before == null) return; // dont validate when initializing the first time

            if (!(after is string))
                return;

            switch (propertyName)
            {
                case nameof(CopyToClipboardHotkey):
                case nameof(CompileHotkey):
                case nameof(CompileAndRunHotkey):
                    Validate();
                    break;
            }
        }

        private void ChangeHotKey(string oldHotKey, string newHotKey)
        {
            if (!string.IsNullOrEmpty(oldHotKey))
            {
                if (_hotkeys.ContainsKey(oldHotKey))
                    _hotkeys[oldHotKey]--;
                else
                    _hotkeys.Add(oldHotKey, 1);
            }
            if (_hotkeys.ContainsKey(newHotKey))
                _hotkeys[newHotKey]++;
            else
                _hotkeys.Add(newHotKey, 1);
        }

        private bool Validate(bool submit = false)
        {
            ResetErrorMessages();

            if (_hotkeys[CopyToClipboardHotkey] > 1)
            {
                AddError(nameof(CopyToClipboardHotkey), "Duplicated hotkey");
                if (submit)
                    CopyToClipboardHotkeyError = "Duplicated hotkey";
                return false;
            }
            if (_hotkeys[CompileHotkey] > 1)
            {
                AddError(nameof(CompileHotkey), "Duplicated hotkey");
                if (submit)
                    CompileHotkeyError = "Duplicated hotkey";
                return false;
            }
            if (_hotkeys[CompileAndRunHotkey] > 1)
            {
                AddError(nameof(CompileAndRunHotkey), "Duplicated hotkey");
                if (submit)
                    CompileAndRunHotkeyError = "Duplicated hotkey";
                return false;
            }

            return true;
        }

        private void ResetErrorMessages()
        {
            ClearErrors();

            CopyToClipboardHotkeyError = null;
            CompileHotkeyError = null;
            CompileAndRunHotkeyError = null;
        }

        private void SaveSettings()
        {
            if (!Validate(submit: true)) return;

            _settings.CopyToClipboardHotkey = CopyToClipboardHotkey;
            _settings.CompileHotkey = CompileHotkey;
            _settings.CompileAndRunHotkey = CompileAndRunHotkey;
            _settings.GenerateExtensionScript = GenerateExtensionScript;

            _settingsAccess.SaveSettings(_settings);
            _eventAggregator.GetEvent<SettingsChangedEvent>().Publish(_settings);
            _eventAggregator.GetEvent<SettingsWindowClosedEvent>().Publish();

            Close();
        }
    }
}
