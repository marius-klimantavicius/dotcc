using System;
using Item = global::LALR.CC.LexicalGrammar.Item;

namespace DotCC.Ir;

internal sealed partial class IrBuilder
{
    private bool TryGnuScalarAlignment(Item attributes, out int alignment)
    {
        var found = false;
        var strongest = 0;
        bool Walk(Item item)
        {
            switch (item.Content)
            {
                case C.AttrListCons list: return Walk(list.Arg0) && Walk(list.Arg2);
                case C.AttrIdent name when Tok(name.Arg0).Trim('_') == "unused": return true;
                case C.AttrCall call when Tok(call.Arg0).Trim('_') == "aligned":
                    found = true;
                    strongest = Math.Max(strongest, CheckedAlignment(ConstEval(BuildExpr(call.Arg2))
                        ?? throw new IrUnsupportedException("GNU aligned requires a constant integer")));
                    return true;
                default: return false;
            }
        }
        var supported = attributes.Content is C.GnuFunctionAttrs attrs && Walk(attrs.Arg3);
        alignment = strongest;
        return supported && found;
    }

    private static CType WithScalarAlignment(CType type, int alignment)
    {
        if (type.Unqualified is not (CType.Prim or CType.LongDoubleStorage16 or CType.Pointer or CType.Func or CType.Enum))
            throw new IrUnsupportedException("GNU aligned type attribute currently requires a scalar type");
        return type with { AlignmentOverride = alignment };
    }
}
