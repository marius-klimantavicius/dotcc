using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class InlineFunctionTests
{
    private static string Translate(bool link, string header, string first, string second, CSharpOutputOptions? options)
    {
        var dir = Path.Combine(Path.GetTempPath(), "dotcc-inline-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            File.WriteAllText(Path.Combine(dir, "shared.h"), header);
            var paths = new[] { Path.Combine(dir, "first.c"), Path.Combine(dir, "second.c") };
            File.WriteAllText(paths[0], first);
            File.WriteAllText(paths[1], second);
            if (link)
            {
                paths = paths.Select(path => {
                    var obj = Path.ChangeExtension(path, ".o"); File.WriteAllText(obj, Compiler.EmitObject(path)); return obj;
                }).ToArray();
                return Compiler.LinkObjects(paths, emit: EmitMode.ManagedLib, outputOptions: options);
            }
            return Compiler.EmitCSharp(paths, emit: EmitMode.ManagedLib, outputOptions: options);
        }
        finally { Directory.Delete(dir, true); }
    }
    private static int Methods(string source, string name) => Regex.Matches(source,
        @"static unsafe \w+\** " + name + @"(?:__\w+)?\(").Count;
    private const string First = "#include \"shared.h\"\nint first(void) { return helper(20); }";
    private const string Second = "#include \"shared.h\"\nint second(void) { return helper(30); }";

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Dedup_is_opt_in_and_exports_use_original_names(bool link)
    {
        const string header = "static inline int leaf(int x) { return x + 1; }\nstatic inline int helper(int x) { return leaf(x); }";
        Methods(Translate(link, header, First, Second, null), "helper").ShouldBe(2);
        var source = Translate(link, header, First, Second,
            new(DeduplicateInline: true, ExportInline: new[] { "help*", "leaf" }));
        Methods(source, "helper").ShouldBe(1);
        Methods(source, "leaf").ShouldBe(1);
        source.ShouldContain("int helper(int x)");
        source.ShouldContain("int leaf(int x)");
        source.ShouldNotContain("__dotcc_inline_ref__");
        source.ShouldNotContain(" helper = &");
        source.ShouldNotContain(" leaf = &");
        source.ShouldContain("return helper(20)");
        source.ShouldContain("return helper(30)");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Export_without_dedup_keeps_other_bodies(bool link)
    {
        var source = Translate(link, "static inline int helper(int x) { return x; }", First, Second,
            new(ExportInline: new[] { "helper" }));
        Methods(source, "helper").ShouldBe(2);
        source.ShouldContain("int helper(int x)");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Changed_macro_values_and_their_callers_stay_separate(bool link)
    {
        const string header = "static inline int leaf(int x) { return x + VALUE; }\nstatic inline int helper(int x) { return leaf(x); }";
        var a = "#define VALUE 1\n" + First;
        var b = "#define VALUE 2\n" + Second;
        var source = Translate(link, header, a, b, new(DeduplicateInline: true));
        Methods(source, "helper").ShouldBe(2);
        Methods(source, "leaf").ShouldBe(2);
        Should.Throw<CompileException>(() => Translate(link, header, a, b,
            new(DeduplicateInline: true, ExportInline: new[] { "helper" }))).Message.ShouldContain("ambiguous --export-inline");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Addresses_in_global_initializers_prevent_merging_and_keep_pointer_caches(bool link)
    {
        const string header = "static inline int helper(int x) { return x; }";
        var source = Translate(link, header, First + "\nint (*a)(int) = helper;", Second + "\nint (*b)(int) = &helper;",
            new(DeduplicateInline: true));
        Methods(source, "helper").ShouldBe(2);
        Regex.Matches(source, @" helper(?:__\w+)? = &").Count.ShouldBe(2);
        Should.Throw<CompileException>(() => Translate(link, header, First + "\nint (*a)(int) = helper;", Second + "\nint (*b)(int) = helper;",
            new(ExportInline: new[] { "helper" }))).Message.ShouldContain("ambiguous --export-inline");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Static_local_state_and_noninline_dependencies_stay_separate(bool link)
    {
        var source = Translate(link,
            "static inline int helper(int x) { static int counter; return ++counter + x; }", First, Second,
            new(DeduplicateInline: true));
        Methods(source, "helper").ShouldBe(2);
        source = Translate(link,
            "static int leaf(int x) { return x; }\nstatic inline int helper(int x) { return leaf(x); }", First, Second,
            new(DeduplicateInline: true));
        Methods(source, "helper").ShouldBe(2);
        Methods(source, "leaf").ShouldBe(2);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Recursive_groups_converge(bool link)
    {
        var source = Translate(link,
            "static inline int helper(int x) { return x > 0 ? helper(x - 1) : 42; }", First, Second,
            new(DeduplicateInline: true, ExportInline: new[] { "helper" }));
        Methods(source, "helper").ShouldBe(1);
        source.ShouldContain("helper(x - 1)");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Locals_are_compared_by_binding_and_signature_types_must_match(bool link)
    {
        var source = Translate(link, "", "static inline int helper(int a) { int b = a + 1; return b; }",
            "static inline int helper(int x) { int y = x + 1; return y; }", new(DeduplicateInline: true));
        Methods(source, "helper").ShouldBe(1);
        source = Translate(link, "", "static inline int helper(int a) { return a; }",
            "static inline unsigned int helper(unsigned int a) { return a; }", new(DeduplicateInline: true));
        Methods(source, "helper").ShouldBe(2);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Missing_patterns_and_name_collisions_are_diagnosed(bool link)
    {
        Should.Throw<CompileException>(() => Translate(link, "static inline int helper(int x) { return x; }", First, Second,
            new(ExportInline: new[] { "missing*" }))).Message.ShouldContain("matched no inline");
        Should.Throw<CompileException>(() => Translate(link, "", "static inline int helper(int x) { return x; }",
            "int helper(int x) { return x + 1; }", new(ExportInline: new[] { "helper" })))
            .Message.ShouldContain("conflicts with an existing declaration");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Unambiguous_shared_helpers_get_clean_names_without_export_selectors(bool link)
    {
        const string header = "static inline int helper(int x) { return x + 1; }";
        var source = Translate(link, header, First, Second, new(DeduplicateInline: true));
        source.ShouldContain("int helper(int x)");
        source.ShouldNotContain("helper__");
        source = Translate(link, "", header, "int helper(int x) { return x - 1; }", new(DeduplicateInline: true));
        Methods(source, "helper").ShouldBe(2); // An external name prevents automatic renaming.
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Unrelated_opaque_type_completion_does_not_split_a_shared_helper(bool link)
    {
        const string header = "struct Opaque; struct Wrapper { int value; struct Opaque *context; }; "
            + "static inline int helper(struct Wrapper *p) { return p->value; }";
        var source = Translate(link, header, "#include \"shared.h\"\nint first(struct Wrapper *p) { return helper(p); }",
            "#include \"shared.h\"\nstruct Opaque { long other; }; int second(struct Wrapper *p) { return helper(p); }",
            new(DeduplicateInline: true));
        Methods(source, "helper").ShouldBe(1);
        source.ShouldNotContain("helper__");
        source.ShouldContain("p->value");
    }

    [Fact]
    public void Incompatible_complete_layouts_are_rejected_before_deduplicating()
    {
        const string helper = "static inline int helper(struct Wrapper *p) { return p->value; }";
        Should.Throw<CompileException>(() => Translate(true, "",
            "struct Wrapper { int value; }; " + helper,
            "struct Wrapper { long pad; int value; }; " + helper,
            new(DeduplicateInline: true))).Message.ShouldContain("conflicting aggregate");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Identical_static_constants_allow_value_readers_and_callers_to_merge(bool link)
    {
        const string header = "struct Value { int parts[2]; }; static const struct Value value = {{3, 7}}; "
            + "static const int offset = 2; "
            + "static inline int leaf(int x) { struct Value copy = value; return copy.parts[0] + copy.parts[1] + offset + x; } "
            + "static inline int helper(int x) { return leaf(x); }";
        Methods(Translate(link, header, First, Second, null), "helper").ShouldBe(2);
        var source = Translate(link, header, First, Second, new(DeduplicateInline: true, ExportInline: new[] { "helper" }));
        Methods(source, "helper").ShouldBe(1);
        Methods(source, "leaf").ShouldBe(1);
        source.ShouldContain("int helper(int x)");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Static_storage_with_mutability_or_observable_identity_does_not_enable_merging(bool link)
    {
        foreach (var header in new[] {
            "static int value = 3; static inline int helper(int x) { return value + x; }",
            "static const volatile int value = 3; static inline int helper(int x) { return value + x; }",
            "static const int value = 3; static inline const int *helper(int x) { return &value; }",
            "struct Value { int n; }; static const struct Value value = {3}; static inline const int *helper(int x) { return &value.n; }",
            "struct Value { int n[2]; }; static const struct Value value = {{3, 7}}; static inline const int *helper(int x) { return value.n; }",
            "static int state; static int *const value = &state; static inline int helper(int x) { return *value + x; }",
        })
        {
            var source = Translate(link, header, First, Second, new(DeduplicateInline: true));
            Methods(source, "helper").ShouldBe(2, header);
        }
    }

    [Fact]
    public void Different_static_constant_initializers_have_different_object_proofs()
    {
        var dir = Path.Combine(Path.GetTempPath(), "dotcc-constant-proof-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            string Shape(int value)
            {
                var path = Path.Combine(dir, "source.c");
                File.WriteAllText(path, $"static const int value = {value}; static inline int helper(int x) {{ return value + x; }}");
                return Compiler.EmitObject(path).Split('\n').Single(l => l.StartsWith("//!!dotcc-obj inline:"));
            }
            Shape(3).ShouldNotBe(Shape(7));
        }
        finally { Directory.Delete(dir, true); }
    }

    [Fact]
    public void Older_objects_require_regeneration_only_when_inline_options_are_used()
    {
        var dir = Path.Combine(Path.GetTempPath(), "dotcc-inline-old-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            var source = Path.Combine(dir, "source.c");
            var obj = Path.Combine(dir, "source.o");
            File.WriteAllText(source, "static inline int helper(int x) { return x; }");
            var fragment = Compiler.EmitObject(source);
            File.WriteAllText(obj, fragment.Replace("//!!dotcc-obj inline-metadata:3\n", ""));
            Compiler.LinkObjects(new[] { obj }, emit: EmitMode.ManagedLib).ShouldContain("int helper__unit_");
            Should.Throw<CompileException>(() => Compiler.LinkObjects(new[] { obj }, emit: EmitMode.ManagedLib,
                outputOptions: new(DeduplicateInline: true))).Message.ShouldContain("regenerate objects");
            Should.Throw<CompileException>(() => Compiler.EmitCSharp(new[] { source }, emit: EmitMode.Object,
                outputOptions: new(DeduplicateInline: true))).Message.ShouldContain("link time");
            Should.Throw<CompileException>(() => Compiler.EmitCSharp(new[] { source }, emit: EmitMode.ManagedLib,
                outputOptions: new(ExportInline: new[] { "[broken]" }))).Message.ShouldContain("invalid --export-inline");
        }
        finally { Directory.Delete(dir, true); }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Bound_reference_relocation_preserves_member_names_and_literal_markers(bool link)
    {
        var source = Translate(link, "static inline int helper(int x) { return x; }", First,
            Second + "\nstruct Pair { int helper; }; int member(struct Pair* p) { return p->helper; }\n"
            + "char *text(void) { return \"/*__dotcc_inline_ref__*/helper\"; }",
            new(DeduplicateInline: true, ExportInline: new[] { "helper" }));
        source.ShouldContain("p->helper");
        source.ShouldContain("\"/*__dotcc_inline_ref__*/helper\\0\"u8");
    }
}
