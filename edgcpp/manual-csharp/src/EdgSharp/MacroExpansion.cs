// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
// Handwritten strict-mode adaptation of macro.c macro_invocation: isolated raw
// argument prescan, replacement rescanning with caller tail, persistent inert
// names, stringization, token pasting, placemarkers and C++20 variadic selection.
namespace EdgSharp;

internal sealed partial class Preprocessor
{
    private sealed class ExpansionInput(IEnumerable<PpToken> source) : IDisposable
    {
        private readonly Stack<IEnumerator<PpToken>> frames = new([source.GetEnumerator()]);
        private PpToken? lookahead;
        private bool? nextSpace;
        internal PpToken? Take()
        {
            if (lookahead != null) { PpToken result = lookahead; lookahead = null; return WithSpace(result); }
            while (frames.TryPeek(out IEnumerator<PpToken>? frame))
            {
                if (frame.MoveNext()) return WithSpace(frame.Current);
                frame.Dispose(); frames.Pop();
            }
            return null;
        }
        private PpToken WithSpace(PpToken token)
        {
            if (nextSpace is not bool space) return token;
            nextSpace = null;
            return token.Copy(space: space);
        }
        internal void CarrySpace(bool space) => nextSpace = space;
        internal PpToken? Peek() => lookahead ??= Take();
        internal void Push(IEnumerable<PpToken> tokens)
        {
            if (lookahead != null) throw new InvalidOperationException("Replacement insertion cannot discard a buffered caller token.");
            frames.Push(tokens.GetEnumerator());
        }
        public void Dispose() { while (frames.TryPop(out var frame)) frame.Dispose(); }
    }
    private static HashSet<string> Union(IEnumerable<string> first, IEnumerable<string> second)
    {
        var result = new HashSet<string>(first, StringComparer.Ordinal);
        result.UnionWith(second);
        return result;
    }
    private IEnumerable<PpToken> Expand(IEnumerable<PpToken> source, int argumentDepth)
    {
        using var input = new ExpansionInput(source);
        while (input.Take() is PpToken token)
        {
            Tick(token.Origin);
            if (token.Kind == TokenKind.EndOfSource) continue; // A physical include boundary is not caller-tail text.
            if (token.Kind != TokenKind.Identifier || token.Unavailable.Contains(token.Text)) { yield return token; continue; }
            if (token.Text is "__LINE__" or "__FILE__")
            {
                transformed = true;
                string spelling = token.Text == "__LINE__" ? token.Origin.Presumed.Line.ToString(System.Globalization.CultureInfo.InvariantCulture) :
                    QuoteFilename(token.Origin.Presumed.FileName);
                var origin = new TokenOrigin(token.Origin.SpellingRanges, token.Origin.Presumed, token.Origin.Include, token.Origin.Expansion, TokenOriginKind.Predefined, [token.Origin]);
                yield return Synthesized(spelling, token.Text == "__LINE__" ? TokenKind.IntegerLiteral : TokenKind.StringLiteral, origin);
                continue;
            }
            if (!macros.TryGetValue(token.Text, out MacroDefinition? macro)) { yield return token; continue; }
            if (macro.IsFunctionLike && input.Peek()?.Kind != TokenKind.LeftParen) { yield return token; continue; }
            MacroExpansionFrame? parent = token.Origin.Expansion;
            int depth = argumentDepth;
            for (var current = parent; current != null; current = current.Parent) ++depth;
            if (depth >= options.MacroDepthLimit)
            {
                Error("PP004", "Macro expansion depth limit exceeded.", token.Origin, DiagnosticClass.ResourceLimit);
                throw new PreprocessingStopped();
            }
            transformed = true;
            TokenOrigin invocation = token.Origin;
            HashSet<string> unavailable = new(token.Unavailable, StringComparer.Ordinal);
            var rawArguments = new List<PpToken[]>();
            var separators = new List<PpToken>();
            if (macro.IsFunctionLike)
            {
                input.Take(); // '('
                var raw = new List<PpToken>();
                int parentheses = 0;
                PpToken? closing = null;
                while (input.Take() is PpToken next)
                {
                    Tick(next.Origin);
                    if (next.Kind == TokenKind.EndOfSource) break;
                    if (next.Kind == TokenKind.RightParen && parentheses == 0)
                    {
                        rawArguments.Add(raw.ToArray()); closing = next; break;
                    }
                    if (next.Kind == TokenKind.Comma && parentheses == 0)
                    {
                        rawArguments.Add(raw.ToArray()); raw.Clear(); separators.Add(next); continue;
                    }
                    if (next.Kind == TokenKind.LeftParen) ++parentheses;
                    if (next.Kind == TokenKind.RightParen) --parentheses;
                    raw.Add(next);
                }
                if (closing == null) { Error("PP005", "Missing ')' in macro invocation.", token.Origin); yield break; }
                // The pinned EDG strict profile rescans actual arguments at
                // the logical line where the invocation's ')' was read.
                invocation = token.Origin.WithContext(new PresumedPosition(closing.Origin.Presumed.FileName,
                    closing.Origin.Presumed.Line, token.Origin.Presumed.Column), token.Origin.Include);
                unavailable.IntersectWith(closing.Unavailable);
                if (macro.Parameters.Count == 0 && rawArguments.Count == 1 && rawArguments[0].Length == 0) rawArguments.Clear();
                int fixedCount = macro.Parameters.Count - (macro.IsVariadic ? 1 : 0);
                if (!macro.IsVariadic && rawArguments.Count != fixedCount || macro.IsVariadic && rawArguments.Count < fixedCount)
                {
                    Error("PP006", "Macro invocation has the wrong number of arguments.", token.Origin);
                    continue;
                }
                if (macro.IsVariadic)
                {
                    var pack = new List<PpToken>();
                    for (int i = fixedCount; i < rawArguments.Count; ++i)
                    {
                        if (i != fixedCount) pack.Add(separators[i - 1]);
                        pack.AddRange(rawArguments[i]);
                    }
                    if (rawArguments.Count > fixedCount) rawArguments.RemoveRange(fixedCount, rawArguments.Count - fixedCount);
                    rawArguments.Add(pack.ToArray());
                }
            }
            var frame = new MacroExpansionFrame(macro.Name, macro.NameOrigin.SpellingRanges[0], invocation, parent);
            unavailable.Add(macro.Name);
            var expanded = new PpToken[]?[rawArguments.Count];
            PpToken[] ContextArgument(int index) => rawArguments[index].Select(arg => arg.Copy(origin: arg.Origin.Expanded(frame, TokenOriginKind.MacroArgument))).ToArray();
            PpToken[] ExpandedArgument(int index) => expanded[index] ??= Expand(ContextArgument(index), argumentDepth + 1).ToArray();
            bool nonemptyPack = macro.IsVariadic && macro.Replacement.Any(part => part.Text == "__VA_OPT__") &&
                ExpandedArgument(macro.Parameters.Count - 1).Length != 0;
            List<PpToken> Substitute(PpToken[] text)
            {
                PpToken[] selected = SelectVariadicText(text, nonemptyPack, frame, macro.IsVariadic,
                    (body, op) => Stringize(Paste(Substitute(body), frame).Where(p => !p.Placemark).ToArray(), op, frame),
                    body => Paste(Substitute(body), frame).Select(p => p.Copy(substituted: true)).ToArray());
                var replacement = new List<PpToken>();
                for (int i = 0; i < selected.Length; ++i)
                {
                    PpToken part = selected[i];
                    if (!part.Substituted && macro.IsFunctionLike && part.Kind == TokenKind.Hash)
                    {
                        if (++i >= selected.Length || selected[i].Kind != TokenKind.Identifier || !macro.Parameters.Contains(selected[i].Text))
                        {
                            Error("PP007", "Stringization requires a macro parameter.", part.Origin); continue;
                        }
                        int index = macro.Parameters.IndexOf(selected[i].Text);
                        replacement.Add(Stringize(rawArguments[index], part, frame).Copy(unavailable: unavailable));
                        continue;
                    }
                    int parameter = !part.Substituted && part.Kind == TokenKind.Identifier ? macro.Parameters.IndexOf(part.Text) : -1;
                    if (parameter >= 0)
                    {
                        bool pasted = i != 0 && selected[i - 1].PasteOperator || i + 1 < selected.Length && selected[i + 1].PasteOperator;
                        PpToken[] value = pasted ? ContextArgument(parameter) : ExpandedArgument(parameter);
                        if (value.Length == 0) replacement.Add(Synthesized("", TokenKind.Error, frame.Invocation, marker: true).Copy(unavailable: unavailable));
                        else replacement.AddRange(value.Select((arg, index) => arg.Copy(unavailable: Union(arg.Unavailable, unavailable), space: index == 0 ? part.SpaceBefore : arg.SpaceBefore)));
                    }
                    else replacement.Add(part.Copy(origin: ReferenceEquals(part.Origin.Expansion, frame) ? part.Origin : part.Origin.Expanded(frame),
                        unavailable: Union(part.Unavailable, unavailable)));
                }
                return replacement;
            }
            PpToken[] substituted = Paste(Substitute(macro.Replacement), frame).Where(part => !part.Placemark).ToArray();
            if (substituted.Length != 0) substituted[0] = substituted[0].Copy(space: token.SpaceBefore);
            else input.CarrySpace(token.SpaceBefore);
            input.Push(substituted);
        }
    }
    private static string QuoteFilename(string name)
    {
        var text = new System.Text.StringBuilder("\"");
        foreach (char c in name)
            text.Append(c switch { '\\' => "\\\\", '"' => "\\\"", '\n' => "\\n", '\r' => "\\r", '\t' => "\\t", '\a' => "\\a", '\b' => "\\b", '\f' => "\\f", '\v' => "\\v",
                _ => c < 32 || c == 127 ? "\\" + Convert.ToString(c, 8).PadLeft(3, '0') : c.ToString() });
        return text.Append('"').ToString();
    }
    private static PpToken Stringize(PpToken[] raw, PpToken op, MacroExpansionFrame frame)
    {
        string content = "";
        foreach (PpToken token in raw)
        {
            if (content.Length != 0 && token.SpaceBefore) content += " ";
            content += token.Text.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal);
        }
        var origins = new[] { op.Origin }.Concat(raw.Select(token => token.Origin)).ToArray();
        var origin = new TokenOrigin(origins.SelectMany(item => item.SpellingRanges), frame.Invocation.Presumed,
            frame.Invocation.Include, frame, TokenOriginKind.Stringization, origins);
        return Synthesized("\"" + content + "\"", TokenKind.StringLiteral, origin);
    }
    private PpToken[] SelectVariadicText(PpToken[] tokens, bool nonempty, MacroExpansionFrame frame, bool variadic,
        Func<PpToken[], PpToken, PpToken> stringize, Func<PpToken[], PpToken[]> substitute)
    {
        var selected = new List<PpToken>();
        for (int i = 0; i < tokens.Length; ++i)
        {
            PpToken token = tokens[i];
            PpToken? stringizeOperator = token.Kind == TokenKind.Hash && i + 1 < tokens.Length && tokens[i + 1].Text == "__VA_OPT__" ? token : null;
            if (stringizeOperator != null) token = tokens[++i];
            if (token.Text != "__VA_OPT__") { selected.Add(token); continue; }
            if (!variadic || ++i >= tokens.Length || tokens[i].Kind != TokenKind.LeftParen)
            { Error("PP008", "__VA_OPT__ requires parentheses in a variadic macro.", token.Origin); continue; }
            var body = new List<PpToken>();
            int depth = 1;
            while (++i < tokens.Length)
            {
                if (tokens[i].Kind == TokenKind.LeftParen) ++depth;
                if (tokens[i].Kind == TokenKind.RightParen && --depth == 0) break;
                body.Add(tokens[i]);
            }
            if (depth != 0) Error("PP009", "Unterminated __VA_OPT__ operand.", token.Origin);
            if (body.Any(part => part.Text == "__VA_OPT__")) Error("PP010", "Nested __VA_OPT__ is not permitted.", token.Origin);
            if (stringizeOperator != null) selected.Add(stringize(nonempty ? body.ToArray() : [], stringizeOperator));
            else if (nonempty && body.Count != 0)
            {
                PpToken[] bodyTokens = substitute(body.ToArray());
                if (bodyTokens.Length != 0) bodyTokens[0] = bodyTokens[0].Copy(space: token.SpaceBefore);
                selected.AddRange(bodyTokens);
            }
            else selected.Add(Synthesized("", TokenKind.Error, frame.Invocation, marker: true));
        }
        return selected.ToArray();
    }
    private IEnumerable<PpToken> Paste(List<PpToken> replacement, MacroExpansionFrame frame)
    {
        var result = new List<PpToken>();
        for (int i = 0; i < replacement.Count; ++i)
        {
            PpToken token = replacement[i];
            if (!token.PasteOperator) { result.Add(token); continue; }
            if (result.Count == 0 || i + 1 >= replacement.Count)
            { Error("PP011", "Token paste cannot begin or end a replacement list.", token.Origin); continue; }
            PpToken left = result[^1], right = replacement[++i];
            result.RemoveAt(result.Count - 1);
            if (left.Placemark) { result.Add(right); continue; }
            if (right.Placemark) { result.Add(left); continue; }
            string spelling = left.Text + right.Text;
            var source = new SourceFile("<token paste>", spelling, SourceFileKind.SynthesizedToken);
            var localDiagnostics = new DiagnosticBag();
            TokenBuffer pasted = new Lexer(source, localDiagnostics, preprocessing: true).Scan();
            if (localDiagnostics.Count != 0 || pasted.Tokens.Count != 2 || pasted.Tokens[0].Kind is TokenKind.Error or TokenKind.PreprocessingOther)
            { Error("PP012", "Token paste does not form exactly one preprocessing token.", token.Origin); continue; }
            var origin = new TokenOrigin(left.Origin.SpellingRanges.Concat(right.Origin.SpellingRanges).Concat(token.Origin.SpellingRanges),
                frame.Invocation.Presumed, frame.Invocation.Include, frame, TokenOriginKind.TokenPaste, [left.Origin, right.Origin, token.Origin]);
            var hidden = new HashSet<string>(left.Unavailable, StringComparer.Ordinal);
            hidden.IntersectWith(right.Unavailable);
            result.Add(new PpToken(pasted.Tokens[0].Kind, source.Range(0, spelling.Length), origin, left.SpaceBefore, unavailable: hidden));
        }
        return result;
    }
}
