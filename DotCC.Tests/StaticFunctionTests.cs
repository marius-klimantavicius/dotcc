using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class StaticFunctionTests
{
    private static void WithSources(Action<string[], string> action, string first, string? second = null)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-static-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var paths = new[] { Path.Combine(directory, "first.c"), Path.Combine(directory, "second.c") };
            File.WriteAllText(paths[0], first);
            File.WriteAllText(paths[1], second ?? first);
            action(paths, directory);
        }
        finally { Directory.Delete(directory, true); }
    }

    private static string Translate(string[] paths, bool objects, CSharpOutputOptions? options,
        CPreprocessingOptions? preprocessing = null)
    {
        if (objects)
        {
            paths = paths.Select(path =>
            {
                var obj = Path.ChangeExtension(path, ".o");
                var unitOptions = preprocessing is null ? null : new CPreprocessingOptions(Array.Empty<MacroOverride>(),
                    functionOverrides: preprocessing.FunctionOverrides.Where(rule => rule.TranslationUnit is null || rule.TranslationUnit == path).ToArray());
                File.WriteAllText(obj, Compiler.EmitObject(path, preprocessing: unitOptions));
                return obj;
            }).ToArray();
            return Compiler.LinkObjects(paths, emit: EmitMode.ManagedLib, outputOptions: options);
        }
        return Compiler.EmitCSharp(paths, emit: EmitMode.ManagedLib, outputOptions: options, preprocessing: preprocessing);
    }

    private static int Methods(string text, string name) => Regex.Matches(text,
        @"static unsafe \w+\** " + name + @"(?:__\w+)?\(").Count;

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Static_deduplication_is_opt_in_and_covers_inline_and_noninline_dependencies(bool objects)
    {
        WithSources((paths, _) =>
        {
            Methods(Translate(paths, objects, null), "leaf").ShouldBe(2);
            var inline = Translate(paths, objects, new(DeduplicateInline: true));
            Methods(inline, "leaf").ShouldBe(2);
            Methods(inline, "helper").ShouldBe(2);
            var output = Translate(paths, objects, new(DeduplicateStatic: true));
            foreach (var name in new[] { "leaf", "helper", "recursive" })
                Methods(output, name).ShouldBe(1, name);
            output.ShouldContain("int helper(int x)");
            output.ShouldContain("return leaf(x)");
            output.ShouldNotContain("__dotcc_inline_ref__");
        }, """
            static int leaf(int x) { return x + 1; }
            static inline int helper(int x) { return leaf(x); }
            static int recursive(int x) { return x ? recursive(x - 1) : helper(x); }
            """);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Identical_immutable_aggregate_readers_and_their_callers_share_implementations(bool objects)
    {
        WithSources((paths, _) =>
        {
            var text = Translate(paths, objects, new(DeduplicateStatic: true));
            Methods(text, "leaf").ShouldBe(1);
            Methods(text, "helper").ShouldBe(1);
        }, """
            struct Value { int a[2]; };
            static const struct Value value = {{3, 7}};
            static const int offset = 2;
            static int leaf(int x) { struct Value copy = value; return copy.a[0] + copy.a[1] + offset + x; }
            static int helper(int x) { return leaf(x); }
            """);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Distinct_storage_and_function_identity_propagate_to_callers(bool objects)
    {
        foreach (var source in new[]
        {
            "static int value; static int leaf(int x) { return ++value + x; }",
            "static const volatile int value = 3; static int leaf(int x) { return value + x; }",
            "static int leaf(int x) { static int value; return ++value + x; }",
            "static const int value = 3; static const int *leaf(int x) { return &value; }",
            "struct Value { int a; }; static const struct Value value = {3}; static const int *leaf(int x) { return &value.a; }",
            "struct Value { int a[2]; }; static const struct Value value = {{3,7}}; static const int *leaf(int x) { return value.a; }",
            "static int leaf(int x) { return x; } static int (*saved)(int) = &leaf;",
        })
            WithSources((paths, _) =>
            {
                var text = Translate(paths, objects, new(DeduplicateStatic: true));
                Methods(text, "leaf").ShouldBe(2, source);
                Methods(text, "helper").ShouldBe(2, source);
            }, source + " static int helper(int x) { return leaf(x) == 0; }",
                source.Replace("saved", "other_saved") + " static int helper(int x) { return leaf(x) == 0; }");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Different_bodies_constants_signatures_and_private_layouts_remain_distinct(bool objects)
    {
        foreach (var pair in new[]
        {
            ("static int leaf(int x) { return x + 1; }", "static int leaf(int x) { return x + 2; }"),
            ("static const int value = 3; static int leaf(int x) { return value + x; }", "static const int value = 7; static int leaf(int x) { return value + x; }"),
            ("static int leaf(int x) { return x; }", "static unsigned leaf(unsigned x) { return x; }"),
            ("struct Value { int a; }; static int leaf(struct Value *p) { return p->a; }", "struct Value { long pad; int a; }; static int leaf(struct Value *p) { return p->a; }"),
        })
            WithSources((paths, _) => Methods(Translate(paths, objects, new(DeduplicateStatic: true)), "leaf").ShouldBe(2),
                pair.Item1, objects ? pair.Item2 : pair.Item2.Replace("value", "other_value"));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Export_inline_selectors_do_not_export_noninline_functions(bool objects)
    {
        WithSources((paths, _) =>
        {
            Should.Throw<CompileException>(() => Translate(paths, objects,
                new(DeduplicateStatic: true, ExportInline: new[] { "leaf" }))).Message.ShouldContain("matched no inline definitions");
            var text = Translate(paths, objects, new(DeduplicateStatic: true, ExportInline: new[] { "*" }));
            Methods(text, "leaf").ShouldBe(1);
            Methods(text, "helper").ShouldBe(1);
        }, "static int leaf(int x) { return x; } static inline int helper(int x) { return leaf(x); }");
    }

    private static string Legacy(string text, bool eraseShapes = false) => string.Join('\n', text.Split('\n').Select(line =>
    {
        if (line == "//!!dotcc-obj inline-metadata:4") return "//!!dotcc-obj inline-metadata:3";
        const string prefix = "//!!dotcc-obj inline:";
        if (!line.StartsWith(prefix, StringComparison.Ordinal)) return line;
        var fields = line[prefix.Length..].Split(' ');
        if (fields[6] != "1") return ""; // Version 3 covered only inline definitions.
        if (eraseShapes) fields[3] = new string(fields[1] == "leaf" ? 'A' : 'B', 64);
        return prefix + string.Join(' ', fields.Take(6));
    }));

    [Fact]
    public void Legacy_inline_metadata_is_supported_but_cannot_claim_static_coverage()
    {
        WithSources((paths, _) =>
        {
            var objects = paths.Select(path =>
            {
                var obj = Path.ChangeExtension(path, ".o");
                File.WriteAllText(obj, Legacy(Compiler.EmitObject(path)));
                return obj;
            }).ToArray();
            var inline = Compiler.LinkObjects(objects, emit: EmitMode.ManagedLib,
                outputOptions: new(DeduplicateInline: true, ExportInline: new[] { "pure" }));
            Methods(inline, "pure").ShouldBe(1);
            Methods(inline, "helper").ShouldBe(2);
            Methods(inline, "leaf").ShouldBe(2);
            Compiler.LinkObjects(objects, emit: EmitMode.ManagedLib).ShouldContain("helper__unit_");
            Should.Throw<CompileException>(() => Compiler.LinkObjects(objects, emit: EmitMode.ManagedLib,
                outputOptions: new(DeduplicateStatic: true))).Message.ShouldContain("regenerate objects");
            File.WriteAllText(objects[1], Compiler.EmitObject(paths[1]));
            var mixed = Compiler.LinkObjects(objects, emit: EmitMode.ManagedLib,
                outputOptions: new(DeduplicateInline: true, ExportInline: new[] { "pure" }));
            Methods(mixed, "pure").ShouldBe(1);
            Methods(mixed, "helper").ShouldBe(2);
            Should.Throw<CompileException>(() => Compiler.LinkObjects(objects, emit: EmitMode.ManagedLib,
                outputOptions: new(DeduplicateStatic: true))).Message.ShouldContain("static metadata");
            File.WriteAllText(objects[0], File.ReadAllText(objects[0]).Replace("inline-metadata:3", "inline-metadata:4"));
            Should.Throw<CompileException>(() => Compiler.LinkObjects(objects, emit: EmitMode.ManagedLib,
                outputOptions: new(DeduplicateStatic: true))).Message.ShouldContain("invalid static metadata");
            Should.Throw<CompileException>(() => Compiler.EmitObject(paths[0], outputOptions: new(DeduplicateStatic: true)))
                .Message.ShouldContain("link time");
        }, "static int leaf(int x) { return x; } static inline int helper(int x) { return leaf(x); } static inline int pure(int x) { return x + 1; }");
    }

    [Theory]
    [InlineData(false, "managedMethod")]
    [InlineData(true, "managedMethod")]
    [InlineData(false, "intrinsic")]
    [InlineData(true, "intrinsic")]
    public void Selected_override_targets_are_part_of_wrapper_and_caller_proofs(bool objects, string kind)
    {
        WithSources((paths, _) =>
        {
            var preprocessing = new CPreprocessingOptions(Array.Empty<MacroOverride>(), functionOverrides:
                paths.Select((path, index) => new FunctionOverride("leaf", new("unsigned int", new[] { "const void *" }),
                    new(kind, kind == "managedMethod" ? "global::Host." + (index == 0 ? "First" : "Second")
                        : index == 0 ? "load.u32.be" : "load.u32.le"), Linkage: "internal", TranslationUnit: path, RequireMatch: true)).ToArray());
            var text = Translate(paths, objects, new(DeduplicateStatic: true), preprocessing);
            Methods(text, "leaf").ShouldBe(2);
            Methods(text, "helper").ShouldBe(2);
            if (objects)
            {
                // Synthetic v3 proofs deliberately omit target distinctions.
                var legacy = paths.Select(path =>
                {
                    var obj = Path.ChangeExtension(path, ".o");
                    File.WriteAllText(obj, Legacy(File.ReadAllText(obj), eraseShapes: true));
                    return obj;
                }).ToArray();
                var old = Compiler.LinkObjects(legacy, emit: EmitMode.ManagedLib, outputOptions: new(DeduplicateInline: true));
                Methods(old, "leaf").ShouldBe(2);
                Methods(old, "helper").ShouldBe(2);
            }
        }, "static inline unsigned int leaf(const void *p) { return 0; } static inline unsigned int helper(const void *p) { return leaf(p); }");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Inline_options_preserve_unrelated_noninline_pointer_properties(bool exportOnly)
    {
        WithSources((paths, _) =>
        {
            var preprocessing = new CPreprocessingOptions(Array.Empty<MacroOverride>(), stableFunctionPointers: new[] { "leaf" });
            var fragment = Compiler.EmitObject(paths[0], preprocessing: preprocessing);
            // Model older automatic cache emission without an actual address use.
            fragment = string.Join('\n', fragment.Split('\n').Where(line => !line.StartsWith("//!!dotcc-obj function-address:", StringComparison.Ordinal))
                .Select(line =>
                {
                    const string prefix = "//!!dotcc-obj inline:";
                    if (!line.StartsWith(prefix, StringComparison.Ordinal)) return line;
                    var fields = line[prefix.Length..].Split(' '); fields[4] = "0"; return prefix + string.Join(' ', fields);
                }));
            var obj = Path.ChangeExtension(paths[0], ".o"); File.WriteAllText(obj, fragment);
            var text = Compiler.LinkObjects(new[] { obj }, emit: EmitMode.ManagedLib,
                outputOptions: exportOnly ? new(ExportInline: new[] { "helper" }) : new(DeduplicateInline: true));
            Regex.IsMatch(text, @"public static delegate\*<int, int> leaf(?:__\w+)?\s*\{").ShouldBeTrue();
        }, "static int leaf(int x) { return x; } static inline int helper(int x) { return leaf(x); }");
    }
}
