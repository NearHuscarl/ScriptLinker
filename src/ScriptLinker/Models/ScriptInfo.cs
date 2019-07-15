using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptLinker.Models
{
    public class ScriptInfo
    {
        public string Author { get; set; } = "";
        public string Description { get; set; } = "";
        public string MapModes { get; set; } = "";

        public ScriptInfo(string author, string description, string mapModes)
        {
            Author = author;
            Description = description;
            MapModes = mapModes;
        }

        public static ScriptInfo Empty
        {
            get { return new ScriptInfo("", "", ""); }
        }

        public bool IsEmpty
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
