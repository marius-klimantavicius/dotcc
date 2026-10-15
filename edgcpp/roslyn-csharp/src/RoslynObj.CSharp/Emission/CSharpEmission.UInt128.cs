using System.Globalization;
using System.Numerics;

namespace RoslynObj.CSharp;

public sealed partial class CSharpEmitter
{
    private sealed partial class Emission
    {
        private static bool IsUInt128Value(CppType type)
        {
            var effective = type;
            while (type is TypeReference reference) type = reference.UnderlyingType;
            return type is ArithmeticType arithmetic && arithmetic.Arithmetic.ArithmeticKind.Name == "unsigned_integer"
                && arithmetic.Arithmetic.Bits == 128 && arithmetic.Arithmetic.Signed == false
                && type.Size == 16 && type.Alignment == 16 && effective.Size == 16 && effective.Alignment == 16;
        }

        private static bool IsRawUInt128(CppType type)
        {
            while (type is TypeReference reference) type = reference.UnderlyingType;
            return type is ArithmeticType arithmetic && arithmetic.Arithmetic.ArithmeticKind.Name == "unsigned_integer"
                && arithmetic.Arithmetic.Bits == 128;
        }

        private static void ValidateUInt128Value(CppType type)
        {
            if (!IsUInt128Value(type)) throw Limit("Unsigned 128-bit values require captured size/alignment 16/16.", "integer128");
            for (var current = type; ;)
            {
                if (current.Details.IsAtomic) throw Limit("Atomic unsigned 128-bit values are unsupported.", "integer128");
                if (current is QualifiedType qualified)
                {
                    var qualifiers = qualified.Qualification;
                    if (qualified.IsVolatile || qualifiers.IsAtomic || qualifiers.AddressSpace is not (null or 0)
                        || qualifiers.IsShared || qualifiers.IsUpcStrict || qualifiers.IsUpcRelaxed || qualifiers.IsUnaligned)
                        throw Limit("Qualified unsigned 128-bit execution requires a separate adapter.", "integer128");
                }
                if (current is not TypeReference reference) break;
                current = reference.UnderlyingType;
            }
        }

        // UInt128 is currently a value representation. CLR local placement is
        // not evidence for the captured 16-byte addressable storage alignment.
        private static void ValidateValueAddress(CppType type)
        {
            if (IsRawUInt128(type))
                throw Limit("Addressable unsigned 128-bit storage has no exercised 16-byte alignment contract.", "integer128-storage");
        }

        private static bool TryWideIntegerLiteral(BigInteger value, CppType type, out string text)
        {
            text = "";
            if (!IsRawUInt128(type)) return false;
            ValidateUInt128Value(type);
            if (value < 0 || value >= (BigInteger.One << 128))
                throw Limit("An unsigned 128-bit constant is outside its captured value range.", "constant");
            var low = (ulong)(value & ulong.MaxValue);
            var high = (ulong)(value >> 64);
            var lower = low.ToString(CultureInfo.InvariantCulture) + "UL";
            text = high == 0 ? $"((UInt128){lower})"
                : $"(((UInt128){high.ToString(CultureInfo.InvariantCulture)}UL << 64) | (UInt128){lower})";
            return true;
        }
    }
}
