/*
Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
Exceptions.
See https://edgcpp.org/LICENSE.txt for license information.
SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
*/

/*

C++ operator new[]();

*/


#include "basics.h"
#include "runtime.h"

#if ABI_CHANGES_FOR_ARRAY_NEW_AND_DELETE

void *operator new[](size_t size) NEW_THROWS
/*
Default array operator new.  Just call the normal operator new.
*/
{
  return operator new(size);
}  /* operator new[] */

#endif /* ABI_CHANGES_FOR_ARRAY_NEW_AND_DELETE */


