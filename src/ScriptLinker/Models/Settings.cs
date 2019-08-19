namespace ScriptLinker.Models
{
    public class Settings
    {
        public bool InitTemplateOnCreated { get; set; } = true;
        public string LastOpenedScript { get; set; } = "";
        public bool StandaloneScript { get; set; } = true;
        public bool IsLinkedFileWindowExpanded { get; set; } = false;
    }
}
