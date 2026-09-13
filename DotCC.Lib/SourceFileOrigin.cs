using System.Runtime.CompilerServices;
using LALR.CC;
using LALR.CC.LexicalGrammar;

namespace DotCC;

/// <summary>A physical source file, shared by all of its tokens. Positions stay
/// in the parser's ordinary line/column/byte coordinates.</summary>
internal sealed record SourceFileOrigin(string Name, string Identity)
{
    // Parser reductions create fresh base Items and carry only Position forward.
    // Associate each action's AST object with its first child's file, so trimmed
    // reductions retain provenance too. Weak keys keep this compilation-local in
    // lifetime without mutable global/current-file state or coordinate collisions.
    private static readonly ConditionalWeakTable<object, SourceFileOrigin> AstOrigins = new();

    internal static SourceFileOrigin? Of(Item item)
    {
        while (true)
        {
            if (item is SourceLocatedItem located) return located.SourceFile;
            if (SourceMappedItem.FileOf(item) is { } mappedFile) return mappedFile;
            if (item.Content is { } content && AstOrigins.TryGetValue(content, out var astFile)) return astFile;
            if (item.Content is Item nested) { item = nested; continue; }
            if (item.Content is Reduction { Children: { Count: > 0 } children }) { item = children[0]; continue; }
            return null;
        }
    }

    internal static object Attach(object ast, Item[] children)
    {
        SourcePacking.Attach(ast, children);
        if (children.Length > 0 && Of(children[0]) is { } source && !AstOrigins.TryGetValue(ast, out _))
            AstOrigins.Add(ast, source);
        return ast;
    }

    internal static Item Rewrite(Item origin, int id, object? content) =>
        FunctionMacroOrigin.Copy<Item>(origin, Of(origin) is { } source
            ? new SourceLocatedItem(id, content, origin.Position, source, SourcePacking.Of(origin))
            : new SourcePackingItem(new Item(id, content, origin.Position), SourcePacking.Of(origin)));
}

internal sealed class SourceLocatedItem(int id, object? content, SourcePosition position, SourceFileOrigin sourceFile, int packing = 0)
    : Item(id, content, position)
{
    internal SourceFileOrigin SourceFile { get; } = sourceFile;
    internal int Packing { get; } = packing;
}
