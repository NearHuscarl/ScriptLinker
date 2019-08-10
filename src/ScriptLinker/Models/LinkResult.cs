using System.Collections.Generic;

namespace ScriptLinker.Models
{
    public class LinkResult
    {
        public string Content { get; set; } = "";
        public HashSet<string> LinkedFiles { get; set; } = new HashSet<string>();
    }
}
