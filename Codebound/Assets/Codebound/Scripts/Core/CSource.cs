using System.Text.RegularExpressions;

namespace Codebound.Core
{
    /// <summary>
    /// Checks C/C++ source before it reaches the compiler. The compiled program runs in WebAssembly and cannot touch
    /// files, but the compiler itself runs on the player's PC and would happily #include any file it is pointed at
    /// (and print it in an error message). So: only &lt;system/headers.h&gt; may be included, and every other way for
    /// the compiler to read a file (__has_include probes, #embed, .incbin in inline assembly) is refused.
    /// </summary>
    public static class CSource
    {
        // any include-like directive anywhere on a line, even after a comment: "/* x */ #include", "%:include", "??=include"
        static readonly Regex IncludeLike = new Regex(@"(#|%:|\?\?=)\s*(/\*.*?\*/\s*)*(include_next|include|import|embed)\b", RegexOptions.Compiled);
        static readonly Regex SafeInclude = new Regex(@"^\s*#\s*include\s*<([A-Za-z0-9_+\-./]+)>\s*(//.*)?$", RegexOptions.Compiled);
        static readonly Regex Forbidden = new Regex(@"__has_include|__has_embed|\bincbin\b|\b(asm|__asm|__asm__)\b", RegexOptions.Compiled);

        /// <summary>Null when the source may be compiled, otherwise a message with the line number.</summary>
        public static string Check(string code)
        {
            var lines = (code ?? "").Replace("\r\n", "\n").Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                // a backslash at the end of a line continues it: check the joined logical line
                string line = lines[i];
                int first = i;
                while (line.EndsWith("\\") && i + 1 < lines.Length) line = line.Substring(0, line.Length - 1) + lines[++i];

                var bad = Forbidden.Match(line);
                if (bad.Success) return $"line {first + 1}: '{bad.Value}' is not allowed on Codebound computers";
                if (!IncludeLike.IsMatch(line)) continue;
                var m = SafeInclude.Match(line);
                if (!m.Success || m.Groups[1].Value.Contains("..") || m.Groups[1].Value.StartsWith("/"))
                    return $"line {first + 1}: only system headers can be included, like #include <stdio.h> or #include <codebound.h>";
            }
            return null;
        }
    }
}
