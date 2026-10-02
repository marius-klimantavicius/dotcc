/*
Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
Exceptions.
See https://edgcpp.org/LICENSE.txt for license information.
SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
*/

/*

munch_dtors.c -- Provides the definition of the _dtors variable that is
                 defined by edg_munch (when edg_munch is used).  This is
                 used to avoid undefined symbols when patch is being used.

*/

#include "basics.h"
#include "runtime.h"

typedef void (*PFV)();
PFV _dtors[] = {0};

