// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
// Handwritten controlling-expression path following preproc.c scan_if_expr and
// expr.c scan_pp_expression: protected defined operands, intmax/uintmax target
// arithmetic, syntactic scanning of dead branches, and short-circuit evaluation.
using System.Numerics;

namespace EdgSharp;

internal sealed partial class Preprocessor
{
    private bool EvaluateCondition(PpToken[] operands, TokenOrigin position)
    {
        var protectedTokens = new List<PpToken>();
        for (int i = 0; i < operands.Length; ++i)
        {
            PpToken token = operands[i];
            if (token.Text != "defined") { protectedTokens.Add(token); continue; }
            bool parenthesized = i + 1 < operands.Length && operands[i + 1].Kind == TokenKind.LeftParen;
            if (parenthesized) ++i;
            if (++i >= operands.Length || operands[i].Kind != TokenKind.Identifier)
            { Error("PP050", "defined requires an identifier operand.", token.Origin); return false; }
            string name = operands[i].Text;
            if (parenthesized && (++i >= operands.Length || operands[i].Kind != TokenKind.RightParen))
            { Error("PP051", "Missing ')' after defined operand.", token.Origin); return false; }
            bool value = macros.ContainsKey(name) || name is "__LINE__" or "__FILE__";
            protectedTokens.Add(Synthesized(value ? "1" : "0", TokenKind.IntegerLiteral, token.Origin));
        }
        PpToken[] expanded = Expand(protectedTokens, 0).ToArray();
        for (int i = 0; i < expanded.Length; ++i)
        {
            PpToken token = expanded[i];
            if (token.Kind != TokenKind.Identifier) continue;
            TokenKind alternative = Lexer.OperatorOrIdentifier(token.Text);
            if (alternative is not (TokenKind.Identifier or TokenKind.Keyword))
            { expanded[i] = Synthesized(token.Text, alternative, token.Origin); continue; }
            if (token.Text is "defined" or "__has_include" or "__has_cpp_attribute" or "__has_embed" or "_Pragma")
            {
                Error("PP052", "This preprocessing operator is not implemented in controlling expressions.", token.Origin, DiagnosticClass.UnsupportedFeature);
                return false;
            }
            expanded[i] = Synthesized(token.Text == "true" ? "1" : "0", TokenKind.IntegerLiteral, token.Origin);
        }
        return new PpExpression(this, expanded, position).Evaluate();
    }
    private readonly record struct PpValue(BigInteger Value, bool Unsigned)
    {
        internal bool True => !Value.IsZero;
    }
    private sealed class PpExpression(Preprocessor owner, PpToken[] tokens, TokenOrigin position)
    {
        private int index, nesting;
        private static readonly BigInteger Modulus = BigInteger.One << 64;
        private static readonly BigInteger SignedMaximum = (BigInteger.One << 63) - 1;
        private sealed class InvalidExpression : Exception { }
        private PpToken? Current => index < tokens.Length ? tokens[index] : null;
        private bool Eat(TokenKind kind) { if (Current?.Kind != kind) return false; ++index; return true; }
        private void Invalid(string message, TokenOrigin? origin = null)
        {
            owner.Error("PP053", message, origin ?? Current?.Origin ?? position);
            throw new InvalidExpression();
        }
        internal bool Evaluate()
        {
            try
            {
                PpValue result = Parse(0, true);
                if (Current != null) Invalid("Unexpected token in preprocessing expression.");
                return result.True;
            }
            catch (InvalidExpression) { return false; }
        }
        private PpValue Parse(int minimum, bool evaluate)
        {
            if (++nesting > 256)
            {
                owner.Error("PP054", "Preprocessing expression nesting limit exceeded.", Current?.Origin ?? position, DiagnosticClass.ResourceLimit);
                throw new InvalidExpression();
            }
            try
            {
                PpValue left = Unary(evaluate);
                while (Current is PpToken op && Precedence(op.Kind) >= minimum && Precedence(op.Kind) >= 0)
                {
                    ++index;
                    bool evaluateRight = evaluate && (op.Kind != TokenKind.AndAnd || left.True) && (op.Kind != TokenKind.OrOr || !left.True);
                    PpValue right = Parse(Precedence(op.Kind) + 1, evaluateRight);
                    left = Binary(op, left, right, evaluate);
                }
                if (minimum == 0 && Eat(TokenKind.Question))
                {
                    PpValue yes = Parse(0, evaluate && left.True);
                    if (!Eat(TokenKind.Colon)) Invalid("Expected ':' in preprocessing conditional expression.");
                    PpValue no = Parse(0, evaluate && !left.True);
                    bool unsigned = yes.Unsigned || no.Unsigned;
                    left = new(evaluate ? Bits(left.True ? yes.Value : no.Value, unsigned) : 0, unsigned);
                }
                return left;
            }
            finally { --nesting; }
        }
        private PpValue Unary(bool evaluate)
        {
            PpToken? token = Current;
            if (token == null) { Invalid("Expected an operand in preprocessing expression."); return default; }
            owner.Tick(token.Origin);
            if (token.Kind is TokenKind.Plus or TokenKind.Minus or TokenKind.Bang or TokenKind.Tilde)
            {
                ++index;
                PpValue operand = Parse(11, evaluate);
                BigInteger value = token.Kind switch { TokenKind.Minus => -operand.Value, TokenKind.Tilde => ~operand.Value,
                    TokenKind.Bang => operand.True ? BigInteger.Zero : BigInteger.One, _ => operand.Value };
                return Checked(value, token.Kind != TokenKind.Bang && operand.Unsigned, evaluate, token.Origin);
            }
            if (Eat(TokenKind.LeftParen))
            {
                PpValue operand = Parse(0, evaluate);
                if (!Eat(TokenKind.RightParen)) Invalid("Expected ')' in preprocessing expression.");
                return operand;
            }
            ++index;
            if (token.Kind == TokenKind.CharacterLiteral)
            {
                owner.Error("PP055", "Character operands in preprocessing expressions are not implemented.", token.Origin, DiagnosticClass.UnsupportedFeature);
                throw new InvalidExpression();
            }
            if (token.Kind != TokenKind.IntegerLiteral) Invalid("Only implemented integer operands are accepted in preprocessing expressions.", token.Origin);
            string spelling = token.Text;
            int radix = 10, offset = 0;
            if (spelling.StartsWith("0x", StringComparison.OrdinalIgnoreCase)) { radix = 16; offset = 2; }
            else if (spelling.StartsWith("0b", StringComparison.OrdinalIgnoreCase)) { radix = 2; offset = 2; }
            else if (spelling.Length > 1 && spelling[0] == '0') radix = 8;
            BigInteger number = 0;
            bool digitSeen = false, lastDigit = false;
            for (; offset < spelling.Length; ++offset)
            {
                char c = spelling[offset];
                int digit = c is >= '0' and <= '9' ? c - '0' : c is >= 'a' and <= 'f' ? c - 'a' + 10 : c is >= 'A' and <= 'F' ? c - 'A' + 10 : -1;
                if (digit >= 0 && digit < radix) { number = number * radix + digit; digitSeen = lastDigit = true; }
                else if (c == '\'' && lastDigit && offset + 1 < spelling.Length && char.IsAsciiHexDigit(spelling[offset + 1])) lastDigit = false;
                else break;
            }
            string suffix = spelling[offset..];
            if (!digitSeen || !lastDigit || suffix.Contains("lL", StringComparison.Ordinal) || suffix.Contains("Ll", StringComparison.Ordinal) ||
                suffix.ToLowerInvariant() is not ("" or "u" or "l" or "ul" or "lu" or "ll" or "ull" or "llu"))
                Invalid("Invalid integer literal in preprocessing expression.", token.Origin);
            bool unsigned = suffix.Contains("u", StringComparison.OrdinalIgnoreCase) || radix != 10 && number > SignedMaximum;
            if (number >= Modulus || !unsigned && number > SignedMaximum) Invalid("Preprocessing integer literal exceeds the implemented intmax/uintmax range.", token.Origin);
            return new(number, unsigned);
        }
        private static int Precedence(TokenKind kind) => kind switch
        {
            TokenKind.OrOr => 1, TokenKind.AndAnd => 2, TokenKind.Pipe => 3, TokenKind.Caret => 4, TokenKind.Ampersand => 5,
            TokenKind.EqualEqual or TokenKind.NotEqual => 6, TokenKind.Less or TokenKind.Greater or TokenKind.LessEqual or TokenKind.GreaterEqual => 7,
            TokenKind.ShiftLeft or TokenKind.ShiftRight => 8, TokenKind.Plus or TokenKind.Minus => 9,
            TokenKind.Star or TokenKind.Slash or TokenKind.Percent => 10, _ => -1
        };
        private static BigInteger Bits(BigInteger value, bool unsigned)
        {
            BigInteger bits = ((value % Modulus) + Modulus) % Modulus;
            return unsigned || bits <= SignedMaximum ? bits : bits - Modulus;
        }
        private PpValue Checked(BigInteger value, bool unsigned, bool evaluate, TokenOrigin origin)
        {
            if (evaluate && !unsigned && (value > SignedMaximum || value < -SignedMaximum - 1)) Invalid("Signed overflow in preprocessing expression.", origin);
            return new(evaluate ? Bits(value, unsigned) : 0, unsigned);
        }
        private PpValue Binary(PpToken op, PpValue left, PpValue right, bool evaluate)
        {
            bool shift = op.Kind is TokenKind.ShiftLeft or TokenKind.ShiftRight;
            bool unsigned = shift ? left.Unsigned : left.Unsigned || right.Unsigned;
            BigInteger l = Bits(left.Value, unsigned), r = shift ? right.Value : Bits(right.Value, unsigned);
            bool boolean = op.Kind is TokenKind.AndAnd or TokenKind.OrOr or TokenKind.EqualEqual or TokenKind.NotEqual or TokenKind.Less or TokenKind.Greater or TokenKind.LessEqual or TokenKind.GreaterEqual;
            if (!evaluate) return new(0, !boolean && unsigned);
            if ((op.Kind is TokenKind.Slash or TokenKind.Percent) && (r.IsZero || !unsigned && l == -SignedMaximum - 1 && r == -1))
                Invalid("Invalid division in preprocessing expression.", op.Origin);
            if (shift && (r < 0 || r >= 64)) Invalid("Invalid shift count in preprocessing expression.", op.Origin);
            BigInteger value = op.Kind switch
            {
                TokenKind.Plus => l + r, TokenKind.Minus => l - r, TokenKind.Star => l * r, TokenKind.Slash => l / r, TokenKind.Percent => l % r,
                TokenKind.ShiftLeft => l << (int)r, TokenKind.ShiftRight => l >> (int)r,
                TokenKind.Pipe => l | r, TokenKind.Caret => l ^ r, TokenKind.Ampersand => l & r,
                TokenKind.AndAnd => left.True && right.True ? 1 : 0, TokenKind.OrOr => left.True || right.True ? 1 : 0,
                TokenKind.EqualEqual => l == r ? 1 : 0, TokenKind.NotEqual => l != r ? 1 : 0,
                TokenKind.Less => l < r ? 1 : 0, TokenKind.Greater => l > r ? 1 : 0,
                TokenKind.LessEqual => l <= r ? 1 : 0, TokenKind.GreaterEqual => l >= r ? 1 : 0,
                _ => throw new InvalidOperationException("Unexpected preprocessing binary operator.")
            };
            return shift ? new(Bits(value, unsigned), unsigned) : Checked(value, !boolean && unsigned, true, op.Origin);
        }
    }
}
