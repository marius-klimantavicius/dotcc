/*
Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
Exceptions. See https://edgcpp.org/LICENSE.txt for license information.
SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
*/

using System.Runtime.InteropServices;

namespace EdgFaithful;

// targ_def.h:1673-1675. Pinned Linux x86-64 configuration only.
// Translation/review pending; the fixed buffer is inline storage, not an owner.
[StructLayout(LayoutKind.Sequential, Pack = 2, Size = 16)]
public unsafe struct an_integer_value
{
    public fixed ushort part[const_ints.INT_VALUE_PARTS_PER_INTEGER_VALUE];
}

public static unsafe partial class const_ints
{
    // Source-derived configuration, not selectable alternative implementations.
    public const bool INTEGER_VALUE_REPR_IS_A_HOST_INTEGER = false;
    public const bool USE_BOOL_FOR_BOOLEAN_IN_CPLUSPLUS = true;
    public const bool FIXED_POINT_ALLOWED = true;
    public const bool INT128_EXTENSIONS_ALLOWED = true;
    public const bool TRUE = true;
    public const bool FALSE = false;
    public const int CHAR_BIT = 8;
    public const int INTERNAL_TARG_CHAR_BIT = 8;
    public const int MAX_SIZEOF_LARGEST_INTEGER = 16;
    public const int MAX_SIZEOF_LARGEST_FIXED_POINT = 8;
    public const int INTEGER_VALUE_REPRESENTATION_SIZE = MAX_SIZEOF_LARGEST_INTEGER;
    public const int SIZEOF_INT_VALUE_PART = sizeof(ushort);
    public const int BITS_IN_INT_VALUE_PART = SIZEOF_INT_VALUE_PART * CHAR_BIT;
    public const int INT_VALUE_PARTS_PER_INTEGER_VALUE =
        (INTEGER_VALUE_REPRESENTATION_SIZE * INTERNAL_TARG_CHAR_BIT) /
        (SIZEOF_INT_VALUE_PART * CHAR_BIT);
    public const int BITS_IN_AN_INTEGER_VALUE =
        BITS_IN_INT_VALUE_PART * INT_VALUE_PARTS_PER_INTEGER_VALUE;
    public const int BITS_IN_HOST_LARGE_INTEGER = sizeof(long) * CHAR_BIT;
    public const int MAX_UINT_VALUE_PART = 0xffff;
    public const int SIGN_BIT_INT_VALUE_PART = 0x8000;
    public const ulong INT_VALUE_PART_BASE = (ulong)MAX_UINT_VALUE_PART + 1;

    // const_ints.h:38-40. Call sites in this closure pass side-effect-free
    // locals/constants and a shift of 16, strictly within the host width.
    public static long signed_shift_right(long value, int bits)
    {
        return unchecked((long)(((ulong)value >> bits) |
            ((value < 0) ? ~(~(ulong)0 >> bits) : 0)));
    }

    // const_ints.h:47-48. A macro lvalue is represented by its live address;
    // callers read storage at the source macro's evaluation point.
    public static bool sign_of(an_integer_value* value)
    {
        return (value->part[0] & SIGN_BIT_INT_VALUE_PART) != 0;
    }
}
