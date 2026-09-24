using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

public sealed class StaticCompoundArrayTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void Nested_static_literals_survive_calls_and_owners_with_wide_vla_extents(bool objects, bool instances)
    {
        string directory = Path.Combine(Path.GetTempPath(), "dotcc-static-compound-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            string path = Path.Combine(directory, "main.c");
            File.WriteAllText(path, """
                #include <stddef.h>
                #include <time.h>
                typedef unsigned long Row[5];
                struct Parameter { int value; };
                struct Entry { struct Parameter *parameters; int count; };
                struct Entry entries[] = {
                    { .parameters = (struct Parameter[]) { {42}, {7} }, .count = 2 }
                };
                int read_value(void) { return entries[0].parameters[0].value; }
                void change_value(void) { entries[0].parameters[0].value = 99; }
                int exercise(size_t length) {
                    Row rows[length];
                    unsigned char bytes[length + 1];
                    struct tm result;
                    time_t stamp = 0;
                    localtime_r(&stamp, &result);
                    rows[1][4] = 42;
                    bytes[length] = 7;
                    return rows[1][4] == 42 && bytes[length] == 7 &&
                        entries[0].parameters[1].value == 7 && entries[0].count == 2 ? 0 : 1;
                }
                int main(void) { return read_value() == 42 ? exercise(3) : 2; }
                """);
            var options = instances ? new CSharpOutputOptions(Runtime: RuntimeProfile.C, InstanceMethods: true) : null;
            var mode = instances ? EmitMode.ManagedLib : EmitMode.Csproj;
            string emitted;
            if (objects)
            {
                string fragment = path + ".o.cs";
                File.WriteAllText(fragment, Compiler.EmitObject(path, outputOptions: instances ? new(InstanceMethods: true) : null));
                emitted = Compiler.LinkObjects([fragment], emit: mode, className: instances ? "ArrayApi" : null, outputOptions: options);
            }
            else emitted = Compiler.EmitCSharp([path], emit: mode, className: instances ? "ArrayApi" : null, outputOptions: options);
            if (instances) emitted += """

                public static class ArrayProbe {
                    public static int Main() {
                        using var first = new ArrayApi(); using var second = new ArrayApi();
                        using (first.__DotCcEnter()) {
                            if (first.main() != 0) return 10;
                            first.change_value();
                        }
                        System.GC.Collect(); System.GC.WaitForPendingFinalizers();
                        using (second.__DotCcEnter()) if (second.main() != 0) return 11;
                        using (first.__DotCcEnter()) return first.read_value() == 99 && first.exercise(4) == 0 ? 0 : 12;
                    }
                }
                """;
            FixtureRunner.CompileAndRunCapturingExit(emitted, []).exit.ShouldBe(0);
        }
        finally { Directory.Delete(directory, true); }
    }
}
