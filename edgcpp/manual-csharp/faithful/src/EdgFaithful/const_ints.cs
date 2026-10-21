/*
Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
Exceptions. See https://edgcpp.org/LICENSE.txt for license information.
SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
*/

using a_boolean = System.Boolean;
using a_host_large_integer = System.Int64;
using a_host_large_unsigned = System.UInt64;
using an_int_value_part = System.UInt16;
using sizeof_t = System.UInt64;

namespace EdgFaithful;

// Manual port of the enabled array-representation blocks of const_ints.c.
// Translated/review-pending. See ledgers/const_ints-batch1.md and .json.
public static unsafe partial class const_ints
{
    // const_ints.c:33-47
    public static void set_integer_value(an_integer_value* intval,
                                         a_host_large_integer value)
    {
        int i;
        a_host_large_integer this_part;

        for (i = INT_VALUE_PARTS_PER_INTEGER_VALUE - 1; i >= 0; --i)
        {
            this_part = value & MAX_UINT_VALUE_PART;
            intval->part[i] = unchecked((an_int_value_part)this_part);
            value = signed_shift_right(value, BITS_IN_INT_VALUE_PART);
        }
    }

    // const_ints.c:50-59. C++ const-reference parameter retains its address.
    public static void set_integer_value(an_integer_value* intval,
                                         an_integer_value* value)
    {
        for (uint i = 0; i < INT_VALUE_PARTS_PER_INTEGER_VALUE; ++i)
        {
            intval->part[i] = value->part[i];
        }
    }

    // const_ints.c:62-76
    public static void set_unsigned_integer_value(an_integer_value* intval,
                                                  a_host_large_unsigned value)
    {
        int i;
        a_host_large_integer this_part;

        for (i = INT_VALUE_PARTS_PER_INTEGER_VALUE - 1; i >= 0; --i)
        {
            this_part = unchecked((a_host_large_integer)(value & MAX_UINT_VALUE_PART));
            intval->part[i] = unchecked((an_int_value_part)this_part);
            value = value >> BITS_IN_INT_VALUE_PART;
        }
    }

    // const_ints.c:79-88. C++ const-reference parameter retains its address.
    public static void set_unsigned_integer_value(an_integer_value* intval,
                                                  an_integer_value* value)
    {
        for (uint i = 0; i < INT_VALUE_PARTS_PER_INTEGER_VALUE; ++i)
        {
            intval->part[i] = value->part[i];
        }
    }

    // const_ints.c:91-142
    public static void conv_integer_value_to_host_large_integer(
        an_integer_value* intval,
        a_boolean is_signed,
        a_host_large_integer* value,
        a_boolean* err)
    {
        int i;
        sizeof_t bits_so_far = 0;
        sizeof_t bits_discarded = BITS_IN_AN_INTEGER_VALUE -
                                 (sizeof(a_host_large_integer) * CHAR_BIT);
        an_int_value_part this_part;
        an_int_value_part empty_bits;
        a_host_large_unsigned result = 0;
        a_boolean overflow = FALSE;
        a_boolean is_negative;

        is_negative = sign_of(intval);
        if (is_signed && is_negative)
        {
            empty_bits = MAX_UINT_VALUE_PART;
        }
        else
        {
            empty_bits = 0;
        }
        for (i = 0; i < (int)INT_VALUE_PARTS_PER_INTEGER_VALUE; ++i)
        {
            this_part = intval->part[i];
            if (bits_so_far < bits_discarded)
            {
                if (this_part != empty_bits)
                {
                    overflow = TRUE;
                }
            }
            else
            {
                result = unchecked((result << BITS_IN_INT_VALUE_PART) + this_part);
            }
            bits_so_far += BITS_IN_INT_VALUE_PART;
        }
        if (is_signed && is_negative != (unchecked((a_host_large_integer)result) < 0))
        {
            overflow = TRUE;
        }
        *value = unchecked((a_host_large_integer)result);
        *err = overflow;
    }

    // const_ints.c:284-334
    public static int cmp_integer_values(an_integer_value* op_1,
                                         a_boolean op_1_signed,
                                         an_integer_value* op_2,
                                         a_boolean op_2_signed)
    {
        int i;
        a_boolean sign_1 = op_1_signed && sign_of(op_1);
        a_boolean sign_2 = op_2_signed && sign_of(op_2);
        int result = 0;

        if (sign_1 != sign_2)
        {
            result = sign_1 ? -1 : 1;
        }
        else
        {
            for (i = 0; i < (int)INT_VALUE_PARTS_PER_INTEGER_VALUE; ++i)
            {
                if (op_1->part[i] > op_2->part[i])
                {
                    result = 1;
                    break;
                }
                else if (op_1->part[i] < op_2->part[i])
                {
                    result = -1;
                    break;
                }
            }
        }
        return result;
    }

    // const_ints.c:634-643. add_integer_values is owned by batch 2.
    public static void incr_integer_value(an_integer_value* intval)
    {
        an_integer_value one;
        a_boolean err;
        set_integer_value(&one, (a_host_large_integer)1);
        add_integer_values(intval, &one, /*is_signed=*/FALSE, &err);
    }

    // const_ints.c:868-879
    public static void or_integer_values(an_integer_value* op_1,
                                         an_integer_value* op_2)
    {
        int i;
        for (i = INT_VALUE_PARTS_PER_INTEGER_VALUE - 1; i >= 0; --i)
        {
            op_1->part[i] = unchecked((an_int_value_part)(op_1->part[i] | op_2->part[i]));
        }
    }

    // const_ints.c:882-893
    public static void and_integer_values(an_integer_value* op_1,
                                          an_integer_value* op_2)
    {
        int i;
        for (i = INT_VALUE_PARTS_PER_INTEGER_VALUE - 1; i >= 0; --i)
        {
            op_1->part[i] = unchecked((an_int_value_part)(op_1->part[i] & op_2->part[i]));
        }
    }

    // const_ints.c:896-907
    public static void xor_integer_values(an_integer_value* op_1,
                                          an_integer_value* op_2)
    {
        int i;
        for (i = INT_VALUE_PARTS_PER_INTEGER_VALUE - 1; i >= 0; --i)
        {
            op_1->part[i] = unchecked((an_int_value_part)(op_1->part[i] ^ op_2->part[i]));
        }
    }

    // const_ints.c:1109-1120
    public static void complement_integer_value(an_integer_value* op_1)
    {
        int i;
        for (i = INT_VALUE_PARTS_PER_INTEGER_VALUE - 1; i >= 0; --i)
        {
            op_1->part[i] = unchecked((an_int_value_part)(~op_1->part[i] & MAX_UINT_VALUE_PART));
        }
    }
}
