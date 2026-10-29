using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

public sealed class BoundedTargetNameExecutionTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void Long_names_keep_types_fields_callbacks_globals_and_distinct_functions_bound(bool objects, bool privateFunctions)
    {
        var suffix = new string('x', 4096);
        var tag = "tag_" + suffix;
        var field = "field_" + suffix;
        var enumeration = "enumeration_" + suffix;
        var enumerator = "enumerator_" + suffix;
        var function = "function_" + suffix;
        var global = "global_" + suffix;
        var callback = "callback_" + suffix;
        var local = "local_" + suffix;
        var storage = privateFunctions ? "static " : "";
        var source = $$"""
            #include <stdio.h>
            struct {{tag}} { int {{field}}; };
            enum {{enumeration}} { {{enumerator}} = 41 };
            {{storage}}int {{function}}A(struct {{tag}} *p) { return p->{{field}}; }
            {{storage}}int {{function}}B(struct {{tag}} *p) {
                int {{local}} = 1;
                return {{function}}A(p) + {{local}};
            }
            static struct {{tag}} {{global}} = { {{enumerator}} };
            int main(void) {
                int (*{{callback}})(struct {{tag}} *) = {{function}}B;
                enum {{enumeration}} value = {{enumerator}};
                int result = {{callback}}(&{{global}});
                printf("long names %d %d\n", result, {{function}}A(&{{global}}));
                return result != 42 || {{function}}A(&{{global}}) != 41 || (int)value != 41;
            }
            """;
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-bounded-names-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var path = Path.Combine(directory, "main.c");
            File.WriteAllText(path, source);
            string emitted;
            if (objects)
            {
                var fragment = Path.Combine(directory, "main.cs");
                var text = Compiler.EmitObject(path);
                // Raw semantic identities remain in the object contract.
                text.ShouldContain(tag);
                File.WriteAllText(fragment, text);
                emitted = Compiler.LinkObjects(new[] { fragment });
            }
            else emitted = Compiler.EmitCSharp(new[] { path });
            var result = FixtureRunner.CompileAndRunCapturingExit(emitted, Array.Empty<string>());
            result.exit.ShouldBe(0);
            result.stdout.Trim().ShouldBe("long names 42 41");
        }
        finally { Directory.Delete(directory, recursive: true); }
    }
}
