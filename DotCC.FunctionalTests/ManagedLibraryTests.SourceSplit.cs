using System;
using System.IO;
using System.Linq;
using System.Runtime.Loader;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

public sealed partial class ManagedLibraryTests
{
    private const string SplitSource = """
        #include <stdlib.h>
        typedef int (*Callback)(int);
        int add(int x) { static int calls; ++calls; return x + calls; }
        Callback current = add;
        int values[] = { 3, 7 };
        Callback get(void) { return &add; }
        const char *text(void) { return "braces { } and // comments é"; }
        int run(int x) { return current(x) + values[1]; }
        int main(void) { return run(34) == 42 ? 0 : 1; }
        """;

    [Theory]
    [InlineData(false, SourceSplit.None, EmitMode.ManagedLib)]
    [InlineData(false, SourceSplit.Function, EmitMode.ManagedLib)]
    [InlineData(true, SourceSplit.Function, EmitMode.ManagedLib)]
    [InlineData(false, SourceSplit.Size, EmitMode.ManagedLib)]
    [InlineData(true, SourceSplit.Size, EmitMode.ManagedLib)]
    [InlineData(false, SourceSplit.Function, EmitMode.Csproj)]
    [InlineData(true, SourceSplit.Size, EmitMode.Csproj)]
    [InlineData(false, SourceSplit.Size, EmitMode.SharedLib)]
    public void Split_sources_compile_and_preserve_cross_file_state_and_callbacks(bool link, SourceSplit split, EmitMode mode)
    {
        var path = Path.Combine(Path.GetTempPath(), "dotcc-split-" + Guid.NewGuid().ToString("N") + ".c");
        var obj = Path.ChangeExtension(path, ".cs");
        try
        {
            File.WriteAllText(path, SplitSource);
            var name = mode == EmitMode.Csproj ? null : "class";
            if (link) File.WriteAllText(obj, Compiler.EmitObject(path));
            var files = link
                ? Compiler.LinkObjectFiles(new[] { obj }, emit: mode, className: name, split: split, splitSize: 1100)
                : Compiler.EmitCSharpFiles(new[] { path }, emit: mode, className: name, split: split, splitSize: 1100);
            if (split == SourceSplit.None)
            {
                files.Count.ShouldBe(1);
                files["Program.cs"].ShouldBe(Compiler.EmitCSharp(new[] { path }, emit: mode, className: name));
            }
            else
            {
                files["Program.cs"].ShouldContain("partial class");
                var functionFiles = files.Where(f => f.Key.StartsWith("Dotcc.Functions.")).ToArray();
                functionFiles.Sum(f => CSharpSyntaxTree.ParseText(f.Value).GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Count()).ShouldBe(5);
                if (split == SourceSplit.Function) functionFiles.Length.ShouldBe(5);
                foreach (var file in functionFiles)
                {
                    file.Value.ShouldNotContain("static int calls");
                    var methods = CSharpSyntaxTree.ParseText(file.Value, cancellationToken: TestContext.Current.CancellationToken).GetRoot(TestContext.Current.CancellationToken).DescendantNodes().OfType<MethodDeclarationSyntax>().ToArray();
                    if (split == SourceSplit.Function) methods.Length.ShouldBe(1);
                    else if (file.Key != functionFiles.Last().Key)
                    {
                        Encoding.UTF8.GetByteCount(file.Value).ShouldBeGreaterThan(1100);
                        Encoding.UTF8.GetByteCount(file.Value.Replace(methods.Last().ToFullString(), "")).ShouldBeLessThanOrEqualTo(1100);
                    }
                }
            }
            var owner = mode == EmitMode.Csproj ? "DotCcProgram" : "@class";
            var consumer = $$"""
                public static unsafe class SplitConsumer {
                    public static int Run() {
                        if ({{owner}}.get() != DotCcFunctionPointers.add) return -1;
                        if (System.Runtime.InteropServices.Marshal.PtrToStringUTF8((nint){{owner}}.text()) != "braces { } and // comments é") return -2;
                        return {{owner}}.run(34) + {{owner}}.run(33);
                    }
                }
                """;
            var trees = files.Select(f => CSharpSyntaxTree.ParseText(f.Value, path: f.Key)).Append(CSharpSyntaxTree.ParseText(consumer, cancellationToken: TestContext.Current.CancellationToken));
            var compilation = CSharpCompilation.Create("Split_" + Guid.NewGuid().ToString("N"), trees, RuntimeReferences(),
                new CSharpCompilationOptions(mode == EmitMode.Csproj ? OutputKind.ConsoleApplication : OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));
            using var stream = new MemoryStream();
            var result = compilation.Emit(stream, cancellationToken: TestContext.Current.CancellationToken);
            result.Success.ShouldBeTrue(string.Join("\n", result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error)));
            stream.Position = 0;
            var context = new AssemblyLoadContext("split-" + Guid.NewGuid().ToString("N"), isCollectible: false);
            context.LoadFromStream(stream).GetType("SplitConsumer")!.GetMethod("Run")!.Invoke(null, null).ShouldBe(84);
        }
        finally { File.Delete(path); File.Delete(obj); }
    }

    [Fact]
    public void Size_split_closes_after_first_crossing_and_is_deterministic()
    {
        var path = Path.Combine(Path.GetTempPath(), "dotcc-split-size-" + Guid.NewGuid().ToString("N") + ".c");
        try
        {
            File.WriteAllText(path, "int first(void) { return 1; } int second(void) { return 2; } int third(void) { return 3; }");
            var individual = Compiler.EmitCSharpFiles(new[] { path }, emit: EmitMode.ManagedLib, split: SourceSplit.Function);
            int target = Encoding.UTF8.GetByteCount(individual.First(f => f.Key.StartsWith("Dotcc.Functions.")).Value);
            var grouped = Compiler.EmitCSharpFiles(new[] { path }, emit: EmitMode.ManagedLib, split: SourceSplit.Size, splitSize: target);
            var first = grouped.First(f => f.Key.StartsWith("Dotcc.Functions.")).Value;
            first.ShouldContain("first()"); first.ShouldContain("second()"); first.ShouldNotContain("third()");
            grouped.ShouldBe(Compiler.EmitCSharpFiles(new[] { path }, emit: EmitMode.ManagedLib, split: SourceSplit.Size, splitSize: target));
            var tiny = Compiler.EmitCSharpFiles(new[] { path }, emit: EmitMode.ManagedLib, split: SourceSplit.Size, splitSize: 1);
            tiny.Keys.Count(k => k.StartsWith("Dotcc.Functions.")).ShouldBe(3);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void Regeneration_removes_only_previous_generated_files_and_can_return_to_single_file()
    {
        var root = Path.Combine(Path.GetTempPath(), "dotcc-split-clean-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            var path = Path.Combine(root, "input.c");
            File.WriteAllText(path, SplitSource);
            var output = Path.Combine(root, "output");
            var split = Compiler.EmitCSharpFiles(new[] { path }, emit: EmitMode.ManagedLib, split: SourceSplit.Function);
            Compiler.WriteCSharpFiles(output, split);
            var userFile = Path.Combine(output, "Custom.cs");
            File.WriteAllText(userFile, "// user sidecar");
            Compiler.WriteCSharpFiles(output, Compiler.EmitCSharpFiles(new[] { path }, emit: EmitMode.ManagedLib));
            Directory.GetFiles(output, "Dotcc.*.g.cs").ShouldBeEmpty();
            File.ReadAllText(userFile).ShouldBe("// user sidecar");
            File.WriteAllText(Path.Combine(output, "Dotcc.SourceFiles.txt"), "../input.c\n");
            Should.Throw<IOException>(() => Compiler.WriteCSharpFiles(output, split));
            File.ReadAllText(path).ShouldBe(SplitSource);
        }
        finally { Directory.Delete(root, true); }
    }

    [Fact]
    public void Old_objects_can_still_link_but_must_be_regenerated_to_split()
    {
        var path = Path.Combine(Path.GetTempPath(), "dotcc-old-split-" + Guid.NewGuid().ToString("N") + ".c");
        var obj = Path.ChangeExtension(path, ".cs");
        try
        {
            File.WriteAllText(path, SplitSource);
            File.WriteAllLines(obj, Compiler.EmitObject(path).Split('\n').Where(line => !line.StartsWith("//!!dotcc-obj function:")));
            Compiler.LinkObjects(new[] { obj }, emit: EmitMode.ManagedLib).ShouldContain("static class DotCcLib");
            Should.Throw<CompileException>(() => Compiler.LinkObjectFiles(new[] { obj }, emit: EmitMode.ManagedLib, split: SourceSplit.Function))
                .Message.ShouldContain("regenerate objects");
        }
        finally { File.Delete(path); File.Delete(obj); }
    }

    [Theory]
    [InlineData(EmitMode.File)]
    [InlineData(EmitMode.Object)]
    public void Splitting_rejects_nonproject_modes_before_reading_input(EmitMode mode)
    {
        Should.Throw<CompileException>(() => Compiler.EmitCSharpFiles(Array.Empty<string>(), emit: mode, split: SourceSplit.Function));
        Should.Throw<CompileException>(() => Compiler.LinkObjectFiles(Array.Empty<string>(), emit: mode, split: SourceSplit.Size));
    }
}
