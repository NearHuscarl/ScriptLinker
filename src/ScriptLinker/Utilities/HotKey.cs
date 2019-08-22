using System;
using System.Text;
using System.Windows.Input;

namespace ScriptLinker.Utilities
{
    public class HotKey : IEquatable<HotKey>
    {
        public Key Key { get; private set; }
        public ModifierKeys Modifiers { get; private set; }

        public HotKey(Key key, ModifierKeys modifiers)
        {
            Key = key;
            Modifiers = modifiers;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            if (!InputUtil.IsModifierKey(Key))
            {
                if (Modifiers.HasFlag(ModifierKeys.Control))
                    sb.Append("Ctrl+");
                if (Modifiers.HasFlag(ModifierKeys.Shift))
                    sb.Append("Shift+");
                if (Modifiers.HasFlag(ModifierKeys.Alt))
                    sb.Append("Alt+");
                if (Modifiers.HasFlag(ModifierKeys.Windows))
                    sb.Append("Win+");
            }

            return sb.Append(Key.ToString()).ToString();
        }

        public bool Equals(HotKey other)
        {
            if (other == null) return false;

            return this.Key == other.Key && this.Modifiers == other.Modifiers;
        }

        public override int GetHashCode()
        {
            // only needed if you're compiling with arithmetic checks enabled
            // (the default compiler behaviour is *disabled*, so most folks won't need this)
            unchecked
            {
                var hash = 13;

                hash = (hash * 7) + Key.GetHashCode();
                hash = (hash * 7) + Modifiers.GetHashCode();

                return hash;
            }
        }
    }
}
