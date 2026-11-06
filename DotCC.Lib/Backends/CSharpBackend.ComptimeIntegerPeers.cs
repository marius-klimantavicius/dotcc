using DotCC.Ir;

namespace DotCC.Backends;

internal sealed partial class CSharpBackend
{
    // Zig's comptime_int carrier is not an arithmetic peer width. A fitting
    // constant adopts the concrete operand's type, including usize in pointer
    // indices. Keep typed i128/u128 operands and unproven magnitudes intact.
    private CExpr CoerceComptimeIntegerPeer(CExpr value, CType peer) =>
        value.Type.Unqualified is CType.Prim { IsComptimeInt: true }
        && peer.Unqualified is CType.Prim { Integer: true, IsComptimeInt: false }
        && TryConstInt(value, out var constant) && ConstFitsTarget(constant, Cs(peer.Unqualified))
            ? new Cast(peer, value) { Type = peer }
            : value;
}
