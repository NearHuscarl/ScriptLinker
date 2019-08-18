using System.Windows.Input;

namespace ScriptLinker.Utilities
{
    class HotKey
    {
        public HotKey(Key key, ModifierKeys modifiers)
        {
            Key = key;
            Modifiers = modifiers;
        }
        public ModifierKeys Modifiers { get; set; }
        public Key Key { get; set; }
    }
}
