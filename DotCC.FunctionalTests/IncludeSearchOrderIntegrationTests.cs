using System;
using System.IO;
using System.Linq;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

public sealed class IncludeSearchOrderIntegrationTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Replacement_header_is_shared_across_units_instead_of_local_library_header(bool objects)
    {
        var root = Path.Combine(Path.GetTempPath(), "dotcc-include-abi-" + Guid.NewGuid().ToString("N"));
        var library = Path.Combine(root, "library");
        var include = Path.Combine(root, "include");
        Directory.CreateDirectory(library);
        Directory.CreateDirectory(include);
        try
        {
            File.WriteAllText(Path.Combine(library, "value.h"), "#define LOCAL 7\n");
            File.WriteAllText(Path.Combine(include, "value.h"),
                "struct Shared { int first; long second; }; struct Shared make(void);\n");
            var first = Path.Combine(library, "caller.c");
            var second = Path.Combine(root, "provider.c");
            File.WriteAllText(first, "#include \"value.h\"\n#include <value.h>\n" +
                "int main(void) { struct Shared s=make(); return s.first+s.second+LOCAL==42 ? 0 : 1; }\n");
            File.WriteAllText(second, "#include <value.h>\n" +
                "struct Shared make(void) { struct Shared s={12,23}; return s; }\n");
            string emitted;
            if (objects)
            {
                var paths = new[] { first, second }.Select(path =>
                {
                    var output = path + ".obj.cs";
                    File.WriteAllText(output, Compiler.EmitObject(path, includeDirs: [include]));
                    return output;
                }).ToArray();
                emitted = Compiler.LinkObjects(paths);
            }
            else emitted = Compiler.EmitCSharp([first, second], includeDirs: [include]);
            FixtureRunner.CompileAndRunCapturingExit(emitted, Array.Empty<string>()).exit.ShouldBe(0);
        }
        finally { Directory.Delete(root, true); }
    }
}
