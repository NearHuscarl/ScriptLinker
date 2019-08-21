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
        public static bool BringWindowToFront(string windowTitle)
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

        public static void SimulateKey(string key, Process process = null)
        {
            process?.WaitForInputIdle();
            SendKeys.SendWait(key);
        }

        public static string GetActiveWindowTitle()
        {
            const int nChars = 256;
            var buff = new StringBuilder(nChars);
            var handle = WinAPI.GetForegroundWindow();

            if (WinAPI.GetWindowText(handle, buff, nChars) > 0)
            {
                return buff.ToString();
            }
            return null;
        }

        public static string GetWindowTitle(IntPtr hWnd)
        {
            var textLength = WinAPI.GetWindowTextLength(hWnd);
            var sb = new StringBuilder(textLength + 1);

            WinAPI.GetWindowText(hWnd, sb, sb.Capacity);
            return sb.ToString();
        }

        public class Window
        {
            public string Title;
            public IntPtr Handle;

            public override string ToString()
            {
                return Title;
            }
        }

        public static IEnumerable<Window> GetWindows()
        {
            var windows = new List<Window>();
            var targetWindowStyle = WinAPI.WindowStyle.Border | WinAPI.WindowStyle.Visible;

            WinAPI.EnumWindows((IntPtr hwnd, IntPtr lParam) =>
            {
                if (((WinAPI.WindowStyle)WinAPI.GetWindowLongPtr(hwnd, WinAPI.GWL.Style) & targetWindowStyle) == targetWindowStyle)
                {
                    var sb = new StringBuilder(100);
                    WinAPI.GetWindowText(hwnd, sb, sb.Capacity);

                    var window = new Window
                    {
                        Handle = hwnd,
                        Title = sb.ToString()
                    };
                    windows.Add(window);
                }

                return true; // continue enumeration
            }, IntPtr.Zero);

            return windows;
        }
    }
}
