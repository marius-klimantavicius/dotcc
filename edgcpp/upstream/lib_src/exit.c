/*
Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
Exceptions.
See https://edgcpp.org/LICENSE.txt for license information.
SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
*/

/*

Exit processing.

*/


/*
Do not include any files that will result in an extern "C" version
of exit being declared.
*/

#include "basics.h"
/* Note that runtime.h is not included. */
#include "edg_exit.h"


extern "C++" NORETURN void exit(int val)
/*
This routine just provides a means of transferring control to our own
version of exit which will do some processing and then call the system
exit routine.  This is needed because one file cannot refer to both
the C and C++ versions of void exit(int).
*/
{
  __edg_exit(val);
}


