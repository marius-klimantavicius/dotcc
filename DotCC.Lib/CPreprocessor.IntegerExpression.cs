using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using LALR.CC.LexicalGrammar;

namespace DotCC;

internal sealed partial class CPreprocessor
{
    private bool EvaluateConditionalExpression(IReadOnlyList<Item> tokens)
    {
        // defined's operand names the macro itself, so protect it before ordinary
        // macro expansion. All remaining identifiers are handled by the parser.
        var protectedTokens = new List<Item>();
        for (int i = 0; i < tokens.Count; i++)
        {
            var token = tokens[i];
            if (token.Content?.ToString() != "defined") { protectedTokens.Add(token); continue; }
            bool parentheses = i + 1 < tokens.Count && tokens[i + 1].Content?.ToString() == "(";
            if (parentheses) i++;
            if (++i >= tokens.Count) throw new CompileException("#if: defined requires a macro name");
            string name = tokens[i].Content?.ToString() ?? "";
            if (name.Length == 0 || !(char.IsLetter(name[0]) || name[0] == '_') ||
                name.Any(c => !(char.IsLetterOrDigit(c) || c == '_')))
                throw new CompileException("#if: defined requires a macro name");
            protectedTokens.Add(SourceMappedItem.Create(_numSymbolId, IsDefined(name) ? "1" : "0", token));
            if (parentheses && (++i >= tokens.Count || tokens[i].Content?.ToString() != ")"))
                throw new CompileException("#if: missing ')' after defined");
        }
        var expanded = NormalizeConditionalCharacters(ExpandDirectiveTokens(protectedTokens)).ToArray();
        return new ConditionalIntegerParser(this, expanded).Evaluate();
    }

    // In preprocessing, all signed and unsigned integer types act as intmax_t
    // and uintmax_t. LP64 dotcc therefore needs one 64-bit payload plus its
    // signedness, including for the usual conversions in a conditional expression.
    private readonly record struct ConditionalInteger(ulong Bits, bool Unsigned = false)
    {
        internal long Signed => unchecked((long)Bits);
        internal bool True => Bits != 0;
        internal static ConditionalInteger Boolean(bool value) => new(value ? 1UL : 0UL);
    }

    private sealed class ConditionalIntegerParser(CPreprocessor cpp, IReadOnlyList<Item> tokens)
    {
        private int _position;
        private string? Peek => _position < tokens.Count ? tokens[_position].Content?.ToString() : null;
        private bool Take(string text)
        {
            if (Peek != text) return false;
            _position++;
            return true;
        }
        private void Require(string text)
        {
            if (!Take(text)) throw new CompileException("#if: expected '" + text + "'");
        }
        internal bool Evaluate()
        {
            var result = Conditional(true);
            if (_position != tokens.Count) throw new CompileException("#if: unexpected token '" + Peek + "'");
            return result.True;
        }
        private ConditionalInteger Conditional(bool evaluate)
        {
            var condition = Binary(1, evaluate);
            if (!Take("?")) return condition;
            var left = Conditional(evaluate && condition.True);
            Require(":");
            var right = Conditional(evaluate && !condition.True);
            return new ConditionalInteger(condition.True ? left.Bits : right.Bits, left.Unsigned || right.Unsigned);
        }
        private static int Precedence(string? op) => op switch
        {
            "||" => 1, "&&" => 2, "|" => 3, "^" => 4, "&" => 5,
            "==" or "!=" => 6, "<" or ">" or "<=" or ">=" => 7,
            "<<" or ">>" => 8, "+" or "-" => 9, "*" or "/" or "%" => 10, _ => 0,
        };
        private ConditionalInteger Binary(int minimum, bool evaluate)
        {
            var left = Unary(evaluate);
            while (Precedence(Peek) >= minimum)
            {
                string op = Peek!;
                _position++;
                bool rightEvaluated = evaluate && (op != "&&" || left.True) && (op != "||" || !left.True);
                var right = Binary(Precedence(op) + 1, rightEvaluated);
                left = Apply(op, left, right, evaluate);
            }
            return left;
        }
        private ConditionalInteger Unary(bool evaluate)
        {
            if (Take("+")) return Unary(evaluate);
            if (Take("-")) { var value = Unary(evaluate); return new(unchecked(0UL - value.Bits), value.Unsigned); }
            if (Take("~")) { var value = Unary(evaluate); return new(~value.Bits, value.Unsigned); }
            if (Take("!")) return ConditionalInteger.Boolean(!Unary(evaluate).True);
            if (Take("(")) { var value = Conditional(evaluate); Require(")"); return value; }
            if (_position >= tokens.Count) throw new CompileException("#if: expected an integer expression");
            var token = tokens[_position++];
            string text = token.Content?.ToString() ?? "";
            if (text is "__has_include" or "__has_embed" or "__has_attribute")
            {
                Require("(");
                var arguments = new List<Item>();
                int depth = 1;
                while (_position < tokens.Count)
                {
                    var argument = tokens[_position++];
                    if (argument.Content?.ToString() == "(") depth++;
                    if (argument.Content?.ToString() == ")" && --depth == 0) break;
                    arguments.Add(argument);
                }
                if (depth != 0) throw new CompileException("#if: unterminated " + text);
                return !evaluate ? default : ParseInteger(cpp.ExpandFuncMacro(text, arguments).Single().Content.ToString()!);
            }
            if (text.Length > 0 && (char.IsDigit(text[0]) || text[0] == '-')) return ParseInteger(text);
            if (text.Length > 0 && (char.IsLetter(text[0]) || text[0] == '_')) return default;
            throw new CompileException("#if: unexpected token '" + text + "'");
        }
        private static ConditionalInteger ParseInteger(string spelling)
        {
            // Character normalization may emit one signed decimal token (for
            // example a multichar constant whose high byte sets the sign bit).
            if (spelling.StartsWith('-') && long.TryParse(spelling, NumberStyles.Integer,
                    CultureInfo.InvariantCulture, out long signed))
                return new(unchecked((ulong)signed));
            string text = spelling.Replace("'", "", StringComparison.Ordinal);
            int end = text.Length;
            while (end > 0 && text[end - 1] is 'u' or 'U' or 'l' or 'L') end--;
            bool unsigned = text[end..].Contains("u", StringComparison.OrdinalIgnoreCase);
            string digits = text[..end];
            int radix = 10, start = 0;
            if (digits.StartsWith("0x", StringComparison.OrdinalIgnoreCase)) { radix = 16; start = 2; }
            else if (digits.StartsWith("0b", StringComparison.OrdinalIgnoreCase)) { radix = 2; start = 2; }
            else if (digits.Length > 1 && digits[0] == '0') { radix = 8; start = 1; }
            ulong value = 0;
            try
            {
                if (start == digits.Length) throw new FormatException();
                for (int i = start; i < digits.Length; i++)
                {
                    char c = digits[i];
                    int digit = c is >= '0' and <= '9' ? c - '0'
                        : c is >= 'a' and <= 'f' ? c - 'a' + 10
                        : c is >= 'A' and <= 'F' ? c - 'A' + 10 : -1;
                    if (digit < 0 || digit >= radix) throw new FormatException();
                    value = checked(value * (uint)radix + (uint)digit);
                }
            }
            catch (Exception error) when (error is FormatException or OverflowException)
            { throw new CompileException("#if: invalid or out-of-range integer '" + spelling + "'"); }
            return new(value, unsigned || value > long.MaxValue);
        }
        private static ConditionalInteger Apply(string op, ConditionalInteger left, ConditionalInteger right, bool evaluate)
        {
            bool unsigned = left.Unsigned || right.Unsigned;
            if (!evaluate) return new(0, op is "<<" or ">>" ? left.Unsigned :
                op is "||" or "&&" or "==" or "!=" or "<" or ">" or "<=" or ">=" ? false : unsigned);
            int Compare() => unsigned ? left.Bits.CompareTo(right.Bits) : left.Signed.CompareTo(right.Signed);
            switch (op)
            {
                case "||": return ConditionalInteger.Boolean(left.True || right.True);
                case "&&": return ConditionalInteger.Boolean(left.True && right.True);
                case "==": return ConditionalInteger.Boolean(left.Bits == right.Bits);
                case "!=": return ConditionalInteger.Boolean(left.Bits != right.Bits);
                case "<": return ConditionalInteger.Boolean(Compare() < 0);
                case ">": return ConditionalInteger.Boolean(Compare() > 0);
                case "<=": return ConditionalInteger.Boolean(Compare() <= 0);
                case ">=": return ConditionalInteger.Boolean(Compare() >= 0);
                case "+": return new(unchecked(left.Bits + right.Bits), unsigned);
                case "-": return new(unchecked(left.Bits - right.Bits), unsigned);
                case "*": return new(unchecked(left.Bits * right.Bits), unsigned);
                case "|": return new(left.Bits | right.Bits, unsigned);
                case "&": return new(left.Bits & right.Bits, unsigned);
                case "^": return new(left.Bits ^ right.Bits, unsigned);
                case "<<": case ">>":
                    if (right.Bits >= 64) throw new CompileException("#if: shift count is outside 0..63");
                    return new(op == "<<" ? left.Bits << (int)right.Bits : left.Unsigned
                        ? left.Bits >> (int)right.Bits : unchecked((ulong)(left.Signed >> (int)right.Bits)), left.Unsigned);
                case "/": case "%":
                    if (right.Bits == 0) throw new CompileException("#if: division by zero");
                    if (!unsigned && left.Signed == long.MinValue && right.Signed == -1)
                        throw new CompileException("#if: signed division overflows intmax_t");
                    return new(unsigned ? (op == "/" ? left.Bits / right.Bits : left.Bits % right.Bits)
                        : unchecked((ulong)(op == "/" ? left.Signed / right.Signed : left.Signed % right.Signed)), unsigned);
                default: throw new CompileException("#if: unsupported operator '" + op + "'");
            }
        }
    }
}
