/*
Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
Exceptions.
See https://edgcpp.org/LICENSE.txt for license information.
SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
*/

/*

fe_init.h -- Declarations relating to fe_init.c (having to do with
             global initialization of the front end).

*/

/* Avoid including these declarations more than once: */
#ifndef FE_INIT_H
#define FE_INIT_H 1

/* Conditionally open the "edg" namespace. */
BEGIN_EDG_NAMESPACE

#if STANDALONE_UTILITY_PROGRAM
extern void standalone_utility_early_init(void);
extern void standalone_utility_late_init(void);
#if CHECKING
EXTERN_THREAD a_boolean
                il_header_has_been_read;
                        /* Set to TRUE once the IL header has been read. */
#endif /* CHECKING */
#else /* !STANDALONE_UTILITY_PROGRAM */
extern void fe_early_init(void);
extern void fe_one_time_init(void);
extern void fe_init_part_1(void);
extern void fe_init_for_pch_prefix_scan(void);
extern void fe_init_part_2(void);
extern void fe_translation_unit_init(void);
#endif /* STANDALONE_UTILITY_PROGRAM */

extern void initialize_opname_names(void);

/* Conditionally close the "edg" namespace. */
END_EDG_NAMESPACE

#endif /* ifndef FE_INIT_H */

