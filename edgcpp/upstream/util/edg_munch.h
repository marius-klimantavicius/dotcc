/*
Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
Exceptions.
See https://edgcpp.org/LICENSE.txt for license information.
SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
*/

/*

Declarations for EDG equivalent of the AT&T munch utility.

*/


/*
Names of the arrays to be created containing pointers to the static
constructor and destructor functions to be called.
*/
#define CTORS_ARRAY_NAME "_ctors"
#define DTORS_ARRAY_NAME "_dtors"

/*
Typedef name to be used for the pointer arrays, and return type of the
function.
*/
#define FUNCTION_PTR_TYPEDEF_NAME "func_ptr"
#define FUNCTION_RETURN_TYPE "void"

/*
Type code output by "nm" for externally visible function definitions.
*/
#define EXTERN_TYPE 'T'

/*
Strings that are used to identify the static constructors and
destructors.
*/
#define CTOR_PREFIX "__sti__"
#define DTOR_PREFIX "__std__"


