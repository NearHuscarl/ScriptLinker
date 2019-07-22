using ScriptLinker.Models;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace ScriptLinker.Access
{
    public class ScriptAccess
    {
        public string ConfigPath { get; set; } = Path.Combine(Directory.GetCurrentDirectory(), "script.xml");

        public ScriptInfo LoadScriptInfo(string projectDir, string entryPoint)
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
                    scriptInfo.EntryPoint = scriptInfoElement.Element("EntryPoint").Value.ToString();
                    scriptInfo.ProjectDirectory = scriptInfoElement.Element("ProjectDirectory").Value.ToString();
                    scriptInfo.Author = scriptInfoElement.Element("Author").Value.ToString();
                    scriptInfo.Description = scriptInfoElement.Element("Description").Value.ToString();
                    scriptInfo.MapModes = scriptInfoElement.Element("MapModes").Value.ToString();
                }
            }

            return scriptInfo;
        }

        public void UpdateScriptInfo(ScriptInfo updatedScriptInfo)
        {
            var scriptDoc = File.Exists(ConfigPath) ? XDocument.Load(ConfigPath) : new XDocument();
            var query = from c in scriptDoc.Elements("Script").Elements("ScriptInfo") select c;
            var isUpdated = false;

            foreach (var scriptInfo in query)
            {
                if (scriptInfo.Element("EntryPoint").Value == updatedScriptInfo.EntryPoint
                    && scriptInfo.Element("ProjectDirectory").Value == updatedScriptInfo.ProjectDirectory)
                {
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
    }
}
