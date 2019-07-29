using System.IO;

namespace ScriptLinker.Utilities
{
    public class PathUtil
    {
        public static bool IsRoot(string path)
        {
            return Path.GetPathRoot(path) == path;
        }
    }
}
