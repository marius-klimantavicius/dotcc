/*
Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
Exceptions.
See https://edgcpp.org/LICENSE.txt for license information.
SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
*/

/*

C++ operator new(size_t, void*);

*/

#include "basics.h"
#include "runtime.h"

#if DEFINE_PLACEMENT_NEW_ROUTINES

void *operator new(size_t, void *ptr) THROW_NOTHING()
/*
Return the value of ptr as the address of the new object.
*/
{
  return ptr;
}  /* operator new (size_t, void*) */

#endif /* DEFINE_PLACEMENT_NEW_ROUTINES */

