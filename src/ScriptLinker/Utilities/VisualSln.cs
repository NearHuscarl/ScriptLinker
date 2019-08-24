using OpenMcdf;
using ScriptLinker.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ScriptLinker.Utilities
{
    public class VisualSln
    {
        private string Path { get; set; }

        private string _suoPath;
        public string SuoPath
        {
            get
            {
                if (_suoPath != null) return _suoPath;

                var directory = new DirectoryInfo(Path);

                _suoPath = directory.EnumerateFiles(".suo", SearchOption.AllDirectories)
                    .Select(f => f.FullName)
                    .FirstOrDefault();

                return _suoPath;
            }
        }
        private Decoder _uniDecoder;

        public VisualSln(string slnPath)
        {
            if (!IsValidSolutionPath(slnPath))
            {
                throw new ArgumentException("slnPath must contain at least one solution file (*.sln)");
            }

            Path = slnPath;
            _uniDecoder = Encoding.Unicode.GetDecoder();
        }

        public static bool IsValidSolutionPath(string path)
        {
            return new DirectoryInfo(path).GetFiles("*.sln").Length > 0;
        }

        private class BreakpointBuilder
        {
            private Breakpoint _breakpoint = new Breakpoint();
            private StringBuilder sb = new StringBuilder();
            private int filePathcharOffset = 0;
            private bool shouldBuild = false;
            private char lastChar;

            public void Add(char chr)
            {
                if (Complete()) return;

                if (lastChar == '\0' && TextUtil.IsAlphabetical(chr))
                {
                    shouldBuild = true;
                }

                if (shouldBuild && TextUtil.IsPrintable(chr))
                {
                    sb.Append(chr);
                }

                if (chr == '\0')
                {
                    var str = sb.ToString();

                    if (str.Length >= 3) // Valid property like File path or Symbol
                    {
                        if (_breakpoint.File == "")
                        {
                            _breakpoint.File = str;
                        }
                        else
                        {
                            _breakpoint.Symbol = str;
                        }

                        shouldBuild = false;
                        sb.Clear();
                    }
                    else // other chars describing other breakpoint properties that happen to be in the printable range
                    {
                        sb.Clear();
                    }
                }

                if (_breakpoint.File != "")
                {
                    filePathcharOffset++;
                }
                if (filePathcharOffset == 2)
                {
                    _breakpoint.LineNumber = chr;
                }
                lastChar = chr;
            }

            public Breakpoint GetBreakpoint()
            {
                return _breakpoint;
            }

            public bool Complete()
            {
                return _breakpoint.File != ""
                    && _breakpoint.Symbol != ""
                    && _breakpoint.LineNumber != -1;
            }
        }

        public IEnumerable<Breakpoint> GetBreakpoints()
        {
            if (SuoPath == null)
            {
                return new List<Breakpoint>();
            }

            var suoFile = new CompoundFile(SuoPath);
            var foundStream = suoFile.RootStorage.GetStream("DebuggerBreakpoints");
            byte[] bytes = foundStream.GetData();
            char[] chars;

            var charCount = _uniDecoder.GetCharCount(bytes, 0, bytes.Length);
            chars = new char[charCount];

            var charsDecodedCount = _uniDecoder.GetChars(bytes, 0, bytes.Length, chars, 0);
            var loopOverSlnPath = false;
            var breakpoints = new List<Breakpoint>();
            var lastChar = '\0';

            BreakpointBuilder bb = null;
            foreach (var chr in chars)
            {
                if (lastChar == '\\' && !TextUtil.IsPrintable(chr) && !loopOverSlnPath)
                {
                    loopOverSlnPath = true;
                }

                // the first string in DebuggerBreakpoints stream is the solution path which is not needed here
                if (loopOverSlnPath)
                {
                    if (bb == null)
                    {
                        bb = new BreakpointBuilder();
                    }
                    if (bb != null)
                    {
                        if (!bb.Complete())
                        {
                            bb.Add(chr);
                        }

                        if (bb.Complete())
                        {
                            breakpoints.Add(bb.GetBreakpoint());
                            bb = null;
                        }
                    }
                }
                
                lastChar = chr;
            }

            suoFile.Close();

            return breakpoints;
        }
    }
}
