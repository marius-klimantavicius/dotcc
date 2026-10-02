/*
Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
Exceptions.
See https://edgcpp.org/LICENSE.txt for license information.
SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
*/

/*

Declarations for all runtime routines.

*/

#ifndef BASICS_H
#define BASICS_H 1

/*
Include the header file that supplies the default configuration
parameters for this version.
*/
   
#include "defines.h"

/*
EXTERN is defined usually as "extern"; in the translation unit that
actually defines storage for external variables, it is defined as an
empty string.  EXTERN is used on the declarations of external variables
in .h files.  This scheme makes it easy to define them in only one
place while using the same source in all places.  Likewise, 
VAR_INITIALIZERS is defined to cause inclusion of initializers for those
variables.
*/
#ifndef EXTERN
#define EXTERN extern
#endif /* ifndef EXTERN */
#ifndef VAR_INITIALIZERS
#define VAR_INITIALIZERS 0
#endif /* ifndef VAR_INITIALIZERS */

/* Macro used to provide an initial value for variables declared EXTERN. */
#if VAR_INITIALIZERS
#define initial_value(value) = value
#else /* VAR_INITIALIZERS */
#define initial_value(value) /* nothing */
#endif /* VAR_INITIALIZERS */

#ifndef CHECKING
/* Include consistency-checking code. */
#define CHECKING 1
#endif /* ifndef CHECKING */

/*
EXTERN_C is used to declare an external function with C linkage.  When
compiling with a C compiler this is just set to ``extern'', but when
compiling with a C++ compiler it is set to ``extern "C"''.
*/
#ifdef __cplusplus
#define EXTERN_C extern "C"
#else /* !defined(__cplusplus) */
#define EXTERN_C extern
#endif /* __cplusplus */

/*
Definition of a generic byte.  Always "unsigned char".
*/
typedef unsigned char a_byte;

/* Simple boolean type: */
typedef int	a_boolean;
typedef a_byte	a_byte_boolean;
#define FALSE 0
#define TRUE 1

/*
Most supported C++ compilers support [[noreturn]], but not all GCC versions,
nor Visual Studio versions.
*/
#ifdef __GNUC__
#if __has_cpp_attribute(noreturn)
#define NORETURN [[noreturn]]
#else  /* !__has_cpp_attribute(noreturn) */
#define NORETURN __attribute__((noreturn))
#endif /* __has_cpp_attribute(noreturn) */
#else /* !defined(__GNUC__) */
#ifdef _WIN32
#define NORETURN __declspec(noreturn)
#else /* !defined(_WIN32) */
#define NORETURN [[noreturn]]
#endif /* defined(_WIN32) */
#endif /* defined(__GNUC__) */

#endif /* BASICS_H */


