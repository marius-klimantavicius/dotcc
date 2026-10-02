/*
Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
Exceptions.
See https://edgcpp.org/LICENSE.txt for license information.
SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
*/

/*

Declaration for __memzero.

*/

#ifndef MEMZERO_H
#define MEMZERO_H

#include "runtime.h"

EXTERN_C void __memzero(void    *buffer,
                        size_t	size);

#endif /* ifndef MEMZERO_H */

