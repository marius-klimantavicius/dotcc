#nullable enable
using System;
using System.Collections.Generic;
using LALR.CC;
using LALR.CC.LexicalGrammar;

namespace DotCC;

/// <summary>Function-like macro substitution, argument prescan, and replacement rescan.</summary>
internal sealed class MacroExpander : RewritingTokenStream
{
    private readonly CPreprocessor _cpp;
    private readonly int _idSymbol, _openParenSymbol, _closeParenSymbol;
    private readonly int _commaSymbol, _stringSymbol, _hashSymbol, _hashHashSymbol;
    private const string VaArgsName = "__VA_ARGS__";
    private const string VaOptName = "__VA_OPT__";

    // Whitespace belongs to the token boundary, independently of source locations.
    // Substitution combines tokens from different definitions/arguments, so their
    // original positions cannot reconstruct adjacency after replacement.
    private readonly record struct Token(Item Item, bool LeadingSpace)
    {
        public int ID => Item.ID;
        public object? Content => Item.Content;
        public SourcePosition Position => Item.Position;
    }

    private sealed class Arguments
    {
        public readonly List<List<Token>> Values = new();
        public readonly List<Token> Commas = new();
        public Item? Closing;
    }

    public MacroExpander(ISyncIterator<Item> inner, CPreprocessor cpp) : base(inner)
    {
        _cpp = cpp;
        var map = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var symbol in C.Definition.SymbolNames) map[symbol.Name] = symbol.ID;
        _idSymbol = map["ID"];
        _openParenSymbol = map["("];
        _closeParenSymbol = map[")"];
        _commaSymbol = map[","];
        _stringSymbol = map["STRING"];
        _hashSymbol = map["#"];
        _hashHashSymbol = map["##"];
    }

    private static bool Separated(Item previous, Item next)
    {
        return previous.Position.Line != next.Position.Line
            || previous.Position.Column + (previous.Content?.ToString()?.Length ?? 0) != next.Position.Column;
    }

    private static List<Token> ReadTokens(IReadOnlyList<Item> items)
    {
        var tokens = new List<Token>(items.Count);
        for (var i = 0; i < items.Count; ++i)
            tokens.Add(new Token(items[i], i != 0 && Separated(items[i - 1], items[i])));
        return tokens;
    }

    private static void AppendReplacement(List<Token> output, IReadOnlyList<Token> replacement, bool leadingSpace)
    {
        for (var i = 0; i < replacement.Count; ++i)
            output.Add(i == 0 ? replacement[i] with { LeadingSpace = leadingSpace } : replacement[i]);
    }

    protected override void ProcessToken(Item token)
    {
        if (token.ID == _idSymbol && token.Content is string name
            && !MacroExpansionItem.IsDisabled(token, name)
            && _cpp.TryGetMacro(name, out var macro) && macro.IsFunctionLike)
        {
            if (TryReadNext(out var next))
            {
                if (next.ID == _openParenSymbol)
                {
                    var args = CollectArgsFromStream(next);
                    var substituted = Substitute(macro, args, new HashSet<string>(StringComparer.Ordinal), token);
                    // A function name may come from an object replacement while
                    // its arguments/closing parenthesis are ordinary source.
                    var hiding = MacroExpansionItem.IntersectDisabled(token, args.Closing);
                    hiding.Add(name);
                    foreach (var expanded in ExpandTokenList(substituted, hiding)) Emit(expanded.Item);
                    return;
                }
                HoldNext(next);
            }
        }
        Emit(token);
    }

    private List<Token> ExpandTokenList(IReadOnlyList<Token> tokens, HashSet<string> hiding)
    {
        var result = new List<Token>();
        var pendingSpace = false;
        for (var i = 0; i < tokens.Count; ++i)
        {
            var token = tokens[i] with { LeadingSpace = tokens[i].LeadingSpace || pendingSpace };
            pendingSpace = false;
            if (token.ID == _idSymbol && token.Content is string name && !hiding.Contains(name)
                && !MacroExpansionItem.IsDisabled(token.Item, name))
            {
                if (_cpp.TryGetMacro(name, out var macro))
                {
                    var activeHiding = MacroExpansionItem.CopyDisabled(token.Item, hiding);
                    if (macro.IsFunctionLike)
                    {
                        if (i + 1 < tokens.Count && tokens[i + 1].ID == _openParenSymbol)
                        {
                            var (args, end) = CollectArgsFromList(tokens, i + 2);
                            if (end >= 0)
                            {
                                var substituted = Substitute(macro, args,
                                    new HashSet<string>(hiding, StringComparer.Ordinal), token.Item);
                                var functionHiding = MacroExpansionItem.IntersectDisabled(token.Item, args.Closing);
                                functionHiding.UnionWith(hiding);
                                functionHiding.Add(name);
                                var replacement = ExpandTokenList(substituted, functionHiding);
                                AppendReplacement(result, replacement, token.LeadingSpace);
                                pendingSpace = replacement.Count == 0 && token.LeadingSpace;
                                i = end;
                                continue;
                            }
                        }
                    }
                    else
                    {
                        activeHiding.Add(name);
                        var replacement = ExpandTokenList(ReadBody(macro.Body, token.Item), activeHiding);
                        AppendReplacement(result, replacement, token.LeadingSpace);
                        pendingSpace = replacement.Count == 0 && token.LeadingSpace;
                        continue;
                    }
                }
                else if (name == "__LINE__" || name == "__FILE__")
                {
                    // Raw argument capture defers these predefined macros too.
                    foreach (var expanded in _cpp.Rewrite(token.Item))
                        result.Add(new Token(expanded, token.LeadingSpace));
                    continue;
                }
            }
            result.Add(token with { Item = MacroExpansionItem.Disable(token.Item, hiding) });
        }
        return result;
    }

    private Arguments CollectArgsFromStream(Item opening)
    {
        var args = new Arguments();
        var current = new List<Token>();
        var previous = opening;
        var depth = 1;
        var wasCapturing = _cpp.CaptureRawMacroArguments;
        _cpp.CaptureRawMacroArguments = true;
        try
        {
            while (TryReadNext(out var item))
            {
                var token = new Token(item, Separated(previous, item));
                previous = item;
                if (token.ID == _openParenSymbol) ++depth;
                else if (token.ID == _closeParenSymbol && --depth == 0)
                {
                    args.Closing = item;
                    if (current.Count > 0 || args.Values.Count > 0) args.Values.Add(current);
                    return args;
                }
                else if (token.ID == _commaSymbol && depth == 1)
                {
                    args.Values.Add(current);
                    args.Commas.Add(token);
                    current = new List<Token>();
                    continue;
                }
                current.Add(token);
            }
            if (current.Count > 0 || args.Values.Count > 0) args.Values.Add(current);
            return args;
        }
        finally { _cpp.CaptureRawMacroArguments = wasCapturing; }
    }

    private (Arguments Args, int End) CollectArgsFromList(IReadOnlyList<Token> tokens, int start)
    {
        var args = new Arguments();
        var current = new List<Token>();
        var depth = 1;
        for (var i = start; i < tokens.Count; ++i)
        {
            var token = tokens[i];
            if (token.ID == _openParenSymbol) ++depth;
            else if (token.ID == _closeParenSymbol && --depth == 0)
            {
                args.Closing = token.Item;
                if (current.Count > 0 || args.Values.Count > 0) args.Values.Add(current);
                return (args, i);
            }
            else if (token.ID == _commaSymbol && depth == 1)
            {
                args.Values.Add(current);
                args.Commas.Add(token);
                current = new List<Token>();
                continue;
            }
            current.Add(token);
        }
        return (args, -1);
    }

    private static List<Token> ReadBody(IReadOnlyList<Item> body, Item invocation)
    {
        var tokens = ReadTokens(body);
        for (var i = 0; i < tokens.Count; ++i)
            tokens[i] = tokens[i] with { Item = MacroExpansionItem.AtInvocation(tokens[i].Item, invocation) };
        return tokens;
    }

    private List<Token> Substitute(MacroDef macro, Arguments args, HashSet<string> hiding, Item invocation)
    {
        var raw = new Dictionary<string, IReadOnlyList<Token>>(StringComparer.Ordinal);
        for (var i = 0; i < macro.Params!.Count; ++i)
            raw[macro.Params[i]] = i < args.Values.Count ? args.Values[i] : Array.Empty<Token>();
        if (macro.IsVariadic)
        {
            var extras = new List<Token>();
            for (var i = macro.Params.Count; i < args.Values.Count; ++i)
            {
                if (i > macro.Params.Count) extras.Add(args.Commas[i - 1]);
                extras.AddRange(args.Values[i]);
            }
            raw[VaArgsName] = extras;
        }
        var expanded = new Dictionary<string, IReadOnlyList<Token>>(StringComparer.Ordinal);
        foreach (var pair in raw)
            expanded[pair.Key] = ExpandTokenList(pair.Value, new HashSet<string>(hiding, StringComparer.Ordinal));
        var result = new List<Token>(macro.Body.Count);
        SubstituteInto(result, ReadBody(macro.Body, invocation), macro, raw, expanded);
        return result;
    }

    private void SubstituteInto(List<Token> result, IReadOnlyList<Token> body, MacroDef macro,
        Dictionary<string, IReadOnlyList<Token>> raw, Dictionary<string, IReadOnlyList<Token>> expanded)
    {
        var pendingSpace = false;
        for (var i = 0; i < body.Count; ++i)
        {
            var token = body[i] with { LeadingSpace = body[i].LeadingSpace || pendingSpace };
            pendingSpace = false;
            if (token.ID == _idSymbol && token.Content as string == VaOptName && macro.IsVariadic
                && i + 1 < body.Count && body[i + 1].ID == _openParenSymbol)
            {
                var depth = 0;
                var close = -1;
                for (var j = i + 1; j < body.Count; ++j)
                {
                    if (body[j].ID == _openParenSymbol) ++depth;
                    else if (body[j].ID == _closeParenSymbol && --depth == 0) { close = j; break; }
                }
                if (close < 0) { result.Add(token); continue; }
                if (raw[VaArgsName].Count > 0)
                {
                    var group = new List<Token>();
                    for (var j = i + 2; j < close; ++j) group.Add(body[j]);
                    SubstituteInto(result, group, macro, raw, expanded);
                }
                i = close;
                continue;
            }
            if (i + 2 < body.Count && body[i + 1].ID == _hashHashSymbol)
            {
                // Consume the entire chain before rescanning: intermediate names
                // must not expand, and every adjacent parameter uses raw tokens.
                // Keep all operand tokens so subsequent pastes join only their
                // boundaries, including empty (placemarker) arguments.
                var pasted = ResolveOperand(token, raw);
                do
                {
                    var next = new List<Token>();
                    AppendPasted(next, pasted, ResolveOperand(body[i + 2], raw), token);
                    pasted = next;
                    i += 2;
                } while (i + 2 < body.Count && body[i + 1].ID == _hashHashSymbol);
                AppendReplacement(result, pasted, token.LeadingSpace);
                continue;
            }
            if (token.ID == _hashSymbol && i + 1 < body.Count && body[i + 1].Content is string name
                && raw.TryGetValue(name, out var argument))
            {
                result.Add(new Token(SourceMappedItem.Create(_stringSymbol, "\"" + Stringify(argument) + "\"", token.Item), token.LeadingSpace));
                ++i;
                continue;
            }
            if (token.ID == _idSymbol && token.Content is string parameter && expanded.TryGetValue(parameter, out var replacement))
            {
                AppendReplacement(result, replacement, token.LeadingSpace);
                pendingSpace = replacement.Count == 0 && token.LeadingSpace;
                continue;
            }
            result.Add(token);
        }
    }

    private IReadOnlyList<Token> ResolveOperand(Token token, Dictionary<string, IReadOnlyList<Token>> parameters)
    {
        return token.ID == _idSymbol && token.Content is string name && parameters.TryGetValue(name, out var value)
            ? value : new[] { token };
    }

    private void AppendPasted(List<Token> output, IReadOnlyList<Token> left, IReadOnlyList<Token> right, Token location)
    {
        if (left.Count == 0) { AppendReplacement(output, right, location.LeadingSpace); return; }
        if (right.Count == 0) { AppendReplacement(output, left, location.LeadingSpace); return; }
        for (var i = 0; i < left.Count - 1; ++i)
            output.Add(i == 0 ? left[i] with { LeadingSpace = location.LeadingSpace } : left[i]);
        var text = (left[^1].Content?.ToString() ?? string.Empty) + (right[0].Content?.ToString() ?? string.Empty);
        output.Add(new Token(SourceMappedItem.Create(_idSymbol, text, location.Item),
            left.Count == 1 ? location.LeadingSpace : left[^1].LeadingSpace));
        for (var i = 1; i < right.Count; ++i) output.Add(right[i]);
    }

    private static string Stringify(IReadOnlyList<Token> tokens)
    {
        var result = new System.Text.StringBuilder();
        foreach (var token in tokens)
        {
            if (result.Length > 0 && token.LeadingSpace) result.Append(' ');
            var text = token.Content?.ToString();
            if (text is null) continue;
            // Quotes/backslashes are escaped within literal preprocessing tokens.
            // Their spelling is retained: do not decode C escape sequences here.
            var literal = text.IndexOf('"') >= 0 || text.IndexOf('\'') >= 0;
            foreach (var ch in text)
            {
                if (literal && (ch == '\\' || ch == '"')) result.Append('\\');
                result.Append(ch);
            }
        }
        return result.ToString();
    }
}
