/*
Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
Exceptions.
See https://edgcpp.org/LICENSE.txt for license information.
SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
*/

/*

Placement version of C++ operator new[]();

*/


#include "basics.h"
#include "runtime.h"

#if ABI_CHANGES_FOR_ARRAY_NEW_AND_DELETE

void *operator new[](size_t			     size,
                     const STD_NAMESPACE::nothrow_t& nothrow_arg)
THROW_NOTHING()
/*
Nothrow version of array operator new.  Just call the normal nothrow
operator new.
*/
{
  return operator new(size, nothrow_arg);
}  /* operator new[] */

#endif /* ABI_CHANGES_FOR_ARRAY_NEW_AND_DELETE */


