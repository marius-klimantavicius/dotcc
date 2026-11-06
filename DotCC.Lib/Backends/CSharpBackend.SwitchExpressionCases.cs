using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using DotCC.Ir;

namespace DotCC.Backends;

internal sealed partial class CSharpBackend
{
    // Repositioning Zig's else prong cannot turn overlapping source cases into
    // an ordered first-match choice. Keep duplicate/range obligations explicit
    // for folded integer labels; C# still validates every rendered pattern.
    private static void ValidateSwitchExpressionCases(SwitchExpr expression)
    {
        if (expression.Arms.Count(a => a.Labels is null) > 1)
            throw new CompileException("switch expression has multiple default prongs");
        var source = expression.Subject.Type.Unqualified;
        if (source is CType.Enum enumeration) source = enumeration.Underlying;
        source = CType.IntegerPromote(source).Unqualified;
        if (source is not CType.Prim { Integer: true, Bytes: <= 8 } primitive) return;
        var intervals = new List<(BigInteger Low, BigInteger High)>();
        BigInteger Ordinal(long value) => primitive.Signed ? TruncateTo(value, primitive)
            : primitive.Bytes == 8 ? new BigInteger(unchecked((ulong)value)) : TruncateTo(value, primitive);
        foreach (var arm in expression.Arms)
            foreach (var label in arm.Labels ?? [])
            {
                if (label.CaseExpr is not { } lowExpression || !FoldConst(lowExpression, out var low)) continue;
                var high = low;
                if (label.HiExpr is { } highExpression && !FoldConst(highExpression, out high)) continue;
                var first = Ordinal(low); var last = Ordinal(high);
                if (first > last) throw new CompileException("switch expression has a reversed case range");
                if (intervals.Any(previous => first <= previous.High && last >= previous.Low))
                    throw new CompileException("switch expression has overlapping case values");
                intervals.Add((first, last));
            }
    }
}
