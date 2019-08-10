using System;
using System.Runtime.InteropServices;
using System.Text;

namespace ScriptLinker.Utilities
{
    public static class WinAPI
    {
        /// <summary>
        /// Retrieves the thread identifier of the calling thread.
        /// </summary>
        /// <returns></returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern uint GetCurrentThreadId();

        /// <summary>
        /// Retrieves the identifier of the thread that created the specified window and, optionally, the identifier of the process
        /// that created the window.
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="lpdwProcessId"></param>
        /// <returns></returns>
        [DllImport("user32.dll", SetLastError = true)]
        internal static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        /// <summary>
        /// Enables the specified process to set the foreground window using the SetForegroundWindow function.
        /// The calling process must already be able to set the foreground window. For more information, see Remarks later in this topic.
        /// </summary>
        /// <param name="dwProcessId"></param>
        /// <returns></returns>
        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool AllowSetForegroundWindow(int dwProcessId);
        internal static readonly int ASFW_ANY = -1; // by MSDN

        /// <summary>
        /// Attaches or detaches the input processing mechanism of one thread to that of another thread.
        /// </summary>
        /// <param name="idAttach"></param>
        /// <param name="idAttachTo"></param>
        /// <param name="fAttach"></param>
        /// <returns></returns>
        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

        /// <summary>
        /// Brings the specified window to the top of the Z order. If the window is a top-level window, it is activated.
        /// If the window is a child window, the top-level parent window associated with the child window is activated.
        /// </summary>
        /// <param name="hWnd"></param>
        /// <returns></returns>
        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool BringWindowToTop(IntPtr hWnd);

        /// <summary>
        /// Enumerates all top-level windows on the screen by passing the handle to each window, in turn, to an
        /// application-defined callback function. EnumWindows continues until the last top-level window is enumerated
        /// or the callback function returns false.
        /// </summary>
        /// <param name="lpEnumFunc"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        /// <summary>
        /// An application-defined callback function used with the EnumWindows or EnumDesktopWindows function.
        /// It receives top-level window handles. The WNDENUMPROC type defines a pointer to this callback function.
        /// EnumWindowsProc is a placeholder for the application-defined function name.
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        internal delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        /// <summary>
        /// Retrieves a handle to the foreground window (the window with which the user is currently working).
        /// The system assigns a slightly higher priority to the thread that creates the foreground window than it does to other threads.
        /// </summary>
        /// <returns></returns>
        [DllImport("user32.dll")]
        internal static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        private static extern IntPtr GetWindowLongPtr32(IntPtr hWnd, GWL nIndex);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
        private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, GWL nIndex);

        /// <summary>
        /// Retrieves information about the specified window. The function also retrieves the 32-bit (DWORD) value
        /// at the specified offset into the extra window memory.
        ///
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="nIndex"></param>
        /// <returns></returns>
        // Note: This static method is required because Win32 does not support GetWindowLongPtr directly
        internal static IntPtr GetWindowLongPtr(IntPtr hWnd, GWL nIndex)
        {
            if (IntPtr.Size == 8)
                return GetWindowLongPtr64(hWnd, nIndex);
            else
                return GetWindowLongPtr32(hWnd, nIndex);
        }

        /// <summary>
        /// WindowLongFlags - Flags for GetWindowLong, GetWindowLongPtr, SetWindowLong & SetWindowLongPtr
        /// </summary>
        internal enum GWL
        {
            WndProc = (-4),
            HInstance = (-6),
            HWndParent = (-8),
            Style = (-16),
            ExStyle = (-20),
            UserData = (-21),
            ID = (-12)
        }

        /// <summary>
        /// Window Styles.
        /// The following styles can be specified wherever a window style is required. After the control has been created, these styles cannot be modified, except as noted.
        /// </summary>
        [Flags()]
        internal enum WindowStyle : uint
        {
            /// <summary>The window has a thin-line border.</summary>
            Border = 0x800000,

            /// <summary>The window has a title bar (includes the Border style).</summary>
            Caption = 0xc00000,

            /// <summary>The window is a child window. A window with this style cannot have a menu bar. This style cannot be used
            /// with the Popup style.</summary>
            Child = 0x40000000,

            /// <summary>Excludes the area occupied by child windows when drawing occurs within the parent window. This style is
            /// used when creating the parent window.</summary>
            ClipChildren = 0x2000000,

            /// <summary>
            /// Clips child windows relative to each other; that is, when a particular child window receives a WM_PAINT message,
            /// the ClipSiblings style clips all other overlapping child windows out of the region of the child window to be updated.
            /// If ClipSiblings is not specified and child windows overlap, it is possible, when drawing within the client area of a
            /// child window, to draw within the client area of a neighboring child window.
            /// </summary>
            ClipSiblings = 0x4000000,

            /// <summary>The window is initially disabled. A disabled window cannot receive input from the user. To change this
            /// after a window has been created, use the EnableWindow function.</summary>
            Disabled = 0x8000000,

            /// <summary>The window has a border of a style typically used with dialog boxes. A window with this style cannot have
            /// a title bar.</summary>
            DlgFrame = 0x400000,

            /// <summary>
            /// The window is the first control of a group of controls. The group consists of this first control and all controls
            /// defined after it, up to the next control with the Group style.
            /// The first control in each group usually has the Tabstop style so that the user can move from group to group.
            /// The user can subsequently change the keyboard focus from one control in the group to the next control in the group
            /// by using the direction keys.
            /// You can turn this style on and off to change dialog box navigation. To change this style after a window has been
            /// created, use the SetWindowLong function.
            /// </summary>
            Group = 0x20000,

            /// <summary>The window has a horizontal scroll bar.</summary>
            HScroll = 0x100000,

            /// <summary>The window is initially maximized.</summary> 
            Maximize = 0x1000000,

            /// <summary>The window has a maximize button. Cannot be combined with the WS_EX_CONTEXTHELP style. The SysMenu style
            /// must also be specified.</summary> 
            MaximizeBox = 0x10000,

            /// <summary>The window is initially minimized.</summary>
            Minimize = 0x20000000,

            /// <summary>The window has a minimize button. Cannot be combined with the WS_EX_CONTEXTHELP style. The SysMenu style
            /// must also be specified.</summary>
            MinimizeBox = 0x20000,

            /// <summary>The window is an overlapped window. An overlapped window has a title bar and a border.</summary>
            Overlapped = 0x0,

            /// <summary>The window is an overlapped window.</summary>
            OverlappedWindow = Overlapped | Caption | SysMenu | SizeFrame | MinimizeBox | MaximizeBox,

            /// <summary>The window is a pop-up window. This style cannot be used with the Child style.</summary>
            Popup = 0x80000000u,

            /// <summary>The window is a pop-up window. The Caption and PopupWindow styles must be combined to make the window
            /// menu visible.</summary>
            PopupWindow = Popup | Border | SysMenu,

            /// <summary>The window has a sizing border.</summary>
            SizeFrame = 0x40000,

            /// <summary>The window has a window menu on its title bar. The Caption style must also be specified.</summary>
            SysMenu = 0x80000,

            /// <summary>
            /// The window is a control that can receive the keyboard focus when the user presses the TAB key.
            /// Pressing the TAB key changes the keyboard focus to the next control with the Tabstop style.  
            /// You can turn this style on and off to change dialog box navigation. To change this style after a window has
            /// been created, use the SetWindowLong function.
            /// For user-created windows and modeless dialogs to work with tab stops, alter the message loop to call the
            /// IsDialogMessage function.
            /// </summary>
            Tabstop = 0x10000,

            /// <summary>The window is initially visible. This style can be turned on and off by using the ShowWindow or
            /// SetWindowPos function.</summary>
            Visible = 0x10000000,

            /// <summary>The window has a vertical scroll bar.</summary>
            VScroll = 0x200000
        }

        /// <summary>
        ///     Copies the text of the specified window's title bar (if it has one) into a buffer. If the specified window is a
        ///     control, the text of the control is copied. However, GetWindowText cannot retrieve the text of a control in another
        ///     application.
        ///     <para>
        ///     Go to https://msdn.microsoft.com/en-us/library/windows/desktop/ms633520%28v=vs.85%29.aspx  for more
        ///     information
        ///     </para>
        /// </summary>
        /// <param name="hWnd">
        ///     C++ ( hWnd [in]. Type: HWND )<br />A <see cref="IntPtr" /> handle to the window or control containing the text.
        /// </param>
        /// <param name="lpString">
        ///     C++ ( lpString [out]. Type: LPTSTR )<br />The <see cref="StringBuilder" /> buffer that will receive the text. If
        ///     the string is as long or longer than the buffer, the string is truncated and terminated with a null character.
        /// </param>
        /// <param name="nMaxCount">
        ///     C++ ( nMaxCount [in]. Type: int )<br /> Should be equivalent to
        ///     <see cref="StringBuilder.Length" /> after call returns. The <see cref="int" /> maximum number of characters to copy
        ///     to the buffer, including the null character. If the text exceeds this limit, it is truncated.
        /// </param>
        /// <returns>
        ///     If the function succeeds, the return value is the length, in characters, of the copied string, not including
        ///     the terminating null character. If the window has no title bar or text, if the title bar is empty, or if the window
        ///     or control handle is invalid, the return value is zero. To get extended error information, call GetLastError.<br />
        ///     This function cannot retrieve the text of an edit control in another application.
        /// </returns>
        /// <remarks>
        ///     If the target window is owned by the current process, GetWindowText causes a WM_GETTEXT message to be sent to the
        ///     specified window or control. If the target window is owned by another process and has a caption, GetWindowText
        ///     retrieves the window caption text. If the window does not have a caption, the return value is a null string. This
        ///     behavior is by design. It allows applications to call GetWindowText without becoming unresponsive if the process
        ///     that owns the target window is not responding. However, if the target window is not responding and it belongs to
        ///     the calling application, GetWindowText will cause the calling application to become unresponsive. To retrieve the
        ///     text of a control in another process, send a WM_GETTEXT message directly instead of calling GetWindowText.<br />For
        ///     an example go to
        ///     <see cref="!:https://msdn.microsoft.com/en-us/library/windows/desktop/ms644928%28v=vs.85%29.aspx#sending">
        ///     Sending a
        ///     Message.
        ///     </see>
        /// </remarks>
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        /// <summary>
        /// The foreground process can call the LockSetForegroundWindow function to disable calls to the SetForegroundWindow function.
        /// </summary>
        /// <param name="uLockCode"></param>
        /// <returns></returns>
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool LockSetForegroundWindow(uint uLockCode);
        internal static readonly uint LSFW_LOCK = 1;
        internal static readonly uint LSFW_UNLOCK = 2;

        /// <summary>
        /// Sets the keyboard focus to the specified window. The window must be attached to the calling thread's message queue.
        /// </summary>
        /// <param name="hWnd"></param>
        /// <returns></returns>
        [DllImport("user32.dll", SetLastError = true)]
        internal static extern IntPtr SetFocus(IntPtr hWnd);

        /// <summary>
        /// Brings the thread that created the specified window into the foreground and activates the window.
        /// Keyboard input is directed to the window, and various visual cues are changed for the user.
        /// The system assigns a slightly higher priority to the thread that created the foreground window than it does to other threads.
        /// </summary>
        /// <param name="hwnd"></param>
        /// <returns></returns>
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetForegroundWindow(IntPtr hwnd);

        /// <summary>
        /// Sets the specified window's show state.
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="nCmdShow"></param>
        /// <returns></returns>
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool ShowWindow(IntPtr hWnd, ShowWindowFlag nCmdShow);

        /// <summary>
        /// Sets the show state of a window without waiting for the operation to complete.
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="nCmdShow"></param>
        /// <returns></returns>
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool ShowWindowAsync(IntPtr hWnd, ShowWindowFlag nCmdShow);

        internal enum ShowWindowFlag
        {
            Hide = 0,
            ShowNormal = 1,
            ShowMinimized = 2,
            ShowMaximized = 3,
            Maximize = 3,
            ShowNormalNoActivate = 4,
            Show = 5,
            Minimize = 6,
            ShowMinNoActivate = 7,
            ShowNoActivate = 8,
            Restore = 9,
            ShowDefault = 10,
            ForceMinimized = 11,
        };
    }
}
