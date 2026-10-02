/*
Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
Exceptions.
See https://edgcpp.org/LICENSE.txt for license information.
SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
*/

/*

il_file.h -- Definitions related to the intermediate language file.

*/

/* Avoid including these declarations more than once: */
#ifndef IL_FILE_H
#define IL_FILE_H 1

#ifndef HOST_ENVIR_H
#include "host_envir.h"
#endif /* ifndef HOST_ENVIR_H */
#ifndef IL_H
#include "il.h"
#endif /* ifndef IL_H */

#if IL_SHOULD_BE_WRITTEN_TO_FILE

/* Conditionally open the "edg" namespace. */
BEGIN_EDG_NAMESPACE

/* The customizable IL file magic string identifier written at the start of an
   il file.  This is checked on the reading end. */
#define IL_FILE_MAGIC_STRING_IDENTIFIER "EDG IL file"
/* The length of the filled-in magic string (with a terminating null) is the
   length of the string here plus 6 for " (v" and ")\f\f", plus the length of
   the version number minus two for the terminating null characters. */
#define LEN_IL_FILE_MAGIC_STRING \
  (sizeof(IL_FILE_MAGIC_STRING_IDENTIFIER) + 6 + sizeof(IL_VERSION_NUMBER) - 2)

using an_il_file_magic_string = Small_string<LEN_IL_FILE_MAGIC_STRING + 1>;
                        /* The type of an IL file magic string. */

inline an_il_file_magic_string current_il_magic_string()
/*
Return the expected IL magic string.
*/
{
  return an_il_file_magic_string(IL_FILE_MAGIC_STRING_IDENTIFIER,
                                 " (v", IL_VERSION_NUMBER, ")\f\f");
}  /* current_il_magic_string */


/*
Definitions for file positioning.  The POSIX ftell and fseek functions
typically limit IL files to 2 GB (32-bit offsets).  If larger IL files are
anticipated, setting LARGE_IL_FILE_SUPPORT to TRUE causes the front end to
use alternative system-dependent routines that permit file sizes larger
than those supported by ftell/fseek.  (Note that on non-Windows systems it
may be necessary to define _FILE_OFFSET_BITS to 64 in order to configure
off_t to be a 64-bit value.)
*/
#ifndef LARGE_IL_FILE_SUPPORT
#define LARGE_IL_FILE_SUPPORT FALSE
#endif /* !defined(LARGE_IL_FILE_SUPPORT) */
#if LARGE_IL_FILE_SUPPORT
#if EDG_WIN32
typedef __int64	a_file_position;
			/* Position in a file, as returned by _ftelli64 and
			   accepted by _fseeki64. */
#define get_file_position(file) _ftelli64(file)
#define set_file_position(file, offset, origin) _fseeki64(file, offset, origin)
#else /* !EDG_WIN32 */
typedef off_t	a_file_position;
			/* Position in a file, as returned by ftello and
			   accepted by fseeko. */
#define get_file_position(file) ftello(file)
#define set_file_position(file, offset, origin) fseeko(file, offset, origin)
#endif /* EDG_WIN32 */
#else /* !LARGE_IL_FILE_SUPPORT */
typedef off_t	a_file_position;
			/* Position in a file as returned by ftell and
			   accepted by fseek. */
#define get_file_position(file) ftell(file)
#define set_file_position(file, offset, origin) fseek(file, offset, origin)
#endif /* LARGE_IL_FILE_SUPPORT */

EXTERN_THREAD a_file_position
		*index_for_il_file /* = NULL */;
			/* Parallel array to mem_region_table.
			   index_for_il_file[i] contains the file offset of
			   region i in the file, or 0 if the region has not
			   yet been written. */

#if ALTERNATE_IL_FILE_FORMAT
/*
In the alternate file format, pointers in the IL entries are replaced by
integers that encode the entry number.  This type is the container for
the encoded form; it is converted to "char *" when stored in memory.
*/
typedef TYPE_FOR_PREFIX_ENTRY_NUMBER
		an_encoded_entry_number;
/*
Tag bit used to indicate a function-scope entry number instead of a
file-scope entry number in an encoded IL entry number.
*/
#define FUNC_ENTRY_NUMBER_BIT                                                 \
  ((an_encoded_entry_number) 0x1 <<                                           \
                            ((CHAR_BIT * sizeof(an_encoded_entry_number)) - 1))
#endif /* ALTERNATE_IL_FILE_FORMAT */

/* Conditionally close the "edg" namespace. */
END_EDG_NAMESPACE

#endif /* IL_SHOULD_BE_WRITTEN_TO_FILE */

#endif /* ifndef IL_FILE_H */

