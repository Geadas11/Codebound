using Codebound.Core;
using NUnit.Framework;

namespace Codebound.Tests
{
    public class CSourceTests
    {
        [TestCase("#include <stdio.h>\n#include <codebound.h>\nint main(void) { puts(\"hi\"); }\n")]
        [TestCase("#include <vector>\n#include <sys/types.h>  // ok\n#define X 1\n#if X\n#endif\nint main() {}\n")]
        [TestCase("// the word include in a comment is fine\nconst char *s = \"#define\";\nint main(void) { return 0; }\n")]
        public void Allows_SystemHeadersAndOrdinaryCode(string code) => Assert.IsNull(CSource.Check(code));

        [TestCase("#include \"C:/Users/me/secret.txt\"\n", 1)]
        [TestCase("int x;\n#include </etc/passwd>\n", 2)]
        [TestCase("#include <../../../../Windows/win.ini>\n", 1)]
        [TestCase("#define F \"x.txt\"\n#include F\n", 2)]
        [TestCase("/* hidden */ #include \"x\"\n", 1)]
        [TestCase("%:include \"x\"\n", 1)]
        [TestCase("??=include \"x\"\n", 1)]
        [TestCase("#  include_next <stdio.h>\n", 1)]
        [TestCase("#embed \"x.bin\"\n", 1)]
        [TestCase("#inc\\\nlude \"x\"\n", 1)]
        [TestCase("#if __has_include(\"C:/x\")\n#endif\n", 1)]
        [TestCase("int main(void) { __asm__(\".incbin \\\"x\\\"\"); }\n", 1)]
        public void Refuses_AnyWayToReadFiles(string code, int line)
        {
            var err = CSource.Check(code);
            Assert.IsNotNull(err, code);
            StringAssert.StartsWith($"line {line}:", err);
        }
    }
}
