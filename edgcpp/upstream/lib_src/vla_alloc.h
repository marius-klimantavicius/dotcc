/*
Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
Exceptions.
See https://edgcpp.org/LICENSE.txt for license information.
SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
*/

/*

Declarations for runtime routines for lowered VLA operations.

*/

#ifndef VLA_ALLOC_H
#define VLA_ALLOC_H

#include "runtime.h"

EXTERN_C void __vla_alloc(void       *ptr,
                          ptrdiff_t  n_bytes);

EXTERN_C void __vla_dealloc(void  *ptr);

#endif /* ifndef VLA_ALLOC_H */

