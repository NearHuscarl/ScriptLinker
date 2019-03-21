using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptImporter.Models
{
    public class CSharpFileInfo
    {
        public string Content { get; set; } = "";
        public List<string> UsingNamespaces { get; set; }
        public string Namespace { get; set; }
    }
}
