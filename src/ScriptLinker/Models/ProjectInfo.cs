using System.Collections.Generic;

namespace ScriptLinker.Models
{
    public class ProjectInfo
    {
        /// <summary>
        /// The folder which contains *.csproj file
        /// </summary>
        public string ProjectDir { get; set; }

        /// <summary>
        /// The path to the main C# file that include all other files
        /// </summary>
        public string EntryPoint { get; set; }

        public string RootNamespace { get; set; }
        public IEnumerable<Breakpoint> Breakpoints { get; set; }

        public ProjectInfo() : this("", "", "")
        {
        }

        public ProjectInfo(string projectDir, string entryPoint, string rootNamespace)
        {
            ProjectDir = projectDir;
            EntryPoint = entryPoint;
            RootNamespace = rootNamespace;
            Breakpoints = new List<Breakpoint>();
        }
    }
}
