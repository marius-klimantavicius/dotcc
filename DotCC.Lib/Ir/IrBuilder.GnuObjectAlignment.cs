#nullable enable
using System;
using Item = global::LALR.CC.LexicalGrammar.Item;

namespace DotCC.Ir;

internal sealed partial class IrBuilder
{
    private int GnuObjectAlignment(Item attributes)
    {
        if (attributes.Content is not C.GnuFunctionAttrs list)
            throw new IrUnsupportedException("GNU object attributes");
        int Alignment(Item item) => item.Content switch
        {
            C.AttrListCons pair => Math.Max(Alignment(pair.Arg0), Alignment(pair.Arg2)),
            C.AttrCall call when Tok(call.Arg0).Trim('_') == "aligned" =>
                CheckedAlignment(ConstEval(BuildExpr(call.Arg2))
                    ?? throw new IrUnsupportedException("GNU aligned requires a constant integer")),
            C.AttrIdent name when Tok(name.Arg0).Trim('_') == "unused" => 0,
            _ => throw new IrUnsupportedException("unsupported GNU static object attribute"),
        };
        return Alignment(list.Arg3);
    }
}
