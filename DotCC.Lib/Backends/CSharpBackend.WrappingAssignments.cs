#nullable enable
using DotCC.Ir;

namespace DotCC.Backends;

internal sealed partial class CSharpBackend
{
    private (string, int) ZigWrappingAssignment(Assign assignment)
    {
        var helper = assignment.CompoundOp switch
        {
            BinOp.Add => "Add",
            BinOp.Sub => "Sub",
            BinOp.Mul => "Mul",
            _ => throw new IrUnsupportedException("invalid Zig wrapping assignment operator"),
        };
        // Evaluating the receiver captures both storage and its old value before the RHS.
        var receiver = $"ZigMath.Wrapping(ref {BareLValue(assignment.Target)})";
        if (_canHoist)
        {
            var temporary = ZigPlaceLocal("__zig_wrap");
            _pending.Add($"var {temporary} = {receiver}");
            receiver = temporary;
        }
        return ($"{receiver}.{helper}(({Cs(assignment.Target.Type)})({Coerced(assignment.Value, assignment.Target.Type)}), {assignment.ZigWrappingBits})", PPrimary);
    }
}
