using System.IO;

namespace ScriptLinker.Utilities
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
    }
}
