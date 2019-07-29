namespace ScriptLinker.Models
{
    public class Breakpoint
    {
        public string File { get; set; } = "";
        public string Symbol { get; set; } = "";
        public int LineNumber { get; set; } = -1;

        public override string ToString()
        {
            return $"{File}:{Symbol}:{LineNumber}";
        }
    }
}
