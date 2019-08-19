using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace ScriptLinker.Utilities
{
    public static class ProjectUtil
    {
        public static readonly XNamespace Msbuild = "http://schemas.microsoft.com/developer/msbuild/2003";

        public static string GetRootNamespace(string projectDir)
        {
            var projectName = Path.GetFileName(projectDir);
            var csprojFile = Path.Combine(projectDir, projectName + ".csproj");

            if (!File.Exists(csprojFile))
            {
                return "";
            }

            var csProjDoc = XDocument.Load(csprojFile);

            if (csProjDoc == null)
            {
                return "";
            }

            return csProjDoc
                .Element(Msbuild + "Project")
                .Elements(Msbuild + "PropertyGroup").First()
                .Element(Msbuild + "RootNamespace").Value.ToString();
        }

        public static bool IsProjectDirectory(string path)
        {
            return new DirectoryInfo(path).GetFiles("*.csproj").Length > 0;
        }

        public static string GetSlnPath(string projectDir)
        {
            var path = projectDir;

            while (!PathUtil.IsRoot(path))
            {
                if (VisualSln.IsValidSolutionPath(path))
                {
                    return path;
                }
                path = Directory.GetParent(path).ToString();
            }

            return null;
        }
    }
}
