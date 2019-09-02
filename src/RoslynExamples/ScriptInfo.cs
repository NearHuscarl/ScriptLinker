using System;

namespace RoslynExamples
{
    public class ScriptInfo : IEquatable<ScriptInfo>
    {
        public string Name { get; set; } = "";
        public string EntryPoint { get; set; } = "";
        public string ProjectFile { get; set; } = "";
        public string Author { get; set; } = "";
        public string Description { get; set; } = "";
        public string MapModes { get; set; } = "";

        public bool IsEmpty()
        {
            return Name == ""
                || EntryPoint == ""
                || ProjectFile == "";
        }

        public bool Equals(ScriptInfo other)
        {
            if (other == null)
            {
                return false;
            }

            return this.Name == other.Name
                && this.EntryPoint == other.EntryPoint
                && this.ProjectFile == other.ProjectFile
                && this.Author == other.Author
                && this.Description == other.Description
                && this.MapModes == other.MapModes;
        }
    }
}
