using System;
using System.IO;
using System.Runtime.CompilerServices;
using DotCC.Ir;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

[Collection("ZigFrontend")]
public sealed class ZigStaticPointerExecutionTests
{
    [Theory]
    [InlineData(false, "identity.zig")]
    [InlineData(true, "identity.zig")]
    [InlineData(false, "testing-allocator.zig")]
    [InlineData(true, "testing-allocator.zig")]
    [InlineData(false, "optional-pointee.zig")]
    [InlineData(true, "optional-pointee.zig")]
    [InlineData(false, "conditional-enum.zig")]
    [InlineData(true, "conditional-enum.zig")]
    [InlineData(false, "array-field-order.zig")]
    [InlineData(true, "array-field-order.zig")]
    [InlineData(false, "pointer-declaration.zig")]
    [InlineData(true, "pointer-declaration.zig")]
    [InlineData(false, "enum-exchange.zig")]
    [InlineData(true, "enum-exchange.zig")]
    [InlineData(false, "packed-arithmetic.zig")]
    [InlineData(true, "packed-arithmetic.zig")]
    [InlineData(false, "packed-compare-exchange.zig")]
    [InlineData(true, "packed-compare-exchange.zig")]
    [InlineData(false, "pointer-integer-width.zig")]
    [InlineData(true, "pointer-integer-width.zig")]
    [InlineData(false, "empty-slice-projection.zig")]
    [InlineData(true, "empty-slice-projection.zig")]
    [InlineData(false, "imported-type-call.zig")]
    [InlineData(true, "imported-type-call.zig")]
    [InlineData(false, "enum-lock.zig")]
    [InlineData(true, "enum-lock.zig")]
    [InlineData(false, "container-module-alias.zig")]
    [InlineData(true, "container-module-alias.zig")]
    [InlineData(false, "uninhabited-union.zig")]
    [InlineData(true, "uninhabited-union.zig")]
    [InlineData(false, "explicit-bitcast-target.zig")]
    [InlineData(true, "explicit-bitcast-target.zig")]
    [InlineData(false, "typeof-peer-width.zig")]
    [InlineData(true, "typeof-peer-width.zig")]
    [InlineData(false, "typeof-return-scope.zig")]
    [InlineData(true, "typeof-return-scope.zig")]
    [InlineData(false, "terminating-switch.zig")]
    [InlineData(true, "terminating-switch.zig")]
    [InlineData(false, "runtime-tag-labels.zig")]
    [InlineData(true, "runtime-tag-labels.zig")]
    [InlineData(false, "captured-bounded-slice.zig")]
    [InlineData(true, "captured-bounded-slice.zig")]
    [InlineData(false, "type-name-declared.zig")]
    [InlineData(true, "type-name-declared.zig")]
    [InlineData(false, "union-tag-comparison.zig")]
    [InlineData(true, "union-tag-comparison.zig")]
    [InlineData(false, "aligned-zero-marker.zig")]
    [InlineData(true, "aligned-zero-marker.zig")]
    [InlineData(false, "ptrcast-effects.zig")]
    [InlineData(true, "ptrcast-effects.zig")]
    [InlineData(false, "pointer-peer-size.zig")]
    [InlineData(true, "pointer-peer-size.zig")]
    [InlineData(false, "null-peer-forwarding.zig")]
    [InlineData(true, "null-peer-forwarding.zig")]
    [InlineData(false, "optional-pointer-reflection.zig")]
    [InlineData(true, "optional-pointer-reflection.zig")]
    [InlineData(false, "optional-pointer-jump-payload.zig")]
    [InlineData(true, "optional-pointer-jump-payload.zig")]
    [InlineData(false, "anyopaque-type-identity.zig")]
    [InlineData(true, "anyopaque-type-identity.zig")]
    [InlineData(false, "pointer-integer-type-name.zig")]
    [InlineData(true, "pointer-integer-type-name.zig")]
    [InlineData(false, "function-pointer-reflection.zig")]
    [InlineData(true, "function-pointer-reflection.zig")]
    [InlineData(false, "function-type-name.zig")]
    [InlineData(true, "function-type-name.zig")]
    [InlineData(false, "imported-enum-backing.zig")]
    [InlineData(true, "imported-enum-backing.zig")]
    [InlineData(false, "container-layout-value.zig")]
    [InlineData(true, "container-layout-value.zig")]
    [InlineData(false, "inline-explicit-union.zig")]
    [InlineData(true, "inline-explicit-union.zig")]
    [InlineData(false, "local-self-node.zig")]
    [InlineData(true, "local-self-node.zig")]
    [InlineData(false, "local-enum-union-methods.zig")]
    [InlineData(true, "local-enum-union-methods.zig")]
    public void Static_addresses_keep_identity_without_reading_runtime_storage(bool objects, string fixture)
    {
        var library = Environment.GetEnvironmentVariable("DOTCC_TARGET_ZIG_LIB");
        if (string.IsNullOrEmpty(library)) Assert.Skip("Set DOTCC_TARGET_ZIG_LIB to the pinned source standard library.");
        var source = Fixture(fixture);
        var options = Options(source, library);
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-static-pointer-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            string emitted;
            if (objects)
            {
                var fragment = Path.Combine(directory, "main.csobj");
                File.WriteAllText(fragment, Compiler.EmitObject(source, zigOptions: options));
                emitted = Compiler.LinkObjects([fragment], emit: EmitMode.ManagedLib);
            }
            else emitted = Compiler.EmitCSharp([source], emit: EmitMode.ManagedLib, zigOptions: options);
            emitted += "\npublic static class StaticPointerHost { public static int Main() => DotCcLib.answer(); }";
            var result = FixtureRunner.CompileAndRunCapturingStreams(emitted, [], TestContext.Current.CancellationToken);
            result.exit.ShouldBe(42);
            result.stdout.ShouldBeEmpty();
            result.stderr.ShouldBeEmpty();
        }
        finally { Directory.Delete(directory, true); }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Mutable_static_contents_remain_unknown_at_compile_time(bool objects)
    {
        var library = Environment.GetEnvironmentVariable("DOTCC_TARGET_ZIG_LIB");
        if (string.IsNullOrEmpty(library)) Assert.Skip("Set DOTCC_TARGET_ZIG_LIB to the pinned source standard library.");
        var source = Fixture("mutable-read.zig");
        var exception = Should.Throw<IrUnsupportedException>(() =>
        {
            if (objects) Compiler.EmitObject(source, zigOptions: Options(source, library));
            else Compiler.EmitCSharp([source], emit: EmitMode.ManagedLib, zigOptions: Options(source, library));
        });
        exception.Message.ShouldContain("did not evaluate at compile time");
    }

    [Theory]
    [InlineData(false, "enum-exchange-wrong-type.zig", "nominal enum type")]
    [InlineData(true, "enum-exchange-wrong-type.zig", "nominal enum type")]
    [InlineData(false, "packed-wrong-type.zig", "nominal packed type")]
    [InlineData(true, "packed-wrong-type.zig", "nominal packed type")]
    [InlineData(false, "packed-cas-wrong-type.zig", "nominal packed type")]
    [InlineData(true, "packed-cas-wrong-type.zig", "nominal packed type")]
    [InlineData(false, "enum-store-wrong-type.zig", "nominal enum type")]
    [InlineData(true, "enum-store-wrong-type.zig", "nominal enum type")]
    [InlineData(false, "enum-cas-wrong-type.zig", "nominal enum type")]
    [InlineData(true, "enum-cas-wrong-type.zig", "nominal enum type")]
    [InlineData(false, "union-tag-wrong-enum.zig", "own tag enum")]
    [InlineData(true, "union-tag-wrong-enum.zig", "own tag enum")]
    [InlineData(false, "union-tag-payload-coercion.zig", "carries a payload")]
    [InlineData(true, "union-tag-payload-coercion.zig", "carries a payload")]
    [InlineData(false, "aligned-field-invalid.zig", "positive power of two")]
    [InlineData(true, "aligned-field-invalid.zig", "positive power of two")]
    [InlineData(false, "ptrcast-const-invalid.zig", "discard const")]
    [InlineData(true, "ptrcast-const-invalid.zig", "discard const")]
    public void Atomic_operations_keep_nominal_operand_validation(bool objects, string fixture, string diagnostic)
    {
        var library = Environment.GetEnvironmentVariable("DOTCC_TARGET_ZIG_LIB");
        if (string.IsNullOrEmpty(library)) Assert.Skip("Set DOTCC_TARGET_ZIG_LIB to the pinned source standard library.");
        var source = Fixture(fixture);
        var exception = Should.Throw<CompileException>(() =>
        {
            if (objects) Compiler.EmitObject(source, zigOptions: Options(source, library));
            else Compiler.EmitCSharp([source], emit: EmitMode.ManagedLib, zigOptions: Options(source, library));
        });
        exception.Message.ShouldContain(diagnostic);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Runtime_slice_length_cannot_be_used_for_inline_unrolling(bool objects)
    {
        var library = Environment.GetEnvironmentVariable("DOTCC_TARGET_ZIG_LIB");
        if (string.IsNullOrEmpty(library)) Assert.Skip("Set DOTCC_TARGET_ZIG_LIB to the pinned source standard library.");
        var source = Fixture("runtime-inline-slice.zig");
        var exception = Should.Throw<IrUnsupportedException>(() =>
        {
            if (objects) Compiler.EmitObject(source, zigOptions: Options(source, library));
            else Compiler.EmitCSharp([source], emit: EmitMode.ManagedLib, zigOptions: Options(source, library));
        });
        exception.Message.ShouldContain("requires a fixed-size array");
    }

    [Theory]
    [InlineData(false, "inhabited-union-invalid-field.zig", "no field")]
    [InlineData(true, "inhabited-union-invalid-field.zig", "no field")]
    [InlineData(false, "uninhabited-union-invalid-value.zig", "noreturn")]
    [InlineData(true, "uninhabited-union-invalid-value.zig", "noreturn")]
    [InlineData(false, "explicit-bitcast-invalid-width.zig", "equal logical source and target bit widths")]
    [InlineData(true, "explicit-bitcast-invalid-width.zig", "equal logical source and target bit widths")]
    [InlineData(false, "typeof-peer-invalid-bitcast.zig", "equal logical source and target bit widths")]
    [InlineData(true, "typeof-peer-invalid-bitcast.zig", "equal logical source and target bit widths")]
    [InlineData(false, "fallthrough-switch-invalid.zig", "comptime")]
    [InlineData(true, "fallthrough-switch-invalid.zig", "comptime")]
    [InlineData(false, "optional-pointer-wrong-field.zig", "active `std.builtin.Type` field is `optional`")]
    [InlineData(true, "optional-pointer-wrong-field.zig", "active `std.builtin.Type` field is `optional`")]
    [InlineData(false, "anyopaque-size-invalid.zig", "has no value or layout")]
    [InlineData(true, "anyopaque-size-invalid.zig", "has no value or layout")]
    [InlineData(false, "anyopaque-value-invalid.zig", "has no value or layout")]
    [InlineData(true, "anyopaque-value-invalid.zig", "has no value or layout")]
    [InlineData(false, "inline-explicit-union-invalid.zig", "missing")]
    [InlineData(true, "inline-explicit-union-invalid.zig", "missing")]
    public void Invalid_source_type_shapes_remain_rejected(bool objects, string fixture, string diagnostic)
    {
        var library = Environment.GetEnvironmentVariable("DOTCC_TARGET_ZIG_LIB");
        if (string.IsNullOrEmpty(library)) Assert.Skip("Set DOTCC_TARGET_ZIG_LIB to the pinned source standard library.");
        var source = Fixture(fixture);
        var exception = Should.Throw<IrUnsupportedException>(() =>
        {
            if (objects) Compiler.EmitObject(source, zigOptions: Options(source, library));
            else Compiler.EmitCSharp([source], emit: EmitMode.ManagedLib, zigOptions: Options(source, library));
        });
        exception.Message.ShouldContain(diagnostic);
    }

    private static ZigCompilationOptions Options(string source, string library) => new("root", [new("root", source, [])],
        outputMode: ZigSourceOutputMode.Lib, optimizeMode: ZigSourceOptimizeMode.Fast, zigLibDir: library,
        stdlibTypes: ZigStdlibTypeModel.Source, functionPointerAbi: ZigFunctionPointerAbi.Managed,
        managedRoots: ["answer"], isTest: true);

    private static string Fixture(string entry, [CallerFilePath] string file = "") =>
        Path.Combine(Path.GetDirectoryName(file)!, "Fixtures", "ZigStaticPointers", entry);
}
