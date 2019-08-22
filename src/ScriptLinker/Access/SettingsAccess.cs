using ScriptLinker.Models;
using ScriptLinker.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace ScriptLinker.Access
{
    public class SettingsAccess
    {
        public string ConfigPath { get; set; } = Path.Combine(Directory.GetCurrentDirectory(), "settings.xml");

        public Settings LoadSettings()
        {
            try
            {
                var settings = new Settings();

                using (FileStream stream = new FileStream(ConfigPath, FileMode.Open))
                {
                    var serializer = new XmlSerializer(typeof(Settings));
                    settings = serializer.Deserialize(stream) as Settings;
                }

                return settings;
            }
            catch (Exception)
            {
                return new Settings();
            }
        }

        public void SaveSettings(Settings settings)
        {
            using (FileStream stream = new FileStream(ConfigPath, FileMode.Create))
            {
                var serializer = new XmlSerializer(typeof(Settings));
                serializer.Serialize(stream, settings);
            }
        }
    }
}
