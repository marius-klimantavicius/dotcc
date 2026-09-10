using System;
using System.IO;
using System.Runtime.Loader;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

public sealed partial class ManagedLibraryTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Canonical_runtime_name_binding_uses_the_complete_link(bool translatedDefinition)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-runtime-identity-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var first = Path.Combine(directory, "first.c");
        var second = Path.Combine(directory, "second.c");
        File.WriteAllText(first, """
            #include <stdlib.h>
            typedef int (*Callback)(int);
            Callback get_abs(void) { return abs; }
            int evaluate(void) { return get_abs()(-42); }
            """);
        File.WriteAllText(second, translatedDefinition ? "int abs(int value) { return value + 42; }" : "int unrelated(void) { return 7; }");
        try
        {
            var a = Path.ChangeExtension(first, ".cs");
            var b = Path.ChangeExtension(second, ".cs");
            File.WriteAllText(a, Compiler.EmitObject(first));
            File.WriteAllText(b, Compiler.EmitObject(second));
            var emitted = Compiler.LinkObjects(new[] { a, b }, emit: EmitMode.ManagedLib);
            var library = Compile("RuntimeIdentity_" + Guid.NewGuid().ToString("N"), emitted, RuntimeReferences());
            var context = new AssemblyLoadContext("dotcc-runtime-identity-" + Guid.NewGuid().ToString("N"), isCollectible: false);
            var assembly = context.LoadFromStream(new MemoryStream(library));
            assembly.GetType("DotCcLib", true)!.GetMethod("evaluate")!.Invoke(null, null).ShouldBe(translatedDefinition ? 0 : 42);
        }
        finally { Directory.Delete(directory, recursive: true); }
    }
}
