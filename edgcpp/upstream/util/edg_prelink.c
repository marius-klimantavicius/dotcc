/*
Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
Exceptions.
See https://edgcpp.org/LICENSE.txt for license information.
SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
*/

/*

Prelink utility for template instantiation.

*/

#include "basics.h"
#ifndef STDLIB_H_INCLUDED
#include <stdlib.h>
/* So that host_envir.h knows that we have included stdlib.h. */
#define STDLIB_H_INCLUDED 1
#endif /* STDLIB_H_INCLUDED */
#include <stdio.h>
#include <ctype.h>
#include <errno.h>

#if __MICROSOFT_OS__
/* Used to get a prototype for chdir. */
#include <direct.h>
/* Used to get a prototype for getpid. */
#include <process.h>
/* Microsoft requires that popen and pclose be called as _popen and _pclose */
#define popen _popen
#define pclose _pclose
#else /* !__MICROSOFT_OS__ */
/* Used to get a prototype for chdir. */
#include <unistd.h>
#endif /* __MICROSOFT_OS__ */

#if defined(__SUNPRO_CC) && __BSD__
/* The SunOS 4.1.3 Sun CC header files do not define the system function. */
EXTERN_C int system(const char *);
#endif /* defined(__SUNPRO_CC) && __BSD__ */

/*
If the EDG namespace is being used, make the names from the namespace
visible.
*/
USING_NAMESPACE_EDG

#include "host_envir.h"
#include "targ_def.h"
#include "edg_prelink.h"
#include "decode.h"

/*
The type of the pointer passed to realloc.  This is usually void* on ANSI
compilers and char* on pcc compilers.  Sun C++ uses char* for some reason
though.
*/
#if defined(__cplusplus) && defined(__SUNPRO_CC) && __BSD__
typedef char *a_realloc_arg;
#else /* !(defined(__cplusplus) && __defined(__SUNPRO_CC) && __BSD__) */
typedef a_void_ptr a_realloc_arg;
#endif /* defined(__cplusplus) && __defined(__SUNPRO_CC) && __BSD__ */

#if !AUTOMATIC_TEMPLATE_INSTANTIATION
 #error -- AUTOMATIC_TEMPLATE_INSTANTIATION must be TRUE when using edg_prelink
#endif /* !AUTOMATIC_TEMPLATE_INSTANTIATION */

/*
The getopt.h include file will provide either the declarations needed
to use the system getopt routine or, if no system version is available,
the body of our own version of the getopt routine.
*/
#include "getopt.h"

/* Forward declarations of pointer types required before their definitions. */
typedef struct a_pl_input_file *a_pl_input_file_ptr;
typedef struct a_pl_object_file *a_pl_object_file_ptr;
typedef struct a_pl_file_list_entry *a_pl_file_list_entry_ptr;


/* An element of a list of input files that can instantiation a
   given symbol. */
typedef struct a_pl_instantiation_site *a_pl_instantiation_site_ptr;
typedef struct a_pl_instantiation_site {
  a_pl_instantiation_site_ptr
		next;
			/* Next instantiation site for the symbol. */
  a_pl_input_file_ptr
		input_file;
			/* Pointer to an input file that can generate
			   the instantiation. */
} a_pl_instantiation_site;


/*
Entry used to record assignments that have been made.  This is used to
detect instantiation loops.
*/
typedef struct a_pl_assignment *a_pl_assignment_ptr;
typedef struct a_pl_assignment {
  a_pl_assignment_ptr
		next;	/* The next entry in the hash table bucket, or NULL
			   for the last entry. */
  char		*name;	/* The name of the assigned entry. */
  int		times_assigned;
			/* The number of times the entry has been assigned to
			   this file. */
} a_pl_assignment;


/* Description of a symbol from an object file.  This structure is
   used for both the representation of the symbol from the object file
   and a description of the symbol in the global symbol table.  Most of
   the fields are only used by global symbol table symbols. */
typedef struct a_pl_symbol *a_pl_symbol_ptr;
typedef struct a_pl_symbol {
  a_pl_symbol_ptr
		next;
			/* The next symbol associated with an object file. */
  a_pl_symbol_ptr
		next_in_symbol_table;
			/* The next symbol in the global symbol table list. */
  a_pl_symbol_ptr
		next_in_request_file;
			/* The next symbol in an instantiation request file. */
  a_pl_symbol_ptr
		next_in_specialization_list;
			/* The next symbol on a linked list of specialization
			   symbols. */
  a_pl_input_file_ptr
		instantiation_file;
			/* The input file responsible for instantiating this
			   symbol. */
  a_pl_instantiation_site_ptr
		possible_instantiation_sites;
			/* List of input files capable of instantiating a
			   template. */
  a_pl_symbol_ptr
		global_sym;
			/* Pointer to the global symbol table entry for this
			   name.  This is present for symbols associated
			   with object files.  This allows you to revisit
			   a symbol without having to look it up again. 
			   For special symbols (such as __CBI__ names) this
			   points to the global symbol for the symbol to which
			   the special symbol refers (e.g., __CBI__xxx points
			   to the global symbol for xxx). */
  a_pl_symbol_ptr
		template_sym;
			/* Pointer to the symbol entry for the template
			   of which a given name is an instance.  Present
			   for instances of exported templates.  This is
			   used to determine if a definition of an exported
			   template is available. */
  a_pl_symbol_ptr
		primary_entry;
			/* For an alternate entry point, this points to the
			   symbol associated with the primary entry point.
			   References to the alternate entry point are treated
			   as references to the primary entry. */
  char		*name;
			/* Name of the symbol. */
  a_byte_boolean
		referenced;
			/* The symbol has been referenced by an object file
			   that has been linked into the executable.
			   A reference can be either an undefined entry
			   from the object file or a TIR entry. */
  a_byte_boolean
		defined;
			/* The symbol is defined.  This is set when an
			   object file that is included in the link defines
		           the symbol.  Not set for tentative definitions. */
  a_byte_boolean
		definition_seen_in_archive;
			/* A definition has been seen in an archive, but may
			   not have been linked in because it may not have
			   been required.  The presence of such a definition
			   should suppress the generation of an
                           instantiation. */
  a_byte_boolean
		tentative_definition;
			/* The symbol has a tentative definition.  This is
			   set when an object file that is included in the link
			   provides a "common" or tentative definition of
			   the symbol. */
  a_byte_boolean
		multiple_definition;
			/* Set if multiple object files included in the link
			   define the symbol. */
  a_byte_boolean
		is_template;
			/* Set if an object file that is included in the link
			   contains a CBI or TIR flag for the symbol. */
  a_byte_boolean
		can_be_instantiated;
			/* Set if an object file that is included in the link
			   contains a CBI flag for the symbol. */
  a_byte_boolean
		do_not_instantiate;
			/* Set if an object file that is included in the link
			   contains a DNI flag for the symbol.  This flag
			   will prevent the prelinker from assigning the
			   symbol to any file, not just the file that contains
			   the DNI flag. */
  a_byte_boolean
		instantiated;
			/* Indicates that the symbol has been instantiated.
			   This is set when it has been determined that
			   an object file has provided the necessary
			   instantiation and is also set when a symbol
			   is assigned to an object file for instantiation. */
  a_byte_boolean
		is_specialization;
			/* TRUE if this entry is for an explicit specialization
			   of a template instance. */
  a_pl_input_file_ptr
		defined_in;
			/* The input file in which the symbol was defined. */
} a_pl_symbol;



/* Structure that represents a single object file.  This could be a
   normal object file or an object within an archive. */
typedef struct a_pl_object_file {
  a_pl_object_file_ptr
		next;
			/* For an archive, points to the next object file
			   in the archive. */
  char		*file_name;
			/* Name of the object file.  Same as the input
			   file_name for an object (.o) file. */
  a_pl_symbol_ptr
		symbols;
			/* Points to a list of symbols associated with this
			   object file. */
  a_byte_boolean
		included_in_output;
			/* TRUE when the file is part of the resulting
			   output.  This is always TRUE for .o files but
			   is only TRUE for objects in an archive if the
			   object is needed to resolve a reference. */
  a_byte_boolean
		is_related_file;
			/* TRUE for object files that are related to a
			   primary input file.  This is used in one
			   instantiation per object mode to designate
			   an object file containing an instantiation. */
  time_t	modification_time;
			/* The last modification time of the file.  Set
			   when doing dependency checking for object files
			   that depend on exported template files. */
} a_pl_object_file;


/* Structure that represents a single input (.o or .a) file. */
typedef struct a_pl_input_file {
  a_pl_input_file_ptr
		next;
			/* Next entry on the list. */
  char		*file_name;
			/* Name of the input file. */
  char          *orig_name;
                        /* Name of the file as specified on the command
			   line.  This is normally the same as file_name,
			   but will be different for library names specified
			   with the -l option (e.g., -lstd). */
  char		*request_file_name;
			/* Name of the instantiation request file
			   associated with this file (if one exists).
			   NULL otherwise. */
  char		*template_info_file_name;
			/* Name of the template information file
			   associated with this file.  This field is set
			   whether or not the file exists. */

  char		*command_line;
			/* The command line to used to recompile the file. */
  char		*compilation_directory;
			/* The directory from which the file should be
			   recompiled. */
  char		*compilation_file_name;
			/* The file name to be recompiled, relative to the
			   directory in which the compilation is done. */
  char		*secondary_files;
			/* An optional list of secondary files to be used when
			   the compilation is done. */
  a_pl_file_list_entry_ptr
		dependencies;
			/* A list of file names on which this object file
			   depends.  The file will be recompiled if any of
			   the dependency files are newer than the object
			   file. */
  char		*instantiation_directory;
			/* The directory containing instantiations associated
			   with this file when one instantiation per object
			   mode is being used. */
  a_const_char	*reserved_lines[INSTANTIATION_REQUEST_LINES_RESERVED + 1];
			/* A buffer used to read the reserved lines of
			   an instantiation request file.  We add one to
			   the size because the number of reserved lines
			   could be zero. */
  a_pl_object_file_ptr
		objects;
			/* When is_archive is FALSE this points to a single
			   object file.  When is_archive is TRUE this points
			   to a list of object files. */
  a_pl_symbol_ptr
		request_list;
			/* List of symbols to be instantiated in this file. */
  a_byte_boolean
		is_archive;
			/* TRUE if the input file is an archive (.a) file. */
  a_byte_boolean
		request_file_updated;
			/* TRUE if the instantiation request file needs
			   to be rewritten because changes have been made
			   to the request list. */
  a_byte_boolean
		recompile;
			/* TRUE if, because of changes in the instantiation
			   list, the file needs to be recompiled. */
  a_byte_boolean
		is_local_file;
			/* TRUE if this file was generated by a compilation
			   in the current directory. */
  
} a_pl_input_file;


/* Structure used to represent a list of file names. */
typedef struct a_pl_file_list_entry {
  a_pl_file_list_entry_ptr
		next;	/* Pointer to the next entry on the list or NULL for
			   the last entry. */
  char		*name;	/* The name of the file. */
} a_pl_file_list_entry;


/* Structure that represents a mixture of command line arguments and
   pointers to input file entries. */
typedef struct a_pl_cmd_line_arg *a_pl_cmd_line_arg_ptr;
typedef struct a_pl_cmd_line_arg {
  a_pl_cmd_line_arg_ptr
		next;
			/* Pointer to the next argument in the list. */
  a_boolean	is_string;
			/* TRUE if the command line argument is a represented
			   as a string. */
  union {
    /* When is_string is TRUE. */
    char	*arg_string;
			/* The character string used as the argument. */
    /* When is_string is FALSE. */
    a_pl_input_file_ptr
		input_file_entry;
			/* When the argument is a file name, this points to
			   the input file entry associated with the file. */
  } variant;
} a_pl_cmd_line_arg;


/* Available lists for dynamically allocated structures. */
static a_pl_object_file_ptr		avail_pl_object_files = NULL;
static a_pl_symbol_ptr			avail_pl_symbols = NULL;
static a_pl_instantiation_site_ptr	avail_pl_instantiation_sites = NULL;

static a_pl_input_file_ptr
		pl_input_files = NULL;
			/* List of input files to be processed. */

static FILE	*f_command_output;
			/* Pointer to the file where the "nm" command
			   output can be read. */

static a_pl_symbol_ptr
		pl_symbol_table_head = NULL;
			/* Pointer to the head of the global symbol list. */

static a_pl_symbol_ptr
		specialization_list = NULL;
			/* Pointer to a list of specialization symbols. */

#define FILE_NAME_BUFFER_SIZE 4096
			/* Maximum size of a file name that can be
			   manipulated. */

static char	pl_file_name_buffer[FILE_NAME_BUFFER_SIZE];
			/* Buffer in which file_names can be manipulated. */

static a_boolean
		verbose = PL_DEFAULT_VERBOSE_MODE;
			/* Determines whether assignment information
			   should be displayed. */

static a_boolean
		suppress_compilation = FALSE;
			/* TRUE if request files should be updated but
			   compilations not done. */

static a_boolean
		mangled_names_in_output = FALSE;
			/* TRUE if identifiers should remain in mangled
			   form in messages that are displayed. */

static a_boolean
		limit_recursion = TRUE;
			/* TRUE if we should give up after a certain number
			   of iterations under the assumption that we've run
			   into an instantiation loop. */

static a_boolean
		do_not_assign_to_nonlocal_objects = FALSE;
			/* TRUE if assignments (and their resulting
			   compilations) may only be done to
			   local object files (i.e., those compiled in
			   the current directory. */

static a_boolean
		check_specialization_errors
				     = PL_DEFAULT_CHECK_SPECIALIZATION_ERRORS;
			/* TRUE if the prelinker should ensure that there
			   are not references to both the specialized and
			   unspecialized versions of a name. */

static a_boolean
		suppress_dependency_checking = FALSE;
			/* TRUE if the prelinker should not do dependency
			   checking of files used to define exported
			   templates. */

static char	**L_directories;
                        /* Pointer to an array of library directories
			   specified with the -L option. */

static int	num_of_L_directories = 0;
                        /* Number of entries in use in the L_directories
			   array. */

static a_boolean
		one_instantiation_per_object = FALSE;
			/* TRUE if one instantiation per object file mode is
			   being used. */

static a_boolean
		use_template_info_file = USE_TEMPLATE_INFO_FILE;
			/* TRUE if a template information file is used. */

static a_boolean
		use_definition_list = PL_DEFAULT_USE_DEFINITION_LIST;
			/* TRUE if the prelinker should create a definition
			   list file to be passed to the front end. */

static char	*temporary_file_name = NULL;
			/* The name to be used as a temporary file for
			   the creation of a definition list file. */

static FILE	*f_informational;
			/* The file to be used when displaying informational
			   messages. */
			

enum an_nm_format_kind {
	nmfk_default,
		/* SunOS 4.1. */
	nmfk_solaris,
		/* Solaris 2. */
	nmfk_SGI,
		/* Silicon Graphics. */
	nmfk_SVR4,
		/* Motorola 88K SVR4. */
        nmfk_HPUX,
		/* HP/UX. */
        nmfk_CLIX,
		/* Clipper (Intergraph). */
        nmfk_gnu,
		/* GNU binutils format. */
	nmfk_MacOSX,
		/* Apple MacOS X. */
	nmfk_MacOSX64,
		/* Apple MacOS X 64 bit (i.e., Snow Leopard and
		   beyond). */
	nmfk_lst
};

static an_nm_format_kind
		nm_format = nmfk_default;
			/* The kind of nm output that is expected. */

static a_boolean
		ignore_invalid_nm_output = FALSE;
			/* TRUE if we should simply ignore invalid nm
			   output lines. */

static a_const_char *message_prefix;
			/* String that is used as the prefix of all diagnostic
                           messages generated by the prelinker. */

static a_boolean
		skip_underscore_prefix
                                    = TARG_EXTERNAL_NAMES_GET_UNDERSCORE_ADDED;
			/* TRUE if external names have an extra underscore
			   prefix.  Can be modified by a command line
                           option. */

static a_boolean
		move_nonlocal_objects_to_curr_dir = FALSE;
			/* If a file from a nonlocal directory needs to be
			   recompiled to generate new instantiations, do
			   the recompilation in the local directory. */

static FILE	*f_obj_file_list = NULL;
			/* File variable for the list of object file names
			   to be passed back to the driver. */

#define CURR_DIR_NAME_SIZE 2048
			/* Maximum size of the current directory name. */

static char	curr_dir_name[CURR_DIR_NAME_SIZE];
			/* Name of the current working directory. */

static int	reserved_request_file_lines =
                                         INSTANTIATION_REQUEST_LINES_RESERVED;
			/* The number of lines of the instantiation
			   information file that are reserved and do
			   not contain instantiation list entries. */


#if DEBUG
static int pl_debug_level = 0;
static void pl_db_symbol(a_pl_symbol_ptr psp,
                         a_const_char    *prefix_string);
#endif /* DEBUG */

/*
Lines from "nm" are read into this buffer for analysis.
*/
#define PL_INPUT_LINE_SIZE 32767
typedef char		a_pl_input_line[PL_INPUT_LINE_SIZE];
static a_pl_input_line	pl_input_line;

/* The table used to record assignments of instantiations. */
#define PL_ASSIGNMENT_TABLE_SIZE	599
static a_pl_assignment_ptr	pl_assignment_table[PL_ASSIGNMENT_TABLE_SIZE];

/* The symbol table used for simulating the link. */
#define PL_SYMBOL_TABLE_SIZE	10007
static a_pl_symbol_ptr	pl_symbol_table[PL_SYMBOL_TABLE_SIZE];

/* The multiplier used in the hash algorithm that generates an index
   in the hash table from an identifier name string.  Do not change
   without investigating the hash table performance that results.
   Prime values are likely to work better than non-prime values. */
#define PL_HASH_FACTOR ((unsigned int)73)


static void pl_internal_error(a_const_char *error_string)
/*
Prints an internal error message and exits with a catastrophic error
exit status.
*/
{
  fprintf(stderr, "%s: internal error: %s\n", message_prefix, error_string);
#if EXIT_ON_INTERNAL_ERROR
  exit(RC_CATASTROPHE);
#else /* !EXIT_ON_INTERNAL_ERROR */
  (void)fflush(stderr);
  abort();
#endif /* EXIT_ON_INTERNAL_ERROR */
}  /* pl_internal_error */

#if CHECKING

static void pl_assertion_failed(a_const_char *filename,
				int          line_number,
				a_const_char *string1,
				a_const_char *string2)
/*
Print an error message when an assertion fails.
*/
{
  fprintf(stderr, "assertion failed: %s%s (%s, line %0d)\n", string1,
          string2, filename, line_number);
  pl_internal_error("assertion failed");
}  /* pl_assertion_failed */


/* Macro to test an assertion and generate an internal error if
   the condition is not TRUE.  The macro expands to nothing when checking
   code is not being used. */
#define check_assertion(test)                                           \
  (((test)) ? (void)0 :                                                 \
    pl_assertion_failed(__FILE__, __LINE__, "", ""))
#else /* CHECKING */
#define check_assertion(test) ((void)0)
#endif /* CHECKING */

/*
Include fe_util.h to bootstrap host_util.h and expose front end utility
functions for use in this program.
*/
#include "fe_util.h"

/*
The host_util.h file is used to define functions that are used by both
the front end, and utility programs such as the prelinker.  Include the
file here to define these functions for the prelinker.
*/

#include "host_util.h"

/*
Determine whether getcwd or getwd should be used to get the current
directory.  getwd is used on BSD, getcwd on other systems.
*/
#if __MICROSOFT_OS__
#define USE_GETCWD 1
#include <direct.h>
#else /* !__MICROSOFT_OS___ */
#if __BSD__
#include <sys/param.h>
EXTERN_C char* getwd(char *pathname);
#define USE_GETCWD 0
#else /* !__BSD__ */
#include <unistd.h>
#define USE_GETCWD 1
#endif /* __BSD__ */
#endif /* __MICROSOFT_OS__ */


static a_boolean pl_is_absolute_file_name(char *file_name)
/*
Test whether or not a file name is absolute (a full path name).
*/
{
#if WINDOWS_PATHS_ALLOWED
  return ((file_name)[0] == DIRECTORY_SEPARATOR) ||
         ((file_name)[0] == '\\') ||
         (isalpha((unsigned char)(file_name)[0]) && ((file_name)[1] == ':'));
#else /* !WINDOWS_PATHS_ALLOWED */
  return (file_name)[0] == DIRECTORY_SEPARATOR;
#endif /* WINDOWS_PATHS_ALLOWED */
}  /* pl_is_absolute_file_name */


static void pl_get_curr_dir_name(void)
/*
Get the current directory name and save it in curr_dir_name.
*/
{
#if USE_GETCWD
  if (getcwd(curr_dir_name, CURR_DIR_NAME_SIZE) == NULL) {
    pl_internal_error("getcwd failed");
  }  /* if */
#else /* !USE_GETCWD */
  (void)getwd(curr_dir_name);
#endif /* USE_GETCWD */
}  /* pl_get_curr_dir_name */


enum a_pl_error_code {
  pl_ec_no_longer_needed,
  pl_ec_assigned_to_file,
  pl_ec_message_prefix,
  pl_ec_executing,
  pl_ec_unrecognized_option,
  pl_ec_error,
  pl_ec_out_of_memory,
  pl_ec_invalid_input,
  pl_ec_bad_instantiation_request_file,
  pl_ec_invalid_nm_format_option,
  pl_ec_command_line_error,
  pl_ec_instantiation_loop,
  pl_ec_lib_file_not_found,
  pl_ec_error_occurred_during_name_decoding,
  pl_ec_warning,
  pl_ec_invalid_reserved_request_lines_option,
  pl_ec_cannot_open_obj_file_list_file,
  pl_ec_cannot_open_request_file,
  pl_ec_cannot_chdir,
  pl_ec_no_nm_info,
  pl_ec_popen_failed,
  pl_ec_specialized_and_instantiated,
  pl_ec_cannot_open_file_for_update,
  pl_ec_nm_returned_error,
  pl_ec_multiple_assignments,
  pl_ec_no_object_file_name_specified,
  pl_ec_invalid_definition_list_option,
  pl_ec_cannot_open_temporary_file,
  pl_ec_adopted_by_file,
  pl_ec_out_of_date,
  pl_ec_corrupted_template_info_file,
  pl_ec_last 	/* must be last */
};


static a_const_char *pl_error_text(a_pl_error_code error_code)
/*
For a given error code, return a pointer to the associated error
string.
*/
{
  a_const_char* m = NULL;
  switch (error_code) {
  case pl_ec_no_longer_needed:
    m = "%s: %s no longer needed in %s\n";
    break;
  case pl_ec_assigned_to_file:
    m = "%s: %s assigned to file %s\n";
    break;
  case pl_ec_message_prefix:
    m = "C++ prelinker";
    break;
  case pl_ec_executing:
    m = "%s: executing: %s\n";
    break;
  case pl_ec_unrecognized_option:
    m = "unrecognized option: %s\n";
    break;
  case pl_ec_error:
    m = "%s: error: ";
    break;
  case pl_ec_out_of_memory:
    m = "out of memory";
    break;
  case pl_ec_invalid_input:
    m = "invalid input format";
    break;
  case pl_ec_bad_instantiation_request_file:
    m = "bad instantiation request file -- "
        "instantiation assigned to more than one file";
    break;
  case pl_ec_invalid_nm_format_option:
    m = "invalid nm format option";
    break;
  case pl_ec_command_line_error:
    m = "command line error";
    break;
  case pl_ec_instantiation_loop:
    m = "instantiation loop";
    break;
  case pl_ec_lib_file_not_found:
    m = "library \"%s\" does not exist in the specified library directories\n";
    break;
  case pl_ec_error_occurred_during_name_decoding:
    m = "an error occurred during name decoding of \"%s\"";
    break;
  case pl_ec_warning:
    m = "%s: warning: ";
    break;
  case pl_ec_invalid_reserved_request_lines_option:
    m = "invalid reserved request file lines option \"%s\"";
    break;
  case pl_ec_cannot_open_obj_file_list_file:
    m = "cannot open object file name list file \"%s\"";
    break;
  case pl_ec_cannot_open_request_file:
    m = "cannot create instantiation request file \"%s\"";
    break;
  case pl_ec_cannot_chdir:
    m = "cannot change to directory \"%s\"";
    break;
  case pl_ec_no_nm_info:
    m = "no output produced by nm -- possible configuration problem";
    break;
  case pl_ec_popen_failed:
    m = "unable to create process for nm command";
    break;
  case pl_ec_specialized_and_instantiated:
    m = "\"%s\" has been referenced as both an explicit specialization "
        "and a generated instantiation";
    break;
  case pl_ec_cannot_open_file_for_update:
    m = "file \"%s\" is read-only";
    break;
  case pl_ec_nm_returned_error:
    m = "nm returned a nonzero error status";
    break;
  case pl_ec_multiple_assignments:
    m = "%s assigned to %s and %s\n";
    break;
  case pl_ec_no_object_file_name_specified:
    m =
 "-O and -N require a new object list file name specified with the -o option";
    break;
  case pl_ec_invalid_definition_list_option:
    m = "invalid definition list option \"%s\"";
    break;
  case pl_ec_cannot_open_temporary_file:
    m = "cannot create temporary file \"%s\"";
    break;
  case pl_ec_adopted_by_file:
    m = "%s: %s adopted by file %s\n";
    break;
  case pl_ec_out_of_date:
    m = "%s: rebuilding %s because %s (used by an exported template file) "
        "has changed\n";
    break;
  case pl_ec_corrupted_template_info_file:
    m = "corrupted template information file or instantiation request file";
    break;
  default:
    pl_internal_error("invalid error code");
  }  /* switch */
  return m;
}  /* pl_error_text */


static void pl_error_with_exit(a_pl_error_code	error_code,
                               char		*insertion_string,
                               a_boolean        exit_when_done)
/*
Prints an error message and exits with an error exit status.  A string
may be inserted into the message by passing a pointer to the string
to be inserted in inseration_string.  This will only be used if
the error text contains a corresponding %s.  If the message contains such
a %s, insertion_string must not be NULL.  If exit_when_done is TRUE,
exit will be called after the message is issued.
*/
{
  fprintf(stderr, pl_error_text(pl_ec_error), message_prefix);
  fprintf(stderr, pl_error_text(error_code), insertion_string);
  fprintf(stderr, "\n");
  if (exit_when_done) exit (RC_ERROR);
}  /* pl_error_with_exit */


static void pl_error(a_pl_error_code	error_code,
                     char		*insertion_string)
/*
Interface to pl_error_with_exit that passes that supplies an
"exit_when_done" argument.
*/
{

  pl_error_with_exit(error_code, insertion_string, /*exit_when_done=*/TRUE);
}  /* pl_error */


static void pl_warning(a_pl_error_code	error_code,
                       char		*insertion_string)
/*
Prints an warning message.  A string may be inserted into the message
by passing a pointer to the string to be inserted in
inseration_string.  This will only be used if the error text contains
a corresponding %s.  If the message contains such a %s,
insertion_string must not be NULL.
*/
{
  fprintf(stderr, pl_error_text(pl_ec_warning), message_prefix);
  fprintf(stderr, pl_error_text(error_code), insertion_string);
  fprintf(stderr, "\n");
}  /* pl_warning */

static a_void_ptr pl_malloc_with_check(sizeof_t size)
/*
Interface to malloc that allocates "size" bytes.  Checks for failure of 
allocation and generates a catastrophic error.
*/
{
  a_void_ptr	ptr;

  if ((ptr = (a_void_ptr)malloc(size)) == NULL) {
    pl_error(pl_ec_out_of_memory, (char *)NULL);
  }  /* if */
  return (ptr);
}  /* pl_malloc_with_check */


static a_void_ptr pl_realloc_with_check(a_void_ptr	old_ptr,
                                        sizeof_t	new_size)
/*
Interface to realloc: reallocate the block pointed to by "old_ptr" to give
it the new size "new_size".  If "old_ptr" is NULL, works like 
malloc_with_check.
*/
{
  a_void_ptr	ptr;

  /* Don't count on realloc allowing a first parameter of NULL to imply
     malloc-like behavior.  The SVID doesn't define realloc that way. */
  if (old_ptr == NULL) {
    ptr = pl_malloc_with_check(new_size);
  } else {
    ptr = (a_void_ptr)realloc((a_realloc_arg)old_ptr, new_size);
    if (ptr == NULL) {
      pl_error(pl_ec_out_of_memory, (char *)NULL);
    }  /* if */
  }  /* if */
  return ptr;
}  /* pl_realloc_with_check */


static void reset_pl_input_file(a_pl_input_file_ptr pifp)
/*
Clear the fields of an input file record that need to be
reset between iterations of the prelinker.
*/
{
  pifp->request_list = NULL;
  pifp->objects = NULL;
  pifp->is_archive = FALSE;
  pifp->request_file_updated = FALSE;
  pifp->recompile = FALSE;
  pifp->is_local_file = TRUE;
  pifp->command_line = NULL;
  pifp->compilation_directory = NULL;
  pifp->compilation_file_name = NULL;
  pifp->secondary_files = NULL;
  pifp->dependencies = NULL;
  pifp->instantiation_directory = NULL;
}  /* reset_pl_input_file */


static a_pl_input_file_ptr alloc_pl_input_file(void)
/*
Allocate an input file, initialize it, and return a pointer to it.
*/
{
  a_pl_input_file_ptr		pifp;

  pifp = (a_pl_input_file_ptr)pl_malloc_with_check(sizeof(a_pl_input_file));
  pifp->next = NULL;
  pifp->file_name = NULL;
  pifp->request_file_name = NULL;
  pifp->template_info_file_name = NULL;
  reset_pl_input_file(pifp);
  return pifp;
}  /* alloc_pl_input_file */


static a_pl_object_file_ptr alloc_pl_object_file(void)
/*
Allocate an object file, initialize it, and return a pointer to it.
*/
{
  a_pl_object_file_ptr		pofp;

  if (avail_pl_object_files != NULL) {
    pofp = avail_pl_object_files;
    avail_pl_object_files = pofp->next;
  } else {
    pofp =
          (a_pl_object_file_ptr)pl_malloc_with_check(sizeof(a_pl_object_file));
  }  /* if */
  pofp->next = NULL;
  pofp->file_name = NULL;
  pofp->symbols = NULL;
  pofp->included_in_output = FALSE;
  pofp->is_related_file = FALSE;
  pofp->modification_time = 0;
  return pofp;
}  /* alloc_pl_object_file */


static void free_pl_object_file(a_pl_object_file_ptr pofp)
/*
Return an object file to the available list.
*/
{
  pofp->next = avail_pl_object_files;
  avail_pl_object_files = pofp;
}  /* free_pl_object_file */


static a_pl_assignment_ptr alloc_pl_assignment(void)
/*
Allocate an assignment entry, initialize it, and return a pointer to it.
*/
{
  a_pl_assignment_ptr	ap;

  ap = (a_pl_assignment_ptr)pl_malloc_with_check(sizeof(a_pl_assignment));
  ap->next = NULL;
  ap->name = NULL;
  ap->times_assigned = 0;
  return ap;
}  /* alloc_pl_assignment */


static a_pl_file_list_entry_ptr alloc_pl_file_list_entry(void)
/*
Allocate a file list entry, initialize it, and return a pointer to it.
*/
{
  a_pl_file_list_entry_ptr	flep;

  flep = (a_pl_file_list_entry_ptr)pl_malloc_with_check(
                                                 sizeof(a_pl_file_list_entry));
  flep->next = NULL;
  flep->name = NULL;
  return flep;
}  /* alloc_pl_file_list_entry */


static a_pl_symbol_ptr alloc_pl_symbol(void)
/*
Allocate a symbol, initialize it, and return a pointer to it.
*/
{
  a_pl_symbol_ptr		psp;

  if (avail_pl_symbols != NULL) {
    psp = avail_pl_symbols;
    avail_pl_symbols = psp->next;
  } else {
    psp = (a_pl_symbol_ptr)pl_malloc_with_check(sizeof(a_pl_symbol));
  }  /* if */
  psp->name = NULL;
  psp->next = NULL;
  psp->next_in_symbol_table = NULL;
  psp->next_in_request_file = NULL;
  psp->next_in_specialization_list = NULL;
  psp->global_sym = NULL;
  psp->template_sym = NULL;
  psp->primary_entry = NULL;
  psp->instantiation_file = NULL;
  psp->possible_instantiation_sites = NULL;
  psp->referenced = FALSE;
  psp->defined = FALSE;
  psp->definition_seen_in_archive = FALSE;
  psp->tentative_definition = FALSE;
  psp->multiple_definition = FALSE;
  psp->is_template = FALSE;
  psp->can_be_instantiated = FALSE;
  psp->do_not_instantiate = FALSE;
  psp->instantiated = FALSE;
  psp->defined_in = NULL;
  return psp;
}  /* alloc_pl_symbol */


static a_pl_instantiation_site_ptr alloc_pl_instantiation_site(void)
/*
Allocate an instantiation_site, initialize it, and return a pointer to it.
*/
{
  a_pl_instantiation_site_ptr		pisp;

  if (avail_pl_instantiation_sites != NULL) {
    pisp = avail_pl_instantiation_sites;
    avail_pl_instantiation_sites = pisp->next;
  } else {
    pisp = (a_pl_instantiation_site_ptr)
                       pl_malloc_with_check(sizeof(a_pl_instantiation_site));
  }  /* if */
  pisp->next = NULL;
  pisp->input_file = NULL;
  return pisp;
}  /* alloc_pl_instantiation_site */


static void free_pl_instantiation_site(a_pl_instantiation_site_ptr pisp)
/*
Return an instantiation_site to the available list.
*/
{
  pisp->next = avail_pl_instantiation_sites;
  avail_pl_instantiation_sites = pisp;
}  /* free_pl_instantiation_site */


static void free_pl_symbol(a_pl_symbol_ptr psp)
/*
Return an input file to the available list.
*/
{
  psp->next = avail_pl_symbols;
  avail_pl_symbols = psp;
}  /* free_pl_symbol */


static a_boolean pl_read_input_line(FILE* f_input)
/*
Reads a line of input from f_input.  Returns TRUE if a line of
input is being returned.  Returns FALSE at end-of-file.  Sets "line_size"
to the number of characters read not including the trailing null character.
*/
{
  char*      buffer_pos = &pl_input_line[0];
  int        size = 0;
  int        ch;
  a_boolean  result;

  while ((ch = getc(f_input)), ch != EOF && ch != '\n') {
    if (++size > PL_INPUT_LINE_SIZE) {
      pl_internal_error("pl_read_input_line: input line too long.");
    }  /* if */
    *buffer_pos++ = ch;
  }  /* while */
  
  /* Terminate string with a null character. */
  *buffer_pos++ = '\0';

  /* Determine whether to return end-of-file (FALSE). */
  result = TRUE;
  if (ch == EOF && size == 0) result = FALSE;

  return (result);
}  /* pl_read_input_line */


static char *pl_copy_string(a_const_char *source)
/*
Allocate space for a copy of the string and make a copy.  Return a pointer
to the copy.
*/
{
  char	*dest;
  dest = (char *)malloc(strlen(source) + 1);
  strcpy(dest, source);
  return dest;
}  /* pl_copy_string */


static a_boolean pl_is_explicit_specialization(char	 *name)
/*
Return TRUE if name represents an explicit specialization of a template.
*/
{
  char		*ptr;
  /* Explicit specializations contain the string "__S" somewhere. */
  ptr =  strstr(name, "__S");
  return ptr != NULL;
}  /* pl_is_explicit_specialization */


/* The maximum size of an name that can be processed. */
#define NAME_DECODE_BUFFER_SIZE 32767

static char *get_nonspecialized_name(char *name)
/*
Given a specialized name "name", return a pointer to the name of the
nonspecialized version of the name.

*/
{
  static char	*name_buffer = NULL;
  char		*from;
  char		*to;

  if (name_buffer == NULL) {
    /* Allocate a buffer into which a copy of the unspecialized name can
       be made. */
    name_buffer = (char *)pl_malloc_with_check(NAME_DECODE_BUFFER_SIZE);
  }  /* if */
  /* Remove any occurrences of "__S" from the name. */
  from = name;
  to = name_buffer;
  while (*from != '\0') {
    if (*from == '_' && from[1] == '_' && from[2] == 'S') {
      from += 3;
      continue;
    }  /* if */
    *to++ = *from++;
  }  /* while */
  /* Terminate the new string. */
  *to = '\0';
  return name_buffer;
}  /* get_nonspecialized_name */


static void pl_invalid_input(void)
/*
Issue an invalid input error and exit.
*/
{
  if (!ignore_invalid_nm_output) pl_error(pl_ec_invalid_input, (char *)NULL);
}  /* pl_invalid_input */


static char *pl_decoded_name(char* encoded_name)
/*
Return a pointer to a temporary buffer containing a decoded name.
*/
{
  a_boolean	error;
  a_boolean	buffer_overflow;
  char		*result;
  static char	decode_buffer[NAME_DECODE_BUFFER_SIZE];
  sizeof_t	required_buffer_size;

  if (mangled_names_in_output) {
    /* Return the original name. */
    result = encoded_name;
#if IA64_ABI
  } else if (encoded_name[0] != '_' || encoded_name[1] != 'Z') {
    /* The IA-64 version of decode_identifier cannot be called for
       non-mangled names (as can occur with global scope inline
       variables).  Use the original name. */
    result = encoded_name;
#endif /* IA64_ABI */
  } else {
    decode_identifier(encoded_name, decode_buffer, NAME_DECODE_BUFFER_SIZE,
                      &error, &buffer_overflow, &required_buffer_size);
    result = decode_buffer;
    if (error) {
      /* An error occurred during decoding of the name.  Issue a diagnostic
         and return a pointer to the original message. */
      pl_warning(pl_ec_error_occurred_during_name_decoding, encoded_name);
      result = encoded_name;
    }  /* if */
  }  /* if */
  return result;
}  /* pl_decoded_name */


static a_boolean pl_scan_solaris_nm_line(char	**name1,
				         char	**name2,
				         char	*type,
				         char	**symbol_name)
/*
Read the output of the nm command.  This routine is written to accept
the output of the nm command on Solaris using the -p -x and -R options.

The nm output is expected to look like:

s.o:

0000000000 f s.o:s.c
0000000000 t s.o:static_func
0000000000 n s.o:DATA.
0000000008 n s.o:RDATA.
0000000000 b s.o:static_int
0000000000 D s.o:glob_def_int
0000000000 U s.o:extern_func
0000000004 D s.o:common_int
0000000000 U s.o:extern_int
0000000008 T s.o:defined_func


/usr/lib/libbsdmalloc.a[malloc.bsd43.o]:

0000000000 f /usr/lib/libbsdmalloc.a:malloc.bsd43.o:malloc.bsd43.c
0000000356 t /usr/lib/libbsdmalloc.a:malloc.bsd43.o:morecore
0000000004 b /usr/lib/libbsdmalloc.a:malloc.bsd43.o:pagesz
0000000000 b /usr/lib/libbsdmalloc.a:malloc.bsd43.o:pagebucket
0000000920 t /usr/lib/libbsdmalloc.a:malloc.bsd43.o:findbucket
0000000008 b /usr/lib/libbsdmalloc.a:malloc.bsd43.o:nextf
0000000528 T /usr/lib/libbsdmalloc.a:malloc.bsd43.o:free
0000000000 T /usr/lib/libbsdmalloc.a:malloc.bsd43.o:malloc
0000000592 T /usr/lib/libbsdmalloc.a:malloc.bsd43.o:realloc

/usr/lib/libbsdmalloc.a[xyz.o]:

0000000000 U /usr/lib/libbsdmalloc.a:xyz.o:sbrk
0000000000 U /usr/lib/libbsdmalloc.a:xyz.o:getpagesize
0000000000 U /usr/lib/libbsdmalloc.a:xyz.o:.div
0000000000 D /usr/lib/libbsdmalloc.a:xyz.o:realloc_srchlen
0000000000 U /usr/lib/libbsdmalloc.a:xyz.o:bcopy

Returns TRUE if the line contains symbol information; returns FALSE
if the line is a blank line, or a header line that should not be
processed further.
*/
{
  a_boolean	result = TRUE;
  char		*pos;
  char		*rest_of_line;
  char		ch;

  /* Clear the pointers to the returned values. */
  *name1 = *name2 = *symbol_name = NULL;
  *type = '\0';
  /* Find the first colon which terminates either the archive or the
     file name. */
  pos = strchr(pl_input_line, ':');
  if (pos == NULL) {
    /* Ignore blank lines.  A nonblank line that doesn't contain
       a colon is an error. */
    if (pl_input_line[0] != '\0') pl_invalid_input();
    result = FALSE;
  } else if (*(pos+1) == '\0') {
    /* A line that just contains a string like "xxx:".  This is an
       archive header that should be ignored. */
    result = FALSE;
  } else {
    /* Skip over the first field which is expected to contain the
       value field.  Skip to a blank. */
    pos = pl_input_line;
    while((ch = *pos), ch != ' ' && ch != '\0') pos++;
    /* Look for blank after value. */
    if (*pos++ != ' ') pl_invalid_input();
    /* Now look for a nonblank. */
    while (*pos == ' ') pos++;
    /* Get the type code. */
    *type = *pos++;
    if (!isalpha((unsigned char)(*type))) pl_invalid_input();
    /* Look for blank after type. */
    if (*pos++ != ' ') pl_invalid_input();
    /* Now look for a nonblank. */
    while (*pos == ' ') pos++;
    /* Note that the Solaris format is not expected to contain
       extra leading underscores. */
    rest_of_line = pos;
    pos = strchr(rest_of_line, ':');
    /* Replace the first colon with a NULL. */
    *pos = '\0';
    *name1 = rest_of_line;
    rest_of_line = pos + 1;
    pos = strchr(rest_of_line, ':');
    if (pos == NULL) {
      /* No second name exists. */
      *name2 = NULL;
    } else {
      /* Replace the second colon with a NULL to terminate the file
         name. */
      *pos = '\0';
      *name2 = rest_of_line;
      rest_of_line = pos + 1;
    }  /* if */
    *symbol_name = rest_of_line;
  }  /* if */
  return result;
}  /* pl_scan_solaris_nm_line */


static a_boolean pl_scan_alternate_nm_line(char	**name1,
				      char	**name2,
				      char	*type,
				      char	**symbol_name)
/*
Read the output of the nm command.  This routine is written to accept
the output of the nm command on systems such as HP/UX and Motorola 88000 SVR4
using the -p, -x and -r options.

For HP/UX the nm output is expected to look like:

t.o:
t.o:                0000000200 T  main
t.o:                0000000000 U  _main
t.o:                0000000240 t  __ct__1AFv
t.o:                0000000000 U  __nw__FUi
t.o:                0000000000 U  printf
t.o:                0000000000 U  __dl__FPv

t2.o:
t2.o:               0000000004 c  __TIR____ct__10A__pt__2_iFv
t2.o:               0000000004 c  __CBI____ct__10A__pt__2_iFv
t2.o:               0000000200 T  main
t2.o:               0000000248 T  __cgi__t2_c_Fri_Sep_10_15_09_24_1993_
t2.o:               0000000000 U  _main
t2.o:               0000000000 U  __ct__10A__pt__2_iFv

.../lib/libC.a[main.o]:
.../lib/libC.a:      0000000004 c  __head
.../lib/libC.a:      0000000200 T  _main
.../lib/libC.a:      0000000000 U  __call_ctors__Fv
.../lib/libC.a:      0000000232 T  __cgi__main_c_Fri_Sep_10_14_43_17_1993_

.../lib/libC.a[placenew.o]:
.../lib/libC.a:      0000000200 T  __nw__FUiPv
.../lib/libC.a:      0000000216 T  __cgi__placenew_c_Fri_Sep_10_14_43_20_1993_

For SVR4 the nm output is expected to look like:

s.o:

0000000000 D s.o:glob_def_int
0000000036 T s.o:defined_func
0000000000 U s.o:extern_int
0000000000 U s.o:extern_func
0000000008 D s.o:common_int


/usr/lib/libapg.a[new.o]:

0000000000 U new.o:_getpid
0000000000 U new.o:_kill


/usr/lib/libapg.a[hack.o]:

0000000000 U hack.o:_fcvt
0000000000 U hack.o:_ecvt
0000000000 U hack.o:_fcntl


For CLIX the nm output is expected to look like:

s.o:
s.o:                00000000 t _static_func
s.o:                00000004 T _defined_func
s.o:                00000000 U _extern_int
s.o:                00000000 U _extern_func
s.o:                00000096 d _static_int
s.o:                00000100 D _glob_def_int
s.o:                00000004 C _common_int

/usr/lib/liby.a[libmai.o]:
/usr/lib/liby.a:    00000000 T _main
/usr/lib/liby.a:    00000000 U _yyparse

/usr/lib/liby.a[libzer.o]:
/usr/lib/liby.a:    00000000 T _yyerror
/usr/lib/liby.a:    00000000 U __iob
/usr/lib/liby.a:    00000000 U _fprintf


Returns TRUE if the line contains symbol information; returns FALSE
if the line is a blank line, or a header line that should not be
processed further.
*/
{
  a_boolean	result = TRUE;
  char		*pos;
  char		*rest_of_line;
  char		ch;
  static char	*name1_buffer = NULL;
  static char	*name2_buffer = NULL;
  static a_boolean
		name2_is_NULL = FALSE;

  /* On the first call allocate a buffer that can be used to store the
     archive name. */
  if (name1_buffer == NULL) {
    name1_buffer = (char*)pl_malloc_with_check(size_t_arg(PL_INPUT_LINE_SIZE));
    name2_buffer = (char*)pl_malloc_with_check(size_t_arg(PL_INPUT_LINE_SIZE));
  }  /* if */
  /* Clear the pointers to the returned values. */
  *name1 = *name2 = *symbol_name = NULL;
  *type = '\0';
  /* Find the first colon which terminates either the archive or the
     file name. */
  pos = strchr(pl_input_line, ':');
  if (pos == NULL) {
    /* Ignore blank lines.  A nonblank line that doesn't contain
       a colon is an error. */
    if (pl_input_line[0] != '\0') pl_invalid_input();
    result = FALSE;
  } else if (strchr(pl_input_line, ' ') == NULL) {
    char	*bracket_pos;
    /* A line that just contains a string like "xxx.o:" or "libx.a[xxx.o]:".
       This is an archive header.  Extract the archive and object file names.
       After the names are extracted the line is not processed. */
    result = FALSE;
    /* Find the "[" that separates the archive name from the object
       file name.  Use the position of the colon if no bracket is found. */
    bracket_pos = strchr(pl_input_line, '[');
    if (bracket_pos != NULL) pos = bracket_pos;
    /* Replace the "[" (or ":") with a NULL. */
    *pos = '\0';
    (void)strcpy(name1_buffer, pl_input_line);
    rest_of_line = pos + 1;
    name2_is_NULL = (bracket_pos == NULL);
    if (!name2_is_NULL) {
      /* Find the "]" that terminates the object file name. */
      pos = strchr(rest_of_line, ']');
      *pos = '\0';
      (void)strcpy(name2_buffer, rest_of_line);
     }  /* if */
  } else {
    /* Return pointers to the names from the header line. */
    *name1 = name1_buffer;
    *name2 = name2_is_NULL ? NULL : name2_buffer;
    /* On HP/UX and some other systems the file name begins each line,
       skip past this file name. */
    if (nm_format == nmfk_HPUX || nm_format == nmfk_CLIX) {
      rest_of_line = pos + 1;
    } else {
      rest_of_line = pl_input_line;
    }  /* if */
    /* The value field may optionally be preceded by one or more blanks.
       Skip over any blanks that appear here. */
    pos = rest_of_line;
    while (*pos == ' ') pos++;
    /* Skip over the first field which is expected to contain the
       value field.  Skip to a blank. */
    while((ch = *pos), ch != ' ' && ch != '\0') pos++;
    /* Look for blank after value. */
    if (*pos++ != ' ') pl_invalid_input();
    /* Now look for a nonblank. */
    while (*pos == ' ') pos++;
    /* Get the type code. */
    *type = *pos++;
    if (!isalpha((unsigned char)(*type))) pl_invalid_input();
    if (nm_format == nmfk_HPUX) {
      /* HP/UX uses lower case letters for some types. */
      switch (*type) {
        case 'c':	*type = 'C'; break;
      }  /* switch */
    }  /* if */
    /* HP/UX has multi-character types.  Skip any remaining characters. */
    if (nm_format == nmfk_HPUX) while (*pos != ' ' && *pos != '\0') pos++;
    /* Look for blank after type. */
    if (*pos++ != ' ') pl_invalid_input();
    /* Now look for a nonblank. */
    while (*pos == ' ') pos++;
    if (nm_format == nmfk_CLIX) {
      /* Skip passed extra underscore at the start of every symbol if an
         underscore is present.   This is only done for CLIX. */
      if (skip_underscore_prefix && *pos == '_') pos++;
    }  /* if */
    rest_of_line = pos;
    /* If the name was not at the start of the line then it is expected
       to appear here.  Skip past the name. */
    if (nm_format != nmfk_HPUX && nm_format != nmfk_CLIX) {
      pos = strchr(rest_of_line, ':');
      rest_of_line = pos + 1;
    }  /* if */
    *symbol_name = rest_of_line;
  }  /* if */
  return result;
}  /* pl_scan_alternate_nm_line */


static a_boolean pl_scan_default_nm_line(char	**name1,
					 char	**name2,
					 char	*type,
					 char	**symbol_name)
/*
Read the output of the nm command.  This routine is written to accept
the output of the nm command on SunOS, SGI and the Gnu binutils nm
command, and may have to be modified for other systems.

Default nm output is expected to look like:

i1.o:00000001 C ___CBI__f__10A__pt__2_dFv
i1.o:0000018c T _f__10A__pt__2_iFv
i1.o:0000016c T _f__Fi
i1.o:00000250 T _g__10A__pt__2_dFv
i1.o:000001ac T _g__10A__pt__2_iFv
i1.o:         U _i2__Fv
i1.o:00000110 T _main
i1.o:         U _x__Fv
i2.o:00000001 C ___CBI__f__10A__pt__2_cFv
i2.o:00000328 T ___cgi__i2_c_Mon_Oct__4_16_54_51_1993_
i2.o:         U ___nw__FUi
i2.o:000001e0 T _f__10A__pt__2_cFv
i2.o:00000284 T _f__10A__pt__2_fFv
i2.o:         U _f__Ff
i2.o:         U _f__Fi
i2.o:00000200 T _g__10A__pt__2_cFv
i2.o:000002a4 T _g__10A__pt__2_fFv
i2.o:00000110 T _i2__Fv

libi.a:
libi.a:x.o:00000004 C ___CBI__f__10A__pt__2_dFv
libi.a:x.o:00000004 C ___CBI__g__10A__pt__2_dFv
libi.a:x.o:00000004 C ___TIR__f__10A__pt__2_dFv
libi.a:x.o:00000004 C ___TIR__g__10A__pt__2_dFv
libi.a:x.o:0000012c T ___cgi__x_c_Tue_Aug_17_14_11_40_1993_
libi.a:y.o:00000110 T _x__Fv
libi.a:y.o:00000004 C ___CBI__f__10A__pt__2_dFv
libi.a:y.o:00000004 C ___CBI__g__10A__pt__2_dFv
libi.a:y.o:00000004 C ___TIR__f__10A__pt__2_dFv
libi.a:y.o:00000004 C ___TIR__g__10A__pt__2_dFv
libi.a:y.o:00000110 T _y__Fv

SGI nm output is expected to look like:

Z.a:A.o:        00000120 t __dt__1AFv
Z.a:A.o:        000001c4 t set__1AFi
Z.a:A.o:        000001d8 t __dt__1BFv
Z.a:A.o:        00000000 T foo__Fv
Z.a:A.o:        00000000 U __nw__FUi
Z.a:A.o:        0000003c D __ptbl_vec__A_C_foo_
Z.a:A.o:        000000cc T main
Z.a:A.o:        00000000 U printf
Z.a:A.o:        00000000 U __dl__FPv
Z.a:A.o:        00000000 U _gp_disp
Z.a:Q.o:        00000000 b foo
Z.a:Q.o:        00000000 t func__Fv
Z.a:Q.o:        00000000 D Name__4Test
Z.a:Q.o:        00000000 U z__4Test
Z.a:Q.o:        00000028 T __ct__4TestFv
Z.a:Q.o:        00000000 U __nw__FUi
Z.a:Q.o:        00000008 D bar
Z.a:Q.o:        000000a8 T main
Z.a:Q.o:        00000000 U _gp_disp
X.o:    00000320 t __dt__1BFv
X.o:    00000000 T __dt__1AFv
X.o:    00000000 U __dl__FPv
X.o:    00000000 D bar
X.o:    0000008c T main
X.o:    00000000 U __nw__FUi
X.o:    00000000 U printf
X.o:    00000030 D __vtbl__1B__X_C
X.o:    00000048 D __vtbl__1A
X.o:    00000000 U _gp_disp

Gnu binutils nm output is expected to look like:

s.o:00000004 C ___CBI__f__Fi
s.o:00000004 C ___TIR__f__Fi
s.o:         U ___main
s.o:         U __main
s.o:         U f(int)
s.o:00000000 T _main
/usr/lib/libbsd.a(daemon.o):         U _chdir
/usr/lib/libbsd.a(daemon.o):         U _close
/usr/lib/libbsd.a(daemon.o):00000010 T _daemon
/usr/lib/libbsd.a(daemon.o):         U _dup2
/usr/lib/libbsd.a(daemon.o):         U _exit
/usr/lib/libbsd.a(daemon.o):         U _fork
/usr/lib/libbsd.a(daemon.o):         U _open
/usr/lib/libbsd.a(daemon.o):         U _setsid
/usr/lib/libbsd.a(logwtmp.o):         U _close
/usr/lib/libbsd.a(logwtmp.o):         U _fstat
/usr/lib/libbsd.a(logwtmp.o):         U _ftruncate
/usr/lib/libbsd.a(logwtmp.o):00000010 T _logwtmp
/usr/lib/libbsd.a(logwtmp.o):         U _open
/usr/lib/libbsd.a(logwtmp.o):         U _strncpy
/usr/lib/libbsd.a(logwtmp.o):         U _time
/usr/lib/libbsd.a(logwtmp.o):         U _write

Returns TRUE if the line contains symbol information; returns FALSE
if the line is a blank line, or a header line that should not be
processed further.
*/
{
  a_boolean	result = TRUE;
  char		*pos;
  char		*rest_of_line;
  char		ch;

  /* Clear the pointers to the returned values. */
  *name1 = *name2 = *symbol_name = NULL;
  *type = '\0';
  pos = pl_input_line;
#if WINDOWS_PATHS_ALLOWED
  /* Skip over the ":" that marks end of drive name, if any. */
  if (*(pos+1) == ':') pos += 2;
#endif /* WINDOWS_PATHS_ALLOWED */
  /* Find the first colon which terminates either the archive or the
     file name. */
  pos = strchr(pos, ':');
  if (pos == NULL) {
    /* Ignore blank lines.  A nonblank line that doesn't contain
       a colon is an error. */
    if (pl_input_line[0] != '\0') pl_invalid_input();
    result = FALSE;
  } else if (*(pos+1) == '\0') {
    /* A line that just contains a string like "xxx:".  This is an
       archive header that should be ignored. */
    result = FALSE;
  } else {
    if (nm_format == nmfk_gnu) {
      char	*paren_pos;
      /* Gnu nm output. */
      /* Replace the first colon with a NULL. */
      *pos = '\0';
      rest_of_line = pos + 1;
      paren_pos = strchr(pl_input_line, '(');
      if (paren_pos == NULL) {
        /* Just a simple file name (i.e., not an archive member). */
        *name1 = pl_input_line;
        *name2 = NULL;
      } else {
        char	*end_of_name2;
        /* A line of the form "archive(member): ..." */
        *name1 = pl_input_line;
        /* Terminate the archive name string. */
        *paren_pos = '\0';
        /* Get a pointer to the start of "member". */
        *name2 = paren_pos + 1;
        end_of_name2 = strchr(*name2, ')');
        /* Terminate the second file name. */
        *end_of_name2 = '\0';
      }  /* if */
    } else {
      /* Non-Gnu format. */
      /* Replace the first colon with a NULL. */
      *pos = '\0';
      *name1 = pl_input_line;
      rest_of_line = pos + 1;
      pos = strchr(rest_of_line, ':');
      if (pos == NULL) {
        /* No second name exists. */
        *name2 = NULL;
      } else {
        /* Replace the second colon with a NULL to terminate the file
           name. */
        *pos = '\0';
        *name2 = rest_of_line;
        rest_of_line = pos + 1;
      }  /* if */
    }  /* if */
    pos = rest_of_line;
    if (nm_format == nmfk_SGI) {
      /* The value field may optionally be preceded by one or more blanks.
         Skip over any blanks that appear here. */
      while (*pos == ' ') pos++;
    }  /* if */
    if (nm_format == nmfk_MacOSX) {
      /* On MacOS, the name is followed by a space and then an 8 character
         value or 8 blanks.  Skip those 9 characters. */
      int i;
      for (i = 0; i < 9; ++i) {
        if (*pos == '\0') pl_invalid_input();
        pos++;
      }  /* for */
    } else if (nm_format == nmfk_MacOSX64) {
      /* On MacOS64, the name is followed by a space and then a 16 character
         value or 16 blanks.  Skip those 17 characters. */
      int i;
      for (i = 0; i < 17; ++i) {
        if (*pos == '\0') pl_invalid_input();
        pos++;
      }  /* for */
    } else {
      /* Skip over the first field which is expected to contain the
         value field.  Skip to a blank. */
      while((ch = *pos), ch != ' ' && ch != '\0') pos++;
    }  /* if */
    /* Look for blank after value. */
    if (*pos++ != ' ') pl_invalid_input();
    /* Now look for a nonblank. */
    while (*pos == ' ') pos++;
    /* Get the type code. */
    *type = *pos++;
    if (!isalpha((unsigned char)(*type))) pl_invalid_input();
    /* Look for blank after type. */
    if (*pos++ != ' ') pl_invalid_input();
    /* Skip passed extra underscore at the start of every symbol if an
       underscore is present.  */
    if (skip_underscore_prefix && *pos == '_') pos++;
    *symbol_name = pos;
    if (nm_format == nmfk_SGI) {
      /* The symbol name may optionally have some extra information after it:
         ex.o:   0000034c T __ct__17A__pt__9_7istreamFv (multiext)
         We scan the rest of the line, and if we hit a blank before reaching
         the end of the line, the blank is replaced with a NULL */
      while((ch = *pos), ch != ' ' && ch != '\0') pos++;
      if (ch == ' ') *pos = '\0';
    }  /* if */
  }  /* if */
  return result;
}  /* pl_scan_default_nm_line */


static void pl_read_nm_output(void)
/*
This routine reads the output of the nm command and builds a data
structure containing the information.  The base of the structure
is a linked list of input files that can be object files or archive files.
Each input file points to one or more object files.  An input file
that is an object file points to a single object file structure
while an archive points to a linked list of object files.
Each object file points to a linked list of symbols that are referenced
or defined in that object file.
*/
{
  char			*input_file_name = NULL;
  char			*obj_file_name = NULL;
  a_boolean		is_archive = FALSE;
  a_pl_object_file_ptr	objects_tail = NULL;
  a_pl_object_file_ptr	pofp = NULL;
  a_pl_input_file_ptr	pifp = NULL;
  a_boolean		any_lines_read = FALSE;

  while (pl_read_input_line(f_command_output)) {
    char		*name1;
    char		*name2;
    char		type;
    char		*symbol_name;
    a_pl_symbol_ptr	psp;
    a_boolean		process_line;
#if DEBUG
    if (pl_debug_level >= 4) {
      fprintf(stderr, "%s\n", pl_input_line);
    }  /* if */
#endif /* DEBUG */
    any_lines_read = TRUE;

    if (nm_format == nmfk_solaris) {
      process_line = pl_scan_solaris_nm_line(&name1, &name2, &type,
                                             &symbol_name);
    } else if (nm_format == nmfk_SVR4 ||
               nm_format == nmfk_HPUX ||
               nm_format == nmfk_CLIX) {
      process_line = pl_scan_alternate_nm_line(&name1, &name2, &type,
                                               &symbol_name);
    } else {
      /* SGI uses a variant of the default format. */
      process_line = pl_scan_default_nm_line(&name1, &name2, &type,
                                             &symbol_name);
    }  /* if */
#if DEBUG
    if (pl_debug_level >= 5) {
      fprintf(stderr, "nm info: name1: %s, name2: %s, type: %c, sym: %s\n",
              name1 == NULL ? "<NULL>" : name1,
              name2 == NULL ? "<NULL>" : name2,
              type,
              symbol_name == NULL ? "<NULL>" : symbol_name);
    }  /* if */
#endif /* DEBUG */
    /* Is this a line that should be skipped such as a blank line or
       header line? */
    if (!process_line) continue;
    /* See if this is the start of a new input file. */
    if (input_file_name == NULL ||
        strcmp(input_file_name, name1) != 0) {
      /* The start of a new input file. */
      /* The input file is an archive if there is also an object name in
         the input line. */
      is_archive = name2 != NULL;
      for (pifp = pl_input_files; pifp != NULL; pifp = pifp->next) {
        /* If the object is from an archive, make sure that the archive
           name matches the input file name.  If the input is not from
           an archive, we then go on to see if any of the object file names
           associated with the input file match.  This is done to find
           an instantiation object file associated with a "primary" object
           file. */
        if (is_archive && strcmp(pifp->file_name, name1) != 0) continue;
        /* Don't search the object file list associated with archives -- they
           can't have associated instantiation files. */
        if (pifp->is_archive) continue;
        /* Look for a match among the instantiation object files associated
           with this input file. */
        for (pofp = pifp->objects; pofp != NULL; pofp = pofp->next) {
          if (strcmp(pofp->file_name, name1) == 0) break;
        }  /* if */
        if (pofp != NULL) break;
        /* If we didn't find a matching object, but the input file matches,
           stop the loop.  This occurs when an input file has no template
           information file. */
        if (strcmp(pifp->file_name, name1) == 0) break;
      }  /* for */
      if (pifp == NULL) {
        pl_internal_error("Input file not in list");
      }  /* if */
      input_file_name = pifp->file_name;
      pifp->is_archive = is_archive;
      objects_tail = NULL;
      if (is_archive) {
        obj_file_name = NULL;
      } else {
        if (pofp == NULL) {
          /* If an instantiation object file with a matching name was not
             found allocate an object file for what should be a primary input
             file and link it to the input file. */
          pofp = alloc_pl_object_file();
          pofp->next = pifp->objects;
          pifp->objects = pofp;
          pofp->file_name = pl_copy_string(input_file_name);
        }  /* if */
      }  /* if */
    }  /* if */
    /* See if this is the start of a new object file within an archive. */
    if (is_archive) {
      if (obj_file_name == NULL ||
          strcmp(obj_file_name, name2) != 0) {
        /* This is a new object file within the archive. */
        obj_file_name = pl_copy_string(name2);
        pofp = alloc_pl_object_file();
        pofp->file_name = obj_file_name;
        /* Add this object file to the list of objects pointed to by the
           input file entry. */
        if (pifp->objects == NULL) pifp->objects = pofp;
        if (objects_tail != NULL) objects_tail->next = pofp;
        objects_tail = pofp;
      }  /* if */
    }  /* if */
    /* See if this is a type of line for which the symbol information should
       be recorded. */
    if (type != 'B' &&
        type != 'D' &&
        type != 'L' &&
        type != 'R' &&
        type != 'S' &&
        type != 'T' &&
        type != 'U' &&
        type != 'V' &&
        type != 'W' &&
        type != 'C') {
       /* Not a type of symbol that we need to process.  Only global
          symbols are processed. */
    } else {
      psp = alloc_pl_symbol();
      psp->name = pl_copy_string(symbol_name);
      /* Set symbol flags. */
      switch (type) {
        case 'B':  /* BSS symbol */
        case 'D':  /* data symbol */
        case 'L':  /* thread-local symbol */
        case 'R':  /* read-only data symbol */
        case 'S':  /* Uninitialized small object. */
        case 'T':  /* text symbol */
        case 'V':  /* Weak definition */
        case 'W':  /* Weak definition */
          psp->defined = TRUE;
          break;
        case 'U':  /* undefined symbol */
          psp->referenced = TRUE;
          break;
        case 'C':  /* common (tentative definition) */
          psp->tentative_definition = TRUE;
          break;
        default:
          break;
      }  /* switch */
      /* Link onto front of list associated with the current object file. */
      psp->next = pofp->symbols;
      pofp->symbols = psp;
    }  /* if */
  }  /* while */
  if (!any_lines_read) {
    /* If there were no nm lines read, issue a warning.  This usually
       indicates that the nm command being used in not correct. */
    pl_warning(pl_ec_no_nm_info, NULL);
  }  /* if */
  return;
}  /* pl_read_nm_output */


static void add_possible_instantiation_site(a_pl_symbol_ptr	psp,
					    a_pl_input_file_ptr	pifp)
/*
Add an input file to a list of files that can instantiate a given symbol.
*/
{
  a_pl_instantiation_site_ptr	pisp;

  /* It is possible for this routine to be called more than once for
     the same input file.  Since the input files are processed one at
     a time we can detect this by checking whether the current input
     file is already pointed to by the first entry. */
  if (psp->possible_instantiation_sites == NULL ||
      psp->possible_instantiation_sites->input_file != pifp) {
    pisp = alloc_pl_instantiation_site();
    pisp->input_file = pifp;
    pisp->next = psp->possible_instantiation_sites;
    psp->possible_instantiation_sites = pisp;
  }  /* if */
}  /* add_possible_instantiation_site */


static unsigned int hash_value_for_name(char	*name)
/*
Return a hash for "name".
*/
{
  unsigned int	hash_value = 0;
  char		*ptr;
  int		length;

  /* Hash the symbol's identifier.  This involves taking the identifier's
     first 3, last 3, and middle 3 characters.  Of course, if the identifier
     has 9 or fewer characters, take the entire identifier. */
  length = (unsigned int)strlen(name);
  ptr = name;
  if (length > 9) {
    hash_value = (unsigned int)*ptr++;
    hash_value = (hash_value * PL_HASH_FACTOR) + (unsigned int)*ptr++;
    hash_value = (hash_value * PL_HASH_FACTOR) + (unsigned int)*ptr;
    ptr = name + (length >> 1) - 1;
    hash_value = (hash_value * PL_HASH_FACTOR) + (unsigned int)*ptr++;
    hash_value = (hash_value * PL_HASH_FACTOR) + (unsigned int)*ptr++;
    hash_value = (hash_value * PL_HASH_FACTOR) + (unsigned int)*ptr;
    ptr = name + length - 3;
    hash_value = (hash_value * PL_HASH_FACTOR) + (unsigned int)*ptr++;
    hash_value = (hash_value * PL_HASH_FACTOR) + (unsigned int)*ptr++;
    hash_value = (hash_value * PL_HASH_FACTOR) + (unsigned int)*ptr;
  } else {
    int a;
    for (a = 0; a < length; a++) {
      hash_value = (hash_value * PL_HASH_FACTOR) + (unsigned int)*ptr++;
    }  /* for */
  }  /* if */
  return hash_value;
}  /* hash_value_for_name */


static a_pl_assignment_ptr pl_find_assignment(char	*name)
/*
Return the assignment entry for "name".  Create a new entry if
none already exists.
*/
{
  unsigned int		hash_value;
  a_pl_assignment_ptr	ap;
  int			bucket_number;

  hash_value = hash_value_for_name(name);
  bucket_number = hash_value % PL_ASSIGNMENT_TABLE_SIZE;
  /* Look for an existing entry. */
  for (ap = pl_assignment_table[bucket_number]; ap != NULL; ap = ap->next) {
    if (strcmp(ap->name, name) == 0) {
      break;
    }  /* if */
  }  /* for */
  if (ap == NULL) {
    /* None was found.  Create one. */
    ap = alloc_pl_assignment();
    ap->name = pl_copy_string(name);
    /* Link the entry into its bucket. */
    ap->next = pl_assignment_table[bucket_number];
    pl_assignment_table[bucket_number] = ap;
  }  /* if */
  return ap;
}  /* pl_find_assignment */


static a_pl_symbol_ptr pl_find_symbol(char		*name,
                                      a_pl_symbol_ptr	other_sym,
				      a_boolean		add,
				      a_boolean		*p_new)
/*
Find a symbol entry with the specified name.  Add the name to the
list if an entry does not already exist.  If p_new is not NULL,
return a flag indicating whether a new entry was created by this
call.
*/
{
  unsigned int		       hash_value;
  a_pl_symbol_ptr	       prev_sym_ptr;
  a_pl_symbol_ptr              sym_ptr    = NULL;
  int                          bucket_number;
  a_boolean		       is_new = FALSE;

  /* If the symbol pointer passed from the caller already contains a pointer
     to the global symbol then simply return that value.  Otherwise,
     look it up in the symbol table. */
  if (other_sym != NULL && other_sym->global_sym != NULL) {
    sym_ptr = other_sym->global_sym;
    goto symbol_found;
  }  /* if */
  hash_value = hash_value_for_name(name);
  /* Look in the symbol bucket saving the position in case this symbol needs
     to be added. */
  bucket_number = hash_value % PL_SYMBOL_TABLE_SIZE;
  if ((sym_ptr = pl_symbol_table[bucket_number]) != NULL) {
    prev_sym_ptr = NULL;
    do {
      if (strcmp(name, sym_ptr->name) == 0) {
        /* We have a match. */
        /* Relink the symbol header at the front of the list of headers,
           so that frequently-used headers will be found quickly. */
        if (prev_sym_ptr != NULL) {
          prev_sym_ptr->next = sym_ptr->next;
          sym_ptr->next = pl_symbol_table[bucket_number];
          pl_symbol_table[bucket_number] = sym_ptr;
        }  /* if */
        goto symbol_found;
      }  /* if */
      prev_sym_ptr = sym_ptr;
    } while ((sym_ptr = sym_ptr->next) != NULL);
  }  /* if */

  /* Exiting this loop indicates that the symbol does not exist in the table;
     allocate a symbol header for it. */
  if (add) {
    sym_ptr = alloc_pl_symbol();
    /* Add this to the list of symbols in the global symbol table. */
    sym_ptr->next_in_symbol_table = pl_symbol_table_head;
    pl_symbol_table_head = sym_ptr;

    /* Link the new header onto the front of the appropriate bucket of the
       symbol table. */
    sym_ptr->next = pl_symbol_table[bucket_number];
    pl_symbol_table[bucket_number] = sym_ptr;
    sym_ptr->name = pl_copy_string(name);
    if (pl_is_explicit_specialization(sym_ptr->name)) {
      /* This name is an explicit specialization.  Add it to a list
         of specializations. */
      sym_ptr->is_specialization = TRUE;
      sym_ptr->next_in_specialization_list = specialization_list;
      specialization_list = sym_ptr;
    }  /* if */
#if DEBUG
    if (pl_debug_level >= 5) {
      fprintf(stderr, "Adding %s to bucket %d\n", name, bucket_number);
    }  /* if */
#endif /* DEBUG */
    is_new = TRUE;
  }  /* if */

symbol_found:
  /* If this symbol refers to an entry point, return the primary
     entry instead. */
  if (sym_ptr != NULL && sym_ptr->primary_entry != NULL) {
    sym_ptr = sym_ptr->primary_entry;
  }  /* if */
  if (sym_ptr != NULL) {
    if (other_sym != NULL && other_sym->global_sym == NULL) {
      /* Record a pointer to the global symbol in the symbol passed by the
         caller. */
      other_sym->global_sym = sym_ptr;
    }  /* if */
  }  /* if */
  /* Return the new flag if the p_new pointer provided by caller is
     non-NULL. */
  if (p_new != NULL) *p_new = is_new;
  return sym_ptr;
}  /* pl_find_symbol */


static void pl_add_predefined_names(void)
/*
This routine is used to introduce names that the linker predefines.
This is not strictly needed because the prelinker doesn't issue
undefined errors.  This can be used if you want the prelinker
to detect such conditions.
*/
{
  char			*name;
  int			pos = 0;
  a_pl_symbol_ptr	sym;

  for (;;) {
    name = pl_predefined_names[pos++]; 
    if (name == NULL) break;
    sym = pl_find_symbol(name, (a_pl_symbol_ptr)NULL, /*add=*/TRUE,
                         (a_boolean*)NULL);
    sym->defined = TRUE;
  }  /* for */
}  /* pl_add_predefined_names */


static void pl_add_symbols_from_object(a_pl_object_file_ptr pofp,
				       a_pl_input_file_ptr  input_file)
/*
Add all of the symbols from a given object file to the global symbol
table.  This handles regular symbols and also processes symbols that
have special meaning for the prelinker.  The special symbols begin
with the prefixes __TIR__, __CBI__, and __DNI__.  When a special symbol
is processed the related symbol (the symbol name with the prefix
removed) is updated to reflect the information provided by the special
symbol.
*/
{
  a_pl_symbol_ptr	psp;

  psp = pofp->symbols;
  while (psp != NULL) {
    a_pl_symbol_ptr	sym;
    a_boolean		is_special_symbol = FALSE;
    if (psp->name[0] == '_' && psp->name[1] == '_') {
      if (strncmp(psp->name, PL_INSTANCE_REQUIRED_PREFIX,
                  PL_INSTANCE_REQUIRED_PREFIX_LEN) == 0) {
        /* The template instance required (TIR) symbol indicates that
           this symbol is required when linking this program. This symbol
           is needed because it is not possible to determine whether
	   a symbol defined in a given file is also referenced in that file.
           The TIR information is needed to detect situations where an
           instantiation is no longer required.  The TIR flag tells
           the prelinker that the symbol is referenced and that the
           symbol is one that can be defined by a generated instantiation. */
        is_special_symbol = TRUE;
        sym = pl_find_symbol(&psp->name[PL_INSTANCE_REQUIRED_PREFIX_LEN],
                             psp, /*add=*/TRUE, (a_boolean*)NULL);
        sym->is_template = TRUE;
        sym->referenced = TRUE;
      } else if (strncmp(psp->name, PL_DO_NOT_INSTANTIATE_PREFIX,
                  PL_DO_NOT_INSTANTIATE_PREFIX_LEN) == 0) {
        /* The "do not instantiate" (DNI) symbol is generated as a result
           of a "do_not_instantiate" pragma in a source program.  The
           DNI symbol tells the prelinker that it may not assign the
           symbol to be instantiated by any file.  In other words,
           the user must either provide a specific definition of the symbol
           or must ensure that it is instantiated using an instantiate
           pragma or an instantiation mode such as -tused. */
        is_special_symbol = TRUE;
        sym = pl_find_symbol(&psp->name[PL_DO_NOT_INSTANTIATE_PREFIX_LEN],
                             psp, /*add=*/TRUE, (a_boolean*)NULL);
        sym->do_not_instantiate = TRUE;
      } else if (!input_file->is_archive &&
                 strncmp(psp->name, PL_CAN_BE_INSTANTIATED_PREFIX,
                  PL_CAN_BE_INSTANTIATED_PREFIX_LEN) == 0) {
        /* The "can_be_instantiated" prefix indicates that the name is
           a template and the template can be instantiated in this file.
           Don't consider this to be a possible instantiation site if
           the input file is an archive. */
        is_special_symbol = TRUE;
        sym = pl_find_symbol(&psp->name[PL_CAN_BE_INSTANTIATED_PREFIX_LEN],
                             psp, /*add=*/TRUE, (a_boolean*)NULL);
        sym->is_template = TRUE;
        if (!input_file->is_archive) {
          sym->can_be_instantiated = TRUE;
          /* Add the current input file to the list of files that could
             instantiate the symbol. */
          add_possible_instantiation_site(sym, input_file);
        }  /* if */
      }  /* if */
    }  /* if */
    if (!is_special_symbol) {
      /* The symbol is not a special symbol.  Update the global symbol
         table to reflect the kind of reference or definition. */
      sym = pl_find_symbol(psp->name, psp, /*add=*/TRUE, (a_boolean*)NULL);
      if (psp->referenced) {
        sym->referenced = TRUE;
      }  /* if */
      /* If this is a definition, check for multiply defined symbols.
         Because of the way entry points are remapped, a symbol can
         appear to be defined more than once in a given file.  Ignore
         such redefinitions. */
      if (psp->defined) {
        if (sym->defined && sym->defined_in != input_file) {
          sym->multiple_definition = TRUE;
        } else {
          sym->defined_in = input_file;
          sym->defined = TRUE;
       }  /* if */
      }  /* if */
      /* Update the global symbol to refer to the template from which this
         instance can be instantiated. */
      if (psp->template_sym != NULL) sym->template_sym = psp->template_sym;
      /* Process flags that can be set based on symbols read from
         a template information file. */
      if (psp->do_not_instantiate) sym->do_not_instantiate = TRUE;
      if (psp->is_template) sym->is_template = TRUE;
      if (psp->can_be_instantiated ||
          (sym->template_sym != NULL && sym->template_sym->defined)) {
        sym->can_be_instantiated = TRUE;
        /* Add the current input file to the list of files that could
           instantiate the symbol.  The test of template_sym is used to
           determine that a given instance is associated with an exported
           template.  If a template definition is available, the instance
           can be instantiated. */
        add_possible_instantiation_site(sym, input_file);
      }  /* if */
      sym->tentative_definition |= psp->tentative_definition;
    }  /* if */
    psp = psp->next;
  }  /* while */
  pofp->included_in_output = TRUE;
}  /* pl_add_symbols_from_object */


static a_boolean pl_any_symbols_referenced(a_pl_object_file_ptr pofp)
/*
Determine whether any of the symbols from an object file is needed
to resolve an undefined reference or a tentative definition.
*/
{
  a_pl_symbol_ptr	psp;
  a_boolean		result = FALSE;

  psp = pofp->symbols;
  while (psp != NULL) {
    a_pl_symbol_ptr	sym;
    if (psp->defined || psp->tentative_definition) {
      /* Only look the symbol up if this is a definition. */
      sym = pl_find_symbol(psp->name, psp, /*add=*/TRUE, (a_boolean*)NULL);
      if (!sym->defined) {
        /* A previously undefined symbol may be resolved by a definition or
           a tentative definition.  A tentative definition may only be
           resolved by a nontentative definition. */
        if (sym->referenced ||
            (sym->tentative_definition && psp->defined)) {
          result = TRUE;
          break;
        }  /* if */
        /* An unreferenced symbol is defined in an archive.  Set the flag
           that indicates that a definition has been seen in the archive.
           This is used to suppress an instantiation if a reference is seen
           later.  This can occur when libraries are specified in the wrong
           order on the link line.  What you want to happen is for the
           prelinker to fail to generate an instantiation and for the linker
           to give an undefined symbol error. */
        sym->definition_seen_in_archive = TRUE;
      }  /* if */
    }  /* if */
    psp = psp->next;
  }  /* while */
  return result;
}  /* pl_any_symbols_referenced */


static void pl_prelink(void)
/*
Simulate a link operation.  For each object (.o) file add all of
the names to the symbol table.
*/
{
  a_pl_input_file_ptr	pifp;

  pifp = pl_input_files;
  while (pifp != NULL) {
    a_pl_object_file_ptr	pofp;
    a_boolean			file_used_from_archive = FALSE;
    pofp = pifp->objects;
    if (!pifp->is_archive) {
      if (pofp != NULL) {
        /* This is a .o file.  Add all of its symbols to the symbol table.
           pofp could be NULL if there was no nm output for this file.
           Note that there can be multiple .o files associated with an
           input file when one instantiation per object mode is used. */
        for (; pofp != NULL; pofp = pofp->next) {
          pl_add_symbols_from_object(pofp, pifp);
        }  /* for */
      }  /* if */
    } else {
      while (pofp != NULL) {
        if (!pofp->included_in_output) {
          /* If the object file is not already part of the linked object
             check whether any of its symbols resolve previously
             unresolved references. */
          if (pl_any_symbols_referenced(pofp)) {
            /* This file is needed.  Add the symbols to the global symbol
               table and set the flag that indicates a new file has been
               added from this archive. */
            pl_add_symbols_from_object(pofp, pifp);
            file_used_from_archive = TRUE;
          }  /* if */
        }  /* if */
        pofp = pofp->next;
      }  /* while */
    }  /* if */
    /* If a file was added from this archive then rescan the files in
       the archive to see if any additional files need to be added.
       This would be needed when a file in an archive references an
       file earlier in the archive. */
    if (!file_used_from_archive) pifp = pifp->next;
  }  /* while */
}  /* pl_prelink */


static void pl_corrupted_template_info_file(void)
/*
This routine is called when the information about the compilation command
to be used is invalid.
*/
{
  pl_error(pl_ec_corrupted_template_info_file, (char*)NULL);
}  /* pl_corrupted_template_info_file */


static char *pl_find_suffix(char	*name)
/*
Find the suffix in the file name specified by "name" and return a
pointer to the first character of the suffix.
*/
{
  char	*last_dot;

  last_dot = strrchr(name, '.');
  return last_dot;
}  /* pl_find_suffix */


static char *pl_derived_name(a_const_char *name,
			     a_const_char *suffix)
/*
Construct in pl_file_name_buffer a file name composed of the base name
of "name" with the suffix specified by "suffix".  The suffix should
include the ".".  Note that this routine returns a pointer to a
static buffer, so the contents must be copied before this routine
is used again.
*/
{
  char			*last_dot;

  /* Copy the original name to the file name buffer.  Ensure that there's
     enough space to add the suffix and that the buffer doesn't overflow.
     Use strncat instead of strncpy, as the latter will pad the entirety of
     the buffer with null characters while the former does not, and also adds
     a terminating null character. */
  pl_file_name_buffer[0] = '\0';
  strncat(pl_file_name_buffer, name,
          FILE_NAME_BUFFER_SIZE - MAX_SUFFIX_LEN - 1);
  last_dot = pl_find_suffix(pl_file_name_buffer);
  if (last_dot == NULL) {
    /* No suffix -- set last_dot as if a suffix had followed the name. */
    last_dot = pl_file_name_buffer + strlen(pl_file_name_buffer);
  }  /* if */
  /* Null-terminate at the last dot so that strncat will properly append. */
  *last_dot = '\0';
  strncat(last_dot, suffix, MAX_SUFFIX_LEN);
  return pl_file_name_buffer;
}  /* pl_derived_name */


static void pl_read_template_info_file(a_pl_input_file_ptr pifp)
/*
Read the template information file associated with pifp.

Each line is made up of a line type code followed by the information for
that line type.
*/
{
  sizeof_t		instantiation_dir_length = 0;
  sizeof_t		compilation_dir_length = 0;
  sizeof_t		instantiation_suffix_length;
  sizeof_t		extra_space;
  FILE			*f_template_info = NULL;
  a_boolean		instantiation_dir_set = FALSE;
  a_pl_object_file_ptr	pofp;
  a_pl_symbol_ptr	last_primary_entry = NULL;

  if (pifp->template_info_file_name != NULL) {
    f_template_info = fopen(pifp->template_info_file_name, "r");
  }  /* if */
  if (f_template_info != NULL) {
    /* Allocate an object file entry for this file, but don't attach it
       to the input file yet -- it must be the first entry on the list. */
    pofp = alloc_pl_object_file();
    pofp->file_name = pl_copy_string(pifp->file_name);
    instantiation_suffix_length = strlen(INSTANTIATION_OBJECT_SUFFIX);
    while (pl_read_input_line(f_template_info)) {
      char		*line_type = pl_input_line;
      char		*info = line_type + 4;
      a_pl_symbol_ptr	sym;

      if (strncmp(line_type, "flg:", 4) == 0) {
        /* An instantiation flag lines of the form "flg:name:flags:template".
           Where "flags" may be one or more of "C", "D", and "T", which
           correspond to CBI, DNI, and TIR.  The ":template" portion is
           optional, and specifies the exported template from which this
           instance is to be instantiated. */
        char	*flag_pos;
        flag_pos = strchr(info, ':');
        if (flag_pos == NULL) pl_corrupted_template_info_file();
        /* Replace the ":" with a NULL so that the name is null terminated. */
        *flag_pos++ = '\0';
        sym = alloc_pl_symbol();
        sym->name = pl_copy_string(info);
        for (; *flag_pos != '\0' && *flag_pos != ':'; flag_pos++) {
          switch (*flag_pos) {
            case 'C':
              sym->can_be_instantiated = TRUE;
              sym->is_template = TRUE;
              break;
            case 'D':
              sym->do_not_instantiate = TRUE;
              break;
            case 'T':
              sym->is_template = TRUE;
              sym->referenced = TRUE;
              break;
            default:
             pl_corrupted_template_info_file();
          };
        }  /* for */
        /* Check for the presence of a template name. */
        if (*flag_pos == ':') {
          char	*name_pos = flag_pos + 1;
          sym->template_sym = pl_find_symbol(name_pos, (a_pl_symbol_ptr)NULL,
                                             /*add=*/TRUE, (a_boolean*)NULL);
        }  /* if */
        sym->next = pofp->symbols;
        pofp->symbols = sym;
        /* Save this symbol as the name referred to by any "entry point"
           entries that may follow. */
        last_primary_entry = sym;
      } else if (strncmp(line_type, "ent:", 4) == 0) {
        /* An alternate entry point.  References to this name should be
           treated as references to the primary entry. */
        char	*name_pos = line_type + 4;
        a_pl_symbol_ptr	global_for_last_primary;
        if (last_primary_entry == NULL) pl_corrupted_template_info_file();
        sym = pl_find_symbol(name_pos, (a_pl_symbol_ptr)NULL,
                             /*add=*/TRUE, (a_boolean*)NULL);
        global_for_last_primary = pl_find_symbol(last_primary_entry->name,
                                                 last_primary_entry,
                                                 /*add=*/TRUE,
                                                 (a_boolean*)NULL);
        sym->primary_entry = global_for_last_primary;
      } else if (strncmp(line_type, "tnm:", 4) == 0) {
        /* A template definition entry for an exported template. */
        char	*name_pos = line_type + 4;
        sym = pl_find_symbol(name_pos, (a_pl_symbol_ptr)NULL,
                             /*add=*/TRUE, (a_boolean*)NULL);
        sym->defined = TRUE;
      } else if (strncmp(line_type, "ifn:", 4) == 0) {
        /* An instantiation file name.  Create the full path name by
           adding in the instantiation directory name.  The "extra_space"
           variable accounts for the space needed for the "/", the "/" added
           when add_compilation_dir is TRUE, the and the trailing null
           terminator. */
        a_pl_object_file_ptr	i_pofp;
        a_boolean		add_compilation_dir = FALSE;
        extra_space = 3;
        if (!instantiation_dir_set) {
          /* If the file does not contain an instantiation directory name,
             use "Template.dir" as the default. */
          pifp->instantiation_directory = pl_copy_string("Template.dir");
          instantiation_dir_length = strlen(pifp->instantiation_directory);
          instantiation_dir_set = TRUE;
        }  /* if */
        if (!pifp->is_local_file &&
            !pl_is_absolute_file_name(pifp->instantiation_directory)) {
          /* The instantiation directory is a pathname that is relative to
             the original compilation directory.  This must be added to the
             instantiation file name. */
          add_compilation_dir = TRUE;
        }  /* if */
        i_pofp = alloc_pl_object_file();
        size_t file_name_size = strlen(info) +
                                instantiation_dir_length +
                                instantiation_suffix_length +
                                (add_compilation_dir ? compilation_dir_length :
                                                       0) +
                                extra_space;
        i_pofp->file_name = (char *)pl_malloc_with_check(file_name_size);
        /* Construct the name of the instantiation object file. */
        snprintf(i_pofp->file_name, file_name_size, "%s%s%s/%s%s",
                add_compilation_dir ? pifp->compilation_directory : "",
                add_compilation_dir ? "/" : "",
                pifp->instantiation_directory,
                info, INSTANTIATION_OBJECT_SUFFIX);
        i_pofp->next = pifp->objects;
        i_pofp->is_related_file = TRUE;
        pifp->objects = i_pofp;
      } else if (strncmp(line_type, "dep:", 4) == 0) {
        /* A dependency entry.  Add this to the list of dependencies for this
           input file. */
        a_pl_file_list_entry_ptr	flep;
        flep = alloc_pl_file_list_entry();
        flep->name = pl_copy_string(info);
        flep->next = pifp->dependencies;
        pifp->dependencies = flep;
      } else if (strncmp(line_type, "cmd:", 4) == 0) {
        /* The compilation command line. */
        pifp->command_line = pl_copy_string(info);
      } else if (strncmp(line_type, "dir:", 4) == 0) {
        /* The compilation directory. */
        pifp->compilation_directory = pl_copy_string(info);
        /* See if the .ii file was built in the current directory. */
        pifp->is_local_file = strcmp(pifp->compilation_directory,
                                     curr_dir_name) == 0;
        compilation_dir_length = strlen(pifp->compilation_directory);
      } else if (strncmp(line_type, "fnm:", 4) == 0) {
        /* The file name of the file to be used to recompile the file. */
        pifp->compilation_file_name = pl_copy_string(info);
      } else if (strncmp(line_type, "stu:", 4) == 0) {
        /* A list of secondary translation units to be used when
           recompiling. */
        pifp->secondary_files = pl_copy_string(info);
      } else if (strncmp(line_type, "idn:", 4) == 0) {
        /* The instantiation directory containing the instantiation object
           files. */
        if (instantiation_dir_set) {
          pl_internal_error("instantiation_dir already set");
        }  /* if */
        pifp->instantiation_directory = pl_copy_string(info);
        instantiation_dir_length = strlen(pifp->instantiation_directory);
        instantiation_dir_set = TRUE;
      } else {
        pl_corrupted_template_info_file();
      }  /* if */
    }  /* while */
    fclose(f_template_info);
    /* Add the object file entry for the primary object file to the
       front of the list of objects for this input file. */
    pofp->next = pifp->objects;
    pifp->objects = pofp;
  }  /* if */
}  /* pl_read_template_info_file */


static void pl_read_command_info_from_request_file(
					a_pl_input_file_ptr	pifp,
					FILE*			f_request)
/*
Read the command line, etc. from the instantiation request file.
*/
{
  int	i;
  /* Read the reserved lines and save them to be rewritten later. */
  for (i = 0; i < reserved_request_file_lines; ++i) {
    (void)pl_read_input_line(f_request);
    pifp->reserved_lines[i] = pl_copy_string(pl_input_line);
  }  /* for */
  /* Copy the command line information, etc. from the reserved lines that
     were just read.  If any of the first three lines is reserved, it
     is assumed to contain the expected information. */
  if (reserved_request_file_lines >= 1) {
    pifp->command_line = pl_copy_string(pifp->reserved_lines[0]);
  }  /* if */
  if (!use_template_info_file) {
    /* If not using a template information file, extract the driver-supplied
       information from the request file. */
#if INSTANTIATION_REQUEST_LINES_RESERVED >= 2
    if (reserved_request_file_lines >= 2) {
      pifp->compilation_directory = pl_copy_string(pifp->reserved_lines[1]);
      /* See if the .ii file was built in the current directory. */
      pifp->is_local_file = strcmp(pifp->compilation_directory,
                                   curr_dir_name) == 0;
    }  /* if */
#endif /* INSTANTIATION_REQUEST_LINES_RESERVED >= 2 */
#if INSTANTIATION_REQUEST_LINES_RESERVED >= 3
    if (reserved_request_file_lines >= 3) {
      pifp->compilation_file_name = pl_copy_string(pifp->reserved_lines[2]);
    }  /* if */
#endif /* INSTANTIATION_REQUEST_LINES_RESERVED >= 3 */
  }  /* if */
  /* If the number of effective reserved lines is less than the
     number in the configuration file, set the remaining lines
     to null strings. */
  for (; i < INSTANTIATION_REQUEST_LINES_RESERVED; ++i) {
    pifp->reserved_lines[i] = "";
  }  /* for */
}  /* pl_read_command_info_from_request_file */


static void pl_read_instantiation_request_files(void)
/*
Read the existing instantiation assignment information from the
.ii files associated with the object files being processed.
*/
{
  a_pl_input_file_ptr	pifp;
  FILE			*f_request;

  pifp = pl_input_files;
  while (pifp != NULL) {
    if (!pifp->is_archive && pifp->request_file_name != NULL) {
      f_request = fopen(pifp->request_file_name, "r");
#if DEBUG
      if (pl_debug_level >= 2) {
        fprintf(stderr, "Opening %s, result=%d\n", pifp->request_file_name,
                f_request != NULL);
      }  /* if */
#endif /* DEBUG */
      if (f_request != NULL) {
        pl_read_command_info_from_request_file(pifp, f_request);
        /* Read the instantiation list. */
        while (pl_read_input_line(f_request)) {
          a_pl_symbol_ptr	sym;
          sym = pl_find_symbol(pl_input_line, (a_pl_symbol_ptr)NULL,
			       /*add=*/TRUE, (a_boolean*)NULL);
          if (sym->instantiation_file != NULL) {
            /* The symbol is in the instantiation list of more than one file.
	       This should not happen. */
            fprintf(stderr, pl_error_text(pl_ec_error), message_prefix);
            fprintf(stderr, pl_error_text(pl_ec_multiple_assignments),
                    pl_decoded_name(sym->name), pifp->file_name,
                    sym->instantiation_file->file_name);
	    pl_error(pl_ec_bad_instantiation_request_file, (char *)NULL);
          }  /* if */
          sym->instantiation_file = pifp;
          /* Add this to the front of the list of instantiation entries
             associated with this file. */
          sym->next_in_request_file = pifp->request_list;
          pifp->request_list = sym;
        }  /* while */
        fclose(f_request);
      }  /* if */
    }  /* if */
    pifp = pifp->next;
  }  /* while */
}  /* pl_read_instantiation_request_files */


static void pl_create_instantiation_file_names(
			a_pl_input_file_ptr	pifp,
			char			**request_file_name,
			char			**template_info_file_name)
/*
Create the names of the .ii and .ti file names to be used.  The .ti file
name is only generated if template information files are being used.
*/
{
  char			*suffix;

  *request_file_name = NULL;
  *template_info_file_name = NULL;
  suffix = pl_find_suffix(pifp->file_name);
  if (suffix != NULL && strcmp(suffix, OBJECT_FILE_SUFFIX) == 0) {
    /* Only look for template information files associated with object
       files. */
    *request_file_name = pl_copy_string(pl_derived_name(pifp->file_name,
                                                INSTANTIATION_REQUEST_SUFFIX));
    if (use_template_info_file && (*request_file_name) != NULL) {
      *template_info_file_name = pl_copy_string(
                       pl_derived_name(pifp->file_name, TEMPLATE_INFO_SUFFIX));
    }  /* if */
  }  /* if */
}  /* pl_create_instantiation_file_names */


static a_boolean pl_check_for_template_file(a_pl_input_file_ptr pifp)
/*
Check for the existence of a .ti or .ii file.  The name of the .ii file
is generated.  The name of the .ti file is generated if they are being used.
*/
{
  FILE			*f_test = NULL;
  char			*request_file_name;
  char			*template_info_file_name;
  char			*file_to_test;

  /* Create the names of the .ti and .ii files. */
  pl_create_instantiation_file_names(pifp, &request_file_name,
                                     &template_info_file_name);
  /* A NULL file name will be returned if "pifp" does not represent an
     object file. */
  if (request_file_name != NULL) {
    /* Look for a .ti file if template information files are being used,
       otherwise look for a .ii file. */
    file_to_test = use_template_info_file ? template_info_file_name
                                          : request_file_name;
    f_test = fopen(file_to_test, "r");
    if (f_test != NULL) {
      fclose(f_test);
      pifp->request_file_name = request_file_name;
      pifp->template_info_file_name = template_info_file_name;
    } else {
      free(request_file_name);
      if (use_template_info_file) free(template_info_file_name);
    }  /* if */
  }  /* if */
  return (f_test != NULL);  
}  /* pl_check_for_template_file */


static a_boolean pl_can_instantiate(a_pl_input_file_ptr	pifp,
				 a_pl_symbol_ptr	psp)
/*
Returns TRUE if the input file is a possible instantiation site
of the symbol.
*/
{
  a_pl_instantiation_site_ptr	pisp;
  a_boolean			result = FALSE;

  pisp = psp->possible_instantiation_sites;
  while (pisp != NULL) {
    if (pisp->input_file == pifp) {
      result = TRUE;
      break;
    }  /* if */
    pisp = pisp->next;
  }  /* while */
  return result;
}  /* pl_can_instantiate */


static void record_assignment(char	*name)
/*
Make a record that "name" was assigned to a file.  This is used to
detect instantiation loops caused by some sort of problem with the
data structures provided by the front end, or an inconsistency between
the data structure and the object file information.  An internal error
is issued if a loop is found.
*/
{
  a_pl_assignment_ptr	ap;

  ap = pl_find_assignment(name);
  /* If the file has been assigned more than once, this is probably an
     error.  Permit an extra couple of assignments because, under some
     unusual circumstances (such as a source file changing during the
     prelink process) an instantiation might be assigned more than once. */
  if (ap->times_assigned > 3) {
    pl_internal_error("instantiation loop");
  }  /* if */
  ap->times_assigned++;
}  /* record_assignment */


static a_boolean pl_check_dependencies(a_pl_input_file_ptr	pifp)
/*
Check the dependency list of "pifp" to see if any of the files on which
it depends are newer than the associated object file.  Return TRUE
if the file should be recompiled.
*/
{
  a_boolean			result = FALSE;
  a_pl_object_file_ptr		pofp = pifp->objects;
  a_pl_file_list_entry_ptr	flep;

  /* The objects file pointer can be NULL in certain error cases. */
  if (pofp == NULL) {
    goto done;
  } else if (pofp->modification_time == 0) {
    /* We have not gotten the modification time of this object file yet.
       Get it now. */
    if (!get_file_modification_time(pofp->file_name,
                                    &pofp->modification_time)) {
      /* We could not get the file modification time.  Something unexpected
         might have happened to the file.  Skip the dependency checking. */
      goto done;
    }  /* if */
  }  /* if */
  for (flep = pifp->dependencies; flep != NULL; flep = flep->next) {
    time_t	dep_time;
    if (get_file_modification_time(flep->name, &dep_time)) {
      if (dep_time > pofp->modification_time) {
        /* We found a newer file.  Don't look any further. */
        result = TRUE;
        if (verbose) {
          fprintf(f_informational, pl_error_text(pl_ec_out_of_date),
                  message_prefix, pifp->file_name, flep->name);
        }  /* if */
        break;
      }  /* if */
    }  /* if */
  }  /* for */
done:
  return result;
}  /* pl_check_dependencies */


static a_boolean pl_determine_actions(a_boolean do_local_files)
/*
Once the link has been performed go through each of the input object
files and determine whether any instantiations need to be added to
or removed from the file.  We first go through the existing instantiations
and find any that need to be removed.  We then go through the symbols
referenced in the file and see if any of them need to be instantiated.
If so, they are assigned to the first file found that is capable of
generating an instantiation.  If the instantiation list is modified
the file is flagged as requiring recompilation.
*/
{
  a_pl_input_file_ptr	pifp;
  a_boolean		done = TRUE;

  for (pifp = pl_input_files; pifp != NULL; pifp = pifp->next) {
    /* Only process the specified kind of files (local or nonlocal). */
    if (pifp->is_local_file != do_local_files) continue;
    /* Skip files with no object file information. */
    if (pifp->objects == NULL) continue;
    if (!pifp->is_archive) {
      a_pl_symbol_ptr	psp;
      a_pl_symbol_ptr	prev_psp;
      /* If there is dependency information, see if the object file is
         up-to-date. */
      if (pifp->dependencies != NULL && 
          !suppress_dependency_checking &&
          pl_check_dependencies(pifp)) {
        /* The file is out of date.  Set the request_file_updated flag
           to force the file to be recompiled. */
        pifp->request_file_updated = TRUE;
        pifp->recompile = TRUE;
        done = FALSE;
      }  /* if */
      /* For each of the entries on the instantiations list, see if the
         symbol is defined in the file. */
      psp = pifp->request_list;
      prev_psp = NULL;
      while (psp != NULL) {
        a_boolean		remove_from_request_file = FALSE;
        a_boolean		recompile_file = FALSE;
        a_boolean		remove_one_inst_per_obj_file = FALSE;
        if (psp->multiple_definition || psp->do_not_instantiate) {
          /* An existing instantiation should be removed.  A symbol will
	     be multiply defined when a new specialization has been
             supplied for an instantiation previously assigned to a file.
             The do_not_instantiate flag may now be set (because a pragma
             was added to a file).  Remove the instantiation from
	     the list and recompile the file. */
          remove_from_request_file = TRUE;
          recompile_file = TRUE;
        } else if (psp->defined_in == NULL ||
                   psp->defined_in != pifp) {
          /* Either the symbol is undefined or it is now defined in a
             different file.  Note that a definition in a file generated in
             one instantiation per object mode is considered a definition in
             the primary file for purposes of this test.  If it is not defined
             or is defined in another file it should be removed from the
             instantiation list for this file.  This will be the case
             when a file that was assigned a given instantiation no
             longer requires that particular instantiation. */
          remove_from_request_file = TRUE;
          /* Only remove the one-instantiation-per-object object file
             if it is not defined somewhere else. */
          remove_one_inst_per_obj_file = psp->defined_in == NULL;
          recompile_file = TRUE;
        } else if (!psp->is_template) {
          /* The symbol no longer represents a template.  Remove it from the
             instantiation request file. */
          remove_from_request_file = TRUE;
          recompile_file = TRUE;
        } else if (!psp->referenced) {
          /* The symbol was referenced by another file and now is not.
             Recompile the file because it may not be needed at all. */
#if 0
          /* Should we provide an option that is not quite so pedantic
             about immediately removing unneeded references. */
#endif /* 0 */
          remove_from_request_file = TRUE;
          recompile_file = TRUE;
          remove_one_inst_per_obj_file = TRUE;
        } else {
          /* Mark this symbol has having been instantiated. */
          psp->instantiated = TRUE;
        }  /* if */
        if (remove_from_request_file) {
          /* Either the symbol is undefined or it is now defined in a
             different file.  In either case it should be removed from the
             instantiation list for this file.  This will be the case
             when a file that was assigned a given instantiation no
             longer requires that particular instantiation.  Remove the
             symbol from the request list for this input file. */
          if (prev_psp != NULL) {
            prev_psp->next_in_request_file = psp->next_in_request_file;
          } else {
            pifp->request_list = psp->next_in_request_file;
          }  /* if */
          psp->instantiation_file = NULL;
          pifp->request_file_updated = TRUE;
          pifp->recompile = recompile_file;
          done = FALSE;
#if ONE_INSTANTIATION_PER_OBJECT
          /* This code has to be disabled when one instantiation per object
             is not enabled because the routine to generate the instantiation
             file name is not present. */
          if (one_instantiation_per_object &&
              remove_one_inst_per_obj_file) {
            /* In one instantiation per object mode, remove the file
               associated with the instantiation that is no longer
               needed. */
            snprintf(pl_file_name_buffer, sizeof(pl_file_name_buffer),
                     "%s/%s%s",
                     pifp->instantiation_directory,
                     generate_instantiation_output_file_name(psp->name),
                     INSTANTIATION_OBJECT_SUFFIX);
            unlink(pl_file_name_buffer);
          }  /* if */
#endif /* ONE_INSTANTIATION_PER_OBJECT */
          if (verbose) {
            fprintf(f_informational, pl_error_text(pl_ec_no_longer_needed),
                    message_prefix, pl_decoded_name(psp->name),
                    pifp->file_name);
          }  /* if */
        }  /* if */
        /* Don't update the previous pointer if the current item was
           actually removed from the list. */
        if (!remove_from_request_file) prev_psp = psp;
        psp = psp->next_in_request_file;
      }  /* while */
      /* Do a very simple assignment of instantiations to files.  Just
         go through the list of possible instantiations and instantiate
         anything that hasn't already been handled. */
      psp = pifp->objects->symbols;
      while (psp != NULL) {
        a_pl_symbol_ptr	sym = psp->global_sym;
#if DEBUG
        if (pl_debug_level >= 4) {
          fprintf(stderr, "File: %s, Symbol: %s\n", pifp->file_name,
		  sym == NULL ? "null" : sym->name);
          if (sym != NULL) {
            pl_db_symbol(sym, "        ");
          }  /* if */
        }  /* if */
#endif /* DEBUG */
        if (sym != NULL && sym->is_template &&
             !sym->instantiated && !sym->do_not_instantiate &&
             sym->can_be_instantiated &&
            (sym->referenced || sym->tentative_definition) && !sym->defined &&
            !sym->definition_seen_in_archive &&
            pl_can_instantiate(pifp, sym)) {
          /* Add this symbol to the list of symbols in the request file list.
             Set the instantiation flag and indicate that the request file has
             been updated and the source file associated with the request
             file must be recompiled. */
          sym->next_in_request_file = pifp->request_list;
          pifp->request_list = sym;
          sym->instantiated = TRUE;
          pifp->request_file_updated = TRUE;
          pifp->recompile = TRUE;
          done = FALSE;
          /* Make a record of this assignment for the purpose of detecting
             a loop caused by some sort of data structure problem. */
          record_assignment(sym->name);
          if (verbose) {
            fprintf(f_informational, pl_error_text(pl_ec_assigned_to_file),
                    message_prefix, pl_decoded_name(sym->name),
                    pifp->file_name);
          }  /* if */
        }  /* if */
        psp = psp->next;
      }  /* while */
    }  /* if */
    /* When using a definition list, only update one file per iteration,
       because the recompilation of one file may affect the decisions made
       later about other files. */
    if (pifp->recompile && use_definition_list) break;
  }  /* for */
  return done;
}  /* pl_determine_actions */


static void pl_change_directory(char *new_dir)
/*
*/
{
#if DEBUG
  if (pl_debug_level >= 2) {
    fprintf(stderr, "Changing to directory %s\n", new_dir);
  }  /* if */
#endif /* DEBUG */
  if (chdir(new_dir) != 0) {
    pl_error(pl_ec_cannot_chdir, new_dir);
  }  /* if */
}  /* pl_change_directory */


static void add_to_command_line(char         **dest,
				a_const_char *source)
/*
Copy characters from source to dest.  If we find a quoted string, add
quotes of an alternate kind around that string (e.g., "abc" is changed to
'"abc"' and 'abc' is changed to "'abc'").  Parentheses that are not in
quotes are escaped.
*/
{
  a_const_char	*from;
  char		*to = *dest;
  a_boolean	in_quote = FALSE;
  char		quote_char = 0;
  a_boolean	is_escaped = FALSE;
  char		outer_quote = 0;

  for (from = source; *from != '\0'; ++from) {
    char	ch = *from;
    a_boolean	is_close_quote = FALSE;
    if (is_escaped) {
      is_escaped = FALSE;
    } else if (ch == '\\') {
      is_escaped = TRUE;
    } else if (!in_quote && (ch == '"' || ch == '\'')) {
      in_quote = TRUE;
      quote_char = ch;
      outer_quote = ch == '"' ? '\'' : '"';
      *to++ = outer_quote;
    } else if (in_quote && ch == quote_char) {
      is_close_quote = TRUE;
    } else if ((in_quote && ch == quote_char) ||
               (!in_quote && (ch == '(' || ch == ')'))) {
      *to++ = '\\';
    }  /* if */
    *to++ = ch;
    if (is_close_quote) {
      in_quote = FALSE;
      *to++ = outer_quote;
    }  /* if */
  }  /* for */
  /* Append a blank, if the command does not already end with a blank. */
  if (*(to-1) != ' ') *to++ = ' ';
  *dest = to;
}  /* add_to_command_line */


static char *build_command_line(a_const_char *part1,
                                a_const_char *part2,
                                a_const_char *part3,
				a_const_char *part4)
/*
Construct the command line by concatenating the strings in part1, part2,
part3, and part4.  If any string contains any quotes, insert an escape (\)
before the quote.  Return a pointer to the dynamically allocated string
created to hold the command.
*/
{
  char		*to;
  sizeof_t	length;
  char		*command;

  /* Allocate the command line with twice the space needed to make room
     for added escape characters. */
  if (part2 == NULL) part2 = "";
  if (part3 == NULL) part3 = "";
  if (part4 == NULL) part4 = "";
  if (part1 == NULL) pl_corrupted_template_info_file();
  length = (strlen(part1) + strlen(part2) + strlen(part3) + strlen(part4)) * 2;
  if (length <= 3) pl_corrupted_template_info_file();
  command = (char *)pl_malloc_with_check(length);
  to = command;
  add_to_command_line(&to, part1);
  add_to_command_line(&to, part2);
  add_to_command_line(&to, part3);
  add_to_command_line(&to, part4);
  /* Replace the last blank with a null. */
  *(to-1) = '\0';
  return command;
}  /* build_command_line */


static int pl_recompile_file(a_pl_input_file_ptr pifp,
                             a_const_char	 *extra_command_args,
			     a_const_char	 *extra_args_for_display)
/*
Execute the command to recompile the file specified by pifp.  The
string specified by extra_command_args is added to the command line
found in pifp.  extra_args_for_display is a version of extra_command_args
to be used when displaying the command line.
*/
{
  char		*command;
  char		*display_command;
  int		result;
  a_boolean	chdir_needed;

  /* If the specified directory is different than the current
     one (and is not NULL), then we need to switch directories
     before doing the compilation. */
  chdir_needed = pifp->compilation_directory != NULL &&
                 strcmp(pifp->compilation_directory, curr_dir_name) != 0;
  if (chdir_needed) {
    /* Go to the appropriate directory before doing the compilation. */
    pl_change_directory(pifp->compilation_directory);
  }  /* if */
  command = build_command_line(pifp->command_line, extra_command_args,
                               pifp->compilation_file_name,
                               pifp->secondary_files);
  if (extra_args_for_display != NULL) {
    /* If an alternate version of the extra arguments was supplied for
       display purposes, create an alternate version of the command
       line. */
    display_command = build_command_line(pifp->command_line,
                                         extra_args_for_display,
                                         pifp->compilation_file_name,
                                         pifp->secondary_files);
  } else {
    display_command = command;
  }  /* if */
  fprintf(f_informational, pl_error_text(pl_ec_executing), message_prefix,
          display_command);
  fflush(f_informational);
  result = system(command);
  /* The return value from the system command is usually the return value of
     the command executed shifted left by 8 bits.  If the return value is
     -1, the system command failed for the reason specified by errno. */
  if (result == -1) {
    result = errno;
  } else {
    result = result >> 8;
  }  /* if */
  if (display_command != command) free(display_command);
  free(command);
  if (chdir_needed) {
    /* Return to the original directory. */
    pl_change_directory(curr_dir_name);
  }  /* if */
  return result;
}  /* pl_recompile_file */


static char *last_dir_separator(char *file_name)
/*
Returns the position of the last directory separator character
in file_name.  Returns NULL if file_name contains no directory name.
*/
{
  char	*ptr;

  ptr = strrchr(file_name, '/');
#if BACKSLASH_IS_ALSO_DIR_SEPARATOR
  {
    /* Under MS-DOS, allow both forward and backward slashes.  If the
       name includes both kinds of slashes, use the last one as the
       last separator. */
    char	*ptr2;
    ptr2 = strrchr(file_name, '\\');
    if (ptr2 != NULL && (ptr == NULL || ptr2 > ptr)) ptr = ptr2;
  }
#endif /* BACKSLASH_IS_ALSO_DIR_SEPARATOR */
  return ptr;
}  /* last_dir_separator */


static void prepare_to_move_nonlocal_file(a_pl_input_file_ptr pifp)
/*
An instantiation is being assigned to an object file from another
directory in "copy if nonlocal" mode.  Update the driver information
so that the file can be recompiled from the current directory.  Note
that the .ii file is not actually rewritten here.  We just update
the information so that when the caller writes a new .ii file it
will be in the current directory and will contain the appropriate
information.
*/
{
  char		*orig_file_name;
  char		*orig_request_file_name;
  char		*orig_template_info_file_name;
  char		*ptr;

  /* Construct new file names with the directory name removed. */
  orig_file_name = pifp->file_name;
  ptr = last_dir_separator(pifp->file_name);
  if (ptr == NULL) {
    fprintf(stderr, "Expected %s to include a directory name\n",
            pifp->file_name);
    pl_internal_error("Directory name missing");
  }  /* if */
  if (pifp->secondary_files != NULL) {
    /* "copy if nonlocal" mode cannot be used when secondary translation units
       have been specified (because their file names do not get updated). */
    pl_internal_error(
                 "copy if nonlocal cannot be used with secondary trans units");
  }  /* if */
  pifp->file_name = pl_copy_string(ptr+1);
  orig_request_file_name = pifp->request_file_name;
  orig_template_info_file_name = pifp->template_info_file_name;
  /* Create the new request file and template info file names.  Note that a
     new template information file name must be created even though this
     routine doesn't copy the file because it will be used during the next
     iteration of the prelinker to read the template information file. */
  pl_create_instantiation_file_names(pifp, &pifp->request_file_name,
                                     &pifp->template_info_file_name);
  /* Construct a possibly updated file name.  If the path name is
     absolute, just keep the original name.  Otherwise, add the
     original directory name and write out the updated path name.*/
  if (pl_is_absolute_file_name(pifp->compilation_file_name)) {
    /* Keep the original compilation file name. */
  } else {
    /* Create a path name consisting of the original directory and the
       file name. */
    snprintf(pl_file_name_buffer, sizeof(pl_file_name_buffer), "%s/%s",
             pifp->compilation_directory, pifp->compilation_file_name);
    free(pifp->compilation_file_name);
    pifp->compilation_file_name = pl_copy_string(pl_file_name_buffer);
  }  /* if */
  /* Construct the new compilation directory and compilation file name. */
  free(pifp->compilation_directory);
  pifp->compilation_directory = pl_copy_string(curr_dir_name);
#if DEBUG
  if (pl_debug_level >= 2) {
    fprintf(stderr, "Moving %s to %s\n", orig_request_file_name,
           pifp->request_file_name);
    fprintf(stderr, "Using %s instead of %s\n", pifp->file_name,
            orig_file_name);
    if (use_template_info_file) {
      fprintf(stderr, "Using %s instead of %s\n",
              pifp->template_info_file_name, orig_template_info_file_name);
    }  /* if */
  }  /* if */
#endif /* DEBUG */
  free(orig_file_name);
  free(orig_request_file_name);
  if (orig_template_info_file_name != NULL) free(orig_template_info_file_name);
  if (!use_template_info_file) {
    /* Copy the driver information from the input file structure to the
       reserved line information that will be written to the file. */
    pifp->reserved_lines[0] = pl_copy_string(pifp->command_line);
#if INSTANTIATION_REQUEST_LINES_RESERVED > 1
    pifp->reserved_lines[1] = pl_copy_string(pifp->compilation_directory);
#if INSTANTIATION_REQUEST_LINES_RESERVED > 2
    pifp->reserved_lines[2] = pl_copy_string(pifp->compilation_file_name);
#endif /* INSTANTIATION_REQUEST_LINES_RESERVED > 2 */
#endif /* INSTANTIATION_REQUEST_LINES_RESERVED > 1 */
  }  /* if */
  /* Indicate that this is now a local file. */
  pifp->is_local_file = TRUE;
}  /* prepare_to_move_nonlocal_file */


static FILE *pl_create_temp_file(void)
/*
Create a temporary file for writing.  Return the file pointer associated
with the file.  Sets the global variable temporary_file_name to the
name to be used for the temporary file.
*/
{
  FILE		*f_temp;
  a_const_char	*tmpdir;

  if (temporary_file_name == NULL) {
    /* Create the name of the temporary file to be created. */
    tmpdir = getenv("TMPDIR");
    if (tmpdir == NULL) {
#if __MICROSOFT_OS__
      tmpdir = "/temp";
#else /* __MICROSOFT_OS__ */
      tmpdir = "/tmp";
#endif /* __MICROSOFT_OS__ */
    }  /* if */
    snprintf(pl_file_name_buffer, sizeof(pl_file_name_buffer), "%s/%0dpltf",
             tmpdir, getpid());
    temporary_file_name = pl_copy_string(pl_file_name_buffer);
  }  /* if */
  f_temp = fopen(temporary_file_name, "w");
  if (f_temp == NULL) {
    pl_error(pl_ec_cannot_open_temporary_file, temporary_file_name);
  }  /* if */
  return f_temp;
}  /* pl_create_temp_file */


static char *pl_create_definition_list_file(void)
/*
Create a file containing a list of all of the entities defined in the
object files and libraries named on the command line.  Return a
pointer to an option string to be passed to the front end that
provides the name of the file created.  This will be something
like "--definition_list_file=/tmp/something".
*/
{
  static char		*definition_list_option = NULL;
  FILE			*f_temp;
  a_pl_input_file_ptr	pifp;
  a_pl_object_file_ptr	pofp;
  a_pl_symbol_ptr	psp;

  f_temp = pl_create_temp_file();
  for (pifp = pl_input_files; pifp != NULL; pifp = pifp->next) {
    for (pofp = pifp->objects; pofp != NULL; pofp = pofp->next) {
      for (psp = pofp->symbols; psp != NULL; psp = psp->next) {
        if (psp->defined) {
          fputs(psp->name, f_temp);
          fputs("\n", f_temp);
        }  /* if */
      }  /* for */
    }  /* for */
  }  /* for */
  fclose(f_temp);
  if (definition_list_option == NULL) {
    /* The first time this routine is called, create the option name.
       This assumes that the temporary file name does not change from one
       call to the next. */
    snprintf(pl_file_name_buffer, sizeof(pl_file_name_buffer),
             "--definition_list_file=%s", temporary_file_name);
    definition_list_option = pl_copy_string(pl_file_name_buffer);
  }  /* if */
  return definition_list_option;
}  /* pl_create_definition_list_file */


static void pl_check_for_adopted_instantiations(a_pl_input_file_ptr pifp)
/*
Read a temporary file created by the front end and update the instantiation
request file with the instantiations adopted by the front end.  Append
the entries to the instantiation request file.
*/
{
  FILE	*f_request;
  FILE	*f_temp;

  f_temp = fopen(temporary_file_name, "r");
  if (f_temp != NULL) {
    /* The front end will put an initial line with the string ":add:" in
       it when writing the list of entities that are to be added to the
       request file.  This is used to verify that the file is not a
       definition list file that was left around. */
    if (pl_read_input_line(f_temp) && strcmp(pl_input_line, ":add:") == 0) {
      /* Update the request file in append mode. */
      f_request = fopen(pifp->request_file_name, "a");
      if (f_request == NULL) {
        pl_error(pl_ec_cannot_open_file_for_update, pifp->request_file_name);
      }  /* if */
      while (pl_read_input_line(f_temp)) {
        fputs(pl_input_line, f_request);
        fputs("\n", f_request);
        /* Make a record of this assignment for the purpose of detecting
           a loop caused by some sort of data structure problem. */
        record_assignment(pl_input_line);
        if (verbose) {
          fprintf(f_informational, pl_error_text(pl_ec_adopted_by_file),
                  message_prefix, pl_decoded_name(pl_input_line),
                  pifp->file_name);
        }  /* if */
      }  /* if */
      fclose(f_request);
    }  /* if */
    fclose(f_temp);
  }  /* if */
}  /* pl_check_for_adopted_instantiations */


static int pl_update_request_files(void)
/*
If the list of instantiates for a given instantiation request file
has changed then write the updated list of instantiations to the file.
*/
{

  a_pl_input_file_ptr		pifp;
  int				return_status = 0;
  int				i;

  pifp = pl_input_files;
  while (pifp != NULL) {
    if (pifp->request_file_updated) {
      a_pl_symbol_ptr	psp;
      FILE		*f_request;
      /* Open the input file in read mode to read the header request. */
      if (pifp->request_file_name == NULL) {
        fprintf(stderr,
 "Input file %s has instantiations but no instantiation request file.\n",
                pifp->file_name);
        pl_internal_error("Instantiation request file is missing");
      }  /* if */
      if (move_nonlocal_objects_to_curr_dir && !pifp->is_local_file) {
        /* If the instantiation request file is in a different
           directory, update the information so that it can be written
           to a copy of the file in the current directory. */
        prepare_to_move_nonlocal_file(pifp);
      }  /* if */
      /* Truncate the original file so that it can be rewritten. */
      f_request = fopen(pifp->request_file_name, "w");
      if (f_request == NULL) {
        pl_error(pl_ec_cannot_open_file_for_update, pifp->request_file_name);
      }  /* if */
      /* Rewrite the reserved lines. */
      for (i = 0; i < reserved_request_file_lines; ++i) {
        fprintf(f_request, "%s\n", pifp->reserved_lines[i]);
      }  /* for */
      /* Write the instantiation list to the file. */
      psp = pifp->request_list;
      while (psp != NULL) {
        fprintf(f_request, "%s\n", psp->name);
        psp = psp->next_in_request_file;
      }  /* while */
      fclose(f_request);
      if (!suppress_compilation && pifp->recompile) {
        char         *definition_list_option = NULL;
        a_const_char *def_list_display_option = NULL;
#if PL_REMOVE_OBJECT_FILE_BEFORE_RECOMPILATION
        (void)unlink(pifp->file_name);
#endif /* PL_REMOVE_OBJECT_FILE_BEFORE_RECOMPILATION */
        if (use_definition_list) {
          definition_list_option = pl_create_definition_list_file(); 
          def_list_display_option = "";
#if DEBUG
          /* When generating debug information, include the definition
             list option in the output. */
          if (pl_debug_level != 0) def_list_display_option = NULL;
#endif /* DEBUG */
        }  /* if */
        return_status = pl_recompile_file(
                       pifp, definition_list_option, def_list_display_option);
        if (use_definition_list) {
          if (return_status == 0) {
            /* Read the definition list file to see if the front end
               adopted any instantiations. */
            pl_check_for_adopted_instantiations(pifp);
          }  /* if */
          /* Remove the temporary file that contains the definition list. */
          unlink(temporary_file_name);
        }  /* if */
        /* Stop if an error occurs. */
        if (return_status != 0) break;
      }  /* if */
    }  /* if */
    pifp = pifp->next;
  }  /* while */
  return return_status;
}  /* pl_update_request_files */


static int pl_remove_instantiation_flags(void)
/*
Recompile all of the object files in such a way that the instantiation
flags will be removed.
*/
{

  a_pl_input_file_ptr		pifp;
  int				return_status = 0;
  int				max_return_status = 0;

  pifp = pl_input_files;
  while (pifp != NULL) {
    /* Only process object files (not archives) that have associated
       instantiation request files and that are also local. */
    if (!pifp->is_archive && pifp->request_file_name != NULL &&
         pifp->is_local_file) {
      /* This depends on the command line being in the first reserved
         line. */
#if PL_REMOVE_OBJECT_FILE_BEFORE_RECOMPILATION
      (void)unlink(pifp->file_name);
#endif /* PL_REMOVE_OBJECT_FILE_BEFORE_RECOMPILATION */
      return_status = pl_recompile_file(pifp,
                                        "--suppress_instantiation_flags",
                                        (char *)NULL);
      if (return_status > max_return_status) max_return_status = return_status;
    }  /* if */
    pifp = pifp->next;
  }  /* while */
  return max_return_status;
}  /* pl_remove_instantiation_flags */


static a_boolean pl_check_for_specialization_errors(void)
/*
Go through the list of specializations and determine whether there
have been any references to the unspecialized version.  Return TRUE
if any errors were detected.
*/
{
  a_pl_symbol_ptr	psp;
  a_boolean		any_errors = FALSE;
  a_boolean		is_new;

  for (psp = specialization_list; psp != NULL;
       psp = psp->next_in_specialization_list) {
    a_pl_symbol_ptr	nonspec_psp;
    char		*nonspec_name;
    /* Get the name of the nonspecialized symbol.  Normally, the string
       returned is expected to point to a portion of the original name. */
    nonspec_name = get_nonspecialized_name(psp->name);
    /* Look up the nonspecialized symbol. */
    nonspec_psp = pl_find_symbol(nonspec_name, (a_pl_symbol_ptr)NULL,
                                 /*add=*/TRUE, &is_new);
#if DEBUG
    if (pl_debug_level >= 1) {
      fprintf(stderr, "original name: %s\n", psp->name);
      fprintf(stderr, "nonspecialized name: %s\n", nonspec_name);
    }  /* if */
#endif /* DEBUG */
    if (nonspec_psp != NULL &&
        (nonspec_psp->referenced || nonspec_psp->defined)) {
      pl_error_with_exit(pl_ec_specialized_and_instantiated,
                         pl_decoded_name(nonspec_name),
                         /*exit_when_done=*/FALSE);
      any_errors = TRUE;
    } else {
      /* Update the nonspecialized symbol entry with the referenced and
         defined flags of the specialization.  The nonspecialized entry
         will serve as a summary of the flags of any specialized versions
         that are present. */
      nonspec_psp->referenced |= psp->referenced;
      nonspec_psp->defined |= psp->defined;
    }  /* if */
  }  /* for */
  return any_errors;
}  /* pl_check_for_specialization_errors */


#if DEBUG
static void pl_db_symbol(a_pl_symbol_ptr psp,
                         a_const_char    *prefix_string)
/*
Display a symbol.
*/
{
  a_pl_instantiation_site_ptr	pisp;
  fprintf(stderr, "%sSymbol: %s", prefix_string, psp->name);
  if (psp->defined) fprintf(stderr, " defined");
  if (psp->referenced) fprintf(stderr, " referenced");
  if (psp->tentative_definition) {
    fprintf(stderr, " tentative_definition");
  }  /* if */
  if (psp->multiple_definition) {
    fprintf(stderr, " multiple_definition");
  }  /* if */
  if (psp->is_template) fprintf(stderr, " is_template");
  if (psp->can_be_instantiated) fprintf(stderr, " can_be_instantiated");
  if (psp->do_not_instantiate) fprintf(stderr, " do_not_instantiate");
  pisp = psp->possible_instantiation_sites;
  if (pisp != NULL) {
    fprintf(stderr, " Instantiation sites:");
    for (; pisp != NULL; pisp = pisp->next) {
      fprintf(stderr, " %s", pisp->input_file->file_name);
    }  /* for */
  }  /* if */
  fprintf(stderr, "\n");
}  /* pl_db_symbol */


static void pl_db_input_files(void)
/*
Display the internal representation of the "nm" output.
*/
{
  a_pl_input_file_ptr	pifp;

  pifp = pl_input_files;
  while (pifp != NULL) {
    a_pl_object_file_ptr	pofp;
    a_pl_symbol_ptr		psp;
    fprintf(stderr, "Input file: %s\n", pifp->file_name);
    pofp = pifp->objects;
    while (pofp != NULL) {
      fprintf(stderr, "  Object file: %s\n", pofp->file_name);
      psp = pofp->symbols;
      while (psp != NULL) {
        pl_db_symbol(psp, "    ");
        psp = psp->next;
      }  /* while */
      pofp = pofp->next;
    }  /* while */
    psp = pifp->request_list;
    if (psp != NULL) {
      fprintf(stderr, "  Instantiation list:\n");
    }  /* if */
    while (psp != NULL) {
      pl_db_symbol(psp, "    ");
      psp = psp->next_in_request_file;
    }  /* while */
    pifp = pifp->next;
  }  /* while */
}  /* pl_db_input_files */


static void pl_db_global_symbols(a_boolean	all)
/*
Display all of the symbols in the global symbol table.
*/
{
  a_pl_symbol_ptr	psp;

  fprintf(stderr, "Global symbol table:\n");
  psp = pl_symbol_table_head;
  while (psp != NULL) {
    if (all ||
        (((psp->referenced && !(psp->defined || psp->tentative_definition)) ||
          psp->multiple_definition) && psp->is_template)
#if 0
        /* Enabling this code displays symbols that are unreferenced or
           referenced only from the file in which they are defined. */
                                 ||
        (psp->defined && !psp->referenced && !psp->tentative_definition)
#endif /* 0 */
                                                                        ) {
      pl_db_symbol(psp, "  ");
    }  /* if */
    psp = psp->next_in_symbol_table;
  }  /* while */
}  /* pl_db_global_symbols */
#endif /* DEBUG */


static void pl_free_all(void)
/*
Free all dynamically allocated data.
*/
{
  a_pl_input_file_ptr	pifp;
  a_pl_symbol_ptr	psp;

  pifp = pl_input_files;
  while (pifp != NULL) {
    a_pl_object_file_ptr	pofp;
    a_pl_object_file_ptr	last_pofp;
    pofp = pifp->objects;
    while (pofp != NULL) {
      a_pl_symbol_ptr	last_psp;
      psp = pofp->symbols;
      while (psp != NULL) {
        last_psp = psp;
        psp = psp->next;
        free(last_psp->name);
        free_pl_symbol(last_psp);
      }  /* while */
      last_pofp = pofp;
      pofp = pofp->next;
      free(last_pofp->file_name);
      free_pl_object_file(last_pofp);
    }  /* while */
    /* Free the dependency list. */
    { a_pl_file_list_entry_ptr	flep;
      a_pl_file_list_entry_ptr	next_flep;
      for (flep = pifp->dependencies; flep != NULL; flep = next_flep) {
        next_flep = flep->next;
        free(flep);
      }  /* for */
    }
    /* The input file structure should not be freed. */
    /* The file name, request file name, and template info file name should
       not be freed. */
    if (pifp->command_line != NULL) free(pifp->command_line);
    if (pifp->compilation_directory != NULL) free(pifp->compilation_directory);
    if (pifp->compilation_file_name != NULL) free(pifp->compilation_file_name);
    if (pifp->secondary_files != NULL) free(pifp->secondary_files);
    if (pifp->instantiation_directory != NULL) {
      free(pifp->instantiation_directory);
    }  /* if */
    pifp = pifp->next;
  }  /* while */

  psp = pl_symbol_table_head;
  while (psp != NULL) {
    a_pl_symbol_ptr	        last_psp;
    a_pl_instantiation_site_ptr	pisp;
    pisp = psp->possible_instantiation_sites;
    while (pisp != NULL) {
      a_pl_instantiation_site_ptr	last_pisp;
      last_pisp = pisp;
      pisp = pisp->next;
      free_pl_instantiation_site(last_pisp);
    }  /* while */
    last_psp = psp;
    psp = psp->next_in_symbol_table;
    free(last_psp->name);
    free_pl_symbol(last_psp);
  }  /* while */
}  /* pl_free_all */


static char	*temp_string;
			/* Pointer to a temporary string buffer. */


static sizeof_t	temp_string_length;
			/* The allocated size of the string buffer. */

static sizeof_t	pos_in_temp_string;
			/* The number of characters used in the buffer. */


#define TEMP_STRING_BUFFER_INCREMENTAL_ALLOCATION 4000
			/* Initial and incremental allocation size for
			   string buffer.  The initial allocation
			   should be such that almost all cases can be
			   accepted (so that the realloc is hardly ever
			   needed). */

static void pl_init_temp_string(void)
/*
Initialize the temporary string buffer to begin construction of a new
string.
*/
{
  pos_in_temp_string = 0;
}  /* pl_init_temp_string */


static void pl_add_to_temp_string(a_const_char *addition)
/*
Add the string specified by "addition" to the temporary string buffer.
*/
{
  int	addition_length;

  addition_length = (int)strlen(addition);
  if (pos_in_temp_string + addition_length >= temp_string_length) {
    temp_string_length += TEMP_STRING_BUFFER_INCREMENTAL_ALLOCATION;
    temp_string = (char *)pl_realloc_with_check((a_void_ptr)temp_string,
                                                temp_string_length);
  }  /* if */
  (void)strcpy(temp_string + pos_in_temp_string, addition);
  pos_in_temp_string += addition_length;
}  /* pl_add_to_temp_string */


static void pl_add_two_to_temp_string(a_const_char *add1,
				      a_const_char *add2)
/*
Add two strings to the temporary string buffer.  This is a convenient
interface to add both a blank separator and a new word.
*/
{
  pl_add_to_temp_string(add1);
  pl_add_to_temp_string(add2);
}  /* pl_add_two_to_temp_string */


static char *pl_find_library_name(char *lib_name)
/*
Look for the library name specified by lib_name in the list of
library directories in L_directories.  lib_name is converted to
a library file name (e.g., -lxxx is converted to libxxx.a).
Return a pointer to the name found.  The pointer returned points
into the pl_input_line buffer, so the string must be copied upon
return.
*/
{
  /* Use pl_input_line as a buffer in which to build file names used when
     searching for the library file name. */
  char	        *string_buffer = pl_input_line;
  a_boolean	found = FALSE;
  char          *result = NULL;
  int		j;

  for (j = 0; j < num_of_L_directories; ++j) {
    FILE	*f_lib;
    snprintf(string_buffer, PL_INPUT_LINE_SIZE, "%s/lib%s.a",
             L_directories[j], lib_name);
#if DEBUG
    if (pl_debug_level >= 3) {
      fprintf(stderr, "Looking for %s\n", string_buffer);
    }  /* if */
#endif /* DEBUG */
    if ((f_lib = fopen(string_buffer, "r")) != NULL) {
      /* Replace the original library file name string with a pointer to
	 the complete path name. */
      result = string_buffer;
      found = TRUE;
      fclose(f_lib);
      break;
    }  /* if */
  }  /* for */
  /* If the library was not found then issue an error and exit. */
  if (!found) {
    fprintf(stderr, pl_error_text(pl_ec_lib_file_not_found), lib_name);
    pl_error(pl_ec_command_line_error, (char *)NULL);
  }  /* if */
  return result;
}  /* pl_find_library_name */


static a_pl_cmd_line_arg_ptr
		cmd_line_head = NULL;
			/* Start of a list of command line information. */

static a_pl_cmd_line_arg_ptr
		cmd_line_tail = NULL;
			/* End of a list of command line information. */

static void pl_add_cmd_line_arg(char			*str,
                                a_pl_input_file_ptr	pifp)
/*
Add a command line argument to the list.  Allocate a new entry, initialize
its fields, and add it to the list.  Either str or pifp will be non-NULL.
*/
{
  a_pl_cmd_line_arg_ptr	pclap;
  pclap = (a_pl_cmd_line_arg_ptr)pl_malloc_with_check
                                                   (sizeof(a_pl_cmd_line_arg));
  pclap->is_string = (str != NULL);
  pclap->next = NULL;
  if (pclap->is_string) {
    pclap->variant.arg_string = str;
  } else {
    pclap->variant.input_file_entry = pifp;
  }  /* if */
  if (cmd_line_head == NULL) cmd_line_head = pclap;
  if (cmd_line_tail != NULL) cmd_line_tail->next = pclap;
  cmd_line_tail = pclap;
}  /* pl_add_cmd_line_arg */


int main(int argc, char *argv[])
{
  int		         arg;
  int		         return_status = 0;
  a_boolean	         done = FALSE;
  a_boolean	         any_template_files = FALSE;
  extern char	         *optarg;
  extern int	         optind;
  int		         optchar;
  long		         number_of_iterations = 0;
  char		         *nm_command = NULL;
  a_pl_cmd_line_arg_ptr  last_arg_to_reemit = 0;
  a_boolean		 suppress_instantiation_flags = FALSE;
  a_boolean		 list_object_files = FALSE;

  /* Set the file to be used for informational messages. */
  f_informational = stderr;
  /* This must be done before any messages are issued. */
  message_prefix = pl_error_text(pl_ec_message_prefix);
  /* Clear the assignment table.  Note that this is done once per
     prelinker invocation. */
  memzero((char *)pl_assignment_table, sizeof(pl_assignment_table));
  /* Allocate arrays to hold pointers to -L directory names and library
     names specified by -l options.  We don't know how many of these will
     appear on the command line so we will simply use the argument count
     as the number of elements. */
  L_directories = (char**)pl_malloc_with_check(argc * sizeof(char*));
  /* Get the current directory name. */
  pl_get_curr_dir_name();

  /* Process command-line options. */
  /* Suppress getopt's error on non-recognized option. */
  opterr = 0;
#define OPTION_LIST "a:bc:d:ef:il:mno:qrs:vuB:DL:NOR:SW:"
  while ((optchar = getopt(argc, argv, OPTION_LIST)) != EOF) {
    switch (optchar) {
      case 'a':
        /* Specify whether a definition list file should be created. */
        if (strcmp(optarg, "0") != 0 && strcmp(optarg, "1") != 0) {
          pl_error(pl_ec_invalid_definition_list_option, optarg);
        }  /* if */
        use_definition_list = atoi(optarg) != 0;
        break;
      case 'b':
        /* This is used when the driver --list_object_files option is
           used.  In this mode, the prelinker just outputs a list of
           object files, which may be different than the input list
           of objects if one instantiation per object mode is used. */
        list_object_files = TRUE;
        break;
      case 'c':
        /* Specify the nm command to be used instead of the default
           value. */
        nm_command = optarg;
        break;
      case 'D':
        /* Do not assign instantiations to nonlocal object files. */
        do_not_assign_to_nonlocal_objects = TRUE;
        break;
      case 'e':
        /* Suppress dependency checking of files used to define exported
           templates. */
        suppress_dependency_checking = TRUE;
        break;
      case 'f':
        /* Specifies the nm line format to be expected. */
        if (strcmp(optarg, "solaris") == 0) {
          nm_format = nmfk_solaris;
        } else if (strcmp(optarg, "SGI") == 0) {
          nm_format = nmfk_SGI;
          ignore_invalid_nm_output = TRUE;
          skip_underscore_prefix = FALSE;
        } else if (strcmp(optarg, "SVR4") == 0) {
          nm_format = nmfk_SVR4;
        } else if (strcmp(optarg, "HPUX") == 0) {
          nm_format = nmfk_HPUX;
        } else if (strcmp(optarg, "CLIX") == 0) {
          nm_format = nmfk_CLIX;
        } else if (strcmp(optarg, "gnu") == 0) {
          nm_format = nmfk_gnu;
        } else if (strcmp(optarg, "MacOSX") == 0) {
          nm_format = nmfk_MacOSX;
        } else if (strcmp(optarg, "MacOSX64") == 0) {
          nm_format = nmfk_MacOSX64;
        } else {
          pl_error(pl_ec_invalid_nm_format_option, (char *)NULL);
        }  /* if */
        break;
      case 'i':
        /* Ignore nm output lines that are not formatted properly. */
        ignore_invalid_nm_output = TRUE;
        break;
      case 'B':
        /* A -Bstatic or -Bdynamic that appears in the option list.
           Treated as the start of the file list. */
        /* Fall through into processing below. */
      case 'l':
        /* Library names (e.g., -lstd).  This is interpreted as the start
	   of a list of file names because -l options may be intermixed
	   with other object names. */
        /* Decrement optind so that this option will be processed
           again below. */
        optind--;
        goto end_of_options;
      case 'L':
        /* Library directory names (e.g., -L/edg/cpfe/lib). */
        L_directories[num_of_L_directories++] = optarg;
        break;
      case 'o':
        /* The name of a new object file list that should be written.
           This is used in one instantiation per object mode and
           when copy nonlocal objects that are recompiled. */
        {
          char	*obj_file_list_file_name;
          obj_file_list_file_name = optarg;
          f_obj_file_list = fopen(obj_file_list_file_name, "w");
          if (f_obj_file_list == NULL) {
            pl_error(pl_ec_cannot_open_obj_file_list_file,
                     obj_file_list_file_name);
          }  /* if */
        }
        break;
#if ONE_INSTANTIATION_PER_OBJECT
      case 'O':
        /* "One instantiation per object" mode. */
        one_instantiation_per_object = TRUE;
        break;
#endif /* ONE_INSTANTIATION_PER_OBJECT */
      case 'W':
        /* Alternate form of the library directory name option (e.g.,
           -Wl,-L/edg/cpfe/lib). */
        if (strncmp(optarg, "l,-L", 4) != 0) {
          fprintf(stderr, pl_error_text(pl_ec_unrecognized_option), optarg);
          pl_error(pl_ec_command_line_error, (char*)NULL);
        }  /* if */
        L_directories[num_of_L_directories++] = &optarg[4];
        break;
      case 'm':
        /* Leave identifier names in mangled format for display. */
        mangled_names_in_output = TRUE;
        break;
      case 'n':
        /* Update the instantiation list files but don't recompile the
           files. */
        suppress_compilation = TRUE;
        break;
      case 'N':
        /* If a file from a nonlocal directory needs to be recompiled,
           do the compilation in the current directory.  The argument
           specifies the name of the file into which a list of object
           files is to be written. */
        move_nonlocal_objects_to_curr_dir = TRUE;
        break;
      case 'r':
        /* Don't stop after a certain number of iterations. */
        limit_recursion = FALSE;
        break;
      case 'R':
        /* Override the number of reserved instantiation request
           file lines. */
        reserved_request_file_lines = atoi(optarg);
        if (reserved_request_file_lines < 0 ||
            reserved_request_file_lines >
                                       INSTANTIATION_REQUEST_LINES_RESERVED) {
          pl_error(pl_ec_invalid_reserved_request_lines_option, optarg);
        }  /* if */
        break;
      case 's':
        /* Check (or do not check) for specialization errors.  If the
           argument is zero, suppress the check, otherwise do the check. */
        check_specialization_errors = atoi(optarg) != 0;
        break;
      case 'S':
        /* "Suppress" the instantiation flags in the object files.
           This causes the prelinker to recompile all of the local
           object files with the --suppress_instantiation_flags option. */
        suppress_instantiation_flags = TRUE;
        break;
      case 'u':
        /* Specify whether names have an extra underscore that should
           be ignored.  The option selects the opposite of the default. */
        skip_underscore_prefix = !TARG_EXTERNAL_NAMES_GET_UNDERSCORE_ADDED;
        break;
      case 'q':
        /* "quiet" (nonverbose) mode. */
        verbose = FALSE;
        break;
      case 'v':
        /* Verbose mode. */
        verbose = TRUE;
        break;
      case 'd':
#if DEBUG
        /* Set the debug level */
        pl_debug_level = atoi(optarg);
        break;        
#endif /* DEBUG */
      default:
        if (optind >= argc) optind = argc-1;
        optarg = argv[optind];
        fprintf(stderr, pl_error_text(pl_ec_unrecognized_option), optarg);
        pl_error(pl_ec_command_line_error, (char*)NULL);
        break;
    }  /* switch */
  }  /* while */
end_of_options:
  if ((one_instantiation_per_object || move_nonlocal_objects_to_curr_dir) &&
      f_obj_file_list == NULL) {
    /* One instantiation per object mode and copy nonlocal object mode
       require that a new object file list be specified. */
    pl_error(pl_ec_no_object_file_name_specified, (char*)NULL);
  }  /* if */
  /* Determine the nm command to be used. */
  if (nm_command != NULL) {
    /* A command was specified on the command line. */
  } else if (nm_format == nmfk_solaris) {
    nm_command = solaris_nm_command;
  } else if (nm_format == nmfk_SGI) {
    nm_command = SGI_nm_command;
  } else if (nm_format == nmfk_CLIX) {
    nm_command = CLIX_nm_command;
  } else if (nm_format == nmfk_SVR4 ||
             nm_format == nmfk_HPUX) {
    nm_command = alternate_nm_command;
  } else if (nm_format == nmfk_gnu) {
    nm_command = gnu_nm_command;
  } else {
    /* Use the default command. */
    nm_command = default_nm_command;
  }  /* if */

  {
    /* Create input file records for each of the input files on the
       command line.  Also create a list of command line arguments for
       the "file list" section of the command line.  This list of arguments
       may be used later (if needed) to build an updated file list if any
       of the input files were renamed. */
    a_pl_input_file_ptr	list_tail = NULL;
    for (arg = optind; arg < argc; ++arg) {
      a_pl_input_file_ptr	pifp;
      char                      *orig_name;
      char                      *file_name;
      orig_name = argv[arg];
      if (strcmp(orig_name, "--") == 0) {
        /* The "--" option marks the end of the list of command line arguments
           that should be re-emitted when the "copy nonlocal objects"
	   option is used. */
	last_arg_to_reemit = cmd_line_tail;
	continue;
      }  /* if */
      if (strncmp(orig_name, "-B", 2) == 0) {
        /* A -Bstatic or -Bdynamic option.  Save the option as a string. */
        pl_add_cmd_line_arg(orig_name, (a_pl_input_file_ptr)NULL);
        continue;
      } else if (strncmp(orig_name, "-l", 2) == 0) {
	/* Bypass the "-l" when searching for the library. */
        char  *lib_name = orig_name + 2;
        file_name = pl_find_library_name(lib_name);
      } else {
        file_name = orig_name;
      }  /* if */
      pifp = alloc_pl_input_file();
      pifp->file_name = pl_copy_string(file_name);
      /* Add this entry to the list of input files. */
      if (pl_input_files == NULL) pl_input_files = pifp;
      pl_add_cmd_line_arg((char *)NULL, pifp);
      if (list_tail != NULL) list_tail->next = pifp;
      list_tail = pifp;
      any_template_files |= pl_check_for_template_file(pifp);
    }  /* for */
  }

  if (any_template_files) {
    do {
      a_pl_input_file_ptr	pifp;
      a_boolean			no_local_changes;
      a_boolean			no_nonlocal_changes;
      int			nm_status;

      /* Reset the information in the input files that needs to be cleared
         between prelinker invocations. */
      for (pifp = pl_input_files; pifp != NULL; pifp = pifp->next) {
        reset_pl_input_file(pifp);
      }  /* for */

      pl_symbol_table_head = NULL;
      specialization_list = NULL;
      memzero((char *)pl_symbol_table, sizeof(pl_symbol_table));

      /* Construct the nm command. */
      pl_init_temp_string();
      pl_add_to_temp_string(nm_command);
      /* Append the file names specified on the command line. */
      for (pifp = pl_input_files; pifp != NULL; pifp = pifp->next) {
        a_pl_object_file_ptr	pofp;
        pl_add_two_to_temp_string(" ", pifp->file_name);
        if (use_template_info_file) {
          pl_read_template_info_file(pifp);
        }  /* if */
        if (one_instantiation_per_object) {
          /* Add each of the template object files to the command line. */
          for (pofp = pifp->objects; pofp != NULL; pofp = pofp->next) {
            if (pofp->is_related_file) {
              /* The is_related_file test prevents the primary file from being
                 output more than once. */
              pl_add_two_to_temp_string(" ", pofp->file_name);
            }  /* if */
          }  /* for */
        }  /* if */
      }  /* for */
      pl_add_to_temp_string(nm_command_suffix);
#if DEBUG
      if (pl_debug_level >= 2) fprintf(stderr, "%s\n", temp_string);
#endif /* DEBUG */

      if (!list_object_files) {
        /* Execute the nm command. */
        f_command_output = popen(temp_string, "r");
        if (f_command_output == NULL) {
          pl_error(pl_ec_popen_failed, (char *)NULL);
        }  /* if */
        /* Read the nm output. */
        pl_read_nm_output();
        nm_status = pclose(f_command_output);
        if (nm_status != 0) {
          /* The nm command returned a nonzero status.  Issue a warning. */
          pl_warning(pl_ec_nm_returned_error, (char*)NULL);
        }  /* if */

        /* Read the information from any existing .ii files. */
        pl_read_instantiation_request_files();

#if DEBUG
        if (pl_debug_level >= 4) {
          pl_db_input_files();
        }  /* if */
#endif /* DEBUG */

        /* Add to the symbol table any names that the linker predefines. */
        pl_add_predefined_names();

        pl_prelink();

#if DEBUG
        if (pl_debug_level >= 4) {
          pl_db_global_symbols(/*all=*/FALSE);
      }  /* if */
#endif /* DEBUG */

        /* Determine what actions, if any, are needed.  Make two passes,
           the first of which attempts to do assignments in local files,
           the second in nonlocal files. */
        no_local_changes = pl_determine_actions(/*do_local_files=*/TRUE);
        /* Only attempt to assign compilations to nonlocal files if
           such assignments are permitted.  Suppress nonlocal assignments
           if we've already assigned something to a local file and we are
           using a definition list file. */
        if (do_not_assign_to_nonlocal_objects ||
            (use_definition_list && !no_local_changes)) {
          /* Don't assign to nonlocal files. */
          no_nonlocal_changes = TRUE; 
        } else {
          no_nonlocal_changes = pl_determine_actions(/*do_local_files=*/FALSE);
        }  /* if */
        done = no_local_changes && no_nonlocal_changes;

        /* Write the modified request files back to the disk. */
        return_status = pl_update_request_files();
        if (limit_recursion && ++number_of_iterations == PL_MAX_ITERATIONS) {
          pl_error(pl_ec_instantiation_loop, (char *)NULL);
        }  /* if */
        /* See if there are any functions for which both an instantiation
           and a specialization exist. */
        if (check_specialization_errors) {
          if (pl_check_for_specialization_errors()) {
            /* Cause the prelink process to stop if specialization errors
               were reported. */
            if (return_status == 0) return_status = 1;
          }  /* if */
        }  /* if */
        if (return_status != 0 || suppress_compilation) done = TRUE;
      } else {
        /* List object file mode. */
        done = TRUE;
      }  /* if */
      if (!done) pl_free_all();
    } while (!done);
  }  /* if */
  if (suppress_instantiation_flags) {
    /* Recompile all of the local object files to remove the instantiation
       flags. */
    pl_remove_instantiation_flags();
  }  /* if */
  if (f_obj_file_list != NULL) {
    /* Generate a list of file names and associated command line options.
       This may be different from the list of object files passed to the
       driver if one of the files was moved as a consequence of a
       recompilation.  This is also needed in one instantiation per
       object mode. */
    a_pl_cmd_line_arg_ptr	pclap;
    for (pclap = cmd_line_head; pclap != NULL; pclap = pclap->next) {
      if (pclap->is_string) {
        fprintf(f_obj_file_list, " %s", pclap->variant.arg_string);
      } else {
        a_pl_object_file_ptr	pofp;
        fprintf(f_obj_file_list, " %s",
                pclap->variant.input_file_entry->file_name);
        /* Generate a list of the associated files created in one instantiation
           per object mode. */
        for (pofp = pclap->variant.input_file_entry->objects;
             pofp != NULL; pofp = pofp->next) {
          if (!pofp->is_related_file) continue;
          fprintf(f_obj_file_list, " %s", pofp->file_name);
        }  /* for */
      }  /* if */
      if (pclap == last_arg_to_reemit) break;
    }  /* for */
    fprintf(f_obj_file_list, "\n");
    fclose(f_obj_file_list);
  }  /* if */

#ifdef USING_PURIFY
  /* When using purify, free memory that would otherwise be reported as
     leaked. */
  pl_free_all();
  if (L_directories != NULL) free(L_directories);
#endif /* USING_PURIFY */

  return (return_status);
}  /* main */


