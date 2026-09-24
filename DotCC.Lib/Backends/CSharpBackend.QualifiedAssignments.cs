#nullable enable
using DotCC.Ir;

namespace DotCC.Backends;

internal sealed partial class CSharpBackend
{
    private string QualifiedPointerSlot(CExpr target, string bare) => StoredAsNint(target) ? bare
        : RootsAtGlobal(target) ? $"*(nint*)({GlobalStorageAddress(target)})" : $"*(nint*)(&{bare})";

    private (string, int) VolatileAssignment(Assign assignment) => assignment.CompoundOp is { } operation
        ? VolatileUpdate(assignment.Target, operation, assignment.Value, false)
        : VolatileStore(assignment.Target, assignment.Value);

    private (string, int) VolatileStore(CExpr target, CExpr value)
    {
        var slot = BareLValue(target);
        var logical = Cs(target.Type.Unqualified);
        string stored = Coerced(value, target.Type.Unqualified);
        // Generic inference does not use C# constant narrowing to infer T.
        if (Cs(value.Type.Unqualified) != logical && !target.Type.IsPointerLowered)
            stored = $"({logical})({stored})";
        if (target.Type.IsPointerLowered)
        {
            slot = QualifiedPointerSlot(target, slot);
            return ($"({logical})VolatileValue.Store(ref {slot}, (nint)({stored}))", PUnary);
        }
        return ($"VolatileValue.Store(ref {slot}, {stored})", PPrimary);
    }

    private (string, int) VolatileUpdate(CExpr target, BinOp operation, CExpr value, bool returnOld)
    {
        var type = target.Type.Unqualified;
        var slot = BareLValue(target);
        bool pointer = target.Type.IsPointerLowered;
        if (pointer) slot = QualifiedPointerSlot(target, slot);
        string oldName = $"__volatileOld{_clCounter++}", argName = $"__volatileArg{_clCounter++}";
        CExpr old = new NameRef(pointer ? $"(({Cs(type)}){oldName})" : oldName) { Type = type };
        CExpr argument = new NameRef(argName) { Type = value.Type.Unqualified };
        var resultType = pointer ? type : operation is BinOp.Shl or BinOp.Shr
            ? CType.IntegerPromote(type) : CType.UsualArithmetic(type, argument.Type);
        var calculation = new Binary(operation, old, argument) { Type = resultType };
        string result = Coerced(calculation, type);
        if (pointer) result = $"(nint)({result})";
        string call = $"VolatileValue.Update(ref {slot}, {Expr(value)}, static ({oldName}, {argName}) => unchecked({result}), {(returnOld ? "true" : "false")})";
        return pointer ? ($"({Cs(type)}){call}", PUnary) : (call, PPrimary);
    }
}
