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

#ifdef __STDCPP_DEFAULT_NEW_ALIGNMENT__

void operator delete(void				*ptr,
                     STD_NAMESPACE::align_val_t         align,
                     const STD_NAMESPACE::nothrow_t&)
THROW_NOTHING()
/*
Nothrow version of aligned operator delete.
*/
{
  operator delete(ptr, align);
}  /* operator delete */ 

#endif /* ifdef __STDCPP_DEFAULT_NEW_ALIGNMENT__ */

