using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Loader;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

public sealed partial class ManagedLibraryTests
{
    [Theory]
    [InlineData(false, SourceSplit.None)]
    [InlineData(false, SourceSplit.Function)]
    [InlineData(false, SourceSplit.Size)]
    [InlineData(true, SourceSplit.None)]
    [InlineData(true, SourceSplit.Function)]
    [InlineData(true, SourceSplit.Size)]
    public void Static_deduplication_preserves_native_results_state_callbacks_and_layouts(bool objects, SourceSplit split)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-static-native-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            File.WriteAllText(Path.Combine(directory, "shared.h"), """
                typedef int (*Callback)(int);
                struct Value { int a[2]; };
                static const struct Value value = {{3, 7}};
                static const int offset = 2;
                static int leaf(int x) { struct Value copy = value; return copy.a[0] + copy.a[1] + offset + x; }
                static inline int helper(int x) { return leaf(x); }
                static int recursive(int x) { return x ? recursive(x - 1) : helper(x); }
                static int callback(int x) { return x + 7; }
                static int state(void) { static int counter; return ++counter; }
                static int variant(void) { return VALUE; }
                static int variant_caller(void) { return variant(); }
                """);
            var paths = new[] { "first.c", "second.c", "main.c", "layout_first.c", "layout_second.c" }
                .Select(name => Path.Combine(directory, name)).ToArray();
            File.WriteAllText(paths[0], """
                #define VALUE 11
                #include "shared.h"
                static Callback first_saved = callback;
                static const int first_identity = 3;
                static const int *constant_address(void) { return &first_identity; }
                int first_layout(void);
                int first(void) { return helper(5) + recursive(3) + variant_caller() + first_layout(); }
                int first_state(void) { return state(); }
                Callback first_pointer(void) { return first_saved; }
                const int *first_address(void) { return constant_address(); }
                """);
            File.WriteAllText(paths[1], """
                #define VALUE 17
                #include "shared.h"
                static Callback second_saved = &callback;
                static const int second_identity = 3;
                static const int *constant_address(void) { return &second_identity; }
                int second_layout(void);
                int second(void) { return helper(5) + recursive(3) + variant_caller() + second_layout(); }
                int second_state(void) { return state(); }
                Callback second_pointer(void) { return second_saved; }
                const int *second_address(void) { return constant_address(); }
                """);
            File.WriteAllText(paths[2], """
                #include <stdio.h>
                typedef int (*Callback)(int);
                int first(void); int second(void);
                int first_state(void); int second_state(void);
                Callback first_pointer(void); Callback second_pointer(void);
                const int *first_address(void); const int *second_address(void);
                int probe(void) {
                    if (first() != 45 || second() != 54) return 1;
                    Callback a = first_pointer(), b = second_pointer();
                    if (a == b || a != first_pointer() || b != second_pointer()) return 2;
                    if (a(3) != 10 || b(4) != 11) return 3;
                    if (first_address() == second_address() || *first_address() != 3 || *second_address() != 3) return 4;
                    if (first_state() != 1 || first_state() != 2 || second_state() != 1) return 5;
                    return 0;
                }
                int main(void) { int result = probe(); printf("%d\n", result); return result; }
                """);
            File.WriteAllText(paths[3], """
                struct Private { int value; };
                static int layout(struct Private *p) { return p->value; }
                int first_layout(void) { struct Private p = {5}; return layout(&p); }
                """);
            File.WriteAllText(paths[4], """
                struct Private { long pad; int value; };
                static int layout(struct Private *p) { return p->value; }
                int second_layout(void) { struct Private p = {0, 8}; return layout(&p); }
                """);
            var executable = Path.Combine(directory, OperatingSystem.IsWindows() ? "native.exe" : "native");
            try { StaticNativeProcess("gcc", new[] { "-std=c17", "-O0", "-o", executable }.Concat(paths).ToArray()); }
            catch (Win32Exception) { Assert.Skip("Native static-deduplication control requires gcc on PATH."); }
            var native = StaticNativeProcess(executable, Array.Empty<string>());
            native.ReplaceLineEndings("\n").ShouldBe("0\n");
            if (objects)
                paths = paths.Select(path =>
                {
                    var obj = Path.ChangeExtension(path, ".o");
                    File.WriteAllText(obj, Compiler.EmitObject(path)); return obj;
                }).ToArray();
            var options = new CSharpOutputOptions(DeduplicateStatic: true, Runtime: RuntimeProfile.C);
            var files = objects
                ? Compiler.LinkObjectFiles(paths, emit: EmitMode.ManagedLib, className: "Api", split: split, splitSize: 256, outputOptions: options)
                : Compiler.EmitCSharpFiles(paths, emit: EmitMode.ManagedLib, className: "Api", split: split, splitSize: 256, outputOptions: options);
            var source = string.Join('\n', files.Values);
            int Count(string name) => Regex.Matches(source, @"static unsafe \w+\** " + name + @"(?:__\w+)?\(").Count;
            foreach (var name in new[] { "leaf", "helper", "recursive" }) Count(name).ShouldBe(1, name);
            foreach (var name in new[] { "callback", "state", "variant", "variant_caller", "constant_address", "layout" }) Count(name).ShouldBe(2, name);
            var compilation = CSharpCompilation.Create("StaticLibrary_" + Guid.NewGuid().ToString("N"),
                files.Select(file => ParseSource(file.Value, path: file.Key)), RuntimeReferences(),
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));
            using var image = new MemoryStream();
            var result = compilation.Emit(image, cancellationToken: TestContext.Current.CancellationToken);
            result.Success.ShouldBeTrue(string.Join('\n', result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error)));
            image.Position = 0;
            var assembly = new AssemblyLoadContext("static-library-" + Guid.NewGuid(), isCollectible: false).LoadFromStream(image);
            var managed = assembly.GetType("Api", true)!.GetMethod("probe")!.Invoke(null, null);
            (managed + "\n").ShouldBe(native.ReplaceLineEndings("\n"));
        }
        finally { Directory.Delete(directory, true); }
    }

    private static string StaticNativeProcess(string executable, string[] arguments)
    {
        var start = new ProcessStartInfo(executable) { RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false };
        foreach (var argument in arguments) start.ArgumentList.Add(argument);
        using var process = Process.Start(start)!;
        var stdout = process.StandardOutput.ReadToEndAsync(TestContext.Current.CancellationToken);
        var stderr = process.StandardError.ReadToEndAsync(TestContext.Current.CancellationToken);
        if (!process.WaitForExit(30_000))
        {
            process.Kill(entireProcessTree: true);
            process.WaitForExit();
            throw new TimeoutException("Native static-deduplication control exceeded 30 seconds: " + executable);
        }
        var output = stdout.GetAwaiter().GetResult();
        process.ExitCode.ShouldBe(0, stderr.GetAwaiter().GetResult());
        return output;
    }
}
