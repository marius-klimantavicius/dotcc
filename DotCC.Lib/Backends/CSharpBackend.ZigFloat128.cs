using DotCC.Ir;

namespace DotCC.Backends;

internal sealed partial class CSharpBackend
{
    /// <summary>Use named runtime conversions where C# operators lack the required width.
    /// Binary128-to-binary32 rounds directly so bits below a binary64 midpoint remain observable.</summary>
    private (string, int)? RenderFloat128Conversion(Cast c)
    {
        var to = c.Target.Unqualified;
        var from = c.Operand.Type?.Unqualified;
        static string? Wide(CType? t) => t switch
        {
            CType.Prim { Integer: true, Bytes: 16, Signed: true } => "Int128",
            CType.Prim { Integer: true, Bytes: 16, Signed: false } => "UInt128",
            CType.Prim { Integer: true, Bytes: 8, Signed: false } => "UInt64",
            _ => null,
        };
        if (to is CType.Float128Type && Wide(from) is { } fromWide)
        {
            return ($"Float128.From{fromWide}({Sub(c.Operand, PAssign)})", PPrimary);
        }
        if (from is CType.Float128Type && Wide(to) is { } toWide)
        {
            return ($"Float128.To{toWide}({Sub(c.Operand, PAssign)})", PPrimary);
        }
        if (from is CType.Float128Type && to == CType.Float)
        {
            return ($"Float128.ToSingle({Sub(c.Operand, PAssign)})", PPrimary);
        }
        return null;
    }

}
