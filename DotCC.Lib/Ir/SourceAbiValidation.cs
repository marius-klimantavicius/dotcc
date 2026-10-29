using System.Collections.Generic;

namespace DotCC.Ir;

internal static class SourceAbiValidation
{
    private static bool Storage(CType type) => type.Unqualified is CType.LongDoubleStorage16;
    private static void Unsupported() => throw new IrUnsupportedException(
        "source ABI long double x87-storage16 supports storage/layout only; floating arithmetic and conversions are unsupported");

    internal static void Conversion(CType source, CType target)
    {
        if ((Storage(source) || Storage(target)) && source.Unqualified != target.Unqualified) Unsupported();
    }

    private static void Condition(CExpr value)
    {
        if (Storage(value.Type)) Unsupported();
    }

    internal static void NumericType(CType type)
    {
        if (Storage(type)) Unsupported();
    }

    private static CType.Func? Signature(CType type) => type.Unqualified switch
    {
        CType.Func function => function,
        CType.Pointer { Pointee: { } pointee } => pointee.Unqualified as CType.Func,
        _ => null,
    };

    private static void NativeCall(CType.Func? signature)
    {
        if (signature is not { IsNativeCallConv: true }) return;
        bool scalar = Storage(signature.Return);
        foreach (var parameter in signature.Params) scalar |= Storage(parameter);
        if (scalar) throw new IrUnsupportedException(
            "source ABI long double x87-storage16 supports storage/layout only; native x87 scalar calling ABI is unsupported");
    }

    private static void Arguments(IReadOnlyList<CExpr> arguments, IReadOnlyList<CType>? parameters)
    {
        for (int index = 0; index < arguments.Count; index++)
        {
            if (parameters is not null && index < parameters.Count)
                Conversion(arguments[index].Type, parameters[index]);
            else if (Storage(arguments[index].Type))
                throw new IrUnsupportedException(
                    "source ABI long double x87-storage16 supports storage/layout only; variadic or unprototyped scalar arguments are unsupported");
        }
    }

    internal static void Expression(CExpr expression, bool materializing = false)
    {
        switch (expression)
        {
            case LitFloat literal when materializing && Storage(literal.Type): Unsupported(); break;
            case Binary binary when Storage(binary.Left.Type) || Storage(binary.Right.Type): Unsupported(); break;
            case Unary unary when Storage(unary.Operand.Type) && unary.Op is not (UnOp.AddrOf or UnOp.Deref): Unsupported(); break;
            case Cast cast: Conversion(cast.Operand.Type, cast.Target); break;
            case Assign assign:
                if (assign.CompoundOp is not null && Storage(assign.Target.Type)) Unsupported();
                Conversion(assign.Value.Type, assign.Target.Type);
                break;
            case CondExpr conditional:
                Condition(conditional.Cond);
                Conversion(conditional.Then.Type, conditional.Type);
                Conversion(conditional.Else.Type, conditional.Type);
                break;
            case Index index: Condition(index.Idx); break;
            case Call call:
                NativeCall(call.CalleeSym is { } symbol ? Signature(symbol.Type) : null);
                Arguments(call.Args, call.ParamTypes);
                break;
            case IndirectCall call:
                var signature = Signature(call.Callee.Type);
                NativeCall(signature);
                Arguments(call.Args, signature?.Params);
                break;
            case VaArgGet argument: NumericType(argument.Target); break;
        }
    }

    internal static void Statement(CStmt statement)
    {
        switch (statement)
        {
            case If branch: Condition(branch.Cond); break;
            case While loop: Condition(loop.Cond); break;
            case DoWhile loop: Condition(loop.Cond); break;
            case For { Cond: { } condition }: Condition(condition); break;
            case Switch selection: Condition(selection.Subject); break;
        }
    }
}
