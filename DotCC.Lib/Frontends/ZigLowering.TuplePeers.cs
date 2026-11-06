#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using DotCC.Ir;

namespace DotCC.Frontends;
internal sealed partial class ZigLowering
{
    // Resolve only source tuple values: an untyped integer meets a declared
    // integer, and null meets a source slice/pointer. Physical integer/pointer
    // carriers alone are not evidence for either conversion.
    private CType.Tuple? TupleValuePeer(IReadOnlyList<TupleNew> arms)
    {
        if (arms.Count == 0) return null;
        var count = arms[0].Elements.Count;
        if (arms.Any(a => a.Elements.Count != count)) return null;
        var types = new List<CType>(count);
        for (var i = 0; i < count; i++)
        {
            var slot = i;
            var values = arms.Select(a => a.Elements[slot]).ToArray();
            var declared = arms.Select(a => ((CType.Tuple)a.TupleType.Unqualified).Elements[slot]).ToArray();
            var concrete = values.Select((v, j) => (Value: v, Type: declared[j]))
                .Where(p => p.Value is not NullPtr && p.Type.Unqualified is not CType.Prim { IsComptimeInt: true }).ToArray();
            if (concrete.Length == 0)
            {
                if (declared.Any(t => t != declared[0])) return null;
                types.Add(declared[0]);
                continue;
            }
            var type = concrete[0].Type;
            if (type.Unqualified is CType.Pointer pointer)
            {
                var inner = pointer with { ZigOptional = false };
                if (concrete.Any(p => p.Type.Unqualified is not CType.Pointer other
                    || (other with { ZigOptional = false }) != inner)) return null;
                type = pointer with { ZigOptional = values.Any(v => v is NullPtr)
                    || concrete.Any(p => p.Type.Unqualified is CType.Pointer { ZigOptional: true }) };
            }
            else if (type.Unqualified is CType.Slice)
            {
                if (concrete.Any(p => p.Type != type)) return null;
                if (values.Any(v => v is NullPtr)) type = new CType.Optional(type);
            }
            else if (type.Unqualified is CType.Prim { Integer: true, IsComptimeInt: false } integer)
            {
                if (concrete.Any(p => p.Type != type)) return null;
                for (var j = 0; j < values.Length; j++)
                    if (declared[j].Unqualified is CType.Prim { IsComptimeInt: true })
                    {
                        var constant = _ir.EvalComptimeValue(values[j]) as IrBuilder.CtInt;
                        if (constant is null) return null;
                        var evidence = arms.First(a => ReferenceEquals(a.Elements[slot], concrete[0].Value));
                        var bits = TupleResultBits(evidence)?[slot] ?? BitcastLoweredLocation(concrete[0].Value).IntegerBits;
                        if (bits is null) return null;
                        var min = integer.Signed ? -(BigInteger.One << (bits.Value - 1)) : BigInteger.Zero;
                        var max = (BigInteger.One << (integer.Signed ? bits.Value - 1 : bits.Value)) - 1;
                        if (constant.Value < min || constant.Value > max)
                            throw new IrUnsupportedException("zig tuple peer integer constant does not fit its declared integer peer");
                    }
            }
            else if (concrete.Any(p => p.Type != type) || values.Any(v => v is NullPtr)) return null;
            types.Add(type);
        }
        return new CType.Tuple(types);
    }

    private int?[]? TuplePeerSourceBits(params CExpr[] values)
    {
        var arms = values.Where(v => !IsNoreturn(v)).OfType<TupleNew>().ToArray();
        if (arms.Length == 0 || arms.Any(a => a.Elements.Count != arms[0].Elements.Count)) return null;
        return Enumerable.Range(0, arms[0].Elements.Count).Select(index =>
        {
            var concrete = arms.Where(a => ((CType.Tuple)a.TupleType.Unqualified).Elements[index].Unqualified
                is not CType.Prim { IsComptimeInt: true }).ToArray();
            var bits = concrete.Select(a => TupleResultBits(a)?[index]).Distinct().ToArray();
            return bits.Length == 1 ? bits[0] : null;
        }).ToArray();
    }

    private CExpr CoerceTupleValuePeer(CExpr value, CType target)
    {
        if (value is not TupleNew tuple || target.Unqualified is not CType.Tuple type
            || tuple.Elements.Count != type.Elements.Count) return value;
        var elements = tuple.Elements.Select((element, index) =>
        {
            var sink = type.Elements[index];
            if (element is NullPtr)
                return sink.Unqualified is CType.Optional ? (CExpr)new DefaultLit { Type = sink }
                    : new NullPtr { Type = sink };
            return element.Type == sink ? element : new Cast(sink, element) { Type = sink };
        }).ToList();
        return new TupleNew(elements, type) { Type = type };
    }
}
