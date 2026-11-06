using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

[Collection("ZigFrontend")]
public sealed class ZigDirectSourceResumeTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "dotcc-source-resume-" + Guid.NewGuid().ToString("N"));
    public ZigDirectSourceResumeTests() => Directory.CreateDirectory(_directory);
    public void Dispose() => Directory.Delete(_directory, true);
    private string Write(string name, string text)
    {
        var path = Path.Combine(_directory, name);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, text);
        return path;
    }

    private ZigCompilationOptions SourceOptions(string path)
    {
        Write("lib/std/std.zig", "");
        return new("entry", [new("entry", path, Array.Empty<ZigModuleDependency>())],
            zigLibDir: Path.Combine(_directory, "lib"), stdlibTypes: ZigStdlibTypeModel.Source);
    }

    [Fact]
    public void Enum_decl_literals_reject_a_constant_of_another_enum_type()
    {
        var path = Write("main.zig", "const Other = enum { value }; const Level = enum { base, pub const alias = Other.value; }; pub fn main() u8 { const value: Level = .alias; _ = value; return 42; }");
        Should.Throw<CompileException>(() => Compiler.EmitObject(path, zigOptions: SourceOptions(path)))
            .Message.ShouldContain("expected enum Level");
    }

    [Fact]
    public void Enum_decl_literals_reject_an_integer_namespace_constant()
    {
        var path = Write("main.zig", "const Level = enum { base, pub const limit = 42; }; pub fn main() u8 { const value: Level = .limit; _ = value; return 42; }");
        Should.Throw<CompileException>(() => Compiler.EmitObject(path, zigOptions: SourceOptions(path)))
            .Message.ShouldContain("expected enum Level");
    }

    [Fact]
    public void A_file_struct_comptime_receiver_must_be_known()
    {
        Write("Source.zig", "value: u8, pub fn read(comptime self: @This()) u8 { return self.value; }");
        var path = Write("main.zig", "const Source = @import(\"Source.zig\"); fn read(value: Source) u8 { return value.read(); } pub fn main() u8 { return read(.{ .value = 42 }); }");
        Should.Throw<CompileException>(() => Compiler.EmitObject(path, zigOptions: SourceOptions(path)))
            .Message.ShouldContain("compile-time-known struct value");
    }

    [Fact]
    public void A_vector_pointer_lane_store_rejects_const_storage()
    {
        var path = Write("main.zig", "const V = @Vector(32, u32); fn write(value: *const V) void { value[31] = 42; } pub fn main() u8 { var value: V = @splat(0); write(&value); return 42; }");
        Should.Throw<CompileException>(() => Compiler.EmitObject(path, zigOptions: SourceOptions(path)))
            .Message.ShouldContain("cannot assign to constant");
    }

    [Fact]
    public void A_tuple_pointer_field_store_rejects_const_storage()
    {
        var path = Write("main.zig", "const Pair = struct { u8, u8 }; fn write(value: *const Pair) void { value[0] = 42; } pub fn main() u8 { var value: Pair = .{ 1, 2 }; write(&value); return 42; }");
        Should.Throw<CompileException>(() => Compiler.EmitObject(path, zigOptions: SourceOptions(path)))
            .Message.ShouldContain("cannot assign to constant");
    }

    [Fact]
    public void An_unnamed_enum_switch_prong_does_not_cover_a_missing_named_tag()
    {
        var path = Write("main.zig", "const Level = enum(u8) { first, second, _ }; fn convert(value: Level) u8 { return switch (value) { .first => 42, _ => 1 }; } pub fn main() u8 { return convert(.second); }");
        Should.Throw<CompileException>(() => Compiler.EmitObject(path, zigOptions: SourceOptions(path)))
            .Message.ShouldContain("does not cover named tag 'second'");
    }

    [Theory]
    [InlineData("u8", "[]const u8")]
    [InlineData("u3", "u8")]
    public void Catch_does_not_claim_arbitrary_error_union_payload_peers(string first, string second)
    {
        var path = Write("main.zig", "fn a() error{Bad}!" + first + " { return error.Bad; } fn b() error{Bad}!" + second + " { return error.Bad; } pub fn main() u8 { const value = a() catch b(); _ = value; return 42; }");
        Should.Throw<CompileException>(() => Compiler.EmitObject(path, zigOptions: SourceOptions(path)))
            .Message.ShouldContain("catch error-union fallback with a different payload type");
    }

    [Fact]
    public void A_live_error_cleanup_still_demands_its_method()
    {
        var path = Write("main.zig", "const Empty = struct {}; fn work(fail: bool) error{Bad}!u8 { const value = Empty{}; errdefer value.missingCleanup(); if (fail) return error.Bad; return 42; } pub fn main() u8 { return work(true) catch 1; }");
        Should.Throw<CompileException>(() => Compiler.EmitObject(path, zigOptions: SourceOptions(path)))
            .Message.ShouldContain("missingCleanup");
    }

    [Fact]
    public void A_failed_comptime_array_bound_does_not_become_a_size()
    {
        var path = Write("main.zig", "fn bound(n: u8) error{Bad}!u8 { _ = n; return error.Bad; } fn make(comptime n: u8) [bound(n) catch |err| @compileError(@errorName(err))]u8 { return @splat(42); } pub fn main() u8 { return make(1)[0]; }");
        Should.Throw<CompileException>(() => Compiler.EmitObject(path, zigOptions: SourceOptions(path)))
            .Message.ShouldContain("array bound");
    }

    [Fact]
    public void A_union_payload_tag_is_not_a_payload_constructor()
    {
        var path = Write("main.zig", "const Value = union(enum) { none, number: u8 }; pub fn main() u8 { const value: Value = .number; _ = value; return 42; }");
        Should.Throw<CompileException>(() => Compiler.EmitObject(path, zigOptions: SourceOptions(path)))
            .Message.ShouldContain("carries a payload");
    }

    [Fact]
    public void Zero_storage_proofs_keep_a_negative_floating_zero()
    {
        var zero = new DotCC.Ir.IrBuilder.CtFloat(0.0, DotCC.Ir.CType.Double);
        var negative = new DotCC.Ir.IrBuilder.CtFloat(BitConverter.Int64BitsToDouble(long.MinValue), DotCC.Ir.CType.Double);
        DotCC.Ir.IrBuilder.IsZero(zero).ShouldBeTrue();
        DotCC.Ir.IrBuilder.IsZero(negative).ShouldBeFalse();
        DotCC.Ir.IrBuilder.IsZero(new DotCC.Ir.IrBuilder.CtArray([negative], DotCC.Ir.CType.Double,
            new DotCC.Ir.CType.Array(DotCC.Ir.CType.Double, 1))).ShouldBeFalse();
    }

    [Theory]
    [InlineData("value == 0")]
    [InlineData("@as(u32, value) == 0")]
    public void Undefined_comptime_values_do_not_silently_become_zero(string condition)
    {
        var path = Write("main.zig", "comptime { const value: u16 = undefined; if (" + condition + ") @compileError(\"ZERO_WAS_INVENTED\"); } pub fn main() u8 { return 42; }");
        var failure = Should.Throw<CompileException>(() => Compiler.EmitObject(path, zigOptions: SourceOptions(path)));
        failure.Message.ShouldNotContain("ZERO_WAS_INVENTED");
    }

    [Fact]
    public void Demanded_type_constructor_still_reports_its_comptime_failure()
    {
        var path = Write("main.zig", "const Bad = @Int(.unsigned, @compileError(\"DEMANDED_TYPE_CONSTRUCTOR\")); pub fn main() u8 { return @typeInfo(Bad).int.bits; }");
        Should.Throw<CompileException>(() => Compiler.EmitObject(path, zigOptions: SourceOptions(path)))
            .Message.ShouldContain("DEMANDED_TYPE_CONSTRUCTOR");
    }

    [Fact]
    public void Empty_volatile_memory_clobber_emits_a_fence_instead_of_disappearing()
    {
        var path = Write("barrier.zig", "pub fn main() void { asm volatile (\"\" ::: .{ .memory = true }); }");
        Compiler.EmitCSharp([path]).ShouldContain("global::System.Threading.Thread.MemoryBarrier()");
        Compiler.EmitObject(path).ShouldContain("global::System.Threading.Thread.MemoryBarrier()");
    }

    [Theory]
    [InlineData("nop", ".{ .memory = true }")]
    [InlineData("", ".{ .memory = false }")]
    [InlineData("", ".{ .memory = true, .cc = true }")]
    public void Memory_barrier_lowering_does_not_accept_other_assembly(string template, string clobbers)
    {
        var path = Write("barrier.zig", "pub fn barrier() void { asm volatile (\"" + template + "\" ::: " + clobbers + "); }");
        Should.Throw<CompileException>(() => Compiler.EmitObject(path)).Message.ShouldContain("inline assembly");
    }

    [Fact]
    public void Conditional_module_alias_resolves_only_the_selected_import()
    {
        Write("enabled.zig", "pub const answer: u8 = 42;");
        var path = Write("main.zig", "const enabled = true; const api = if (enabled) @import(\"enabled.zig\") else @import(\"missing.zig\"); comptime { const local = api; if (local.answer != 42) @compileError(\"SELECTED_NAMESPACE_LOST\"); } pub fn main() u8 { return api.answer; }");
        var options = SourceOptions(path);
        Compiler.EmitObject(path, zigOptions: options).ShouldNotBeNullOrWhiteSpace();
        Compiler.EmitCSharp([path], zigOptions: options).ShouldNotBeNullOrWhiteSpace();
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void Source_root_poison_is_lazy_and_still_fires_when_referenced(bool objects, bool referenced)
    {
        var path = Write("main.zig", """
            const self = @This();
            comptime { if (@import("root") != self) @compileError("ROOT_IDENTITY_LOST"); }
            pub const guarded: u8 = if (false) 1 else @compileError("RELEASE_GUARD_MUST_FIRE_WHEN_USED");
            """ + "\npub fn main() u8 { return " + (referenced ? "guarded" : "42") + "; }");
        var options = SourceOptions(path);
        string Emit() => objects ? Compiler.EmitObject(path, zigOptions: options)
            : Compiler.EmitCSharp([path], zigOptions: options);
        if (referenced) Should.Throw<CompileException>(() => Emit()).Message.ShouldContain("RELEASE_GUARD_MUST_FIRE_WHEN_USED");
        else Emit().ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Generic_function_fields_reject_incompatible_comptime_parameters()
    {
        var path = Write("main.zig", """
            const Config = struct { callback: fn (comptime flag: bool, value: anytype) u8, answer: u8 = 42 };
            fn wrong(flag: bool, value: anytype) u8 { return if (flag) @intCast(value) else 0; }
            const options: Config = .{ .callback = wrong };
            pub fn main() u8 { return options.answer; }
            """);
        var options = SourceOptions(path);
        Should.Throw<CompileException>(() => Compiler.EmitObject(path, zigOptions: options))
            .Message.ShouldContain("incompatible comptime/anytype");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Deferred_module_candidates_do_not_consume_function_local_reflection(bool objects)
    {
        Write("api.zig", """
            pub const Unused = if (true) Poison() else u8;
            fn Poison() type { @compileError("UNUSED_TYPE_MUST_STAY_LAZY"); }
            pub fn Make(comptime T: type) type {
                const info = @typeInfo(T).int;
                return @Int(.unsigned, info.bits);
            }
            pub const A = Make(u3);
            pub const B = Make(u5);
            """);
        var path = Write("main.zig", "const a = @import(\"api.zig\"); pub fn main() u8 { return @bitSizeOf(a.A) + @bitSizeOf(a.B); }");
        var options = SourceOptions(path);
        var result = objects ? Compiler.EmitObject(path, zigOptions: options)
            : Compiler.EmitCSharp([path], zigOptions: options);
        result.ShouldNotBeNullOrWhiteSpace();
        result.ShouldNotContain("UNUSED_TYPE_MUST_STAY_LAZY");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Imported_enum_annotations_use_the_declaring_type(bool objects)
    {
        Write("api.zig", "pub const Mode = enum(u8) { fast, safe }; pub const mode: Mode = .fast;");
        var path = Write("main.zig", "const a = @import(\"api.zig\"); fn choose(x: a.Mode) u8 { return if (x == .fast) 42 else 0; } pub fn main() u8 { return choose(a.mode); }");
        var result = objects ? Compiler.EmitObject(path) : Compiler.EmitCSharp([path]);
        result.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Reexported_generic_types_resolve_nested_layouts_in_the_declaring_module()
    {
        Write("factory.zig", "pub fn Box(comptime T: type) type { return struct { pub const Layout = struct { value: T }; }; }");
        Write("api.zig", "const factory = @import(\"factory.zig\"); pub const Alias = factory.Box(u16);");
        var path = Write("main.zig", "const api = @import(\"api.zig\"); pub fn main() u8 { const x: api.Alias.Layout = .{ .value = 42 }; return @intCast(x.value); }");
        Compiler.EmitObject(path).ShouldNotBeNullOrWhiteSpace();
        Compiler.EmitCSharp([path]).ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Typed_optional_module_constants_preserve_their_wrapper_and_lazy_fallback()
    {
        Write("api.zig", "pub const present: ?u32 = 42; pub const absent: ?u32 = null;");
        var path = Write("main.zig", "const api = @import(\"api.zig\"); pub fn main() u8 { const p = api.present orelse @compileError(\"UNTAKEN_OPTIONAL_FALLBACK\"); const n = api.absent orelse 0; return @intCast(p + n); }");
        Compiler.EmitObject(path).ShouldNotBeNullOrWhiteSpace();
        Compiler.EmitCSharp([path]).ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Source_alias_chains_keep_nonbyte_integer_widths()
    {
        Write("sizes.zig", "pub const Id = u9;");
        Write("api.zig", "const sizes = @import(\"sizes.zig\"); pub const Id = sizes.Id;");
        var path = Write("main.zig", """
            const api = @import("api.zig");
            const Id = api.Id;
            const P = packed struct { value: Id = 0 };
            comptime { if (@bitSizeOf(P) != 9) @compileError("LOST_SOURCE_WIDTH"); }
            pub fn main() u8 { return @bitSizeOf(P); }
            """);
        var options = SourceOptions(path);
        Compiler.EmitObject(path, zigOptions: options).ShouldNotBeNullOrWhiteSpace();
        Compiler.EmitCSharp([path], zigOptions: options).ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Missing_declared_enum_type_is_not_erased_to_a_literal()
    {
        Write("api.zig", "pub const mode: MissingMode = .fast;");
        var path = Write("main.zig", "const a = @import(\"api.zig\"); pub fn main() u8 { return @intFromEnum(a.mode); }");
        Should.Throw<CompileException>(() => Compiler.EmitObject(path)).Message.ShouldContain("MissingMode");
    }
}
