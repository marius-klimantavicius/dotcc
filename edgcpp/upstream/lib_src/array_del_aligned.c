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
#ifdef __STDCPP_DEFAULT_NEW_ALIGNMENT__

void operator delete[](void *ptr,
                       STD_NAMESPACE::align_val_t align) THROW_NOTHING()
/*
Default aligned array operator delete.  Just call the normal aligned
operator delete.
*/
{
  operator delete(ptr, align);
}  /* operator delete[] */

#endif /* ifdef __STDCPP_DEFAULT_NEW_ALIGNMENT__ */
#endif /* ABI_CHANGES_FOR_ARRAY_NEW_AND_DELETE */

