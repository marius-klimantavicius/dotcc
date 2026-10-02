/*
Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
Exceptions.
See https://edgcpp.org/LICENSE.txt for license information.
SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
*/

/*

set_new_handler routine to allow the user to affect the behavior of the
default operator new() when memory cannot be allocated.

*/

#include "basics.h"
#include "runtime.h"
#pragma hdrstop

#ifndef NULL
#define NULL 0
#endif /* ifndef NULL */

/*
If the runtime should be defined in the std namespace, open
the std namespace.
*/
#ifdef __EDG_RUNTIME_USES_NAMESPACES
namespace std {
#endif /* ifdef __EDG_RUNTIME_USES_NAMESPACES */

new_handler set_new_handler(new_handler handler) THROW_NOTHING()
/*
Set _new_handler to the new function pointer provided and return the
previous value of _new_handler.
*/
{
  new_handler rr = _new_handler;
  _new_handler = handler;
  return rr;
}  /* set_new_handler */


new_handler get_new_handler(void) THROW_NOTHING()
/*
Returns a pointer to the function that handles "new" operations.
*/
{
  return _new_handler;
}  /* get_new_handler */

/*
If the runtime should be defined in the std namespace, close
the std namespace.
*/
#ifdef __EDG_RUNTIME_USES_NAMESPACES
}  /* namespace std */
#endif /* ifdef __EDG_RUNTIME_USES_NAMESPACES */

