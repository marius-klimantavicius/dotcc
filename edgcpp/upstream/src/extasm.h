/*
Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
Exceptions.
See https://edgcpp.org/LICENSE.txt for license information.
SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
*/

/*

extasm.h -- Declarations related to extasm.c (having to do with 
            extended asm() statements, a GNU C extension).

*/

/* Avoid including these declarations more than once: */
#ifndef EXTASM_H
#define EXTASM_H 1

#if GNU_EXTENSIONS_ALLOWED

/* Conditionally open the "edg" namespace. */
BEGIN_EDG_NAMESPACE

extern a_named_register name_to_register(a_const_char *name);

extern an_asm_operand_ptr asm_operands_spec(a_boolean *seen_tok_colon_colon,
                                            int       *number_of_constraints);

extern a_named_register_list_ptr asm_clobbers_spec(
                                              a_boolean *seen_tok_colon_colon);

extern a_label_list_ptr asm_labels_spec(a_boolean *seen_tok_colon_colon);

extern void validate_operands_and_clobbers(an_asm_entry_ptr  asm_entry);

extern void extasm_one_time_init(void);

/* Conditionally close the "edg" namespace. */
END_EDG_NAMESPACE

#endif /* GNU_EXTENSIONS_ALLOWED */

#endif /* EXTASM_H */

