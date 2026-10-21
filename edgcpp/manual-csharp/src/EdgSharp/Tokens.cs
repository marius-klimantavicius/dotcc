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
    public Token(TokenKind kind, SourceRange range) : this(kind, range, range.File.OriginForRange(range)) { }
    internal Token(TokenKind kind, SourceRange range, TokenOrigin origin) => (Kind, Range, Origin) = (kind, range, origin);
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
    private bool ppDirectiveName;
    private int ppLastLine, headerLine;
    private string? ppDirective;
    private int ppDirectiveLine;
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
        var result = new List<Token>();
        while (position < text.Length)
        {
            if (SkipWhiteSpaceOrComment()) continue;
            Token token = GetToken();
            result.Add(token);
            if (preprocessing)
            {
                int line = source.PositionAt(token.Range.Start).Line;
                if (ppDirectiveName)
                {
                    if (line == ppLastLine && token.Spelling is "include" or "include_next") headerLine = line;
                    if (line == ppLastLine) { ppDirective = token.Spelling; ppDirectiveLine = line; }
                    ppDirectiveName = false;
                }
                else if (token.Kind == TokenKind.Hash && line != ppLastLine) ppDirectiveName = true;
                ppLastLine = line;
            }
        }
        result.Add(new Token(TokenKind.EndOfSource, source.Range(position, 0)));
        return new TokenBuffer(source, result.ToArray());
    }
    private bool SkipWhiteSpaceOrComment()
    {
        if (text[position] is ' ' or '\t' or '\r' or '\n' or '\v' or '\f') { ++position; return true; }
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
        int start = position;
        char ch = text[position];
        if (preprocessing && headerLine != 0)
        {
            bool header = source.PositionAt(position).Line == headerLine && ch is '<' or '"';
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
        if (IsIdentifierStart(ch))
        {
            ++position;
            while (position < text.Length && IsIdentifierPart(text[position])) ++position;
            string spelling = text[start..position];
            // Prefixes/raw strings need preprocessing-phase handling; do not split them into success-shaped identifiers.
            if (position < text.Length && text[position] is '\'' or '"' && spelling is "L" or "u" or "U" or "u8" or "R" or "LR" or "uR" or "UR" or "u8R")
            {
                if (!preprocessing) diagnostics.Add("UNS002", "Prefixed literal values are not implemented.", source.Range(start, position - start), DiagnosticClass.UnsupportedFeature);
                if (spelling.EndsWith('R') && text[position] == '"') return ScanRaw(start);
                return ScanQuoted(start);
            }
            TokenKind kind = preprocessing ? TokenKind.Identifier : OperatorOrIdentifier(spelling);
            return new Token(kind, source.Range(start, position - start));
        }
        if (char.IsAsciiDigit(ch) || ch == '.' && position + 1 < text.Length && char.IsAsciiDigit(text[position + 1]))
        {
            ++position;
            while (position < text.Length)
            {
                char next = text[position];
                if (IsIdentifierPart(next) || next is '.' or '\'') ++position;
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
            return new Token(floating ? TokenKind.FloatingLiteral : TokenKind.IntegerLiteral, source.Range(start, position - start));
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
        ++position;
        if (preprocessing) return new Token(TokenKind.PreprocessingOther, source.Range(start, 1));
        bool unsupported = ch > 127 || ch == '\\';
        diagnostics.Add(unsupported ? "UNS003" : "LEX002", unsupported ? "Unicode identifiers and universal character names are not implemented." : "Invalid source character.",
            source.Range(start, 1), unsupported ? DiagnosticClass.UnsupportedFeature : DiagnosticClass.InvalidSource);
        return new Token(TokenKind.Error, source.Range(start, 1));
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
        return IsIdentifierStart(suffix[0]) && suffix.All(IsIdentifierPart);
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
        position = LogicalInput.RawLiteralEnd(text, quote, out bool closed);
        var range = source.Range(start, position - start);
        if (!closed)
        {
            bool deferredDefinition = preprocessing && ppDirective == "define" && source.PositionAt(start).Line == ppDirectiveLine;
            diagnostics.Add(deferredDefinition ? "UNS005" : "LEX004", deferredDefinition ?
                "Deferred raw-literal recognition in a macro definition is not implemented." : "Malformed or unterminated raw string literal.",
                range, deferredDefinition ? DiagnosticClass.UnsupportedFeature : DiagnosticClass.InvalidSource);
        }
        return new Token(closed ? TokenKind.StringLiteral : TokenKind.Error, range);
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
        var range = source.Range(start, position - start);
        if (!closed) diagnostics.Add("LEX003", "Unterminated quoted literal.", range);
        return new Token(closed ? quote == '"' ? TokenKind.StringLiteral : TokenKind.CharacterLiteral : TokenKind.Error, range);
    }
    private static bool IsIdentifierStart(char ch) => char.IsAsciiLetter(ch) || ch == '_';
    private static bool IsIdentifierPart(char ch) => IsIdentifierStart(ch) || char.IsAsciiDigit(ch);
}
