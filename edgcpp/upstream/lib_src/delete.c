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


void operator delete(void *ptr) THROW_NOTHING()
/*
Free the memory pointed to by ptr.
*/
{
  if (ptr != NULL) {
    free(ptr);
  }  /* if */
}  /* operator delete */ 


