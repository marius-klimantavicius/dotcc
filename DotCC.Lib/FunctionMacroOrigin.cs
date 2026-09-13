using System.Runtime.CompilerServices;
using LALR.CC.LexicalGrammar;

namespace DotCC;

/// <summary>Whether a declarator token was produced by a function-like macro.
/// Independent of hide sets, which are only preprocessor rescan state.</summary>
internal static class FunctionMacroOrigin
{
    private sealed class Marker;
    private static readonly ConditionalWeakTable<Item, Marker> Origins = new();

    internal static bool Contains(Item item) => Origins.TryGetValue(item, out _);
    internal static Item Mark(Item item)
    {
        Origins.GetValue(item, _ => new Marker());
        return item;
    }
    internal static T Copy<T>(Item origin, T item) where T : Item
    {
        if (Contains(origin)) Mark(item);
        return item;
    }
}
