using System;
using System.IO;
using System.Text.RegularExpressions;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class CanonicalFunctionPointerTests
{
    [Fact]
    public void Macro_generated_helpers_have_addresses_only_when_C_uses_them()
    {
        var dir = Path.Combine(Path.GetTempPath(), "dotcc-macro-pointers-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, "main.c");
        File.WriteAllText(Path.Combine(dir, "helpers.h"), """
            #define MAKE(name) static int name(int x) { return x + 1; }
            #define PASTED(name) MAKE(helper_ ## name)
            MAKE(unused_address)
            MAKE(callback)
            PASTED(pasted)
            #undef MAKE
            """);
        File.WriteAllText(path, """
            #include "helpers.h"
            #define TYPE(x) x
            TYPE(int) ordinary(int x) { return x; }
            int (*get_callback(void))(int) { return callback; }
            int call(void) { return unused_address(1) + helper_pasted(2); }
            """);
        try
        {
            foreach (var link in new[] { false, true })
            {
                string emitted;
                if (link)
                {
                    var obj = Path.Combine(dir, "main.o");
                    File.WriteAllText(obj, Compiler.EmitObject(path));
                    emitted = Compiler.LinkObjects(new[] { obj }, emit: EmitMode.ManagedLib);
                }
                else emitted = Compiler.EmitCSharp(new[] { path }, emit: EmitMode.ManagedLib);
                System.Text.RegularExpressions.Regex.IsMatch(emitted, @" unused_address(?:__unit_[A-F0-9]+)? = &").ShouldBeFalse();
                System.Text.RegularExpressions.Regex.IsMatch(emitted, @" helper_pasted(?:__unit_[A-F0-9]+)? = &").ShouldBeFalse();
                System.Text.RegularExpressions.Regex.IsMatch(emitted, @" callback(?:__unit_[A-F0-9]+)? = &").ShouldBeTrue();
                emitted.ShouldContain(" ordinary = &");
                System.Text.RegularExpressions.Regex.IsMatch(emitted, @"int unused_address(?:__unit_[A-F0-9]+)?\(int x\)").ShouldBeTrue();
            }
        }
        finally { Directory.Delete(dir, true); }
    }

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
            emitted.ShouldContain("DotCcProgramFunctionPointers.add");
            emitted.ShouldContain("DotCcProgramFunctionPointers.abs");
        }
        finally { File.Delete(path); }
    }
}
