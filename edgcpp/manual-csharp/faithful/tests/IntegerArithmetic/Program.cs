using System;
using System.Runtime.InteropServices;
using EdgFaithful;

// Authored, unexecuted tests. The supervisor owns execution after shared HOLD.
// UInt128/Int128 occur only in test expectations, never in the ported routines.
internal static unsafe class Program
{
    private static int checks;
    private static readonly UInt128 Sign = (UInt128)1 << 127;
    private static readonly UInt128 Max = UInt128.MaxValue;

    private delegate void Binary(an_integer_value* a, an_integer_value* b,
                                  bool isSigned, bool* err);
    private delegate void Mixed(an_integer_value* a, bool aSigned,
                                 an_integer_value* b, bool bSigned, bool* err);

    [StructLayout(LayoutKind.Sequential)]
    private struct GuardedValue
    {
        public ushort before;
        public an_integer_value value;
        public ushort after;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct GuardedBoolean
    {
        public byte before;
        public bool value;
        public byte after;
    }

    private static int Main()
    {
        DistinctArithmetic();
        ExactAliases();
        MixedArithmetic();
        ShiftBoundaries();
        MasksAndSignExtension();
        Negation();
        Console.WriteLine($"IntegerArithmetic: {checks} checks passed.");
        return 0;
    }

    private static void Check(bool condition, string name)
    {
        ++checks;
        if (!condition) throw new InvalidOperationException(name);
    }

    private static an_integer_value FromBits(UInt128 bits)
    {
        an_integer_value result;
        for (int i = 0; i < 8; ++i)
            result.part[i] = unchecked((ushort)(bits >> (112 - i * 16)));
        return result;
    }

    private static UInt128 ReadBits(an_integer_value* value)
    {
        UInt128 result = 0;
        for (int i = 0; i < 8; ++i) result = (result << 16) | value->part[i];
        return result;
    }

    private static UInt128[] Values() =>
    [
        0, 1, 0x7fff, 0x8000, 0xffff, 0x10000,
        ((UInt128)1 << 32) - 1, (UInt128)1 << 32,
        ((UInt128)1 << 48) - 1, (UInt128)1 << 48,
        (UInt128)long.MaxValue, (UInt128)1 << 63, ulong.MaxValue,
        (UInt128)1 << 64, ((UInt128)1 << 80) - 1, (UInt128)1 << 80,
        ((UInt128)1 << 96) - 1, (UInt128)1 << 96,
        ((UInt128)1 << 112) - 1, (UInt128)1 << 112,
        Sign - 1, Sign, Sign + 1, Max - 1, Max,
        ((UInt128)0x123456789abcdef0UL << 64) | 0xfedcba9876543210UL,
        ((UInt128)0xaaaaaaaaaaaaaaaaUL << 64) | 0xaaaaaaaaaaaaaaaaUL,
        ((UInt128)0x5555555555555555UL << 64) | 0x5555555555555555UL
    ];

    private static void RunBinary(string name, Binary operation,
                                   UInt128 a, UInt128 b, bool isSigned,
                                   UInt128 expected, bool expectedError,
                                   bool alias = false)
    {
        for (int initial = 0; initial != 2; ++initial)
        {
            GuardedValue first = new() { before = 0x1357, value = FromBits(a), after = 0x2468 };
            GuardedValue second = new() { before = 0xabcd, value = FromBits(b), after = 0xef01 };
            GuardedBoolean error = new() { before = 0x39, value = initial != 0, after = 0xc6 };
            an_integer_value* other = alias ? &first.value : &second.value;
            operation(&first.value, other, isSigned, &error.value);
            Check(ReadBits(&first.value) == expected, name + " result");
            Check(error.value == expectedError, name + " error overwrite");
            Check(ReadBits(other) == (alias ? expected : b), name + " operand identity/mutation");
            Check(ReadBits(&second.value) == b, name + " independent second storage");
            Check(first.before == 0x1357 && first.after == 0x2468 &&
                  second.before == 0xabcd && second.after == 0xef01, name + " value write bounds");
            Check(error.before == 0x39 && error.after == 0xc6, name + " bool write bounds");
            Check((other == &first.value) == alias, name + " alias partition");
        }
    }

    private static void RunMixed(string name, Mixed operation,
                                  UInt128 a, bool aSigned, UInt128 b, bool bSigned,
                                  UInt128 expected, bool expectedError, bool alias = false)
    {
        for (int initial = 0; initial != 2; ++initial)
        {
            an_integer_value first = FromBits(a);
            an_integer_value second = FromBits(b);
            bool error = initial != 0;
            an_integer_value* other = alias ? &first : &second;
            operation(&first, aSigned, other, bSigned, &error);
            Check(ReadBits(&first) == expected, name + " result");
            Check(error == expectedError, name + " replacement error");
            Check(ReadBits(other) == (alias ? expected : b), name + " operand identity/mutation");
            Check(ReadBits(&second) == b, name + " independent second storage");
            Check((other == &first) == alias, name + " alias partition");
        }
    }

    private static void DistinctArithmetic()
    {
        foreach (UInt128 a in Values())
        foreach (UInt128 b in Values())
        {
            UInt128 sum = unchecked(a + b);
            UInt128 difference = unchecked(a - b);
            bool aNegative = (a & Sign) != 0;
            bool bNegative = (b & Sign) != 0;
            bool signedAddOverflow = aNegative == bNegative && ((sum & Sign) != 0) != aNegative;
            bool signedSubtractOverflow = aNegative != bNegative && ((difference & Sign) != 0) != aNegative;
            RunBinary("unsigned add", const_ints.add_integer_values, a, b, false, sum, sum < a);
            RunBinary("signed add", const_ints.add_integer_values, a, b, true, sum, signedAddOverflow);
            RunBinary("unsigned subtract", const_ints.subtract_integer_values, a, b, false, difference, a < b);
            RunBinary("signed subtract", const_ints.subtract_integer_values, a, b, true,
                      difference, signedSubtractOverflow);
        }
    }

    private static void ExactAliases()
    {
        // The active native branch rereads op_2 after mutating op_1. These
        // error expectations deliberately preserve the resulting native quirks.
        RunBinary("alias add zero", const_ints.add_integer_values, 0, 77, true, 0, false, true);
        RunBinary("alias add one", const_ints.add_integer_values, 1, 77, true, 2, false, true);
        RunBinary("alias signed maximum add", const_ints.add_integer_values,
                  Sign - 1, 77, true, Max - 1, false, true);
        RunBinary("alias signed minimum add", const_ints.add_integer_values,
                  Sign, 77, true, 0, false, true);
        RunBinary("alias minus one add", const_ints.add_integer_values,
                  Max, 77, true, Max - 1, false, true);
        RunBinary("alias unsigned maximum add", const_ints.add_integer_values,
                  Max, 77, false, Max - 1, true, true);
        RunBinary("alias negative subtract", const_ints.subtract_integer_values,
                  Max, 77, true, 0, true, true);
        RunBinary("alias minimum subtract", const_ints.subtract_integer_values,
                  Sign, 77, true, 0, true, true);
        RunBinary("alias positive subtract", const_ints.subtract_integer_values,
                  Sign - 1, 77, true, 0, false, true);
        RunBinary("alias unsigned subtract", const_ints.subtract_integer_values,
                  Max, 77, false, 0, false, true);
    }

    private static void MixedArithmetic()
    {
        RunMixed("mixed add equal unsigned flags", const_ints.add_mixed_signed_integer_values,
                 Max, false, 1, false, 0, true);
        RunMixed("mixed add equal signed flags", const_ints.add_mixed_signed_integer_values,
                 Sign - 1, true, 1, true, Sign, true);
        RunMixed("mixed add signed zero unsigned zero", const_ints.add_mixed_signed_integer_values,
                 0, true, 0, false, 0, false);
        RunMixed("mixed add signed maximum", const_ints.add_mixed_signed_integer_values,
                 Sign - 1, true, 1, false, Sign, true);
        RunMixed("mixed add signed minimum", const_ints.add_mixed_signed_integer_values,
                 Sign, true, 1, false, Sign + 1, false);
        RunMixed("mixed add unsigned plus negative", const_ints.add_mixed_signed_integer_values,
                 2, false, Max, true, 1, false);
        RunMixed("mixed add native negative underflow flag", const_ints.add_mixed_signed_integer_values,
                 0, false, Max, true, Max, false);
        RunMixed("mixed add signed negative plus unsigned maximum", const_ints.add_mixed_signed_integer_values,
                 Max, true, Max, false, Max - 1, true);
        RunMixed("mixed add alias original signed copy", const_ints.add_mixed_signed_integer_values,
                 Sign - 1, true, 77, false, Max - 1, true, true);
        RunMixed("mixed add alias original right copy", const_ints.add_mixed_signed_integer_values,
                 Max, false, 77, true, Max - 1, false, true);
        RunMixed("mixed add alias matching signed flags", const_ints.add_mixed_signed_integer_values,
                 Sign, true, 77, true, 0, false, true);

        RunMixed("mixed subtract equal unsigned flags", const_ints.subtract_mixed_signed_integer_values,
                 0, false, 1, false, Max, true);
        RunMixed("mixed subtract equal signed flags", const_ints.subtract_mixed_signed_integer_values,
                 Sign, true, 1, true, Sign - 1, true);
        RunMixed("mixed subtract positive unsigned second", const_ints.subtract_mixed_signed_integer_values,
                 2, true, 1, false, 1, false);
        RunMixed("mixed subtract unsigned underflow", const_ints.subtract_mixed_signed_integer_values,
                 0, false, 1, true, Max, true);
        RunMixed("mixed subtract signed underflow", const_ints.subtract_mixed_signed_integer_values,
                 Sign, true, 1, false, Sign - 1, true);
        RunMixed("mixed subtract negative second", const_ints.subtract_mixed_signed_integer_values,
                 2, false, Max, true, 3, false);
        RunMixed("mixed subtract negative second wraps", const_ints.subtract_mixed_signed_integer_values,
                 Max, false, Max, true, 0, true);
        // Equality deliberately takes source cmp <= 0 and reports an error.
        RunMixed("mixed subtract unsigned zero equality", const_ints.subtract_mixed_signed_integer_values,
                 2, true, 0, false, 2, true);
        RunMixed("mixed subtract signed zero equality", const_ints.subtract_mixed_signed_integer_values,
                 2, false, 0, true, 2, true);
        RunMixed("mixed subtract alias post-write negative sign", const_ints.subtract_mixed_signed_integer_values,
                 Max, false, 77, true, 0, false, true);
        RunMixed("mixed subtract alias original signed negative", const_ints.subtract_mixed_signed_integer_values,
                 Max, true, 77, false, 0, true, true);
        RunMixed("mixed subtract alias original zero", const_ints.subtract_mixed_signed_integer_values,
                 0, false, 77, true, 0, true, true);
        RunMixed("mixed subtract alias equal signed flags", const_ints.subtract_mixed_signed_integer_values,
                 Max, true, 77, true, 0, true, true);
    }

    private static void ShiftBoundaries()
    {
        int[] counts = [0, 1, 15, 16, 17, 31, 32, 33, 47, 48, 49, 63, 64, 65,
                        79, 80, 81, 95, 96, 97, 111, 112, 113, 126, 127];
        foreach (UInt128 bits in Values())
        foreach (int count in counts)
        {
            for (int initial = 0; initial != 2; ++initial)
            {
                an_integer_value value = FromBits(bits);
                bool err = initial != 0;
                const_ints.shift_left_integer_value(&value, count, &err);
                Check(ReadBits(&value) == unchecked(bits << count), $"left {count} result");
                bool expectedError = count != 0 && (bits >> (128 - count)) != 0;
                Check(err == expectedError, $"left {count} lost bits");
            }
            for (int flags = 0; flags != 4; ++flags)
            {
                bool isSigned = (flags & 1) != 0;
                bool extend = (flags & 2) != 0;
                an_integer_value value = FromBits(bits);
                const_ints.shift_right_integer_value(&value, count, isSigned, extend);
                UInt128 expected = isSigned && extend
                    ? unchecked((UInt128)(unchecked((Int128)bits) >> count)) : bits >> count;
                Check(ReadBits(&value) == expected, $"right {count} flags {flags}");
            }
        }
    }

    private static void MasksAndSignExtension()
    {
        // Widths are deliberately bounded to 1..128. INIT-001 widths and
        // negative/out-of-contract shift counts have no portable test oracle.
        for (int bits = 1; bits <= 128; ++bits)
        {
            UInt128 mask = bits == 128 ? Max : ((UInt128)1 << bits) - 1;
            an_integer_value value = FromBits(0x12345678);
            const_ints.make_integer_value_mask(&value, (nuint)bits);
            Check(ReadBits(&value) == mask, $"mask width {bits}");
            UInt128 fieldSign = (UInt128)1 << (bits - 1);
            UInt128[] inputs = [0, 1, fieldSign - 1, fieldSign, mask, Max,
                ((UInt128)0x123456789abcdef0UL << 64) | 0xfedcba9876543210UL];
            foreach (UInt128 input in inputs)
            {
                UInt128 field = input & mask;
                UInt128 expected = (field & fieldSign) != 0 ? field | ~mask : field;
                value = FromBits(input);
                const_ints.sign_extend_integer_value(&value, (nuint)bits);
                Check(ReadBits(&value) == expected, $"extend width {bits}");
            }
        }
    }

    private static void Negation()
    {
        foreach (UInt128 bits in Values())
        for (int initial = 0; initial != 2; ++initial)
        {
            an_integer_value value = FromBits(bits);
            bool err = initial != 0;
            const_ints.negate_integer_value(&value, &err);
            Check(ReadBits(&value) == unchecked((UInt128)0 - bits), "negate result");
            Check(err == (bits == Sign), "negate signed minimum error");
        }
    }
}
