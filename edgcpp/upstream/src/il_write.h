/*
Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
Exceptions.
See https://edgcpp.org/LICENSE.txt for license information.
SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
*/

/*

il_write.h -- Declarations relating to il_write.c (having to do with
              writing the intermediate language to a file).

*/

/* Avoid including these declarations more than once. */
#ifndef IL_WRITE_H
#define IL_WRITE_H 1

#ifndef HOST_ENVIR_H
#include "host_envir.h"
#endif /* ifndef HOST_ENVIR_H */

#if IL_SHOULD_BE_WRITTEN_TO_FILE

#ifndef MEM_TABLES_H
#include "mem_tables.h"
#endif /* ifndef MEM_TABLES_H */
#ifndef IL_FILE_H
#include "il_file.h"
#endif /* ifndef IL_FILE_H */

/* Conditionally open the "edg" namespace. */
BEGIN_EDG_NAMESPACE

EXTERN_THREAD FILE
		*f_il_output /* = NULL */;
			/* File to which the intermediate language is 
			   written.  NULL if file should not be written. */
EXTERN_THREAD a_const_char
		*il_file_name /* = NULL */;
			/* Name of the IL file, NULL if there isn't one or
			   it's a temporary file. */

extern void start_il_file(void);

extern void finish_il_file(void);

extern void close_il_output_file(void);

extern void cancel_il_file(void);

extern void write_memory_region(a_memory_region_number region_number);

extern void il_write_early_init(void);

#if CHECKING && DEBUG && ALTERNATE_IL_FILE_FORMAT
extern void trace_entry(a_memory_region_number memory_region_number,
                        an_il_entry_kind       entry_kind,
                        an_il_entry_number     entry_number);
#endif /* CHECKING && DEBUG && ALTERNATE_IL_FILE_FORMAT */

/* Conditionally close the "edg" namespace. */
END_EDG_NAMESPACE

#endif /* IL_SHOULD_BE_WRITTEN_TO_FILE */

#endif /* ifndef IL_WRITE_H */

