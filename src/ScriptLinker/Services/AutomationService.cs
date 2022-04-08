using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using ScriptLinker.DataLogic;
using ScriptLinker.Models;
using ScriptLinker.Infrastructure.Win;

namespace ScriptLinker.Services
{
    class AutomationService : IDisposable
    {
        private readonly Linker _linker;
        private WinService _winService;

        public AutomationService()
        {
            _linker = new Linker();
            _winService = new WinService();
        }

        public void RemoveFileModificationDetectedDialogOnCreated()
        {
            _winService.ForegroundChanged -= RemoveFileModificationDetectedDialog;
            _winService.ForegroundChanged += RemoveFileModificationDetectedDialog;
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

        public LinkResult CopyToClipboard(ProjectInfo projectInfo, ScriptInfo scriptInfo, LinkOption option)
        {
            var result = _linker.Link(projectInfo, scriptInfo, option);

            Clipboard.SetText(result.Content);

            return result;
        }

        public LinkResult Compile(ProjectInfo projectInfo, ScriptInfo scriptInfo, LinkOption option, Action onCompiled = null)
        {
            var sfdProcess = Process.GetProcessesByName("Superfighters Deluxe").FirstOrDefault();

            if (sfdProcess == null)
                return new LinkResult(AutomationError.SfdNotOpen);

            var result = CopyToClipboard(projectInfo, scriptInfo, option);
            var sourceCode = result.Content;
            //var sourceCode = await Task.Run(() => _linker.Link(ProjectInfo, ScriptInfo));

            // Switch to the Script Editor window
            WinUtil.BringWindowToFront("Script Editor");

            // Wait until the window is switching back
            while (WinUtil.GetActiveWindowTitle() == null) { }

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

                onCompiled?.Invoke();

                return result;
            }
            else
                return new LinkResult(AutomationError.ScriptEditorNotOpen);
        }

        public LinkResult CompileAndRun(ProjectInfo projectInfo, ScriptInfo scriptInfo, LinkOption option)
        {
            return Compile(projectInfo, scriptInfo, option, RunMapEditorTestIfSuccess);
        }

        private void RunMapEditorTestIfSuccess()
        {
            _winService.ForegroundChanged += OnCompileResultDialogOpened;
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

                _winService.ForegroundChanged -= OnCompileResultDialogOpened;
            }
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
                _winService.Dispose();
                IsDisposed = true;
            }
        }

        #endregion
    }
}
