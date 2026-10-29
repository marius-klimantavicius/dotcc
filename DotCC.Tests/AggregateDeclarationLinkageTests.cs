using System;
using System.IO;
using System.Linq;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class AggregateDeclarationLinkageTests
{
    [Fact]
    public void Typeof_prototype_for_indirect_call_keeps_its_TU_types_without_linking_the_declared_symbol()
    {
        WithObjects("struct Engine; int external(struct Engine *p) { return p != 0; }", """
            struct Header;
            int external(struct Header *p);
            void *get_api(int operation);
            int bridge(void *p) { return ((__typeof__(&external))get_api(0))(p); }
            """, objects =>
        {
            var graph = AggregateIdentities.Read(File.ReadAllText(objects[1])).ShouldBeOfType<AggregateIdentitySet>();
            graph.Linkage.ShouldNotContain(root => root.Symbol == "external");
            graph.ShouldContain(node => node.Name == "Header");
            var linked = Compiler.LinkObjects(objects, emit: EmitMode.ManagedLib);
            linked.ShouldContain("delegate*<");
            linked.ShouldContain("get_api(0)");
        });
    }

    [Theory]
    [InlineData("int bridge(void *p) { return external(p); }")]
    [InlineData("void *bridge(void) { return (void *)&external; }")]
    [InlineData("void *bridge(void) { return (void *)external; }")]
    [InlineData("int (*saved)(struct Header *) = external;")]
    public void Actual_calls_and_function_addresses_keep_external_type_checks(string use)
    {
        WithObjects("struct Engine; int external(struct Engine *p) { return p != 0; }",
            "struct Header; int external(struct Header *p); " + use, objects =>
        {
            var error = Should.Throw<CompileException>(() => Compiler.LinkObjects(objects, emit: EmitMode.ManagedLib));
            error.Message.ShouldContain("conflicting external type contracts for 'external'");
        });
    }

    [Fact]
    public void Actual_variadic_calls_keep_checks_even_though_they_are_not_native_import_candidates()
    {
        WithObjects("struct Engine; int external(struct Engine *p, ...) { return p != 0; }", """
            struct Header;
            int external(struct Header *p, ...);
            int bridge(void *p) { return external(p, 7); }
            """, objects =>
        {
            var error = Should.Throw<CompileException>(() => Compiler.LinkObjects(objects, emit: EmitMode.ManagedLib));
            error.Message.ShouldContain("conflicting external type contracts for 'external'");
        });
    }

    [Theory]
    [InlineData("int bridge(void) { return shared.value; }")]
    [InlineData("void *bridge(void) { return &shared; }")]
    [InlineData("int bridge(void) { return 0; }")]
    public void External_objects_keep_conservative_contracts_including_unused_declarations(string use)
    {
        WithObjects("struct Engine { int value; }; struct Engine shared;",
            "struct Header { long value; }; extern struct Header shared; " + use, objects =>
        {
            var error = Should.Throw<CompileException>(() => Compiler.LinkObjects(objects, emit: EmitMode.ManagedLib));
            error.Message.ShouldContain("conflicting external type contracts for 'shared'");
        });
    }

    [Fact]
    public void Typeof_indirect_boundary_preserves_distinct_enum_and_opaque_struct_names()
    {
        WithObjects("""
            enum EngineResult { OK=0, FAILED=1 };
            struct EngineAllocator;
            enum EngineResult external(struct EngineAllocator *p) { return p ? OK : FAILED; }
            """, """
            enum HeaderResult { SUCCESS=0, FAILURE=1 };
            struct HeaderAllocator;
            enum HeaderResult external(struct HeaderAllocator *p);
            void *get_api(int operation);
            int bridge(void *p) { return ((__typeof__(&external))get_api(0))(p); }
            """, objects =>
        {
            var graph = AggregateIdentities.Read(File.ReadAllText(objects[1])).ShouldBeOfType<AggregateIdentitySet>();
            graph.Linkage.ShouldNotContain(root => root.Symbol == "external");
            graph.Declarations.ShouldContain(c => c.Symbol == "external" && c.References.SequenceEqual(new[] { "HeaderResult" }));
            graph.Single(n => n.Name == "HeaderResult").Exposed.ShouldBeTrue();
            graph.Single(n => n.Name == "HeaderAllocator").Exposed.ShouldBeTrue();
            Compiler.LinkObjects(objects, emit: EmitMode.ManagedLib).ShouldContain("get_api(0)");
        });
    }

    [Theory]
    [InlineData("int bridge(void *p) { return external(p); }")]
    [InlineData("void *bridge(void) { return (void *)&external; }")]
    [InlineData("void *bridge(void) { return (void *)external; }")]
    [InlineData("enum HeaderResult (*saved)(struct HeaderAllocator *) = external;")]
    public void Enum_returning_function_calls_and_addresses_keep_the_entire_signature_contract(string use)
    {
        WithObjects("""
            enum EngineResult { OK=0 }; struct EngineAllocator;
            enum EngineResult external(struct EngineAllocator *p) { return OK; }
            """, """
            enum HeaderResult { SUCCESS=0 }; struct HeaderAllocator;
            enum HeaderResult external(struct HeaderAllocator *p);
            """ + use, objects =>
        {
            Should.Throw<CompileException>(() => Compiler.LinkObjects(objects, emit: EmitMode.ManagedLib))
                .Message.ShouldContain("conflicting external type contracts for 'external'");
        });
    }

    [Fact]
    public void Actual_runtime_calls_keep_the_entire_signature_contract()
    {
        WithObjects("struct Engine; void free(struct Engine *p) { }",
            "struct Header; void free(struct Header *p); void bridge(void *p) { free(p); }", objects =>
        {
            Should.Throw<CompileException>(() => Compiler.LinkObjects(objects, emit: EmitMode.ManagedLib))
                .Message.ShouldContain("conflicting external type contracts for 'free'");
        });
    }

    [Theory]
    [InlineData("int external(enum Token);")]
    [InlineData("int external(int (*callback)(enum Token));")]
    [InlineData("struct Box { enum Token token; struct Box *next; }; int external(struct Box *);")]
    public void Unused_function_declarations_preserve_shared_enum_values_through_typed_edges(string declaration)
    {
        WithObjects("enum Token { VALUE=19 }; " + declaration,
            "enum Token { VALUE=23 }; " + declaration, objects =>
        {
            Should.Throw<CompileException>(() => Compiler.LinkObjects(objects, emit: EmitMode.ManagedLib))
                .Message.ShouldContain("conflicting aggregate declarations for 'Token'");
        });
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Shared_enum_declarations_constrain_definitions_in_either_object_order(bool reverse)
    {
        WithObjects("enum Token { VALUE=19 }; int external(enum Token t) { return t; }",
            "enum Token { VALUE=23 }; int external(enum Token);", objects =>
        {
            if (reverse) Array.Reverse(objects);
            Should.Throw<CompileException>(() => Compiler.LinkObjects(objects, emit: EmitMode.ManagedLib))
                .Message.ShouldContain("conflicting aggregate declarations for 'Token'");
        });
    }

    [Fact]
    public void Shared_enum_declarations_do_not_bind_differently_named_unused_containers()
    {
        WithObjects("enum Token { VALUE=7 }; struct EngineBox { enum Token token; int value; }; int external(struct EngineBox *);",
            "enum Token { VALUE=7 }; struct HeaderBox { enum Token token; long value; }; int external(struct HeaderBox *);", objects =>
        {
            var graph = AggregateIdentities.Read(File.ReadAllText(objects[0])).ShouldBeOfType<AggregateIdentitySet>();
            graph.Single(n => n.Name == "Token").Exposed.ShouldBeTrue();
            graph.Linkage.ShouldNotContain(c => c.Symbol == "external");
            Compiler.LinkObjects(objects, emit: EmitMode.ManagedLib).ShouldContain("HeaderBox");
        });
    }

    [Fact]
    public void Unrelated_function_declarations_do_not_give_enum_tags_linkage()
    {
        WithObjects("enum Token { VALUE=19 }; int first(enum Token);",
            "enum Token { VALUE=23 }; int second(enum Token);", objects =>
        {
            Compiler.LinkObjects(objects, emit: EmitMode.ManagedLib).ShouldContain("__DotCcPrivate_");
        });
    }

    [Fact]
    public void Removing_unused_enum_declaration_evidence_cannot_bypass_exposure_validation()
    {
        WithObjects("enum Token { VALUE=19 }; int external(enum Token);", "int other(void) { return 0; }", objects =>
        {
            File.WriteAllLines(objects[0], File.ReadAllLines(objects[0])
                .Where(line => !line.StartsWith(AggregateIdentities.DeclarationPrefix, StringComparison.Ordinal)));
            Should.Throw<CompileException>(() => Compiler.LinkObjects(objects, emit: EmitMode.ManagedLib))
                .Message.ShouldContain("incomplete aggregate linkage roots");
        });
    }

    [Fact]
    public void Version_three_enum_objects_require_regeneration_for_unused_declaration_evidence()
    {
        WithObjects("enum Token { VALUE=19 }; int external(enum Token);", "int other(void) { return 0; }", objects =>
        {
            File.WriteAllLines(objects[0], File.ReadAllLines(objects[0])
                .Where(line => !line.StartsWith(AggregateIdentities.DeclarationPrefix, StringComparison.Ordinal))
                .Select(line => line == AggregateIdentities.LinkageVersion ? "//!!dotcc-obj aggregate-linkage:3" : line));
            Should.Throw<CompileException>(() => Compiler.LinkObjects(objects, emit: EmitMode.ManagedLib))
                .Message.ShouldContain("enum identity metadata");
        });
    }

    [Fact]
    public void Version_three_non_enum_objects_require_regeneration_for_unused_declarations()
    {
        WithObjects("struct State { int value; }; int first(struct State *p) { return p != 0; }",
            "struct State { long value; }; int second(struct State *p) { return p != 0; }", objects =>
        {
            foreach (var path in objects)
                File.WriteAllText(path, File.ReadAllText(path).Replace(AggregateIdentities.LinkageVersion,
                    "//!!dotcc-obj aggregate-linkage:3", StringComparison.Ordinal));
            Should.Throw<CompileException>(() => Compiler.LinkObjects(objects, emit: EmitMode.ManagedLib))
                .Message.ShouldContain("aggregate declaration metadata");
        });
    }

    private static void WithObjects(string provider, string consumer, Action<string[]> check)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-typeonly-linkage-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var objects = new string[2];
            var sources = new[] { provider, consumer };
            for (int i = 0; i < sources.Length; i++)
            {
                var source = Path.Combine(directory, "unit" + i + ".c");
                File.WriteAllText(source, sources[i]);
                objects[i] = Path.ChangeExtension(source, ".obj.cs");
                File.WriteAllText(objects[i], Compiler.EmitObject(source));
            }
            check(objects);
        }
        finally { Directory.Delete(directory, true); }
    }
}
