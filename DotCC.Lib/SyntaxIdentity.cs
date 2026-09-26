using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using LALR.CC;
using LALR.CC.LexicalGrammar;

namespace DotCC;

/// <summary>Position-independent syntax identity without recursively formatting
/// generated grammar records. Those records have no shared child interface;
/// retain their reduction frames at construction instead of using reflection
/// (the compiler also runs under NativeAOT).</summary>
internal static class SyntaxIdentity
{
    private sealed record Frame(string Action, Item[] Children);
    private static readonly ConditionalWeakTable<object, Frame> Frames = new();

    internal static object Attach(object ast, string action, Item[] children)
    {
        // Own the frame independently of the parser's reduction buffer. Weak
        // keys limit its lifetime to the corresponding compilation's AST.
        Frames.Add(ast, new Frame(action, (Item[])children.Clone()));
        return ast;
    }

    internal static string Of(Item root)
    {
        var result = new StringBuilder();
        var pending = new Stack<Item>();
        pending.Push(root);
        while (pending.TryPop(out var item))
        {
            result.Append(item.ID.ToString(CultureInfo.InvariantCulture)).Append(':');
            switch (item.Content)
            {
                case null:
                    result.Append('N');
                    break;
                case string text:
                    result.Append('T');
                    AppendText(text);
                    break;
                case Item nested:
                    result.Append('I');
                    pending.Push(nested);
                    break;
                case Reduction reduction:
                    result.Append('R');
                    PushChildren(reduction.Children);
                    break;
                case { } ast when Frames.TryGetValue(ast, out var frame):
                    result.Append('A');
                    AppendText(frame.Action);
                    PushChildren(frame.Children);
                    break;
                default:
                    throw new InvalidOperationException("Missing syntax identity for " + item.Content.GetType().FullName);
            }
        }
        return result.ToString();

        // Length-prefix both text and child lists: literal contents cannot
        // collide with framing, and differently shaped syntax stays distinct.
        void AppendText(string text) => result.Append(text.Length.ToString(CultureInfo.InvariantCulture)).Append(':').Append(text);
        void PushChildren(IList<Item> children)
        {
            result.Append(children.Count.ToString(CultureInfo.InvariantCulture)).Append(':');
            for (var index = children.Count - 1; index >= 0; --index)
                pending.Push(children[index]);
        }
    }
}
