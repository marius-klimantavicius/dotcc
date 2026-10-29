using Item = global::LALR.CC.LexicalGrammar.Item;

namespace DotCC.Ir;

internal sealed partial class IrBuilder
{
    private static (Item Name, CType Type) QualifyPointerDeclarator(Item name, CType type)
    {
        while (true)
        {
            switch (name.Content)
            {
                case C.FnPtrArrayNameConst q: type = type.WithQuals(TypeQual.Const); name = q.Arg1; break;
                case C.FnPtrArrayNameVolatile q: type = type.WithQuals(TypeQual.Volatile); name = q.Arg1; break;
                case C.FnPtrArrayNameRestrict q: type = type.WithQuals(TypeQual.Restrict); name = q.Arg1; break;
                case C.FnPtrArrayNamePointer p: type = new CType.Pointer(type); name = p.Arg1; break;
                default: return (name, type);
            }
        }
    }

    private (string Name, CType Type) NamedArrayPointerParameter(Item element, Item name, Item? dimensions)
    {
        var array = dimensions is null ? new CType.Array(ResolveType(element), null)
            : MakeArrayType(ResolveType(element), TryConstDims(dimensions)
                ?? throw new IrUnsupportedException("array parameter inner dimensions must be constant"));
        var (terminal, type) = QualifyPointerDeclarator(name, new CType.Pointer(array));
        if (terminal.Content is not C.FnPtrScalarName scalar)
            throw new IrUnsupportedException("pointer-to-array parameter requires a scalar declarator name");
        return (Tok(scalar.Arg0), type);
    }

    private (Item Name, CType Type) CallbackParameter(Item declarator)
    {
        var (_, name, type) = CallbackDeclarator(declarator);
        return name.Content switch
        {
            C.FnPtrScalarName scalar => (scalar.Arg0, type),
            C.FnPtrArrayNameImplicit array => (array.Arg0, new CType.Pointer(type)),
            C.FnPtrArrayNameSized array => (array.Arg0, DecayParam(MakeArrayType(type,
                TryConstDims(array.Arg1) ?? throw new IrUnsupportedException("non-constant callback parameter array bound")))),
            C.FnPtrArrayNameImplicitOuter array => (array.Arg0, new CType.Pointer(MakeArrayType(type,
                TryConstDims(array.Arg3) ?? throw new IrUnsupportedException("non-constant callback parameter array bound")))),
            _ => throw new IrUnsupportedException(TypeName(name.Content)),
        };
    }

    private CType QualifiedFnPtrType(Item result, Item? parameters, Item modifiers)
    {
        CType type = FnPtrType(result, parameters);
        while (true)
        {
            switch (modifiers.Content)
            {
                case C.AbstractFnPtrConst q: type = type.WithQuals(TypeQual.Const); modifiers = q.Arg1; break;
                case C.AbstractFnPtrVolatile q: type = type.WithQuals(TypeQual.Volatile); modifiers = q.Arg1; break;
                case C.AbstractFnPtrRestrict q: type = type.WithQuals(TypeQual.Restrict); modifiers = q.Arg1; break;
                case C.AbstractFnPtrPointer p: type = new CType.Pointer(type); modifiers = p.Arg1; break;
                case C.AbstractFnPtrConstOne: return type.WithQuals(TypeQual.Const);
                case C.AbstractFnPtrVolatileOne: return type.WithQuals(TypeQual.Volatile);
                case C.AbstractFnPtrRestrictOne: return type.WithQuals(TypeQual.Restrict);
                case C.AbstractFnPtrPointerOne: return new CType.Pointer(type);
                default: throw new IrUnsupportedException(TypeName(modifiers.Content));
            }
        }
    }
}
