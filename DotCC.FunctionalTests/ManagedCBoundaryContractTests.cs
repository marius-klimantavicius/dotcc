using System;
using System.IO;
using System.Linq;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

[Collection("ZigFrontend")]
public sealed class ManagedCBoundaryContractTests
{
    private const string Zig = """
        const Status = enum(c_int) { ok = 0, bad = -1 };
        const Kind = enum(c_int) { first = 0, second = 1 };
        const Box = extern struct { kind: Kind, value: u32 };
        const Owner = struct { value: u32 };
        const VTable = extern struct { next: *const fn (?*anyopaque, c_int) callconv(.c) c_int };
        var owner: Owner = .{ .value = 19 };
        export fn shared_box(box: Box, out: *Box) callconv(.c) Status {
            out.* = box;
            out.value += 2;
            return .ok;
        }
        export fn shared_owner() callconv(.c) *Owner { return &owner; }
        export fn shared_read(ptr: *Owner) callconv(.c) u32 { return ptr.value; }
        export fn shared_callback(table: *const VTable, value: c_int) callconv(.c) c_int { return table.next(null, value); }
        """;
    private const string C = """
        typedef enum { C_OK=0, C_BAD=-1, C_MAX_VALUE=2147483647 } CStatus;
        typedef enum { C_FIRST=0, C_SECOND=1 } CKind;
        typedef struct { CKind kind; unsigned value; } CBox;
        struct COwner;
        CStatus shared_box(CBox box, CBox *out);
        struct COwner *shared_owner(void);
        unsigned shared_read(struct COwner *owner);
        struct CVTable { int (*next)(void *, int); };
        int shared_callback(const struct CVTable *, int);
        static int next(void *ctx, int value) { (void)ctx; return value; }
        static const struct CVTable table = { next };
        int answer(void) {
            CBox a={C_SECOND,21}, b={0};
            if (shared_box(a,&b)!=C_OK || b.kind!=C_SECOND || a.value!=21) return -1;
            return (int)b.value+shared_callback(&table,(int)shared_read(shared_owner()));
        }
        """;

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Cross_language_adapters_preserve_enums_by_value_records_out_writes_and_opaque_handles(bool reverse)
        => WithObjects(Zig, C, paths => {
            if (reverse) Array.Reverse(paths);
            var code = Compiler.LinkObjects(paths, emit: EmitMode.ManagedLib);
            code.ShouldContain("__DotCcManagedCBridge_");
            var result = FixtureRunner.CompileAndRunCapturingStreams(code
                + "\npublic static class BoundaryHost { public static int Main() => DotCcLib.answer(); }", [], TestContext.Current.CancellationToken);
            result.exit.ShouldBe(42);
            result.stdout.ShouldBeEmpty(); result.stderr.ShouldBeEmpty();
        });

    [Theory]
    [InlineData("C_SECOND=1", "C_SECOND=2", "enum value domains")]
    [InlineData("unsigned value", "unsigned long value", "extent/alignment")]
    [InlineData("CKind kind; unsigned value", "unsigned value; CKind kind", "member names/offsets")]
    [InlineData("CBox *out", "CBox **out", "source type kinds")]
    public void Conflicting_cross_language_enum_aggregate_and_pointer_contracts_are_rejected(string before, string after, string diagnostic)
        => WithObjects(Zig, C.Replace(before, after, StringComparison.Ordinal), paths =>
            Should.Throw<CompileException>(() => Compiler.LinkObjects(paths, emit: EmitMode.ManagedLib)).Message.ShouldContain(diagnostic));

    [Fact]
    public void Missing_declared_shapes_cannot_be_inferred_from_generated_code()
        => WithObjects(Zig, C, paths => {
            File.WriteAllText(paths[0], string.Join("\n", File.ReadAllLines(paths[0])
                .Where(line => !line.StartsWith("//!!dotcc-obj managed-boundary-", StringComparison.Ordinal))));
            Should.Throw<CompileException>(() => Compiler.LinkObjects(paths, emit: EmitMode.ManagedLib))
                .Message.ShouldContain("witness");
        });

    [Fact]
    public void Curated_type_models_cannot_certify_the_source_boundary()
        => WithObjects(Zig, C, paths => {
            File.WriteAllText(paths[0], File.ReadAllText(paths[0]).Replace(
                ManagedBoundaryContracts.SourceProfilePrefix + "source", ManagedBoundaryContracts.SourceProfilePrefix + "curated", StringComparison.Ordinal));
            Should.Throw<CompileException>(() => Compiler.LinkObjects(paths, emit: EmitMode.ManagedLib))
                .Message.ShouldContain("requires source stdlib type witnesses");
        });

    [Fact]
    public void Separate_adapters_cannot_erase_two_C_callers_original_nominal_obligations()
        => WithObjects(Zig, C, paths => {
            string source = Path.Combine(Path.GetDirectoryName(paths[1])!, "other.c");
            File.WriteAllText(source, """
                typedef enum { C_OK=0, C_BAD=-1, C_MAX_VALUE=2147483647 } CStatus;
                typedef enum { C_FIRST=0, C_SECOND=1 } CKind;
                typedef struct { CKind kind; unsigned value; } DifferentBox;
                CStatus shared_box(DifferentBox box, DifferentBox *out);
                int other_answer(void) { DifferentBox value={0}; return shared_box(value,&value); }
                """);
            string obj = source + ".csobj";
            File.WriteAllText(obj, Compiler.EmitObject(source));
            Should.Throw<CompileException>(() => Compiler.LinkObjects([..paths, obj], emit: EmitMode.ManagedLib))
                .Message.ShouldContain("conflicting external type contracts");
        });

    [Fact]
    public void Declared_shape_metadata_is_bound_to_existing_nominal_identity_hash()
        => WithObjects(Zig, C, paths => {
            string text = File.ReadAllText(paths[0]);
            string prefix = "//!!dotcc-obj managed-boundary-type:";
            string line = text.Split('\n').First(l => l.StartsWith(prefix, StringComparison.Ordinal));
            string changed = line[..(line.LastIndexOf(' ') + 1)] + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("struct|0|0|0"));
            File.WriteAllText(paths[0], text.Replace(line, changed, StringComparison.Ordinal));
            Should.Throw<CompileException>(() => Compiler.LinkObjects(paths, emit: EmitMode.ManagedLib))
                .Message.ShouldContain("do not match aggregate identity proof");
        });

    [Fact]
    public void Cross_language_conversion_is_not_enabled_for_native_library_output()
        => WithObjects(Zig, C, paths =>
            Should.Throw<CompileException>(() => Compiler.LinkObjects(paths, emit: EmitMode.SharedLib))
                .Message.ShouldContain("requires static managed-library output"));

    [Fact]
    public void Shared_enum_member_names_cannot_be_permuted_with_the_same_value_domain()
        => WithObjects(Zig.Replace("first = 0, second = 1", "C_FIRST = 0, C_SECOND = 1", StringComparison.Ordinal),
            C.Replace("C_FIRST=0, C_SECOND=1", "C_FIRST=1, C_SECOND=0", StringComparison.Ordinal), paths =>
                Should.Throw<CompileException>(() => Compiler.LinkObjects(paths, emit: EmitMode.ManagedLib))
                    .Message.ShouldContain("shared enum member values"));

    [Fact]
    public void Explicit_pointer_alignment_is_not_silently_erased_from_boundary_signatures()
        => WithObjects(Zig.Replace("out: *Box", "out: *align(16) Box", StringComparison.Ordinal), C, paths =>
            Should.Throw<CompileException>(() => Compiler.LinkObjects(paths, emit: EmitMode.ManagedLib))
                .Message.ShouldContain("unsupported managed boundary signature witness"));

    [Fact]
    public void Sentinel_array_storage_is_not_inferred_from_older_logical_count_descriptors()
        => WithObjects(Zig.Replace("export fn", "pub fn", StringComparison.Ordinal)
                .Replace("const Box = extern struct { kind: Kind, value: u32 };",
                    "const Box = extern struct { kind: Kind, value: u32, reserved: [2:0]u8 };", StringComparison.Ordinal),
            C.Replace("CKind kind; unsigned value;", "CKind kind; unsigned value; unsigned char reserved[2];", StringComparison.Ordinal), paths =>
                Should.Throw<CompileException>(() => Compiler.LinkObjects(paths, emit: EmitMode.ManagedLib))
                    .Message.ShouldContain("missing declared shape witness"),
            roots: ["shared_box", "shared_owner", "shared_read", "shared_callback"]);

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Reflected_extern_union_aliases_preserve_each_distinct_alternative(bool conflict)
    {
        string zig = """
            const Coordinate = extern struct { x: u16, y: u32 };
            const Value = extern union { active: Coordinate, viewport: Coordinate, _padding: [2]u64 };
            const Point = extern struct { tag: c_int, value: Value };
            pub fn shared_point(point: Point) callconv(.c) c_int { return @intCast(point.value.active.x + point.value.active.y); }
            """;
        string c = """
            struct CCoordinate { unsigned short x; unsigned y; };
            union CValue { struct CCoordinate coordinate; unsigned long _padding[2]; };
            struct CPoint { int tag; union CValue value; };
            int shared_point(struct CPoint);
            int answer(void) { struct CPoint p={0}; p.value.coordinate.x=19; p.value.coordinate.y=23; return shared_point(p); }
            """;
        if (conflict) c = c.Replace("union CValue {", "union CValue { double extra;", StringComparison.Ordinal);
        WithObjects(zig, c, paths => {
            if (conflict) Should.Throw<CompileException>(() => Compiler.LinkObjects(paths, emit: EmitMode.ManagedLib))
                .Message.ShouldContain("different union alternatives");
            else FixtureRunner.CompileAndRunCapturingStreams(Compiler.LinkObjects(paths, emit: EmitMode.ManagedLib)
                + "\npublic static class UnionHost { public static int Main() => DotCcLib.answer(); }", [], TestContext.Current.CancellationToken).exit.ShouldBe(42);
        }, roots: ["shared_point"]);
    }

    private static void WithObjects(string zig, string c, Action<string[]> test, string[]? roots = null)
    {
        var library = Environment.GetEnvironmentVariable("DOTCC_TARGET_ZIG_LIB");
        if (string.IsNullOrEmpty(library)) Assert.Skip("Set DOTCC_TARGET_ZIG_LIB to the pinned source standard library.");
        string directory = Path.Combine(Path.GetTempPath(), "managed-c-boundary-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            string zp = Path.Combine(directory, "engine.zig"), cp = Path.Combine(directory, "boundary.c");
            if (roots is not null) zig += "\ncomptime {\n" + string.Concat(roots.Select(name =>
                "    @export(&" + name + ", .{ .name = \"" + name + "\" });\n")) + "}\n";
            File.WriteAllText(zp, zig); File.WriteAllText(cp, c);
            var options = new ZigCompilationOptions("root", [new("root", zp, [])], outputMode: ZigSourceOutputMode.Lib,
                zigLibDir: library, stdlibTypes: ZigStdlibTypeModel.Source,
                functionPointerAbi: ZigFunctionPointerAbi.Managed, managedRoots: roots);
            string zo = zp + ".csobj", co = cp + ".csobj";
            File.WriteAllText(zo, Compiler.EmitObject(zp, zigOptions: options));
            File.WriteAllText(co, Compiler.EmitObject(cp));
            test([zo, co]);
        }
        finally { Directory.Delete(directory, recursive: true); }
    }
}
