namespace ScriptLinker.Models
{
    public class Settings
    {
        public bool InitTemplateOnCreated { get; set; } = true;
        public string LastOpenedScript { get; set; } = "";
        public bool GenerateExtensionScript { get; set; } = true;
        public bool IsLinkedFileWindowExpanded { get; set; } = false;

        public string CopyToClipboardHotkey { get; set; } = "F4";
        public string CompileHotkey { get; set; } = "F6";
        public string CompileAndRunHotkey { get; set; } = "F8";
    }
}
