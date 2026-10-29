using System;
using System.IO;
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
