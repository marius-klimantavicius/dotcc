/*
Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
Exceptions. See https://edgcpp.org/LICENSE.txt for license information.
SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
*/

namespace EdgFaithful;

public static unsafe partial class mem_manage
{
    // mem_manage.c:1693-1699. Does not clear, free, or replace payload storage.
    public static void reset_text_buffer(a_text_buffer* buffer)
    {
        buffer->size = 0;
    }

    // mem_manage.c:1782-1792. Native && avoids reading payload when size is zero.
    public static void remove_null_terminator_from_text_buffer(a_text_buffer* buffer)
    {
        if (buffer->size > 0 && buffer->buffer[unchecked(buffer->size - 1)] == 0)
        {
            buffer->size = unchecked(buffer->size - 1);
        }
    }
}
