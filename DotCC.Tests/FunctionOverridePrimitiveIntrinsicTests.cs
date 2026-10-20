using System;
using System.Collections.Generic;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class FunctionOverridePrimitiveIntrinsicTests
{
    public static IEnumerable<object[]> MemoryTargets()
    {
        foreach (var order in new[] { "le", "be", "native" })
        foreach (var bits in new[] { 16, 32, 64 })
        foreach (var store in new[] { false, true })
        foreach (var pointer in new[] { "void *", "unsigned char *" })
        {
            var type = "uint" + bits + "_t";
            var scalar = bits == 16 ? "ushort" : bits == 32 ? "uint" : "ulong";
            var expected = order == "native"
                ? "Unsafe." + (store ? "WriteUnaligned<" : "ReadUnaligned<") + scalar + ">"
                : "BinaryPrimitives." + (store ? "WriteUInt" : "ReadUInt") + bits + (order == "le" ? "LittleEndian" : "BigEndian");
            yield return new object[] { (store ? "store.u" : "load.u") + bits + "." + order,
                store ? "void" : type, store ? pointer : "const " + pointer, store ? type : "", expected };
        }
    }

    [Theory]
    [MemberData(nameof(MemoryTargets))]
    public void Ordinary_void_and_byte_pointer_contracts_bind_exact_memory_operations(string target, string result, string pointer, string value, string expected)
    {
        var parameters = value.Length == 0 ? new[] { pointer } : new[] { pointer, value };
        WithPrototype(target, result, parameters, (path, options) =>
            Compiler.EmitCSharp(new[] { path }, emit: EmitMode.ManagedLib, preprocessing: options).ShouldContain(expected));
    }

    [Theory]
    [InlineData("bswap.u32", "unsigned int", "unsigned int", "ReverseEndianness")]
    [InlineData("bswap.u64", "unsigned long", "unsigned long", "ReverseEndianness")]
    [InlineData("bswap.u64", "unsigned long long", "unsigned long long", "ReverseEndianness")]
    [InlineData("clz.u32", "unsigned int", "unsigned int", "LeadingZeroCount")]
    [InlineData("ctz.u32", "unsigned int", "unsigned int", "TrailingZeroCount")]
    [InlineData("clz.u64", "unsigned int", "unsigned long", "LeadingZeroCount")]
    [InlineData("ctz.u64", "unsigned int", "unsigned long long", "TrailingZeroCount")]
    [InlineData("highbit.u32", "unsigned int", "unsigned int", "LeadingZeroCount")]
    [InlineData("commonbytes.u64", "unsigned int", "unsigned long", "BitConverter.IsLittleEndian")]
    [InlineData("is-little-endian.u32", "unsigned int", "", "BitConverter.IsLittleEndian")]
    [InlineData("is-little-endian.i32", "int", "", "BitConverter.IsLittleEndian")]
    public void Scalar_targets_bind_their_original_integer_result_contract(string target, string result, string parameter, string expected)
    {
        WithPrototype(target, result, parameter.Length == 0 ? Array.Empty<string>() : new[] { parameter }, (path, options) =>
            Compiler.EmitCSharp(new[] { path }, emit: EmitMode.ManagedLib, preprocessing: options).ShouldContain(expected));
    }

    [Theory]
    [InlineData("load.u32.native", "unsigned int", "const volatile void *")]
    [InlineData("load.u32.be", "unsigned int", "const _Atomic(unsigned char) *")]
    [InlineData("load.u16.le", "unsigned short", "const unsigned short *")]
    [InlineData("load.u64.native", "long long", "const void *")]
    [InlineData("load.u32.be", "unsigned long", "const void *")]
    [InlineData("store.u32.native", "void", "const void *")]
    [InlineData("store.u32.be", "void", "volatile void *")]
    [InlineData("store.u32.le", "void", "_Atomic(unsigned char) *")]
    public void Memory_targets_reject_incompatible_widths_and_access_qualifiers(string target, string result, string pointer)
    {
        var parameters = target.StartsWith("store.", StringComparison.Ordinal) ? new[] { pointer, "unsigned int" } : new[] { pointer };
        WithPrototype(target, result, parameters, (path, options) =>
            Should.Throw<CompileException>(() => Compiler.EmitCSharp(new[] { path }, emit: EmitMode.ManagedLib, preprocessing: options)));
    }

    [Theory]
    [InlineData("bswap.u32", "unsigned int", "int")]
    [InlineData("bswap.u64", "unsigned int", "unsigned long")]
    [InlineData("clz.u32", "int", "unsigned int")]
    [InlineData("ctz.u64", "unsigned int", "unsigned int")]
    [InlineData("highbit.u32", "unsigned long", "unsigned int")]
    [InlineData("commonbytes.u64", "unsigned int", "long")]
    [InlineData("is-little-endian.u32", "int", "")]
    [InlineData("is-little-endian.i32", "int", "int")]
    public void Scalar_targets_reject_incompatible_integer_width_signedness_and_arity(string target, string result, string parameter)
    {
        WithPrototype(target, result, parameter.Length == 0 ? Array.Empty<string>() : new[] { parameter }, (path, options) =>
            Should.Throw<CompileException>(() => Compiler.EmitCSharp(new[] { path }, emit: EmitMode.ManagedLib, preprocessing: options)));
    }

    private static void WithPrototype(string target, string result, string[] parameters, Action<string, CPreprocessingOptions> test)
    {
        var path = Path.Combine(Path.GetTempPath(), "dotcc-primitive-contract-" + Guid.NewGuid().ToString("N") + ".c");
        try
        {
            File.WriteAllText(path, "#include <stdint.h>\n" + result + " operation(" + (parameters.Length == 0 ? "void" : string.Join(",", parameters)) + ");\n");
            var options = new CPreprocessingOptions(Array.Empty<MacroOverride>(), functionOverrides: new[]
            {
                new FunctionOverride("operation", new(result, parameters), new("intrinsic", target), RequireMatch: true)
            });
            test(path, options);
        }
        finally { File.Delete(path); }
    }
}
