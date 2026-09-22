using System;
using System.Collections.Generic;
using DotCC.Ir;

namespace DotCC;

// One closed registry shared by profile validation, C signature checking and
// emission. These targets retain ordinary call effects in the IR.
internal sealed record FunctionOverrideIntrinsic(
    string ManagedMethod,
    string SignatureDescription,
    Func<CType.Func, bool> MatchesSignature,
    Func<IReadOnlyList<string>, IReadOnlyList<string>>? AdaptArguments = null)
{
    // Arguments have already been rendered by the backend. Each descriptor
    // must retain their order and use each exactly once.
    public string RenderCall(IReadOnlyList<string> arguments) =>
        ManagedMethod + "(" + string.Join(", ", AdaptArguments?.Invoke(arguments) ?? arguments) + ")";

    public static FunctionOverrideIntrinsic? Find(string name) => name switch
    {
        "load.i32.le" => LittleEndian(false, 4, true),
        "load.u16.le" => LittleEndian(false, 2, false),
        "load.u32.le" => LittleEndian(false, 4, false),
        "load.u64.le" => LittleEndian(false, 8, false),
        "store.u16.le" => LittleEndian(true, 2, false),
        "store.u32.le" => LittleEndian(true, 4, false),
        "store.u64.le" => LittleEndian(true, 8, false),
        "popcount.u64" => new("global::System.Numerics.BitOperations.PopCount",
            "signed 32-bit result and one unsigned 64-bit integer",
            type => Integer(type.Return, 4, true) && type.Params.Count == 1
                && Integer(type.Params[0], 8, false)),
        _ => null,
    };

    private static bool Integer(CType type, int bytes, bool signed) =>
        type.Unqualified is CType.Prim { Integer: true } p && p.Bytes == bytes && p.Signed == signed;

    private static FunctionOverrideIntrinsic LittleEndian(bool store, int bytes, bool signed) => new(
        "global::System.Buffers.Binary.BinaryPrimitives." + (store ? "Write" : "Read")
            + (signed ? "Int" : "UInt") + bytes * 8 + "LittleEndian",
        store ? "void(unsigned char *, unsigned " + bytes * 8 + "-bit integer)"
            : (signed ? "signed " : "unsigned ") + bytes * 8 + "-bit result and one unsigned-byte pointer",
        type => type.Params.Count == (store ? 2 : 1)
            && type.Params[0].Unqualified is CType.Pointer pointer
            && pointer.Pointee.Unqualified == CType.UChar
            && (!store || !pointer.Pointee.IsConst)
            && (store ? type.Return.Unqualified is CType.VoidType && Integer(type.Params[1], bytes, signed)
                : Integer(type.Return, bytes, signed)),
        arguments =>
        {
            var buffer = "new global::System." + (store ? "Span" : "ReadOnlySpan")
                + "<byte>(" + arguments[0] + ", " + bytes + ")";
            return store ? new[] { buffer, arguments[1] } : new[] { buffer };
        });
}
