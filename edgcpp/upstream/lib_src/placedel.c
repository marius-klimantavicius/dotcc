/*
Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
Exceptions.
See https://edgcpp.org/LICENSE.txt for license information.
SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
*/

/*

C++ operator delete(void*, void*);

*/

#include "basics.h"
#include "runtime.h"

#if ABI_CHANGES_FOR_PLACEMENT_DELETE

void operator delete(void *, void *) THROW_NOTHING()
/*
Placement operator delete -- does nothing.
*/
{
}  /* operator delete (void*, void*) */

#endif /* ABI_CHANGES_FOR_PLACEMENT_DELETE */

