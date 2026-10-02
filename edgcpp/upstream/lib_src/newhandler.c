/*
Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
Exceptions.
See https://edgcpp.org/LICENSE.txt for license information.
SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
*/

/*

C++ default new handler.

*/

#include "basics.h"
#include "runtime.h"

extern void __default_new_handler(void)
/*
The default new handler routine that is called when operator new cannot
allocate memory.  When exceptions are enabled, this routine simply throws
a bad_alloc exception.  When exceptions are not enabled, we simply return
to the caller, which detects that the default new handler is being used
and returns a NULL pointer to the caller.
*/
{
#if EXCEPTION_HANDLING
  throw STD_NAMESPACE::bad_alloc();
#else /* !EXCEPTION_HANDLING */
  return;
#endif /* EXCEPTION_HANDLING */
}  /* __default_new_handler */

#if ABI_COMPATIBILITY_VERSION >= 406 && EXCEPTION_HANDLING

EXTERN_C void __throw_bad_array_new_length(void)
/*
This routine is called by lowered code or the run-time library if it has been
determined that the overall size of an array new operation is invalid for
some reason (i.e., overflow, less than zero, too small for initializer list).
*/
{
  throw STD_NAMESPACE::bad_array_new_length();
}  /* __throw_bad_array_new_length */

#endif /* ABI_COMPATIBILITY_VERSION >= 406 && EXCEPTION_HANDLING */

