/*
Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
Exceptions.
See https://edgcpp.org/LICENSE.txt for license information.
SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
*/

/*
decode.h -- Declarations for decode.c (name demangler for C++).
*/

/* Avoid including these declarations more than once: */
#ifndef DECODE_H
#define DECODE_H 1

void decode_identifier(a_const_char *id,
                       char         *output_buffer,
                       sizeof_t     output_buffer_size,
                       a_boolean    *err,
                       a_boolean    *buffer_overflow_err,
                       sizeof_t     *required_buffer_size);

#endif /* ifndef DECODE_H */


