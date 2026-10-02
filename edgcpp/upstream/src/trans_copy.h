/*
Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
Exceptions.
See https://edgcpp.org/LICENSE.txt for license information.
SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
*/

/*

trans_copy.h -- Declarations related to trans_copy.c (copying of IL
                from secondary translation units to the primary
                translation unit).

*/

/* Avoid including these declarations more than once: */
#ifndef TRANS_COPY_H
#define TRANS_COPY_H 1

/* Conditionally open the "edg" namespace. */
BEGIN_EDG_NAMESPACE

extern void copy_secondary_trans_unit_IL_to_primary(void);

extern
void mark_secondary_trans_unit_IL_entities_used_from_primary_as_needed(void);

extern void switch_canonical_for_deleted_definition(
                                                 a_source_correspondence *scp);

extern void trans_copy_one_time_init(void);

/* Conditionally close the "edg" namespace. */
END_EDG_NAMESPACE

#endif /* ifndef TRANS_COPY_H */

