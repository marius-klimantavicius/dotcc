// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
// Handwritten adaptation of EDG float_pt.c/float_type.h and floating.c:
// explicit target representation, exact significand scaling, half-ULP comparison,
// ties to even, gradual underflow and carry renormalization. See the floating
// implementation handoff for precise routines, hashes and omitted branches.
using System.Numerics;

namespace EdgSharp;

// These are numerical formats, not C++ type identities or host/ABI discoveries.
// Only these two qualified encodings can be constructed in this foundation.
internal sealed class TargetFloatFormat
{
    internal static TargetFloatFormat Binary32 { get; } = new("binary32", 32, 8, 23, 127);
    internal static TargetFloatFormat Binary64 { get; } = new("binary64", 64, 11, 52, 1023);

    internal string Name { get; }
    internal int StorageBits { get; }
    internal int ExponentBits { get; }
    internal int FractionBits { get; }
    internal int Precision => FractionBits + 1;
    internal int ExponentBias { get; }
    internal int MinNormalExponent => 1 - ExponentBias;
    internal int MaxNormalExponent => ExponentBias;
    internal int MinSubnormalExponent => MinNormalExponent - FractionBits;
    internal ulong SignMask => 1UL << (StorageBits - 1);
    internal ulong FractionMask => (1UL << FractionBits) - 1;
    internal ulong ExponentMask => ((1UL << ExponentBits) - 1) << FractionBits;
    internal ulong QuietNaNMask => 1UL << (FractionBits - 1);

    private TargetFloatFormat(string name, int storageBits, int exponentBits, int fractionBits, int bias)
        => (Name, StorageBits, ExponentBits, FractionBits, ExponentBias) =
            (name, storageBits, exponentBits, fractionBits, bias);
}

internal enum TargetFloatClass { Zero, Subnormal, Normal, Infinity, NaN }

// Object equality is representation equality, including NaN payload and zero sign.
// C++ numerical equality/order must use FloatingValues.Compare, not Equals.
internal sealed class TargetFloatValue : IEquatable<TargetFloatValue>
{
    internal TargetFloatFormat Format { get; }
    internal ulong Bits { get; }
    internal bool IsNegative => (Bits & Format.SignMask) != 0;
    internal ulong Fraction => Bits & Format.FractionMask;
    internal int BiasedExponent => (int)((Bits & Format.ExponentMask) >> Format.FractionBits);
    internal TargetFloatClass Class => (Bits & Format.ExponentMask) == Format.ExponentMask
        ? Fraction == 0 ? TargetFloatClass.Infinity : TargetFloatClass.NaN
        : BiasedExponent == 0
            ? Fraction == 0 ? TargetFloatClass.Zero : TargetFloatClass.Subnormal
            : TargetFloatClass.Normal;
    internal bool IsFinite => Class is TargetFloatClass.Zero or TargetFloatClass.Subnormal or TargetFloatClass.Normal;
    internal bool IsZero => Class == TargetFloatClass.Zero;
    internal bool IsNaN => Class == TargetFloatClass.NaN;
    internal bool IsSignalingNaN => IsNaN && (Fraction & Format.QuietNaNMask) == 0;
    internal ulong NaNPayload => IsNaN ? Fraction & ~Format.QuietNaNMask : 0;

    private TargetFloatValue(TargetFloatFormat format, ulong bits) => (Format, Bits) = (format, bits);

    internal static TargetFloatValue FromBits(TargetFloatFormat format, ulong bits)
    {
        ArgumentNullException.ThrowIfNull(format);
        if (format.StorageBits == 32 && bits > uint.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(bits), "Payload exceeds binary32 storage.");
        return new(format, bits);
    }

    internal static TargetFloatValue Zero(TargetFloatFormat format, bool negative = false)
        => FromBits(format, negative ? format.SignMask : 0);
    internal static TargetFloatValue Infinity(TargetFloatFormat format, bool negative = false)
        => FromBits(format, format.ExponentMask | (negative ? format.SignMask : 0));
    internal static TargetFloatValue NaN(TargetFloatFormat format, ulong payload = 0, bool signaling = false, bool negative = false)
    {
        if ((payload & ~(format.FractionMask & ~format.QuietNaNMask)) != 0 || signaling && payload == 0)
            throw new ArgumentOutOfRangeException(nameof(payload), "Signaling NaNs need a nonzero payload; payload excludes the quiet bit.");
        return FromBits(format, format.ExponentMask | payload | (signaling ? 0 : format.QuietNaNMask) |
            (negative ? format.SignMask : 0));
    }

    internal TargetFloatValue Negate() => FromBits(Format, Bits ^ Format.SignMask);
    internal TargetFloatValue QuietNaN() => IsNaN ? FromBits(Format, Bits | Format.QuietNaNMask) : this;
    public bool Equals(TargetFloatValue? other) => other != null && ReferenceEquals(Format, other.Format) && Bits == other.Bits;
    public override bool Equals(object? obj) => obj is TargetFloatValue other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Format, Bits);
}

[Flags]
internal enum FloatingFlags
{
    None = 0,
    Inexact = 1,
    // Tininess is detected AFTER rounding, including a nonzero value rounded to zero.
    // Exact zero has no Tiny flag. Underflow means Tiny AND Inexact, not any subnormal.
    Tiny = 2,
    Underflow = 4,
    Overflow = 8,
    InvalidOperation = 16,
    DivideByZero = 32,
    IntegerOutOfRange = 64,
    ResourceLimit = 128
}

internal readonly record struct FloatingResult(TargetFloatValue? Value, FloatingFlags Flags = FloatingFlags.None)
{
    internal bool HasValue => Value != null;
    // Exactness is a finite numerical fact; copying a NaN payload does not
    // assert a mathematical exact value, even if no exception flag was raised.
    internal bool IsExact => Value is { IsFinite: true } &&
        (Flags & (FloatingFlags.Inexact | FloatingFlags.InvalidOperation | FloatingFlags.DivideByZero)) == 0;
}

internal readonly record struct FloatingIntegerResult(BigInteger? Value, FloatingFlags Flags = FloatingFlags.None);
internal enum FloatingOrder { Less, Equal, Greater, Unordered }

// Immutable per-call policy. Hard ceilings also apply to caller-supplied policies.
// BigInteger inputs already owned by callers are inspected before copying/scaling.
internal sealed class FloatingLimits
{
    internal static FloatingLimits Default { get; } = new();
    internal int MaxTokenCharacters { get; }
    internal int MaxSignificandDigits { get; }
    internal int MaxExponentDigits { get; }
    internal int MaxIntermediateBits { get; }

    internal FloatingLimits(int maxTokenCharacters = 16_384, int maxSignificandDigits = 4_096,
        int maxExponentDigits = 4_096, int maxIntermediateBits = 32_768)
    {
        if (maxTokenCharacters is < 1 or > 65_536) throw new ArgumentOutOfRangeException(nameof(maxTokenCharacters));
        if (maxSignificandDigits is < 1 or > 16_384) throw new ArgumentOutOfRangeException(nameof(maxSignificandDigits));
        if (maxExponentDigits is < 1 or > 16_384) throw new ArgumentOutOfRangeException(nameof(maxExponentDigits));
        if (maxIntermediateBits is < 64 or > 65_536) throw new ArgumentOutOfRangeException(nameof(maxIntermediateBits));
        (MaxTokenCharacters, MaxSignificandDigits, MaxExponentDigits, MaxIntermediateBits) =
            (maxTokenCharacters, maxSignificandDigits, maxExponentDigits, maxIntermediateBits);
    }
}

internal static class FloatingValues
{
    private const FloatingFlags LostTiny = FloatingFlags.Tiny | FloatingFlags.Underflow | FloatingFlags.Inexact;
    private static FloatingResult Resource => new(null, FloatingFlags.ResourceLimit);

    internal static FloatingResult FromInteger(TargetFloatFormat format, BigInteger integer, FloatingLimits? limits = null)
    {
        limits ??= FloatingLimits.Default;
        if (integer.GetBitLength() > limits.MaxIntermediateBits) return Resource;
        return RoundRational(format, integer.Sign < 0, BigInteger.Abs(integer), BigInteger.One, 0, limits);
    }

    // Round |numerator / denominator * 2^binaryExponent| exactly once to the target.
    // Sign is separate to preserve -0. This is an internal numeric boundary, so
    // negative magnitudes/nonpositive denominators are programmer errors.
    internal static FloatingResult RoundRational(TargetFloatFormat format, bool negative, BigInteger numerator,
        BigInteger denominator, int binaryExponent = 0, FloatingLimits? limits = null)
    {
        ArgumentNullException.ThrowIfNull(format);
        if (numerator.Sign < 0) throw new ArgumentOutOfRangeException(nameof(numerator));
        if (denominator.Sign <= 0) throw new ArgumentOutOfRangeException(nameof(denominator));
        limits ??= FloatingLimits.Default;
        long nBits = numerator.GetBitLength(), dBits = denominator.GetBitLength();
        if (nBits > limits.MaxIntermediateBits || dBits > limits.MaxIntermediateBits) return Resource;
        if (numerator.IsZero) return new(TargetFloatValue.Zero(format, negative));

        // The true floor(log2(value)) is either upper or upper-1. These tests
        // decide extreme exponents without ever shifting by a supplied exponent.
        long upper = nBits - dBits + binaryExponent;
        if (upper > format.MaxNormalExponent + 1L) return Overflow(format, negative);
        if (upper < format.MinSubnormalExponent - 1L) return new(TargetFloatValue.Zero(format, negative), LostTiny);

        int difference = (int)(nBits - dBits); // Inputs were bounded above.
        int comparison = difference >= 0
            ? numerator.CompareTo(denominator << difference)
            : (numerator << -difference).CompareTo(denominator);
        long exponent = upper - (comparison < 0 ? 1 : 0);
        if (exponent > format.MaxNormalExponent) return Overflow(format, negative);
        if (exponent < format.MinSubnormalExponent - 1L) return new(TargetFloatValue.Zero(format, negative), LostTiny);

        // Normal spacing is 2^(exponent-fractionBits); below emin, clamp to
        // subnormal spacing. Rounding directly at that spacing avoids a second
        // rounding of an already-rounded normal significand near underflow.
        int quantum = (int)Math.Max(exponent - format.FractionBits, format.MinSubnormalExponent);
        long shift = (long)binaryExponent - quantum;
        if (shift >= 0)
        {
            if (nBits + shift > limits.MaxIntermediateBits) return Resource;
            numerator <<= (int)shift;
        }
        else
        {
            if (dBits - shift > limits.MaxIntermediateBits) return Resource;
            denominator <<= (int)-shift;
        }

        BigInteger significand = BigInteger.DivRem(numerator, denominator, out BigInteger remainder);
        bool inexact = !remainder.IsZero;
        int half = remainder.CompareTo(denominator - remainder);
        if (half > 0 || half == 0 && !significand.IsEven) ++significand;

        ulong hidden = 1UL << format.FractionBits;
        ulong rounded = (ulong)significand; // At most precision+1 <= 54 bits.
        if (rounded >= hidden * 2)
        {
            rounded >>= 1;
            ++quantum;
        }
        int roundedExponent = quantum + format.FractionBits;
        if (roundedExponent > format.MaxNormalExponent) return Overflow(format, negative);
        bool tiny = rounded < hidden;
        ulong bits = tiny ? rounded : ((ulong)(roundedExponent + format.ExponentBias) << format.FractionBits) | (rounded - hidden);
        if (negative) bits |= format.SignMask;
        FloatingFlags flags = inexact ? FloatingFlags.Inexact : FloatingFlags.None;
        if (tiny) flags |= FloatingFlags.Tiny | (inexact ? FloatingFlags.Underflow : FloatingFlags.None);
        return new(TargetFloatValue.FromBits(format, bits), flags);
    }

    private static FloatingResult Overflow(TargetFloatFormat format, bool negative)
        => new(TargetFloatValue.Infinity(format, negative), FloatingFlags.Overflow | FloatingFlags.Inexact);

    // C++ floating -> integer conversion truncates toward zero before checking
    // representability. This does not wrap or saturate a failed conversion.
    internal static FloatingIntegerResult ToInteger(TargetFloatValue value, int width, bool signed, FloatingLimits? limits = null)
    {
        if (width < 1) throw new ArgumentOutOfRangeException(nameof(width));
        limits ??= FloatingLimits.Default;
        if (width > limits.MaxIntermediateBits) return new(null, FloatingFlags.ResourceLimit);
        if (!value.IsFinite) return new(null, FloatingFlags.InvalidOperation);
        Decompose(value, out BigInteger magnitude, out int exponent);
        bool inexact = false;
        if (exponent >= 0)
        {
            if (!FitsShift(magnitude, exponent, limits)) return new(null, FloatingFlags.ResourceLimit);
            magnitude <<= exponent;
        }
        else
        {
            BigInteger truncated = magnitude >> -exponent;
            inexact = (truncated << -exponent) != magnitude;
            magnitude = truncated;
        }
        BigInteger integer = value.IsNegative ? -magnitude : magnitude;
        bool outside;
        if (signed)
        {
            // Only the negative endpoint includes magnitude 2^(width-1).
            // No unsigned 2^width allocation is needed at the budget boundary.
            outside = integer.Sign < 0 ? magnitude > (BigInteger.One << (width - 1)) : magnitude.GetBitLength() >= width;
        }
        else outside = integer.Sign < 0 || magnitude.GetBitLength() > width;
        FloatingFlags flags = inexact ? FloatingFlags.Inexact : FloatingFlags.None;
        return outside
            ? new(null, flags | FloatingFlags.InvalidOperation | FloatingFlags.IntegerOutOfRange)
            : new(integer, flags);
    }

    internal static FloatingResult Convert(TargetFloatValue value, TargetFloatFormat destination, FloatingLimits? limits = null)
    {
        ArgumentNullException.ThrowIfNull(destination);
        // A representation-preserving copy does not quiet signaling NaNs.
        if (ReferenceEquals(value.Format, destination)) return new(value);
        if (value.IsNaN)
        {
            int shift = destination.FractionBits - value.Format.FractionBits;
            ulong payload = shift >= 0 ? value.NaNPayload << shift : value.NaNPayload >> -shift;
            // Cross-format conversion quiets NaNs; truncated payload bits are
            // deliberately not a finite numerical Inexact condition.
            return new(TargetFloatValue.NaN(destination, payload, negative: value.IsNegative),
                value.IsSignalingNaN ? FloatingFlags.InvalidOperation : FloatingFlags.None);
        }
        if (value.Class == TargetFloatClass.Infinity) return new(TargetFloatValue.Infinity(destination, value.IsNegative));
        Decompose(value, out BigInteger significand, out int exponent);
        return RoundRational(destination, value.IsNegative, significand, BigInteger.One, exponent, limits);
    }

    // Operands must already have undergone C++ usual arithmetic conversions.
    // No implicit type/rank/promotion choices occur inside this numeric module.
    internal static FloatingResult Add(TargetFloatValue left, TargetFloatValue right, FloatingLimits? limits = null)
    {
        RequireSameFormat(left, right);
        if (left.IsNaN || right.IsNaN) return PropagateNaN(left, right);
        if (left.Class == TargetFloatClass.Infinity || right.Class == TargetFloatClass.Infinity)
        {
            if (left.Class == TargetFloatClass.Infinity && right.Class == TargetFloatClass.Infinity && left.IsNegative != right.IsNegative)
                return Invalid(left.Format);
            return new(left.Class == TargetFloatClass.Infinity ? left : right);
        }
        limits ??= FloatingLimits.Default;
        Decompose(left, out BigInteger l, out int le);
        Decompose(right, out BigInteger r, out int re);
        int common = Math.Min(le, re);
        if (!FitsShift(l, le - common, limits) || !FitsShift(r, re - common, limits)) return Resource;
        l <<= le - common;
        r <<= re - common;
        long sumBound = Math.Max(l.GetBitLength(), r.GetBitLength()) + 1;
        if (left.IsNegative) l = -l;
        if (right.IsNegative) r = -r;
        // Bound the possible carry before allocating the sum.
        if (sumBound > limits.MaxIntermediateBits) return Resource;
        BigInteger sum = l + r;
        bool negative = sum.Sign < 0 || sum.IsZero && left.IsNegative && right.IsNegative;
        return RoundRational(left.Format, negative, BigInteger.Abs(sum), BigInteger.One, common, limits);
    }

    internal static FloatingResult Subtract(TargetFloatValue left, TargetFloatValue right, FloatingLimits? limits = null)
    {
        RequireSameFormat(left, right);
        if (left.IsNaN || right.IsNaN) return PropagateNaN(left, right);
        return Add(left, right.Negate(), limits);
    }

    internal static FloatingResult Multiply(TargetFloatValue left, TargetFloatValue right, FloatingLimits? limits = null)
    {
        RequireSameFormat(left, right);
        if (left.IsNaN || right.IsNaN) return PropagateNaN(left, right);
        bool negative = left.IsNegative != right.IsNegative;
        if (left.Class == TargetFloatClass.Infinity || right.Class == TargetFloatClass.Infinity)
            return left.IsZero || right.IsZero ? Invalid(left.Format) : new(TargetFloatValue.Infinity(left.Format, negative));
        if (left.IsZero || right.IsZero) return new(TargetFloatValue.Zero(left.Format, negative));
        limits ??= FloatingLimits.Default;
        Decompose(left, out BigInteger l, out int le);
        Decompose(right, out BigInteger r, out int re);
        if (l.GetBitLength() + r.GetBitLength() > limits.MaxIntermediateBits) return Resource;
        return RoundRational(left.Format, negative, l * r, BigInteger.One, le + re, limits);
    }

    internal static FloatingResult Divide(TargetFloatValue left, TargetFloatValue right, FloatingLimits? limits = null)
    {
        RequireSameFormat(left, right);
        if (left.IsNaN || right.IsNaN) return PropagateNaN(left, right);
        bool negative = left.IsNegative != right.IsNegative;
        if (left.IsZero && right.IsZero || left.Class == TargetFloatClass.Infinity && right.Class == TargetFloatClass.Infinity)
            return Invalid(left.Format);
        if (left.Class == TargetFloatClass.Infinity) return new(TargetFloatValue.Infinity(left.Format, negative));
        if (right.Class == TargetFloatClass.Infinity) return new(TargetFloatValue.Zero(left.Format, negative));
        if (right.IsZero) return new(TargetFloatValue.Infinity(left.Format, negative), FloatingFlags.DivideByZero);
        if (left.IsZero) return new(TargetFloatValue.Zero(left.Format, negative));
        Decompose(left, out BigInteger l, out int le);
        Decompose(right, out BigInteger r, out int re);
        return RoundRational(left.Format, negative, l, r, le - re, limits);
    }

    internal static FloatingOrder Compare(TargetFloatValue left, TargetFloatValue right)
    {
        RequireSameFormat(left, right);
        // Quiet numerical predicates: all relations except != are false for
        // unordered operands. Signaling exception policy belongs to evaluation.
        if (left.IsNaN || right.IsNaN) return FloatingOrder.Unordered;
        if (left.IsZero && right.IsZero) return FloatingOrder.Equal;
        if (left.IsNegative != right.IsNegative) return left.IsNegative ? FloatingOrder.Less : FloatingOrder.Greater;
        int order = left.Bits.CompareTo(right.Bits);
        if (left.IsNegative) order = -order;
        return order < 0 ? FloatingOrder.Less : order > 0 ? FloatingOrder.Greater : FloatingOrder.Equal;
    }

    internal static bool NumericallyEqual(TargetFloatValue left, TargetFloatValue right) => Compare(left, right) == FloatingOrder.Equal;
    internal static bool ToBoolean(TargetFloatValue value) => !value.IsZero;

    private static FloatingResult Invalid(TargetFloatFormat format) => new(TargetFloatValue.NaN(format), FloatingFlags.InvalidOperation);
    private static FloatingResult PropagateNaN(TargetFloatValue left, TargetFloatValue right)
        => new((left.IsNaN ? left : right).QuietNaN(),
            left.IsSignalingNaN || right.IsSignalingNaN ? FloatingFlags.InvalidOperation : FloatingFlags.None);

    private static void RequireSameFormat(TargetFloatValue left, TargetFloatValue right)
    {
        if (!ReferenceEquals(left.Format, right.Format))
            throw new ArgumentException("Explicit floating conversion is required before a binary operation.");
    }

    private static bool FitsShift(BigInteger magnitude, int shift, FloatingLimits limits)
        => magnitude.IsZero || magnitude.GetBitLength() + shift <= limits.MaxIntermediateBits;

    private static void Decompose(TargetFloatValue value, out BigInteger significand, out int exponent)
    {
        significand = value.Fraction;
        if (value.BiasedExponent == 0) exponent = value.Format.MinSubnormalExponent;
        else
        {
            significand += BigInteger.One << value.Format.FractionBits;
            exponent = value.BiasedExponent - value.Format.ExponentBias - value.Format.FractionBits;
        }
    }
}
