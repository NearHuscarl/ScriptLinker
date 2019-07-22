using ScriptLinker.Models;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace ScriptLinker.Access
{
    public class SettingsAccess
    {
        public string ConfigPath { get; set; } = Path.Combine(Directory.GetCurrentDirectory(), "settings.xml");

        public Settings LoadSettings()
        {
            if (!File.Exists(ConfigPath))
                return new Settings();

            var doc = XDocument.Load(ConfigPath);
            var settingsElement = doc.Element("Settings");
            var settings = new Settings
            {
                EntryPoint = settingsElement.Element("EntryPoint").Value.ToString(),
                ProjectDirectory = settingsElement.Element("ProjectDirectory").Value.ToString(),
                StandaloneScript = XmlConvert.ToBoolean(settingsElement.Element("StandaloneScript").Value.ToString()),
                IsLinkedFileWindowExpanded = XmlConvert.ToBoolean(
                    settingsElement.Element("IsLinkedFileWindowExpanded").Value.ToString())
            };

            return settings;
        }

        public void SaveSettings(Settings settings)
        {
            var settingsDoc = new XDocument(new XElement("Settings",
                new XElement("EntryPoint", settings.EntryPoint),
                new XElement("ProjectDirectory", settings.ProjectDirectory),
                new XElement("StandaloneScript", settings.StandaloneScript),
                new XElement("IsLinkedFileWindowExpanded", settings.IsLinkedFileWindowExpanded)));

            settingsDoc.Save(ConfigPath);
        }
    }
}
