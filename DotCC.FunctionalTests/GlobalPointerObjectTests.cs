using System;
using System.IO;
using System.Linq;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

public sealed class GlobalPointerObjectTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void Cross_unit_pointer_and_callback_slots_have_a_stable_addressable_storage_contract(bool objects, bool instances)
    {
        string directory = Path.Combine(Path.GetTempPath(), "dotcc-pointer-object-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            string[] sources = [
                """
                int values[2] = { 20, 40 };
                int *pointer = values;
                int increment(int x) { return x + 1; }
                int (*callback)(int) = increment;
                int observe(void) { return *pointer + callback(1); }
                """,
                """
                extern int values[2];
                extern int *pointer;
                extern int (*callback)(int);
                int observe(void);
                int doubled(int x) { return x * 2; }
                int main(void) {
                    int **address = &pointer;
                    int (**callback_address)(int) = &callback;
                    if (observe() != 22 || *address != values) return 1;
                    if (pointer++ != values || pointer != values + 1) return 6;
                    if (--pointer != values || pointer != values) return 7;
                    ++pointer;
                    pointer--;
                    int steps = 0;
                    for (; pointer != values + 2; pointer++) steps++;
                    if (steps != 2 || pointer != values + 2) return 8;
                    pointer = values + 1;
                    callback = doubled;
                    if (observe() != 42 || (*callback_address)(7) != 14) return 2;
                    *address = values;
                    *callback_address = doubled;
                    return observe() == 22 && pointer == values ? 0 : 3;
                }
                """];
            var paths = sources.Select((source, index) =>
            {
                string path = Path.Combine(directory, "unit" + index + ".c");
                File.WriteAllText(path, source);
                return path;
            }).ToArray();
            var options = instances ? new CSharpOutputOptions(Runtime: RuntimeProfile.C, InstanceMethods: true) : null;
            var mode = instances ? EmitMode.ManagedLib : EmitMode.Csproj;
            string emitted;
            if (objects)
            {
                var fragments = paths.Select(path =>
                {
                    string fragment = path + ".o.cs";
                    File.WriteAllText(fragment, Compiler.EmitObject(path,
                        outputOptions: instances ? new(InstanceMethods: true) : null));
                    return fragment;
                }).ToArray();
                emitted = Compiler.LinkObjects(fragments, emit: mode, className: instances ? "PointerApi" : null, outputOptions: options);
            }
            else emitted = Compiler.EmitCSharp(paths, emit: mode, className: instances ? "PointerApi" : null, outputOptions: options);
            if (instances) emitted += """

                public static class PointerProbe {
                    public static int Main() {
                        using var first = new PointerApi(); using var second = new PointerApi();
                        return first.main() == 0 && second.observe() == 22 && second.main() == 0 ? 0 : 5;
                    }
                }
                """;
            FixtureRunner.CompileAndRunCapturingExit(emitted, []).exit.ShouldBe(0);
        }
        finally { Directory.Delete(directory, true); }
    }
}
