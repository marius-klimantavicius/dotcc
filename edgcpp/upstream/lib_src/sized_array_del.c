/*
Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
Exceptions.
See https://edgcpp.org/LICENSE.txt for license information.
SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
*/

/*

C++ operator delete[](void *, size_t);

*/


#include "basics.h"
#include "runtime.h"

#if ABI_CHANGES_FOR_ARRAY_NEW_AND_DELETE

#ifdef __cpp_sized_deallocation

void operator delete[](void *ptr, size_t size) THROW_NOTHING()
/*
Default array operator delete with size.  Just call the normal
operator delete[].
*/
{
  operator delete[](ptr);
}  /* operator delete[] */

#endif /* __cpp_sized_deallocation */

#endif /* ABI_CHANGES_FOR_ARRAY_NEW_AND_DELETE */

