#nullable enable

namespace DotCC.Libc;

public static unsafe partial class Libc
{
    public const int FP_NAN = 0, FP_INFINITE = 1, FP_ZERO = 2, FP_SUBNORMAL = 3, FP_NORMAL = 4;

    // The compiler's documented long-double representation is IEEE binary64.
    public static double ceill(double value) => global::System.Math.Ceiling(value);

    /// <summary>Nearest/ties-even conversion under the runtime's fixed default
    /// rounding mode. Floating-point exception flags are not modeled.</summary>
    public static long llrint(double value)
        => RoundedLong(global::System.Math.Round(value, global::System.MidpointRounding.ToEven));

    /// <summary>Nearest integer, with half-way cases rounded away from zero.</summary>
    public static long llroundl(double value)
        => RoundedLong(global::System.Math.Round(value, global::System.MidpointRounding.AwayFromZero));

    private static long RoundedLong(double value)
    {
        if (double.IsNaN(value) || value < -9223372036854775808.0 || value >= 9223372036854775808.0)
        {
            errno = EDOM;
            return long.MinValue;
        }
        return (long)value;
    }

    public static double modf(double value, double* integral)
    {
        *integral = global::System.Math.Truncate(value);
        if (double.IsNaN(value)) return value;
        if (double.IsInfinity(value)) return global::System.Math.CopySign(0.0, value);
        double fraction = value - *integral;
        return fraction == 0.0 ? global::System.Math.CopySign(0.0, value) : fraction;
    }

    public static int fpclassify(double value)
        => double.IsNaN(value) ? FP_NAN : double.IsInfinity(value) ? FP_INFINITE
            : value == 0.0 ? FP_ZERO : double.IsSubnormal(value) ? FP_SUBNORMAL : FP_NORMAL;
    public static int fpclassify(float value)
        => float.IsNaN(value) ? FP_NAN : float.IsInfinity(value) ? FP_INFINITE
            : value == 0.0f ? FP_ZERO : float.IsSubnormal(value) ? FP_SUBNORMAL : FP_NORMAL;
    // C99 type-generic header dispatch preserves float subnormals before promotion.
    public static int __dotcc_fpclassify_float(float value) => fpclassify(value);
    public static int __dotcc_fpclassify_double(double value) => fpclassify(value);

    /// <summary>Parse in the compiler's binary64 long-double model. Reuses the
    /// C-locale decimal/hex parser and reports finite numeric range errors;
    /// explicit infinity/NaN spellings remain successful conversions.</summary>
    public static double strtold(byte* input, byte** endptr)
    {
        byte* end;
        double value = strtod(input, &end);
        if (endptr != null) *endptr = end;
        if (end == input) return value;
        byte* digits = input;
        while (*digits == (byte)' ' || (*digits >= 9 && *digits <= 13)) digits++;
        if (*digits == (byte)'+' || *digits == (byte)'-') digits++;
        bool numeric = *digits == (byte)'.' || (*digits >= (byte)'0' && *digits <= (byte)'9');
        if (numeric && (double.IsInfinity(value) || double.IsSubnormal(value)
            || value == 0.0 && NumericMantissaNonzero(digits, end))) errno = ERANGE;
        return value;
    }

    private static bool NumericMantissaNonzero(byte* digits, byte* end)
    {
        bool hex = end - digits >= 2 && digits[0] == (byte)'0' && (digits[1] == (byte)'x' || digits[1] == (byte)'X');
        if (hex) digits += 2;
        for (; digits < end; digits++)
        {
            byte c = *digits;
            if (hex ? c == (byte)'p' || c == (byte)'P' : c == (byte)'e' || c == (byte)'E') break;
            if (c >= (byte)'1' && c <= (byte)'9'
                || hex && ((c >= (byte)'a' && c <= (byte)'f') || (c >= (byte)'A' && c <= (byte)'F'))) return true;
        }
        return false;
    }
}
