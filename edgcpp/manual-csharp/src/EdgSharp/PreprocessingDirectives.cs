// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
// Handwritten strict preprocessing directives following preproc.c conditional
// stacks, proc_define/undef/include/line and lexical.c input-file activation.
namespace EdgSharp;

internal sealed partial class Preprocessor
{
    private sealed class Conditional(bool parent, bool condition, TokenOrigin origin)
    {
        internal bool ParentActive { get; } = parent;
        internal bool Active { get; set; } = parent && condition;
        internal bool Taken { get; set; } = parent && condition;
        internal bool ElseSeen { get; set; }
        internal TokenOrigin Origin { get; } = origin;
    }
    private static bool ReservedMacro(string name) => name is "defined" or "__LINE__" or "__FILE__" or "__cplusplus";
    private IEnumerable<PpToken> ReadActive(SourceFile physical, IncludeFrame? include, int depth)
    {
        PpToken[] raw = Tokenize(physical);
        var conditionals = new List<Conditional>();
        string presumedName = physical.Name;
        int lineBias = 0;
        for (int index = 0; index < raw.Length;)
        {
            int start = index, line = raw[index].LogicalLine;
            while (index < raw.Length && raw[index].LogicalLine == line) ++index;
            PpToken[] tokens = raw[start..index];
            SourceFile logical = tokens[0].Range.File;
            int lineEnd = logical.Text.IndexOfAny(['\r', '\n'], tokens[^1].Range.End);
            if (lineEnd < 0) lineEnd = logical.Text.Length;
            int physicalLine = logical.OriginForRange(logical.Range(lineEnd, 0)).SpellingRanges[^1].EndPosition.Line;
            tokens = tokens.Select(token => token.Copy(origin: token.Origin.WithContext(
                new PresumedPosition(presumedName, physicalLine + lineBias, token.Origin.SpellingRanges[0].StartPosition.Column), include))).ToArray();
            bool active = conditionals.Count == 0 || conditionals[^1].Active;
            if (tokens[0].Kind != TokenKind.Hash || !tokens[0].StartsLine)
            {
                if (active) foreach (PpToken token in tokens) yield return token;
                continue;
            }
            transformed = true;
            Tick(tokens[0].Origin);
            if (tokens.Length == 1) continue;
            string directive = tokens[1].Text;
            PpToken[] operands = tokens[2..];
            if (directive is "if" or "ifdef" or "ifndef")
            {
                bool value = false;
                if (active)
                {
                    if (directive == "if") value = EvaluateCondition(operands, tokens[0].Origin);
                    else value = DefinedCondition(operands, tokens[0].Origin, directive == "ifndef");
                }
                conditionals.Add(new(active, value, tokens[0].Origin));
                continue;
            }
            if (directive is "else" or "elif" or "endif")
            {
                if (conditionals.Count == 0)
                { Error("PP020", "Conditional directive has no matching #if in this physical file.", tokens[0].Origin); continue; }
                Conditional conditional = conditionals[^1];
                if (directive == "endif")
                {
                    if (operands.Length != 0) Error("PP021", "Unexpected tokens after #endif.", operands[0].Origin);
                    conditionals.RemoveAt(conditionals.Count - 1); continue;
                }
                if (conditional.ElseSeen) Error("PP022", "Conditional branch follows #else.", tokens[0].Origin);
                if (directive == "else")
                {
                    if (operands.Length != 0) Error("PP023", "Unexpected tokens after #else.", operands[0].Origin);
                    conditional.Active = conditional.ParentActive && !conditional.Taken;
                    conditional.Taken |= conditional.Active;
                    conditional.ElseSeen = true;
                }
                else
                {
                    conditional.Active = conditional.ParentActive && !conditional.Taken && EvaluateCondition(operands, tokens[0].Origin);
                    conditional.Taken |= conditional.Active;
                }
                continue;
            }
            if (!active) continue;
            switch (directive)
            {
                case "define": Define(operands, tokens[0].Origin); break;
                case "undef":
                    if (operands.Length != 1 || operands[0].Kind != TokenKind.Identifier)
                        Error("PP024", "#undef requires one macro identifier.", tokens[0].Origin);
                    else if (ReservedMacro(operands[0].Text)) Error("PP025", "This predefined name cannot be undefined.", operands[0].Origin);
                    else macros.Remove(operands[0].Text);
                    break;
                case "include":
                    PpToken[] header = operands.Length == 1 && operands[0].Kind == TokenKind.HeaderName ? operands : Expand(operands, 0).ToArray();
                    string? name = null;
                    bool system = false;
                    if (header.Length == 1 && header[0].Kind is TokenKind.HeaderName or TokenKind.StringLiteral && header[0].Text.Length >= 2)
                    {
                        string text = header[0].Text;
                        system = text[0] == '<';
                        if (text[^1] == (system ? '>' : '"')) name = text[1..^1];
                    }
                    else if (header.Length >= 2 && header[0].Kind == TokenKind.Less && header[^1].Kind == TokenKind.Greater)
                    {
                        system = true;
                        name = string.Concat(header[1..^1].Select((token, i) => (i != 0 && token.SpaceBefore ? " " : "") + token.Text));
                    }
                    if (name == null || name.Length == 0)
                    { Error("PP026", "#include requires one nonempty quoted or angle header name.", tokens[0].Origin); break; }
                    if (depth >= options.IncludeDepthLimit)
                    { Error("PP027", "Include depth limit exceeded.", tokens[0].Origin, DiagnosticClass.ResourceLimit); break; }
                    SourceFile? included = ResolveInclude(physical, name, system);
                    if (included == null)
                    { Error("PP028", $"Include file '{name}' was not found in the explicitly configured search paths.", tokens[0].Origin); break; }
                    if (once.Contains(IncludePaths.Normalize(included.Name))) break;
                    var frame = new IncludeFrame(included, tokens[0].Origin, include);
                    includes.Add(frame);
                    yield return Synthesized("", TokenKind.EndOfSource, tokens[0].Origin);
                    foreach (PpToken token in ReadActive(included, frame, depth + 1)) yield return token;
                    yield return Synthesized("", TokenKind.EndOfSource, tokens[0].Origin);
                    break;
                case "line":
                    PpToken[] specification = Expand(operands, 0).ToArray();
                    if (specification.Length is < 1 or > 2 || !int.TryParse(specification[0].Text,
                        System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out int number) || number <= 0 ||
                        specification.Length == 2 && (specification[1].Kind != TokenKind.StringLiteral || specification[1].Text.Length < 2 || specification[1].Text[^1] != '"'))
                    { Error("PP029", "Invalid #line number or filename.", tokens[0].Origin); break; }
                    lineBias = number - (physicalLine + 1);
                    if (specification.Length == 2)
                    {
                        PpToken filename = specification[1];
                        if (!filename.Text.StartsWith('"'))
                        {
                            Error("PP056", "Nonordinary #line filename spellings are outside the implemented compatibility profile.", filename.Origin,
                                filename.Text.StartsWith("L\"", StringComparison.Ordinal) ? DiagnosticClass.InvalidSource : DiagnosticClass.UnsupportedFeature);
                            break;
                        }
                        string? decoded = DecodeLineFilename(filename);
                        if (decoded != null) presumedName = decoded;
                    }
                    break;
                case "pragma":
                    if (operands.Length == 1 && operands[0].Text == "once") once.Add(IncludePaths.Normalize(physical.Name));
                    else Error("PP030", "This pragma is not implemented.", tokens[0].Origin, DiagnosticClass.UnsupportedFeature);
                    break;
                case "error": Error("PP031", "#error " + string.Join(" ", operands.Select(token => token.Text)), tokens[0].Origin); break;
                case "warning": Error("PP032", "#warning is not recognized by the pinned strict C++20 profile.", tokens[0].Origin); break;
                default: Error("PP033", "This preprocessing directive is not implemented: #" + directive, tokens[0].Origin, DiagnosticClass.UnsupportedFeature); break;
            }
        }
        foreach (Conditional conditional in conditionals) Error("PP034", "Missing #endif before the end of this physical file.", conditional.Origin);
    }
    private SourceFile? ResolveInclude(SourceFile including, string name, bool system)
    {
        IEnumerable<string> roots = name.StartsWith('/') ? [""] : system ? options.SystemDirectories :
            new[] { IncludePaths.Directory(IncludePaths.Normalize(including.Name)) }.Concat(options.QuoteDirectories).Concat(options.SystemDirectories);
        foreach (string root in roots.Distinct(StringComparer.Ordinal))
        {
            string path = IncludePaths.Combine(root, name);
            if (files.TryGetValue(path, out SourceFile? retained)) return retained;
            if (options.IncludeProvider?.TryRead(path, out string? text) == true && text != null)
            {
                var source = new SourceFile(path, text);
                files.Add(path, source); physicalSources.Add(source); return source;
            }
        }
        return null;
    }
    private string? DecodeLineFilename(PpToken filename)
    {
        var result = new System.Text.StringBuilder();
        string text = filename.Text;
        for (int i = 1; i < text.Length - 1; ++i)
        {
            char c = text[i];
            if (c != '\\') { result.Append(c); continue; }
            if (++i >= text.Length - 1) { Error("PP057", "Incomplete escape in #line filename.", filename.Origin); return null; }
            c = text[i];
            string escapes = "abfnrtv\\\"'?";
            string values = "\a\b\f\n\r\t\v\\\"'?";
            int known = escapes.IndexOf(c);
            if (known >= 0) { result.Append(values[known]); continue; }
            bool hexadecimal = c == 'x';
            int value = 0, digits = 0, maximum = hexadecimal ? int.MaxValue : 3;
            if (hexadecimal) ++i;
            for (; i < text.Length - 1 && digits < maximum; ++i)
            {
                char d = text[i];
                int digit = d is >= '0' and <= '9' ? d - '0' : d is >= 'a' and <= 'f' ? d - 'a' + 10 : d is >= 'A' and <= 'F' ? d - 'A' + 10 : -1;
                if (digit < 0 || digit >= (hexadecimal ? 16 : 8)) break;
                value = ((value * (hexadecimal ? 16 : 8)) + digit) & 255; ++digits;
            }
            --i;
            if (digits == 0) { Error("PP058", "This #line filename escape is not implemented.", filename.Origin, DiagnosticClass.UnsupportedFeature); return null; }
            result.Append((char)value);
        }
        return result.ToString();
    }
    private bool DefinedCondition(PpToken[] operands, TokenOrigin origin, bool negate)
    {
        if (operands.Length != 1 || operands[0].Kind != TokenKind.Identifier)
        { Error("PP035", "This conditional directive requires one identifier.", origin); return false; }
        bool defined = macros.ContainsKey(operands[0].Text) || operands[0].Text is "__LINE__" or "__FILE__";
        return negate ? !defined : defined;
    }
    private void Define(PpToken[] tokens, TokenOrigin origin)
    {
        if (tokens.Length == 0 || tokens[0].Kind != TokenKind.Identifier)
        { Error("PP036", "#define requires a macro identifier.", origin); return; }
        string name = tokens[0].Text;
        if (ReservedMacro(name)) { Error("PP037", "This predefined name cannot be redefined.", tokens[0].Origin); return; }
        bool function = tokens.Length > 1 && tokens[1].Kind == TokenKind.LeftParen && !tokens[1].SpaceBefore;
        bool variadic = false;
        var parameters = new List<string>();
        int position = 1;
        if (function)
        {
            position = 2;
            if (position < tokens.Length && tokens[position].Kind != TokenKind.RightParen)
                while (position < tokens.Length)
                {
                    if (tokens[position].Kind == TokenKind.Ellipsis)
                    { variadic = true; parameters.Add("__VA_ARGS__"); ++position; break; }
                    if (tokens[position].Kind != TokenKind.Identifier || tokens[position].Text is "__VA_ARGS__" or "__VA_OPT__")
                    { Error("PP038", "Expected a macro parameter name.", tokens[position].Origin); return; }
                    if (parameters.Contains(tokens[position].Text)) Error("PP039", "Duplicate macro parameter name.", tokens[position].Origin);
                    parameters.Add(tokens[position++].Text);
                    if (position >= tokens.Length || tokens[position].Kind != TokenKind.Comma) break;
                    ++position;
                }
            if (position >= tokens.Length || tokens[position].Kind != TokenKind.RightParen)
            { Error("PP040", "Missing ')' in macro parameter list.", origin); return; }
            ++position;
        }
        else if (tokens.Length > 1 && !tokens[1].SpaceBefore)
        { Error("PP048", "An object macro replacement must be separated from its name by whitespace.", tokens[1].Origin); return; }
        PpToken[] replacement = tokens[position..];
        if (replacement.Length != 0 && (replacement[0].Kind == TokenKind.HashHash || replacement[^1].Kind == TokenKind.HashHash))
            Error("PP041", "Token paste cannot begin or end a macro replacement list.", origin);
        for (int i = 0; i < replacement.Length; ++i)
        {
            PpToken part = replacement[i];
            if (!variadic && part.Text is "__VA_ARGS__" or "__VA_OPT__") Error("PP042", "Variadic macro operator outside a variadic macro.", part.Origin);
            if (function && part.Kind == TokenKind.Hash && (i + 1 == replacement.Length ||
                !parameters.Contains(replacement[i + 1].Text) && replacement[i + 1].Text != "__VA_OPT__"))
                Error("PP043", "# must precede a macro parameter or __VA_OPT__.", part.Origin);
            if (part.Text == "__VA_OPT__")
            {
                if (i + 1 >= replacement.Length || replacement[i + 1].Kind != TokenKind.LeftParen)
                { Error("PP045", "__VA_OPT__ requires a parenthesized operand.", part.Origin); continue; }
                int nesting = 1, end = i + 2;
                for (; end < replacement.Length; ++end)
                {
                    if (replacement[end].Text == "__VA_OPT__") Error("PP046", "__VA_OPT__ cannot be nested.", replacement[end].Origin);
                    if (replacement[end].Kind == TokenKind.LeftParen) ++nesting;
                    if (replacement[end].Kind == TokenKind.RightParen && --nesting == 0) break;
                }
                if (nesting != 0) Error("PP047", "Unterminated __VA_OPT__ operand.", part.Origin);
                else if (end > i + 2 && (replacement[i + 2].Kind == TokenKind.HashHash || replacement[end - 1].Kind == TokenKind.HashHash))
                    Error("PP049", "Token paste cannot begin or end a __VA_OPT__ operand.", part.Origin);
            }
        }
        var definition = new MacroDefinition(name, tokens[0].Origin, function, variadic, parameters.ToArray(),
            replacement.Select(token => token.Copy(pasteOperator: token.Kind == TokenKind.HashHash)).ToArray());
        if (macros.TryGetValue(name, out MacroDefinition? previous))
        {
            if (!EquivalentMacro(previous, definition)) Error("PP044", "Conflicting macro redefinition.", tokens[0].Origin);
            return; // Equivalent redeclarations retain the canonical original definition and origin.
        }
        macros[name] = definition;
    }
    private static bool EquivalentMacro(MacroDefinition first, MacroDefinition second)
    {
        if (first.IsFunctionLike != second.IsFunctionLike || first.IsVariadic != second.IsVariadic ||
            !first.Parameters.SequenceEqual(second.Parameters) || first.Replacement.Length != second.Replacement.Length) return false;
        for (int i = 0; i < first.Replacement.Length; ++i)
        {
            PpToken left = first.Replacement[i], right = second.Replacement[i];
            int l = first.Parameters.IndexOf(left.Text), r = second.Parameters.IndexOf(right.Text);
            if ((l >= 0 || r >= 0) ? l != r : left.Text != right.Text) return false;
            if (i != 0 && left.SpaceBefore != right.SpaceBefore) return false;
        }
        return true;
    }
}
