// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
// Handwritten EDG literals.c conv_float_literal, exprutil.c arithmetic
// conversions and interpret.c typed scalar operation dispatch adaptation.
using System.Numerics;

namespace EdgSharp;

public enum FloatingKind { Float, Double }
public enum FloatingClassification { Zero, Subnormal, Normal, Infinity, NaN }

/// <summary>A target C++ type, independent of the CLR numerical model.</summary>
public sealed class FloatingType : SemanticType
{
    public FloatingKind FloatingKind { get; }
    public int Width => Format.StorageBits;
    public int Precision => Format.Precision;
    internal TargetFloatFormat Format { get; }
    internal FloatingType(FloatingKind kind, TypeQualifiers qualifiers)
        : base(TypeKind.Floating, (qualifiers.HasFlag(TypeQualifiers.Const) ? "const " : "") +
            (kind == FloatingKind.Float ? "float" : "double"), qualifiers)
        => (FloatingKind, Format) = (kind, kind == FloatingKind.Float ? TargetFloatFormat.Binary32 : TargetFloatFormat.Binary64);
}

/// <summary>Immutable target payload. It never represents bits as an integer constant.</summary>
public sealed class FloatingConstantValue
{
    public FloatingKind Kind { get; }
    public ulong Bits => Numeric.Bits;
    public int Width => Numeric.Format.StorageBits;
    public bool IsNegative => Numeric.IsNegative;
    public FloatingClassification Classification => (FloatingClassification)Numeric.Class;
    internal TargetFloatValue Numeric { get; }
    internal FloatingConstantValue(TargetFloatValue value)
        => (Kind, Numeric) = (ReferenceEquals(value.Format, TargetFloatFormat.Binary32) ? FloatingKind.Float : FloatingKind.Double, value);
}

public sealed class FloatingLiteralExpression : Expression
{
    public FloatingConstantValue Value { get; }
    public bool IsExact { get; }
    public FloatingEvaluationCondition Conditions { get; }
    internal FloatingLiteralExpression(FloatingType type, Token token, TargetFloatValue value, bool exact, FloatingFlags flags)
        : base(type, ValueCategory.PRValue, token.Range) => (Value, IsExact, Conditions) = (new(value), exact, (FloatingEvaluationCondition)flags);
}

internal sealed partial class TypeTable
{
    private readonly Dictionary<(FloatingKind, TypeQualifiers), FloatingType> floatingTypes = [];
    internal FloatingType Floating(FloatingKind kind, TypeQualifiers qualifiers = 0)
    {
        var key = (kind, qualifiers);
        if (!floatingTypes.TryGetValue(key, out FloatingType? type)) floatingTypes.Add(key, type = new(kind, qualifiers));
        return type;
    }
    internal static bool IsArithmetic(SemanticType type) => type is IntegerType or FloatingType;
    internal SemanticType ArithmeticType(SemanticType left, SemanticType right)
    {
        if (left is FloatingType || right is FloatingType)
        {
            FloatingKind kind = left is FloatingType { FloatingKind: FloatingKind.Double } || right is FloatingType { FloatingKind: FloatingKind.Double }
                ? FloatingKind.Double : FloatingKind.Float;
            return Floating(kind);
        }
        return UsualArithmeticConversions((IntegerType)left, (IntegerType)right);
    }
}

internal sealed partial class Parser
{
    private StandardConversionRank ArithmeticConversionRank(SemanticType source, SemanticType target)
    {
        if (source is IntegerType si && target is IntegerType ti) return IntegerConversionRank(si, ti);
        if (source is FloatingType sf && target is FloatingType tf)
            return sf.FloatingKind == tf.FloatingKind ? StandardConversionRank.ExactMatch :
                sf.FloatingKind == FloatingKind.Float && tf.FloatingKind == FloatingKind.Double ? StandardConversionRank.Promotion : StandardConversionRank.Conversion;
        return StandardConversionRank.Conversion;
    }

    private Expression FloatingLiteral(Token token)
    {
        FloatingLiteralResult decoded = FloatingLiterals.Decode(token.Spelling, FloatingLiteralProfile.Binary32Binary64, FloatingPolicy());
        if (decoded.Status != FloatingDecodeStatus.Decoded)
        {
            DiagnosticClass classification = decoded.Status == FloatingDecodeStatus.ResourceLimit ? DiagnosticClass.ResourceLimit :
                decoded.Status == FloatingDecodeStatus.ValidUnsupported ? DiagnosticClass.UnsupportedFeature : DiagnosticClass.InvalidSource;
            state.Diagnostics.Add(decoded.Status == FloatingDecodeStatus.Malformed ? "LEX011" : "FLT001", $"Floating literal: {decoded.Issue}.", token.Range, classification);
            return new ErrorExpression(types.Error, token.Range);
        }
        if ((decoded.Numeric.Flags & FloatingFlags.Overflow) != 0 ||
            !decoded.IsHexadecimal && (decoded.Numeric.Flags & FloatingFlags.Underflow) != 0 && decoded.Value!.IsZero)
        {
            Fail("FLT002", "Floating literal is outside the target format's range.", token.Range);
            return new ErrorExpression(types.Error, token.Range);
        }
        if (decoded.IsHexadecimal && (decoded.Numeric.Flags & FloatingFlags.Inexact) != 0)
            state.Diagnostics.Add("FLT003", "Hexadecimal floating literal is rounded to its target format.", token.Range, severity: DiagnosticSeverity.Warning);
        FloatingType type = types.Floating(decoded.Suffix == FloatingLiteralSuffix.Float ? FloatingKind.Float : FloatingKind.Double);
        return new FloatingLiteralExpression(type, token, decoded.Value!, decoded.IsExact, decoded.Numeric.Flags);
    }

    private FloatingLimits FloatingPolicy() => new(maxIntermediateBits: options.FloatingIntermediateBitLimit);

    private Expression ConvertArithmetic(Expression expression, SemanticType target)
    {
        SemanticType source = expression.Type;
        if (expression.ValueCategory != ValueCategory.PRValue)
        {
            source = types.Unqualified(source);
            expression = new ConversionExpression(expression, source, ConversionKind.LValueToRValue, expression.ConstantValue);
        }
        target = types.Unqualified(target);
        if (TypeTable.Equivalent(source, target)) return expression;
        ConversionKind kind = target is IntegerType { IntegerKind: IntegerKind.Bool } ? ConversionKind.BooleanConversion :
            source is FloatingType && target is FloatingType ?
                ArithmeticConversionRank(source, target) == StandardConversionRank.Promotion ? ConversionKind.FloatingPromotion : ConversionKind.FloatingConversion :
            ConversionKind.FloatingIntegralConversion;
        return new ConversionExpression(expression, target, kind, null);
    }

    private Expression FloatingBinary(Token op, Expression left, Expression right, SourceRange range)
    {
        if (!TypeTable.IsArithmetic(left.Type) || !TypeTable.IsArithmetic(right.Type))
        { Fail("SEM025", "Arithmetic operator requires arithmetic operands.", op.Range); return new ErrorExpression(types.Error, range); }
        bool comparison = op.Kind is TokenKind.EqualEqual or TokenKind.NotEqual or TokenKind.Less or TokenKind.Greater or TokenKind.LessEqual or TokenKind.GreaterEqual;
        if (!comparison && op.Kind is not (TokenKind.Plus or TokenKind.Minus or TokenKind.Star or TokenKind.Slash))
        { Fail("FLT004", "This operator requires integer operands.", op.Range); return new ErrorExpression(types.Error, range); }
        SemanticType operation = types.ArithmeticType(left.Type, right.Type);
        var result = new BinaryExpression(op.Kind, Convert(left, operation), Convert(right, operation),
            comparison ? types.Integer(IntegerKind.Bool) : operation, range, null);
        DiagnoseFloatingOperation(result);
        return result;
    }

    private readonly record struct NumericalFoldFact(ConstantEvaluationStatus Status, ConstantRuntimeValue Value, string Message);
    private readonly Dictionary<Expression, NumericalFoldFact?> numericFoldFacts = [];
    private bool numericFoldSaturated;

    private NumericalFoldFact? OptionalArithmeticConstant(Expression expression)
    {
        // Only immutable closed numeric syntax enters this TU-owned memo. No
        // name/reference/call/store is followed, no body is instantiated, and
        // no side-effect/dependency query walks a growing expression prefix.
        // Each operation interprets only its already-folded child VALUES.
        if (unevaluatedParsing || numericFoldSaturated) return null;
        var pending = new Stack<(Expression Expression, bool Finish)>();
        pending.Push((expression, false));
        int inspected = 0;
        while (pending.TryPop(out var item))
        {
            Expression current = item.Expression;
            if (numericFoldFacts.ContainsKey(current)) continue;
            if (++inspected > options.ConstantEvaluationStepLimit || numericFoldFacts.Count >= options.ConstantEvaluationStepLimit)
            {
                numericFoldSaturated = true; numericFoldFacts.Clear(); return null;
            }
            if (!TypeTable.IsArithmetic(current.Type)) { numericFoldFacts[current] = null; continue; }
            if (current is FloatingLiteralExpression literal)
            { numericFoldFacts[current] = new(ConstantEvaluationStatus.Success, ConstantRuntimeValue.Float(literal.Value.Numeric), ""); continue; }
            if (current.ConstantValue is BigInteger integer && current.ValueCategory == ValueCategory.PRValue)
            { numericFoldFacts[current] = new(ConstantEvaluationStatus.Success, ConstantRuntimeValue.Scalar(integer), ""); continue; }
            Expression[] children = current switch
            {
                ParenthesizedExpression p => [p.Operand], ConversionExpression c => [c.Operand],
                UnaryExpression u => [u.Operand], BinaryExpression b => [b.Left,b.Right], _ => []
            };
            if (children.Length == 0) { numericFoldFacts[current] = null; continue; }
            if (!item.Finish)
            {
                pending.Push((current,true));
                for (int index=children.Length-1;index>=0;--index)
                    if (!numericFoldFacts.ContainsKey(children[index])) pending.Push((children[index],false));
                continue;
            }
            if (children.Any(child => !numericFoldFacts.TryGetValue(child,out NumericalFoldFact? fact) || fact?.Status != ConstantEvaluationStatus.Success))
            { numericFoldFacts[current] = null; continue; }
            Expression Leaf(int index)
            {
                NumericalFoldFact fact = numericFoldFacts[children[index]] ?? throw new InvalidOperationException("Missing completed numeric child fact.");
                return new NumericalFoldLeaf(children[index].Type, children[index].Range, fact.Value);
            }
            Expression operation = current switch
            {
                ParenthesizedExpression => Leaf(0),
                ConversionExpression c => new ConversionExpression(Leaf(0),c.Type,c.Conversion,null),
                UnaryExpression u => new UnaryExpression(u.Operator,Leaf(0),u.Type,u.Range,null),
                BinaryExpression binary => new BinaryExpression(binary.Operator,Leaf(0),Leaf(1),binary.Type,binary.Range,null),
                _ => throw new InvalidOperationException("Unexpected closed numeric node.")
            };
            ConstantEvaluationResult result = new ConstantInterpreter(options).Evaluate(operation);
            ConstantRuntimeValue value = result.FloatingValue is { } floating ? ConstantRuntimeValue.Float(floating.Numeric) :
                result.Value is BigInteger scalar ? ConstantRuntimeValue.Scalar(scalar) : default;
            numericFoldFacts[current] = new(result.Status,value,result.Message);
        }
        return numericFoldFacts.GetValueOrDefault(expression);
    }

    private static bool IsNumericZero(NumericalFoldFact value)
        => value.Value.Floating?.IsZero == true || value.Value.Integer == BigInteger.Zero;

    private void DiagnoseFloatingOperation(BinaryExpression expression)
    {
        if (unevaluatedParsing) return;
        NumericalFoldFact? left = OptionalArithmeticConstant(expression.Left);
        NumericalFoldFact? right = OptionalArithmeticConstant(expression.Right);
        if (expression.Operator == TokenKind.Slash && left?.Status != ConstantEvaluationStatus.Success && right is { Status: ConstantEvaluationStatus.Success } divisor && IsNumericZero(divisor))
            state.Diagnostics.Add("FLT005", "Floating division by a known zero divisor.", expression.Right.Range, severity: DiagnosticSeverity.Warning);
        NumericalFoldFact? folded = OptionalArithmeticConstant(expression);
        if (folded is { Status: ConstantEvaluationStatus.UndefinedBehavior } invalid)
            state.Diagnostics.Add("FLT006", "Floating constant operation is outside its supported range: " + invalid.Message, expression.Range, severity: DiagnosticSeverity.Warning);
    }

    private void DiagnoseFloatingCompoundZero(Expression divisor)
    {
        NumericalFoldFact? value = OptionalArithmeticConstant(divisor);
        if (value is { Status: ConstantEvaluationStatus.Success } divisorValue && IsNumericZero(divisorValue))
            state.Diagnostics.Add("FLT005", "Floating division by a known zero divisor.", divisor.Range, severity: DiagnosticSeverity.Warning);
    }
}
