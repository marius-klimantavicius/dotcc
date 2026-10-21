/*
Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
Exceptions. See https://edgcpp.org/LICENSE.txt for license information.
SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
*/

// host_envir.h:3750-3751. Native typedefs are aliases, not distinct wrappers.
global using a_signed_diag_number = System.Int64;
global using an_unsigned_diag_number = System.UInt64;

namespace EdgFaithful;

// host_envir.h:3732-3745. a_byte is unsigned char; preserve the byte base and
// every original implicit discriminant (0..11), including internal-only values.
// Review/validation pending; no reduced severity enum or default int storage.
public enum an_error_severity : byte
{
    es_default,
    es_once,
    es_more_info,
    es_none,
    es_remark,
    es_warning,
    es_command_line_warning,
    es_discretionary_error,
    es_error,
    es_catastrophe,
    es_command_line_error,
    es_internal_error
}
