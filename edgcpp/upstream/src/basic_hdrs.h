/*
Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
Exceptions.
See https://edgcpp.org/LICENSE.txt for license information.
SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
*/

/*

basic_hdrs.h -- Inclusion of low-level universal header files.

*/

/* Avoid including these declarations more than once. */
#ifndef BASIC_HDRS_H
#define BASIC_HDRS_H 1

#ifdef _lint
/* Directives to control lint diagnostics. */
#include "lint.h"
#endif /* ifdef _lint */

/* Basic declarations.  basics.h also pulls in defines.h, which provides
   default configuration parameters for this version. */
#include "basics.h"

/* Language configuration. */
#include "lang_feat.h"

/* Host environment configuration. */
#include "host_envir.h"

/* Target configuration. */
#include "targ_def.h"

#endif /* ifndef BASIC_HDRS_H */

