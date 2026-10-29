using Item = global::LALR.CC.LexicalGrammar.Item;

namespace DotCC.Ir;

internal sealed partial class IrBuilder
{
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
