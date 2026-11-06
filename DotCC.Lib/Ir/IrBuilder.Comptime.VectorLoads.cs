using System;

namespace DotCC.Ir;

internal sealed partial class IrBuilder
{
    private bool TryEvalComptimeVectorLoad(Call call, out ComptimeValue? value)
    {
        value = null;
        if (call.Callee is not ("ZigVec.Load64" or "ZigVec.Load128" or "ZigVec.Load256" or "ZigVec.Load512")
            || call.Type.Unqualified is not CType.Vector { Element.Unqualified: CType.Prim { Integer: true } } vector
            || call.Args.Count != 1) return false;
        var source = EvalComptime(call.Args[0]);
        if (ComptimeArrayWindow(source) is not var (elements, offset, element)
            || element.Unqualified != vector.Element.Unqualified)
        {
            ComptimeMiss ??= "vector load requires a known array window with matching integer elements";
            return true;
        }
        var available = source is CtSlice slice ? slice.Length : elements.LongLength - offset;
        if (offset < 0 || vector.Count < 0 || available < vector.Count || offset + vector.Count > elements.LongLength)
            throw new ComptimeAbort("comptime vector load is out of bounds");
        var lanes = new ComptimeValue[vector.Count];
        for (var i = 0; i < lanes.Length; i++)
        {
            var lane = elements[offset + i];
            if (lane is CtUndefined) throw new ComptimeAbort("comptime vector load reads an undefined element");
            lanes[i] = CloneComptime(lane);
        }
        // The vector's logical source lane count controls this value. Padded
        // managed SIMD carrier lanes do not add source elements or memory reads.
        value = new CtArray(lanes, vector.Element, vector);
        return true;
    }
}
