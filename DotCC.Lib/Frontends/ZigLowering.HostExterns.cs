#nullable enable
using System.Linq;
using DotCC.Ir;

namespace DotCC.Frontends;

internal sealed partial class ZigLowering
{
    // Only declarations from the configured source stdlib acquire this binding.
    // Preserve it on the call even though ordinary libc calls erase CalleeSym.
    private void BindSourceHostExtern(Symbol symbol)
    {
        if (!UsesSourceStdlib || !IsStdModule("c.zig")) return;
        var name = symbol.Name;
        if (name is not ("openat64" or "unlinkat" or "preadv64" or "lseek64"
            or "mmap64" or "mremap" or "__errno_location" or "malloc_usable_size")) return;
        var fn = (CType.Func)symbol.Type;
        var p = fn.Params;
        static bool Integer(CType type, int bytes, bool signed) => type.Unqualified is CType.Prim
            { Integer: true } integer && integer.Bytes == bytes && integer.Signed == signed;
        static bool Pointer(CType type) => type.Unqualified is CType.Pointer;
        bool Packed32(int slot) => p[slot].Unqualified is CType.Named named
            && _ir.Types.FirstOrDefault(t => t.Name == named.Name)?.ZigStorage is
                { Kind: ZigStorageKind.PackedInteger, LogicalBits: 32, NativeSize: 4 };
        bool Iovec(CType type) => type.Unqualified is CType.Pointer { Pointee.Unqualified: CType.Named named }
            && _ir.SizeOfConst(named) == 16 && _ir.StructFieldsOf(named.Name) is { Count: 2 } fields
            && Pointer(fields[0].Type) && Integer(fields[1].Type, 8, false);
        bool valid = name switch
        {
            "__errno_location" => p.Count == 0 && !fn.Variadic && fn.Return.Unqualified is CType.Pointer result
                && Integer(result.Pointee, 4, true),
            "malloc_usable_size" => p.Count == 1 && !fn.Variadic && Pointer(p[0]) && Integer(fn.Return, 8, false),
            "openat64" => p.Count == 3 && fn.Variadic && Integer(fn.Return, 4, true)
                && Integer(p[0], 4, true) && Pointer(p[1]) && Packed32(2),
            "unlinkat" => p.Count == 3 && !fn.Variadic && Integer(fn.Return, 4, true)
                && Integer(p[0], 4, true) && Pointer(p[1]) && Integer(p[2], 4, false),
            "preadv64" => p.Count == 4 && !fn.Variadic && Integer(fn.Return, 8, true)
                && Integer(p[0], 4, true) && Iovec(p[1]) && Integer(p[2], 4, false) && Integer(p[3], 8, true),
            "lseek64" => p.Count == 3 && !fn.Variadic && Integer(fn.Return, 8, true)
                && Integer(p[0], 4, true) && Integer(p[1], 8, true) && Integer(p[2], 4, true),
            "mmap64" => p.Count == 6 && !fn.Variadic && Pointer(fn.Return) && Pointer(p[0])
                && Integer(p[1], 8, false) && Packed32(2) && Packed32(3)
                && Integer(p[4], 4, true) && Integer(p[5], 8, true),
            "mremap" => p.Count == 4 && fn.Variadic && Pointer(fn.Return) && Pointer(p[0])
                && Integer(p[1], 8, false) && Integer(p[2], 8, false) && Packed32(3),
            _ => false,
        };
        if (!valid) throw new IrUnsupportedException("zig source libc host binding requires its Linux x64 LP64 signature: " + name);
        symbol.IntrinsicOverride = new("zigHostExtern", name);
    }
}
