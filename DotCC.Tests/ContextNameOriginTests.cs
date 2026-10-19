using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class ContextNameOriginTests
{
    private static CSharpOutputOptions Options(bool instance) =>
        new(Runtime: RuntimeProfile.C, StateContext: !instance, InstanceMethods: instance);

    [Theory]
    [InlineData("int __DotCcUser(void) { return 1; }")]
    [InlineData("int __DotCcUser = 1; int probe(void) { return __DotCcUser; }")]
    [InlineData("struct __DotCcUser { int value; }; int probe(struct __DotCcUser *p) { return p->value; }")]
    [InlineData("typedef struct { int value; } __DotCcUser; int probe(__DotCcUser *p) { return p->value; }")]
    [InlineData("typedef union { int value; } __DotCcUser; int probe(__DotCcUser *p) { return p->value; }")]
    [InlineData("struct __DotCcTypedef_AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA { int value; }; int probe(struct __DotCcTypedef_AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA *p) { return p->value; }")]
    public void Authored_reserved_names_remain_rejected_direct_and_object(string source)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-context-origin-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var path = Path.Combine(directory, "unit.c");
            var obj = Path.Combine(directory, "unit.o");
            File.WriteAllText(path, source);
            foreach (bool instance in new[] { false, true })
            {
                var options = Options(instance);
                Should.Throw<CompileException>(() => Compiler.EmitCSharp(new[] { path },
                    emit: EmitMode.ManagedLib, outputOptions: options)).Message.ShouldContain("reserves the __DotCc");
                File.WriteAllText(obj, Compiler.EmitObject(path, outputOptions: new(InstanceMethods: instance)));
                Should.Throw<CompileException>(() => Compiler.LinkObjects(new[] { obj },
                    emit: EmitMode.ManagedLib, outputOptions: options)).Message.ShouldContain("reserves the __DotCc");
            }
        }
        finally { Directory.Delete(directory, true); }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Explicit_reserved_owner_is_not_exempted_by_generated_types(bool instance)
    {
        var path = Path.Combine(Path.GetTempPath(), "dotcc-context-owner-" + Guid.NewGuid().ToString("N") + ".c");
        var obj = path + ".o";
        File.WriteAllText(path, "typedef struct { int value; } Cell; int probe(Cell *p) { return p->value; }");
        try
        {
            Should.Throw<CompileException>(() => Compiler.EmitCSharp(new[] { path }, emit: EmitMode.ManagedLib,
                className: "__DotCcUser", outputOptions: Options(instance))).Message.ShouldContain("reserves the __DotCc");
            File.WriteAllText(obj, Compiler.EmitObject(path, outputOptions: new(InstanceMethods: instance)));
            Should.Throw<CompileException>(() => Compiler.LinkObjects(new[] { obj }, emit: EmitMode.ManagedLib,
                className: "__DotCcUser", outputOptions: Options(instance))).Message.ShouldContain("reserves the __DotCc");
        }
        finally { File.Delete(path); File.Delete(obj); }
    }

    [Fact]
    public void Anonymous_alias_without_aggregate_origin_does_not_grant_an_exemption()
    {
        var path = Path.Combine(Path.GetTempPath(), "dotcc-context-metadata-" + Guid.NewGuid().ToString("N") + ".c");
        var obj = path + ".o";
        File.WriteAllText(path, "typedef struct { int value; } Cell; int probe(Cell *p) { return p->value; }");
        try
        {
            var text = Compiler.EmitObject(path, outputOptions: new(InstanceMethods: true));
            text = string.Join('\n', text.Split('\n').Where(line =>
                line != AggregateIdentities.Version && !line.StartsWith(AggregateIdentities.Prefix, StringComparison.Ordinal)));
            File.WriteAllText(obj, text);
            // Existing object-origin validation remains authoritative; merely
            // carrying an anonymous-typedef alias cannot bless an identity.
            Should.Throw<CompileException>(() => Compiler.LinkObjects(new[] { obj }, emit: EmitMode.ManagedLib,
                outputOptions: Options(true))).Message.ShouldContain("aggregate identity metadata");
        }
        finally { File.Delete(path); File.Delete(obj); }
    }
}
