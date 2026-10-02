/*
Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
Exceptions.
See https://edgcpp.org/LICENSE.txt for license information.
SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
*/

/*

fe_wrapup.h - declarations related to end of front end processing.

*/

/* Avoid including these declarations more than once: */
#ifndef FE_WRAPUP_H
#define FE_WRAPUP_H 1

/* Conditionally open the "edg" namespace. */
BEGIN_EDG_NAMESPACE

extern void translation_unit_wrapup(void);

extern void fe_wrapup(void);

extern void fe_wrapup_part_2(void);

#if MAKE_FRONT_END_CALLABLE
extern void fe_cleanup(void);
#endif /* MAKE_FRONT_END_CALLABLE */

/* Conditionally close the "edg" namespace. */
END_EDG_NAMESPACE

#endif /* ifndef FE_WRAPUP_H */

