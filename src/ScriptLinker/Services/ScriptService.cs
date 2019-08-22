using ScriptLinker.DataLogic;
using ScriptLinker.Models;
using ScriptLinker.Utilities;
using System.Collections.Generic;
using System.IO;

namespace ScriptLinker.Services
{
    class ScriptService
    {
        private Linker m_linker;

        public ScriptService()
        {
            m_linker = new Linker();
        }

        //public bool Validate(ScriptInfo scriptInfo)
        //{

        //}

        public ProjectInfo GetProjectInfo(ScriptInfo scriptInfo)
        {
            var projectInfo = new ProjectInfo
            {
                ProjectDir = scriptInfo.ProjectDirectory,
                EntryPoint = scriptInfo.EntryPoint,
                RootNamespace = ProjectUtil.GetRootNamespace(scriptInfo.ProjectDirectory)
            };

            if (projectInfo.RootNamespace != "")
            {
                var slnPath = ProjectUtil.GetSlnPath(scriptInfo.ProjectDirectory);

                if (slnPath != null)
                {
                    var visualSln = new VisualSln(slnPath);
                    projectInfo.Breakpoints = visualSln != null ? visualSln.GetBreakpoints() : new List<Breakpoint>();
                }
            }

            return projectInfo;
        }

        public void AddTemplate(ProjectInfo projectInfo, string entryPoint)
        {
            if (string.IsNullOrWhiteSpace(entryPoint)) return;

            BackupFile(entryPoint);

            var fileInfo = m_linker.ReadCSharpFile(projectInfo, entryPoint);
            var myNamespace = string.IsNullOrEmpty(fileInfo.Namespace) ? "SFDScript" : fileInfo.Namespace;
            var className = string.IsNullOrEmpty(fileInfo.ClassName) ?
                Path.GetFileNameWithoutExtension(entryPoint) : fileInfo.ClassName;
            var template = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "ScriptTemplate.txt"));

            File.WriteAllText(entryPoint, template
                .Replace("{{Namespace}}", myNamespace)
                .Replace("{{ClassName}}", className));
        }

        public string GetBackupFolder()
        {
            return Path.Combine(Directory.GetCurrentDirectory(), "Backup");
        }

        public string GetBackupFile(string path)
        {
            var backupFolder = GetBackupFolder();
            var backupFile = "~" + PathUtil.GetPathWithoutDrive(path).Replace(Path.DirectorySeparatorChar, '.');

            return Path.Combine(backupFolder, backupFile);
        }

        private void BackupFile(string entryPoint)
        {
            var backupFile = GetBackupFile(entryPoint);
            var text = File.ReadAllText(entryPoint);

            Directory.CreateDirectory(GetBackupFolder());
            File.WriteAllText(backupFile, text);
        }

        public LinkResult Link(ProjectInfo projectInfo, ScriptInfo scriptInfo)
        {
            return m_linker.Link(projectInfo, scriptInfo);
        }
    }
}
