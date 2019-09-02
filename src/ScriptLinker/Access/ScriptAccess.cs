using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using ScriptLinker.Models;
using ScriptLinker.Infrastructure;

namespace ScriptLinker.Access
{
    public class ScriptAccess
    {
        public string ConfigPath { get; set; } = Path.Combine(Constant.DataDirectory, "script.xml");

        public List<string> GetScriptNames()
        {
            var names = new List<string>();

            if (!File.Exists(ConfigPath))
                return names;

            var scriptDoc = XDocument.Load(ConfigPath);
            var scriptInfos = scriptDoc.Element("Script").Elements("ScriptInfo");
            var scriptInfo = new ScriptInfo();

            foreach (var scriptInfoElement in scriptInfos)
            {
                names.Add(scriptInfoElement.Element("Name").Value);
            }

            return names;
        }

        public ScriptInfo LoadScriptInfo(string projectDir, string entryPoint)
        {
            try
            {
                if (!File.Exists(ConfigPath))
                    return new ScriptInfo();

                var scriptDoc = XDocument.Load(ConfigPath);
                var scriptInfos = scriptDoc.Element("Script").Elements("ScriptInfo");
                var scriptInfo = new ScriptInfo();

                foreach (var scriptInfoElement in scriptInfos)
                {
                    if (scriptInfoElement.Element("EntryPoint").Value == entryPoint
                       && scriptInfoElement.Element("ProjectDirectory").Value == projectDir)
                    {
                        scriptInfo.Name = scriptInfoElement.Element("Name").Value;
                        scriptInfo.EntryPoint = scriptInfoElement.Element("EntryPoint").Value;
                        scriptInfo.ProjectDirectory = scriptInfoElement.Element("ProjectDirectory").Value;
                        scriptInfo.Author = scriptInfoElement.Element("Author").Value;
                        scriptInfo.Description = scriptInfoElement.Element("Description").Value;
                        scriptInfo.MapModes = scriptInfoElement.Element("MapModes").Value;
                        break;
                    }
                }

                return scriptInfo;
            }
            catch (Exception)
            {
                return new ScriptInfo();
            }
        }

        public ScriptInfo LoadScriptInfo(string scriptName)
        {
            try
            {
                if (!File.Exists(ConfigPath))
                    return new ScriptInfo();

                var scriptDoc = XDocument.Load(ConfigPath);
                var scriptInfos = scriptDoc.Element("Script").Elements("ScriptInfo");
                var scriptInfo = new ScriptInfo();

                foreach (var scriptInfoElement in scriptInfos)
                {
                    if (scriptInfoElement.Element("Name").Value == scriptName)
                    {
                        scriptInfo.Name = scriptInfoElement.Element("Name").Value;
                        scriptInfo.EntryPoint = scriptInfoElement.Element("EntryPoint").Value;
                        scriptInfo.ProjectDirectory = scriptInfoElement.Element("ProjectDirectory").Value;
                        scriptInfo.Author = scriptInfoElement.Element("Author").Value;
                        scriptInfo.Description = scriptInfoElement.Element("Description").Value;
                        scriptInfo.MapModes = scriptInfoElement.Element("MapModes").Value;
                        break;
                    }
                }

                return scriptInfo;
            }
            catch (Exception)
            {
                return new ScriptInfo();
            }
        }

        public void UpdateScriptInfo(ScriptInfo updatedScriptInfo)
        {
            if (updatedScriptInfo.IsEmpty()) return;

            var scriptDoc = File.Exists(ConfigPath) ? XDocument.Load(ConfigPath) : new XDocument();
            var query = from c in scriptDoc.Elements("Script").Elements("ScriptInfo") select c;
            var isUpdated = false;

            foreach (var scriptInfo in query)
            {
                if (scriptInfo.Element("Name").Value == updatedScriptInfo.Name
                    || scriptInfo.Element("EntryPoint").Value == updatedScriptInfo.EntryPoint
                    && scriptInfo.Element("ProjectDirectory").Value == updatedScriptInfo.ProjectDirectory)
                {
                    scriptInfo.Element("Name").Value = updatedScriptInfo.Name;
                    scriptInfo.Element("EntryPoint").Value = updatedScriptInfo.EntryPoint;
                    scriptInfo.Element("ProjectDirectory").Value = updatedScriptInfo.ProjectDirectory;
                    scriptInfo.Element("Author").Value = updatedScriptInfo.Author;
                    scriptInfo.Element("Description").Value = updatedScriptInfo.Description;
                    scriptInfo.Element("MapModes").Value = updatedScriptInfo.MapModes;
                    isUpdated = true;
                    break;
                }
            }

            if (!isUpdated)
            {
                scriptDoc.Element("Script").Add(
                    new XElement("ScriptInfo",
                        new XElement("Name", updatedScriptInfo.Name),
                        new XElement("EntryPoint", updatedScriptInfo.EntryPoint),
                        new XElement("ProjectDirectory", updatedScriptInfo.ProjectDirectory),
                        new XElement("Author", updatedScriptInfo.Author),
                        new XElement("Description", updatedScriptInfo.Description),
                        new XElement("MapModes", updatedScriptInfo.MapModes)
                    )
                );
            }

            scriptDoc.Save(ConfigPath);
        }

        public void RemoveScriptInfo(string scriptName)
        {
            if (!File.Exists(ConfigPath)) return;

            var scriptDoc = XDocument.Load(ConfigPath);
            var query = from c in scriptDoc.Elements("Script").Elements("ScriptInfo") select c;

            foreach (var scriptInfo in query.ToList())
            {
                if (scriptInfo.Element("Name").Value == scriptName)
                {
                    scriptInfo.Remove();
                    break;
                }
            }

            scriptDoc.Save(ConfigPath);
        }

        public void RemoveNotFoundScriptInfo()
        {
            if (!File.Exists(ConfigPath)) return;

            var scriptDoc = XDocument.Load(ConfigPath);
            var query = from c in scriptDoc.Elements("Script").Elements("ScriptInfo") select c;

            foreach (var scriptInfo in query.ToList())
            {
                var entryPoint = scriptInfo.Element("EntryPoint").Value;
                var projectDirectory = scriptInfo.Element("ProjectDirectory").Value;

                if (!Directory.Exists(projectDirectory) || !File.Exists(entryPoint))
                {
                    scriptInfo.Remove();
                }
            }

            scriptDoc.Save(ConfigPath);
        }
    }
}
