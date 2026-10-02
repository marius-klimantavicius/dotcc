/*
Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
Exceptions.
See https://edgcpp.org/LICENSE.txt for license information.
SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
*/

/*
version.h -- Front end version number.
*/

/* Avoid including these declarations more than once. */
#ifndef VERSION_H
#define VERSION_H 1

/* Conditionally open the "edg" namespace. */
BEGIN_EDG_NAMESPACE

/*
Definition of the version number of this version.  It is made a separate
file to make updates easy.
*/
#define VERSION_NUMBER "7.0"  /* September 28, 2026. */

/*
Version number used to set a predefined macro that expands to the
front end version.  This must be a numeric value.
*/
#define VERSION_NUMBER_FOR_MACRO 700

/*
The date and time that this version was built.  These variables will
be defined when fe_init.c is compiled.
*/
#ifndef __DATE__
#define __DATE__ "[date unknown]"
#endif /* ifndef __DATE__ */

#ifndef __TIME__
#define __TIME__ "[time unknown]"
#endif /* ifndef __TIME__ */

#ifdef _MSC_VER
/* Suppress MSVC warnings about use of __DATE__ and __TIME__. */
#pragma warning( push )
#pragma warning( disable : 5048 )
#endif /* defined(_MSC_VER) */

EXTERN a_const_char
		*build_date
#if VAR_INITIALIZERS
			    = __DATE__
#endif /* VAR_INITIALIZERS */
                                      ;

EXTERN a_const_char
		*build_time
#if VAR_INITIALIZERS
			    = __TIME__
#endif /* VAR_INITIALIZERS */
                                      ;

#ifdef _MSC_VER
#pragma warning( pop )
#endif /* defined(_MSC_VER) */

/* Conditionally close the "edg" namespace. */
END_EDG_NAMESPACE

#endif /* ifndef VERSION_H */

