using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using DotCC.Frontends;
using DotCC.Ir;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

[Collection("ZigFrontend")]
public sealed class ZigSourceFileTypeExecutionTests
{
    [Fact]
    public void Actual_source_builtin_target_methods_and_value_copies_evaluate()
    {
        var library = Environment.GetEnvironmentVariable("DOTCC_TARGET_ZIG_LIB");
        if (string.IsNullOrEmpty(library)) Assert.Skip("Set DOTCC_TARGET_ZIG_LIB to the pinned source standard library.");
        var path = Path.Combine(Path.GetDirectoryName(Fixture())!, "builtin-target-value.zig");
        var options = new ZigCompilationOptions("root", [new("root", path, [])],
            outputMode: ZigSourceOutputMode.Lib, optimizeMode: ZigSourceOptimizeMode.Fast,
            zigLibDir: library, stdlibTypes: ZigStdlibTypeModel.Source, functionPointerAbi: ZigFunctionPointerAbi.Managed);
        var ir = new ZigFrontend().BuildIr(new FrontendRequest([path], ZigOptions: options));
        var answer = ir.Functions.Single(function => function.Sym.Name == "targetAnswer");
        var result = ir.EvalComptimeValue(new Call(answer.Sym.Name, []) { CalleeSym = answer.Sym, Type = CType.UChar });
        result.ShouldBeOfType<IrBuilder.CtInt>().Value.ShouldBe(42);
    }

    [Theory]
    [InlineData(false, "opaque-source.zig")]
    [InlineData(true, "opaque-source.zig")]
    [InlineData(false, "main.zig")]
    [InlineData(true, "main.zig")]
    [InlineData(false, "undefined.zig")]
    [InlineData(true, "undefined.zig")]
    [InlineData(false, "callbacks.zig")]
    [InlineData(true, "callbacks.zig")]
    [InlineData(false, "roots.zig")]
    [InlineData(true, "roots.zig")]
    [InlineData(false, "integer-provenance.zig")]
    [InlineData(true, "integer-provenance.zig")]
    [InlineData(false, "abi-target.zig")]
    [InlineData(true, "abi-target.zig")]
    [InlineData(false, "builtin-target-value.zig")]
    [InlineData(true, "builtin-target-value.zig")]
    [InlineData(false, "enum-switch-capture.zig")]
    [InlineData(true, "enum-switch-capture.zig")]
    [InlineData(false, "optional-record-global.zig")]
    [InlineData(true, "optional-record-global.zig")]
    [InlineData(false, "nested-function-alias.zig")]
    [InlineData(true, "nested-function-alias.zig")]
    [InlineData(false, "lazy-generic-type-const.zig")]
    [InlineData(true, "lazy-generic-type-const.zig")]
    [InlineData(false, "switch-typed-parameter.zig")]
    [InlineData(true, "switch-typed-parameter.zig")]
    [InlineData(false, "comptime-record-copy.zig")]
    [InlineData(true, "comptime-record-copy.zig")]
    [InlineData(false, "pointer-provenance.zig")]
    [InlineData(true, "pointer-provenance.zig")]
    [InlineData(false, "pointer-alignment.zig")]
    [InlineData(true, "pointer-alignment.zig")]
    [InlineData(false, "layout-forward.zig")]
    [InlineData(true, "layout-forward.zig")]
    [InlineData(false, "zero-wrappers.zig")]
    [InlineData(true, "zero-wrappers.zig")]
    [InlineData(false, "labeled-prongs.zig")]
    [InlineData(true, "labeled-prongs.zig")]
    [InlineData(false, "comptime-receiver.zig")]
    [InlineData(true, "comptime-receiver.zig")]
    [InlineData(false, "comptime-file-receiver.zig")]
    [InlineData(true, "comptime-file-receiver.zig")]
    [InlineData(false, "comptime-optional-slice.zig")]
    [InlineData(true, "comptime-optional-slice.zig")]
    [InlineData(false, "inline-tuple.zig")]
    [InlineData(true, "inline-tuple.zig")]
    [InlineData(false, "computed-enum-field.zig")]
    [InlineData(true, "computed-enum-field.zig")]
    [InlineData(false, "vector-bitcast.zig")]
    [InlineData(true, "vector-bitcast.zig")]
    [InlineData(false, "imported-extern.zig")]
    [InlineData(true, "imported-extern.zig")]
    [InlineData(false, "selected-switch-jump.zig")]
    [InlineData(true, "selected-switch-jump.zig")]
    [InlineData(false, "union-value-switch.zig")]
    [InlineData(true, "union-value-switch.zig")]
    [InlineData(false, "array-bound-catch.zig")]
    [InlineData(true, "array-bound-catch.zig")]
    [InlineData(false, "labeled-tuple-null-pointers.zig")]
    [InlineData(true, "labeled-tuple-null-pointers.zig")]
    [InlineData(false, "error-optional-enum-return.zig")]
    [InlineData(true, "error-optional-enum-return.zig")]
    [InlineData(false, "optional-anonymous-struct.zig")]
    [InlineData(true, "optional-anonymous-struct.zig")]
    [InlineData(false, "optional-zero-payload.zig")]
    [InlineData(true, "optional-zero-payload.zig")]
    [InlineData(false, "packed-boolean-compound.zig")]
    [InlineData(true, "packed-boolean-compound.zig")]
    [InlineData(false, "default-identifier.zig")]
    [InlineData(true, "default-identifier.zig")]
    [InlineData(false, "discard-parameters.zig")]
    [InlineData(true, "discard-parameters.zig")]
    [InlineData(false, "module-comptime-options.zig")]
    [InlineData(true, "module-comptime-options.zig")]
    [InlineData(false, "dead-aggregate-errdefer.zig")]
    [InlineData(true, "dead-aggregate-errdefer.zig")]
    [InlineData(false, "dead-errdefer.zig")]
    [InlineData(true, "dead-errdefer.zig")]
    [InlineData(false, "zero-array-default.zig")]
    [InlineData(true, "zero-array-default.zig")]
    [InlineData(false, "catch-error-peer.zig")]
    [InlineData(true, "catch-error-peer.zig")]
    [InlineData(false, "container-array-constant.zig")]
    [InlineData(true, "container-array-constant.zig")]
    [InlineData(false, "scalar-switch-capture.zig")]
    [InlineData(true, "scalar-switch-capture.zig")]
    [InlineData(false, "optional-storage-address.zig")]
    [InlineData(true, "optional-storage-address.zig")]
    [InlineData(false, "effectful-slice-view.zig")]
    [InlineData(true, "effectful-slice-view.zig")]
    [InlineData(false, "typed-result-contexts.zig")]
    [InlineData(true, "typed-result-contexts.zig")]
    [InlineData(false, "tuple-branch-peers.zig")]
    [InlineData(true, "tuple-branch-peers.zig")]
    [InlineData(false, "void-catch-result.zig")]
    [InlineData(true, "void-catch-result.zig")]
    [InlineData(false, "generic-sentinel-return.zig")]
    [InlineData(true, "generic-sentinel-return.zig")]
    [InlineData(false, "union-payload-sentinel.zig")]
    [InlineData(true, "union-payload-sentinel.zig")]
    [InlineData(false, "container-sentinel-alias.zig")]
    [InlineData(true, "container-sentinel-alias.zig")]
    [InlineData(false, "truncate-literal.zig")]
    [InlineData(true, "truncate-literal.zig")]
    [InlineData(false, "wide-cell-interleave.zig")]
    [InlineData(true, "wide-cell-interleave.zig")]
    [InlineData(false, "enum-backing-inference.zig")]
    [InlineData(true, "enum-backing-inference.zig")]
    [InlineData(false, "rgb-positional-writer.zig")]
    [InlineData(true, "rgb-positional-writer.zig")]
    [InlineData(false, "actual-string-decoding.zig")]
    [InlineData(true, "actual-string-decoding.zig")]
    [InlineData(false, "hash-packed-array.zig")]
    [InlineData(true, "hash-packed-array.zig")]
    [InlineData(false, "cursor-hyperlink-id.zig")]
    [InlineData(true, "cursor-hyperlink-id.zig")]
    [InlineData(false, "packed-array-bitcast.zig")]
    [InlineData(true, "packed-array-bitcast.zig")]
    [InlineData(false, "bit-count-locals.zig")]
    [InlineData(true, "bit-count-locals.zig")]
    [InlineData(false, "error-type-members.zig")]
    [InlineData(true, "error-type-members.zig")]
    [InlineData(false, "enum-decl-alias.zig")]
    [InlineData(true, "enum-decl-alias.zig")]
    [InlineData(false, "optional-decl-call.zig")]
    [InlineData(true, "optional-decl-call.zig")]
    [InlineData(false, "cursor-style-compatibility.zig")]
    [InlineData(true, "cursor-style-compatibility.zig")]
    [InlineData(false, "nonexhaustive-switch.zig")]
    [InlineData(true, "nonexhaustive-switch.zig")]
    [InlineData(false, "recursive-union-tag.zig")]
    [InlineData(true, "recursive-union-tag.zig")]
    [InlineData(false, "local-container-owner.zig")]
    [InlineData(true, "local-container-owner.zig")]
    [InlineData(false, "atomic-rmw.zig")]
    [InlineData(true, "atomic-rmw.zig")]
    [InlineData(false, "atomic-access.zig")]
    [InlineData(true, "atomic-access.zig")]
    [InlineData(false, "std-function-value.zig")]
    [InlineData(true, "std-function-value.zig")]
    [InlineData(false, "method-alias.zig")]
    [InlineData(true, "method-alias.zig")]
    [InlineData(false, "action-value-forwarder.zig")]
    [InlineData(true, "action-value-forwarder.zig")]
    [InlineData(false, "comptime-bool-selection.zig")]
    [InlineData(true, "comptime-bool-selection.zig")]
    [InlineData(false, "comptime-optional-int-call.zig")]
    [InlineData(true, "comptime-optional-int-call.zig")]
    [InlineData(false, "optional-enum-backing-cast.zig")]
    [InlineData(true, "optional-enum-backing-cast.zig")]
    [InlineData(false, "labeled-void-break.zig")]
    [InlineData(true, "labeled-void-break.zig")]
    [InlineData(false, "imported-optional-slice.zig")]
    [InlineData(true, "imported-optional-slice.zig")]
    [InlineData(false, "hoisted-module-constant.zig")]
    [InlineData(true, "hoisted-module-constant.zig")]
    [InlineData(false, "comptime-feature-query.zig")]
    [InlineData(true, "comptime-feature-query.zig")]
    [InlineData(false, "reflected-format-widths.zig")]
    [InlineData(true, "reflected-format-widths.zig")]
    [InlineData(false, "tuple-width-forwarding.zig")]
    [InlineData(true, "tuple-width-forwarding.zig")]
    [InlineData(false, "typeinfo-switch-binding.zig")]
    [InlineData(true, "typeinfo-switch-binding.zig")]
    [InlineData(false, "tag-name-string-argument.zig")]
    [InlineData(true, "tag-name-string-argument.zig")]
    [InlineData(false, "inline-known-slice.zig")]
    [InlineData(true, "inline-known-slice.zig")]
    [InlineData(false, "vector-array-local.zig")]
    [InlineData(true, "vector-array-local.zig")]
    [InlineData(false, "vector-lane-shifts.zig")]
    [InlineData(true, "vector-lane-shifts.zig")]
    [InlineData(false, "value-control-bitcast-location.zig")]
    [InlineData(true, "value-control-bitcast-location.zig")]
    [InlineData(false, "storage-vector-comparisons.zig")]
    [InlineData(true, "storage-vector-comparisons.zig")]
    [InlineData(false, "comptime-discard-evaluation.zig")]
    [InlineData(true, "comptime-discard-evaluation.zig")]
    [InlineData(false, "parallel-for-many-pointer.zig")]
    [InlineData(true, "parallel-for-many-pointer.zig")]
    [InlineData(false, "parallel-for-operand-order.zig")]
    [InlineData(true, "parallel-for-operand-order.zig")]
    [InlineData(false, "parallel-for-bitcast-width.zig")]
    [InlineData(true, "parallel-for-bitcast-width.zig")]
    [InlineData(false, "comptime-slice-block-argument.zig")]
    [InlineData(true, "comptime-slice-block-argument.zig")]
    [InlineData(false, "computed-offset-field.zig")]
    [InlineData(true, "computed-offset-field.zig")]
    [InlineData(false, "comptime-slice-string-capture.zig")]
    [InlineData(true, "comptime-slice-string-capture.zig")]
    [InlineData(false, "field-mask-path.zig")]
    [InlineData(true, "field-mask-path.zig")]
    [InlineData(false, "ordered-array-elements.zig")]
    [InlineData(true, "ordered-array-elements.zig")]
    [InlineData(false, "narrow-wide-field-copy.zig")]
    [InlineData(true, "narrow-wide-field-copy.zig")]
    [InlineData(false, "comptime-local-field-store.zig")]
    [InlineData(true, "comptime-local-field-store.zig")]
    [InlineData(false, "comptime-enum-names.zig")]
    [InlineData(true, "comptime-enum-names.zig")]
    [InlineData(false, "color-scheme-size.zig")]
    [InlineData(true, "color-scheme-size.zig")]
    [InlineData(false, "optional-function-value-if.zig")]
    [InlineData(true, "optional-function-value-if.zig")]
    [InlineData(false, "runtime-struct-temporary.zig")]
    [InlineData(true, "runtime-struct-temporary.zig")]
    [InlineData(false, "comptime-int-set.zig")]
    [InlineData(true, "comptime-int-set.zig")]
    [InlineData(false, "key-flag-stack.zig")]
    [InlineData(true, "key-flag-stack.zig")]
    [InlineData(false, "enum-name-map.zig")]
    [InlineData(true, "enum-name-map.zig")]
    [InlineData(false, "typed-slice-array-copy.zig")]
    [InlineData(true, "typed-slice-array-copy.zig")]
    [InlineData(false, "enum-value-declarations.zig")]
    [InlineData(true, "enum-value-declarations.zig")]
    [InlineData(false, "file-namespace-demand.zig")]
    [InlineData(true, "file-namespace-demand.zig")]
    [InlineData(false, "constant-retry-loop.zig")]
    [InlineData(true, "constant-retry-loop.zig")]
    [InlineData(false, "extern-bitcast-widths.zig")]
    [InlineData(true, "extern-bitcast-widths.zig")]
    [InlineData(false, "selected-comptime-string.zig")]
    [InlineData(true, "selected-comptime-string.zig")]
    [InlineData(false, "narrow-overflow-results.zig")]
    [InlineData(true, "narrow-overflow-results.zig")]
    [InlineData(false, "ascii-vector-lanes.zig")]
    [InlineData(true, "ascii-vector-lanes.zig")]
    [InlineData(false, "reflection-sentinel-pointer.zig")]
    [InlineData(true, "reflection-sentinel-pointer.zig")]
    [InlineData(false, "module-optional-string.zig")]
    [InlineData(true, "module-optional-string.zig")]
    [InlineData(false, "integer-array-bitcast.zig")]
    [InlineData(true, "integer-array-bitcast.zig")]
    [InlineData(false, "noreturn-fallback-block.zig")]
    [InlineData(true, "noreturn-fallback-block.zig")]
    [InlineData(false, "comptime-optional-zero.zig")]
    [InlineData(true, "comptime-optional-zero.zig")]
    [InlineData(false, "comptime-array-argument.zig")]
    [InlineData(true, "comptime-array-argument.zig")]
    [InlineData(false, "comptime-scalar-pointers.zig")]
    [InlineData(true, "comptime-scalar-pointers.zig")]
    [InlineData(false, "comptime-pinned-struct.zig")]
    [InlineData(true, "comptime-pinned-struct.zig")]
    [InlineData(false, "call-arguments.zig")]
    [InlineData(true, "call-arguments.zig")]
    [InlineData(false, "call-arguments-metadata.zig")]
    [InlineData(true, "call-arguments-metadata.zig")]
    public void Empty_file_structs_and_generic_module_reexports_keep_their_source_identity(bool objects, string entry)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-empty-source-file-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var library = Path.Combine(directory, "lib");
            Directory.CreateDirectory(Path.Combine(library, "std"));
            File.WriteAllText(Path.Combine(library, "std", "std.zig"), "pub const Target = @import(\"Target.zig\");");
            File.WriteAllText(Path.Combine(library, "std", "Target.zig"), "pub const Abi = enum { gnu, musl, pub fn isMusl(abi: Abi) bool { return abi == .musl; } pub fn isGnu(abi: Abi) bool { return abi == .gnu; } };");
            if (entry is "vector-array-local.zig" or "comptime-discard-evaluation.zig" or "builtin-target-value.zig" or "field-mask-path.zig" or "comptime-enum-names.zig" or "color-scheme-size.zig" or "key-flag-stack.zig" or "enum-name-map.zig" or "comptime-int-set.zig" or "hash-packed-array.zig" or "actual-string-decoding.zig" or "rgb-positional-writer.zig" or "wide-cell-interleave.zig" or "enum-backing-inference.zig" or "module-comptime-options.zig")
            {
                var realLibrary = Environment.GetEnvironmentVariable("DOTCC_TARGET_ZIG_LIB");
                if (string.IsNullOrEmpty(realLibrary)) Assert.Skip("Set DOTCC_TARGET_ZIG_LIB to the pinned source standard library.");
                library = realLibrary;
            }
            if (entry == "atomic-compare-exchange.zig")
            {
                File.AppendAllText(Path.Combine(library, "std", "std.zig"), "pub const lang = @import(\"lang.zig\");");
                File.WriteAllText(Path.Combine(library, "std", "lang.zig"), "pub const AtomicOrder = enum { unordered, monotonic, acquire, release, acq_rel, seq_cst };");
            }
            if (entry == "std-function-value.zig")
            {
                File.AppendAllText(Path.Combine(library, "std", "std.zig"), "pub const Io = @import(\"Io.zig\");");
                File.WriteAllText(Path.Combine(library, "std", "Io.zig"), "pub fn noCrashHandler(userdata: ?*anyopaque) void { _ = userdata; }");
            }
            var path = Path.Combine(Path.GetDirectoryName(Fixture())!, entry);
            var actualLibrary = entry is "vector-array-local.zig" or "comptime-discard-evaluation.zig" or "builtin-target-value.zig" or "field-mask-path.zig" or "comptime-enum-names.zig" or "color-scheme-size.zig" or "key-flag-stack.zig" or "enum-name-map.zig" or "comptime-int-set.zig" or "hash-packed-array.zig" or "actual-string-decoding.zig" or "rgb-positional-writer.zig" or "wide-cell-interleave.zig" or "enum-backing-inference.zig" or "module-comptime-options.zig";
            ZigModuleDefinition[] modules = entry == "key-flag-stack.zig"
                ? [new("root", path, [new("key", "key")]), new("key", Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Fixture())!, "../../../libghostty/ref/ghostty-93967b7f9478e94dcbb42125c8a90f34feb40d75/src/terminal/kitty/key.zig")), [])]
                : entry == "comptime-int-set.zig"
                    ? [new("root", path, [new("intset", "intset")]), new("intset", Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Fixture())!, "../../../libghostty/ref/ghostty-93967b7f9478e94dcbb42125c8a90f34feb40d75/src/datastruct/comptime_int_set.zig")), [])]
                    : entry == "actual-string-decoding.zig"
                        ? [new("root", path, [new("encoding", "encoding")]), new("encoding", Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Fixture())!, "../../../libghostty/ref/ghostty-93967b7f9478e94dcbb42125c8a90f34feb40d75/src/os/string_encoding.zig")), [])]
                        : [new("root", path, [])];
            var options = new ZigCompilationOptions("root", modules,
                outputMode: actualLibrary ? ZigSourceOutputMode.Lib : null,
                optimizeMode: actualLibrary ? ZigSourceOptimizeMode.Fast : null,
                zigLibDir: library, stdlibTypes: ZigStdlibTypeModel.Source, functionPointerAbi: ZigFunctionPointerAbi.Managed,
                managedRoots: entry == "roots.zig" ? ["selected_entry"] : actualLibrary && entry != "builtin-target-value.zig" ? ["answer"] : null);
            var emitMode = actualLibrary ? EmitMode.ManagedLib : EmitMode.Csproj;
            string emitted;
            if (objects)
            {
                var fragment = Path.Combine(directory, "main.csobj");
                File.WriteAllText(fragment, Compiler.EmitObject(path, zigOptions: options));
                emitted = Compiler.LinkObjects([fragment], emit: emitMode);
            }
            else emitted = Compiler.EmitCSharp([path], emit: emitMode, zigOptions: options);
            if (entry == "builtin-target-value.zig")
                emitted += "\npublic static class TargetHost { public static int Main() => DotCcLib.targetAnswer(); }";
            if (actualLibrary && entry != "builtin-target-value.zig")
                emitted += "\npublic static class FieldMaskHost { public static int Main() => DotCcLib.answer(); }";
            var result = FixtureRunner.CompileAndRunCapturingStreams(emitted, [], TestContext.Current.CancellationToken);
            result.exit.ShouldBe(42);
            result.stdout.ShouldBeEmpty();
            result.stderr.ShouldBeEmpty();
        }
        finally { Directory.Delete(directory, true); }
    }
    [Theory]
    [InlineData(false, "reflected-tagged-union.zig")]
    [InlineData(true, "reflected-tagged-union.zig")]
    [InlineData(false, "reflected-result-union.zig")]
    [InlineData(true, "reflected-result-union.zig")]
    [InlineData(false, "reflected-union-alignment.zig")]
    [InlineData(true, "reflected-union-alignment.zig")]
    public void Reflected_tagged_union_retains_enum_payloads_and_storage(bool objects, string entry)
        => Empty_file_structs_and_generic_module_reexports_keep_their_source_identity(objects, entry);

    [Theory]
    [InlineData(false, "optional-type-default.zig")]
    [InlineData(true, "optional-type-default.zig")]
    [InlineData(false, "optional-type-present.zig")]
    [InlineData(true, "optional-type-present.zig")]
    [InlineData(false, "optional-type-values.zig")]
    [InlineData(true, "optional-type-values.zig")]
    public void Optional_type_fallback_is_lazy_and_preserves_type_evidence(bool objects, string entry)
        => Empty_file_structs_and_generic_module_reexports_keep_their_source_identity(objects, entry);

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Optional_type_fallback_also_works_with_default_curated_std(bool objects)
    {
        var path = Path.Combine(Path.GetDirectoryName(Fixture())!, "optional-type-values.zig");
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-optional-type-curated-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        string emitted;
        try
        {
            if (objects)
            {
                var fragment = Path.Combine(directory, "main.csobj");
                File.WriteAllText(fragment, Compiler.EmitObject(path));
                emitted = Compiler.LinkObjects([fragment], emit: EmitMode.Csproj);
            }
            else emitted = Compiler.EmitCSharp([path], emit: EmitMode.Csproj);
        }
        finally { Directory.Delete(directory, true); }
        var result = FixtureRunner.CompileAndRunCapturingStreams(emitted, [], TestContext.Current.CancellationToken);
        result.exit.ShouldBe(42);
        result.stdout.ShouldBeEmpty();
        result.stderr.ShouldBeEmpty();
    }

    private static string Fixture([CallerFilePath] string file = "")
        => Path.Combine(Path.GetDirectoryName(file)!, "Fixtures", "ZigSourceFileType", "main.zig");
}
