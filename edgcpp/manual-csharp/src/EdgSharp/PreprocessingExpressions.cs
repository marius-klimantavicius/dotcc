// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
// Handwritten controlling-expression path following preproc.c scan_if_expr and
// expr.c scan_pp_expression: protected defined operands, intmax/uintmax target
// arithmetic, syntactic scanning of dead branches, and short-circuit evaluation.
using System.Numerics;

namespace EdgSharp;

internal sealed partial class Preprocessor
{
    private DecodedCharacterLiteral? DecodeControllingCharacter(PpToken token)
        => CharacterLiteralDecoder.Decode(token.Text, DiagnosticRange(token.Origin), target, diagnostics);
    private bool EvaluateCondition(IEnumerable<PpToken> operands, TokenOrigin position, Action? requestHeader = null)
    {
        IEnumerable<PpToken> ProtectDefined()
        {
            using IEnumerator<PpToken> input = operands.GetEnumerator();
            while (input.MoveNext())
            {
                PpToken token = input.Current;
                if (token.IdentifierText != "defined") { yield return token; continue; }
                if (!input.MoveNext()) { Error("PP050", "defined requires an identifier operand.", token.Origin); yield break; }
                bool parenthesized = input.Current.Kind == TokenKind.LeftParen;
                if (parenthesized && !input.MoveNext()) { Error("PP050", "defined requires an identifier operand.", token.Origin); yield break; }
                if (input.Current.Kind != TokenKind.Identifier) { Error("PP050", "defined requires an identifier operand.", token.Origin); yield break; }
                DiagnoseIdentifier(input.Current);
                string name = input.Current.IdentifierText;
                if (parenthesized && (!input.MoveNext() || input.Current.Kind != TokenKind.RightParen))
                { Error("PP051", "Missing ')' after defined operand.", token.Origin); yield break; }
                yield return Synthesized(IsDefined(name) ? "1" : "0", TokenKind.IntegerLiteral, token.Origin);
            }
        }
        PpToken[] expanded;
        ++controllingExpressionDepth;
        try { expanded = Expand(ProtectDefined(), 0, requestHeader: requestHeader).ToArray(); }
        finally { --controllingExpressionDepth; }
        for (int i = 0; i < expanded.Length; ++i)
        {
            PpToken token = expanded[i];
            if (token.Kind != TokenKind.Identifier) continue;
            DiagnoseIdentifier(token);
            TokenKind alternative = Lexer.OperatorOrIdentifier(token.IdentifierText);
            if (alternative is not (TokenKind.Identifier or TokenKind.Keyword))
            { expanded[i] = Synthesized(token.Text, alternative, token.Origin); continue; }
            if (token.IdentifierText is "defined" or "__has_cpp_attribute" or "__has_embed" or "_Pragma")
            {
                Error("PP052", "This preprocessing operator is not implemented in controlling expressions.", token.Origin, DiagnosticClass.UnsupportedFeature);
                return false;
            }
            expanded[i] = token.Text is "true" or "false" ? Synthesized(token.Text, TokenKind.Keyword, token.Origin) : Synthesized("0", TokenKind.IntegerLiteral, token.Origin);
        }
        return new PpExpression(this, expanded, position).Evaluate();
    }
    private readonly record struct PpValue(BigInteger Value, bool Unsigned, int Width = 64)
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
                    (bool unsigned, int width) = CommonType(yes, no);
                    left = new(evaluate ? Bits(left.True ? yes.Value : no.Value, unsigned, width) : 0, unsigned, width);
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
                return Checked(value, token.Kind != TokenKind.Bang && operand.Unsigned, evaluate, token.Origin, token.Kind == TokenKind.Bang ? 32 : operand.Width);
            }
            if (Eat(TokenKind.LeftParen))
            {
                PpValue operand = Parse(0, evaluate);
                if (!Eat(TokenKind.RightParen)) Invalid("Expected ')' in preprocessing expression.");
                return operand;
            }
            ++index;
            if (token.Kind == TokenKind.Keyword && token.Text is "true" or "false") return new(token.Text == "true" ? 1 : 0, false, 32);
            if (token.Kind == TokenKind.CharacterLiteral)
            {
                DecodedCharacterLiteral? decoded = owner.DecodeControllingCharacter(token);
                if (decoded == null) throw new InvalidExpression();
                // EDG's character token retains its target type and undergoes
                // integral promotion. It is not rescanned as a numeric token:
                // only numeric pp literals acquire intmax/uintmax widths.
                return new(decoded.Value, decoded.Kind == IntegerKind.Char32, 32);
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
        private static BigInteger Bits(BigInteger value, bool unsigned, int width = 64)
        {
            BigInteger modulus = BigInteger.One << width;
            BigInteger bits = ((value % modulus) + modulus) % modulus;
            return unsigned || bits < (modulus >> 1) ? bits : bits - modulus;
        }
        private static (bool Unsigned, int Width) CommonType(PpValue left, PpValue right)
        {
            if (left.Unsigned == right.Unsigned) return (left.Unsigned, Math.Max(left.Width, right.Width));
            PpValue u = left.Unsigned ? left : right, s = left.Unsigned ? right : left;
            return u.Width >= s.Width ? (true, u.Width) : (false, s.Width);
        }
        private PpValue Checked(BigInteger value, bool unsigned, bool evaluate, TokenOrigin origin, int width = 64)
        {
            BigInteger maximum = (BigInteger.One << (width - 1)) - 1;
            if (evaluate && !unsigned && (value > maximum || value < -maximum - 1)) Invalid("Signed overflow in preprocessing expression.", origin);
            return new(evaluate ? Bits(value, unsigned, width) : 0, unsigned, width);
        }
        private PpValue Binary(PpToken op, PpValue left, PpValue right, bool evaluate)
        {
            bool shift = op.Kind is TokenKind.ShiftLeft or TokenKind.ShiftRight;
            (bool unsigned, int width) = shift ? (left.Unsigned, left.Width) : CommonType(left, right);
            BigInteger l = Bits(left.Value, unsigned, width), r = shift ? right.Value : Bits(right.Value, unsigned, width);
            bool boolean = op.Kind is TokenKind.AndAnd or TokenKind.OrOr or TokenKind.EqualEqual or TokenKind.NotEqual or TokenKind.Less or TokenKind.Greater or TokenKind.LessEqual or TokenKind.GreaterEqual;
            if (!evaluate) return new(0, !boolean && unsigned, boolean ? 32 : width);
            if ((op.Kind is TokenKind.Slash or TokenKind.Percent) && (r.IsZero || !unsigned && l == -(BigInteger.One << (width - 1)) && r == -1))
                Invalid("Invalid division in preprocessing expression.", op.Origin);
            if (shift && (r < 0 || r >= width)) Invalid("Invalid shift count in preprocessing expression.", op.Origin);
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
            return shift ? new(Bits(value, unsigned, width), unsigned, width) : Checked(value, !boolean && unsigned, true, op.Origin, boolean ? 32 : width);
        }
    }
}
