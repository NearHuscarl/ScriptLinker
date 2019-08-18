using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

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

        public static void OpenFile(string path)
        {
            if (File.Exists(path))
            {
                Process.Start(path);
            }
            else
            {
                MessageBox.Show($"File not found: {path}", "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        public static void OpenDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                Process.Start(path);
            }
            else
            {
                MessageBox.Show($"Directory not found: {path}", "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}
