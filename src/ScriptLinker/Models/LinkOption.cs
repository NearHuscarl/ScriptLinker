using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptLinker.Models
{
    public class LinkOption
    {
        public bool Minified = false;

        public static readonly LinkOption Default = new LinkOption();
    }
}
