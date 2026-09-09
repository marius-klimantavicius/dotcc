using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

public sealed class InlineAggregateInitializationTests
{
    [Fact]
    public void Separate_objects_keep_distinct_array_initializer_factory_shapes()
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-inline-init-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var sources = new[]
            {
                "typedef int (*Callback)(int); struct Counts { int values[2]; int marker; Callback callbacks[2]; }; static struct Counts first_value = {{10}}; int first(void) { return first_value.values[0] + first_value.values[1] + first_value.marker; }",
                "typedef int (*Callback)(int); struct Counts { int values[2]; int marker; Callback callbacks[2]; }; static int second_callback(int value) { return value + 1; } static struct Counts second_value = {{20,30},7,{second_callback}}; int second(void) { return second_value.callbacks[0](second_value.values[0] + second_value.values[1] + second_value.marker); }",
                "#include <stdio.h>\nint first(void); int second(void); int main(void) { printf(\"%d\\n\", first() + second()); return 0; }"
            };
            var objects = new string[sources.Length];
            for (var i = 0; i < sources.Length; ++i)
            {
                var path = Path.Combine(directory, "unit" + i + ".c");
                File.WriteAllText(path, sources[i]);
                objects[i] = Path.ChangeExtension(path, ".cs");
                var fragment = Compiler.EmitObject(path);
                // A second emission must choose exactly the same helper names.
                Compiler.EmitObject(path).ShouldBe(fragment);
                File.WriteAllText(objects[i], fragment);
            }
            var linked = Compiler.LinkObjects(objects);
            FixtureRunner.CompileAndRun(linked, Array.Empty<string>()).ShouldBe("68\n");
        }
        finally { Directory.Delete(directory, recursive: true); }
    }
}
