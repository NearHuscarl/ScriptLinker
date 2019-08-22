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
        protected readonly IEventAggregator m_eventAggregator;
        private SettingsAccess m_settingsAccess;
        private Settings m_settings;
        private Dictionary<string, int> m_hotKeys = new Dictionary<string, int>();

        public ICommand SaveSettingsCommand { get; private set; }

        private string copyToClipboardHotkey;
        public string CopyToClipboardHotkey
        {
            get { return copyToClipboardHotkey; }
            set
            {
                if (string.IsNullOrEmpty(value)) return;
                ChangeHotKey(copyToClipboardHotkey, value);
                SetPropertyAndNotify(ref copyToClipboardHotkey, value);
            }
        }

        private string copyToClipboardHotkeyError;
        public string CopyToClipboardHotkeyError
        {
            get { return copyToClipboardHotkeyError; }
            set { SetPropertyAndNotify(ref copyToClipboardHotkeyError, value); }
        }

        private string compileHotkey;
        public string CompileHotkey
        {
            get { return compileHotkey; }
            set
            {
                if (string.IsNullOrEmpty(value)) return;
                ChangeHotKey(compileHotkey, value);
                SetPropertyAndNotify(ref compileHotkey, value);
            }
        }

        private string compileHotkeyError;
        public string CompileHotkeyError
        {
            get { return compileHotkeyError; }
            set { SetPropertyAndNotify(ref compileHotkeyError, value); }
        }

        private string compileAndRunHotkey;
        public string CompileAndRunHotkey
        {
            get { return compileAndRunHotkey; }
            set
            {
                if (string.IsNullOrEmpty(value)) return;
                ChangeHotKey(compileAndRunHotkey, value);
                SetPropertyAndNotify(ref compileAndRunHotkey, value);
            }
        }

        private string compileAndRunHotkeyError;
        public string CompileAndRunHotkeyError
        {
            get { return compileAndRunHotkeyError; }
            set { SetPropertyAndNotify(ref compileAndRunHotkeyError, value); }
        }

        private bool generateExtensionScript;
        public bool GenerateExtensionScript
        {
            get { return generateExtensionScript; }
            set { SetPropertyAndNotify(ref generateExtensionScript, value); }
        }

        public OptionViewModel(IEventAggregator eventAggregator, Action closeAction)
        {
            m_eventAggregator = eventAggregator;
            m_settingsAccess = new SettingsAccess();

            m_settings = m_settingsAccess.LoadSettings();

            CopyToClipboardHotkey = m_settings.CopyToClipboardHotkey;
            CompileHotkey = m_settings.CompileHotkey;
            CompileAndRunHotkey = m_settings.CompileAndRunHotkey;
            GenerateExtensionScript = m_settings.GenerateExtensionScript;

            Close = closeAction;

            SaveSettingsCommand = new DelegateCommand(SaveSettings);
        }

        private string ReadKeyEvent(KeyEventArgs e)
        {
            // Fetch the actual shortcut key
            var key = (e.Key == Key.System ? e.SystemKey : e.Key);

            if (InputUtil.IsModifierKey(key)) return "";

            var sb = new StringBuilder();

            if ((Keyboard.Modifiers & ModifierKeys.Control) != 0)
            {
                sb.Append("Ctrl+");
            }
            if ((Keyboard.Modifiers & ModifierKeys.Shift) != 0)
            {
                sb.Append("Shift+");
            }
            if ((Keyboard.Modifiers & ModifierKeys.Alt) != 0)
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
                if (m_hotKeys.ContainsKey(oldHotKey))
                    m_hotKeys[oldHotKey]--;
                else
                    m_hotKeys.Add(oldHotKey, 1);
            }
            if (m_hotKeys.ContainsKey(newHotKey))
                m_hotKeys[newHotKey]++;
            else
                m_hotKeys.Add(newHotKey, 1);
        }

        private bool Validate()
        {
            ResetErrorMessages();

            if (m_hotKeys[CopyToClipboardHotkey] > 1)
            {
                CopyToClipboardHotkeyError = "Duplicated hotkey";
                return false;
            }
            if (m_hotKeys[CompileHotkey] > 1)
            {
                CompileHotkeyError = "Duplicated hotkey";
                return false;
            }
            if (m_hotKeys[CompileAndRunHotkey] > 1)
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

            m_settings.CopyToClipboardHotkey = CopyToClipboardHotkey;
            m_settings.CompileHotkey = CompileHotkey;
            m_settings.CompileAndRunHotkey = CompileAndRunHotkey;
            m_settings.GenerateExtensionScript = GenerateExtensionScript;

            m_settingsAccess.SaveSettings(m_settings);
            m_eventAggregator.GetEvent<SettingsChangedEvent>().Publish(m_settings);

            Close();
        }
    }
}
