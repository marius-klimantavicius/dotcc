using System;
using System.IO;
using System.Linq;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

public sealed class AggregateLinkageComponentTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Private_tag_variant_and_exposed_opaque_parent_work_in_nested_owned_libraries(bool objects)
    {
        WithDirectory(directory =>
        {
            var paths = Sources(directory,
                """
                struct Session; struct Session *make(void); int read(struct Session *); int private_count(void);
                int main(void) { return read(make()) == 37 && private_count() == 5 ? 0 : 1; }
                """,
                """
                struct State { long value; }; struct Session { struct State state; };
                static struct Session session = {{37}};
                struct Session *make(void) { return &session; }
                int read(struct Session *p) { return (int)p->state.value++; }
                """,
                """
                struct State { int values[5]; }; static struct State local = {{1,2,3,4,5}};
                int private_count(void) { return sizeof(local.values) / sizeof(int); }
                """);
            if (objects) paths = Objects(paths);
            var options = new CSharpOutputOptions(NestTypes: true, Runtime: RuntimeProfile.C, StateContext: true,
                LiteralPool: true, DeduplicateInline: true);
            string code = objects
                ? Compiler.LinkObjects(paths, emit: EmitMode.ManagedLib, className: "Api", namespaceName: "Managed.Components", outputOptions: options)
                : Compiler.EmitCSharp(paths, emit: EmitMode.ManagedLib, className: "Api", namespaceName: "Managed.Components", outputOptions: options);
            code += """

                public static class ConsumerEntry {
                    public static int Main() {
                        using (var first = Api.__DotCcCreateContext()) {
                            using var binding = first.Enter();
                            if (Api.main() != 0 || Api.main() != 1) return 1;
                        }
                        using (var fresh = Api.__DotCcCreateContext()) {
                            using var binding = fresh.Enter();
                            return Api.main();
                        }
                    }
                }
                """;
            FixtureRunner.CompileAndRunCapturingExit(code, Array.Empty<string>()).exit.ShouldBe(0);
        });
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void Unrelated_extern_apis_share_compatible_graphs_without_linking_their_private_tags(bool objects, bool reverse)
    {
        WithDirectory(directory =>
        {
            var paths = Sources(directory,
                """
                struct Zone; struct Regex;
                struct Zone *make_zone(void); int read_zone(struct Zone *);
                struct Regex *make_regex(void); int read_regex(struct Regex *);
                int main(void) { return read_zone(make_zone()) + read_regex(make_regex()) == 42 ? 0 : 1; }
                """,
                """
                struct State { long value; }; struct Zone { struct State state; };
                static struct Zone zone = {{19}};
                struct Zone *make_zone(void) { return &zone; }
                """,
                """
                struct State { long value; }; struct Zone { struct State state; };
                int read_zone(struct Zone *p) { return (int)p->state.value; }
                """,
                """
                struct State { int value; struct State *next; };
                struct Regex { struct State *state; int (*read)(struct State *); };
                static struct State state = {23, &state};
                static int read(struct State *p) { return p == p->next ? p->value : -1; }
                static struct Regex regex = {&state, read};
                struct Regex *make_regex(void) { return &regex; }
                """,
                """
                struct State { int value; struct State *next; };
                struct Regex { struct State *state; int (*read)(struct State *); };
                int read_regex(struct Regex *p) { return p->read(p->state); }
                """);
            if (objects) paths = Objects(paths);
            if (reverse) Array.Reverse(paths);
            string code = objects ? Compiler.LinkObjects(paths) : Compiler.EmitCSharp(paths);
            FixtureRunner.CompileAndRunCapturingExit(code, Array.Empty<string>()).exit.ShouldBe(0);
        });
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void Unused_shared_external_objects_and_functions_keep_strong_aggregate_abi_checks(bool objects, bool function)
    {
        WithDirectory(directory =>
        {
            string external = function ? "int shared(struct State *);" : "extern struct State *shared;";
            var paths = Sources(directory, "struct State { int value; }; " + external + " int main(void) { return 0; }",
                "struct State { long value; }; " + external);
            if (objects) paths = Objects(paths);
            var error = Should.Throw<CompileException>(() => { if (objects) Compiler.LinkObjects(paths); else Compiler.EmitCSharp(paths); });
            error.Message.ShouldContain("conflicting aggregate declarations for 'State'");
            error.Message.ShouldContain(Path.GetFileName(paths[0])); error.Message.ShouldContain(Path.GetFileName(paths[1]));
        });
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Shared_aggregate_prototype_cannot_change_pointer_depth(bool objects)
    {
        WithDirectory(directory =>
        {
            var paths = Sources(directory,
                "struct State { int value; }; int shared(struct State *); int main(void) { return 0; }",
                "struct State { int value; }; int shared(struct State **);");
            if (objects) paths = Objects(paths);
            Should.Throw<CompileException>(() => { if (objects) Compiler.LinkObjects(paths); else Compiler.EmitCSharp(paths); })
                .Message.ShouldContain("conflicting external type contracts");
        });
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void Incomplete_external_arrays_keep_declaration_bounds_before_execution_decay(bool objects, bool rows)
    {
        WithDirectory(directory =>
        {
            string header = "struct Entry { int value; }; extern const struct Entry options" + (rows ? "[][2]" : "[]") + ";\n";
            string first = header + (rows
                ? "int main(void) { return sizeof(options[0])==8 && options[0][1].value+options[1][0].value==42 ? 0 : 1; }"
                : "int main(void) { return options[0].value+options[1].value==42 ? 0 : 1; }");
            string second = header + (rows
                ? "const struct Entry options[2][2] = {{{1},{19}},{{23},{4}}};"
                : "const struct Entry options[] = {{19},{23}};");
            var paths = Sources(directory, first, second);
            if (objects)
            {
                paths = Objects(paths);
                var definition = AggregateIdentities.Read(File.ReadAllText(paths[1])).ShouldBeOfType<AggregateIdentitySet>();
                definition.Linkage.Single(c => c.Symbol == "options").ArrayBounds.ShouldBe(rows ? new int?[] { 2, 2 } : new int?[] { 2 });
            }
            string code = objects ? Compiler.LinkObjects(paths) : Compiler.EmitCSharp(paths);
            FixtureRunner.CompileAndRunCapturingExit(code, Array.Empty<string>()).exit.ShouldBe(0);
        });
    }

    [Theory]
    [InlineData(false, "*options")]
    [InlineData(true, "*options")]
    [InlineData(false, "options[0]")]
    [InlineData(true, "options[0]")]
    [InlineData(false, "options[3]")]
    [InlineData(true, "options[3]")]
    public void Pointer_objects_and_explicit_zero_or_conflicting_known_array_bounds_are_not_incomplete_arrays(bool objects, string declaration)
    {
        WithDirectory(directory =>
        {
            var paths = Sources(directory, "struct Entry { int value; }; extern struct Entry " + declaration + "; int main(void) { return 0; }",
                "struct Entry { int value; }; struct Entry options[2];");
            if (objects) paths = Objects(paths);
            Should.Throw<CompileException>(() => { if (objects) Compiler.LinkObjects(paths); else Compiler.EmitCSharp(paths); })
                .Message.ShouldContain("conflicting external type contracts");
        });
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Authored_reserved_tag_cannot_hide_behind_a_generated_private_identity(bool objects)
    {
        WithDirectory(directory =>
        {
            var paths = Sources(directory,
                "struct __DotCcUser { int value; }; int first(struct __DotCcUser *p) { return p->value; }",
                "struct __DotCcUser { long value; }; int second(struct __DotCcUser *p) { return (int)p->value; }");
            if (objects) paths = Objects(paths);
            var options = new CSharpOutputOptions(Runtime: RuntimeProfile.C, StateContext: true);
            Should.Throw<CompileException>(() => {
                if (objects) Compiler.LinkObjects(paths, emit: EmitMode.ManagedLib, outputOptions: options);
                else Compiler.EmitCSharp(paths, emit: EmitMode.ManagedLib, outputOptions: options);
            }).Message.ShouldContain("reserves the __DotCc");
        });
    }

    [Fact]
    public void Legacy_objects_do_not_silently_guess_new_linkage_components()
    {
        WithDirectory(directory =>
        {
            var paths = Objects(Sources(directory,
                "struct State { int value; }; int first(struct State *p) { return p->value; } int main(void) { return 0; }",
                "struct State { long value; }; int second(struct State *p) { return (int)p->value; }"));
            foreach (string path in paths)
                File.WriteAllLines(path, File.ReadAllLines(path).Where(line => line != AggregateIdentities.LinkageVersion
                    && !line.StartsWith(AggregateIdentities.LinkagePrefix, StringComparison.Ordinal)));
            Should.Throw<CompileException>(() => Compiler.LinkObjects(paths)).Message.ShouldContain("conflicting aggregate declarations");
        });
    }

    private static string[] Sources(string directory, params string[] sources) => sources.Select((source, index) =>
    {
        string path = Path.Combine(directory, "unit" + index + ".c"); File.WriteAllText(path, source); return path;
    }).ToArray();
    private static string[] Objects(string[] paths) => paths.Select(path =>
    {
        string obj = path + ".cs"; File.WriteAllText(obj, Compiler.EmitObject(path)); return obj;
    }).ToArray();
    private static void WithDirectory(Action<string> action)
    {
        string directory = Path.Combine(Path.GetTempPath(), "dotcc-linkage-component-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try { action(directory); } finally { Directory.Delete(directory, true); }
    }
}
