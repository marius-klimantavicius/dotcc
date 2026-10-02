/*
Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
Exceptions.
See https://edgcpp.org/LICENSE.txt for license information.
SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
*/

/*

Declarations relating to main.c -- program startup and termination.

*/

#ifndef MAIN_H
#define MAIN_H 1

/*
The __linkl structure for each source module that has file scope initialization
or termination routines.
*/
struct __linkl {
  struct __linkl
		*next;		/* Pointer to the next struct __linkl in
				   the linked list. */
  void		(*ctor)();	/* Pointer to the initialization function,
				   NULL, if none. */
  void		(*dtor)();	/* Pointer to the termination function,
				   NULL, if none. */
};


/*
The AT&T patch utility will link all the struct __linkl *__link defined
in separate compilations onto a linked list pointed to by __head.
*/
EXTERN struct __linkl	*__head 
				/* Pointer to the head of the linked list
				   of initialization and termination
				   structures. */
#if VAR_INITIALIZERS
                        = NULL
#endif /* VAR_INITIALIZERS */
                              ;


/*
The AT&T munch utility creates arrays of pointers to static constructor
and destructor pointers.
*/
typedef void (*a_void_function_ptr)();
extern a_void_function_ptr _ctors[];
extern a_void_function_ptr _dtors[];

#endif /* MAIN_H */


