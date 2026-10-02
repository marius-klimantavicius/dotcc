/*
Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
Exceptions.
See https://edgcpp.org/LICENSE.txt for license information.
SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
*/

/*

il_display.h -- Declarations related to il_display (display the IL
                in human-readable form).

*/

/* Avoid including these declarations more than once. */
#ifndef IL_DISPLAY_H
#define IL_DISPLAY_H 1

#ifndef MEM_TABLES_H
#include "mem_tables.h"
#endif /* ifndef MEM_TABLES_H */

/* Conditionally open the "edg" namespace. */
BEGIN_EDG_NAMESPACE

extern void disp_file_scope_il(void);

extern void disp_routine_scope_il(a_memory_region_number region_number);

extern void do_il_display(char *filename);

extern void pragma_il_display(a_pending_pragma_ptr  ppp,
                              a_symbol_ptr          sym_ptr,
                              a_statement_ptr       stmt_ptr);

/* Conditionally close the "edg" namespace. */
END_EDG_NAMESPACE

#endif /* ifndef IL_DISPLAY_H */

