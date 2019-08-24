using System;
using System.Windows.Input;

namespace ScriptLinker.Utilities
{
    class InputUtil
    {
        private static KeyConverter _keyConverter = new KeyConverter();

        public static Key WinformsToWPFKey(System.Windows.Forms.Keys formsKey)
        {
            return KeyInterop.KeyFromVirtualKey((int)formsKey);
        }

        public static System.Windows.Forms.Keys WPFToWinformsKey(Key wpfKey)
        {
            return (System.Windows.Forms.Keys)KeyInterop.VirtualKeyFromKey(wpfKey);
        }

        public static bool IsModifierKey(Key key)
        {
            return (key == Key.LeftShift || key == Key.RightShift
                || key == Key.LeftCtrl || key == Key.RightCtrl
                || key == Key.LeftAlt || key == Key.RightAlt
                || key == Key.LWin || key == Key.RWin);
        }

        public static bool IsSame(Key key, ModifierKeys modifiers)
        {
            return (((key == Key.LeftShift || key == Key.RightShift) && modifiers == ModifierKeys.Shift)
                || ((key == Key.LeftCtrl || key == Key.RightCtrl) && modifiers == ModifierKeys.Control)
                || ((key == Key.LeftAlt || key == Key.RightAlt) && modifiers == ModifierKeys.Alt)
                || ((key == Key.LWin || key == Key.RWin) && modifiers == ModifierKeys.Windows));
        }

        public static HotKey Parse(string str)
        {
            var key = Key.None;
            var modifiers = ModifierKeys.None;
            var keyNames = str.Split('+');
            
            foreach (var keyName in keyNames)
            {
                var keyVal = (Key)_keyConverter.ConvertFromString(keyName);

                switch (keyVal)
                {
                    case Key.LeftShift:
                    case Key.RightShift:
                        modifiers = modifiers.Add(ModifierKeys.Shift);
                        break;

                    case Key.LeftCtrl:
                    case Key.RightCtrl:
                        modifiers = modifiers.Add(ModifierKeys.Control);
                        break;

                    case Key.LeftAlt:
                    case Key.RightAlt:
                        modifiers = modifiers.Add(ModifierKeys.Alt);
                        break;

                    case Key.LWin:
                    case Key.RWin:
                        modifiers = modifiers.Add(ModifierKeys.Windows);
                        break;

                    default:
                        key = keyVal;
                        break;
                }
            }

            return new HotKey(key, modifiers);
        }
    }
}
