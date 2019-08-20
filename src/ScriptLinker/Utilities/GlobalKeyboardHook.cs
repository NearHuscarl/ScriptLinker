using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ScriptLinker.Utilities
{
    class GlobalKeyboardHook
    {
        const int WM_KEYDOWN = 0x100;
        const int WM_KEYUP = 0x101;
        const int WM_SYSKEYDOWN = 0x104;
        const int WM_SYSKEYUP = 0x105;

        private readonly WinAPI.KeyboardHookProc keyboardHookProc;
        /// <summary>
        /// The collections of keys to watch for
        /// </summary>
        public HashSet<Keys> HookedKeys = new HashSet<Keys>();
        /// <summary>
        /// Handle to the hook, need this to unhook and call the next hook
        /// </summary>
        IntPtr hookID = IntPtr.Zero;

        /// <summary>
        /// Occurs when one of the hooked keys is pressed
        /// </summary>
        public event KeyEventHandler KeyDown;
        /// <summary>
        /// Occurs when one of the hooked keys is released
        /// </summary>
        public event KeyEventHandler KeyUp;
        /// <summary>
        /// Occurs when one of the hooked keys is pressed and released
        /// </summary>
        //public event KeyEventHandler KeyPressed;

        /// <summary>
        /// Initializes a new instance of the <see cref="globalKeyboardHook"/> class and installs the keyboard hook.
        /// </summary>
        public GlobalKeyboardHook()
        {
            keyboardHookProc = HookCallback;
            Hook();
        }

        /// <summary>
        /// Installs the global hook
        /// </summary>
        public void Hook()
        {
            using (var currentProcess = Process.GetCurrentProcess())
            using (var currentModule = currentProcess.MainModule)
            {
                hookID = WinAPI.SetWindowsHookEx(WinAPI.HookType.KeyboardLL, keyboardHookProc,
                    WinAPI.GetModuleHandle(currentModule.ModuleName), 0);
            }
        }

        /// <summary>
        /// Uninstalls the global hook
        /// </summary>
        public void Unhook()
        {
            WinAPI.UnhookWindowsHookEx(hookID);
        }

        /// <summary>
        /// The callback for the keyboard hook
        /// </summary>
        /// <param name="code">The hook code, if it isn't >= 0, the function shouldn't do anyting</param>
        /// <param name="wParam">The event type</param>
        /// <param name="lParam">The keyhook event information</param>
        /// <returns></returns>
        public IntPtr HookCallback(int code, IntPtr wParam, ref WinAPI.KeyboardHookStruct lParam)
        {
            if (code >= 0)
            {
                var key = (Keys)lParam.vkCode;

                if (HookedKeys.Contains(key))
                {
                    var eventArgs = new KeyEventArgs(key);
                    if ((wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN) && (KeyDown != null))
                    {
                        KeyDown(this, eventArgs);
                    }
                    else if ((wParam == (IntPtr)WM_KEYUP || wParam == (IntPtr)WM_SYSKEYUP) && (KeyUp != null))
                    {
                        KeyUp(this, eventArgs);
                    }
                    if (eventArgs.Handled)
                        return new IntPtr(1);
                }
            }

            return WinAPI.CallNextHookEx(hookID, code, wParam, ref lParam);
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
                if (disposing)
                {
                    // Release managed resource
                }

                // Release unmanaged resource
                Unhook();
                IsDisposed = true;
            }
        }

        ~GlobalKeyboardHook()
        {
            Dispose(false);
        }

        #endregion

    }
}
