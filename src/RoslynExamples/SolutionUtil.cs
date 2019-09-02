using ScriptLinker.Infrastructure.Utilities;
using System;
using System.Collections.Generic;
using System.IO;

namespace RoslynExamples
{
    class ProjectSummary
    {
        public string Name { get; set; }
        public string File { get; set; }
        public string Guid { get; set; }
    }

    static class SolutionUtil
    {
        internal static IEnumerable<ProjectSummary> GetProjects(string solutionFile)
        {
            var solutionDir = Path.GetDirectoryName(solutionFile);

            foreach (var line in File.ReadLines(solutionFile))
            {
                if (line.TrimStart().StartsWith("Project("))
                {
                    var projectParts = line.Trim('[', ']', ' ').Split(',', '=');

                    yield return new ProjectSummary()
                    {
                        Name = projectParts[1].Trim(' ', '"'),
                        File = Path.Combine(solutionDir, projectParts[2].Trim(' ', '"')),
                        Guid = projectParts[3].Trim(' ', '"'),
                    };
                }
            }
        }

        internal static string GetSolutionFile(string projectFile)
        {
            var directory = Path.GetDirectoryName(projectFile);

            while (!PathUtil.IsRoot(directory))
            {
                var solutionFiles = new DirectoryInfo(directory).GetFiles("*.sln");

                foreach (var solutionFile in solutionFiles)
                {
                    foreach (var project in GetProjects(solutionFile.FullName))
                    {
                        if (project.File == projectFile)
                            return solutionFile.FullName;
                    }
                }

                directory = Directory.GetParent(directory).ToString();
            }

            return string.Empty;
        }

        internal static string GetProjectFile(string documentPath)
        {
            var directory = Path.GetDirectoryName(documentPath);

            while (!PathUtil.IsRoot(directory))
            {
                var projectFiles = new DirectoryInfo(directory).GetFiles("*.csproj");

                foreach (var projectFile in projectFiles)
                {
                    if (IsInProject(projectFile.FullName, documentPath))
                    {
                        return projectFile.FullName;
                    }
                }

                directory = Directory.GetParent(directory).ToString();
            }

            return string.Empty;
        }

        private static bool IsInProject(string projectFile, string documentPath)
        {
            var projectDirectory = Path.GetDirectoryName(projectFile);
            var relativeDocumentPath = PathUtil.GetRelativePath(documentPath, projectDirectory);

            foreach (var line in File.ReadLines(projectFile))
            {
                if (line.TrimStart().StartsWith("<Compile"))
                {
                    if (line.Contains(relativeDocumentPath))
                        return true;
                }
            }

            return false;
        }
    }
}
