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

void operator delete[](void				*ptr,
                       const STD_NAMESPACE::nothrow_t&	nothrow_arg)
THROW_NOTHING()
/*
Nothrow version of array operator delete.  Just call the normal nothrow
operator delete.
*/
{
  operator delete(ptr, nothrow_arg);
}  /* operator delete[] */

#endif /* ABI_CHANGES_FOR_ARRAY_NEW_AND_DELETE */

