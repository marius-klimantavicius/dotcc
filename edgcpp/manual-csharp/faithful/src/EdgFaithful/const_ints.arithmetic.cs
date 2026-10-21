/*
Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
Exceptions.
See https://edgcpp.org/LICENSE.txt for license information.
SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
*/

namespace EdgFaithful;

// Handwritten counterparts of const_ints.c, in source order. The pinned native
// build selects !INTEGER_VALUE_REPR_IS_A_HOST_INTEGER. See const_ints-batch2.md.
public static unsafe partial class const_ints
{
    public static void add_integer_values(an_integer_value* op_1,
                                          an_integer_value* op_2,
                                          bool is_signed,
                                          bool* err)
    {
        int i;
        long carry = 0;
        bool sign_1 = sign_of(op_1);

        for (i = INT_VALUE_PARTS_PER_INTEGER_VALUE - 1; i >= 0; --i)
        {
            long work;
            work = (long)op_1->part[i] + (long)op_2->part[i] + carry;
            if (work > MAX_UINT_VALUE_PART)
            {
                work -= (long)INT_VALUE_PART_BASE;
                carry = 1;
            }
            else
            {
                carry = 0;
            }
            op_1->part[i] = unchecked((ushort)work);
        }
        if (!is_signed)
        {
            *err = carry != 0;
        }
        else
        {
            // This read of op_2 is after all writes, including when op_1==op_2.
            *err = (sign_1 == sign_of(op_2)) && (sign_1 != sign_of(op_1));
        }
    }

    public static void add_mixed_signed_integer_values(an_integer_value* op_1,
                                                       bool op_1_signed,
                                                       an_integer_value* op_2,
                                                       bool op_2_signed,
                                                       bool* err)
    {
        an_integer_value orig_value;

        orig_value = op_1_signed ? *op_1 : *op_2;
        add_integer_values(op_1, op_2, op_1_signed, err);
        if (op_1_signed != op_2_signed)
        {
            *err = cmp_integer_values(&orig_value, true, op_1, op_1_signed) > 0;
        }
    }

    public static void subtract_mixed_signed_integer_values(an_integer_value* op_1,
                                                            bool op_1_signed,
                                                            an_integer_value* op_2,
                                                            bool op_2_signed,
                                                            bool* err)
    {
        an_integer_value orig_value;

        orig_value = *op_1;
        subtract_integer_values(op_1, op_2, op_1_signed, err);
        if (op_1_signed != op_2_signed)
        {
            bool op_2_sign = op_2_signed && sign_of(op_2);
            int cmp = cmp_integer_values(&orig_value, op_1_signed,
                                          op_1, op_1_signed);
            if (op_2_sign)
            {
                *err = cmp > 0;
            }
            else
            {
                *err = cmp <= 0;
            }
        }
    }

    public static void make_integer_value_mask(an_integer_value* mask, nuint bits)
    {
        set_integer_value(mask, (long)0);
        complement_integer_value(mask);
        shift_right_integer_value(mask,
            unchecked((int)((nuint)BITS_IN_AN_INTEGER_VALUE - bits)),
            false, false);
    }

    public static void sign_extend_integer_value(an_integer_value* value, nuint bits)
    {
        int shift_bits = unchecked((int)((nuint)BITS_IN_AN_INTEGER_VALUE - bits));
        bool err;
        shift_left_integer_value(value, shift_bits, &err);
        shift_right_integer_value(value, shift_bits, true, true);
    }

    // const_ints.c:913-917 captures the caller's fill_value in the macro.
    // An explicit argument carries that local into this C# counterpart. Every
    // call argument is side-effect-free; value remains a pointer to live storage.
    private static ulong get_part(an_integer_value* value, int part_gp,
                                   ushort fill_value)
    {
        return (ulong)((part_gp < 0 ||
                        part_gp >= INT_VALUE_PARTS_PER_INTEGER_VALUE)
            ? fill_value : value->part[part_gp]);
    }

    public static void shift_left_integer_value(an_integer_value* op_1,
                                                int op_2,
                                                bool* err)
    {
        int i;
        int part_offset;
        int first_part_shift;
        int second_part_shift;
        bool overflow = false;

        part_offset = op_2 / (int)BITS_IN_INT_VALUE_PART;
        first_part_shift = op_2 % (int)BITS_IN_INT_VALUE_PART;
        second_part_shift = (int)BITS_IN_INT_VALUE_PART - first_part_shift;
        for (i = 0; i < (int)INT_VALUE_PARTS_PER_INTEGER_VALUE; ++i)
        {
            ulong work;
            ushort fill_value = 0;
            work = op_1->part[i];
            if (i < part_offset && work != 0)
            {
                overflow = true;
            }
            else if (i == part_offset && (work >> second_part_shift) != 0)
            {
                overflow = true;
            }
            work = get_part(op_1, i + part_offset, fill_value) << first_part_shift |
                   get_part(op_1, i + part_offset + 1, fill_value) >> second_part_shift;
            op_1->part[i] = unchecked((ushort)(work & MAX_UINT_VALUE_PART));
        }
        *err = overflow;
    }

    public static void shift_right_integer_value(an_integer_value* op_1,
                                                 int op_2,
                                                 bool is_signed,
                                                 bool sign_extend)
    {
        int i;
        int part_offset;
        int first_part_shift;
        int second_part_shift;
        ushort fill_value;

        part_offset = op_2 / (int)BITS_IN_INT_VALUE_PART;
        first_part_shift = op_2 % (int)BITS_IN_INT_VALUE_PART;
        second_part_shift = (int)BITS_IN_INT_VALUE_PART - first_part_shift;
        if (is_signed && sign_extend)
        {
            fill_value = unchecked((ushort)(-(sign_of(op_1) ? 1 : 0) & MAX_UINT_VALUE_PART));
        }
        else
        {
            fill_value = 0;
        }
        for (i = INT_VALUE_PARTS_PER_INTEGER_VALUE - 1; i >= 0; --i)
        {
            ulong work;
            work = get_part(op_1, i - part_offset, fill_value) >> first_part_shift |
                   get_part(op_1, i - part_offset - 1, fill_value) << second_part_shift;
            op_1->part[i] = unchecked((ushort)(work & MAX_UINT_VALUE_PART));
        }
    }

    public static void subtract_integer_values(an_integer_value* op_1,
                                               an_integer_value* op_2,
                                               bool is_signed,
                                               bool* err)
    {
        int i;
        long borrow = 0;
        bool sign_1 = sign_of(op_1);

        for (i = INT_VALUE_PARTS_PER_INTEGER_VALUE - 1; i >= 0; --i)
        {
            long work;
            work = (long)op_1->part[i] - (long)op_2->part[i] - borrow;
            if (work < 0)
            {
                work += (long)INT_VALUE_PART_BASE;
                borrow = 1;
            }
            else
            {
                borrow = 0;
            }
            op_1->part[i] = unchecked((ushort)work);
        }
        if (!is_signed)
        {
            *err = borrow != 0;
        }
        else
        {
            // Preserve the source's post-write op_2 sign read under aliasing.
            *err = (sign_1 != sign_of(op_2)) && (sign_1 != sign_of(op_1));
        }
    }

    public static void negate_integer_value(an_integer_value* op_1, bool* err)
    {
        an_integer_value result;
        set_integer_value(&result, (long)0);
        subtract_integer_values(&result, op_1, true, err);
        *op_1 = result;
    }
}
