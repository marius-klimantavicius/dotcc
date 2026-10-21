// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
// Handwritten EDG lexical.c scan_number / literals.c conv_float_literal
// adaptation. Decimal conversion retains floating.c's exact power-of-five /
// power-of-two and half-ULP decisions; quotient/remainder replaces its preliminary
// approximate float followed by bigint adjustment. No host floating parser.
using System.Numerics;

namespace EdgSharp;

// Choosing this profile is an explicit numerical target choice. It does not
// qualify an EDG executable, C++ type identity, storage alignment or long double.
internal sealed class FloatingLiteralProfile
{
    internal static FloatingLiteralProfile Binary32Binary64 { get; } =
        new(TargetFloatFormat.Binary32, TargetFloatFormat.Binary64);
    internal TargetFloatFormat FloatFormat { get; }
    internal TargetFloatFormat DoubleFormat { get; }
    internal bool DigitSeparators { get; }
    internal bool Hexadecimal { get; }

    internal FloatingLiteralProfile(TargetFloatFormat floatFormat, TargetFloatFormat doubleFormat,
        bool digitSeparators = true, bool hexadecimal = true)
    {
        ArgumentNullException.ThrowIfNull(floatFormat);
        ArgumentNullException.ThrowIfNull(doubleFormat);
        (FloatFormat, DoubleFormat, DigitSeparators, Hexadecimal) = (floatFormat, doubleFormat, digitSeparators, hexadecimal);
    }
}

internal enum FloatingDecodeStatus { Decoded, Malformed, ValidUnsupported, ResourceLimit }
internal enum FloatingLiteralSuffix
{
    None, Float, LongDouble, Float16, BFloat16, Float32, Float64, Float128,
    Float32x, Float64x, Float80Extension, Float128Extension, Imaginary, UserDefined
}
internal enum FloatingLiteralIssue
{
    None, MissingDigits, NotFloatingLiteral, MissingExponent, InvalidSeparator, InvalidSuffix,
    UnsupportedDialect, UnqualifiedFormat, ImaginarySuffix, UserDefinedSuffix, UnqualifiedIdentifierSyntax,
    TokenLimit, SignificandLimit, ExponentLimit, IntermediateLimit
}

// Numeric range outcomes still have Status=Decoded. They are not lexical
// errors. The primary frontend chooses EDG profile-specific diagnostics from
// Numeric.Flags. No source/AST/diagnostic owner is captured by this result.
internal readonly record struct FloatingLiteralResult(FloatingDecodeStatus Status, FloatingLiteralSuffix Suffix,
    bool IsHexadecimal, int SuffixOffset, int ErrorOffset, FloatingLiteralIssue Issue, FloatingResult Numeric)
{
    internal TargetFloatValue? Value => Numeric.Value;
    internal bool IsExact => Status == FloatingDecodeStatus.Decoded && Numeric.IsExact;
}

internal static class FloatingLiterals
{
    // More than four times the absolute maximum token length plus any binary64
    // exponent. Saturation cannot be canceled by a bounded significand's scale.
    private const int ExponentSaturation = 1_000_000;

    internal static FloatingLiteralResult Decode(string spelling, FloatingLiteralProfile profile, FloatingLimits? limits = null)
    {
        ArgumentNullException.ThrowIfNull(spelling);
        ArgumentNullException.ThrowIfNull(profile);
        limits ??= FloatingLimits.Default;
        if (spelling.Length > limits.MaxTokenCharacters)
            return Failure(FloatingDecodeStatus.ResourceLimit, FloatingLiteralIssue.TokenLimit, limits.MaxTokenCharacters);
        if (spelling.Length == 0) return Failure(FloatingDecodeStatus.Malformed, FloatingLiteralIssue.MissingDigits, 0);

        bool hex = spelling.Length >= 2 && spelling[0] == '0' && spelling[1] is 'x' or 'X';
        int radix = hex ? 16 : 10;
        int position = hex ? 2 : 0;
        var digits = new SignificandScan { FirstNonzero = -1, LastNonzero = -1 };
        if (!ScanDigits(spelling, ref position, radix, false, ref digits, limits, out FloatingLiteralIssue issue))
            return ScanFailure(issue, position, hex);
        bool dot = position < spelling.Length && spelling[position] == '.';
        if (dot)
        {
            ++position;
            if (!ScanDigits(spelling, ref position, radix, true, ref digits, limits, out issue))
                return ScanFailure(issue, position, hex);
        }
        if (digits.Count == 0) return Failure(FloatingDecodeStatus.Malformed, FloatingLiteralIssue.MissingDigits, position, hex);

        bool hasExponent = position < spelling.Length && (hex ? spelling[position] is 'p' or 'P' : spelling[position] is 'e' or 'E');
        int exponent = 0;
        if (hasExponent)
        {
            int exponentMarker = position;
            ++position;
            bool negative = position < spelling.Length && spelling[position] == '-';
            bool signedExponent = position < spelling.Length && spelling[position] is '+' or '-';
            if (signedExponent) ++position;
            int count = 0;
            bool previousDigit = false;
            while (position < spelling.Length)
            {
                char c = spelling[position];
                if (c is >= '0' and <= '9')
                {
                    if (++count > limits.MaxExponentDigits)
                        return Failure(FloatingDecodeStatus.ResourceLimit, FloatingLiteralIssue.ExponentLimit, position, hex);
                    int digit = c - '0';
                    exponent = exponent <= (ExponentSaturation - digit) / 10 ? exponent * 10 + digit : ExponentSaturation;
                    ++position;
                    previousDigit = true;
                }
                else if (c == '\'')
                {
                    if (!previousDigit || position + 1 >= spelling.Length || spelling[position + 1] is < '0' or > '9')
                        return Failure(FloatingDecodeStatus.Malformed, FloatingLiteralIssue.InvalidSeparator, position, hex);
                    digits.Separator = true;
                    ++position;
                    previousDigit = false;
                }
                else break;
            }
            if (count == 0)
            {
                // EDG scan_number restores possible_start_of_ud_suffix when
                // a decimal point already identifies a floating numeric core.
                // The e/E and its identifier tail belong to the whole UDL
                // suffix; actual literal-operator lookup remains unsupported.
                // A sign cannot be an identifier part, and misplaced apostrophes
                // have already failed above. Hex still requires a real exponent.
                if (!hex && dot && !signedExponent) position = exponentMarker;
                else return Failure(FloatingDecodeStatus.Malformed, FloatingLiteralIssue.MissingExponent, position, hex);
            }
            if (negative) exponent = -exponent;
        }
        else if (hex) return Failure(FloatingDecodeStatus.Malformed, FloatingLiteralIssue.MissingExponent, position, hex);
        else if (!dot) return Failure(FloatingDecodeStatus.Malformed, FloatingLiteralIssue.NotFloatingLiteral, position);

        int suffixOffset = position;
        ReadOnlySpan<char> suffixText = spelling.AsSpan(suffixOffset);
        FloatingLiteralSuffix suffix = ClassifySuffix(suffixText);
        if (suffix == FloatingLiteralSuffix.UserDefined)
        {
            // Reuse the accepted scalar/UCN identifier validator, under the
            // token length bound. Unknown identifier suffixes need UDL lookup,
            // including reserved spellings such as ff; they are not silently
            // accepted as a second builtin suffix or called malformed C++.
            UnicodeIdentifiers.Problem? problem = UnicodeIdentifiers.Analyze(spelling[suffixOffset..], out _);
            if (problem is { } invalid)
                return new(invalid.Unsupported ? FloatingDecodeStatus.ValidUnsupported : FloatingDecodeStatus.Malformed,
                    suffix, hex, suffixOffset, suffixOffset + invalid.Offset,
                    invalid.Unsupported ? FloatingLiteralIssue.UnqualifiedIdentifierSyntax : FloatingLiteralIssue.InvalidSuffix, default);
        }

        if ((!profile.DigitSeparators && digits.Separator) || (!profile.Hexadecimal && hex))
            return Unsupported(FloatingLiteralIssue.UnsupportedDialect, suffix, hex, suffixOffset);
        if (suffix is not (FloatingLiteralSuffix.None or FloatingLiteralSuffix.Float))
            return Unsupported(suffix == FloatingLiteralSuffix.UserDefined ? FloatingLiteralIssue.UserDefinedSuffix :
                suffix == FloatingLiteralSuffix.Imaginary ? FloatingLiteralIssue.ImaginarySuffix : FloatingLiteralIssue.UnqualifiedFormat,
                suffix, hex, suffixOffset);

        TargetFloatFormat format = suffix == FloatingLiteralSuffix.Float ? profile.FloatFormat : profile.DoubleFormat;
        FloatingLiteralResult Finish(FloatingResult numeric) => new(
            (numeric.Flags & FloatingFlags.ResourceLimit) != 0 ? FloatingDecodeStatus.ResourceLimit : FloatingDecodeStatus.Decoded,
            suffix, hex, suffixOffset, (numeric.Flags & FloatingFlags.ResourceLimit) != 0 ? 0 : -1,
            (numeric.Flags & FloatingFlags.ResourceLimit) != 0 ? FloatingLiteralIssue.IntermediateLimit : FloatingLiteralIssue.None, numeric);

        if (digits.FirstNonzero < 0) return Finish(new(TargetFloatValue.Zero(format)));

        // Removing trailing zero digits reduces exact power and integer work.
        int power = exponent - (hex ? 4 * digits.FractionCount : digits.FractionCount) +
            (hex ? 4 * digits.TrailingZeros : digits.TrailingZeros);
        int significantDigits = digits.Count - digits.LeadingZeros - digits.TrailingZeros;
        if (!hex)
        {
            int decimalOrder = significantDigits - 1 + power;
            // Conservative integer-only range guards: 10^309 > 2^1024 and
            // 10^-325 < 2^-1075; analogous binary32 bounds are 10^39/10^-47.
            // Boundary orders go through exact arithmetic below.
            int high = ReferenceEquals(format, TargetFloatFormat.Binary32) ? 38 : 308;
            int low = ReferenceEquals(format, TargetFloatFormat.Binary32) ? -46 : -324;
            if (decimalOrder > high)
                return Finish(new(TargetFloatValue.Infinity(format), FloatingFlags.Overflow | FloatingFlags.Inexact));
            if (decimalOrder < low)
                return Finish(new(TargetFloatValue.Zero(format), FloatingFlags.Tiny | FloatingFlags.Underflow | FloatingFlags.Inexact));
        }

        // Bound every accumulation and power before allocating a BigInteger.
        // A decimal digit requires fewer than 4 bits, and 5^n fewer than 3n+1.
        long significandBound = (long)significantDigits * 4;
        long fiveBound = hex ? 0 : (long)Math.Abs(power) * 3 + 1;
        if (significandBound > limits.MaxIntermediateBits ||
            !hex && (power >= 0 ? significandBound + fiveBound : fiveBound) > limits.MaxIntermediateBits)
            return Finish(new(null, FloatingFlags.ResourceLimit));

        BigInteger significand = BigInteger.Zero;
        for (int offset = digits.FirstNonzero; offset <= digits.LastNonzero; ++offset)
        {
            int digit = Digit(spelling[offset], radix);
            if (digit >= 0) significand = significand * radix + digit;
        }
        // Narrow root-authorized adaptation: pinned EDG's hex subnormal carry
        // followed by unconditional exponent reset can pack half-min-normal.
        // Keep correct target rounding here, including the two witnessed inputs;
        // do not copy that defect into decimal/conversion/arithmetic semantics.
        // Exact witnesses, midpoint proof and policy are retained in the handoff.
        if (hex) return Finish(FloatingValues.RoundRational(format, false, significand, BigInteger.One, power, limits));

        BigInteger numerator = significand, denominator = BigInteger.One;
        // 10^power = 5^power * 2^power. Bounds above cap Pow independently
        // of hostile exponent spelling. No process/TU power cache is retained.
        if (power >= 0) numerator *= BigInteger.Pow(new BigInteger(5), power);
        else denominator = BigInteger.Pow(new BigInteger(5), -power);
        return Finish(FloatingValues.RoundRational(format, false, numerator, denominator, power, limits));
    }

    private struct SignificandScan
    {
        internal int Count;
        internal int FractionCount;
        internal int LeadingZeros;
        internal int TrailingZeros;
        internal int FirstNonzero;
        internal int LastNonzero;
        internal bool Separator;
    }

    private static bool ScanDigits(string text, ref int position, int radix, bool fraction,
        ref SignificandScan scan, FloatingLimits limits, out FloatingLiteralIssue issue)
    {
        issue = FloatingLiteralIssue.None;
        bool previousDigit = false;
        while (position < text.Length)
        {
            int digit = Digit(text[position], radix);
            if (digit >= 0)
            {
                if (++scan.Count > limits.MaxSignificandDigits) { issue = FloatingLiteralIssue.SignificandLimit; return false; }
                if (fraction) ++scan.FractionCount;
                if (digit == 0)
                {
                    if (scan.FirstNonzero < 0) ++scan.LeadingZeros;
                    else ++scan.TrailingZeros;
                }
                else
                {
                    if (scan.FirstNonzero < 0) scan.FirstNonzero = position;
                    scan.LastNonzero = position;
                    scan.TrailingZeros = 0;
                }
                ++position;
                previousDigit = true;
            }
            else if (text[position] == '\'')
            {
                if (!previousDigit || position + 1 >= text.Length || Digit(text[position + 1], radix) < 0)
                { issue = FloatingLiteralIssue.InvalidSeparator; return false; }
                scan.Separator = true;
                ++position;
                previousDigit = false;
            }
            else break;
        }
        return true;
    }

    private static int Digit(char c, int radix) => c is >= '0' and <= '9' ? c - '0' :
        radix == 16 ? c is >= 'a' and <= 'f' ? c - 'a' + 10 : c is >= 'A' and <= 'F' ? c - 'A' + 10 : -1 : -1;

    private static FloatingLiteralSuffix ClassifySuffix(ReadOnlySpan<char> suffix) => suffix switch
    {
        "" => FloatingLiteralSuffix.None,
        "f" or "F" => FloatingLiteralSuffix.Float,
        "l" or "L" => FloatingLiteralSuffix.LongDouble,
        "f16" or "F16" => FloatingLiteralSuffix.Float16,
        "bf16" or "BF16" => FloatingLiteralSuffix.BFloat16,
        "f32" or "F32" => FloatingLiteralSuffix.Float32,
        "f64" or "F64" => FloatingLiteralSuffix.Float64,
        "f128" or "F128" => FloatingLiteralSuffix.Float128,
        "f32x" or "F32x" => FloatingLiteralSuffix.Float32x,
        "f64x" or "F64x" => FloatingLiteralSuffix.Float64x,
        "w" or "W" => FloatingLiteralSuffix.Float80Extension,
        "q" or "Q" => FloatingLiteralSuffix.Float128Extension,
        _ => IsImaginarySuffix(suffix) ? FloatingLiteralSuffix.Imaginary : FloatingLiteralSuffix.UserDefined
    };

    private static bool IsImaginarySuffix(ReadOnlySpan<char> suffix)
    {
        static bool Imaginary(char c) => c is 'i' or 'I' or 'j' or 'J';
        static bool Precision(char c) => c is 'f' or 'F' or 'l' or 'L' or 'w' or 'W' or 'q' or 'Q';
        return suffix.Length == 1 && Imaginary(suffix[0]) || suffix.Length == 2 &&
            (Imaginary(suffix[0]) && Precision(suffix[1]) || Precision(suffix[0]) && Imaginary(suffix[1]));
    }

    private static FloatingLiteralResult Failure(FloatingDecodeStatus status, FloatingLiteralIssue issue, int offset, bool hex = false)
        => new(status, FloatingLiteralSuffix.None, hex, -1, offset, issue,
            status == FloatingDecodeStatus.ResourceLimit ? new(null, FloatingFlags.ResourceLimit) : default);
    private static FloatingLiteralResult ScanFailure(FloatingLiteralIssue issue, int offset, bool hex)
        => Failure(issue == FloatingLiteralIssue.SignificandLimit ? FloatingDecodeStatus.ResourceLimit : FloatingDecodeStatus.Malformed, issue, offset, hex);
    private static FloatingLiteralResult Unsupported(FloatingLiteralIssue issue, FloatingLiteralSuffix suffix, bool hex, int suffixOffset)
        => new(FloatingDecodeStatus.ValidUnsupported, suffix, hex, suffixOffset, suffixOffset, issue, default);
}
