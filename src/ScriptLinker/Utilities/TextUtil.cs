namespace ScriptLinker.Utilities
{
    public static class TextUtil
    {
        /// <summary>
        /// Return if the character code is printable: letters, digits, punctuation marks, and a few miscellaneous symbols
        /// </summary>
        /// <param name="characterCode"></param>
        /// <returns></returns>
        public static bool IsPrintable(int characterCode)
        {
            return 32 <= characterCode && characterCode <= 126;
        }

        public static bool IsAlphabetical(int characterCode)
        {
            return (65 <= characterCode && characterCode <= 90)
                || (97 <= characterCode && characterCode <= 122);
        }
    }
}
