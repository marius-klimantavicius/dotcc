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

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Explicit_packed_integer_backing_preserves_scalar_values_and_nested_extern_storage(bool reverse)
    {
        WithObjects(PackedZig, PackedC, paths => {
            if (reverse) Array.Reverse(paths);
            var code = Compiler.LinkObjects(paths, emit: EmitMode.ManagedLib);
            FixtureRunner.CompileAndRunCapturingStreams(code
                + "\npublic static class PackedBoundaryHost { public static int Main() => DotCcLib.answer(); }",
                [], TestContext.Current.CancellationToken).exit.ShouldBe(42);
        }, roots: ["shared_flags", "shared_style"]);
    }

    [Theory]
    [InlineData("unsigned short", "short")]
    [InlineData("unsigned short", "unsigned char")]
    public void Packed_integer_boundary_requires_exact_scalar_width_and_signedness(string before, string after)
        => WithObjects(PackedZig, PackedC.Replace(before, after, StringComparison.Ordinal), paths =>
            Should.Throw<CompileException>(() => Compiler.LinkObjects(paths, emit: EmitMode.ManagedLib)),
            roots: ["shared_flags", "shared_style"]);

    [Fact]
    public void Packed_integer_boundary_does_not_infer_a_scalar_backing_from_equal_byte_counts()
        => WithObjects(PackedZig.Replace("packed struct(u16)", "packed struct", StringComparison.Ordinal), PackedC,
            paths => Should.Throw<CompileException>(() => Compiler.LinkObjects(paths, emit: EmitMode.ManagedLib)),
            roots: ["shared_flags", "shared_style"]);

    private const string PackedZig = """
        const Flags = packed struct(u16) { bold: bool = false, underline: u3 = 0, padding: u12 = 0 };
        const Style = extern struct { flags: Flags, reserved: u16 };
        pub fn shared_flags(value: Flags, style: *Style) callconv(.c) Flags {
            _ = style;
            var result = value;
            result.bold = true;
            return result;
        }
        pub fn shared_style(style: *Style) callconv(.c) void {
            style.flags.underline = 5;
            style.reserved += 1;
        }
        """;
    private const string PackedC = """
        struct CStyle { unsigned short flags; unsigned short reserved; };
        unsigned short shared_flags(unsigned short value, struct CStyle *style);
        void shared_style(struct CStyle *style);
        int answer(void) {
            struct CStyle style = { 0xa000, 18 };
            style.flags = shared_flags(style.flags, &style);
            shared_style(&style);
            return style.flags == 0xa00b && style.reserved == 19 ? 42 : -1;
        }
        """;

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Nonexhaustive_Zig_enum_retains_full_backing_domain_without_inventing_named_members(bool reverse)
        => WithObjects(OpenEnumZig, OpenEnumC, paths => {
            if (reverse) Array.Reverse(paths);
            FixtureRunner.CompileAndRunCapturingStreams(Compiler.LinkObjects(paths, emit: EmitMode.ManagedLib)
                + "\npublic static class OpenEnumBoundaryHost { public static int Main() => DotCcLib.answer(); }",
                [], TestContext.Current.CancellationToken).exit.ShouldBe(42);
        }, roots: ["accept_mode"]);

    [Fact]
    public void Nonexhaustive_enum_boundary_still_checks_shared_named_values()
        => WithObjects(OpenEnumZig.Replace("enum(c_int) { _ }", "enum(c_int) { C_SET = 1, _ }", StringComparison.Ordinal),
            OpenEnumC.Replace("C_SET=1", "C_SET=2", StringComparison.Ordinal), paths =>
                Should.Throw<CompileException>(() => Compiler.LinkObjects(paths, emit: EmitMode.ManagedLib))
                    .Message.ShouldContain("shared enum member values"), roots: ["accept_mode"]);

    [Fact]
    public void Nonexhaustive_enum_boundary_requires_the_exact_declared_integer_backing()
        => WithObjects(OpenEnumZig.Replace("enum(c_int)", "enum(c_uint)", StringComparison.Ordinal), OpenEnumC,
            paths => Should.Throw<CompileException>(() => Compiler.LinkObjects(paths, emit: EmitMode.ManagedLib))
                .Message.ShouldContain("scalar representation"), roots: ["accept_mode"]);

    private const string OpenEnumZig = """
        const State = enum(c_int) { _ };
        pub fn accept_mode(state: State) callconv(.c) c_int {
            return if (@backingInt(state) >= 0 and @backingInt(state) <= 4) 42 else -1;
        }
        """;
    private const string OpenEnumC = """
        typedef enum { C_UNRECOGNIZED=0, C_SET=1, C_RESET=2, C_PERMANENT_SET=3,
            C_PERMANENT_RESET=4, C_STATE_MAX_VALUE=2147483647 } CState;
        int accept_mode(CState state);
        int answer(void) {
            return accept_mode(C_SET)==42 && accept_mode((CState)99)==-1 ? 42 : 1;
        }
        """;

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Reflected_union_preserves_declared_enum_integer_aliases_and_payloadless_tags(bool reverse)
        => WithObjects(IntegerUnionZig, IntegerUnionC, paths => {
            if (reverse) Array.Reverse(paths);
            FixtureRunner.CompileAndRunCapturingStreams(Compiler.LinkObjects(paths, emit: EmitMode.ManagedLib)
                + "\npublic static class IntegerUnionHost { public static int Main() => DotCcLib.answer(); }",
                [], TestContext.Current.CancellationToken).exit.ShouldBe(42);
        }, roots: ["shared_payload"]);

    [Fact]
    public void Union_enum_alias_still_requires_exact_integer_signedness()
        => WithObjects(IntegerUnionZig.Replace("underline: c_int", "underline: c_uint", StringComparison.Ordinal),
            IntegerUnionC, paths => Should.Throw<CompileException>(() => Compiler.LinkObjects(paths, emit: EmitMode.ManagedLib)),
            roots: ["shared_payload"]);

    private const string IntegerUnionZig = """
        const Value = extern union { no_data: void, underline: c_int, _padding: [2]u64 };
        pub fn shared_payload(value: Value) callconv(.c) c_int { return @intCast(value.underline); }
        """;
    private const string IntegerUnionC = """
        typedef enum { C_NONE=0, C_SINGLE=1, C_DOUBLE=2, C_CURLY=3, C_DOTTED=4,
            C_DASHED=5, C_UNDERLINE_MAX_VALUE=2147483647 } CUnderline;
        union CValue { CUnderline underline; unsigned long _padding[2]; };
        int shared_payload(union CValue value);
        int answer(void) { union CValue value={0}; value.underline=C_CURLY; return shared_payload(value)==3 ? 42 : -1; }
        """;

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Plain_char_pointer_preserves_byte_representation_and_pointee_qualifiers(bool reverse)
        => WithObjects(ByteBufferZig, ByteBufferC, paths => {
            if (reverse) Array.Reverse(paths);
            FixtureRunner.CompileAndRunCapturingStreams(Compiler.LinkObjects(paths, emit: EmitMode.ManagedLib)
                + "\npublic static class ByteBufferHost { public static int Main() => DotCcLib.answer(); }",
                [], TestContext.Current.CancellationToken).exit.ShouldBe(42);
        }, roots: ["shared_bytes"]);

    [Fact]
    public void Character_buffer_witness_does_not_remove_const_qualification()
        => WithObjects(ByteBufferZig.Replace("[*]const u8", "[*]u8", StringComparison.Ordinal), ByteBufferC,
            paths => Should.Throw<CompileException>(() => Compiler.LinkObjects(paths, emit: EmitMode.ManagedLib))
                .Message.ShouldContain("qualifiers"), roots: ["shared_bytes"]);

    [Fact]
    public void Character_buffer_witness_does_not_relax_by_value_signedness()
        => WithObjects("""
            const Status=enum(c_int){ok=0,bad=-1};
            pub fn shared_byte(value:u8) callconv(.c) Status { return if(value==127) .ok else .bad; }
            """, """
            typedef enum { C_OK=0,C_BAD=-1,C_STATUS_MAX_VALUE=2147483647 } CStatus;
            CStatus shared_byte(char value);
            int answer(void){return shared_byte(127)==C_OK?42:1;}
            """, paths => Should.Throw<CompileException>(() => Compiler.LinkObjects(paths, emit: EmitMode.ManagedLib))
                .Message.ShouldContain("scalar representation"), roots: ["shared_byte"]);

    private const string ByteBufferZig = """
        const Status = enum(c_int) { ok=0, bad=-1 };
        pub fn shared_bytes(bytes:[*]const u8,count:usize) callconv(.c) Status {
            return if(count==2 and bytes[0]==255 and bytes[1]==';') .ok else .bad;
        }
        """;
    private const string ByteBufferC = """
        typedef enum { C_OK=0,C_BAD=-1,C_STATUS_MAX_VALUE=2147483647 } CStatus;
        CStatus shared_bytes(const char *bytes,unsigned long count);
        int answer(void){return shared_bytes("\xff;",2)==C_OK?42:1;}
        """;

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
