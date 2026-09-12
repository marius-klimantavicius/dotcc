using System;
using System.IO;
using System.Linq;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

public sealed class ObjectAggregateLinkTests
{
    [Theory]
    [InlineData(false, null)]
    [InlineData(true, null)]
    [InlineData(false, "Linked.Types")]
    [InlineData(true, "Linked.Types")]
    public void Complete_aggregate_replaces_opaque_declaration_in_either_order(bool reverse, string? ns)
    {
        WithDirectory(directory =>
        {
            var objects = Emit(directory,
                "struct State; struct State *get(void); int read(struct State *); int main(void) { return read(get()) == 42 ? 0 : 1; }",
                "struct State { int value; }; static struct State state = {42}; struct State *get(void) { return &state; } int read(struct State *p) { return p->value; }");
            if (reverse) Array.Reverse(objects);
            var linked = Compiler.LinkObjects(objects, namespaceName: ns);
            FixtureRunner.CompileAndRunCapturingExit(linked, Array.Empty<string>()).exit.ShouldBe(0);
        });
    }

    [Fact]
    public void Shared_header_anonymous_types_have_stable_identity_despite_prior_private_declarations()
    {
        WithDirectory(directory =>
        {
            File.WriteAllText(Path.Combine(directory, "shared.h"), """
                #define PAIR struct { int first; } a; struct { long second; } b;
                struct Shared { PAIR union { int value; unsigned bits; }; };
                struct Shared make(void);
                """);
            var objects = Emit(directory,
                """
                struct { int unrelated; } private_before;
                #include "shared.h"
                int main(void) { struct Shared s = make(); return s.a.first == 3 && s.b.second == 7 && s.value == 11 ? 0 : 1; }
                """,
                """
                #include "shared.h"
                struct Shared make(void) { struct Shared s; s.a.first = 3; s.b.second = 7; s.value = 11; return s; }
                """);
            var linked = Compiler.LinkObjects(objects);
            FixtureRunner.CompileAndRunCapturingExit(linked, Array.Empty<string>()).exit.ShouldBe(0);
        });
    }

    [Fact]
    public void Shared_header_identity_uses_resolved_path_across_include_aliases()
    {
        WithDirectory(directory =>
        {
            var include = Path.Combine(directory, "inc");
            Directory.CreateDirectory(include);
            File.WriteAllText(Path.Combine(include, "shared.h"),
                "struct Shared { struct { int value; } child; }; struct Shared make(void);\n");
            var first = Path.Combine(directory, "first.c");
            var second = Path.Combine(directory, "second.c");
            File.WriteAllText(first, "#include \"inc/shared.h\"\nint main(void) { struct Shared s = make(); return s.child.value == 42 ? 0 : 1; }");
            File.WriteAllText(second, "#include \"shared.h\"\nstruct Shared make(void) { struct Shared s; s.child.value = 42; return s; }");
            var objects = new[] { first, second }.Select(path =>
            {
                var obj = Path.ChangeExtension(path, ".obj.cs");
                File.WriteAllText(obj, Compiler.EmitObject(path, includeDirs: new[] { directory, include }));
                return obj;
            }).ToArray();
            FixtureRunner.CompileAndRunCapturingExit(Compiler.LinkObjects(objects), Array.Empty<string>()).exit.ShouldBe(0);
        });
    }

    [Fact]
    public void Unrelated_private_anonymous_types_do_not_collide_across_objects()
    {
        WithDirectory(directory =>
        {
            var objects = Emit(directory,
                "int peer(void); struct { int map; } local_a; int main(void) { local_a.map = 19; return local_a.map + peer() == 42 ? 0 : 1; }",
                "struct { long peer; } local_b; int peer(void) { local_b.peer = 23; return (int)local_b.peer; }");
            FixtureRunner.CompileAndRunCapturingExit(Compiler.LinkObjects(objects), Array.Empty<string>()).exit.ShouldBe(0);
        });
    }

    [Theory]
    [InlineData("struct Shared { int value; };", "struct Shared { long value; };")]
    [InlineData("struct Shared { int value; };", "struct Shared { int other; };")]
    [InlineData("struct Shared { int value; };", "union Shared { int value; };")]
    [InlineData("struct Shared { char c; int value; };", "#pragma pack(1)\nstruct Shared { char c; int value; };")]
    [InlineData("struct Shared { unsigned value:3; };", "struct Shared { unsigned value:4; };")]
    public void Conflicting_complete_declarations_report_both_objects(string first, string second)
    {
        WithDirectory(directory =>
        {
            var objects = Emit(directory, first + "\nint main(void) { return 0; }", second);
            var error = Should.Throw<CompileException>(() => Compiler.LinkObjects(objects));
            error.Message.ShouldContain("conflicting aggregate declarations for 'Shared'");
            error.Message.ShouldContain(objects[0]);
            error.Message.ShouldContain(objects[1]);
        });
    }

    [Fact]
    public void Mixed_legacy_aggregate_metadata_requires_regeneration()
    {
        WithDirectory(directory =>
        {
            var objects = Emit(directory, "struct Shared { int value; }; int main(void) { return 0; }", "struct Shared { int value; };");
            File.WriteAllLines(objects[1], File.ReadAllLines(objects[1]).Where(l => !l.StartsWith("//!!dotcc-obj aggregate:", StringComparison.Ordinal)));
            Should.Throw<CompileException>(() => Compiler.LinkObjects(objects)).Message.ShouldContain("regenerate older objects");
        });
    }

    private static string[] Emit(string directory, params string[] sources) => sources.Select((source, index) =>
    {
        var path = Path.Combine(directory, "unit" + index + ".c");
        File.WriteAllText(path, source);
        var fragment = Compiler.EmitObject(path, includeDirs: new[] { directory });
        fragment.ShouldBe(Compiler.EmitObject(path, includeDirs: new[] { directory }));
        var obj = Path.ChangeExtension(path, ".obj.cs");
        File.WriteAllText(obj, fragment);
        return obj;
    }).ToArray();

    private static void WithDirectory(Action<string> action)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-aggregate-link-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try { action(directory); }
        finally { Directory.Delete(directory, recursive: true); }
    }
}
