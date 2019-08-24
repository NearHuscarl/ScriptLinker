using Prism.Events;
using ScriptLinker.Access;
using ScriptLinker.Events;
using ScriptLinker.Models;
using ScriptLinker.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace ScriptLinker.ViewModels
{
    class OptionViewModel : ViewModelBase
    {
        protected readonly IEventAggregator _eventAggregator;
        private SettingsAccess _settingsAccess;
        private Settings _settings;
        private Dictionary<string, int> _hotKeys = new Dictionary<string, int>();

        public ICommand SaveSettingsCommand { get; private set; }

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

        private void ChangeHotKey(string oldHotKey, string newHotKey)
        {
            if (!string.IsNullOrEmpty(oldHotKey))
            {
                if (_hotKeys.ContainsKey(oldHotKey))
                    _hotKeys[oldHotKey]--;
                else
                    _hotKeys.Add(oldHotKey, 1);
            }
            if (_hotKeys.ContainsKey(newHotKey))
                _hotKeys[newHotKey]++;
            else
                _hotKeys.Add(newHotKey, 1);
        }

        private bool Validate()
        {
            ResetErrorMessages();

            if (_hotKeys[CopyToClipboardHotkey] > 1)
            {
                CopyToClipboardHotkeyError = "Duplicated hotkey";
                return false;
            }
            if (_hotKeys[CompileHotkey] > 1)
            {
                CompileHotkeyError = "Duplicated hotkey";
                return false;
            }
            if (_hotKeys[CompileAndRunHotkey] > 1)
            {
                CompileAndRunHotkeyError = "Duplicated hotkey";
                return false;
            }

            return true;
        }

        private void ResetErrorMessages()
        {
            CopyToClipboardHotkeyError = null;
            CompileHotkeyError = null;
            CompileAndRunHotkeyError = null;
        }

        private void SaveSettings()
        {
            if (!Validate()) return;

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
