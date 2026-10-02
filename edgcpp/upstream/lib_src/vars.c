/*
Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
Exceptions.
See https://edgcpp.org/LICENSE.txt for license information.
SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
*/

/*

Performs initialization of global variables used by the runtime.

*/

#define EXTERN /* empty */
#define VAR_INITIALIZERS 1

#include "basics.h"
#include "runtime.h"
#include "main.h"
#include "vec_newdel.h"
#include "eh.h"



