using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using System.IO;
using System.Linq;

namespace RoslynExamples.Extensions
{
    static class WorkspaceExtensions
    {
        internal static DocumentId GetDocumentId(this MSBuildWorkspace workspace, string filePath)
        {
            var solution = workspace.CurrentSolution;
            return solution.GetDocumentIdsWithFilePath(filePath).Single();
        }

        internal static Project GetProject(this Solution solution, string projectPath)
        {
            return solution.Projects
                .Where(p => p.Name == Path.GetFileNameWithoutExtension(projectPath))
                .First();
        }
    }
}
