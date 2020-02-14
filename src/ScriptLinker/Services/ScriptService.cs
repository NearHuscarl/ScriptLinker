using System;
using System.Collections.Generic;
using System.IO;
using ScriptLinker.DataLogic;
using ScriptLinker.Models;
using ScriptLinker.Utilities;
using ScriptLinker.Infrastructure;
using ScriptLinker.Infrastructure.Utilities;

namespace ScriptLinker.Services
{
    class ScriptService
    {
        private Linker _linker;

        public ScriptService()
        {
            _linker = new Linker();
        }

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

            var fileInfo = _linker.ReadCSharpFile(projectInfo, entryPoint);
            var myNamespace = string.IsNullOrEmpty(fileInfo.Namespace) ? "SFDScript" : fileInfo.Namespace;
            var className = string.IsNullOrEmpty(fileInfo.ClassName) ?
                Path.GetFileNameWithoutExtension(entryPoint) : fileInfo.ClassName;
            var template = File.ReadAllText(Constant.TemplatePath);

            File.WriteAllText(entryPoint, template
                .Replace("{{Namespace}}", myNamespace)
                .Replace("{{ClassName}}", className));
        }

        public string GetBackupFolder()
        {
            return Path.Combine(Constant.DataDirectory, "Backup");
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

        public void CheckRemoveBackupFiles()
        {
            var backupFolder = new DirectoryInfo(GetBackupFolder());

            if (!backupFolder.Exists) return;

            foreach (var backupFile in backupFolder.GetFiles("~*"))
            {
                var creationTime = backupFile.CreationTimeUtc;
                var timeNow = DateTime.UtcNow;

                if (timeNow.Subtract(creationTime).Hours >= 72)
                {
                    backupFile.Delete();
                }
            }
        }

        public async void CreateExtensionScript(string bundlePath, string sourceCode)
        {
            var destinationPath = Path.Combine(Constant.ScriptDirectory, Path.GetFileName(bundlePath));

            await FileUtil.CopyFileAsync(bundlePath, destinationPath);
        }
    }
}
