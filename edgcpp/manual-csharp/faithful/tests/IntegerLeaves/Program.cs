using System;
using System.Globalization;
using System.Runtime.InteropServices;
using EdgFaithful;

// Authored for the supervisor to execute after all shared writers HOLD.
// UInt128/Int128 are test expectations only; the port uses the native part array.
internal static unsafe class Program
{
    private static int checks;

    [StructLayout(LayoutKind.Sequential)]
    private struct AlignmentProbe
    {
        public byte prefix;
        public an_integer_value value;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct GuardedValue
    {
        public ushort before;
        public an_integer_value value;
        public ushort after;
    }

    private static int Main(string[] args)
    {
        if (args.Length == 1 && args[0] == "--stream") return Stream();
        if (args.Length == 1 && args[0] == "--abi") return Abi();
        LayoutAndStorage();
        SetHostValues();
        CopyValues();
        CompareValues();
        ConvertValues();
        BitwiseValues();
        IncrementValues();
        SignedShiftMacro();
        Console.WriteLine($"IntegerLeaves: {checks} checks passed.");
        return 0;
    }

    // Test-only transport shared with tools/integer_oracle.cpp. No validation
    // result is implied by implementing this adapter; the supervisor runs it.
    private static int Stream()
    {
        string? line;
        while ((line = Console.ReadLine()) != null)
        {
            string[] fields = line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            if (fields.Length == 0) continue;
            if (fields.Length != 7 || fields[5].Length != 32 || fields[6].Length != 32)
                return 2;
            int alias = int.Parse(fields[1], CultureInfo.InvariantCulture);
            bool signed1 = int.Parse(fields[2], CultureInfo.InvariantCulture) != 0;
            bool signed2 = int.Parse(fields[3], CultureInfo.InvariantCulture) != 0;
            int count = int.Parse(fields[4], CultureInfo.InvariantCulture);
            an_integer_value a = FromBits(UInt128.Parse(fields[5], NumberStyles.AllowHexSpecifier,
                                                        CultureInfo.InvariantCulture));
            an_integer_value b = FromBits(UInt128.Parse(fields[6], NumberStyles.AllowHexSpecifier,
                                                        CultureInfo.InvariantCulture));
            an_integer_value* other = alias != 0 ? &a : &b;
            bool err = false;
            long host = 0;
            int comparison = 0;
            switch (fields[0])
            {
                case "add": const_ints.add_integer_values(&a, other, signed1, &err); break;
                case "add-mixed": const_ints.add_mixed_signed_integer_values(&a, signed1, other, signed2, &err); break;
                case "subtract": const_ints.subtract_integer_values(&a, other, signed1, &err); break;
                case "subtract-mixed": const_ints.subtract_mixed_signed_integer_values(&a, signed1, other, signed2, &err); break;
                case "left": const_ints.shift_left_integer_value(&a, count, &err); break;
                case "right": const_ints.shift_right_integer_value(&a, count, signed1, signed2); break;
                case "mask": const_ints.make_integer_value_mask(&a, unchecked((nuint)count)); break;
                case "extend": const_ints.sign_extend_integer_value(&a, unchecked((nuint)count)); break;
                case "negate": const_ints.negate_integer_value(&a, &err); break;
                case "increment": const_ints.incr_integer_value(&a); break;
                case "or": const_ints.or_integer_values(&a, other); break;
                case "and": const_ints.and_integer_values(&a, other); break;
                case "xor": const_ints.xor_integer_values(&a, other); break;
                case "complement": const_ints.complement_integer_value(&a); break;
                case "compare": comparison = const_ints.cmp_integer_values(&a, signed1, other, signed2); break;
                case "extract": const_ints.conv_integer_value_to_host_large_integer(&a, signed1, &host, &err); break;
                case "set-signed":
                    host = unchecked((long)(ulong)ReadBits(other));
                    const_ints.set_integer_value(&a, host);
                    host = 0;
                    break;
                case "set-unsigned":
                    const_ints.set_unsigned_integer_value(&a, unchecked((ulong)ReadBits(other)));
                    break;
                case "copy-signed": const_ints.set_integer_value(&a, other); break;
                case "copy-unsigned": const_ints.set_unsigned_integer_value(&a, other); break;
                default: return 3;
            }
            Console.WriteLine(string.Join(" ",
                ReadBits(&a).ToString("x32", CultureInfo.InvariantCulture),
                ReadBits(other).ToString("x32", CultureInfo.InvariantCulture),
                err ? "1" : "0", comparison.ToString(CultureInfo.InvariantCulture),
                unchecked((ulong)host).ToString("x16", CultureInfo.InvariantCulture),
                other == &a ? "1" : "0"));
        }
        return 0;
    }

    private static int Abi()
    {
        AlignmentProbe probe;
        long alignment = (byte*)&probe.value - (byte*)&probe;
        Console.WriteLine($"{{\"integer_size\":{sizeof(an_integer_value)},\"integer_alignment\":{alignment}," +
            $"\"part_size\":{sizeof(ushort)},\"parts\":{const_ints.INT_VALUE_PARTS_PER_INTEGER_VALUE}," +
            $"\"bits\":{const_ints.BITS_IN_AN_INTEGER_VALUE},\"host_signed_size\":{sizeof(long)}," +
            $"\"host_unsigned_size\":{sizeof(ulong)},\"sizeof_t_size\":{sizeof(ulong)}," +
            $"\"boolean_size\":{sizeof(bool)},\"char_bit\":{const_ints.CHAR_BIT}," +
            "\"host_repr\":0,\"fixed_point\":1," +
            $"\"max_fixed_point_size\":{const_ints.MAX_SIZEOF_LARGEST_FIXED_POINT},\"checking\":1}}");
        return 0;
    }

    private static void Check(bool condition, string label)
    {
        ++checks;
        if (!condition) throw new InvalidOperationException(label);
    }

    private static void CheckBits(an_integer_value* value, UInt128 expected, string label)
    {
        Check(ReadBits(value) == expected, label);
    }

    private static UInt128 ReadBits(an_integer_value* value)
    {
        UInt128 result = 0;
        for (int i = 0; i < 8; ++i) result = (result << 16) | value->part[i];
        return result;
    }

    private static an_integer_value FromBits(UInt128 value)
    {
        an_integer_value result;
        for (int i = 0; i < 8; ++i)
            result.part[i] = unchecked((ushort)(value >> ((7 - i) * 16)));
        return result;
    }

    private static UInt128[] Values()
    {
        return new UInt128[]
        {
            0, 1, 0x7fff, 0x8000, 0xffff, 0x10000,
            0x7fffffff, 0x80000000, 0xffffffff, (UInt128)1 << 32,
            ((UInt128)1 << 48) - 1, (UInt128)1 << 48,
            (UInt128)long.MaxValue, (UInt128)1 << 63,
            (UInt128)ulong.MaxValue, (UInt128)1 << 64,
            ((UInt128)1 << 80) - 1, (UInt128)1 << 80,
            ((UInt128)1 << 96) - 1, (UInt128)1 << 96,
            ((UInt128)1 << 112) - 1, (UInt128)1 << 112,
            ((UInt128)1 << 127) - 1, (UInt128)1 << 127,
            UInt128.MaxValue - (UInt128)long.MaxValue,
            UInt128.MaxValue - 1, UInt128.MaxValue,
            ((UInt128)0x123456789abcdef0UL << 64) | 0xfedcba9876543210UL,
            ((UInt128)0xaaaaaaaaaaaaaaaaUL << 64) | 0xaaaaaaaaaaaaaaaaUL,
            ((UInt128)0x5555555555555555UL << 64) | 0x5555555555555555UL
        };
    }

    private static void LayoutAndStorage()
    {
        Check(IntPtr.Size == 8, "pinned 64-bit host");
        Check(BitConverter.IsLittleEndian, "pinned little-endian host");
        Check(sizeof(bool) == 1, "a_boolean storage");
        Check(sizeof(ushort) == 2 && sizeof(long) == 8 && sizeof(ulong) == 8,
              "native scalar widths");
        Check(sizeof(an_integer_value) == 16, "integer size");
        AlignmentProbe probe;
        Check((byte*)&probe.value - (byte*)&probe == 2, "integer alignment");
        an_integer_value value = FromBits(((UInt128)0x123456789abcdef0UL << 64) |
                                          0xfedcba9876543210UL);
        Check((byte*)value.part - (byte*)&value == 0, "part offset");
        Check(value.part[0] == 0x1234 && value.part[7] == 0x3210, "high part first");
        Check((byte*)&value.part[1] - (byte*)&value.part[0] == 2, "part stride");
        an_integer_value copy = value;
        copy.part[0] = 0;
        Check(value.part[0] == 0x1234, "struct copy owns independent inline storage");
        Check(const_ints.INT_VALUE_PART_BASE == 65536UL &&
              const_ints.BITS_IN_AN_INTEGER_VALUE == 128, "pinned source constants");
    }

    private static void SetHostValues()
    {
        long[] signedValues =
        {
            long.MinValue, long.MinValue + 1, -0x100000001L, -0x10000L,
            -0x8001L, -1, 0, 1, 0x7fff, 0x8000, 0xffff, 0x10000,
            0xffffffffL, 0x100000000L, long.MaxValue - 1, long.MaxValue
        };
        foreach (long input in signedValues)
        {
            GuardedValue storage;
            storage.before = 0x1234;
            storage.after = 0xabcd;
            const_ints.set_integer_value(&storage.value, input);
            CheckBits(&storage.value, unchecked((UInt128)(Int128)input), $"set signed {input}");
            Check(storage.before == 0x1234 && storage.after == 0xabcd, "set signed bounds");
        }
        ulong[] unsignedValues =
        {
            0, 1, 0x7fff, 0x8000, 0xffff, 0x10000, 0xffffffff, 0x100000000,
            (ulong)long.MaxValue, 1UL << 63, ulong.MaxValue - 1, ulong.MaxValue
        };
        foreach (ulong input in unsignedValues)
        {
            GuardedValue storage;
            storage.before = 0x1234;
            storage.after = 0xabcd;
            const_ints.set_unsigned_integer_value(&storage.value, input);
            CheckBits(&storage.value, input, $"set unsigned {input}");
            Check(storage.before == 0x1234 && storage.after == 0xabcd, "set unsigned bounds");
        }
    }

    private static void CopyValues()
    {
        foreach (UInt128 bits in Values())
        {
            an_integer_value* slots = stackalloc an_integer_value[2];
            slots[0] = FromBits(bits);
            slots[1] = FromBits(~bits);
            const_ints.set_integer_value(&slots[1], &slots[0]);
            CheckBits(&slots[0], bits, "signed copy source lower address");
            CheckBits(&slots[1], bits, "signed copy destination higher address");
            slots[0] = FromBits(~bits);
            const_ints.set_integer_value(&slots[0], &slots[1]);
            CheckBits(&slots[0], bits, "signed copy destination lower address");
            CheckBits(&slots[1], bits, "signed copy source higher address");
            const_ints.set_integer_value(&slots[0], &slots[0]);
            CheckBits(&slots[0], bits, "signed copy exact alias");
            slots[1] = FromBits(~bits);
            const_ints.set_unsigned_integer_value(&slots[1], &slots[0]);
            CheckBits(&slots[0], bits, "unsigned copy source lower address");
            CheckBits(&slots[1], bits, "unsigned copy destination higher address");
            slots[0] = FromBits(~bits);
            const_ints.set_unsigned_integer_value(&slots[0], &slots[1]);
            CheckBits(&slots[0], bits, "unsigned copy destination lower address");
            CheckBits(&slots[1], bits, "unsigned copy source higher address");
            const_ints.set_unsigned_integer_value(&slots[0], &slots[0]);
            CheckBits(&slots[0], bits, "unsigned copy exact alias");
        }
    }

    private static int ExpectedComparison(UInt128 left, bool leftSigned,
                                           UInt128 right, bool rightSigned)
    {
        bool leftNegative = leftSigned && unchecked((Int128)left) < 0;
        bool rightNegative = rightSigned && unchecked((Int128)right) < 0;
        if (leftNegative != rightNegative) return leftNegative ? -1 : 1;
        return left.CompareTo(right);
    }

    private static void CompareValues()
    {
        foreach (UInt128 leftBits in Values())
        foreach (UInt128 rightBits in Values())
        for (int flags = 0; flags != 4; ++flags)
        {
            bool leftSigned = (flags & 1) != 0;
            bool rightSigned = (flags & 2) != 0;
            an_integer_value left = FromBits(leftBits);
            an_integer_value right = FromBits(rightBits);
            int expected = ExpectedComparison(leftBits, leftSigned, rightBits, rightSigned);
            Check(const_ints.cmp_integer_values(&left, leftSigned, &right, rightSigned) == expected,
                  "compare distinct operands");
            Check(const_ints.cmp_integer_values(&right, rightSigned, &left, leftSigned) == -expected,
                  "compare operand reversal");
            CheckBits(&left, leftBits, "compare left remains intact");
            CheckBits(&right, rightBits, "compare right remains intact");
            Check(const_ints.cmp_integer_values(&left, leftSigned, &left, rightSigned) ==
                  ExpectedComparison(leftBits, leftSigned, leftBits, rightSigned),
                  "compare exact alias with all signedness pairs");
        }
        // Exercise the first differing part and both break arms at every index.
        for (int part = 0; part < 8; ++part)
        {
            an_integer_value left = FromBits(0);
            an_integer_value right = FromBits((UInt128)1 << ((7 - part) * 16));
            Check(const_ints.cmp_integer_values(&left, false, &right, false) == -1,
                  $"compare less at part {part}");
            Check(const_ints.cmp_integer_values(&right, false, &left, false) == 1,
                  $"compare greater at part {part}");
        }
    }

    private static void ConvertValues()
    {
        foreach (UInt128 bits in Values())
        for (int signedFlag = 0; signedFlag < 2; ++signedFlag)
        {
            bool signed = signedFlag != 0;
            an_integer_value input = FromBits(bits);
            long value = 0x12345678;
            Int128 signedBits = unchecked((Int128)bits);
            bool expectedError = signed
                ? signedBits < long.MinValue || signedBits > long.MaxValue
                : bits > ulong.MaxValue;
            bool err = !expectedError;
            const_ints.conv_integer_value_to_host_large_integer(&input, signed, &value, &err);
            Check(value == unchecked((long)(ulong)bits), "conversion returns low host bits even on overflow");
            Check(err == expectedError, "conversion overwrites error flag");
            CheckBits(&input, bits, "conversion input remains intact");
        }
        // Each discarded part independently triggers overflow without a sign mismatch.
        for (int part = 0; part < 4; ++part)
        {
            an_integer_value input = FromBits(0);
            input.part[part] = 1;
            long value;
            bool err;
            const_ints.conv_integer_value_to_host_large_integer(&input, false, &value, &err);
            Check(err && value == 0, $"discarded positive part {part}");
            input = FromBits(UInt128.MaxValue);
            input.part[part] = 0xfffe;
            const_ints.conv_integer_value_to_host_large_integer(&input, true, &value, &err);
            Check(err && value == -1, $"discarded negative part {part}");
        }
    }

    private static void BitwiseValues()
    {
        foreach (UInt128 leftBits in Values())
        {
            an_integer_value complemented = FromBits(leftBits);
            const_ints.complement_integer_value(&complemented);
            CheckBits(&complemented, ~leftBits, "complement");
            const_ints.complement_integer_value(&complemented);
            CheckBits(&complemented, leftBits, "complement twice");
            for (int operation = 0; operation < 3; ++operation)
            {
                foreach (UInt128 rightBits in Values())
                {
                    UInt128 expected = operation == 0 ? leftBits | rightBits
                        : operation == 1 ? leftBits & rightBits : leftBits ^ rightBits;
                    an_integer_value left = FromBits(leftBits);
                    an_integer_value right = FromBits(rightBits);
                    ApplyBitwise(operation, &left, &right);
                    CheckBits(&left, expected, "bitwise result in first operand");
                    CheckBits(&right, rightBits, "bitwise second operand remains intact");
                    left = FromBits(leftBits);
                    right = FromBits(rightBits);
                    ApplyBitwise(operation, &right, &left);
                    CheckBits(&right, expected, "bitwise result in reversed first operand");
                    CheckBits(&left, leftBits, "bitwise reversed second operand remains intact");
                }
                an_integer_value alias = FromBits(leftBits);
                ApplyBitwise(operation, &alias, &alias);
                CheckBits(&alias, operation == 2 ? 0 : leftBits, "bitwise exact operand/result alias");
            }
        }
    }

    private static void ApplyBitwise(int operation, an_integer_value* left, an_integer_value* right)
    {
        if (operation == 0) const_ints.or_integer_values(left, right);
        else if (operation == 1) const_ints.and_integer_values(left, right);
        else const_ints.xor_integer_values(left, right);
    }

    private static void IncrementValues()
    {
        foreach (UInt128 bits in Values())
        {
            GuardedValue storage;
            storage.before = 0x1234;
            storage.after = 0xabcd;
            storage.value = FromBits(bits);
            const_ints.incr_integer_value(&storage.value);
            CheckBits(&storage.value, unchecked(bits + 1), "increment wraps through 128 bits");
            Check(storage.before == 0x1234 && storage.after == 0xabcd, "increment bounds");
        }
        for (int partBoundary = 1; partBoundary < 8; ++partBoundary)
        {
            UInt128 next = (UInt128)1 << (partBoundary * 16);
            an_integer_value value = FromBits(next - 1);
            const_ints.incr_integer_value(&value);
            CheckBits(&value, next, $"increment carry across {partBoundary} parts");
        }
    }

    private static void SignedShiftMacro()
    {
        long[] values = { long.MinValue, -65537, -1, 0, 1, 65536, long.MaxValue };
        foreach (long value in values)
        {
            // Defined source shifts only. C# shift-count masking is not a native contract.
            for (int bits = 0; bits < 64; ++bits)
                Check(const_ints.signed_shift_right(value, bits) == value >> bits,
                      $"signed shift {value}/{bits}");
        }
    }
}
