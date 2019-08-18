using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptLinker.Models
{
    public class CSharpFileInfo
    {
        public string Content { get; set; } = "";
        public List<string> UsingNamespaces { get; set; } = new List<string>();
        public string Namespace { get; set; } = "";
        public string ClassName { get; set; } = "";
        public bool IsPartialClass { get; set; } = false;
        /// <summary>
        /// Class that inherits from GameScriptInterface
        /// </summary>
        public bool IsEntryPoint { get; set; } = false;
    }
}
