using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptLinker.Models
{
    public class ScriptInfo
    {
        public string EntryPoint { get; set; } = "";
        public string ProjectDirectory { get; set; } = "";
        public string Author { get; set; } = "";
        public string Description { get; set; } = "";
        public string MapModes { get; set; } = "";
    }
}
