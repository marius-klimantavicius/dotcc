// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
// Handwritten preprocessing state following EDG preproc.c directive/conditional
// and include activation contracts, lexical.c logical input and source maps,
// and macro.c definition/expansion contracts. No host preprocessor is invoked.
using System.Collections.ObjectModel;
using System.Collections.Frozen;
using System.Text;

namespace EdgSharp;

public interface IIncludeProvider
{
    /// <summary>Reads an explicitly searched canonical path. Providers must return stable contents during one analysis.</summary>
    bool TryRead(string path, out string? text);
}

public sealed class InMemoryIncludeProvider : IIncludeProvider
{
    private readonly Dictionary<string, string> files;
    public InMemoryIncludeProvider(IEnumerable<KeyValuePair<string, string>> files)
        => this.files = files.ToDictionary(pair => IncludePaths.Normalize(pair.Key), pair => pair.Value, StringComparer.Ordinal);
    public bool TryRead(string path, out string? text) => files.TryGetValue(IncludePaths.Normalize(path), out text);
}

public sealed class PreprocessingOptions
{
    public IIncludeProvider? IncludeProvider { get; }
    public ReadOnlyCollection<string> QuoteDirectories { get; }
    public ReadOnlyCollection<string> SystemDirectories { get; }
    public ReadOnlyDictionary<string, string> PredefinedMacros { get; }
    public int IncludeDepthLimit { get; }
    public int MacroDepthLimit { get; }
    public int WorkLimit { get; }
    /// <summary>Configured UTC translation time, or null to capture UTC once when a session is created. Session.Options exposes the effective timestamp.</summary>
    public DateTimeOffset? TranslationTimestamp { get; }
    public bool Hosted { get; }
    public PreprocessingOptions(IIncludeProvider? includeProvider = null, IEnumerable<string>? quoteDirectories = null,
        IEnumerable<string>? systemDirectories = null, IEnumerable<KeyValuePair<string, string>>? predefinedMacros = null,
        int includeDepthLimit = 64, int macroDepthLimit = 128, int workLimit = 100000,
        DateTimeOffset? translationTimestamp = null, bool hosted = true)
    {
        if (includeDepthLimit <= 0 || macroDepthLimit <= 0 || workLimit <= 0) throw new ArgumentOutOfRangeException(nameof(workLimit), "Preprocessing limits must be positive.");
        (IncludeProvider, IncludeDepthLimit, MacroDepthLimit, WorkLimit) = (includeProvider, includeDepthLimit, macroDepthLimit, workLimit);
        QuoteDirectories = Array.AsReadOnly((quoteDirectories ?? []).Select(IncludePaths.Normalize).ToArray());
        SystemDirectories = Array.AsReadOnly((systemDirectories ?? []).Select(IncludePaths.Normalize).ToArray());
        PredefinedMacros = new((predefinedMacros ?? []).ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal));
        if (PredefinedMacros.Keys.Any(name => name is "defined" or "__LINE__" or "__FILE__" or "__cplusplus" or "__DATE__" or "__TIME__" or "__STDC_HOSTED__" or "__has_include"))
            throw new ArgumentException("Reserved predefined names use dedicated options and cannot be overridden through generic macro definitions.", nameof(predefinedMacros));
        TranslationTimestamp = translationTimestamp?.ToUniversalTime();
        Hosted = hosted;
    }
    internal PreprocessingOptions CaptureTranslationTimestamp() => TranslationTimestamp != null ? this :
        new(IncludeProvider, QuoteDirectories, SystemDirectories, PredefinedMacros, IncludeDepthLimit, MacroDepthLimit, WorkLimit, DateTimeOffset.UtcNow, Hosted);
}

internal static class IncludePaths
{
    internal static string Normalize(string path)
    {
        bool rooted = path.StartsWith('/');
        var parts = new List<string>();
        foreach (string part in path.Split('/'))
        {
            if (part is "" or ".") continue;
            if (part == ".." && parts.Count != 0 && parts[^1] != "..") parts.RemoveAt(parts.Count - 1);
            else if (part != ".." || !rooted) parts.Add(part);
        }
        return (rooted ? "/" : "") + string.Join('/', parts);
    }
    internal static string Directory(string path) => path.LastIndexOf('/') is int slash && slash >= 0 ? slash == 0 ? "/" : path[..slash] : "";
    internal static string Combine(string directory, string name) => Normalize(name.StartsWith('/') ? name : directory.Length == 0 ? name : directory + "/" + name);
}

public sealed class MacroDefinition
{
    public string Name { get; }
    public TokenOrigin NameOrigin { get; }
    public bool IsFunctionLike { get; }
    public bool IsVariadic { get; }
    public ReadOnlyCollection<string> Parameters { get; }
    public string ReplacementSpelling => string.Join(" ", Replacement.Select(token => token.Text));
    internal PpToken[] Replacement { get; }
    internal MacroDefinition(string name, TokenOrigin origin, bool function, bool variadic, string[] parameters, PpToken[] replacement)
        => (Name, NameOrigin, IsFunctionLike, IsVariadic, Parameters, Replacement) =
            (name, origin, function, variadic, Array.AsReadOnly((string[])parameters.Clone()), (PpToken[])replacement.Clone());
}

public sealed class PreprocessingResult
{
    public SourceFile Source { get; }
    public TokenBuffer Tokens { get; }
    public ReadOnlyCollection<SourceFile> PhysicalSources { get; }
    public ReadOnlyCollection<IncludeFrame> Includes { get; }
    public ReadOnlyDictionary<string, MacroDefinition> Macros { get; }
    public ReadOnlyCollection<Diagnostic> Diagnostics { get; }
    public DateTimeOffset TranslationTimestamp { get; }
    public bool Hosted { get; }
    public bool IsSuccess => Diagnostics.All(d => d.Severity != DiagnosticSeverity.Error);
    internal PreprocessingResult(SourceFile source, TokenBuffer tokens, SourceFile[] physical, IncludeFrame[] includes,
        Dictionary<string, MacroDefinition> macros, ReadOnlyCollection<Diagnostic> diagnostics, DateTimeOffset timestamp, bool hosted)
        => (Source, Tokens, PhysicalSources, Includes, Macros, Diagnostics, TranslationTimestamp, Hosted) =
            (source, tokens, Array.AsReadOnly(physical), Array.AsReadOnly(includes), new(macros), diagnostics, timestamp, hosted);
}

internal sealed class PpToken
{
    internal TokenKind Kind { get; }
    internal SourceRange Range { get; }
    internal string Text => Range.Spelling;
    internal TokenOrigin Origin { get; }
    internal bool SpaceBefore { get; }
    internal bool StartsLine { get; }
    internal int LogicalLine { get; }
    internal FrozenSet<string> Unavailable { get; }
    internal bool Placemark { get; }
    internal bool Substituted { get; }
    internal bool PasteOperator { get; }
    internal PpToken(TokenKind kind, SourceRange range, TokenOrigin origin, bool space = false, bool start = false, int line = 0,
        IEnumerable<string>? unavailable = null, bool placemark = false, bool substituted = false, bool pasteOperator = false)
        => (Kind, Range, Origin, SpaceBefore, StartsLine, LogicalLine, Unavailable, Placemark, Substituted, PasteOperator) =
            (kind, range, origin, space, start, line, (unavailable ?? []).ToFrozenSet(StringComparer.Ordinal), placemark, substituted, pasteOperator);
    internal PpToken Copy(TokenOrigin? origin = null, IEnumerable<string>? unavailable = null, bool? space = null, bool? substituted = null, bool? pasteOperator = null)
        => new(Kind, Range, origin ?? Origin, space ?? SpaceBefore, StartsLine, LogicalLine, unavailable ?? Unavailable, Placemark, substituted ?? Substituted, pasteOperator ?? PasteOperator);
}

internal static class LogicalInput
{
    internal static SourceFile Normalize(SourceFile source)
    {
        var text = new StringBuilder();
        var maps = new List<SourceMapSegment>();
        int runStart = 0, offset = 0;
        bool changed = false;
        while (offset < source.Text.Length)
        {
            if (source.Text[offset] == '\\' && offset + 1 < source.Text.Length && source.Text[offset + 1] is '\r' or '\n')
            {
                int count = offset - runStart;
                if (count != 0)
                {
                    maps.Add(new(text.Length, count, TokenOrigin.Direct(source.Range(runStart, count))));
                    text.Append(source.Text, runStart, count);
                }
                offset += source.Text[offset + 1] == '\r' && offset + 2 < source.Text.Length && source.Text[offset + 2] == '\n' ? 3 : 2;
                runStart = offset;
                changed = true;
            }
            else ++offset;
        }
        int tail = source.Text.Length - runStart;
        if (tail != 0)
        {
            maps.Add(new(text.Length, tail, TokenOrigin.Direct(source.Range(runStart, tail))));
            text.Append(source.Text, runStart, tail);
        }
        maps.Add(new(text.Length, 0, TokenOrigin.Direct(source.Range(source.Text.Length, 0))));
        SourceFile spliced = changed ? new SourceFile(source.Name + " [logical input]", text.ToString(), SourceFileKind.LogicalInput, maps.ToArray()) : source;
        return spliced;
    }
    // The real lexer calls this against physical spelling when it recognizes a
    // raw token, restoring phase-2 modifications in its delimiter and r-chars.
    internal static int RawLiteralEnd(string text, int quote) => RawLiteralEnd(text, quote, out _);
    internal static int RawLiteralEnd(string text, int quote, out bool closed)
    {
        closed = false;
        int open = quote + 1;
        while (open < text.Length && open - quote <= 17 && text[open] != '(')
        {
            if (text[open] is ' ' or '\t' or '\v' or '\f' or '\r' or '\n' or '\\' or ')') return quote + 1;
            ++open;
        }
        if (open >= text.Length || text[open] != '(' || open - quote > 17) return quote + 1;
        string end = ")" + text[(quote + 1)..open] + "\"";
        int close = text.IndexOf(end, open + 1, StringComparison.Ordinal);
        closed = close >= 0;
        return close < 0 ? text.Length : close + end.Length;
    }
}

internal sealed partial class Preprocessor(PreprocessingOptions options, DiagnosticBag diagnostics, TargetInfo? targetProfile = null)
{
    private readonly TargetInfo target = targetProfile ?? TargetInfo.Lp64;
    private readonly Dictionary<string, MacroDefinition> macros = new(StringComparer.Ordinal);
    private readonly Dictionary<string, SourceFile> files = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string?> includeContents = new(StringComparer.Ordinal);
    private readonly Dictionary<string, IncludeReadResult> includeReads = new(StringComparer.Ordinal);
    private readonly Dictionary<string, IncludeProbeResult> includeProbes = new(StringComparer.Ordinal);
    private readonly List<SourceFile> physicalSources = [];
    private readonly List<IncludeFrame> includes = [];
    private readonly HashSet<string> once = new(StringComparer.Ordinal);
    private bool transformed;
    private int work;
    private int controllingExpressionDepth;
    private sealed class PreprocessingStopped : Exception { }
    private void Error(string code, string message, TokenOrigin origin, DiagnosticClass kind = DiagnosticClass.InvalidSource)
        => diagnostics.Add(code, message, DiagnosticRange(origin), kind);
    private static SourceRange DiagnosticRange(TokenOrigin origin)
    {
        // Retain the actual origin in a one-token mapped diagnostic buffer.
        string text = origin.PrimaryPhysicalRange.Spelling;
        var source = new SourceFile("<preprocessing diagnostic>", text, SourceFileKind.PreprocessedPresentation, [new(0, text.Length, origin)]);
        return source.Range(0, text.Length);
    }
    private void Tick(TokenOrigin origin)
    {
        if (++work <= options.WorkLimit) return;
        Error("PP001", "Preprocessing work limit exceeded.", origin, DiagnosticClass.ResourceLimit);
        throw new PreprocessingStopped();
    }
    private static PpToken Synthesized(string spelling, TokenKind kind, TokenOrigin origin, bool marker = false)
    {
        var source = new SourceFile("<generated preprocessing token>", spelling, SourceFileKind.SynthesizedToken);
        return new PpToken(kind, source.Range(0, spelling.Length), origin, placemark: marker);
    }
    private IEnumerable<PpToken> Tokenize(SourceFile physical)
    {
        SourceFile logical = LogicalInput.Normalize(physical);
        return Tokenize(new Lexer(logical, diagnostics, preprocessing: true));
    }
    private static IEnumerable<PpToken> Tokenize(Lexer lexer)
    {
        int previousEnd = 0;
        int previousLine = 0;
        foreach (Token token in lexer.ScanTokens())
        {
            if (token.Kind == TokenKind.EndOfSource) yield break;
            int line = lexer.LogicalLine;
            bool startsLine = line > previousLine;
            int lineEnd = lexer.Input.Text.IndexOfAny(['\r', '\n'], lexer.LastTokenEnd);
            if (lineEnd < 0) lineEnd = lexer.Input.Text.Length;
            SourcePosition physicalEnd = lexer.Input.OriginForRange(lexer.Input.Range(lineEnd, 0)).SpellingRanges[^1].EndPosition;
            TokenOrigin origin = token.Origin.WithContext(new(physicalEnd.File.Name, physicalEnd.Line, token.Origin.SpellingRanges[0].StartPosition.Column), null);
            yield return new PpToken(token.Kind, token.Range, origin, lexer.LastTokenStart != previousEnd, startsLine, line);
            previousEnd = lexer.LastTokenEnd;
            previousLine = line;
        }
    }
    internal PreprocessingResult Process(SourceFile source)
    {
        files.Add(IncludePaths.Normalize(source.Name), source);
        physicalSources.Add(source);
        DateTimeOffset timestamp = options.TranslationTimestamp ?? throw new InvalidOperationException("Preprocessing requires the session's captured translation timestamp.");
        string[] months = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];
        var builtins = new Dictionary<string, string>(options.PredefinedMacros, StringComparer.Ordinal)
        {
            ["__cplusplus"] = "202002L", ["__STDC_HOSTED__"] = options.Hosted ? "1" : "0",
            ["__DATE__"] = QuoteFilename(months[timestamp.Month - 1] + " " + timestamp.Day.ToString(System.Globalization.CultureInfo.InvariantCulture).PadLeft(2, ' ') + " " + timestamp.Year.ToString("D4", System.Globalization.CultureInfo.InvariantCulture)),
            ["__TIME__"] = QuoteFilename(timestamp.ToString("HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture))
        };
        foreach (var (name, value) in builtins)
        {
            var definition = new SourceFile("<predefined " + name + ">", name + " " + value, SourceFileKind.Configuration);
            PpToken[] tokens = Tokenize(definition).ToArray();
            if (tokens.Length == 0 || tokens[0].Kind != TokenKind.Identifier || tokens[0].Text != name)
            {
                Error("PP002", "Invalid configured macro name.", TokenOrigin.Direct(definition.Range(0, definition.Text.Length)));
                continue;
            }
            macros[name] = new MacroDefinition(name, tokens[0].Origin, false, false, [], tokens[1..]);
        }
        var output = new List<PpToken>();
        try { output.AddRange(Expand(ReadActive(source, null, 0), 0)); }
        catch (PreprocessingStopped) { }
        foreach (PpToken token in output)
            if (token.Kind == TokenKind.PreprocessingOther)
                Error("PP003", "This active preprocessing token has no implemented language token.", token.Origin,
                    token.Text.Any(c => c > 127 || c == '\\') ? DiagnosticClass.UnsupportedFeature : DiagnosticClass.InvalidSource);
        TokenBuffer buffer = Materialize(source, output);
        return new(source, buffer, physicalSources.ToArray(), includes.ToArray(), new Dictionary<string, MacroDefinition>(macros, StringComparer.Ordinal), diagnostics.Snapshot(), timestamp, options.Hosted);
    }
    private TokenBuffer Materialize(SourceFile primary, List<PpToken> output)
    {
        if (!transformed && output.All(token => token.Origin.SpellingRanges.Count == 1 &&
            ReferenceEquals(token.Origin.SpellingRanges[0].File, primary) && token.Origin.SpellingRanges[0].Spelling == token.Text))
        {
            Token[] direct = output.Select(token => new Token(token.Kind == TokenKind.Identifier ? Lexer.OperatorOrIdentifier(token.Text) : token.Kind,
                token.Origin.SpellingRanges[0], token.Origin)).Append(new Token(TokenKind.EndOfSource, primary.Range(primary.Text.Length, 0))).ToArray();
            return new TokenBuffer(primary, direct);
        }
        var text = new StringBuilder();
        var maps = new List<SourceMapSegment>();
        var positions = new List<(int Start, PpToken Token)>();
        foreach (PpToken token in output)
        {
            if (text.Length != 0) text.Append(' ');
            positions.Add((text.Length, token));
            maps.Add(new(text.Length, token.Text.Length, token.Origin));
            text.Append(token.Text);
        }
        TokenOrigin eof = TokenOrigin.Direct(primary.Range(primary.Text.Length, 0));
        maps.Add(new(text.Length, 0, eof));
        var source = new SourceFile(primary.Name + " [preprocessed presentation]", text.ToString(), SourceFileKind.PreprocessedPresentation, maps.ToArray());
        Token[] tokens = positions.Select(item => new Token(item.Token.Kind == TokenKind.Identifier ? Lexer.OperatorOrIdentifier(item.Token.Text) : item.Token.Kind,
            source.Range(item.Start, item.Token.Text.Length), item.Token.Origin)).Append(new Token(TokenKind.EndOfSource, source.Range(source.Text.Length, 0), eof)).ToArray();
        return new TokenBuffer(source, tokens);
    }
}
