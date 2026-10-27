using System;
using System.Globalization;
using DotCC.Ir;

namespace DotCC.Backends;

internal sealed partial class CSharpBackend
{
    private readonly record struct AggregateNumber(CType.Prim Type, Int128 Integer, double Floating)
    {
        internal bool Truth => Type.Integer ? Integer != 0 : Floating != 0;
    }

    private static bool AggregatePrimitive(CType.Prim type) => (type.Name, type.Bytes, type.Integer) switch
    {
        ("_Bool" or "char" or "signed char" or "unsigned char" or "char8_t", 1, true) => true,
        ("short" or "unsigned short" or "char16_t" or "wchar_t", 2, true) => true,
        ("int" or "unsigned int" or "char32_t", 4, true) => true,
        ("long" or "unsigned long" or "long long" or "unsigned long long", 8, true) => true,
        ("float", 4, false) or ("double" or "long double", 8, false) => true,
        _ => false
    };

    private static CType.Prim? AggregateScalarType(CType type)
    {
        if (type.IsVolatile || type.IsAtomic) return null;
        if (type.Unqualified is CType.Enum enumeration) return AggregateScalarType(enumeration.Underlying);
        return type.Unqualified is CType.Prim primitive && AggregatePrimitive(primitive) ? primitive : null;
    }

    private static bool AggregateIntegerLiteral(LitInt literal, out Int128 number)
    {
        if (literal.Value is { } value) { number = value; return true; }
        string digits = literal.Digits;
        if (digits.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
            && ulong.TryParse(digits.AsSpan(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var hex))
        { number = hex; return true; }
        if (digits.StartsWith("0b", StringComparison.OrdinalIgnoreCase))
        {
            number = 0;
            if (digits.Length is <= 2 or > 66) return false;
            foreach (char bit in digits.AsSpan(2))
            {
                if (bit is not ('0' or '1')) return false;
                number = number * 2 + (bit - '0');
            }
            return true;
        }
        return Int128.TryParse(digits, NumberStyles.Integer, CultureInfo.InvariantCulture, out number);
    }

    private bool TryAggregateScalarBits(CExpr expression, CType target, out ulong bits)
    {
        bits = 0;
        if (AggregateScalarType(target) is not { } type
            || AggregateConstant(expression, 0) is not { } value
            || ConvertAggregateNumber(value, type) is not { } converted) return false;
        bits = type.Integer ? unchecked((ulong)converted.Integer)
            : type.Bytes == 4 ? BitConverter.SingleToUInt32Bits((float)converted.Floating)
            : BitConverter.DoubleToUInt64Bits(converted.Floating);
        return true;
    }

    private static AggregateNumber? ConvertAggregateNumber(AggregateNumber value, CType.Prim type)
    {
        if (type.Name == "_Bool") return new(type, value.Truth ? 1 : 0, 0);
        if (!type.Integer)
        {
            // Convert integer directly to float, avoiding double rounding.
            double number = type.Bytes == 4
                ? value.Type.Integer ? (float)value.Integer : (float)value.Floating
                : value.Type.Integer ? (double)value.Integer : value.Floating;
            return double.IsFinite(number) ? new(type, 0, number) : null;
        }
        Int128 integer;
        if (value.Type.Integer) integer = value.Integer;
        else
        {
            double truncated = Math.Truncate(value.Floating);
            // Check before converting to the interpreter's carrier. C's
            // out-of-range floating-to-integer conversion is not a byte value.
            if (!double.IsFinite(truncated) || truncated < -18446744073709551616.0
                || truncated >= 18446744073709551616.0) return null;
            integer = (Int128)truncated;
            int width = type.Bytes * 8;
            Int128 min = type.Signed ? -(Int128.One << (width - 1)) : 0;
            Int128 max = (Int128.One << (type.Signed ? width - 1 : width)) - 1;
            if (integer < min || integer > max) return null;
        }
        var modulus = Int128.One << (type.Bytes * 8);
        integer &= modulus - 1;
        if (type.Signed && integer >= (modulus >> 1)) integer -= modulus;
        return new(type, integer, 0);
    }

    // Unlike general comptime evaluation, this evaluator converts at EVERY
    // typed C operation. No text-spliced floating values, wide integer
    // intermediates carried into a later cast, variable reads, or calls.
    private AggregateNumber? AggregateConstant(CExpr expression, int depth)
    {
        if (depth > 128 || AggregateScalarType(expression.Type) is not { } type) return null;
        AggregateNumber? result;
        switch (expression)
        {
            case LitInt literal when AggregateIntegerLiteral(literal, out var integer):
                result = new AggregateNumber((CType.Prim)CType.LongLong, integer, 0);
                break;
            case LitBool literal:
                result = new AggregateNumber((CType.Prim)CType.Bool, literal.Value ? 1 : 0, 0);
                break;
            case LitFloat literal:
                string text = literal.Text.TrimEnd('f', 'F');
                if (type.Bytes == 4)
                    result = float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var single)
                        && float.IsFinite(single) ? new AggregateNumber(type, 0, single) : null;
                else
                    result = double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var number)
                        && double.IsFinite(number) ? new AggregateNumber(type, 0, number) : null;
                break;
            case EnumConstRef reference:
                result = new AggregateNumber((CType.Prim)CType.LongLong, reference.Sym.ConstValue, 0);
                break;
            // The legacy constexpr cache uses wide intermediate arithmetic.
            // Until it carries a C-width proof, read the actual object through
            // the existing initializer path instead of serializing its cache.
            case VarRef:
                return null;
            case SizeOfExpr or OffsetOf:
                // Only these layout facts use the general folder. Arithmetic
                // parents always return to this typed evaluator.
                result = _foldConstant(expression) is { } folded && PayloadInteger(folded, out var size)
                    ? new AggregateNumber(type, size, 0) : null;
                break;
            case DefaultLit:
                result = new AggregateNumber(type, 0, 0);
                break;
            case Paren paren:
                result = AggregateConstant(paren.Inner, depth + 1);
                break;
            case Cast cast:
                result = AggregateConstant(cast.Operand, depth + 1);
                break;
            case CondExpr conditional:
                if (AggregateConstant(conditional.Cond, depth + 1) is not { } condition) return null;
                result = AggregateConstant(condition.Truth ? conditional.Then : conditional.Else, depth + 1);
                break;
            case Unary unary:
                result = AggregateUnary(unary, depth + 1);
                break;
            case Binary binary:
                result = AggregateBinary(binary, depth + 1);
                break;
            default:
                return null;
        }
        return result is { } value ? ConvertAggregateNumber(value, type) : null;
    }

    private AggregateNumber? AggregateUnary(Unary expression, int depth)
    {
        if (AggregateConstant(expression.Operand, depth) is not { } value) return null;
        if (expression.Op == UnOp.LogNot)
            return new AggregateNumber((CType.Prim)CType.Int, value.Truth ? 0 : 1, 0);
        if (expression.Op is not (UnOp.Plus or UnOp.Neg or UnOp.BitNot)) return null;
        if (AggregateScalarType(expression.Type) is not { } type
            || ConvertAggregateNumber(value, type) is not { } operand) return null;
        if (expression.Op == UnOp.Plus) return operand;
        if (!type.Integer)
            return expression.Op == UnOp.Neg ? new AggregateNumber(type, 0, -operand.Floating) : null;
        if (expression.Op == UnOp.Neg)
        {
            if (type.Signed && operand.Integer == -(Int128.One << (type.Bytes * 8 - 1))) return null;
            return new AggregateNumber(type, -operand.Integer, 0);
        }
        return new AggregateNumber(type, ~operand.Integer, 0);
    }

    private AggregateNumber? AggregateBinary(Binary expression, int depth)
    {
        if (AggregateConstant(expression.Left, depth) is not { } left) return null;
        if (expression.Op == BinOp.LogAnd && !left.Truth || expression.Op == BinOp.LogOr && left.Truth)
            return new AggregateNumber((CType.Prim)CType.Int, left.Truth ? 1 : 0, 0);
        if (AggregateConstant(expression.Right, depth) is not { } right) return null;
        if (expression.Op is BinOp.LogAnd or BinOp.LogOr)
            return new AggregateNumber((CType.Prim)CType.Int, right.Truth ? 1 : 0, 0);
        if (expression.Op is BinOp.Shl or BinOp.Shr)
        {
            // C promotes each shift operand independently, not to a common type.
            if (AggregateScalarType(CType.IntegerPromote(left.Type)) is not { Integer: true } promoted
                || ConvertAggregateNumber(left, promoted) is not { } operand || !right.Type.Integer
                || right.Integer < 0 || right.Integer >= promoted.Bytes * 8) return null;
            int shift = (int)right.Integer;
            Int128 shifted;
            if (expression.Op == BinOp.Shl)
            {
                if (promoted.Signed && (operand.Integer < 0
                    || operand.Integer > (((Int128.One << (promoted.Bytes * 8 - 1)) - 1) >> shift))) return null;
                shifted = operand.Integer << shift;
            }
            else shifted = operand.Integer >> shift;
            return ConvertAggregateNumber(new(promoted, shifted, 0), promoted);
        }
        if (AggregateScalarType(CType.UsualArithmetic(left.Type, right.Type)) is not { } common
            || ConvertAggregateNumber(left, common) is not { } a
            || ConvertAggregateNumber(right, common) is not { } b) return null;
        if (expression.Op is BinOp.Lt or BinOp.Gt or BinOp.Le or BinOp.Ge or BinOp.Eq or BinOp.Ne)
        {
            int comparison = common.Integer ? a.Integer.CompareTo(b.Integer) : a.Floating.CompareTo(b.Floating);
            bool truth = expression.Op switch
            {
                BinOp.Lt => comparison < 0, BinOp.Gt => comparison > 0,
                BinOp.Le => comparison <= 0, BinOp.Ge => comparison >= 0,
                BinOp.Eq => comparison == 0, _ => comparison != 0
            };
            return new AggregateNumber((CType.Prim)CType.Int, truth ? 1 : 0, 0);
        }
        if (!common.Integer)
        {
            // Float arithmetic must round in float itself (especially division),
            // rather than computing in double and rounding a second time.
            double number = common.Bytes == 4 ? expression.Op switch
            {
                BinOp.Add => (float)a.Floating + (float)b.Floating,
                BinOp.Sub => (float)a.Floating - (float)b.Floating,
                BinOp.Mul => (float)a.Floating * (float)b.Floating,
                BinOp.Div when b.Floating != 0 => (float)a.Floating / (float)b.Floating,
                _ => double.NaN
            } : expression.Op switch
            {
                BinOp.Add => a.Floating + b.Floating, BinOp.Sub => a.Floating - b.Floating,
                BinOp.Mul => a.Floating * b.Floating,
                BinOp.Div when b.Floating != 0 => a.Floating / b.Floating,
                _ => double.NaN
            };
            return double.IsFinite(number) ? ConvertAggregateNumber(new(common, 0, number), common) : null;
        }
        Int128 x = a.Integer, y = b.Integer;
        if (expression.Op is BinOp.Div or BinOp.Mod && (y == 0
            || common.Signed && x == -(Int128.One << (common.Bytes * 8 - 1)) && y == -1)) return null;
        Int128 result;
        switch (expression.Op)
        {
            case BinOp.Add: result = unchecked(x + y); break;
            case BinOp.Sub: result = unchecked(x - y); break;
            case BinOp.Mul: result = unchecked(x * y); break;
            case BinOp.Div: result = x / y; break;
            case BinOp.Mod: result = x % y; break;
            case BinOp.BitAnd: result = x & y; break;
            case BinOp.BitOr: result = x | y; break;
            case BinOp.BitXor: result = x ^ y; break;
            default: return null;
        }
        if (common.Signed && (result < -(Int128.One << (common.Bytes * 8 - 1))
            || result >= (Int128.One << (common.Bytes * 8 - 1)))) return null;
        return ConvertAggregateNumber(new(common, result, 0), common);
    }
}
