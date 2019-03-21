using ScriptImporter.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace ScriptImporter.Utilities
{
    public static class FileUtil
    {
        public static IEnumerable<string> GetScriptFiles(string directory)
        {
            foreach (string file in Directory.EnumerateFiles(directory, "*.cs", SearchOption.AllDirectories))
            {
                if (Regex.Match(file, @"(\\obj\\|\\Properties\\)").Success)
                    continue;

                yield return file;
            }
        }

        public static string GetNamespace(string filePath)
        {
            using (var file = File.OpenText(filePath))
            {
                var line = "";

                while ((line = file.ReadLine()) != null)
                {
                    var match = RegexPattern.Namespace.Match(line);

                    if (match.Success)
                    {
                        return match.Groups[1].Value;
                    }
                }
            }

            return "";
        }

        public static ScriptInfo ReadOutputScriptInfo(string outputPath)
        {
            var scriptInfo = new ScriptInfo();

            if (!File.Exists(outputPath))
                return scriptInfo;

            using (var file = File.OpenText(outputPath))
            {
                var line = "";
                var readLastInput = false;

                while ((line = file.ReadLine()) != null)
                {
                    if (readLastInput) break;
                    Match match;

                    match = Regex.Match(line, @"^\* author: (.*)");
                    if (match.Success)
                    {
                        scriptInfo.Author = match.Groups[1].Value;
                        continue;
                    }

                    match = Regex.Match(line, @"^\* description: (.*)");
                    if (match.Success)
                    {
                        scriptInfo.Description = match.Groups[1].Value;
                        continue;
                    }

                    match = Regex.Match(line, @"^\* mapmodes: (.*)");
                    if (match.Success)
                    {
                        scriptInfo.MapModes = match.Groups[1].Value;
                        readLastInput = true;
                    }
                }
            }

            return scriptInfo;
        }
    }
}
