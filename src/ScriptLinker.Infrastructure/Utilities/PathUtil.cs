using System;
using System.IO;

namespace ScriptLinker.Infrastructure.Utilities
{
    public class PathUtil
    {
        public static bool IsRoot(string path)
        {
            return Path.GetPathRoot(path) == path;
        }

        /// <summary>
        /// "C:\foo\bar" => "foo\bar"
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetPathWithoutDrive(string path)
        {
            return path.Substring(Path.GetPathRoot(path).Length);
        }

        // thank you Jon Skeet
        public static string GetRelativePath(string path, string directoryPath)
        {
            if (!path.StartsWith(directoryPath))
            {
                throw new Exception("Unable to make relative path");
            }
            else
            {
                // The +1 is to avoid the directory separator
                return path.Substring(directoryPath.Length + 1);
            }
        }
    }
}
