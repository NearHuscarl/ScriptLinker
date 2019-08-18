namespace ScriptLinker.Models
{
    public class Settings
    {
        public string LastOpenedScript { get; set; } = "";
        public bool StandaloneScript { get; set; } = true;
        public bool IsLinkedFileWindowExpanded { get; set; } = false;
    }
}
