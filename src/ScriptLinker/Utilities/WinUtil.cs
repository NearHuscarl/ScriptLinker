using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace ScriptLinker.Utilities
{
    public static class WinUtil
    {
        /// <summary>
        /// Usage: WinUtil.BringWindowToFront("windowTitle");
        /// to switch the focus to another application
        /// </summary>
        /// <param name="processName"></param>
        public static bool BringWindowToFront(string windowTitle, Action action)
        {
            var windows = GetWindows();

            foreach (var window in windows)
            {
                if (window.Title == windowTitle)
                {
                    var fgThread = WinAPI.GetWindowThreadProcessId(WinAPI.GetForegroundWindow(), out uint a);
                    var appThread = WinAPI.GetCurrentThreadId();

                    // the issue facing the design of SetForegroundWindow is that it can be used for focus stealing.
                    // Focus is something that users should control. Applications that change the focus can be troublesome.
                    // And so SetForegroundWindow attempts to defend against focus stealers. From document:
                    //
                    // A process can set the foreground window only if one of the following conditions is true:
                    // - The process is the foreground process.
                    // ...
                    //
                    // Note that a process that is being debugged is always granted permission to set foreground window.
                    // That explains why you see no problems while debugging. But outside a debugger, if your process is
                    // not the foreground process, then calls to SetForegroundWindow fail.
                    //
                    // The trick is to make windows 'think' that our process and the target window are related by
                    // attaching the app thread to the currently focused window thread (foreground thread)
                    if (fgThread != appThread)
                    {
                        WinAPI.AttachThreadInput(fgThread, appThread, true);

                        WinAPI.LockSetForegroundWindow(WinAPI.LSFW_UNLOCK);
                        WinAPI.AllowSetForegroundWindow(WinAPI.ASFW_ANY);
                        WinAPI.BringWindowToTop(window.Handle);
                        // set user the focus to the window
                        WinAPI.SetForegroundWindow(window.Handle);

                        WinAPI.AttachThreadInput(fgThread, appThread, false);
                    }
                    else
                    {
                        WinAPI.LockSetForegroundWindow(WinAPI.LSFW_UNLOCK);
                        WinAPI.AllowSetForegroundWindow(WinAPI.ASFW_ANY);
                        WinAPI.BringWindowToTop(window.Handle);
                        WinAPI.SetForegroundWindow(window.Handle);
                    }

                    return true;
                }
            }

            return false;
        }

        public static bool BringWindowToFront(Process process)
        {
            // check if the process is running
            if (process != null)
            {
                // check if the window is hidden / minimized
                if (process.MainWindowHandle == IntPtr.Zero)
                {
                    // the window is hidden so try to restore it before setting focus.
                    WinAPI.ShowWindow(process.Handle, WinAPI.ShowWindowFlag.Restore);
                }

                // set user the focus to the window
                return WinAPI.SetForegroundWindow(process.MainWindowHandle);
            }
            else
            {
                return false;
            }
        }

        public static void Simulate(Process process, string key)
        {
            process.WaitForInputIdle();
            SendKeys.SendWait(key);
        }

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        public static string GetActiveWindowTitle()
        {
            const int nChars = 256;
            var Buff = new StringBuilder(nChars);
            var handle = GetForegroundWindow();

            if (GetWindowText(handle, Buff, nChars) > 0)
            {
                return Buff.ToString();
            }
            return null;
        }


        public delegate bool EnumWindowsProc(IntPtr hwnd, int lParam);

        [DllImport("user32")]
        private static extern int GetWindowLongA(IntPtr hWnd, int index);

        [DllImport("USER32.DLL")]
        private static extern bool EnumWindows(EnumWindowsProc enumFunc, int lParam);

        private const int GWL_STYLE = -16;

        private const ulong WS_VISIBLE = 0x10000000L;
        private const ulong WS_BORDER = 0x00800000L;
        private const ulong TARGETWINDOW = WS_BORDER | WS_VISIBLE;

        public class Window
        {
            public string Title;
            public IntPtr Handle;

            public override string ToString()
            {
                return Title;
            }
        }

        public static List<Window> GetWindows()
        {
            var windows = new List<Window>();

            EnumWindows((IntPtr hwnd, int lParam) =>
            {
                if (((ulong)GetWindowLongA(hwnd, GWL_STYLE) & TARGETWINDOW) == TARGETWINDOW)
                {
                    StringBuilder sb = new StringBuilder(100);
                    GetWindowText(hwnd, sb, sb.Capacity);

                    Window t = new Window
                    {
                        Handle = hwnd,
                        Title = sb.ToString()
                    };
                    windows.Add(t);
                }

                return true; // continue enumeration
            }, 0);

            return windows;
        }
    }
}
