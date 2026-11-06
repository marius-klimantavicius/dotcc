#nullable enable
using System.Linq;
using DotCC.Ir;

namespace DotCC.Frontends;

internal sealed partial class ZigLowering
{
    // Bind the configured std file's OS wrapper, never a same-named user
    // declaration or arbitrary asm/syscall. Keep a real typed function body so
    // aliases and managed function pointers use exactly the same boundary.
    private Block? TryLowerSourceHostBody(Symbol function, System.Collections.Generic.List<Symbol> parameters)
    {
        if (!UsesSourceStdlib || !IsStdModule("os/linux.zig")) return null;
        if (function.Name == "getrandom")
            return new Block([new Return(new Call("__dotcc_linux_getrandom",
                parameters.Select(p => (CExpr)new VarRef(p) { Type = p.Type }).ToList()) { Type = ((CType.Func)function.Type).Return })]);
        if (function.Name is "futex_3arg" or "futex_4arg")
        {
            var futexArgs = parameters.Select(p => (CExpr)new VarRef(p) { Type = p.Type, IsLValue = true }).ToList();
            var layout = BitcastLayout(BitcastParamLocation(function, 1, parameters[1].Type), "Linux futex operation");
            if (layout.LogicalBits != 32) throw new IrUnsupportedException("zig Linux futex host binding requires a 32-bit source operation");
            futexArgs[1] = new BitCast(CType.UInt, futexArgs[1]) { Type = CType.UInt,
                ZigLayout = new(layout, new(32, false, ZigBitcastRepresentation.Integer)) };
            if (futexArgs.Count == 4)
                futexArgs[3] = new Cast(new CType.Pointer(CType.Void), futexArgs[3]) { Type = new CType.Pointer(CType.Void) };
            return new Block([new Return(new Call("__dotcc_linux_" + function.Name, futexArgs) { Type = ((CType.Func)function.Type).Return })]);
        }
        if (function.Name != "statx") return null;
        var signature = (CType.Func)function.Type;
        if (parameters.Count != 5 || signature.Return != CType.ULong
            || parameters[4].Type.Unqualified is not CType.Pointer output
            || _ir.SizeOfConst(output.Pointee) != 256)
            throw new IrUnsupportedException("zig Linux statx host binding requires the source Linux LP64 signature and 256-byte Statx layout");
        var maskLayout = BitcastLayout(BitcastParamLocation(function, 3, parameters[3].Type), "Linux statx request mask");
        if (maskLayout.LogicalBits != 32)
            throw new IrUnsupportedException("zig Linux statx host binding requires a 32-bit source request mask");
        var args = parameters.Select(p => (CExpr)new VarRef(p) { Type = p.Type, IsLValue = true }).ToList();
        args[3] = new BitCast(CType.UInt, args[3]) { Type = CType.UInt,
            ZigLayout = new(maskLayout, new(32, false, ZigBitcastRepresentation.Integer)) };
        args[4] = new Cast(new CType.Pointer(CType.Void), args[4]) { Type = new CType.Pointer(CType.Void) };
        return new Block([new Return(new Call("__dotcc_linux_statx", args) { Type = signature.Return })]);
    }
}
