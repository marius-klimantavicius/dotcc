// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
// Handwritten adaptation of lexical.c get_token, terminate_token_cache and
// reusable_cache_stack. No native token structures or generated input is used.
using System.Collections.ObjectModel;

namespace EdgSharp;

public enum TokenKind
{
    EndOfSource, Error, Identifier, Keyword, IntegerLiteral, FloatingLiteral, StringLiteral, CharacterLiteral,
    LeftParen, RightParen, LeftBrace, RightBrace, LeftBracket, RightBracket,
    Semicolon, Comma, Dot, Colon, Scope, Question, Hash, HashHash,
    Plus, Minus, Star, Slash, Percent, Equal, EqualEqual, NotEqual, Bang, Tilde,
    Less, Greater, LessEqual, GreaterEqual, Ampersand, Pipe, Caret, AndAnd, OrOr,
    ShiftLeft, ShiftRight, PlusPlus, MinusMinus, Arrow, ArrowStar, DotStar,
    PlusEqual, MinusEqual, StarEqual, SlashEqual, PercentEqual, AmpersandEqual,
    PipeEqual, CaretEqual, ShiftLeftEqual, ShiftRightEqual, Ellipsis, Spaceship, PreprocessingOther, HeaderName
}

public readonly record struct Token
{
    public TokenKind Kind { get; }
    public SourceRange Range { get; }
    public TokenOrigin Origin { get; }
    public string Spelling => Range.Spelling;
    /// <summary>Scalar identifier identity (direct text and UCNs compare equal), without Unicode normalization. Other tokens retain their spelling.</summary>
    public string IdentifierText { get; }
    // f_is_identifier_char diagnoses malformed UCNs while collecting pp-number
    // parts and ud-suffixes, independently of deferred literal conversion.
    internal Diagnostic? ScannedUcnDiagnostic { get; }
    public Token(TokenKind kind, SourceRange range) : this(kind, range, range.File.OriginForRange(range)) { }
    internal Token(TokenKind kind, SourceRange range, TokenOrigin origin, Diagnostic? scannedUcnDiagnostic = null)
        => (Kind, Range, Origin, IdentifierText, ScannedUcnDiagnostic) = (kind, range, origin,
            kind is TokenKind.Identifier or TokenKind.Keyword ? UnicodeIdentifiers.Identity(range.Spelling) : range.Spelling, scannedUcnDiagnostic);
}

public sealed class TokenBuffer
{
    private readonly Token[] tokens;
    public SourceFile Source { get; }
    public ReadOnlyCollection<Token> Tokens { get; }
    internal TokenBuffer(SourceFile source, Token[] tokens)
    {
        Source = source;
        this.tokens = tokens;
        Tokens = Array.AsReadOnly(tokens);
    }
    public TokenCursor CreateCursor() => new(this, 0, tokens.Length);
    /// <summary>Retains an immutable token interval with its own terminal EOF for future rescanning.</summary>
    public TokenBuffer Retain(int startIndex, int count)
    {
        if (startIndex < 0 || count < 0 || startIndex > tokens.Length - 1 - count)
            throw new ArgumentOutOfRangeException(nameof(count));
        var retained = new Token[count + 1];
        Array.Copy(tokens, startIndex, retained, 0, count);
        int end = count == 0 ? tokens[startIndex].Range.Start : retained[count - 1].Range.End;
        retained[count] = new Token(TokenKind.EndOfSource, Source.Range(end, 0));
        return new TokenBuffer(Source, retained);
    }
    internal Token At(int index) => tokens[index];
}

public readonly struct TokenCheckpoint
{
    internal TokenCursor Owner { get; }
    internal int Index { get; }
    internal TokenCheckpoint(TokenCursor owner, int index) => (Owner, Index) = (owner, index);
}

/// <summary>A cursor checkpoint restores lexical position only. Semantic transactions are separate.</summary>
public sealed class TokenCursor
{
    private readonly TokenBuffer buffer;
    private readonly int end;
    public int Index { get; private set; }
    public Token Current => buffer.At(Index);
    internal Token Previous => buffer.At(Math.Max(0, Index - 1));
    internal TokenBuffer Retain(int startIndex, int count) => buffer.Retain(startIndex, count);
    internal TokenCursor(TokenBuffer buffer, int start, int end) => (this.buffer, Index, this.end) = (buffer, start, end);
    public Token Advance()
    {
        Token previous = Current;
        if (Index < end - 1) ++Index;
        return previous;
    }
    public Token Peek(int distance = 1)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(distance);
        return buffer.At((int)Math.Min((long)Index + distance, end - 1));
    }
    public TokenCheckpoint Checkpoint() => new(this, Index);
    public void Restore(TokenCheckpoint checkpoint)
    {
        if (!ReferenceEquals(checkpoint.Owner, this)) throw new ArgumentException("Checkpoint belongs to another cursor.", nameof(checkpoint));
        Index = checkpoint.Index;
    }
}

internal sealed class Lexer(SourceFile source, DiagnosticBag diagnostics, bool preprocessing = false)
{
    private readonly string text = source.Text;
    private int position;
    private Diagnostic? scannedUcnDiagnostic;
    private bool ppDirectiveName;
    private int ppLastLine, headerLine;
    private string? ppDirective;
    private int ppDirectiveLine;
    internal int LogicalLine { get; private set; } = 1;
    internal int LastTokenStart { get; private set; }
    internal int LastTokenEnd { get; private set; }
    private readonly Dictionary<int, int> logicalLineEnds = [];
    internal int EndOfLogicalLine(int line) => logicalLineEnds.TryGetValue(line, out int end) ? end : text.Length;
    internal SourceFile Input => source;
    internal void RequestHeaderName() => headerLine = LogicalLine;
    private static readonly HashSet<string> keywords = new(StringComparer.Ordinal)
    {
        "alignas", "alignof", "and", "and_eq", "asm", "auto", "bitand", "bitor", "bool", "break", "case", "catch",
        "char", "char8_t", "char16_t", "char32_t", "class", "compl", "concept", "const", "consteval", "constexpr",
        "constinit", "const_cast", "continue", "co_await", "co_return", "co_yield", "decltype", "default", "delete", "do",
        "double", "dynamic_cast", "else", "enum", "explicit", "export", "extern", "false", "float", "for", "friend",
        "goto", "if", "inline", "int", "long", "mutable", "namespace", "new", "noexcept", "not", "not_eq", "nullptr",
        "operator", "or", "or_eq", "private", "protected", "public", "register", "reinterpret_cast", "requires", "return",
        "short", "signed", "sizeof", "static", "static_assert", "static_cast", "struct", "switch", "template", "this",
        "thread_local", "throw", "true", "try", "typedef", "typeid", "typename", "union", "unsigned", "using", "virtual",
        "void", "volatile", "wchar_t", "while", "xor", "xor_eq"
    };
    internal static TokenKind ClassifyIdentifier(string spelling) => keywords.Contains(spelling) ? TokenKind.Keyword : TokenKind.Identifier;
    // Longest spelling first, as get_token's punctuation cases require.
    private static readonly (string Spelling, TokenKind Kind)[] punctuation =
    [
        ("%:%:", TokenKind.HashHash), (">>=", TokenKind.ShiftRightEqual), ("<<=", TokenKind.ShiftLeftEqual),
        ("<=>", TokenKind.Spaceship), ("...", TokenKind.Ellipsis), ("->*", TokenKind.ArrowStar),
        ("::", TokenKind.Scope), ("++", TokenKind.PlusPlus), ("--", TokenKind.MinusMinus), ("->", TokenKind.Arrow),
        (".*", TokenKind.DotStar), ("==", TokenKind.EqualEqual), ("!=", TokenKind.NotEqual), ("<=", TokenKind.LessEqual),
        (">=", TokenKind.GreaterEqual), ("&&", TokenKind.AndAnd), ("||", TokenKind.OrOr), ("<<", TokenKind.ShiftLeft),
        (">>", TokenKind.ShiftRight), ("+=", TokenKind.PlusEqual), ("-=", TokenKind.MinusEqual), ("*=", TokenKind.StarEqual),
        ("/=", TokenKind.SlashEqual), ("%=", TokenKind.PercentEqual), ("&=", TokenKind.AmpersandEqual),
        ("|=", TokenKind.PipeEqual), ("^=", TokenKind.CaretEqual), ("##", TokenKind.HashHash),
        ("<:", TokenKind.LeftBracket), (":>", TokenKind.RightBracket), ("<%", TokenKind.LeftBrace),
        ("%>", TokenKind.RightBrace), ("%:", TokenKind.Hash),
        ("(", TokenKind.LeftParen), (")", TokenKind.RightParen), ("{", TokenKind.LeftBrace), ("}", TokenKind.RightBrace),
        ("[", TokenKind.LeftBracket), ("]", TokenKind.RightBracket), (";", TokenKind.Semicolon), (",", TokenKind.Comma),
        (".", TokenKind.Dot), (":", TokenKind.Colon), ("?", TokenKind.Question), ("#", TokenKind.Hash),
        ("+", TokenKind.Plus), ("-", TokenKind.Minus), ("*", TokenKind.Star), ("/", TokenKind.Slash), ("%", TokenKind.Percent),
        ("=", TokenKind.Equal), ("!", TokenKind.Bang), ("~", TokenKind.Tilde), ("<", TokenKind.Less), (">", TokenKind.Greater),
        ("&", TokenKind.Ampersand), ("|", TokenKind.Pipe), ("^", TokenKind.Caret)
    ];
    internal TokenBuffer Scan()
    {
        Token[] tokens = ScanTokens().ToArray();
        if (tokens.All(token => ReferenceEquals(token.Range.File, source))) return new TokenBuffer(source, tokens);
        var text = new System.Text.StringBuilder();
        var maps = new List<SourceMapSegment>();
        var offsets = new List<int>();
        foreach (Token token in tokens)
        {
            if (token.Kind == TokenKind.EndOfSource) break;
            if (text.Length != 0) text.Append(' ');
            offsets.Add(text.Length); maps.Add(new(text.Length, token.Spelling.Length, token.Origin)); text.Append(token.Spelling);
        }
        TokenOrigin eof = tokens[^1].Origin;
        maps.Add(new(text.Length, 0, eof));
        var presentation = new SourceFile(source.Name + " [lexical presentation]", text.ToString(), SourceFileKind.PreprocessedPresentation, maps.ToArray(), source.Encoding);
        return new TokenBuffer(presentation, tokens.Take(tokens.Length - 1).Select((token, index) =>
            new Token(token.Kind, presentation.Range(offsets[index], token.Spelling.Length), token.Origin, token.ScannedUcnDiagnostic)).Append(new Token(TokenKind.EndOfSource, presentation.Range(text.Length, 0), eof)).ToArray());
    }
    internal IEnumerable<Token> ScanTokens()
    {
        while (position < text.Length)
        {
            if (SkipWhiteSpaceOrComment()) continue;
            LastTokenStart = position;
            Token token = GetToken();
            LastTokenEnd = position;
            if (preprocessing)
            {
                int line = LogicalLine;
                if (ppDirectiveName)
                {
                    if (line == ppLastLine && token.Spelling is "include" or "include_next") headerLine = line;
                    if (line == ppLastLine) { ppDirective = token.Spelling; ppDirectiveLine = line; }
                    ppDirectiveName = false;
                }
                else if (token.Kind == TokenKind.Hash && line != ppLastLine) ppDirectiveName = true;
                ppLastLine = line;
            }
            yield return token;
        }
        yield return new Token(TokenKind.EndOfSource, source.Range(position, 0));
    }
    private bool SkipWhiteSpaceOrComment()
    {
        if (text[position] is ' ' or '\t' or '\r' or '\n' or '\v' or '\f')
        {
            int at = position;
            char c = text[position++];
            if (c is '\r' or '\n')
            {
                logicalLineEnds[LogicalLine] = at;
                if (c == '\r' && position < text.Length && text[position] == '\n') ++position;
                ++LogicalLine;
            }
            return true;
        }
        if (position + 1 >= text.Length || text[position] != '/') return false;
        if (text[position + 1] == '/')
        {
            position += 2;
            while (position < text.Length && text[position] is not ('\r' or '\n'))
            {
                CheckSplice();
                ++position;
            }
            return true;
        }
        if (text[position + 1] != '*') return false;
        int start = position;
        position += 2;
        while (position + 1 < text.Length && !(text[position] == '*' && text[position + 1] == '/'))
        {
            CheckSplice();
            ++position;
        }
        if (position + 1 < text.Length) position += 2;
        else
        {
            position = text.Length;
            diagnostics.Add("LEX001", "Unterminated block comment.", source.Range(start, position - start));
        }
        return true;
    }
    private void CheckSplice()
    {
        if (text[position] == '\\' && position + 1 < text.Length && text[position + 1] is '\r' or '\n')
            diagnostics.Add("UNS001", "Backslash-newline splicing is not implemented.", source.Range(position, 2), DiagnosticClass.UnsupportedFeature);
    }
    private Token GetToken()
    {
        scannedUcnDiagnostic = null;
        int start = position;
        char ch = text[position];
        if (preprocessing && headerLine != 0)
        {
            bool header = LogicalLine == headerLine && ch is '<' or '"';
            headerLine = 0;
            if (header)
            {
                char close = ch == '<' ? '>' : '"';
                ++position;
                while (position < text.Length && text[position] != close && text[position] is not ('\r' or '\n')) ++position;
                if (position < text.Length && text[position] == close) ++position;
                return new Token(TokenKind.HeaderName, source.Range(start, position - start));
            }
        }
        int identifierLength = UnicodeIdentifiers.PartLength(text, position, initial: true);
        if (identifierLength != 0)
        {
            position += identifierLength;
            int partLength;
            while (position < text.Length && (partLength = UnicodeIdentifiers.PartLength(text, position, initial: false)) != 0)
                position += partLength;
            string spelling = text[start..position];
            // Prefixes/raw strings need preprocessing-phase handling; do not split them into success-shaped identifiers.
            if (position < text.Length && text[position] is '\'' or '"' && spelling is "L" or "u" or "U" or "u8" or "R" or "LR" or "uR" or "UR" or "u8R")
            {
                if (!preprocessing && text[position] == '"') diagnostics.Add("UNS002", "Prefixed string literal values are not implemented.", source.Range(start, position - start), DiagnosticClass.UnsupportedFeature);
                if (spelling.EndsWith('R') && text[position] == '"') return ScanRaw(start);
                return ScanQuoted(start);
            }
            if (!preprocessing) UnicodeIdentifiers.Diagnose(spelling, source.Range(start, position - start), diagnostics);
            TokenKind kind = preprocessing ? TokenKind.Identifier : OperatorOrIdentifier(UnicodeIdentifiers.Identity(spelling));
            return new Token(kind, source.Range(start, position - start));
        }
        if (char.IsAsciiDigit(ch) || ch == '.' && position + 1 < text.Length && char.IsAsciiDigit(text[position + 1]))
        {
            ++position;
            while (position < text.Length)
            {
                char next = text[position];
                int part = ScanIdentifierPart(initial: false);
                if (part != 0) position += part;
                else if (next is '.' or '\'') ++position;
                else if (next is '+' or '-' && text[position - 1] is 'e' or 'E' or 'p' or 'P') ++position;
                else break;
            }
            string spelling = text[start..position];
            int suffix = spelling.IndexOf('_');
            string number = suffix < 0 ? spelling : spelling[..suffix];
            bool hex = number.StartsWith("0x", StringComparison.OrdinalIgnoreCase);
            bool floating = number.Contains('.') || (hex ? number.IndexOfAny(['p', 'P']) >= 0 : number.IndexOfAny(['e', 'E']) >= 0);
            if (!preprocessing && floating && !IsValidFloatingSpelling(spelling))
                diagnostics.Add("LEX011", "Malformed floating literal spelling.", source.Range(start, position - start));
            SourceRange range = source.Range(start, position - start);
            return new Token(floating ? TokenKind.FloatingLiteral : TokenKind.IntegerLiteral, range, source.OriginForRange(range), scannedUcnDiagnostic);
        }
        if (ch is '\'' or '"') return ScanQuoted(start);
        foreach (var (spelling, kind) in punctuation)
        {
            if (!text.AsSpan(position).StartsWith(spelling, StringComparison.Ordinal)) continue;
            // C++'s <:: exception: '<' followed by '::' is not the '<:' digraph unless followed by ':' or '>'.
            if (spelling == "<:" && position + 2 < text.Length && text[position + 2] == ':' &&
                (position + 3 >= text.Length || text[position + 3] is not (':' or '>'))) continue;
            position += spelling.Length;
            return new Token(kind, source.Range(start, position - start));
        }
        UnicodeIdentifiers.ReadScalar(text, position, out _, out int scalarLength);
        position += Math.Max(1, scalarLength);
        if (preprocessing) return new Token(TokenKind.PreprocessingOther, source.Range(start, position - start));
        bool unsupported = ch == '\\';
        diagnostics.Add(unsupported ? "UNS003" : "LEX002", unsupported ? "This source escape is not implemented." : "Invalid source character or Unicode identifier start.",
            source.Range(start, position - start), unsupported ? DiagnosticClass.UnsupportedFeature : DiagnosticClass.InvalidSource);
        return new Token(TokenKind.Error, source.Range(start, position - start));
    }
    internal static TokenKind OperatorOrIdentifier(string spelling) => spelling switch
    {
                "and" => TokenKind.AndAnd, "or" => TokenKind.OrOr, "not" => TokenKind.Bang,
                "bitand" => TokenKind.Ampersand, "bitor" => TokenKind.Pipe, "xor" => TokenKind.Caret, "compl" => TokenKind.Tilde,
                "and_eq" => TokenKind.AmpersandEqual, "or_eq" => TokenKind.PipeEqual, "xor_eq" => TokenKind.CaretEqual, "not_eq" => TokenKind.NotEqual,
                _ => ClassifyIdentifier(spelling)
    };
    // lexical.c scan_number keeps pp-number collection separate from active
    // number validation. Values and literal-operator resolution remain semantic
    // work; this recognizes significand/exponent/suffix boundaries only.
    internal static bool IsValidFloatingSpelling(string spelling)
    {
        bool hex = spelling.StartsWith("0x", StringComparison.OrdinalIgnoreCase), point = false, exponent = false;
        int index = hex ? 2 : 0, digits = 0;
        bool valid = true;
        digits += Digits(hex);
        if (index < spelling.Length && spelling[index] == '.') { point = true; ++index; digits += Digits(hex); }
        if (digits == 0 || !valid) return false;
        if (index < spelling.Length && (hex ? spelling[index] is 'p' or 'P' : spelling[index] is 'e' or 'E'))
        {
            int marker = index++;
            bool sign = index < spelling.Length && spelling[index] is '+' or '-';
            if (sign) ++index;
            if (Digits(false) != 0) exponent = true;
            else if (sign) return false;
            else index = marker; // Potential user-defined suffix, as in scan_number.
        }
        if (!valid || hex && point && !exponent) return false;
        string suffix = spelling[index..];
        if (suffix.Length == 0) return point || exponent;
        return UnicodeIdentifiers.Analyze(suffix, out _) == null;
        int Digits(bool hexadecimal)
        {
            int count = 0;
            while (index < spelling.Length)
            {
                bool digit = hexadecimal ? char.IsAsciiHexDigit(spelling[index]) : char.IsAsciiDigit(spelling[index]);
                if (digit) { ++count; ++index; continue; }
                if (spelling[index] != '\'') break;
                if (count == 0 || index + 1 == spelling.Length || !(hexadecimal ? char.IsAsciiHexDigit(spelling[index + 1]) : char.IsAsciiDigit(spelling[index + 1])))
                { valid = false; break; }
                ++index;
            }
            return count;
        }
    }
    private Token ScanRaw(int start)
    {
        int quote = position;
        SourceRange physicalQuote = source.OriginForRange(source.Range(quote, 1)).SpellingRanges[0];
        SourceFile physical = physicalQuote.File;
        int physicalEnd = LogicalInput.RawLiteralEnd(physical.Text, physicalQuote.Start, out bool closed);
        position = source.OffsetFromPhysical(physical, physicalEnd);
        int suffix = position;
        if (closed) ScanLiteralSuffix();
        string spelling = text[start..quote] + physical.Text[physicalQuote.Start..physicalEnd] + text[suffix..position];
        TokenOrigin prefix = source.OriginForRange(source.Range(start, quote - start));
        var contributors = prefix.SpellingRanges.Concat(new[] { physical.Range(physicalQuote.Start, physicalEnd - physicalQuote.Start) }).ToList();
        if (position != suffix) contributors.AddRange(source.OriginForRange(source.Range(suffix, position - suffix)).SpellingRanges);
        var origin = new TokenOrigin(contributors, prefix.Presumed, related: [prefix]);
        SourceRange range = source.Range(start, position - start);
        if (range.Spelling != spelling)
        {
            var restored = new SourceFile("<restored raw token>", spelling, SourceFileKind.SynthesizedToken, [new(0, spelling.Length, origin)]);
            range = restored.Range(0, spelling.Length);
        }
        if (!closed)
        {
            bool deferredDefinition = preprocessing && ppDirective == "define" && LogicalLine == ppDirectiveLine;
            diagnostics.Add(deferredDefinition ? "UNS005" : "LEX004", deferredDefinition ?
                "Deferred raw-literal recognition in a macro definition is not implemented." : "Malformed or unterminated raw string literal.",
                range, deferredDefinition ? DiagnosticClass.UnsupportedFeature : DiagnosticClass.InvalidSource);
        }
        return new Token(closed ? TokenKind.StringLiteral : TokenKind.Error, range, origin, scannedUcnDiagnostic);
    }
    private Token ScanQuoted(int start)
    {
        char quote = text[position++];
        bool closed = false;
        while (position < text.Length && text[position] is not ('\r' or '\n'))
        {
            char ch = text[position++];
            if (ch == quote) { closed = true; break; }
            if (ch == '\\' && position < text.Length)
            {
                if (text[position] is '\r' or '\n')
                {
                    diagnostics.Add("UNS001", "Backslash-newline splicing is not implemented.", source.Range(position - 1, 2), DiagnosticClass.UnsupportedFeature);
                    break;
                }
                ++position;
            }
        }
        if (closed) ScanLiteralSuffix();
        var range = source.Range(start, position - start);
        if (!closed) diagnostics.Add("LEX003", "Unterminated quoted literal.", range);
        return new Token(closed ? quote == '"' ? TokenKind.StringLiteral : TokenKind.CharacterLiteral : TokenKind.Error,
            range, source.OriginForRange(range), scannedUcnDiagnostic);
    }
    // These are lexical UCN syntax/scalar checks only. In preprocessing mode
    // retain the first issue for the active/skip boundary; do not diagnose
    // while a skipped group is merely being tokenized. Literal contents never
    // call this helper: only pp-number identifier parts and actual ud-suffixes.
    private int ScanIdentifierPart(bool initial)
    {
        int length = UnicodeIdentifiers.PartLength(text, position, initial, out UnicodeIdentifiers.Problem? problem);
        if (problem is UnicodeIdentifiers.Problem issue)
        {
            DiagnosticClass kind = issue.Unsupported ? DiagnosticClass.UnsupportedFeature : DiagnosticClass.InvalidSource;
            SourceRange range = source.Range(issue.Offset, issue.Length);
            scannedUcnDiagnostic ??= new Diagnostic(issue.Code, kind, issue.Message, range);
            if (!preprocessing) diagnostics.Add(issue.Code, issue.Message, range, kind);
        }
        return length;
    }
    private void ScanLiteralSuffix()
    {
        int length = ScanIdentifierPart(initial: true);
        if (length == 0) return;
        position += length;
        while (position < text.Length && (length = ScanIdentifierPart(initial: false)) != 0) position += length;
    }
}
