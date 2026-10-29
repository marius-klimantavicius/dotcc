using System;
using System.Collections.Generic;
using LALR.CC.LexicalGrammar;

namespace DotCC.Ir;

internal sealed partial class IrBuilder
{
    private void BuildAggregateTypeDeclaration(Item type, Item declaration)
    {
        // Do not resolve the inner type first: that would register an unpacked
        // definition before its postfix layout attributes have been collected.
        var attributes = new List<Item>();
        while (type.Content is C.TypeGnuAttributes attributed)
        {
            attributes.Add(attributed.Arg1);
            type = attributed.Arg0;
        }
        // A bare tag can share this reduced Type path with the older explicit
        // forward-declaration productions. Preserve its normal tag binding.
        if (attributes.Count == 0 && type.Content is C.TypeStruct structure)
        { ReferenceAggregate(structure.Arg1, isUnion: false); return; }
        if (attributes.Count == 0 && type.Content is C.TypeUnion unionReference)
        { ReferenceAggregate(unionReference.Arg1, isUnion: true); return; }
        // The shared Type terminator can also reduce existing enum
        // definitions. Preserve their ordinary registration and scope instead
        // of treating this parser choice as an unsupported record declaration.
        if (attributes.Count == 0 && type.Content is C.TypeTaggedEnum or C.TypeAnonymousEnum)
        { ResolveType(type); return; }
        var (tag, members, union) = type.Content switch
        {
            C.TypeTaggedStruct aggregate => (Tok(aggregate.Arg1), aggregate.Arg3, false),
            C.TypeTaggedUnion aggregate => (Tok(aggregate.Arg1), aggregate.Arg3, true),
            _ => throw new IrUnsupportedException("standalone type declaration requires an aggregate definition"),
        };
        int alignment = 0, pack = SourcePacking.Of(declaration);
        foreach (var attribute in attributes)
        {
            alignment = Math.Max(alignment, GnuAggregateAlignment(attribute));
            if (GnuAggregatePack(attribute, declaration) == 1) pack = 1;
        }
        BuildStructDef(tag, members, null, union, alignment, pack, pragmaPack: SourcePacking.Of(declaration));
    }
}
