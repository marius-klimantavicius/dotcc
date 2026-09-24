using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

public sealed class LiteralPoolTests
{
    private static SyntaxTree ParseSource(string source, string path = "", CancellationToken cancellationToken = default) =>
        CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Preview), path: path, cancellationToken: cancellationToken);

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Storage_mode_is_selected_at_final_emission(bool link)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-literal-modes-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var a = Path.Combine(directory, "a.c");
            var b = Path.Combine(directory, "b.c");
            File.WriteAllText(a, """
                #include <uchar.h>
                extern int puts(const char *);
                extern const char *second(void);
                const int numbers[] = { 7, 42 };
                const unsigned char binary[] = { 255, 128 };
                const char *global_text = "one";
                int main(void) {
                    const char *text = "one";
                    const char *unicode = "é😀";
                    const char16_t *wide = u"DotCcLiterals.ConstArray";
                    if (text[0] != 'o' || global_text[1] != 'n' || second()[0] != 's') return 1;
                    if (numbers[1] != 42 || binary[0] != 255 || (unsigned char)unicode[2] != 240) return 2;
                    if (wide[0] != 'D' || wide[14] != 'C') return 3;
                    puts("ok"); return 0;
                }
                """);
            File.WriteAllText(b, "const char *second(void) { return \"second\"; }");
            var paths = new[] { a, b };
            if (link)
                paths = paths.Select(path => { var obj = path + ".cs"; File.WriteAllText(obj, Compiler.EmitObject(path)); return obj; }).ToArray();
            foreach (bool enabled in new[] { false, true })
            {
                // Reuse the same objects for both choices; null also checks the API default.
                var options = enabled ? new CSharpOutputOptions(LiteralPool: true) : null;
                var emitted = link
                    ? Compiler.LinkObjects(paths, emit: EmitMode.Csproj, outputOptions: options)
                    : Compiler.EmitCSharp(paths, emit: EmitMode.Csproj, outputOptions: options);
                if (enabled)
                {
                    emitted.ShouldContain("DotCcLiterals.S0");
                    emitted.ShouldContain("Libc.GlobalArrayFrom<int>(new int[]{ 7, 42 })");
                    emitted.ShouldContain("Libc.GlobalArrayFrom<byte>(new byte[]{ 255, 128 })");
                    emitted.ShouldNotContain("Libc.L(");
                    emitted.ShouldNotContain("Libc.L<");
                    var pool = ParseSource(emitted, cancellationToken: TestContext.Current.CancellationToken)
                        .GetRoot(TestContext.Current.CancellationToken).DescendantNodes().OfType<ClassDeclarationSyntax>()
                        .Single(c => c.Identifier.ValueText == "DotCcProgramLiterals");
                    var names = pool.Members.OfType<FieldDeclarationSyntax>()
                        .Where(f => f.Modifiers.Any(SyntaxKind.ConstKeyword))
                        .SelectMany(f => f.Declaration.Variables).Select(v => v.Identifier.ValueText).ToArray();
                    names.ShouldBe(new[] { "S0", "S1", "S2", "S3" }); // "one" deduplicated, plus Unicode, second and ok.
                }
                else
                {
                    emitted.ShouldContain("Libc.L(\"one\\0\"u8)");
                    emitted.ShouldContain("Libc.L<int>(new int[]{ 7, 42 })");
                    emitted.ShouldContain("Libc.L(new byte[]{ 255, 128 })");
                    emitted.ShouldNotContain("using DotCcLiterals =");
                    emitted.ShouldNotContain("class DotCcProgramLiterals");
                    emitted.ShouldNotContain("/*\"one\"*/");
                }
                FixtureRunner.CompileAndRun(emitted, Array.Empty<string>()).ReplaceLineEndings("\n").ShouldBe("ok\n");
            }
        }
        finally { Directory.Delete(directory, true); }
    }

    [Fact]
    public void Pool_flag_is_rejected_during_object_emission()
    {
        Should.Throw<CompileException>(() => Compiler.EmitCSharp(Array.Empty<string>(), emit: EmitMode.Object,
            outputOptions: new(LiteralPool: true))).Message.ShouldContain("set at link time");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Literal_previews_are_safe_comments_and_preserve_bytes(bool link)
    {
        var path = Path.Combine(Path.GetTempPath(), "dotcc-literal-previews-" + Guid.NewGuid().ToString("N") + ".c");
        var obj = path + ".cs";
        var cases = new (string Source, string Preview, byte[] Bytes)[]
        {
            ("Hello, world", "/*\"Hello, world\"*/", "Hello, world"u8.ToArray()),
            ("", "/*\"\"*/", []),
            ("a\\0b\\0", "/*\"a\\0b\\0\"*/", [97, 0, 98, 0]),
            ("*/ /* //!!dotcc-obj section:functions", "/*\"*\\u002F \\u002F* //!!dotcc-obj section:functions\"*/", "*/ /* //!!dotcc-obj section:functions"u8.ToArray()),
            ("\\\"\\\\\\n\\r\\t\\001", "/*\"\\\"\\\\\\n\\r\\t\\u0001\"*/", [34, 92, 10, 13, 9, 1]),
            ("é😀\\xe2\\x80\\xa8\\xe2\\x80\\xa9", "/*\"é😀\\u2028\\u2029\"*/", "é😀\u2028\u2029"u8.ToArray()),
            ("\\xffZ\\200", "/*\"\\xFFZ\\x80\"*/", [255, 90, 128]),
            (new string('x', 300), "/*\"" + new string('x', 160) + "\"…*/", Enumerable.Repeat((byte)'x', 300).ToArray())
        };
        try
        {
            var source = new System.Text.StringBuilder("extern int puts(const char *); int main(void) {\n");
            for (int i = 0; i < cases.Length; i++)
            {
                source.Append("const char *p").Append(i).Append(" = \"").Append(cases[i].Source).Append("\";\n");
                var expected = cases[i].Bytes.Append((byte)0).ToArray();
                for (int j = 0; j < expected.Length; j++)
                    source.Append("if ((unsigned char)p").Append(i).Append('[').Append(j).Append("] != ").Append(expected[j]).Append(") return 1;\n");
            }
            source.Append("puts(\"ok\"); return 0; }");
            File.WriteAllText(path, source.ToString());
            string emitted;
            if (link)
            {
                File.WriteAllText(obj, Compiler.EmitObject(path));
                emitted = Compiler.LinkObjects(new[] { obj }, emit: EmitMode.Csproj, outputOptions: new(LiteralPool: true));
            }
            else emitted = Compiler.EmitCSharp(new[] { path }, emit: EmitMode.Csproj, outputOptions: new(LiteralPool: true));
            var tree = ParseSource(emitted, cancellationToken: TestContext.Current.CancellationToken);
            var comments = tree.GetRoot(TestContext.Current.CancellationToken).DescendantTrivia()
                .Where(t => t.IsKind(SyntaxKind.MultiLineCommentTrivia)).Select(t => t.ToFullString()).ToHashSet();
            foreach (var item in cases) comments.ShouldContain(item.Preview);
            FixtureRunner.CompileAndRun(emitted, Array.Empty<string>()).ReplaceLineEndings("\n").ShouldBe("ok\n");
        }
        finally { File.Delete(path); File.Delete(obj); }
    }

    [Fact]
    public void Two_translations_keep_independent_pools_without_libc()
    {
        var path = Path.Combine(Path.GetTempPath(), "dotcc-independent-literals-" + Guid.NewGuid().ToString("N") + ".c");
        try
        {
            File.WriteAllText(path, "const char *text(void) { return \"shared text\"; }");
            var trees = new List<SyntaxTree>();
            foreach (var owner in new[] { "First", "Second" })
            {
                var emitted = Compiler.EmitCSharp(new[] { path }, emit: EmitMode.ManagedLib, className: owner, outputOptions: new(LiteralPool: true));
                var root = ParseSource(emitted, cancellationToken: TestContext.Current.CancellationToken).GetRoot(TestContext.Current.CancellationToken);
                var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>();
                var api = classes.Single(c => c.Identifier.ValueText == owner);
                var pool = classes.Single(c => c.Identifier.ValueText == owner + "Literals");
                // Compile both translated APIs and pools together, with no libc source or reference.
                var source = "using System; using System.Runtime.CompilerServices; using System.Runtime.InteropServices;\n"
                    + "using DotCcLiterals = " + owner + "Literals;\n" + api.ToFullString() + pool.ToFullString();
                trees.Add(ParseSource(source, cancellationToken: TestContext.Current.CancellationToken));
            }
            trees.Add(ParseSource("""
                public static unsafe class Check {
                    public static bool Run() => First.text() != Second.text()
                        && System.Runtime.InteropServices.Marshal.PtrToStringUTF8((nint)First.text()) == "shared text"
                        && System.Runtime.InteropServices.Marshal.PtrToStringUTF8((nint)Second.text()) == "shared text";
                }
                """, cancellationToken: TestContext.Current.CancellationToken));
            var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator).Select(file => MetadataReference.CreateFromFile(file));
            var compilation = CSharpCompilation.Create("IndependentPools", trees, references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));
            using var image = new MemoryStream();
            var result = compilation.Emit(image, cancellationToken: TestContext.Current.CancellationToken);
            result.Success.ShouldBeTrue(string.Join("\n", result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error)));
            image.Position = 0;
            var context = new AssemblyLoadContext("independent-pools", isCollectible: true);
            try { context.LoadFromStream(image).GetType("Check")!.GetMethod("Run")!.Invoke(null, null).ShouldBe(true); }
            finally { context.Unload(); }
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void Zig_unicode_and_error_names_use_the_pool()
    {
        var path = Path.Combine(Path.GetTempPath(), "dotcc-zig-literals-" + Guid.NewGuid().ToString("N") + ".zig");
        try
        {
            File.WriteAllText(path, """
                extern fn printf(format: [*c]const u8, ...) c_int;
                pub fn main() u8 {
                    const name = @errorName(error.Foo);
                    _ = printf("%s %s\n", "\u{2764}\u{1F600}", name.ptr);
                    return 0;
                }
                """);
            var emitted = Compiler.EmitCSharp(new[] { path }, emit: EmitMode.Csproj, outputOptions: new(LiteralPool: true));
            emitted.ShouldNotContain("Libc.L(");
            emitted.ShouldContain("DotCcLiterals.Pointer");
            FixtureRunner.CompileAndRun(emitted, Array.Empty<string>()).ReplaceLineEndings("\n").ShouldBe("❤😀 Foo\n");
        }
        finally { File.Delete(path); }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Legacy_or_corrupt_literal_objects_require_recompilation(bool corrupt)
    {
        var path = Path.Combine(Path.GetTempPath(), "dotcc-literal-object-" + Guid.NewGuid().ToString("N") + ".c");
        var obj = path + ".cs";
        try
        {
            File.WriteAllText(path, "int main(void) { return \"text\"[0]; }");
            var fragment = Compiler.EmitObject(path);
            if (corrupt)
            {
                const string prefix = "// literal-bytes: ";
                int start = fragment.IndexOf(prefix, StringComparison.Ordinal) + prefix.Length;
                fragment = fragment[..start] + (fragment[start] == 'A' ? 'B' : 'A') + fragment[(start + 1)..];
            }
            else fragment = fragment.Replace("//!!dotcc-obj literal-pool:2\n", "");
            File.WriteAllText(obj, fragment);
            Should.Throw<CompileException>(() => Compiler.LinkObjects(new[] { obj })).Message.ShouldContain("regenerate objects");
        }
        finally { File.Delete(path); File.Delete(obj); }
    }

    [Theory]
    [InlineData(false, false, false, OptimizationLevel.Debug)]
    [InlineData(false, true, true, OptimizationLevel.Release)]
    [InlineData(true, false, true, OptimizationLevel.Debug)]
    [InlineData(true, true, false, OptimizationLevel.Release)]
    public void Pooled_bytes_survive_gc_and_are_collected_with_the_library(bool link, bool split, bool nested, OptimizationLevel optimization)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-literals-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var a = Path.Combine(directory, "a.c");
            var b = Path.Combine(directory, "b.c");
            File.WriteAllText(a, """
                const char *global_text = "one";
                const int numbers[] = { -7, 42, 0 };
                const int twin[] = { -7, 42, 0 };
                const double real_values[] = { -0.0, 1.25 };
                const char *first(void) { return "one"; }
                const char *second(void) { return "second"; }
                const char *unicode(void) { return "é😀"; }
                const char *binary(void) { return "\xffZ\200"; }
                const char *embedded(void) { return "a\0b"; }
                const char *empty(void) { return ""; }
                int check_arrays(void) { return numbers != twin && numbers[1] == 42 && twin[0] == -7 && real_values[1] == 1.25; }
                int unicode_size(void) { return sizeof("é😀"); }
                """);
            var longText = string.Concat(Enumerable.Repeat("ab\"\\😀é\n", 400));
            // Exercise split boundaries around quotes, escapes and surrogate pairs.
            var cLong = longText.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n");
            File.WriteAllText(b, "const char *duplicate(void) { return \"one\"; }\nconst char *long_text(void) { return \"" + cLong + "\"; }");
            var paths = new[] { a, b };
            if (link)
            {
                paths = paths.Select(path => { var obj = path + ".cs"; File.WriteAllText(obj, Compiler.EmitObject(path)); return obj; }).ToArray();
            }
            var options = new CSharpOutputOptions { NestTypes = nested, Runtime = RuntimeProfile.C, LiteralPool = true };
            var files = link
                ? Compiler.LinkObjectFiles(paths, EmitMode.ManagedLib, className: "LiteralLibrary", namespaceName: "PoolTests", split: split ? SourceSplit.Function : SourceSplit.None, outputOptions: options)
                : Compiler.EmitCSharpFiles(paths, emit: EmitMode.ManagedLib, className: "LiteralLibrary", namespaceName: "PoolTests", split: split ? SourceSplit.Function : SourceSplit.None, outputOptions: options);
            foreach (var text in files.Values)
            {
                text.ShouldNotContain("Libc.L(");
                text.ShouldNotContain("Libc.L<");
                foreach (var line in text.Split('\n').Where(line => line.TrimStart().StartsWith('"') && line.Contains("u8")))
                    line.Length.ShouldBeLessThanOrEqualTo(510);
            }
            var consumer = """
                using System;
                using System.Runtime.InteropServices;
                using Api = PoolTests.LiteralLibrary;
                public static unsafe class LiteralConsumer
                {
                    public static void FirstAccess() { if (Marshal.PtrToStringUTF8((nint)Api.first()) != "one") throw new Exception("first access"); }
                    public static string Check()
                    {
                        byte* first = Api.first();
                        byte* binary = Api.binary();
                        GC.Collect(2, GCCollectionMode.Forced, true, true);
                        // Public global pointer slots use the stable nint storage ABI.
                        if (first != Api.first() || first != Api.duplicate() || (nint)first != Api.Globals.global_text) throw new Exception("identity");
                        if (Marshal.PtrToStringUTF8((nint)first) != "one" || Marshal.PtrToStringUTF8((nint)Api.second()) != "second") throw new Exception("text");
                        if (Marshal.PtrToStringUTF8((nint)Api.unicode()) != "é😀" || Api.unicode_size() != 7) throw new Exception("unicode");
                        if (!new ReadOnlySpan<byte>(binary, 4).SequenceEqual(new byte[] { 255, 90, 128, 0 })) throw new Exception("binary");
                        if (!new ReadOnlySpan<byte>(Api.embedded(), 4).SequenceEqual(new byte[] { 97, 0, 98, 0 }) || *Api.empty() != 0) throw new Exception("nul");
                        if (Api.check_arrays() != 1) throw new Exception("arrays");
                        if (Marshal.PtrToStringUTF8((nint)POOLLIBC.strerror(2)) != "No such file or directory") throw new Exception("runtime");
                        if (Marshal.PtrToStringUTF8((nint)POOLLIBC.setlocale(0, null)) != "C") throw new Exception("locale");
                        return Marshal.PtrToStringUTF8((nint)Api.long_text());
                    }
                }
                """.Replace("POOLLIBC", nested ? "PoolTests.LiteralLibrary.Libc" : "PoolTests.Libc");
            var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator).Select(path => MetadataReference.CreateFromFile(path));
            var trees = files.Select(f => ParseSource(f.Value, path: f.Key, cancellationToken: TestContext.Current.CancellationToken)).Append(ParseSource(consumer, cancellationToken: TestContext.Current.CancellationToken));
            var compilation = CSharpCompilation.Create("Literals_" + Guid.NewGuid().ToString("N"), trees, references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true, optimizationLevel: optimization));
            using var image = new MemoryStream();
            var result = compilation.Emit(image, cancellationToken: TestContext.Current.CancellationToken);
            result.Success.ShouldBeTrue(string.Join("\n", result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error)));
            var weak = ExecuteAndUnload(image.ToArray(), nested, longText);
            for (int i = 0; i < 80 && (weak.Context.IsAlive || weak.Storage.IsAlive); ++i)
            {
                GC.Collect(2, GCCollectionMode.Forced, true, true);
                GC.WaitForPendingFinalizers();
                // Parallel.For's completed workers can briefly retain the first-access delegate.
                if (weak.Context.IsAlive || weak.Storage.IsAlive) Thread.Sleep(25);
            }
            weak.Context.IsAlive.ShouldBeFalse("the pool must not prevent assembly unloading");
            weak.Storage.IsAlive.ShouldBeFalse("the pool must be reclaimed with its assembly");
        }
        finally { Directory.Delete(directory, true); }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (WeakReference Context, WeakReference Storage) ExecuteAndUnload(byte[] image, bool nested, string expected)
    {
        var context = new AssemblyLoadContext("literal-pool", isCollectible: true);
        var assembly = context.LoadFromStream(new MemoryStream(image));
        var consumer = assembly.GetType("LiteralConsumer")!;
        var first = (Action)consumer.GetMethod("FirstAccess")!.CreateDelegate(typeof(Action));
        Parallel.For(0, 16, _ => first());
        consumer.GetMethod("Check")!.Invoke(null, null).ShouldBe(expected);
        var pool = assembly.GetType(nested ? "PoolTests.LiteralLibrary+LiteralLibraryLiterals" : "PoolTests.LiteralLibraryLiterals")!;
        var storage = (byte[])pool.GetField("Storage", BindingFlags.NonPublic | BindingFlags.Static)!.GetValue(null)!;
        var weak = (new WeakReference(context), new WeakReference(storage));
        context.Unload();
        GC.KeepAlive(assembly);
        return weak;
    }

    [Fact]
    public void Wrapped_u8_expression_is_one_compiled_blob()
    {
        var text = string.Concat(Enumerable.Repeat("one\0second\0😀\0", 40000));
        var expression = LiteralPool.FormatUtf8(text, "    ");
        var tree = ParseSource("class Test { static System.ReadOnlySpan<byte> Data => " + expression + "; }", cancellationToken: TestContext.Current.CancellationToken);
        // The compiler evaluates every chunk into a single UTF-8 value; no per-chunk allocations.
        var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator).Select(path => MetadataReference.CreateFromFile(path));
        var compilation = CSharpCompilation.Create("WrappedLiteral", new[] { tree }, references, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Release));
        using var stream = new MemoryStream();
        var result = compilation.Emit(stream, cancellationToken: TestContext.Current.CancellationToken);
        result.Success.ShouldBeTrue(string.Join("\n", result.Diagnostics));
        using var pe = new System.Reflection.PortableExecutable.PEReader(new MemoryStream(stream.ToArray()));
        var metadata = System.Reflection.Metadata.PEReaderExtensions.GetMetadataReader(pe);
        metadata.FieldDefinitions.Count(handle => metadata.GetFieldDefinition(handle).GetRelativeVirtualAddress() != 0).ShouldBe(1);
        expression.Split('\n').Length.ShouldBeLessThan(2500);
    }
}
