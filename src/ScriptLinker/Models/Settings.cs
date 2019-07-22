namespace ScriptLinker.Models
{
    public class Settings
    {
        public string EntryPoint { get; set; } = "";
        public string ProjectDirectory { get; set; } = "";
        public bool StandaloneScript { get; set; } = true;
        public bool IsLinkedFileWindowExpanded { get; set; } = false;
    }
}
