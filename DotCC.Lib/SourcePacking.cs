using System.Runtime.CompilerServices;
using LALR.CC;
using LALR.CC.LexicalGrammar;

namespace DotCC;

/// <summary>Declaration-site pragma packing, retained across macro expansion,
/// included-file buffering, token rewriting, and parser reductions.</summary>
internal static class SourcePacking
{
    private sealed record State(int Value);
    private static readonly ConditionalWeakTable<object, State> AstPacking = new();

    internal static int? Recorded(Item item) => item switch
    {
        SourceMappedItem mapped => mapped.Packing,
        SourceLocatedItem located => located.Packing,
        SourcePackingItem packed => packed.Packing,
        _ => null,
    };

    internal static int Of(Item item)
    {
        while (true)
        {
            if (Recorded(item) is { } packing) return packing;
            if (item.Content is { } content && AstPacking.TryGetValue(content, out var state)) return state.Value;
            if (item.Content is Item nested) { item = nested; continue; }
            if (item.Content is Reduction { Children: { Count: > 0 } children }) { item = children[0]; continue; }
            return 0;
        }
    }

    internal static Item Stamp(Item item, int packing)
    {
        if (Recorded(item).HasValue) return item;
        // Preserve derived macro hide-set and conditional-expression markers.
        var mapped = item as SourceMappedItem ?? new SourceMappedItem(item);
        mapped.Packing = packing;
        return mapped;
    }

    internal static void Attach(object ast, Item[] children)
    {
        // Default packing needs no AST entry. Most translation units never use
        // pack, so avoid allocating a second annotation for every reduction.
        if (children.Length > 0 && Of(children[0]) is var packing && packing != 0
            && !AstPacking.TryGetValue(ast, out _))
            AstPacking.Add(ast, new State(packing));
    }
}

internal sealed class SourcePackingItem(Item item, int packing) : Item(item.ID, item.Content, item.Position)
{
    internal int Packing { get; } = packing;
}
