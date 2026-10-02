/*
Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
Exceptions.
See https://edgcpp.org/LICENSE.txt for license information.
SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
*/

/*

defines.h -- Defines configuration parameters for a given version of the
             runtime library.

*/

#define _XOPEN_VERSION 0
#define _POSIX_C_SOURCE 0

#ifdef __sun
/* The Solaris version uses a version of gcc that has IA-64 support. */
#define SYSTEM_RUNTIME_HAS_IA64_SUPPORT TRUE
/* The following is needed to get the declaration for memalign. */
#define __EXTENSIONS__
#endif /* ifdef __sun */

