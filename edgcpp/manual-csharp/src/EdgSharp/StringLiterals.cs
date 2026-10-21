// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
// Handwritten literals.c conv_string_literal/concat_string_literals and
// lexical.c concat_adjacent_string_literals; il.c string_literal_type.
using System.Collections.ObjectModel;
using System.Numerics;

namespace EdgSharp;

public sealed class ArrayType : SemanticType
{
    public SemanticType ElementType { get; }
    public int Length { get; }
    internal ArrayType(SemanticType element, int length)
        : base(TypeKind.Array, $"{element}[{length}]", element.Qualifiers) => (ElementType, Length) = (element, length);
}
public sealed class PointerType : SemanticType
{
    public SemanticType PointedToType { get; }
    internal PointerType(SemanticType element, TypeQualifiers qualifiers = 0)
        : base(TypeKind.Pointer, element + " *" + (qualifiers.HasFlag(TypeQualifiers.Const) ? " const" : ""), qualifiers) => PointedToType = element;
}
internal sealed partial class TypeTable
{
    private readonly Dictionary<(SemanticType, int), ArrayType> arrays = [];
    private readonly Dictionary<(SemanticType, TypeQualifiers), PointerType> pointers = [];
    internal ArrayType Array(SemanticType element, int length)
    {
        if (length < 0) throw new ArgumentOutOfRangeException(nameof(length));
        if (length > 80 || RequiresOwnerLifetime(element)) return new(element, length);
        var key = (element, length);
        if (!arrays.TryGetValue(key, out ArrayType? value)) arrays.Add(key, value = new(element, length));
        return value;
    }
    internal PointerType Pointer(SemanticType element, TypeQualifiers qualifiers = 0)
    {
        if (RequiresOwnerLifetime(element)) return new(element, qualifiers);
        var key = (element, qualifiers);
        if (!pointers.TryGetValue(key, out PointerType? value)) pointers.Add(key, value = new(element, qualifiers));
        return value;
    }
    internal static bool RequiresOwnerLifetime(SemanticType type) => ContainsScopedOrDependentType(type) || (type switch
    {
        ArrayType array => array.Length > 80 || RequiresOwnerLifetime(array.ElementType),
        PointerType pointer => RequiresOwnerLifetime(pointer.PointedToType),
        ReferenceType reference => RequiresOwnerLifetime(reference.ReferredType),
        FunctionType function => RequiresOwnerLifetime(function.ReturnType) || function.ParameterTypes.Any(RequiresOwnerLifetime),
        _ => false
    });
    internal static bool Equivalent(SemanticType first, SemanticType second)
    {
        if (ReferenceEquals(first, second)) return true;
        if (first.Qualifiers != second.Qualifiers) return false;
        return (first, second) switch
        {
            (ArrayType left, ArrayType right) => left.Length == right.Length && Equivalent(left.ElementType, right.ElementType),
            (PointerType left, PointerType right) => Equivalent(left.PointedToType, right.PointedToType),
            (ReferenceType left, ReferenceType right) => left.ReferenceKind == right.ReferenceKind && Equivalent(left.ReferredType, right.ReferredType),
            (FunctionType left, FunctionType right) => Equivalent(left.ReturnType, right.ReturnType) && left.ParameterTypes.Count == right.ParameterTypes.Count && left.ParameterTypes.Zip(right.ParameterTypes).All(pair => Equivalent(pair.First, pair.Second)),
            _ => false
        };
    }
}

/// <summary>One authored string token and its decoded units before adjacent concatenation.</summary>
public sealed class StringLiteralPart
{
    public Token Token { get; }
    public LiteralEncoding Encoding { get; }
    public bool IsRaw { get; }
    public ReadOnlyCollection<uint> CodeUnits { get; }
    internal bool HasWideOrdinaryEntity { get; }
    internal bool HasOrdinaryRangeOverflow { get; }
    internal StringLiteralPart(Token token, LiteralEncoding encoding, bool raw, uint[] units, bool wide, bool overflow)
        => (Token, Encoding, IsRaw, CodeUnits, HasWideOrdinaryEntity, HasOrdinaryRangeOverflow) = (token, encoding, raw, System.Array.AsReadOnly(units), wide, overflow);
}

/// <summary>A const-element array lvalue; CodeUnits includes exactly one terminating zero.</summary>
public sealed class StringLiteralExpression : Expression
{
    public new ArrayType Type => (ArrayType)base.Type;
    public LiteralEncoding Encoding { get; }
    public ReadOnlyCollection<uint> CodeUnits { get; }
    public ReadOnlyCollection<StringLiteralPart> Parts { get; }
    internal StringLiteralExpression(ArrayType type, LiteralEncoding encoding, uint[] units, StringLiteralPart[] parts)
        : base(type, ValueCategory.LValue, SourceRange.Cover(parts[0].Token.Range, parts[^1].Token.Range))
        => (Encoding, CodeUnits, Parts) = (encoding, System.Array.AsReadOnly(units), System.Array.AsReadOnly(parts));
}
public sealed class SizeOfExpression : Expression
{
    public Expression? Operand { get; }
    public SemanticType OperandType { get; }
    public BigInteger ByteSize { get; }
    internal SizeOfExpression(Expression? operand, SemanticType type, IntegerType result, BigInteger size, SourceRange range)
        : base(result, ValueCategory.PRValue, range, size) => (Operand, OperandType, ByteSize) = (operand, type, size);
}
public sealed class SubscriptExpression : Expression
{
    public Expression ArrayOrPointer { get; }
    public Expression Index { get; }
    public bool PointerOperandIsSecond { get; }
    public Expression SourceLeft => PointerOperandIsSecond ? Index : ArrayOrPointer;
    public Expression SourceRight => PointerOperandIsSecond ? ArrayOrPointer : Index;
    public override bool IsValueDependent => ScalarExpressionDependencies.IsValueDependent(this);
    internal SubscriptExpression(Expression array, Expression index, SemanticType element, SourceRange range, bool pointerSecond)
        : base(element, ValueCategory.LValue, range) => (ArrayOrPointer, Index, PointerOperandIsSecond) = (array, index, pointerSecond);
}
public sealed class DependentSizeOfExpression : Expression
{
    public SemanticType OperandType { get; }
    public override bool IsValueDependent => true;
    internal DependentSizeOfExpression(SemanticType operandType, IntegerType result, SourceRange range)
        : base(result, ValueCategory.PRValue, range) => OperandType = operandType;
}
public sealed class PointerArithmeticExpression : Expression
{
    public Expression Pointer { get; }
    public Expression Offset { get; }
    public TokenKind Operator { get; }
    public override bool IsValueDependent => ScalarExpressionDependencies.IsValueDependent(this);
    internal PointerArithmeticExpression(Expression pointer, Expression offset, TokenKind op, PointerType type, SourceRange range)
        : base(type, ValueCategory.PRValue, range) => (Pointer, Offset, Operator) = (pointer, offset, op);
}

internal static class StringLiteralDecoder
{
    internal static StringLiteralPart? Decode(Token token, TargetInfo target, DiagnosticBag diagnostics, int limit)
    {
        string text = token.Spelling;
        int quote = text.IndexOf('"');
        string prefix = quote < 0 ? text : text[..quote];
        bool raw = prefix.EndsWith('R');
        if (raw) prefix = prefix[..^1];
        LiteralEncoding encoding = prefix switch
        {
            "" => LiteralEncoding.Ordinary, "u8" => LiteralEncoding.Utf8, "u" => LiteralEncoding.Utf16,
            "U" => LiteralEncoding.Utf32, "L" => LiteralEncoding.Wide, _ => (LiteralEncoding)(-1)
        };
        if (quote < 0 || (int)encoding < 0) { Error("Malformed string encoding prefix."); return null; }
        int start = quote + 1, end;
        if (raw)
        {
            int open = text.IndexOf('(', start);
            if (open < 0) { Error("Malformed raw string delimiter."); return null; }
            string delimiter = text[start..open];
            string terminator = ")" + delimiter + "\"";
            end = text.IndexOf(terminator, open + 1, StringComparison.Ordinal);
            if (end < 0) { Error("Unterminated raw string."); return null; }
            if (end + terminator.Length != text.Length) { Unsupported("User-defined string literal suffixes are not implemented."); return null; }
            start = open + 1;
        }
        else
        {
            end = start;
            while (end < text.Length)
            {
                if (text[end] == '\\') { end += 2; continue; }
                if (text[end] == '"') break;
                ++end;
            }
            if (end >= text.Length) { Error("Unterminated string literal."); return null; }
            if (end + 1 != text.Length) { Unsupported("User-defined string literal suffixes are not implemented."); return null; }
        }
        int bits = encoding switch { LiteralEncoding.Utf16 => target.Char16Bits, LiteralEncoding.Utf32 => target.Char32Bits, LiteralEncoding.Wide => target.WCharBits, _ => target.CharBits };
        BigInteger mask = (BigInteger.One << bits) - 1;
        var units = new List<uint>();
        bool wide = false, overflow = false;
        Span<uint> utf8 = stackalloc uint[4];
        for (int position = start; position < end;)
        {
            BigInteger value;
            bool scalar = false;
            if (!raw && text[position] == '\\')
            {
                int escapeStart = position++;
                if (position == end) { Error("Incomplete string escape."); return null; }
                char c = text[position++];
                const string escapes = "'\"?\\abfnrtv";
                uint[] simple = [39, 34, 63, 92, 7, 8, 12, 10, 13, 9, 11];
                int simpleIndex = escapes.IndexOf(c);
                if (simpleIndex >= 0) value = simple[simpleIndex];
                else if (c is 'u' or 'U')
                {
                    if (!UnicodeIdentifiers.ReadUcn(text, escapeStart, out int code, out int length, out UnicodeIdentifiers.Problem? issue))
                    {
                        diagnostics.Add(issue!.Value.Code, issue.Value.Message, token.Range,
                            issue.Value.Unsupported ? DiagnosticClass.UnsupportedFeature : DiagnosticClass.InvalidSource); return null;
                    }
                    position = escapeStart + length; value = code; scalar = true;
                }
                else if (c == 'N' && position < end && text[position] == '{')
                { Unsupported("Named Unicode escapes require an unimplemented language profile."); return null; }
                else if (c == 'x' || c is >= '0' and <= '7')
                {
                    bool hex = c == 'x'; int count = 0; value = 0;
                    if (!hex) { --position; }
                    while (position < end && (hex || count < 3))
                    {
                        int digit = UnicodeIdentifiers.Hex(text[position]);
                        if (digit < 0 || !hex && digit > 7) break;
                        value = value * (hex ? 16 : 8) + digit; ++position; ++count;
                        if (value > mask)
                        {
                            if (encoding != LiteralEncoding.Ordinary) { Error("String escape is outside the target code-unit range."); return null; }
                            overflow = true; value &= mask;
                        }
                    }
                    if (count == 0) { Error("A hexadecimal string escape requires a digit."); return null; }
                }
                else
                {
                    if (c > 127) { Unsupported("Unknown non-ASCII escaped characters are not implemented."); return null; }
                    value = c;
                    diagnostics.Add("LEX021", "Unrecognized escape sequence.", token.Range, severity: DiagnosticSeverity.Warning);
                }
            }
            else
            {
                if (raw && text[position] == '\r' && position + 1 < end && text[position + 1] == '\n') ++position;
                if (!UnicodeIdentifiers.ReadScalar(text, position, out int code, out int length))
                { Error("String literal contains an unpaired surrogate."); return null; }
                value = code; scalar = true; position += length;
            }
            if (scalar && encoding is LiteralEncoding.Ordinary or LiteralEncoding.Utf8)
            {
                int count = UnicodeIdentifiers.EncodeUtf8((int)value, utf8);
                wide |= count > 1;
                for (int i = 0; i < count; ++i) units.Add(utf8[i]);
            }
            else if (scalar && encoding == LiteralEncoding.Utf16 && value > 0xffff)
            {
                int code = (int)value - 0x10000;
                units.Add((uint)(0xd800 | code >> 10)); units.Add((uint)(0xdc00 | code & 1023));
            }
            else
            {
                if (value < 0 || value > mask)
                {
                    if (encoding != LiteralEncoding.Ordinary) { Error("String escape is outside the target code-unit range."); return null; }
                    overflow = true; value &= mask;
                }
                units.Add((uint)value);
            }
            if (units.Count >= limit) { diagnostics.Add("RES010", "String literal code-unit limit exceeded.", token.Range, DiagnosticClass.ResourceLimit); return null; }
        }
        return new(token, encoding, raw, units.ToArray(), wide && !raw, overflow);
        void Error(string message) => diagnostics.Add("LEX040", message, token.Range);
        void Unsupported(string message) => diagnostics.Add("UNS040", message, token.Range, DiagnosticClass.UnsupportedFeature);
    }
    internal static uint[]? Concatenate(StringLiteralPart[] parts, LiteralEncoding encoding, DiagnosticBag diagnostics, int limit)
    {
        var units = new List<uint>();
        foreach (StringLiteralPart part in parts)
        {
            if (part.Encoding == encoding) units.AddRange(part.CodeUnits);
            else if (part.Encoding == LiteralEncoding.Ordinary)
            {
                // The source part is already decoded: do not recognize escapes
                // a second time. Rescan its UTF-8 bytes as target scalars.
                uint[] source = part.CodeUnits.ToArray();
                for (int i = 0; i < source.Length;)
                {
                    int initial = i;
                    uint first = source[i++]; int scalar, count;
                    if (first < 128) { scalar = (int)first; count = 0; }
                    else if (first is >= 0xc2 and <= 0xdf) { scalar = (int)(first & 31); count = 1; }
                    else if (first is >= 0xe0 and <= 0xef) { scalar = (int)(first & 15); count = 2; }
                    else if (first is >= 0xf0 and <= 0xf4) { scalar = (int)(first & 7); count = 3; }
                    else { scalar = (int)first; count = -1; }
                    int remaining = count;
                    bool malformed = count < 0;
                    while (remaining-- > 0)
                    {
                        if (i == source.Length || source[i] is < 0x80 or > 0xbf) { malformed = true; break; }
                        scalar = (scalar << 6) | (int)(source[i++] & 63);
                    }
                    if (!UnicodeIdentifiers.IsScalar(scalar) || count == 1 && scalar < 0x80 || count == 2 && scalar < 0x800 || count == 3 && scalar < 0x10000)
                        malformed = true;
                    if (malformed)
                    {
                        if (encoding == LiteralEncoding.Utf8)
                        {
                            diagnostics.Add("UNS041", "Invalid-byte ordinary-to-UTF8 concatenation is not qualified for the pinned native profile.", part.Token.Range, DiagnosticClass.UnsupportedFeature);
                            return null;
                        }
                        // conv_single_wide_char's is_rescan fallback: an octal
                        // or hex-produced byte need not be a valid UTF-8 entity.
                        scalar = (int)first; i = initial + 1;
                    }
                    if (encoding == LiteralEncoding.Utf8)
                    {
                        for (int k = i - count - 1; k < i; ++k) units.Add(source[k]);
                    }
                    else if (encoding == LiteralEncoding.Utf16 && scalar > 0xffff)
                    { int code = scalar - 0x10000; units.Add((uint)(0xd800 | code >> 10)); units.Add((uint)(0xdc00 | code & 1023)); }
                    else units.Add((uint)scalar);
                }
            }
            else { diagnostics.Add("LEX041", "Adjacent nonordinary string prefixes must agree.", part.Token.Range); return null; }
            if (units.Count >= limit) { diagnostics.Add("RES010", "String literal code-unit limit exceeded.", part.Token.Range, DiagnosticClass.ResourceLimit); return null; }
        }
        if (encoding == LiteralEncoding.Ordinary && parts.Any(part => part.HasWideOrdinaryEntity))
        { diagnostics.Add("LEX040", "An ordinary nonraw string entity is too wide for the strict execution character profile.", parts.First(part => part.HasWideOrdinaryEntity).Token.Range); return null; }
        if (encoding == LiteralEncoding.Ordinary && parts.Any(part => part.HasOrdinaryRangeOverflow))
        { diagnostics.Add("LEX040", "String escape is outside the target code-unit range.", parts.First(part => part.HasOrdinaryRangeOverflow).Token.Range); return null; }
        units.Add(0); return units.ToArray();
    }
}

internal sealed partial class Parser
{
    private Expression ScanStringLiteral()
    {
        var parts = new List<StringLiteralPart>();
        SourceRange first = Current.Range, last = first;
        bool valid = true;
        while (Is(TokenKind.StringLiteral))
        {
            Token token = Take(); last = token.Range;
            StringLiteralPart? part = StringLiteralDecoder.Decode(token, options.Target, state.Diagnostics, options.StringLiteralCodeUnitLimit);
            if (part == null) valid = false; else parts.Add(part);
        }
        if (!valid) return new ErrorExpression(types.Error, SourceRange.Cover(first, last));
        LiteralEncoding encoding = parts.Select(part => part.Encoding).FirstOrDefault(kind => kind != LiteralEncoding.Ordinary);
        uint[]? units = StringLiteralDecoder.Concatenate(parts.ToArray(), encoding, state.Diagnostics, options.StringLiteralCodeUnitLimit);
        if (units == null) return new ErrorExpression(types.Error, SourceRange.Cover(first, last));
        IntegerKind kind = encoding switch { LiteralEncoding.Utf8 => IntegerKind.Char8, LiteralEncoding.Utf16 => IntegerKind.Char16, LiteralEncoding.Utf32 => IntegerKind.Char32, LiteralEncoding.Wide => IntegerKind.WChar, _ => IntegerKind.Char };
        return new StringLiteralExpression(types.Array(types.Integer(kind, TypeQualifiers.Const), units.Length), encoding, units, parts.ToArray());
    }
}

internal sealed partial class Parser
{
    private bool unevaluatedParsing;
    private SemanticType ScanObjectDeclarator(SemanticType type)
    {
        if (!Eat(TokenKind.Star)) return ScanReferenceDeclarator(type);
        if (InTemplateScope() || type is not IntegerType)
            UnsupportedAndThrow("Only nondependent pointers to integer character/scalar types are implemented.");
        TypeQualifiers qualifiers = Eat("const") ? TypeQualifiers.Const : TypeQualifiers.None;
        if (Is(TokenKind.Star) || Is(TokenKind.Ampersand) || Is(TokenKind.AndAnd))
            UnsupportedAndThrow("Compound pointer/reference declarators are not implemented.");
        return types.Pointer(type, qualifiers);
    }
    private SemanticType ScanArrayDeclarator(SemanticType element)
    {
        if (!Eat(TokenKind.LeftBracket)) return element;
        if (element is not IntegerType || InTemplateScope() && activeTemplate == null)
            UnsupportedAndThrow("Only nondependent one-dimensional scalar arrays are implemented.");
        int length = 0;
        if (!Is(TokenKind.RightBracket))
        {
            Expression bound = ScanExprFull();
            if (prototypeAnalysis && bound.IsValueDependent)
                UnsupportedAndThrow("Dependent array bounds are not implemented.");
            if (bound.Type is not IntegerType integerBound)
            { Fail("ARR001", "An array bound requires an integer constant expression.", bound.Range); throw new ParseFailure(); }
            ConstantEvaluationResult value = new ConstantInterpreter(options).Evaluate(Convert(bound, types.Integer(integerBound.IntegerKind)));
            if (value.Status != ConstantEvaluationStatus.Success || value.Value is not BigInteger count || count <= 0)
            { Fail("ARR001", "An array bound must be a positive supported constant size.", bound.Range); throw new ParseFailure(); }
            if (count > int.MaxValue)
            { state.Diagnostics.Add("RES011", "This array bound exceeds the managed graph's current representation limit.", bound.Range, DiagnosticClass.ResourceLimit); throw new ParseFailure(); }
            length = (int)count;
        }
        Expect(TokenKind.RightBracket, "']' after array bound");
        if (Is(TokenKind.LeftBracket)) UnsupportedAndThrow("Multidimensional array declarators are not implemented.");
        return types.Array(element, length);
    }
    private Expression ScanSizeOf()
    {
        Token start = Take();
        bool saved = unevaluatedParsing;
        unevaluatedParsing = true;
        Expression? operand = null;
        SemanticType type;
        SourceRange end;
        try
        {
            if (Eat(TokenKind.LeftParen))
            {
                if (StartsDeclaration()) type = ScanArrayDeclarator(ScanObjectDeclarator(ScanType()));
                else { operand = ScanExprFull(); type = operand.Type; }
                end = Expect(TokenKind.RightParen, "')' after sizeof operand").Range;
            }
            else { operand = ScanExprFull(10); type = operand.Type; end = operand.Range; }
        }
        finally { unevaluatedParsing = saved; }
        BigInteger? size = ObjectSize(type);
        if (TypeTable.ContainsScopedOrDependentType(type))
            return new DependentSizeOfExpression(type, types.Integer(IntegerKind.UnsignedLong), SourceRange.Cover(start.Range, end));
        if (!size.HasValue)
        {
            if (type.Kind != TypeKind.Error) Fail("ARR002", "sizeof requires an implemented complete object type.", SourceRange.Cover(start.Range, end));
            return new ErrorExpression(types.Error, SourceRange.Cover(start.Range, end));
        }
        return new SizeOfExpression(operand, type, types.Integer(IntegerKind.UnsignedLong), size.Value, SourceRange.Cover(start.Range, end));
    }
    private BigInteger? ObjectSize(SemanticType type) => type switch
    {
        IntegerType integer => integer.Width / options.Target.CharBits,
        PointerType => options.Target.PointerBits / options.Target.CharBits,
        ReferenceType reference => ObjectSize(reference.ReferredType),
        ArrayType { Length: > 0 } array when ObjectSize(array.ElementType) is BigInteger element => element * array.Length,
        _ => null
    };
    private Expression ScanSubscript(Expression left)
    {
        Take();
        Expression right = ScanExprFull();
        Token end = Expect(TokenKind.RightBracket, "']' after subscript");
        SourceRange range = SourceRange.Cover(left.Range, end.Range);
        bool pointerSecond = left.Type is IntegerType && right.Type is ArrayType or PointerType;
        if (pointerSecond) (left, right) = (right, left);
        if (left.Type is ArrayType array)
            left = new ConversionExpression(left, types.Pointer(array.ElementType), ConversionKind.ArrayToPointer, null);
        if (left.Type is not PointerType pointer || pointer.PointedToType is not IntegerType || right.Type is not IntegerType integer)
        {
            if (prototypeAnalysis && (TypeDependent(left) || TypeDependent(right)))
            { Unsupported("Dependent subscripting is not implemented.", left.Range); return new ErrorExpression(types.Error, left.Range); }
            Fail("ARR003", "Subscripting requires a scalar pointer/array and an integer index.", left.Range);
            return new ErrorExpression(types.Error, SourceRange.Cover(left.Range, end.Range));
        }
        left = Convert(left, pointer);
        right = Convert(right, types.Integer(integer.IntegerKind));
        return new SubscriptExpression(left, right, pointer.PointedToType, range, pointerSecond);
    }
    private Expression ArrayInitializer(Expression expression, ArrayType array, VariableDeclaration declaration)
    {
        if (expression is not StringLiteralExpression literal || array.ElementType is not IntegerType target || literal.Type.ElementType is not IntegerType source)
        {
            Unsupported("Only string copy-initialization of scalar character arrays is implemented.", expression.Range);
            return new ErrorExpression(types.Error, expression.Range);
        }
        bool ordinary = source.IntegerKind == IntegerKind.Char && target.IntegerKind is IntegerKind.Char or IntegerKind.SignedChar or IntegerKind.UnsignedChar;
        bool utf8 = source.IntegerKind == IntegerKind.Char8 && target.IntegerKind is IntegerKind.Char8 or IntegerKind.Char or IntegerKind.UnsignedChar;
        if (!ordinary && !utf8 && source.IntegerKind != target.IntegerKind)
        { Fail("ARR004", "String encoding does not match the destination array element type.", expression.Range); return new ErrorExpression(types.Error, expression.Range); }
        if (array.Length == 0)
        {
            array = types.Array(array.ElementType, literal.CodeUnits.Count);
            state.SetVariableType(declaration, array);
        }
        if (array.Length < literal.CodeUnits.Count)
        { Fail("ARR005", "String initializer including its terminator is too long for the C++ array.", expression.Range); return new ErrorExpression(types.Error, expression.Range); }
        return new ArrayInitializationExpression(literal, array);
    }
}

public sealed class ArrayInitializationExpression : Expression
{
    public StringLiteralExpression Literal { get; }
    internal ArrayInitializationExpression(StringLiteralExpression literal, ArrayType type)
        : base(type, ValueCategory.PRValue, literal.Range) => Literal = literal;
}
