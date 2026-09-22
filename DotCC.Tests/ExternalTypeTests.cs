using System;
using System.IO;
using System.Linq;
using DotCC.Layout;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class ExternalTypeTests
{
    private static void WithSource(string source, Action<string, string> action)
    {
        var dir = Path.Combine(Path.GetTempPath(), "dotcc-external-types-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            var path = Path.Combine(dir, "test.c");
            File.WriteAllText(path, source);
            action(path, Path.Combine(dir, "overrides.json"));
        }
        finally { Directory.Delete(dir, true); }
    }

    private static CPreprocessingOptions Options(ExternalTypeLayout? layout = null) =>
        new(Array.Empty<MacroOverride>(), externalTypes: new[] { new ExternalTypeOverride("ManagedHandle", layout) });

    [Fact]
    public void Names_are_available_without_a_typedef_and_do_not_emit_a_storage_type()
    {
        WithSource("ManagedHandle identity(ManagedHandle x) { return x; } ManagedHandle *pointer(ManagedHandle *x) { return x; }", (path, _) =>
        {
            var output = Compiler.EmitCSharp(new[] { path }, emit: EmitMode.ManagedLib, preprocessing: Options());
            output.ShouldContain("ManagedHandle identity(ManagedHandle x)");
            output.ShouldNotContain("struct ManagedHandle");
            Should.Throw<CompileException>(() => Compiler.EmitCSharp(new[] { path }, emit: EmitMode.ManagedLib));
        });
    }

    [Fact]
    public void Json_and_cli_names_merge_with_layout_and_have_stable_immutable_hashes()
    {
        WithSource("ManagedHandle pass(ManagedHandle x) { return x; }", (path, profile) =>
        {
            File.WriteAllText(profile, """{"version":1,"externalTypes":[{"name":"ManagedHandle","layout":{"size":4,"alignment":4}}]}""");
            var options = CPreprocessingOptions.Load(profile, typeNames: new[] { "ManagedHandle", "OtherHandle", "OtherHandle" });
            options.ExternalTypes.Count.ShouldBe(2);
            options.ExternalTypes[0].Layout.ShouldBe(new ExternalTypeLayout(4, 4));
            options.WithoutReport().ProfileHash.ShouldBe(options.ProfileHash);
            options.ProfileHash.ShouldNotBe(Options().ProfileHash);
            Compiler.EmitCSharp(new[] { path }, emit: EmitMode.ManagedLib, preprocessing: options).ShouldContain("ManagedHandle pass");
            Compiler.EmitDependencyRule(path, new[] { "test.o" }, includeSystemHeaders: true, preprocessing: options).ShouldContain(profile);
            var array = new[] { new ExternalTypeOverride("ManagedHandle", new(4, 4)) };
            var immutable = new CPreprocessingOptions(Array.Empty<MacroOverride>(), externalTypes: array);
            array[0] = new("OtherHandle");
            immutable.ExternalTypes[0].Name.ShouldBe("ManagedHandle");
        });
    }

    [Fact]
    public void Layout_drives_size_alignment_arrays_offsets_and_macro_constants()
    {
        WithSource("""
            #include <stddef.h>
            #define HANDLE_BYTES sizeof(ManagedHandle)
            #define HANDLE_ALIGNMENT _Alignof(ManagedHandle)
            struct Packet { char tag; ManagedHandle handles[2]; char tail; };
            _Static_assert(sizeof(ManagedHandle) == 4, "size");
            _Static_assert(_Alignof(ManagedHandle) == 4, "alignment");
            _Static_assert(sizeof(struct Packet) == 16, "packet");
            _Static_assert(offsetof(struct Packet, tail) == 12, "offset");
            int tail_offset(void) { return offsetof(struct Packet, tail); }
            """, (path, _) =>
        {
            var output = Compiler.EmitObject(path, preprocessing: Options(new(4, 4)));
            output.ShouldContain("HANDLE_BYTES = unchecked((ulong)(4UL))");
            output.ShouldContain("HANDLE_ALIGNMENT = unchecked((ulong)(4UL))");
            var document = OffsetDocument.ReadSource(output).Single();
            var model = new OffsetLayoutModel(name => document.Aggregates[name]);
            model.Aggregate("Packet").Size.ShouldBe(16);
            model.Offset("Packet", new[] { "tail" }).ShouldBe(12);
            var objectPath = Path.ChangeExtension(path, ".o");
            File.WriteAllText(objectPath, output);
            string.Join("\n", Compiler.LinkObjectFiles(new[] { objectPath }, emit: EmitMode.ManagedLib).Values).ShouldContain("public const ulong Value = 12UL");
        });
    }

    [Theory]
    [InlineData("unsigned long f(void) { return sizeof(ManagedHandle); }")]
    [InlineData("unsigned long f(void) { return _Alignof(ManagedHandle); }")]
    [InlineData("struct Packet { ManagedHandle value; }; int f(void) { return sizeof(struct Packet); }")]
    public void Missing_layout_is_diagnosed_for_layout_operations(string source)
    {
        WithSource(source, (path, _) => Should.Throw<CompileException>(() =>
            Compiler.EmitCSharp(new[] { path }, emit: EmitMode.ManagedLib, preprocessing: Options())).Message.ShouldContain("layout metadata"));
    }

    [Theory]
    [InlineData("int f(ManagedHandle value) { return value.member; }", "member")]
    [InlineData("int f(ManagedHandle *value) { return value->member; }", "member")]
    [InlineData("ManagedHandle f(void) { ManagedHandle value = {.member = 1}; return value; }", "member")]
    [InlineData("typedef int ManagedHandle;", "conflicts")]
    [InlineData("struct ManagedHandle { int value; };", "conflicts")]
    [InlineData("enum ManagedHandle { HANDLE_ZERO };", "conflicts")]
    public void External_names_cannot_silently_acquire_members_or_be_redefined(string source, string diagnostic)
    {
        WithSource(source, (path, _) => Should.Throw<CompileException>(() =>
            Compiler.EmitCSharp(new[] { path }, emit: EmitMode.ManagedLib, preprocessing: Options(new(4, 4)))).Message.ShouldContain(diagnostic));
    }

    [Theory]
    [InlineData("int")]
    [InlineData("restrict")]
    [InlineData("_Bool")]
    [InlineData("div_t")]
    [InlineData("timespec")]
    [InlineData("class")]
    [InlineData("__internal")]
    [InlineData("a.b")]
    public void Reserved_or_invalid_names_are_rejected(string name) => Should.Throw<CompileException>(() =>
        new CPreprocessingOptions(Array.Empty<MacroOverride>(), externalTypes: new[] { new ExternalTypeOverride(name) }));

    [Theory]
    [InlineData(0, 4)]
    [InlineData(4, 0)]
    [InlineData(4, 3)]
    [InlineData(3, 4)]
    [InlineData(256, 256)]
    public void Invalid_layout_is_rejected(int size, int alignment) =>
        Should.Throw<CompileException>(() => Options(new(size, alignment)));

    [Fact]
    public void Duplicate_names_cannot_silently_replace_explicit_layout()
    {
        Should.Throw<CompileException>(() => new CPreprocessingOptions(Array.Empty<MacroOverride>(), externalTypes:
            new[] { new ExternalTypeOverride("ManagedHandle", new(4, 4)), new ExternalTypeOverride("ManagedHandle", new(8, 8)) }))
            .Message.ShouldContain("conflicting externalTypes layout");
    }

    [Theory]
    [InlineData("null")]
    [InlineData("[{\"name\":\"ManagedHandle\",\"layout\":null}]")]
    [InlineData("[{\"name\":\"ManagedHandle\",\"layout\":{\"size\":4}}]")]
    [InlineData("[{\"name\":\"ManagedHandle\",\"fields\":[]}]")]
    [InlineData("[{\"name\":\"ManagedHandle\",\"layout\":{\"size\":4,\"alignment\":4,\"packing\":1}}]")]
    public void Profile_schema_is_strict(string types)
    {
        WithSource("", (_, profile) =>
        {
            File.WriteAllText(profile, "{\"version\":1,\"externalTypes\":" + types + "}");
            Should.Throw<CompileException>(() => CPreprocessingOptions.Load(profile));
        });
    }

    [Fact]
    public void Objects_reject_inconsistent_layout_contracts()
    {
        WithSource("ManagedHandle a(ManagedHandle x) { return x; }", (path, _) =>
        {
            var first = Path.ChangeExtension(path, ".o");
            File.WriteAllText(first, Compiler.EmitObject(path, preprocessing: Options(new(4, 4))));
            File.WriteAllText(path, "ManagedHandle b(ManagedHandle x) { return x; }");
            var second = Path.ChangeExtension(path, ".second.o");
            File.WriteAllText(second, Compiler.EmitObject(path, preprocessing: Options(new(8, 8))));
            Should.Throw<CompileException>(() => Compiler.LinkObjectFiles(new[] { first, second }, emit: EmitMode.ManagedLib))
                .Message.ShouldContain("conflicting external type layout");
        });
    }
}
