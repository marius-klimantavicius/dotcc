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
    private static bool ReservedMacro(string name) => name is "defined" or "__LINE__" or "__FILE__" or "__cplusplus" or "__DATE__" or "__TIME__" or "__STDC_HOSTED__" or "__has_include";
    private bool IsDefined(string name) => macros.ContainsKey(name) || name is "__LINE__" or "__FILE__" or "__has_include";
    private IEnumerable<PpToken> ReadActive(SourceFile physical, IncludeFrame? include, int depth)
    {
        if (!physical.Encoding.Validate(physical, diagnostics)) yield break;
        var lexer = new Lexer(LogicalInput.Normalize(physical), diagnostics, preprocessing: true);
        using IEnumerator<PpToken> raw = Tokenize(lexer).GetEnumerator();
        bool available = raw.MoveNext();
        var conditionals = new List<Conditional>();
        string presumedName = physical.Name;
        int lineBias = 0;
        PpToken Context(PpToken token) => token.Copy(origin: token.Origin.WithContext(
            new PresumedPosition(presumedName, token.Origin.Presumed.Line + lineBias, token.Origin.Presumed.Column), include));
        IEnumerable<PpToken> LineOperands(int line, bool validateIdentifiers = false)
        {
            while (available && raw.Current.LogicalLine == line)
            {
                PpToken operand = Context(raw.Current);
                if (validateIdentifiers) DiagnoseIdentifier(operand);
                yield return operand;
                available = raw.MoveNext();
            }
        }
        while (available)
        {
            int line = raw.Current.LogicalLine;
            PpToken first = raw.Current;
            var collected = new List<PpToken> { first };
            available = raw.MoveNext();
            bool active = conditionals.Count == 0 || conditionals[^1].Active;
            if (first.Kind == TokenKind.Hash && first.StartsLine && available && raw.Current.LogicalLine == line)
            {
                PpToken name = raw.Current;
                collected.Add(name);
                available = raw.MoveNext();
                if (name.Text is "if" or "elif")
                {
                    transformed = true;
                    TokenOrigin origin = Context(first).Origin;
                    Tick(origin);
                    if (name.Text == "if")
                    {
                        bool value = active && EvaluateCondition(LineOperands(line, active), origin, lexer.RequestHeaderName);
                        foreach (PpToken unused in LineOperands(line, active)) { }
                        conditionals.Add(new(active, value, origin));
                    }
                    else if (conditionals.Count == 0)
                    {
                        Error("PP020", "Conditional directive has no matching #if in this physical file.", origin);
                        foreach (PpToken unused in LineOperands(line, active)) { }
                    }
                    else
                    {
                        Conditional conditional = conditionals[^1];
                        if (conditional.ElseSeen) Error("PP022", "Conditional branch follows #else.", origin);
                        // The first #elif leaving a taken active group is
                        // initially scanned while diagnostics are enabled, even
                        // though its expression is not subsequently evaluated.
                        bool scanActive = active || conditional.ParentActive && !conditional.Taken;
                        conditional.Active = conditional.ParentActive && !conditional.Taken && EvaluateCondition(LineOperands(line, scanActive), origin, lexer.RequestHeaderName);
                        foreach (PpToken unused in LineOperands(line, scanActive)) { }
                        conditional.Taken |= conditional.Active;
                    }
                    continue;
                }
            }
            while (available && raw.Current.LogicalLine == line) { collected.Add(raw.Current); available = raw.MoveNext(); }
            int directiveEnd = lexer.EndOfLogicalLine(line);
            int physicalLine = lexer.Input.OriginForRange(lexer.Input.Range(directiveEnd, 0)).SpellingRanges[^1].EndPosition.Line;
            PpToken[] tokens = collected.Select(Context).ToArray();
            if (tokens[0].Kind != TokenKind.Hash || !tokens[0].StartsLine)
            {
                if (active) foreach (PpToken token in tokens) { DiagnoseIdentifier(token); yield return token; }
                continue;
            }
            transformed = true;
            Tick(tokens[0].Origin);
            if (tokens.Length == 1) continue;
            string directive = tokens[1].Text;
            PpToken[] operands = tokens[2..];
            if (directive is "ifdef" or "ifndef")
            {
                bool value = false;
                if (active)
                {
                    value = DefinedCondition(operands, tokens[0].Origin, directive == "ifndef");
                }
                conditionals.Add(new(active, value, tokens[0].Origin));
                continue;
            }
            if (directive is "else" or "endif")
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
                continue;
            }
            if (!active) continue;
            foreach (PpToken token in tokens) DiagnoseIdentifier(token);
            switch (directive)
            {
                case "define": Define(operands, tokens[0].Origin); break;
                case "undef":
                    if (operands.Length != 1 || operands[0].Kind != TokenKind.Identifier)
                        Error("PP024", "#undef requires one macro identifier.", tokens[0].Origin);
                    else if (ReservedMacro(operands[0].IdentifierText)) Error("PP025", "This predefined name cannot be undefined.", operands[0].Origin);
                    else { DiagnoseIdentifier(operands[0]); macros.Remove(operands[0].IdentifierText); }
                    break;
                case "include":
                    PpToken[] header = Expand(operands, 0, headerOperand: true).ToArray();
                    if (!ReadHeaderName(header, out string name, out bool system))
                    { Error("PP026", "#include requires one nonempty quoted or angle header name.", tokens[0].Origin); break; }
                    if (depth >= options.IncludeDepthLimit)
                    { Error("PP027", "Include depth limit exceeded.", tokens[0].Origin, DiagnosticClass.ResourceLimit); break; }
                    SourceFile? included = ResolveInclude(physical, name, system, tokens[0].Origin, out bool failed);
                    if (included == null)
                    { if (!failed) Error("PP028", $"Include file '{name}' was not found in the explicitly configured search paths.", tokens[0].Origin); break; }
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
    private SourceFile? ResolveInclude(SourceFile including, string name, bool system, TokenOrigin origin, out bool failed)
    {
        failed = false;
        foreach (string path in IncludeSearchPaths(including, name, system))
        {
            if (files.TryGetValue(path, out SourceFile? retained)) return retained;
            if (options.IncludeProvider is IStructuredIncludeProvider structured)
            {
                if (!includeReads.TryGetValue(path, out IncludeReadResult? read)) includeReads.Add(path, read = structured.Read(path));
                if (read.Status == IncludeInputStatus.Missing) continue;
                if (read.Status == IncludeInputStatus.Failure)
                { ReportIncludeFailure(read.Failure!, origin); failed = true; return null; }
                string canonical = read.CanonicalPath!;
                if (files.TryGetValue(canonical, out retained)) { files.TryAdd(path, retained); return retained; }
                var source = new SourceFile(canonical, read.Text!, encoding: read.Encoding);
                files.Add(canonical, source); files.TryAdd(path, source); physicalSources.Add(source); return source;
            }
            if (ReadIncludeText(path) is string text)
            {
                var source = new SourceFile(path, text, encoding: SourceEncodingInfo.Managed(options.SourceInput));
                files.Add(path, source); physicalSources.Add(source); return source;
            }
        }
        return null;
    }
    private bool ProbeInclude(SourceFile including, string name, bool system, TokenOrigin origin)
    {
        foreach (string path in IncludeSearchPaths(including, name, system))
        {
            if (files.ContainsKey(path)) return true;
            if (options.IncludeProvider is IStructuredIncludeProvider structured)
            {
                if (!includeProbes.TryGetValue(path, out IncludeProbeResult? probe)) includeProbes.Add(path, probe = structured.Probe(path));
                if (probe.Status == IncludeInputStatus.Found) return true;
                if (probe.Status == IncludeInputStatus.Failure) { ReportIncludeFailure(probe.Failure!, origin); return false; }
            }
            else if (ReadIncludeText(path) != null) return true;
        }
        return false;
    }
    private void ReportIncludeFailure(IncludeInputFailure failure, TokenOrigin origin)
    {
        DiagnosticClass classification = failure.Kind switch
        {
            IncludeFailureKind.InvalidEncoding => DiagnosticClass.InvalidSource,
            IncludeFailureKind.UnsupportedEncoding => DiagnosticClass.UnsupportedFeature,
            IncludeFailureKind.UnsupportedHost => DiagnosticClass.UnsupportedFeature,
            IncludeFailureKind.LimitExceeded => DiagnosticClass.ResourceLimit,
            _ => DiagnosticClass.InputFailure
        };
        Error("PP063", "Include input failure (" + failure.Kind + "): " + failure.Message, origin, classification);
    }
    private IEnumerable<string> IncludeSearchPaths(SourceFile including, string name, bool system)
    {
        IEnumerable<string> roots = name.StartsWith('/') ? [""] : system ? options.SystemDirectories :
            new[] { IncludePaths.Directory(IncludePaths.Normalize(including.Name)) }.Concat(options.QuoteDirectories).Concat(options.SystemDirectories);
        return roots.Select(root => IncludePaths.Combine(root, name)).Distinct(StringComparer.Ordinal);
    }
    private string? ReadIncludeText(string path)
    {
        if (files.TryGetValue(path, out SourceFile? retained)) return retained.Text;
        if (includeContents.TryGetValue(path, out string? known)) return known;
        string? text = null;
        if (options.IncludeProvider != null && options.IncludeProvider.TryRead(path, out string? value)) text = value;
        includeContents.Add(path, text); // Cache misses as well as hits for one analysis.
        return text;
    }
    private static bool ReadHeaderName(PpToken[] header, out string name, out bool system)
    {
        name = ""; system = false;
        if (header.Length == 1 && header[0].Kind is TokenKind.HeaderName or TokenKind.StringLiteral && header[0].Text.Length >= 2)
        {
            string text = header[0].Text;
            system = text[0] == '<';
            if ((system || text[0] == '"') && text[^1] == (system ? '>' : '"')) name = text[1..^1];
        }
        else if (header.Length >= 2 && header[0].Kind == TokenKind.Less && Array.FindIndex(header, token => token.Kind == TokenKind.Greater) == header.Length - 1)
        {
            system = true;
            if (header[0].Origin.Expansion == null && header[^1].Origin.Expansion == null && ReferenceEquals(header[0].Range.File, header[^1].Range.File))
            {
                string literal = SourceRange.Cover(header[0].Range, header[^1].Range).Spelling;
                name = literal[1..^1];
            }
            else name = string.Concat(header[1..^1].Select((token, i) => (i != 0 && token.SpaceBefore ? " " : "") + token.Text));
        }
        return name.Length != 0 && name.IndexOfAny(['\r', '\n']) < 0;
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
        DiagnoseIdentifier(operands[0]);
        bool defined = IsDefined(operands[0].IdentifierText);
        return negate ? !defined : defined;
    }
    private void Define(PpToken[] tokens, TokenOrigin origin)
    {
        if (tokens.Length == 0 || tokens[0].Kind != TokenKind.Identifier)
        { Error("PP036", "#define requires a macro identifier.", origin); return; }
        DiagnoseIdentifier(tokens[0]);
        string name = tokens[0].IdentifierText;
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
                    if (tokens[position].Kind != TokenKind.Identifier || tokens[position].IdentifierText is "__VA_ARGS__" or "__VA_OPT__")
                    { Error("PP038", "Expected a macro parameter name.", tokens[position].Origin); return; }
                    DiagnoseIdentifier(tokens[position]);
                    if (parameters.Contains(tokens[position].IdentifierText)) Error("PP039", "Duplicate macro parameter name.", tokens[position].Origin);
                    parameters.Add(tokens[position++].IdentifierText);
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
            DiagnoseIdentifier(part);
            if (!variadic && part.IdentifierText is "__VA_ARGS__" or "__VA_OPT__") Error("PP042", "Variadic macro operator outside a variadic macro.", part.Origin);
            if (function && part.Kind == TokenKind.Hash && (i + 1 == replacement.Length ||
                !parameters.Contains(replacement[i + 1].IdentifierText) && replacement[i + 1].IdentifierText != "__VA_OPT__"))
                Error("PP043", "# must precede a macro parameter or __VA_OPT__.", part.Origin);
            if (part.IdentifierText == "__VA_OPT__")
            {
                if (i + 1 >= replacement.Length || replacement[i + 1].Kind != TokenKind.LeftParen)
                { Error("PP045", "__VA_OPT__ requires a parenthesized operand.", part.Origin); continue; }
                int nesting = 1, end = i + 2;
                for (; end < replacement.Length; ++end)
                {
                    if (replacement[end].IdentifierText == "__VA_OPT__") Error("PP046", "__VA_OPT__ cannot be nested.", replacement[end].Origin);
                    if (replacement[end].Kind == TokenKind.LeftParen) ++nesting;
                    if (replacement[end].Kind == TokenKind.RightParen && --nesting == 0) break;
                }
                if (nesting != 0) Error("PP047", "Unterminated __VA_OPT__ operand.", part.Origin);
                else if (end > i + 2 && (replacement[i + 2].Kind == TokenKind.HashHash || replacement[end - 1].Kind == TokenKind.HashHash))
                    Error("PP049", "Token paste cannot begin or end a __VA_OPT__ operand.", part.Origin);
            }
        }
        var definition = new MacroDefinition(name, tokens[0].Origin, function, variadic, parameters.ToArray(),
            replacement.Select(token => token.Copy(pasteOperator: token.Kind == TokenKind.HashHash,
                effectiveSpelling: token.Kind == TokenKind.Identifier && !parameters.Contains(token.IdentifierText)
                    ? UnicodeIdentifiers.CanonicalUcnSpelling(token.Text) : null)).ToArray());
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
            int l = first.Parameters.IndexOf(left.IdentifierText), r = second.Parameters.IndexOf(right.IdentifierText);
            if (left.Kind != right.Kind || ((l >= 0 || r >= 0) ? l != r : left.IdentifierText != right.IdentifierText)) return false;
            if (i != 0 && left.SpaceBefore != right.SpaceBefore) return false;
        }
        return true;
    }
}
