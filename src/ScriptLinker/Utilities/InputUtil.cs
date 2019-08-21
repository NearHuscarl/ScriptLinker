using System;

namespace ScriptLinker.Utilities
{
    class InputUtil
    {
        public static System.Windows.Input.Key WinformsToWPFKey(System.Windows.Forms.Keys formsKey)
        {
            return System.Windows.Input.KeyInterop.KeyFromVirtualKey((int)formsKey);
        }

        public static System.Windows.Forms.Keys WPFToWinformsKey(System.Windows.Input.Key wpfKey)
        {
            return (System.Windows.Forms.Keys)System.Windows.Input.KeyInterop.VirtualKeyFromKey(wpfKey);
        }
    }
}
