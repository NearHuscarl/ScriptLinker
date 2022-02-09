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
        public string ConfigPath { get; set; } = Path.Combine(Constant.DataDirectory, "scripts.xml");

        public List<string> GetScriptNames()
        {
            var names = new List<string>();

            if (!File.Exists(ConfigPath))
                return names;

            var scriptDoc = XDocument.Load(ConfigPath);
            var scriptInfos = scriptDoc.Element("Scripts").Elements("ScriptInfo");
            var scriptInfo = new ScriptInfo();

            foreach (var scriptInfoElement in scriptInfos)
            {
                names.Add(scriptInfoElement.Element("Name").Value);
            }

            return names;
        }

        private DateTime ParseDate(string date)
        {
            return DateTime.TryParse(date ?? "", out DateTime dateTime) ? dateTime : DateTime.MinValue;
        }

        public ScriptInfo LoadScriptInfo(string scriptName)
        {
            try
            {
                if (!File.Exists(ConfigPath))
                    return new ScriptInfo();

                var scriptDoc = XDocument.Load(ConfigPath);
                var scriptInfos = scriptDoc.Element("Scripts").Elements("ScriptInfo");
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
                        scriptInfo.LastAccess = ParseDate(scriptInfoElement.Element("LastAccess")?.Value);
                        break;
                    }
                }

                return scriptInfo;
            }
            catch (Exception e)
            {
                return new ScriptInfo();
            }
        }

        public void UpdateScriptInfo(ScriptInfo updatedScriptInfo)
        {
            if (updatedScriptInfo.IsEmpty()) return;

            var scriptDoc = File.Exists(ConfigPath) ? XDocument.Load(ConfigPath) : new XDocument(new XElement("Scripts"));
            var query = from c in scriptDoc.Elements("Scripts").Elements("ScriptInfo") select c;
            var isUpdated = false;

            foreach (var scriptInfo in query)
            {
                if (scriptInfo.Element("Name").Value == updatedScriptInfo.Name
                    || scriptInfo.Element("EntryPoint").Value == updatedScriptInfo.EntryPoint
                    && scriptInfo.Element("ProjectDirectory").Value == updatedScriptInfo.ProjectDirectory)
                {
                    scriptInfo.ReplaceWith(new XElement("ScriptInfo",
                        new XElement("Name", updatedScriptInfo.Name),
                        new XElement("EntryPoint", updatedScriptInfo.EntryPoint),
                        new XElement("ProjectDirectory", updatedScriptInfo.ProjectDirectory),
                        new XElement("Author", updatedScriptInfo.Author),
                        new XElement("Description", updatedScriptInfo.Description),
                        new XElement("MapModes", updatedScriptInfo.MapModes),
                        new XElement("LastAccess", updatedScriptInfo.LastAccess)
                        ));
                    isUpdated = true;
                    break;
                }
            }

            if (!isUpdated)
            {
                scriptDoc.Element("Scripts").Add(
                    new XElement("ScriptInfo",
                        new XElement("Name", updatedScriptInfo.Name),
                        new XElement("EntryPoint", updatedScriptInfo.EntryPoint),
                        new XElement("ProjectDirectory", updatedScriptInfo.ProjectDirectory),
                        new XElement("Author", updatedScriptInfo.Author),
                        new XElement("Description", updatedScriptInfo.Description),
                        new XElement("MapModes", updatedScriptInfo.MapModes),
                        new XElement("LastAccess", updatedScriptInfo.LastAccess)
                    )
                );
            }

            var sortedScriptInfo = scriptDoc.Element("Scripts")
                .Elements("ScriptInfo")
                .OrderByDescending((s) => s.Element("LastAccess")?.Value ?? DateTime.MinValue.ToString())
                .ToList();

            scriptDoc.Element("Scripts").RemoveNodes();
            foreach (var s in sortedScriptInfo)
            {
                scriptDoc.Element("Scripts").Add(s);
            }
            scriptDoc.Save(ConfigPath);
        }

        public void RemoveScriptInfo(string scriptName)
        {
            if (!File.Exists(ConfigPath)) return;

            var scriptDoc = XDocument.Load(ConfigPath);
            var query = from c in scriptDoc.Elements("Scripts").Elements("ScriptInfo") select c;

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
            var query = from c in scriptDoc.Elements("Scripts").Elements("ScriptInfo") select c;

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
