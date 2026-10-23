using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class ObjectDuplicateDefinitionTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Explicit_C_inclusion_and_the_same_separate_core_object_report_both_origins(bool prune)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-duplicate-tu-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var core = Path.Combine(directory, "core.c");
            var test = Path.Combine(directory, "test.c");
            File.WriteAllText(core, "int shared(int value) { return value + 3; }");
            File.WriteAllText(test, "#include \"core.c\"\nint main(void) { return shared(1); }");
            var objects = new[] { Path.Combine(directory, "core.obj.cs"), Path.Combine(directory, "test.obj.cs") };
            File.WriteAllText(objects[0], Compiler.EmitObject(core));
            File.WriteAllText(objects[1], Compiler.EmitObject(test, includeDirs: new[] { directory }));
            var error = Should.Throw<CompileException>(() => Compiler.LinkObjects(objects,
                outputOptions: new(DeduplicateInline: true, PruneUnusedInline: prune)));
            error.Message.ShouldContain("duplicate definition of function 'shared'");
            error.Message.ShouldContain(objects[0]); error.Message.ShouldContain(objects[1]);
        }
        finally { Directory.Delete(directory, true); }
    }
}
