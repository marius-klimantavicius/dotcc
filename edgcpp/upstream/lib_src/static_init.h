/*
Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
Exceptions.
See https://edgcpp.org/LICENSE.txt for license information.
SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
*/

/*

Declarations relating to static_init.c -- initialization and termination.

*/

#ifndef STATIC_INIT_H
#define STATIC_INIT_H 1

/*
External declarations for the functions that perform the static
initialization and destruction.
*/

extern void __call_ctors();
extern void __call_dtors();
extern void __register_finalization_routine(void);

#endif /* STATIC_INIT_H */



