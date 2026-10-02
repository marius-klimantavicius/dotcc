/*
Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
Exceptions.
See https://edgcpp.org/LICENSE.txt for license information.
SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
*/

/*

C++ operator delete(void *, size_t);

*/

#include "basics.h"
#include "runtime.h"

#ifdef __cpp_sized_deallocation

void operator delete(void *ptr, size_t size) THROW_NOTHING()
/*
Free the memory pointed to by ptr.  size specifies the size of the object.
*/
{
  operator delete(ptr);
}  /* operator delete */

#endif /* __cpp_sized_deallocation */

