/*
Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
Exceptions.
See https://edgcpp.org/LICENSE.txt for license information.
SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
*/

/*

C++ operator new[](size_t, void*) (placement array new).

*/


#include "basics.h"
#include "runtime.h"

#if ABI_CHANGES_FOR_ARRAY_NEW_AND_DELETE && DEFINE_PLACEMENT_NEW_ROUTINES

void *operator new[](size_t, void* ptr) THROW_NOTHING()
/*
Placement array operator new.
*/
{
  return ptr;
}  /* operator new[](size_t, void*) */

#endif /* ABI_CHANGES_FOR_ARRAY_NEW_AND_DELETE &&
          DEFINE_PLACEMENT_NEW_ROUTINES*/


