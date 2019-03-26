using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptImporter.Models
{
    public class ScriptInfo
    {
        public string Author { get; set; } = "";
        public string Description { get; set; } = "";
        public string MapModes { get; set; } = "";

        public bool Empty
        {
            get {
                return
                  string.IsNullOrWhiteSpace(Author) &&
                  string.IsNullOrWhiteSpace(Description) &&
                  string.IsNullOrWhiteSpace(MapModes);
            }
        }
    }
}
