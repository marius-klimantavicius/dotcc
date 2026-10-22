using System;
using System.IO;
using System.Linq;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

public sealed class EnumIdentityLinkTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void Independent_parser_enums_keep_every_original_member_and_value(bool objects, bool reverse)
        => WithSources(objects, reverse, [
            "int first(void); int second(void); int main(void) { return first()+second()==42 ? 0 : 1; }",
            "enum yytokentype { SHARED=7, FIRST=3 }; static enum yytokentype first_token=SHARED; int first(void) { return first_token+FIRST; }",
            "enum yytokentype { SHARED=19, SECOND=13 }; static enum yytokentype second_token=SHARED; int second(void) { return second_token+SECOND; }"
        ], code => FixtureRunner.CompileAndRunCapturingExit(code, Array.Empty<string>()).exit.ShouldBe(0));

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void Enum_edges_partition_private_parent_layouts_and_initializers(bool objects, bool reverse)
        => WithSources(objects, reverse, [
            "int first(void); int second(void); int main(void) { return first()+second()==42 ? 0 : 1; }",
            "enum Token { VALUE=19 }; struct Box { enum Token token; }; static struct Box first_box={VALUE}; int first(void) { return first_box.token; }",
            "enum Token { VALUE=23 }; struct Box { enum Token token; }; static struct Box second_box={VALUE}; int second(void) { return second_box.token; }"
        ], code => FixtureRunner.CompileAndRunCapturingExit(code, Array.Empty<string>()).exit.ShouldBe(0));

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Anonymous_enum_typedef_aliases_have_declaration_identity(bool objects)
        => WithSources(objects, false, [
            "int first(void); int second(void); int main(void) { return first()+second()==42 ? 0 : 1; }",
            "typedef enum { VALUE=19, FIRST=3 } Token; static Token first_token=VALUE; int first(void) { return first_token+FIRST-FIRST; }",
            "typedef enum { VALUE=23, SECOND=7 } Token; static Token second_token=VALUE; int second(void) { return second_token+SECOND-SECOND; }"
        ], code => FixtureRunner.CompileAndRunCapturingExit(code, Array.Empty<string>()).exit.ShouldBe(0));

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Enum_identity_preserves_escaped_type_names_and_member_aliases(bool objects)
        => WithSources(objects, false, [
            "enum event { A=19, B=23 }; enum event roundtrip(enum event); int main(void) { return roundtrip(A)+roundtrip(B)==42 ? 0 : 1; }",
            "enum event { A=19, B=23 }; enum event roundtrip(enum event t) { return t; }"
        ], code => FixtureRunner.CompileAndRunCapturingExit(code, Array.Empty<string>()).exit.ShouldBe(0));

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void Compatible_shared_enum_contracts_ignore_declaration_order_but_preserve_values(bool objects, bool reverse)
        => WithSources(objects, reverse, [
            "enum Token { A=19, B=23 }; enum Token roundtrip(enum Token); int main(void) { return roundtrip(A)+roundtrip(B)==42 ? 0 : 1; }",
            "enum Token { B=23, A=19 }; enum Token roundtrip(enum Token t) { return t; }"
        ], code => FixtureRunner.CompileAndRunCapturingExit(code, Array.Empty<string>()).exit.ShouldBe(0));

    [Theory]
    [InlineData(false, "extern enum Token shared;")]
    [InlineData(true, "extern enum Token shared;")]
    [InlineData(false, "int shared(enum Token);")]
    [InlineData(true, "int shared(enum Token);")]
    [InlineData(false, "extern int (*shared)(enum Token);")]
    [InlineData(true, "extern int (*shared)(enum Token);")]
    [InlineData(false, "struct Box { enum Token token; }; extern struct Box *shared;")]
    [InlineData(true, "struct Box { enum Token token; }; extern struct Box *shared;")]
    public void Unused_shared_enum_objects_prototypes_callbacks_and_parent_fields_remain_strong(bool objects, string declaration)
    {
        WithDirectory(directory =>
        {
            var paths = Sources(directory, ["enum Token { VALUE=19 }; " + declaration + " int main(void) { return 0; }",
                "enum Token { VALUE=23 }; " + declaration]);
            if (objects) paths = Objects(paths);
            Should.Throw<CompileException>(() => Emit(paths, objects)).Message.ShouldContain("conflicting aggregate declarations for 'Token'");
        });
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Fixed_underlying_types_are_part_of_the_shared_enum_contract(bool objects)
    {
        WithDirectory(directory =>
        {
            var paths = Sources(directory, [
                "enum Token : unsigned char { VALUE=7 }; int shared(enum Token); int main(void) { return 0; }",
                "enum Token : unsigned long { VALUE=7 }; int shared(enum Token);"
            ]);
            if (objects) paths = paths.Select(path => {
                string obj = path + ".cs"; File.WriteAllText(obj, Compiler.EmitObject(path, dialect: new CDialect(2023))); return obj;
            }).ToArray();
            Should.Throw<CompileException>(() => objects ? Compiler.LinkObjects(paths) : Compiler.EmitCSharp(paths, dialect: new CDialect(2023)))
                .Message.ShouldContain("conflicting external type contracts");
        });
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Authored_reserved_enum_tags_do_not_gain_an_identity_exemption(bool objects)
    {
        WithDirectory(directory =>
        {
            var paths = Sources(directory, ["enum __DotCcUser { VALUE=19 }; int first(void) { return VALUE; }",
                "enum __DotCcUser { VALUE=23 }; int second(void) { return VALUE; }"]);
            if (objects) paths = Objects(paths);
            var options = new CSharpOutputOptions(Runtime: RuntimeProfile.C, StateContext: true, NestTypes: true);
            Should.Throw<CompileException>(() => objects
                ? Compiler.LinkObjects(paths, emit: EmitMode.ManagedLib, outputOptions: options)
                : Compiler.EmitCSharp(paths, emit: EmitMode.ManagedLib, outputOptions: options))
                .Message.ShouldContain("reserves the __DotCc");
        });
    }

    [Fact]
    public void Legacy_enum_objects_require_identity_regeneration()
    {
        WithDirectory(directory =>
        {
            var paths = Objects(Sources(directory, ["enum Token { VALUE=19 }; int main(void) { return VALUE==19 ? 0 : 1; }"]));
            File.WriteAllText(paths[0], File.ReadAllText(paths[0]).Replace(AggregateIdentities.LinkageVersion,
                "//!!dotcc-obj aggregate-linkage:2", StringComparison.Ordinal));
            Should.Throw<CompileException>(() => Compiler.LinkObjects(paths)).Message.ShouldContain("enum identity metadata");
        });
    }

    private static string Emit(string[] paths, bool objects) => objects ? Compiler.LinkObjects(paths) : Compiler.EmitCSharp(paths);
    private static void WithSources(bool objects, bool reverse, string[] sources, Action<string> check)
        => WithDirectory(directory => {
            var paths = Sources(directory, sources);
            if (objects) paths = Objects(paths);
            if (reverse) Array.Reverse(paths);
            check(Emit(paths, objects));
        });
    private static string[] Sources(string directory, string[] sources) => sources.Select((source, index) => {
        string path = Path.Combine(directory, "enum-unit" + index + ".c"); File.WriteAllText(path, source); return path;
    }).ToArray();
    private static string[] Objects(string[] paths) => paths.Select(path => {
        string obj = path + ".cs"; File.WriteAllText(obj, Compiler.EmitObject(path)); return obj;
    }).ToArray();
    private static void WithDirectory(Action<string> action)
    {
        string directory = Path.Combine(Path.GetTempPath(), "dotcc-enum-identity-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try { action(directory); } finally { Directory.Delete(directory, true); }
    }
}
