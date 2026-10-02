/*
Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
Exceptions.
See https://edgcpp.org/LICENSE.txt for license information.
SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
*/

/*

Member functions of the bad_alloc and bad_array_new_length classes.

*/

#include "basics.h"
#include "runtime.h"

/* The definition of the exception classes can be disabled. */
#if USE_EDG_EXCEPTION_CLASSES

/*
If the runtime should be defined in the std namespace, open
the std namespace.
*/
#ifdef __EDG_RUNTIME_USES_NAMESPACES
namespace std {
#endif /* ifdef __EDG_RUNTIME_USES_NAMESPACES */

#if EXCEPTION_HANDLING

/*
The bad_alloc class is only supplied when the runtime supports exception
handling.
*/

bad_alloc::bad_alloc() THROW_NOTHING()
/*
Constructor for bad_alloc.
*/
{
}  /* bad_alloc::bad_alloc */


bad_alloc::bad_alloc(const bad_alloc& rhs) THROW_NOTHING() : exception(rhs)
/*
Copy constructor for bad_alloc.  Currently does nothing.
*/
{
}  /* bad_alloc::bad_alloc */


bad_alloc& bad_alloc::operator=(const bad_alloc& rhs) THROW_NOTHING()
/*
Assignment operator for bad_alloc.  Currently does nothing.
*/
{
  /* Call the base class assignment operator. */
  exception::operator=(rhs);
  return *this;
}  /* bad_alloc::operator= */


bad_alloc::~bad_alloc() THROW_NOTHING()
/*
Destructor for bad_alloc.
*/
{
}  /* bad_alloc::~bad_alloc */


const char* bad_alloc::what() const THROW_NOTHING()
/*
Return a string providing information about the exception.  Currently,
no additional information is available.
*/
{
  return "";
}  /* bad_alloc::what */


/*
The bad_array_new_length class is used to report array new errors that can only
be detected at run-time, namely a value that is too small (less than zero or
less than the number of elements in a braced-init-list) or too large.
*/

bad_array_new_length::bad_array_new_length() THROW_NOTHING()
/*
Constructor for bad_array_new_length.
*/
{
}  /* bad_array_new_length::bad_array_new_length */


bad_array_new_length::~bad_array_new_length() THROW_NOTHING()
/*
Destructor for bad_array_new_length.
*/
{
}  /* bad_array_new_length::~bad_array_new_length */


#endif /* EXCEPTION_HANDLING */


/*
If the runtime should be defined in the std namespace, close
the std namespace.
*/
#ifdef __EDG_RUNTIME_USES_NAMESPACES
}  /* namespace std */
#endif /* ifdef __EDG_RUNTIME_USES_NAMESPACES */

#endif /* USE_EDG_EXCEPTION_CLASSES */


