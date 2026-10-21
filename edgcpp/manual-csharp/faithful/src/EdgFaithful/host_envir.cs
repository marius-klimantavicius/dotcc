/*
Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
Exceptions. See https://edgcpp.org/LICENSE.txt for license information.
SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
*/

using sizeof_t = System.UInt64;

namespace EdgFaithful;

// Manual source port, Linux LP64/signed-char profile. Review/validation pending.
// See ledgers/host_envir-batch1.{md,json}; no native library calls in these leaves.
public static unsafe partial class host_envir
{
    // host_envir.c:2764-2780. Read only the current pair; stop at first mismatch.
    public static int smemcmp(sbyte* s1, sbyte* s2, sizeof_t length)
    {
        int cmp = 0;

        for (; length > 0; length--)
        {
            cmp = unchecked((byte)(*s1++)) - unchecked((byte)(*s2++));
            if (cmp != 0) break;
        }
        return cmp;
    }

    // host_envir.c:4985-5018. unsigned long is 64 bits in the selected profile.
    public static uint unicode_to_utf8(ulong uc, byte* chars)
    {
        uint len;

        if (uc <= 0x7f)
        {
            len = 1;
            chars[0] = unchecked((byte)uc);
        }
        else if (uc <= 0x7ff)
        {
            len = 2;
            chars[0] = unchecked((byte)((uc >> 6) | 0xc0));
            chars[1] = unchecked((byte)((uc & 0x3f) | 0x80));
        }
        else if (uc <= 0xffff)
        {
            len = 3;
            chars[0] = unchecked((byte)((uc >> 12) | 0xe0));
            chars[1] = unchecked((byte)(((uc >> 6) & 0x3f) | 0x80));
            chars[2] = unchecked((byte)((uc & 0x3f) | 0x80));
        }
        else
        {
            len = 4;
            chars[0] = unchecked((byte)(((uc >> 18) & 0x7) | 0xf0));
            chars[1] = unchecked((byte)(((uc >> 12) & 0x3f) | 0x80));
            chars[2] = unchecked((byte)(((uc >> 6) & 0x3f) | 0x80));
            chars[3] = unchecked((byte)((uc & 0x3f) | 0x80));
        }
        return len;
    }
}
