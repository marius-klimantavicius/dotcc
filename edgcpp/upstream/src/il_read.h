/*
Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
Exceptions.
See https://edgcpp.org/LICENSE.txt for license information.
SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
*/

/*

il_read.h -- Declarations relating to il_read.c (having to do with
             reading the intermediate language).

*/

/* Avoid including these declarations more than once. */
#ifndef IL_READ_H
#define IL_READ_H 1

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
#ifndef IL_WALK_H
#include "il_walk.h"
#endif /* ifndef IL_WALK_H */

/* Conditionally open the "edg" namespace. */
BEGIN_EDG_NAMESPACE

#if ALTERNATE_IL_FILE_FORMAT
EXTERN_THREAD char
		*entry_array_base_array[(int)iek_last],
		*fs_entry_array_base_array[(int)iek_last];
			/* For each IL entry kind, a pointer to an array
			   of entries of that kind, which is the full
			   set of entries.  One can convert an entry number
			   to an entry address by multiplying the entry
			   number by the entry size and adding the base address
			   from this array.  The "fs_" array is for the file
			   scope, the other is for a function scope. */

EXTERN_THREAD sizeof_t
		entry_length_with_prefix[(int)iek_last],
		fs_entry_length_with_prefix[(int)iek_last];
			/* The each IL entry kind, the length of the IL
			   entry including any prefix, in function scopes
			   and (the fs_ version) in the file scope. */
EXTERN_THREAD sizeof_t
		length_of_entry_prefix[(int)iek_last],
		fs_length_of_entry_prefix[(int)iek_last];
			/* For each IL entry kind, the length of the prefix
			   preceding the entry, in function scopes and
			   (the fs_ version) in the file scope. */
#endif /* ALTERNATE_IL_FILE_FORMAT */

/* Read the intermediate language for the file scope. */
extern void il_read(FILE *f_il_input);
/* Read the intermediate language for one memory region. */
extern void read_memory_region(a_memory_region_number region_number);

/* Conditionally close the "edg" namespace. */
END_EDG_NAMESPACE

#endif /* IL_SHOULD_BE_WRITTEN_TO_FILE */

#endif /* ifndef IL_READ_H */

