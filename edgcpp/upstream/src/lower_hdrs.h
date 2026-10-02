/*
Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
Exceptions.
See https://edgcpp.org/LICENSE.txt for license information.
SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
*/

/*

lower_hdrs.h -- Inclusion of header files used by files involved in IL
                lowering.

*/

/* Avoid including these declarations more than once. */
#ifndef LOWER_HDRS_H
#define LOWER_HDRS_H 1

#include "folding.h"
#include "lower_eh.h"
#include "lower_il.h"
#include "lower_init.h"
#include "lower_name.h"
#if MINIMAL_INLINING
#include "inline.h"
#endif /* MINIMAL_INLINING */
#include "lower_c99.h"
#include "pch.h"
				   
#endif /* ifndef LOWER_HDRS_H */

