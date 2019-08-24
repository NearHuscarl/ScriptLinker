using System.Collections.Generic;

namespace ScriptLinker.Models
{
    public enum AutomationError
    {
        None,
        SfdNotOpen,
        ScriptEditorNotOpen,
    }

    public class LinkResult
    {
        public LinkResult(AutomationError error)
        {
            Error = error;
        }

        public LinkResult() : this(AutomationError.None)
        {

        }

        public long Elapsed { get; set; } = 0L;
        public string Content { get; set; } = "";
        public HashSet<string> LinkedFiles { get; set; } = new HashSet<string>();
        public AutomationError Error { get; set; } = AutomationError.None;
    }
}
