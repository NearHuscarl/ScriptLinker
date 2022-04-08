using System;

namespace ScriptLinker.Models
{
    public class ScriptInfo : IEquatable<ScriptInfo>
    {
        public string Name { get; set; } = "";
        public string EntryPoint { get; set; } = "";
        public string ProjectDirectory { get; set; } = "";
        public string Author { get; set; } = "";
        public string Description { get; set; } = "";
        public string MapModes { get; set; } = "";
        public DateTime LastAccess { get; set; } = DateTime.Now;

        public bool IsEmpty()
        {
            return Name == ""
                || EntryPoint == ""
                || ProjectDirectory == "";
        }

        public bool Equals(ScriptInfo other)
        {
            if (other == null)
            {
                return false;
            }

            return Name == other.Name
                && EntryPoint == other.EntryPoint
                && ProjectDirectory == other.ProjectDirectory
                && Author == other.Author
                && Description == other.Description
                && MapModes == other.MapModes;
        }
    }
}
