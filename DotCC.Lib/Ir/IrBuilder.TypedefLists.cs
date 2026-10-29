using System;
using Item = global::LALR.CC.LexicalGrammar.Item;

namespace DotCC.Ir;

internal sealed partial class IrBuilder
{
    private void BuildTypedefList(Item typeItem, Item names, bool local = false, Item? attributes = null)
    {
        // The generic typedef production can also consume an anonymous enum.
        // Retain its real enum identity under the first alias, as the dedicated
        // enum-typedef production does, rather than reducing it to plain int.
        var first = typeItem.Content is C.TypeAnonymousEnum anonymous
            ? RegisterEnum(null, null, anonymous.Arg2, FirstTypedefName(names)) : ResolveType(typeItem);
        if (attributes is not null)
        {
            if (TryGnuScalarAlignment(attributes, out var alignment)) first = WithScalarAlignment(first, alignment);
            else ValidateGnuSignatureAttributes(attributes);
        }
        var element = first;
        for (var i = 0; i < CountLiteralStars(typeItem); i++)
            element = element.Unqualified switch
            {
                CType.Pointer p => p.Pointee,
                CType.Func f => f with { IsFunctionType = true },
                _ => element,
            };
        BindTypedefNames(names, first, element, local);
    }

    private void BindTypedefNames(Item names, CType first, CType element, bool local = false)
    {
        void Add(Item name, CType type)
        {
            if (local) BuildLocalTypedef(name, type);
            else _typedefs[UserTypedefName(Tok(name))] = type;
        }
        void Bind(Item item, CType type)
        {
            switch (item.Content)
            {
                case C.TypedefName n:
                    Add(n.Arg0, type);
                    break;
                case C.TypedefArrayName n:
                    Add(n.Arg0, MakeArrayType(type,
                        TryConstDims(n.Arg1) ?? throw new IrUnsupportedException("non-constant array typedef bound")));
                    break;
                case C.TypedefNameTailPlain n: Bind(n.Arg0, type); break;
                case C.TypedefNameTailPtr n: Bind(n.Arg1, PointerType(type)); break;
                case C.TypedefNameTailConstPtr n: Bind(n.Arg2, PointerType(type).WithQuals(TypeQual.Const)); break;
                case C.TypedefNameTailVolatilePtr n: Bind(n.Arg2, PointerType(type).WithQuals(TypeQual.Volatile)); break;
                case C.TypedefNameTailRestrictPtr n: Bind(n.Arg2, PointerType(type).WithQuals(TypeQual.Restrict)); break;
                default: throw new IrUnsupportedException(TypeName(item.Content));
            }
        }
        void Walk(Item item)
        {
            switch (item.Content)
            {
                case C.TypedefNameListOne n: Bind(n.Arg0, first); break;
                case C.TypedefNameListCons n: Walk(n.Arg0); Bind(n.Arg2, element); break;
                default: throw new IrUnsupportedException(TypeName(item.Content));
            }
        }
        Walk(names);
    }

    private static string FirstTypedefName(Item item) => item.Content switch
    {
        C.TypedefNameListOne n => FirstTypedefName(n.Arg0),
        C.TypedefNameListCons n => FirstTypedefName(n.Arg0),
        C.TypedefName n => Tok(n.Arg0),
        C.TypedefArrayName n => Tok(n.Arg0),
        _ => throw new IrUnsupportedException(TypeName(item.Content)),
    };

    private void BuildAggregateTypedef(string? tag, Item members, Item names, bool union,
        int alignment = 0, int pack = 0, int? pragmaPack = null)
    {
        var alias = FirstTypedefName(names);
        var canonical = tag is null ? AnonymousTypedefName(members, alias) : AggregateName(tag);
        BuildStructDef(canonical, members, alias, union, alignment, pack, pragmaPack, tagged: tag is not null);
        var type = new CType.Named(canonical);
        BindTypedefNames(names, type, type);
    }

    private void BuildEnumTypedef(string? tag, Item members, Item names)
    {
        var type = RegisterEnum(tag, null, members, FirstTypedefName(names));
        BindTypedefNames(names, type, type);
    }

    private CStmt BuildLocalTypedefList(Item type, Item names, Item? attributes = null)
    {
        BuildTypedefList(type, names, local: true, attributes: attributes);
        return new Seq(Array.Empty<CStmt>());
    }
}
