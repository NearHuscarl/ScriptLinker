using ScriptLinker.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ScriptLinker.Utilities
{
    public static class FileUtil
    {
        public static IEnumerable<string> GetScriptFiles(string directory)
        {
            if (!Directory.Exists(directory)) yield break;

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

            const int bufferSize = 128;
            using (var fileStream = File.OpenRead(outputPath))
            using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, bufferSize))
            {
                var line = "";
                var readLastInput = false;

                while ((line = streamReader.ReadLine()) != null)
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

        public static async Task CopyFileAsync(string sourcePath, string destinationPath)
        {
            using (var sourceStream = new FileStream(sourcePath,
                FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan))
            {
                using (var destinationStream = new FileStream(destinationPath,
                    FileMode.Create, FileAccess.Write, FileShare.None, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan))
                {
                    await sourceStream.CopyToAsync(destinationStream);
                }
            }
        }

        public static async Task WriteTextAsync(string filePath, string text)
        {
            // it has to be UTF8 encoding or the /startscript <script-file> will throw error
            byte[] encodedText = Encoding.UTF8.GetBytes(text);

            using (var sourceStream = new FileStream(filePath,
                FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
            {
                await sourceStream.WriteAsync(encodedText, 0, encodedText.Length);
            };
        }
    }
}
