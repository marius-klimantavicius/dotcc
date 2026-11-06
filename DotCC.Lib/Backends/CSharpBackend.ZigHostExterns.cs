using System;
using System.Linq;
using DotCC.Ir;

namespace DotCC.Backends;

internal sealed partial class CSharpBackend
{
    private string ZigHostExternCall(Call call, string name)
    {
        if (call.ParamTypes is not { } parameters || call.Args.Count < parameters.Count)
            throw new IrUnsupportedException("zig source libc host binding lost its declared signature: " + name);
        string Pointer(int index)
        {
            var argument = call.Args[index];
            // Zig permits a sentinel slice at a many-pointer parameter. The
            // borrowed data pointer is the C argument; the length is not an ABI slot.
            return argument.Type.Unqualified is CType.Slice
                ? $"{Sub(argument, PPostfix)}.Ptr" : Expr(argument);
        }
        string Packed(int index)
        {
            var argument = call.Args[index];
            if (ZigStorage(parameters[index]) is not { Kind: ZigStorageKind.PackedInteger, LogicalBits: 32, NativeSize: 4 })
                throw new IrUnsupportedException("zig source libc host flags lost their source packed u32 layout: " + name);
            return $"ZigBitcastWidth.Scalar<{Cs(argument.Type)}, uint>({Expr(argument)}, 32, false)";
        }
        string Scalar(int index, CType type) => CoercedArg(call.Args[index], type, bindRuntimeOverload: true);
        string[] arguments = name switch
        {
            "__errno_location" => [],
            "malloc_usable_size" => [Pointer(0)],
            "openat64" when call.Args.Count is 3 or 4 => [Scalar(0, CType.Int), Pointer(1), Packed(2),
                call.Args.Count == 4 ? Scalar(3, CType.UInt) : "0U"],
            "unlinkat" => [Scalar(0, CType.Int), Pointer(1), Scalar(2, CType.UInt)],
            "preadv64" => [Scalar(0, CType.Int), Pointer(1), Scalar(2, CType.UInt), Scalar(3, CType.Long)],
            "lseek64" => [Scalar(0, CType.Int), Scalar(1, CType.Long), Scalar(2, CType.Int)],
            "mmap64" => [Pointer(0), Scalar(1, CType.ULong), Packed(2), Packed(3), Scalar(4, CType.Int), Scalar(5, CType.Long)],
            "mremap" when call.Args.Count is 4 or 5 => [Pointer(0), Scalar(1, CType.ULong), Scalar(2, CType.ULong), Packed(3),
                call.Args.Count == 5 ? Pointer(4) : "null"],
            _ => throw new IrUnsupportedException("unsupported source libc host call arity: " + name),
        };
        return $"__dotcc_zig_c_{name}({string.Join(", ", arguments)})";
    }
}
