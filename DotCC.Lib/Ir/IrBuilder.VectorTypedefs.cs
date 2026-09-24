using Item = global::LALR.CC.LexicalGrammar.Item;

namespace DotCC.Ir;

internal sealed partial class IrBuilder
{
    private void BuildAttributedTypedef(Item typeItem, Item nameItem, Item attributeItem)
    {
        if (attributeItem.Content is not C.GnuFunctionAttrs attributes ||
            attributes.Arg3.Content is not C.AttrCall attribute ||
            Tok(attribute.Arg0).Trim('_') != "vector_size")
            throw new IrUnsupportedException("unsupported GNU typedef attribute");
        var element = ResolveType(typeItem);
        if (element.Unqualified is not CType.Prim { Bytes: > 0 })
            throw new IrUnsupportedException("GNU vector_size requires a primitive arithmetic element type");
        if (ConstEval(BuildExpr(attribute.Arg2)) is not { } bytes || bytes <= 0 || bytes > int.MaxValue ||
            bytes % element.SizeOf != 0 || (bytes & (bytes - 1)) != 0)
            throw new IrUnsupportedException("GNU vector_size requires a positive power-of-two byte size divisible by its element size");
        // Use the ordinary per-unit typedef registry. A block-local typedef of
        // the same name shadows this through the existing symbol lookup order.
        _typedefs[UserTypedefName(Tok(nameItem))] = new CType.UnsupportedVector(element, (int)bytes);
    }

    private static CType RequireSupportedTypedef(CType type, string name)
    {
        if (type.Unqualified is CType.UnsupportedVector vector)
            throw new IrUnsupportedException("GNU vector_size(" + vector.Bytes + ") typedef '" + name +
                "' is used; vector type lowering is not implemented");
        return type;
    }
}
