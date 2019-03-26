using System.Collections.Generic;

namespace ScriptImporter.Models
{
    public class MergeInfo
    {
        public string Content { get; set; }
        public HashSet<string> MergedFiles { get; set; }
    }
}
