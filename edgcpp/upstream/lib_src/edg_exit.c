/*
Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
Exceptions.
See https://edgcpp.org/LICENSE.txt for license information.
SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
*/

/*

Exit processing.

*/

#include "basics.h"
#pragma hdrstop
#include "static_init.h"

/* The version of exit to be called must be the system exit routine not the
   interface routine in the runtime that has C++ linkage. */
extern "C" void exit(int);

/* Routine in throw.c that does exit processing for exception handling.
   Declared here to prevent pulling in all of eh.h that would redeclare
   exit. */
extern "C" void __eh_exit_processing(void);

/* NORETURN (but generates warnings in some configurations). */
void __edg_exit(int val)
/*
Do any wrapup processing required by the runtime including any
exception handling processing that must be done.  Then call the
system exit routine to complete the exit processing.
*/
{
  __eh_exit_processing();
  /* __call_dtors is called here and is also registered as an atexit
     function that will be called by the system exit routine.  This means
     that it will be called twice when the EDG C++ linkage version of
     exit is being used.  This is intended.  There is no ill effect of
     calling __call_dtors more than once. */
  __call_dtors();
  exit(val);
}


