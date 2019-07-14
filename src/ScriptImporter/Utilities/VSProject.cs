using System.IO;
using System.Xml.Linq;

namespace ScriptImporter.Utilities
{
    public static class VSProject
    {
        public static readonly XNamespace Msbuild = "http://schemas.microsoft.com/developer/msbuild/2003";

        public static XDocument GetProjectInfo(string projectDir)
        {
            var projectName = Path.GetFileName(projectDir);
            var csprojFile = Path.Combine(projectDir, projectName + ".csproj");

            if (!File.Exists(csprojFile))
                return null;

            return XDocument.Load(csprojFile);
        }
    }
}
