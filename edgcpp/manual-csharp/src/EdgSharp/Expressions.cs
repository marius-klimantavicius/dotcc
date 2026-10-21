// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
// Handwritten bounded port of expr.c scan_expr_full, scan_identifier,
// scan_call_arguments/scan_function_call/scan_return_expression, exprutil.c
// determine_arithmetic_conversions_full and literals.c integer candidate ordering.
using System.Numerics;

namespace EdgSharp;

internal sealed partial class Parser
{
    // As in EDG scan_expr_full: dispatch one operand first, then consume postfix
    // and binary operators while their precedence is at least the caller's bound.
    // Left-associative operators recurse at precedence+1. Semantic actions and
    // target conversions happen immediately, before scanning the next operator.
    private Expression ScanExprFull(int minimumPrecedence = 0)
    {
        Expression left = ScanOperand();
        while (true)
        {
            if (Is(TokenKind.LeftParen)) { left = ScanFunctionCall(left); continue; }
            int precedence = BinaryPrecedence(Current.Kind);
            if (precedence < minimumPrecedence || precedence < 0) break;
            Token op = Take();
            Expression right = ScanExprFull(precedence + 1);
            left = Binary(op, left, right);
        }
        if (IsUnsupportedExpressionContinuation() && !Is(TokenKind.Comma))
            UnsupportedAndThrow("This expression operator or expression form is not implemented.");
        return left;
    }
    private static int BinaryPrecedence(TokenKind kind) => kind switch
    {
        TokenKind.OrOr => 1,
        TokenKind.AndAnd => 2,
        TokenKind.Pipe => 3,
        TokenKind.Caret => 4,
        TokenKind.Ampersand => 5,
        TokenKind.EqualEqual or TokenKind.NotEqual => 6,
        TokenKind.Less or TokenKind.Greater or TokenKind.LessEqual or TokenKind.GreaterEqual => 7,
        TokenKind.Plus or TokenKind.Minus => 8,
        TokenKind.Star or TokenKind.Slash or TokenKind.Percent => 9,
        _ => -1
    };
    private Expression ScanOperand()
    {
        Token token = Current;
        if (token.Kind is TokenKind.Plus or TokenKind.Minus or TokenKind.Bang or TokenKind.Tilde)
        {
            Take();
            Expression operand = ScanExprFull(10);
            if (operand.Type.Kind == TypeKind.Error) return operand;
            if (prototypeAnalysis && TypeDependent(operand))
                return new DependentUnaryExpression(token.Kind, operand, token.Kind == TokenKind.Bang ? types.Integer(IntegerKind.Bool) : types.Dependent,
                    SourceRange.Cover(token.Range, operand.Range));
            if (operand.Type is not IntegerType integer)
            {
                if (operand.Type.Kind is TypeKind.Function or TypeKind.OverloadSet) Unsupported("Function-to-pointer decay and conversions of function designators are not implemented.", operand.Range);
                else Fail("SEM020", "Unary operator requires an integer operand.", token.Range);
                return new ErrorExpression(types.Error, SourceRange.Cover(token.Range, operand.Range));
            }
            IntegerType target = token.Kind == TokenKind.Bang ? types.Integer(IntegerKind.Bool) : types.Promote(integer);
            operand = Convert(operand, token.Kind == TokenKind.Bang ? types.Integer(IntegerKind.Bool) : target);
            BigInteger? value = null;
            if (operand.ConstantValue is BigInteger constant)
            {
                BigInteger folded = token.Kind switch
                {
                    TokenKind.Plus => constant,
                    TokenKind.Minus => -constant,
                    TokenKind.Bang => constant.IsZero ? BigInteger.One : BigInteger.Zero,
                    TokenKind.Tilde => ~constant,
                    _ => throw new InvalidOperationException()
                };
                value = FoldWithinTarget(folded, target);
            }
            return new UnaryExpression(token.Kind, operand, target, SourceRange.Cover(token.Range, operand.Range), value);
        }
        if (Eat(TokenKind.LeftParen))
        {
            if (StartsDeclaration()) UnsupportedAndThrow("Casts and type-id expressions are not implemented.");
            Expression operand = ScanExprFull();
            if (Is(TokenKind.Comma)) UnsupportedAndThrow("The comma expression operator is not implemented.");
            Token end = Expect(TokenKind.RightParen, "')' after expression");
            return new ParenthesizedExpression(operand, SourceRange.Cover(token.Range, end.Range));
        }
        if (token.Kind == TokenKind.IntegerLiteral) { Take(); return IntegerLiteral(token); }
        if (token.Kind == TokenKind.CharacterLiteral)
        {
            Take();
            DecodedCharacterLiteral? character = CharacterLiteralDecoder.Decode(token.Spelling, token.Range, options.Target, state.Diagnostics);
            return character == null ? new ErrorExpression(types.Error, token.Range) : new CharacterLiteralExpression(types.Integer(character.Kind), token.Range, character);
        }
        if (token.Kind == TokenKind.FloatingLiteral && !Lexer.IsValidFloatingSpelling(token.Spelling))
        {
            Take(); Fail("LEX011", "Malformed floating literal spelling.", token.Range);
            return new ErrorExpression(types.Error, token.Range);
        }
        if (Is("true") || Is("false")) { Take(); return new LiteralExpression(types.Integer(IntegerKind.Bool), token.Range, token.Spelling == "true" ? BigInteger.One : BigInteger.Zero); }
        if (token.Kind == TokenKind.Identifier)
        {
            Take();
            Symbol[]? symbols = VisibleSymbols(token.Spelling);
            if (symbols == null)
            {
                if (prototypeAnalysis && Is(TokenKind.LeftParen))
                    return new UnresolvedFunctionExpression(token.Spelling, types.Dependent, token.Range);
                Fail("SEM021", $"Unresolved identifier '{token.Spelling}'.", token.Range);
                return new ErrorExpression(types.Error, token.Range);
            }
            if (symbols.Length != 1 || symbols[0].Kind == SymbolKind.FunctionTemplate)
                return new OverloadSetExpression(types.OverloadSet, symbols, token.Range);
            Symbol symbol = symbols[0];
            if (symbol.Kind is SymbolKind.TypeAlias or SymbolKind.TemplateParameter)
            {
                Unsupported("Type names in expression position and functional casts are not implemented.", token.Range);
                return new ErrorExpression(types.Error, token.Range);
            }
            if (symbol.Name == "main" && symbol.Scope.Kind == ScopeKind.TranslationUnit)
                Unsupported("The special use rules of global main are not implemented.", token.Range);
            return new ReferenceExpression(symbol, token.Range);
        }
        if (token.Kind is TokenKind.FloatingLiteral or TokenKind.StringLiteral or TokenKind.CharacterLiteral || token.Kind == TokenKind.Keyword ||
            token.Kind is TokenKind.Star or TokenKind.Ampersand or TokenKind.PlusPlus or TokenKind.MinusMinus or TokenKind.LeftBrace or TokenKind.LeftBracket or TokenKind.Scope or TokenKind.Hash or TokenKind.HashHash)
            UnsupportedAndThrow("This literal, unary operator or expression construct is not implemented.");
        Fail("PAR003", "Expected an expression operand.", token.Range);
        throw new ParseFailure();
    }
    private Expression ScanFunctionCall(Expression callee)
    {
        Take(); // '('
        var arguments = new List<Expression>();
        if (!Is(TokenKind.RightParen))
        {
            while (true)
            {
                arguments.Add(ScanExprFull());
                if (!Eat(TokenKind.Comma)) break;
            }
        }
        Token closing = Expect(TokenKind.RightParen, "')' after call arguments");
        Expression unwrapped = callee;
        while (unwrapped is ParenthesizedExpression parenthesized) unwrapped = parenthesized.Operand;
        Symbol[] functions = unwrapped switch
        {
            ReferenceExpression reference when NameBinding.IsFunction(reference.ReferencedSymbol) => [reference.ReferencedSymbol],
            OverloadSetExpression overload => overload.Functions.ToArray(),
            _ => []
        };
        SourceRange range = SourceRange.Cover(callee.Range, closing.Range);
        if (unwrapped is UnresolvedFunctionExpression unresolved)
        {
            if (arguments.Any(TypeDependent)) Unsupported("Dependent calls without a definition-visible ordinary candidate require unimplemented argument-dependent lookup.", callee.Range);
            else Fail("SEM021", $"Unresolved function '{unresolved.Name}'.", callee.Range);
            return new ErrorExpression(types.Error, range);
        }
        if (functions.Length == 0)
        {
            if (prototypeAnalysis && TypeDependent(callee))
                return new DependentCallExpression(callee, [], arguments.ToArray(), types.Dependent, range);
            if (callee.Type.Kind != TypeKind.Error) Fail("SEM022", "The called expression is not a supported function designator.", callee.Range);
            return new ErrorExpression(types.Error, range);
        }
        if (arguments.Any(argument => argument.Type.Kind == TypeKind.Error)) return new ErrorExpression(types.Error, range);
        if (prototypeAnalysis && arguments.Any(TypeDependent))
            return new DependentCallExpression(callee, functions, arguments.ToArray(), types.Dependent, range);
        if (arguments.Any(argument => argument.Type.Kind is TypeKind.Function or TypeKind.OverloadSet))
        {
            Unsupported("Function-to-pointer decay and function designator arguments are not implemented.", callee.Range);
            return new ErrorExpression(types.Error, range);
        }
        OverloadCandidate[] candidates = functions.Select(function => MatchCandidate(function, arguments)).ToArray();
        OverloadCandidate? selected = SelectBestCandidate(candidates);
        if (selected == null)
        {
            if (candidates.Any(candidate => candidate.IsViable))
                Fail("SEM026", $"Ambiguous call to '{functions[0].Name}': no candidate is better than every other viable candidate.", callee.Range);
            else Fail("SEM023", $"No viable function '{functions[0].Name}' for the supplied argument count and types.", callee.Range);
            return new ErrorExpression(types.Error, range);
        }
        FunctionTemplateInstance? instance = selected.Template != null ? InstantiateCandidate(selected) : null;
        if (selected.Template != null && instance == null) return new ErrorExpression(types.Error, range);
        Symbol selectedFunction = instance?.Symbol ?? selected.Function;
        FunctionType type = selected.FunctionType;
        for (int i = 0; i < arguments.Count; ++i)
            arguments[i] = type.ParameterTypes[i] is ReferenceType reference
                ? BindReference(arguments[i], reference, ReferenceBindingContext.Argument,
                    selectedBinding: selected.ArgumentConversions[i].ReferenceBinding)
                : Convert(arguments[i], type.ParameterTypes[i]);
        // EDG il.c prvalue_type/il_return_type_of remove scalar cv from a call's
        // expression type while preserving the declared function return type.
        return new CallExpression(selectedFunction, instance, selected, candidates, callee, arguments.ToArray(), types.Unqualified(type.ReturnType), range);
    }
    private Expression Convert(Expression expression, SemanticType target)
    {
        if (expression.Type.Kind == TypeKind.Error || target.Kind == TypeKind.Error) return new ErrorExpression(types.Error, expression.Range);
        if (prototypeAnalysis && (TypeDependent(expression) || target.Kind is TypeKind.TemplateParameter or TypeKind.Dependent))
            return new DependentConversionExpression(expression, target);
        if (target.Kind == TypeKind.Void && expression.Type.Kind == TypeKind.Void) return expression;
        if (expression.Type is not IntegerType source || target is not IntegerType destination)
        {
            if (expression.Type.Kind is TypeKind.Function or TypeKind.OverloadSet) Unsupported("Function-to-pointer decay and conversions of function designators are not implemented.", expression.Range);
            else Fail("SEM024", $"Cannot convert '{expression.Type}' to '{target}' in the implemented scalar subset.", expression.Range);
            return new ErrorExpression(types.Error, expression.Range);
        }
        if (expression.ValueCategory != ValueCategory.PRValue)
        {
            // lvalue-to-rvalue drops top-level cv, independently from integral conversion.
            expression = new ConversionExpression(expression, types.Integer(source.IntegerKind), ConversionKind.LValueToRValue, expression.ConstantValue);
            source = (IntegerType)expression.Type;
        }
        IntegerType unqualified = types.Integer(destination.IntegerKind);
        if (ReferenceEquals(types.Unqualified(source), unqualified)) return expression;
        ConversionKind kind = destination.IntegerKind == IntegerKind.Bool ? ConversionKind.BooleanConversion :
            IntegerConversionRank(source, unqualified) == StandardConversionRank.Promotion ? ConversionKind.IntegralPromotion : ConversionKind.IntegralConversion;
        BigInteger? value = expression.ConstantValue is BigInteger constant ? TypeTable.ConvertInteger(constant, unqualified) : null;
        return new ConversionExpression(expression, unqualified, kind, value);
    }
    private Expression Binary(Token op, Expression left, Expression right)
    {
        SourceRange range = SourceRange.Cover(left.Range, right.Range);
        if (left.Type.Kind == TypeKind.Error || right.Type.Kind == TypeKind.Error) return new ErrorExpression(types.Error, range);
        if (prototypeAnalysis && (TypeDependent(left) || TypeDependent(right)))
            return new DependentBinaryExpression(op.Kind, left, right, op.Kind is TokenKind.AndAnd or TokenKind.OrOr ? types.Integer(IntegerKind.Bool) : types.Dependent, range);
        if (op.Kind is TokenKind.AndAnd or TokenKind.OrOr)
        {
            IntegerType boolean = types.Integer(IntegerKind.Bool);
            left = Convert(left, boolean); right = Convert(right, boolean);
            BigInteger? folded = null;
            if (left.ConstantValue is BigInteger lhsValue)
            {
                bool skip = op.Kind == TokenKind.AndAnd ? lhsValue.IsZero : !lhsValue.IsZero;
                if (skip) folded = lhsValue.IsZero ? BigInteger.Zero : BigInteger.One;
                else if (right.ConstantValue is BigInteger rhsValue) folded = rhsValue.IsZero ? BigInteger.Zero : BigInteger.One;
            }
            return new BinaryExpression(op.Kind, left, right, boolean, range, folded);
        }
        if (left.Type is not IntegerType lhs || right.Type is not IntegerType rhs)
        {
            if (left.Type.Kind is TypeKind.Function or TypeKind.OverloadSet || right.Type.Kind is TypeKind.Function or TypeKind.OverloadSet) Unsupported("Function-to-pointer decay and pointer operations are not implemented.", op.Range);
            else Fail("SEM025", "This binary operator requires integer operands.", op.Range);
            return new ErrorExpression(types.Error, range);
        }
        IntegerType operationType = types.UsualArithmeticConversions(lhs, rhs);
        // EDG determines the common operation type after the integral
        // promotions. Retain both steps when the promoted int must then
        // convert to a wider or unsigned common type.
        left = Convert(left, types.Promote(lhs));
        right = Convert(right, types.Promote(rhs));
        left = Convert(left, operationType);
        right = Convert(right, operationType);
        bool comparison = op.Kind is TokenKind.EqualEqual or TokenKind.NotEqual or TokenKind.Less or TokenKind.Greater or TokenKind.LessEqual or TokenKind.GreaterEqual;
        IntegerType resultType = comparison ? types.Integer(IntegerKind.Bool) : operationType;
        BigInteger? value = null;
        if (left.ConstantValue is BigInteger l && right.ConstantValue is BigInteger r)
        {
            BigInteger? folded = op.Kind switch
            {
                TokenKind.Plus => l + r,
                TokenKind.Minus => l - r,
                TokenKind.Star => l * r,
                TokenKind.Slash => r.IsZero || operationType.IsSigned && l == operationType.Minimum && r == -1 ? null : l / r,
                TokenKind.Percent => r.IsZero || operationType.IsSigned && l == operationType.Minimum && r == -1 ? null : l % r,
                TokenKind.Ampersand => l & r,
                TokenKind.Pipe => l | r,
                TokenKind.Caret => l ^ r,
                TokenKind.EqualEqual => l == r ? BigInteger.One : BigInteger.Zero,
                TokenKind.NotEqual => l != r ? BigInteger.One : BigInteger.Zero,
                TokenKind.Less => l < r ? BigInteger.One : BigInteger.Zero,
                TokenKind.Greater => l > r ? BigInteger.One : BigInteger.Zero,
                TokenKind.LessEqual => l <= r ? BigInteger.One : BigInteger.Zero,
                TokenKind.GreaterEqual => l >= r ? BigInteger.One : BigInteger.Zero,
                _ => throw new InvalidOperationException("Unknown implemented binary operator.")
            };
            if (folded.HasValue) value = FoldWithinTarget(folded.Value, resultType);
        }
        return new BinaryExpression(op.Kind, left, right, resultType, range, value);
    }
    private static BigInteger? FoldWithinTarget(BigInteger value, IntegerType type)
        => !type.IsSigned ? TypeTable.ConvertInteger(value, type) : value >= type.Minimum && value <= type.Maximum ? value : null;

    private Expression IntegerLiteral(Token token)
    {
        string spelling = token.Spelling;
        int numberBase = 10, position = 0;
        if (spelling.StartsWith("0x", StringComparison.OrdinalIgnoreCase)) { numberBase = 16; position = 2; }
        else if (spelling.StartsWith("0b", StringComparison.OrdinalIgnoreCase)) { numberBase = 2; position = 2; }
        else if (spelling.Length > 1 && spelling[0] == '0') numberBase = 8;
        BigInteger value = BigInteger.Zero;
        bool haveDigit = false, previousDigit = false, malformed = false;
        while (position < spelling.Length)
        {
            char ch = spelling[position];
            int digit = DigitValue(ch);
            if (digit >= 0 && digit < numberBase)
            {
                value = value * numberBase + digit;
                haveDigit = previousDigit = true;
                ++position;
            }
            else if (ch == '\'')
            {
                int next = position + 1 < spelling.Length ? DigitValue(spelling[position + 1]) : -1;
                if (!previousDigit || next < 0 || next >= numberBase) malformed = true;
                previousDigit = false;
                ++position;
            }
            else break;
        }
        string suffix = spelling[position..];
        if (haveDigit && !malformed && suffix.StartsWith('_'))
        {
            Unsupported("User-defined integer literal suffixes are not implemented.", token.Range);
            return new ErrorExpression(types.Error, token.Range);
        }
        string lower = suffix.ToLowerInvariant();
        if (suffix.Contains("lL", StringComparison.Ordinal) || suffix.Contains("Ll", StringComparison.Ordinal)) malformed = true;
        if (lower is not ("" or "u" or "l" or "ul" or "lu" or "ll" or "ull" or "llu")) malformed = true;
        if (!haveDigit || malformed)
        {
            Fail("LEX010", "Malformed or unsupported integer literal suffix/digit sequence.", token.Range);
            return new ErrorExpression(types.Error, token.Range);
        }
        bool unsigned = lower.Contains('u');
        int longCount = lower.Count(c => c == 'l');
        IntegerKind[] candidates = (unsigned, longCount, numberBase == 10) switch
        {
            (false, 0, true) => [IntegerKind.Int, IntegerKind.Long, IntegerKind.LongLong],
            (false, 0, false) => [IntegerKind.Int, IntegerKind.UnsignedInt, IntegerKind.Long, IntegerKind.UnsignedLong, IntegerKind.LongLong, IntegerKind.UnsignedLongLong],
            (true, 0, _) => [IntegerKind.UnsignedInt, IntegerKind.UnsignedLong, IntegerKind.UnsignedLongLong],
            (false, 1, true) => [IntegerKind.Long, IntegerKind.LongLong],
            (false, 1, false) => [IntegerKind.Long, IntegerKind.UnsignedLong, IntegerKind.LongLong, IntegerKind.UnsignedLongLong],
            (true, 1, _) => [IntegerKind.UnsignedLong, IntegerKind.UnsignedLongLong],
            (false, 2, true) => [IntegerKind.LongLong],
            (false, 2, false) => [IntegerKind.LongLong, IntegerKind.UnsignedLongLong],
            (true, 2, _) => [IntegerKind.UnsignedLongLong],
            _ => throw new InvalidOperationException("Validated integer suffix.")
        };
        foreach (IntegerKind kind in candidates)
        {
            IntegerType type = types.Integer(kind);
            if (value <= type.Maximum) return new LiteralExpression(type, token.Range, value);
        }
        Unsupported("Integer literal requires an extended integer type beyond the implemented LP64 types.", token.Range);
        return new ErrorExpression(types.Error, token.Range);
    }
    private static int DigitValue(char ch) => ch is >= '0' and <= '9' ? ch - '0' :
        ch is >= 'a' and <= 'f' ? ch - 'a' + 10 : ch is >= 'A' and <= 'F' ? ch - 'A' + 10 : -1;
}
