using System;
using System.Collections.Generic;
using DotCC.Ir;

namespace DotCC;

// One closed registry shared by profile validation, C signature checking and
// emission. These targets retain ordinary call effects in the IR. All C formal
// conversions run before an argument reaches one of these renderers.
internal sealed record FunctionOverrideIntrinsic(
    string ManagedMethod,
    string SignatureDescription,
    Func<CType.Func, bool> MatchesSignature,
    Func<IReadOnlyList<string>, IReadOnlyList<string>>? AdaptArguments = null,
    Func<string, string>? AdaptResult = null,
    Func<IReadOnlyList<string>, string>? RenderExpression = null,
    Func<IReadOnlyList<string>, string>? RenderDiscardExpression = null)
{
    // A cast or derived expression needs a discard at a C# statement sink.
    public bool IsStatementExpression => AdaptResult is null && RenderExpression is null;

    // Each argument is evaluated once, in its original position. The only
    // conditional descriptor has one argument, used once in its selected branch;
    // its condition is the immutable host-endian fact, with no argument effects.
    // Derived/result renderers parenthesize the entire expression because a Call
    // otherwise has postfix precedence in the C# backend.
    public string RenderCall(IReadOnlyList<string> arguments)
    {
        var call = RenderExpression?.Invoke(arguments)
            ?? Invocation(arguments);
        return AdaptResult?.Invoke(call) ?? call;
    }

    // Removing an unused scalar result conversion/derivation preserves the
    // argument effects without introducing an assignment to a user variable
    // named '_'. These closed BCL operations have no other observable effects.
    public string RenderDiscard(IReadOnlyList<string> arguments) =>
        RenderDiscardExpression?.Invoke(arguments) ?? Invocation(arguments);

    private string Invocation(IReadOnlyList<string> arguments) =>
        ManagedMethod + "(" + string.Join(", ", AdaptArguments?.Invoke(arguments) ?? arguments) + ")";

    public static FunctionOverrideIntrinsic? Find(string name) => name switch
    {
        "load.i32.le" => Memory(false, 4, true, "le"),
        "load.u16.le" => Memory(false, 2, false, "le"),
        "load.u32.le" => Memory(false, 4, false, "le"),
        "load.u64.le" => Memory(false, 8, false, "le"),
        "store.u16.le" => Memory(true, 2, false, "le"),
        "store.u32.le" => Memory(true, 4, false, "le"),
        "store.u64.le" => Memory(true, 8, false, "le"),
        "load.u16.be" => Memory(false, 2, false, "be"),
        "load.u32.be" => Memory(false, 4, false, "be"),
        "load.u64.be" => Memory(false, 8, false, "be"),
        "store.u16.be" => Memory(true, 2, false, "be"),
        "store.u32.be" => Memory(true, 4, false, "be"),
        "store.u64.be" => Memory(true, 8, false, "be"),
        "load.u16.native" => Memory(false, 2, false, "native"),
        "load.u32.native" => Memory(false, 4, false, "native"),
        "load.u64.native" => Memory(false, 8, false, "native"),
        "store.u16.native" => Memory(true, 2, false, "native"),
        "store.u32.native" => Memory(true, 4, false, "native"),
        "store.u64.native" => Memory(true, 8, false, "native"),
        "bswap.u32" => ByteSwap(4),
        "bswap.u64" => ByteSwap(8),
        "clz.u32" => CountZeros(4, true),
        "clz.u64" => CountZeros(8, true),
        "ctz.u32" => CountZeros(4, false),
        "ctz.u64" => CountZeros(8, false),
        "highbit.u32" => new("global::System.Numerics.BitOperations.LeadingZeroCount",
            "unsigned 32-bit result and one nonzero unsigned 32-bit integer",
            type => Scalar(type, 4, 4), AdaptResult: call => "(31U - (uint)(" + call + "))"),
        "commonbytes.u64" => new("global::System.Numerics.BitOperations.TrailingZeroCount",
            "unsigned 32-bit result and one nonzero unsigned 64-bit integer",
            type => Scalar(type, 4, 8), RenderExpression: arguments =>
                "((uint)(global::System.BitConverter.IsLittleEndian ? global::System.Numerics.BitOperations.TrailingZeroCount("
                + arguments[0] + ") : global::System.Numerics.BitOperations.LeadingZeroCount(" + arguments[0] + ")) >> 3)"),
        "is-little-endian.u32" => Endian(false),
        "is-little-endian.i32" => Endian(true),
        "popcount.u64" => new("global::System.Numerics.BitOperations.PopCount",
            "signed 32-bit result and one unsigned 64-bit integer",
            type => Integer(type.Return, 4, true) && type.Params.Count == 1
                && Integer(type.Params[0], 8, false)),
        _ => null,
    };

    private static bool Integer(CType type, int bytes, bool signed) =>
        type.Unqualified is CType.Prim { Integer: true } p && p.Bytes == bytes && p.Signed == signed;

    private static bool Scalar(CType.Func type, int resultBytes, int parameterBytes) =>
        Integer(type.Return, resultBytes, false) && type.Params.Count == 1 && Integer(type.Params[0], parameterBytes, false);

    private static FunctionOverrideIntrinsic ByteSwap(int bytes) => new(
        "global::System.Buffers.Binary.BinaryPrimitives.ReverseEndianness",
        "unsigned " + bytes * 8 + "-bit result and one unsigned integer of the same width",
        type => Scalar(type, bytes, bytes));

    private static FunctionOverrideIntrinsic CountZeros(int bytes, bool leading) => new(
        "global::System.Numerics.BitOperations." + (leading ? "LeadingZeroCount" : "TrailingZeroCount"),
        "unsigned 32-bit result and one unsigned " + bytes * 8 + "-bit integer",
        type => Scalar(type, 4, bytes), AdaptResult: call => "((uint)(" + call + "))");

    private static FunctionOverrideIntrinsic Endian(bool signed) => new(
        "global::System.BitConverter.IsLittleEndian",
        (signed ? "signed" : "unsigned") + " 32-bit result and no parameters",
        type => Integer(type.Return, 4, signed) && type.Params.Count == 0,
        RenderExpression: _ => signed ? "(global::System.BitConverter.IsLittleEndian ? 1 : 0)"
            : "(global::System.BitConverter.IsLittleEndian ? 1U : 0U)",
        RenderDiscardExpression: _ => "");

    private static bool BufferPointer(CType type, bool store) =>
        type.Unqualified is CType.Pointer pointer
            && (pointer.Pointee.Unqualified == CType.UChar || pointer.Pointee.Unqualified is CType.VoidType)
            && (!store || !pointer.Pointee.IsConst);

    private static FunctionOverrideIntrinsic Memory(bool store, int bytes, bool signed, string order)
    {
        var scalar = signed ? "int" : bytes == 2 ? "ushort" : bytes == 4 ? "uint" : "ulong";
        var method = order == "native"
            ? "global::System.Runtime.CompilerServices.Unsafe." + (store ? "WriteUnaligned<" : "ReadUnaligned<") + scalar + ">"
            : "global::System.Buffers.Binary.BinaryPrimitives." + (store ? "Write" : "Read")
                + (signed ? "Int" : "UInt") + bytes * 8 + (order == "le" ? "LittleEndian" : "BigEndian");
        return new(method,
            store ? "void(void * or unsigned char *, unsigned " + bytes * 8 + "-bit integer)"
                : (signed ? "signed " : "unsigned ") + bytes * 8 + "-bit result and one void or unsigned-byte pointer",
            type => type.Params.Count == (store ? 2 : 1) && BufferPointer(type.Params[0], store)
                && (store ? type.Return.Unqualified is CType.VoidType && Integer(type.Params[1], bytes, signed)
                    : Integer(type.Return, bytes, signed)),
            order == "native" ? null : arguments =>
            {
                var buffer = "new global::System." + (store ? "Span" : "ReadOnlySpan")
                    + "<byte>(" + arguments[0] + ", " + bytes + ")";
                return store ? new[] { buffer, arguments[1] } : new[] { buffer };
            });
    }
}
