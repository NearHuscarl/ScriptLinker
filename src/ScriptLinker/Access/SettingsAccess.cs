using ScriptLinker.Models;
using System;
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
            try
            {
                if (!File.Exists(ConfigPath))
                    return new Settings();

                var doc = XDocument.Load(ConfigPath);
                var settingsElement = doc.Element("Settings");
                var settings = new Settings
                {
                    InitTemplateOnCreated = XmlConvert.ToBoolean(settingsElement.Element("InitTemplateOnCreated").Value),
                    LastOpenedScript = settingsElement.Element("LastOpenedScript").Value,
                    StandaloneScript = XmlConvert.ToBoolean(settingsElement.Element("StandaloneScript").Value),
                    IsLinkedFileWindowExpanded = XmlConvert.ToBoolean(
                        settingsElement.Element("IsLinkedFileWindowExpanded").Value)
                };
                return settings;
            }
            catch (Exception)
            {
                return new Settings();
            }
        }

        public void SaveSettings(Settings settings)
        {
            var settingsDoc = new XDocument(new XElement("Settings",
                new XElement("InitTemplateOnCreated", settings.InitTemplateOnCreated),
                new XElement("LastOpenedScript", settings.LastOpenedScript),
                new XElement("StandaloneScript", settings.StandaloneScript),
                new XElement("IsLinkedFileWindowExpanded", settings.IsLinkedFileWindowExpanded)));

            settingsDoc.Save(ConfigPath);
        }

        public void SaveSettings(string field, string value)
        {
            var settingsDoc = File.Exists(ConfigPath) ? XDocument.Load(ConfigPath) : new XDocument();

            settingsDoc.Element("Settings").Element(field).Value = value;
            settingsDoc.Save(ConfigPath);
        }

        public void SaveSettings(string field, bool value)
        {
            SaveSettings(field, XmlConvert.ToString(value));
        }
    }
}
