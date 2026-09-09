#nullable enable
using System;
using System.Collections.Generic;
using LALR.CC.LexicalGrammar;

namespace DotCC;

/// <summary>Replacement tokens retain the macro names disabled when they were
/// produced, even when object and function expansion cross stream boundaries.
/// Content and position remain the original token's parser-visible values.</summary>
internal sealed class MacroExpansionItem : Item
{
    private readonly HashSet<string> _disabled;

    private MacroExpansionItem(Item item, HashSet<string> disabled)
        : base(item.ID, item.Content, item.Position) => _disabled = disabled;

    internal static bool IsDisabled(Item item, string name) =>
        item is MacroExpansionItem expansion && expansion._disabled.Contains(name);

    internal static HashSet<string> CopyDisabled(Item item, IEnumerable<string>? context = null)
    {
        var names = context is null ? new HashSet<string>(StringComparer.Ordinal)
            : new HashSet<string>(context, StringComparer.Ordinal);
        if (item is MacroExpansionItem expansion) names.UnionWith(expansion._disabled);
        return names;
    }

    internal static Item Disable(Item item, IReadOnlySet<string> names)
    {
        if (names.Count == 0) return item;
        if (item is MacroExpansionItem expansion && expansion._disabled.IsSupersetOf(names)) return item;
        // Snapshot the context: callers push/pop names while recursively rescanning.
        return new MacroExpansionItem(item, CopyDisabled(item, names));
    }

    internal static HashSet<string> IntersectDisabled(Item invocation, Item? closing)
    {
        var names = CopyDisabled(invocation);
        if (closing is MacroExpansionItem expansion) names.IntersectWith(expansion._disabled);
        else names.Clear();
        return names;
    }
}
