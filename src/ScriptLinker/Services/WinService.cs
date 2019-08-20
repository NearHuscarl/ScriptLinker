using ScriptLinker.Utilities;
using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Windows.Automation;
using System.Diagnostics;

namespace ScriptLinker.Services
{
    public class WinService : IDisposable
    {
        private GlobalKeyboardHook m_globalKeyboardHook;

        public event KeyEventHandler GlobalKeyDown
        {
            add { m_globalKeyboardHook.KeyDown += value; }
            remove { m_globalKeyboardHook.KeyDown -= value; }
        }

        public event KeyEventHandler GlobalKeyUp
        {
            add { m_globalKeyboardHook.KeyUp += value; }
            remove { m_globalKeyboardHook.KeyUp -= value; }
        }

        public WinService()
        {
            m_globalKeyboardHook = new GlobalKeyboardHook();
        }

        public void AddGlobalHookedKey(Keys key)
        {
            m_globalKeyboardHook.HookedKeys.Add(key);
        }

        public void InitKillFileModificationDetectedDialog()
        {
            // https://stackoverflow.com/a/3497278/9449426
            Automation.AddAutomationEventHandler(
                WindowPattern.WindowOpenedEvent,
                AutomationElement.RootElement,
                TreeScope.Descendants,
                (sender, e) =>
                {
                    var element = sender as AutomationElement;

                    if (element.GetText() == "Microsoft Visual Studio")
                    {
                        // Focus and enter to accept external changes to Visual Studio files
                        WinUtil.BringWindowToFront("Microsoft Visual Studio");
                        WinUtil.SimulateKey("{ENTER}");
                    }
                });
        }

        #region Dispose pattern

        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                m_globalKeyboardHook.Dispose();
                Automation.RemoveAllEventHandlers();
                IsDisposed = true;
            }
        }

        #endregion
    }
}
