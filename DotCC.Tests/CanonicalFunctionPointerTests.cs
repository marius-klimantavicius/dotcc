using System;
using System.IO;
using System.Text.RegularExpressions;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class CanonicalFunctionPointerTests
{
    [Fact]
    public void Every_function_designator_reuses_one_typed_static_address()
    {
        var path = Path.Combine(Path.GetTempPath(), "dotcc-canonical-" + Guid.NewGuid().ToString("N") + ".c");
        File.WriteAllText(path, """
            #include <stdlib.h>
            typedef int (*Callback)(int);
            static int add(int x) { return x + 1; }
            static Callback table[] = {add, &add};
            int main(void) {
                Callback local = add;
                void *erased = (void*)&add;
                int (*runtime)(int) = abs;
                return local(41) != 42 || table[0] != local || table[1] != &add
                    || erased != (void*)add || runtime != &abs;
            }
            """);
        try
        {
            var emitted = Compiler.EmitCSharp(new[] { path });
            Regex.IsMatch(emitted, @"public static readonly delegate\*<int, int> add = &\w+\.add;").ShouldBeTrue();
            Regex.IsMatch(emitted, @"public static readonly delegate\*<int, int> abs = &\w+\.abs;").ShouldBeTrue();
            Regex.Matches(emitted, @"&\w+\.add\b").Count.ShouldBe(1);
            Regex.Matches(emitted, @"&\w+\.abs\b").Count.ShouldBe(1);
            emitted.ShouldContain(" = global::Libc;");
            emitted.ShouldContain("DotCcFunctionPointers.add");
            emitted.ShouldContain("DotCcFunctionPointers.abs");
        }
        finally { File.Delete(path); }
    }
}
