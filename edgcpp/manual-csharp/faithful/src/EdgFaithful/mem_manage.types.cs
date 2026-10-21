/*
Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
Exceptions. See https://edgcpp.org/LICENSE.txt for license information.
SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
*/

using System.Runtime.InteropServices;
using sizeof_t = System.UInt64;

namespace EdgFaithful;

// mem_manage.h:652-664. Pinned LP64, native signed char; review pending.
// Separate native object and payload lifetimes; no managed ownership policy.
[StructLayout(LayoutKind.Sequential, Pack = 8, Size = 32)]
public unsafe struct a_text_buffer
{
    public sizeof_t allocated_size;
    public sizeof_t size;
    public sizeof_t allocation_increment;
    public sbyte* buffer;
}
