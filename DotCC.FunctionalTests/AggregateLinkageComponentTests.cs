using System;
using System.IO;
using System.Linq;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

public sealed class AggregateLinkageComponentTests
{
    [Theory]
    [InlineData(false, false, false)]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(true, true, false)]
    [InlineData(false, false, true)]
    [InlineData(true, false, true)]
    [InlineData(false, true, true)]
    [InlineData(true, true, true)]
    public void Shared_container_retains_compiler_state_identity_separate_from_executor_state(bool objects, bool scoped, bool reverse)
    {
        WithDirectory(directory =>
        {
            string compile = scoped ? "RegCompVars" : "vars", execute = scoped ? "RegExecVars" : "vars";
            string header = "struct Map { struct " + compile + " *v; int value; }; struct Map *make(void); int color(struct Map *); int consume(struct Map *);\n";
            var paths = Sources(directory,
                header + "int main(void) { return consume(make()) == 42 ? 0 : 1; }",
                header + "struct " + compile + " { int error; }; static struct " + compile + " state = {19}; static struct Map map = {&state,23}; struct Map *make(void) { return &map; } int color(struct Map *p) { return p->v->error+p->value; }",
                header + "struct " + execute + " { long inputs[3]; }; int consume(struct Map *p) { struct " + execute + " local = {{1,2,3}}; return color(p)+(int)(local.inputs[0]+local.inputs[1]+local.inputs[2])-6; }");
            if (objects) paths = Objects(paths);
            if (reverse) Array.Reverse(paths);
            if (!scoped)
            {
                Should.Throw<CompileException>(() => { if (objects) Compiler.LinkObjects(paths); else Compiler.EmitCSharp(paths); })
                    .Message.ShouldContain("conflicting aggregate declarations for 'vars'");
            }
            else
            {
                string code = objects ? Compiler.LinkObjects(paths) : Compiler.EmitCSharp(paths);
                FixtureRunner.CompileAndRunCapturingExit(code, Array.Empty<string>()).exit.ShouldBe(0);
            }
        });
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void Forward_union_keeps_its_kind_and_completes_across_units(bool objects, bool reverse)
    {
        WithDirectory(directory =>
        {
            var paths = Sources(directory,
                "union Value; union Value *make(void); int read(union Value *); int main(void) { return read(make()) == 37 ? 0 : 1; }",
                "union Value { long value; char bytes[8]; }; static union Value value = {37}; union Value *make(void) { return &value; } int read(union Value *p) { return (int)p->value; }");
            if (objects)
            {
                paths = Objects(paths);
                var shape = AggregateIdentities.Read(File.ReadAllText(paths[0]))!.Single(n => n.Name == "Value").Shape;
                shape.IsIncomplete.ShouldBeTrue();
                shape.IsUnion.ShouldBeTrue();
            }
            if (reverse) Array.Reverse(paths);
            string code = objects ? Compiler.LinkObjects(paths) : Compiler.EmitCSharp(paths);
            FixtureRunner.CompileAndRunCapturingExit(code, Array.Empty<string>()).exit.ShouldBe(0);
        });
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Forward_union_cannot_complete_as_struct_in_a_shared_contract(bool objects)
    {
        WithDirectory(directory =>
        {
            var paths = Sources(directory,
                "union Value; int shared(union Value *); int main(void) { return 0; }",
                "struct Value { long value; }; int shared(struct Value *);");
            if (objects) paths = Objects(paths);
            Should.Throw<CompileException>(() => { if (objects) Compiler.LinkObjects(paths); else Compiler.EmitCSharp(paths); })
                .Message.ShouldContain("conflicting aggregate declarations for 'Value'");
        });
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void Parser_semantic_namespace_preserves_real_unused_external_contract_checks(bool objects, bool scoped)
    {
        WithDirectory(directory =>
        {
            string sync = scoped ? "SyncRepYYSTYPE" : "YYSTYPE";
            string replication = scoped ? "ReplicationYYSTYPE" : "YYSTYPE";
            // The real replication interface includes the synchronous parser
            // interface before completing its own generated semantic union.
            // An unused declaration is still a shared C contract.
            string api = "union " + sync + "; int syncrep_yylex(union " + sync + " *);\n";
            string generated(string tag, string fields) =>
                "#if !defined " + tag + " && !defined YYSTYPE_IS_DECLARED\nunion " + tag + " { " + fields + " }; typedef union " + tag + " " + tag + ";\n#define YYSTYPE_IS_DECLARED 1\n#endif\n";
            var paths = Sources(directory,
                api + generated(replication, "long recptr; void *node;") +
                    "int main(void) { " + replication + " v = {37}; return v.recptr == 37 ? 0 : 1; }",
                api + generated(sync, "char *str; void *config;") +
                    "int syncrep_yylex(union " + sync + " *p) { return p == 0; }");
            if (objects) paths = Objects(paths);
            if (!scoped)
            {
                Should.Throw<CompileException>(() => { if (objects) Compiler.LinkObjects(paths); else Compiler.EmitCSharp(paths); })
                    .Message.ShouldContain("conflicting aggregate declarations for 'YYSTYPE'");
            }
            else
            {
                string code = objects ? Compiler.LinkObjects(paths) : Compiler.EmitCSharp(paths);
                FixtureRunner.CompileAndRunCapturingExit(code, Array.Empty<string>()).exit.ShouldBe(0);
            }
        });
    }

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
