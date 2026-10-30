using DotCC.Ir;
using Index = DotCC.Ir.Index;

namespace DotCC.Backends;

internal sealed partial class CSharpBackend
{
    private readonly HashSet<string> _definedFunctionNames = new(StringComparer.Ordinal);

    private string? AllocationSizeArgument(Call call, int index)
    {
        // The runtime's int malloc overload has the same allocation contract as
        // its size_t overload for a single C# sizeof result (always nonnegative
        // and int-sized). Keep that simple argument bare; array-size products
        // and all other size_t expressions still require unsigned rendering.
        // Relocatable calls can still bind to a size_t-only C definition later.
        if (_relocatable || index != 0 || call.CalleeSym is not { Name: "malloc", FromSystemHeader: true } function
            || _definedFunctionNames.Contains(function.TargetName)
            || call.ParamTypes is not { Count: 1 } parameters || parameters[0].Unqualified != CType.SizeT)
            return null;
        var argument = call.Args[index];
        while (argument is Paren parenthesized) argument = parenthesized.Inner;
        return argument is SizeOfExpr size && size.Of.Unqualified is not CType.Array
            ? SizeofText(size.Of) : null;
    }

    private static bool IsArrayObject(CExpr expression)
    {
        while (expression is Paren parenthesized) expression = parenthesized.Inner;
        return expression.Type.Unqualified is CType.Array || expression is VarRef { Sym.DeclaredArrayType: not null };
    }

    // BCL Volatile.Read<T> is a reference-type overload. C's _Bool, enums,
    // extended scalars and aggregate values instead need our unmanaged load.
    private string TypedVolatileRead(CType type, string storage) => Cs(type.Unqualified) is
        "bool" or "byte" or "sbyte" or "short" or "ushort" or "int" or "uint"
        or "long" or "ulong" or "nint" or "nuint" or "float" or "double"
        ? VolatileRead(storage)
        : $"Libc.VolatileValue.Load(ref {storage})";

    // Member selection uses an aggregate's storage, not its value. In
    // particular ((*volatile_pointer)).field must not read a temporary
    // aggregate before reading/writing the selected qualified field.
    private string TypedMemberBase(Member member)
    {
        if (member.Arrow) return Sub(member.Base, PPostfix);
        var aggregate = member.Base;
        while (aggregate is Paren parenthesized) aggregate = parenthesized.Inner;
        if (!aggregate.IsLValue) return Sub(member.Base, PPostfix);
        var storage = BareLValue(aggregate);
        return aggregate is VarRef or Member or Index ? storage : $"({storage})";
    }

    // Unary arithmetic on an enum renders its promoted compatible integer.
    // Older IR can still carry the enum type on that expression, including
    // through parentheses/comma expressions. Sink conversions must follow the
    // emitted value rather than incorrectly accepting it as the original enum.
    private static CType RenderedScalarType(CExpr value)
    {
        if (value.Type.Unqualified is not CType.Enum enumeration) return value.Type.Unqualified;
        return IsEnumValueRendered(value) ? enumeration : CType.IntegerPromote(enumeration.Underlying);
    }

    private static bool IsEnumValueRendered(CExpr value) => value.Type.Unqualified is CType.Enum && value switch
    {
        Paren parenthesized => IsEnumValueRendered(parenthesized.Inner),
        Unary { Op: UnOp.Plus or UnOp.Neg or UnOp.BitNot } => false,
        CommaOp comma when comma.Items.Count > 0 => IsEnumValueRendered(comma.Items[^1]),
        CommaSeq comma when comma.Items.Count > 0 => IsEnumValueRendered(comma.Items[^1]),
        ComptimeFold { Resolved: { } resolved } => IsEnumValueRendered(resolved),
        _ => true,
    };

    // CBool exposes explicit integer conversions at several widths. An
    // explicit C# floating cast has several equally valid paths through those
    // conversions; normalize through C's integer 0/1 value first.
    private string TypedNumericCastOperand(CExpr value, string target) =>
        target != "CBool" && IsBooleanRendered(value)
            ? BooleanIntegerText(value) : Sub(value, PUnary);

    private string BooleanIntegerText(CExpr value)
    {
        var inner = value;
        while (inner is Paren parenthesized) inner = parenthesized.Inner;
        return inner is LitBool literal ? literal.Value ? "1" : "0" : $"(int)({Expr(value)})";
    }

    private string TypedCastOperand(CExpr value, CType target)
    {
        if (target.Unqualified is CType.Prim or CType.Enum && target.Unqualified != CType.Bool
            && IsBooleanRendered(value)) return BooleanIntegerText(value);
        var inner = value;
        while (inner is Paren parenthesized) inner = parenthesized.Inner;
        // C# can parse (CBool)*p or (System.UInt128)*p as multiplication,
        // and (Enum)-x as subtraction. A parenthesized prefix operand is
        // unambiguous for named scalar types and ordinary primitive casts alike.
        return inner is Unary
            ? $"({Expr(value)})" : Sub(value, PUnary);
    }
}
