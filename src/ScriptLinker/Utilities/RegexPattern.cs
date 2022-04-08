using System.Text.RegularExpressions;

namespace ScriptLinker.Utilities
{
    public static class RegexPattern
    {
        public static Regex AccessModifier = new Regex(@"(public|protected|private|internal)?");
        public static Regex Identifier = new Regex(@"[a-zA-Z0-9_]+"); // variable, class, method

        public static Regex Namespace = new Regex(@"^\s*namespace (.*)");
        // public partial class ClassName : GameScriptInterface
        public static Regex Class = new Regex($@"^\s*{AccessModifier}\s+(partial\s)?class\s+({Identifier})\s*:\s*({Identifier})");
        public static Regex EntryPointClass = new Regex($@"^\s*{AccessModifier}\s+(partial\s)?class\s+{Identifier}\s*:\s*GameScriptInterface");
        public static Regex PartialClass = new Regex($@"^\s*{AccessModifier}\s+partial\s+class\s+{Identifier}");
        // public ClassName()
        public static Regex Constructor = new Regex($@"^\s*{AccessModifier}\s+{Identifier}\(\)(?!.*?;).*$");
        public static Regex UsingStatement = new Regex(@"^\s*using\s+(.*);");

        public static Regex Comment = new Regex(@"^\s*\/\/");
        public static Regex Directive = new Regex(@"^\s*#");
    }
}
