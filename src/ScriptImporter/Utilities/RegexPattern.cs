using System.Text.RegularExpressions;

namespace ScriptImporter.Utilities
{
    public static class RegexPattern
    {
        public static Regex Namespace = new Regex(@"^\s*namespace (.*)");
        // public partial class GameScript() : base(null) { }
        public static Regex GameScriptClass = new Regex(@"^\s*public\s+(partial)+\s+class\s+GameScript()");
        public static Regex GameScriptCtor = new Regex(@"^\s*public\s+GameScript()");
        public static Regex UsingStatement = new Regex(@"^\s*using\s+(.*);");
    }
}
