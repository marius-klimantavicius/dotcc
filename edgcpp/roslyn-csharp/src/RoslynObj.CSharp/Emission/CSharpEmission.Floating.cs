using System.Globalization;
using System.Numerics;
using RoslynObj;

namespace RoslynObj.CSharp;

public sealed partial class CSharpEmitter
{
    private sealed partial class Emission
    {
        private string FloatingLiteral(FloatingValue floating, CppType type)
        {
            var name = TypeName(type);
            if (name is not ("float" or "double")) throw Limit("A floating value has a nonfloating selected type.", "floating");
            var exact = floating.Value;
            if (exact.Radix != 2) throw Limit("Floating constants require exact binary components.", "floating");
            var precision = name == "float" ? 24 : 53;
            var exponentBits = name == "float" ? 8 : 11;
            var bias = (1 << (exponentBits - 1)) - 1;
            var fractionBits = precision - 1;
            var totalBits = name == "float" ? 32 : 64;
            var sign = exact.IsNegative ? BigInteger.One << (totalBits - 1) : BigInteger.Zero;
            var exponentMask = (BigInteger.One << exponentBits) - 1;
            BigInteger bits;
            if (exact.Class == FloatingPointClass.Infinity) bits = sign | exponentMask << fractionBits;
            else if (exact.Class == FloatingPointClass.NaN)
            {
                if (exact.NaNPayload is not { } payload || exact.IsQuietNaN is not { } quiet || payload < 0 || payload >= BigInteger.One << fractionBits)
                    throw Limit("NaN payload/quiet state cannot be represented exactly in its selected IEEE format.", "floating");
                if (quiet) payload |= BigInteger.One << (fractionBits - 1);
                else payload &= ~(BigInteger.One << (fractionBits - 1));
                if (payload.IsZero) throw Limit("A signaling NaN has no retained nonzero payload.", "floating");
                bits = sign | exponentMask << fractionBits | payload;
            }
            else if (exact.Significand.IsZero) bits = sign;
            else
            {
                if (exact.Significand.Sign < 0) throw Limit("Exact floating significand has an unsupported sign representation.", "floating");
                var length = checked((int)exact.Significand.GetBitLength());
                var unbiased = exact.Exponent + length - 1;
                if (unbiased >= 1 - bias)
                {
                    if (unbiased > bias) throw Limit("Finite floating value exceeds its selected IEEE exponent range.", "floating");
                    var significand = ExactShift(exact.Significand, precision - length);
                    var fraction = significand - (BigInteger.One << fractionBits);
                    bits = sign | (unbiased + bias) << fractionBits | fraction;
                }
                else
                {
                    var fraction = ExactShift(exact.Significand, checked((int)(exact.Exponent - (1 - bias - fractionBits))));
                    if (fraction <= 0 || fraction >= BigInteger.One << fractionBits)
                        throw Limit("Subnormal floating value does not fit its selected IEEE format.", "floating");
                    bits = sign | fraction;
                }
            }
            // Ordinary finite values use the shortest round-trip C# literal. Exact
            // bit constructors retain signed zero and supported special payloads.
            if (name == "float")
            {
                var raw = checked((uint)bits);
                var value = BitConverter.Int32BitsToSingle(unchecked((int)raw));
                if (float.IsFinite(value) && value != 0) return value.ToString("R", CultureInfo.InvariantCulture) + "F";
                return $"BitConverter.Int32BitsToSingle(unchecked((int)0x{raw:x8}U))";
            }
            else
            {
                var raw = checked((ulong)bits);
                var value = BitConverter.Int64BitsToDouble(unchecked((long)raw));
                if (double.IsFinite(value) && value != 0) return value.ToString("R", CultureInfo.InvariantCulture) + "D";
                return $"BitConverter.Int64BitsToDouble(unchecked((long)0x{raw:x16}UL))";
            }
        }

        private static BigInteger ExactShift(BigInteger value, int shift)
        {
            if (shift >= 0) return value << shift;
            var mask = (BigInteger.One << -shift) - 1;
            if ((value & mask) != 0) throw Limit("The selected floating format would round retained exact components.", "floating");
            return value >> -shift;
        }
    }
}
