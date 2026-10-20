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
    PipeEqual, CaretEqual, ShiftLeftEqual, ShiftRightEqual, Ellipsis, Spaceship
}

public readonly record struct Token(TokenKind Kind, SourceRange Range)
{
    public string Spelling => Range.Spelling;
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

internal sealed class Lexer(SourceFile source, DiagnosticBag diagnostics)
{
    private readonly string text = source.Text;
    private int position;
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
            result.Add(GetToken());
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
        if (IsIdentifierStart(ch))
        {
            ++position;
            while (position < text.Length && IsIdentifierPart(text[position])) ++position;
            string spelling = text[start..position];
            // Prefixes/raw strings need preprocessing-phase handling; do not split them into success-shaped identifiers.
            if (position < text.Length && text[position] is '\'' or '"' && spelling is "L" or "u" or "U" or "u8" or "R" or "LR" or "uR" or "UR" or "u8R")
            {
                diagnostics.Add("UNS002", "Prefixed and raw literals are not implemented.", source.Range(start, position - start), DiagnosticClass.UnsupportedFeature);
                return ScanQuoted(start);
            }
            TokenKind kind = spelling switch
            {
                "and" => TokenKind.AndAnd, "or" => TokenKind.OrOr, "not" => TokenKind.Bang,
                "bitand" => TokenKind.Ampersand, "bitor" => TokenKind.Pipe, "xor" => TokenKind.Caret, "compl" => TokenKind.Tilde,
                "and_eq" => TokenKind.AmpersandEqual, "or_eq" => TokenKind.PipeEqual, "xor_eq" => TokenKind.CaretEqual, "not_eq" => TokenKind.NotEqual,
                _ => keywords.Contains(spelling) ? TokenKind.Keyword : TokenKind.Identifier
            };
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
            bool hex = spelling.StartsWith("0x", StringComparison.OrdinalIgnoreCase);
            bool floating = spelling.Contains('.') || (hex ? spelling.IndexOfAny(['p', 'P']) >= 0 : spelling.IndexOfAny(['e', 'E']) >= 0);
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
        bool unsupported = ch > 127 || ch == '\\';
        diagnostics.Add(unsupported ? "UNS003" : "LEX002", unsupported ? "Unicode identifiers, universal character names and line splicing are not implemented." : "Invalid source character.",
            source.Range(start, 1), unsupported ? DiagnosticClass.UnsupportedFeature : DiagnosticClass.InvalidSource);
        return new Token(TokenKind.Error, source.Range(start, 1));
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
