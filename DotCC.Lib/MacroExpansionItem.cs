#nullable enable
using System;
using System.Collections.Generic;
using LALR.CC.LexicalGrammar;

namespace DotCC;

/// <summary>Replacement tokens retain the macro names disabled when they were
/// produced, even when object and function expansion cross stream boundaries.
/// Content and position remain the original token's parser-visible values.</summary>
internal sealed class MacroExpansionItem : SourceMappedItem
{
    private readonly HashSet<string> _disabled;
    private readonly bool _fullyExpanded;
    private static readonly HashSet<string> NoDisabledNames = new(StringComparer.Ordinal);

    private MacroExpansionItem(Item item, HashSet<string> disabled, bool fullyExpanded = false)
        : base(item)
    {
        _disabled = disabled;
        _fullyExpanded = fullyExpanded || (item as MacroExpansionItem)?._fullyExpanded == true;
    }

    private MacroExpansionItem(Item item, HashSet<string> disabled, Item origin)
        : base(item, origin)
    {
        _disabled = disabled;
        _fullyExpanded = (item as MacroExpansionItem)?._fullyExpanded == true;
    }

    /// <summary>An included file has already passed through its own expander.
    /// Its output cannot be rescanned against later definitions by the parent.
    /// Macro definitions themselves remain live for subsequent parent tokens.</summary>
    internal static Item FinishInclude(Item item) => item is MacroExpansionItem { _fullyExpanded: true }
        ? item : new MacroExpansionItem(item, (item as MacroExpansionItem)?._disabled ?? NoDisabledNames, fullyExpanded: true);

    internal static Item AtInvocation(Item item, Item invocation) => item is MacroExpansionItem expansion
        ? new MacroExpansionItem(item, expansion._disabled, invocation)
        : new SourceMappedItem(item, invocation);

    internal static bool IsDisabled(Item item, string name) =>
        item is MacroExpansionItem expansion && (expansion._fullyExpanded || expansion._disabled.Contains(name));

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
