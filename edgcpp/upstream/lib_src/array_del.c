/*
Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
Exceptions.
See https://edgcpp.org/LICENSE.txt for license information.
SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
*/

/*

C++ operator delete();

*/


#include "basics.h"
#include "runtime.h"

#if ABI_CHANGES_FOR_ARRAY_NEW_AND_DELETE

void operator delete[](void *ptr) THROW_NOTHING()
/*
Default array operator delete.  Just call the normal operator delete.
*/
{
  operator delete(ptr);
}  /* operator delete[] */

#endif /* ABI_CHANGES_FOR_ARRAY_NEW_AND_DELETE */

