/*
Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
Exceptions.
See https://edgcpp.org/LICENSE.txt for license information.
SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
*/

/*

Driver program.

*/

#include <stdlib.h>
#include <signal.h>
#include "basics.h"

#if __WIN32__
#include "host_envir.h"
#else /* !__WIN32__ */
#include "host_envir.h"
#endif /* !__WIN32__ */

#define CPFE_COMMAND			"cpfe"
#if __WIN32__
#define C_COMMAND			"cl -nologo"
#define GEN_C_OBJECT_FILE_SUFFIX	".int.obj"
#define EXECUTABLE_FILE_SUFFIX		".exe"
#define DEFAULT_OUTPUT_FILE_NAME	"aout"
#ifndef DEFAULT_EDG_BASE
#define DEFAULT_EDG_BASE		"\\edg"
#endif /* ifndef DEFAULT_EDG_BASE */
#define PATH_DELIMITER			"\\"
#define LIBC_NAME			"libedg.lib"
#define DEFAULT_DEFINES			""
#define LINKER_OPTIONS			"/Zi -link /debug"
#define EDG_MUNCH			"edg_munch"
#define MUNCH_C_FILE			"munchtmp.c"
#define MUNCH_OBJ_FILE			"munchtmp.obj"
#define EDG_PRELINK			"edg_prelink"
#define DEFAULT_MICROSOFT_INCLUDE	"\\progra~1\\micros~1\\vc98\\include"
#define DEFAULT_MSVC_TARGET_VERSION	1300
#else /* !__WIN32__ */
#define C_COMMAND			"cc"
#define GEN_C_OBJECT_FILE_SUFFIX	".int.o"
#define EXECUTABLE_FILE_SUFFIX		""
#define DEFAULT_OUTPUT_FILE_NAME	"a.out"
#ifndef DEFAULT_EDG_BASE
#define DEFAULT_EDG_BASE		"/edg/cpfe"
#endif /* ifndef DEFAULT_EDG_BASE */
#define PATH_DELIMITER			"/"
#define LIBC_NAME			"libC.a"
#define DEFAULT_DEFINES			""
#define LINKER_OPTIONS			""
#define EDG_MUNCH			"edg_munch"
#define MUNCH_C_FILE			"munchtmp.c"
#define MUNCH_OBJ_FILE			"munchtmp.o"
#define EDG_PRELINK			"edg_prelink"
#endif /* __WIN32__ */

typedef struct a_cl_argument *a_cl_argument_ptr;
typedef struct a_cl_argument {
  a_cl_argument_ptr
		next;
			/* Pointer to the next argument in the list. */
  a_const_char	*str;
			/* Pointer to the argument text. */
} a_cl_argument;


typedef struct a_command_line *a_command_line_ptr;
typedef struct a_command_line {
  a_cl_argument_ptr
		args;
			/* Pointer to the argument list.  The first argument
		 	   is the command name. */
  a_cl_argument_ptr
		tail;
			/* Pointer to the last argument. */
  int		length;
			/* Length of the command line.  It is assumed that one
			   blank will be placed between the arguments. */
  int 		elements;
			/* Number of entries on the list of arguments. */
} a_command_line;


a_command_line	file_list;
			/* List of input files to be processed. */

a_command_line	link_command;
			/* The command line that will be used to link
			   the executable (if needed). */

a_command_line	object_file_list;
			/* The list of object files to be passed to the
			   linker. */

a_command_line	compile_options;
			/* The options from the edgcc command line that need
			   to be passed to cpfe for each compilation. */

a_command_line	link_options;
			/* The options to be passed to the linker. */

a_command_line	c_to_obj_options;
			/* The options to be passed to the C to obj
                           compiler. */

a_command_line	instantiation_command_line;
			/* The options to be recorded in the instantiation
			   information file. */

static a_boolean
		use_munch = FALSE;
			/* TRUE if edg_munch should be used for static
			   initialization. */

#if __WIN32__

static long	msvc_target_version;
			/* The version of the Microsoft compiler being
			   used to compile the generated code. */

#endif /* __WIN32__ */

/*
Buffer into which commands are built.
*/
#define STRING_BUFFER_SIZE 32767
static char string_buffer[STRING_BUFFER_SIZE];


static int cflag = FALSE;
			/* -c option specified. */
static int gflag = FALSE;
			/* -g option specified. */
static int fe_only = FALSE;
			/* Run only the front end. */
static int multi_trans_unit_mode = FALSE;
			/* Use multiple translation unit mode. */
static int preprocess_only = FALSE;
			/* Running in preprocessing mode. */
static a_const_char
		*edg_base;
			/* Where the EDG bin, lib, and include directories
                           reside. */
static a_const_char
		*cpfe_command;
			/* Name of the front end executable. */
static char	*edg_lib;
			/* $EDG_BASE/lib. */
static char	*edg_include;
			/* $EDG_BASE/include. */
static char	*edg_munch;
			/* EDG_BASE/lib/edg_munch. */
#if __WIN32__
static char	*munch_nm;
			/* EDG_BASE/lib/munch_nm. */
static char	*microsoft_include;
			/* EDG_MS_INCLUDE */
#endif /* __WIN32__ */

static char	*edg_prelink;
			/* EDG_BASE/lib/edg_prelink. */
static char	*prelink_options = NULL;
			/* Prelinker options. */
static char	*prelink_default_options = NULL;
			/* Prelinker options from EDG_PRELINK_DEFAULT_OPTIONS
                           environment variable. */
static char	*edg_libc;
			/* $EDG_BASE/lib/LIBC_NAME. */
static char *output_file_name = NULL;
			/* Name of the generated executable. */
static int return_status = 0;
			/* Return status of this command. */
static a_boolean debug = FALSE;
			/* Should debugging information be displayed. */

#define CURR_DIR_NAME_SIZE 2048
			/* Maximum size of the current directory name. */

static char	curr_dir_name[CURR_DIR_NAME_SIZE];
			/* Name of the current working directory. */

#if 0
static char		temp_file[L_tmpnam];
			/* Temporary file for miscellaneous use. */
#endif /* 0 */


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


static void internal_error(a_const_char* error_string)
/*
Prints an internal error message and exits with a catastrophic error
exit status.
*/
{
  fprintf(stderr, "%s: internal error: %s\n", "edgcc", error_string);
#if EXIT_ON_INTERNAL_ERROR
  exit(RC_CATASTROPHE);
#else /* !EXIT_ON_INTERNAL_ERROR */
  (void)fflush(stderr);
  abort();
#endif /* EXIT_ON_INTERNAL_ERROR */
}  /* internal_error */


static void get_curr_dir_name(void)
/*
Get the current directory name and save it in curr_dir_name.
*/
{
#if USE_GETCWD
  if (getcwd(curr_dir_name, CURR_DIR_NAME_SIZE) == NULL) {
    internal_error("getcwd failed");
  }  /* if */
#else /* !USE_GETCWD */
  (void)getwd(curr_dir_name);
#endif /* USE_GETCWD */
}  /* get_curr_dir_name */


/* Forward declaration of cleanup routine. */
static void wrapup();

void error_exit(void)
/*
Exit with an error status.
*/
{
  wrapup();
  exit(1);
}


static void *alloc_general(sizeof_t size)
{
  void*		ptr;

  ptr = malloc(size);
  if (ptr == NULL) {
    fprintf(stderr, "edgcc: out of memory.\n");
    error_exit();
  }  /* if */
  return ptr;
}  /* alloc_general */


static char *copy_of_string(char *source)
/*
Allocate space for a copy of the string and make a copy.  Return a pointer
to the copy.
*/
{
  char	*dest;
  dest = (char *)alloc_general(strlen(source) + 1);
  strcpy(dest, source);
  return dest;
}  /* copy_of_string */


void init_command_line(a_command_line_ptr clp)
/*
Initialize the fields of a command line.
*/
{
  clp->args = NULL;
  clp->tail = NULL;
  clp->length = 0;
  clp->elements = 0;
}  /* init_command_line */


static a_cl_argument_ptr
		 avail_cl_arguments = NULL;
			/* List of freed cl_argument records. */


static void add_cl_argument(a_command_line_ptr	clp,
			    a_const_char	*new_arg)
/*
Add an argument to a command line.  The argument pointer points to the
actual string passed by the caller, not a copy.
*/
{
  a_cl_argument_ptr	clap;

  if (avail_cl_arguments != NULL) {
    clap = avail_cl_arguments;
    avail_cl_arguments = clap->next;
  } else {
    clap = (a_cl_argument_ptr)alloc_general(sizeof(a_cl_argument));
  }  /* if */
  clap->next = NULL;
  clap->str = new_arg;
  /* Add the argument to the end of the list. */
  if (clp->args == NULL) clp->args = clap;
  if (clp->tail != NULL) clp->tail->next = clap;
  clp->tail = clap;
  /* Increment the size.  Include a space for a blank after the argument. */
  clp->length += sizeof(new_arg) + 1;
  clp->elements++;
}  /* add_cl_argument */


static void free_cl_argument(a_cl_argument_ptr clap)
/*
Put a cl_argument back on the available list.
*/
{
  clap->next = avail_cl_arguments;
  avail_cl_arguments = clap;
}  /* free_cl_argument */


static void append_command_line(a_command_line_ptr clp,
				a_command_line_ptr add_clp)
/*
Append one command line to an existing command line.
*/
{
  a_cl_argument_ptr	clap;
  clap = add_clp->args;
  while (clap != NULL) {
    add_cl_argument(clp, clap->str);
    clap = clap->next;
  }  /* while */
}  /* append_command_line */


/*
Define some maximum lengths for MS-DOS file name handling stuff.  Note that the
defined lengths INCLUDE the terminating null character.  "__MAXDRIVE__" is the
length of the disk drive letter which includes the trailing ":".  "__MAXDIR__"
is the length of the directory part of the path including a trailing slash.
"__MAXFILE__" is the length of the base file name.  "__MAXEXT__" is the length
of the extension including the leading ".".
*/
#if __WIN32__
#if __TURBOC__
#define __MAXDRIVE__ MAXDRIVE
#define __MAXDIR__   MAXDIR
#define __MAXFILE__  MAXFILE
#define __MAXEXT__   MAXEXT

#else /* __TURBOC__ */
#if __MSC__
#define __MAXDRIVE__ _MAX_DRIVE
#define __MAXDIR__   _MAX_DIR
#define __MAXFILE__  _MAX_FNAME
#define __MAXEXT__   _MAX_EXT
#else /* __MSC__ */
#if __ZTC__
/* Zortech doesn't currently (version 3.0) define these values. */
#define __MAXDRIVE__ 3
#define __MAXDIR__   66
#define __MAXFILE__  9
#define __MAXEXT__   5
#else /* __ZTC */
error -- unknown MS-DOS compiler.
#endif /* __ZTC */
#endif /* __MSC__ */
#endif /* __TURBOC__ */
#endif /* __WIN32__ */


/*
Define a macro which takes a complete file path and breaks it up into the parts
as described above.
*/
#if __WIN32__
#if __TURBOC__
#define split_path(path, drive, dir, file, ext) \
	  (void)fnsplit(path, drive, dir, file, ext)
#define merge_path(path, drive, dir, file, ext) \
	  fnmerge(path, drive, dir, file, ext)

#else /* __TURBOC__ */
#if __MSC__
#define split_path(path, drive, dir, file, ext) \
	  _splitpath(path, drive, dir, file, ext)
/* MSC does not have a function to assemble a path from its components, so use
   "strcpy" and "strcat" to reassemble. */
#define merge_path(path, drive, dir, file, ext) \
	  (void)strcpy(path, drive);		\
	  strcat(path, dir);			\
	  strcat(path, file);			\
	  strcat(path, ext);
#else /* __MSC__ */
#if __ZTC__
static void split_path(char *path,
                      char *drive,
                      char *dir,
                      char *file,
                      char *ext)
/*
Zortech doesn't provide a version of split_path so we use our own version.
This routine is similar to the Borland "fnsplit" and the Microsoft
"_splitpath" except that this routine does not accept NULL values for
path name components and doesn't return a value as the Borland version
does.
*/
{
  char		*colon_pos;
  char		*last_slash_pos;
  char		*start_pos = path;
  char		*dot_pos;
  sizeof_t	length;

  /* Initialize the components of the string in case they are not
     explicitly specified. */
  strcpy(drive, "");
  strcpy(dir, "");
  strcpy(file, "");
  strcpy(ext, "");
  /* Get the drive.  Copy the drive to the destination -- the colon is
     copied too. */
  colon_pos = strchr(path, ':');
  if (colon_pos != NULL) {
    length = colon_pos - path + 1;
    length = (length < __MAXDRIVE__) ? length : __MAXDRIVE__ - 1;
    strncat(drive, start_pos, length);
    /* If the drive name was truncated add the terminating colon. */
    if (length == (__MAXDRIVE__ - 1)) {
      drive[length-1] = ':';
    }  /* if */
    start_pos = colon_pos + 1;
  }  /* if */
  /* See if there is a directory name.  If so, copy it to the destination
     including leading and trailing slashes. */
  last_slash_pos = strrchr(start_pos, '\\');
  if (last_slash_pos != NULL) {
    length = last_slash_pos - start_pos + 1;
    length = (length < __MAXDIR__) ? length : __MAXDIR__ - 1;
    strncat(dir, start_pos, length);
    start_pos = last_slash_pos + 1;
  }  /* if */
  /* Find the '.' that ends the filename if one exists.  Copy the file name
     and extension.  The '.' is part of the extension. */
  dot_pos = strchr(start_pos, '.');
  if (dot_pos != NULL) {
    /* An extension exists. */
    length = dot_pos - start_pos;
    length = (length < __MAXFILE__) ? length : __MAXFILE__ - 1;
    strncat(file, start_pos, length);
    length = __MAXEXT__ - 1;
    strncat(ext, dot_pos, length);
  } else {
    /* No extension exists. */
    length = __MAXFILE__ - 1;
    strncat(file, start_pos, length);
  }  /* if */
}  /* split_path */


/* Zortech does not have a function to assemble a path from its components,
   so use  "strcpy" and "strcat" to reassemble. */
#define merge_path(path, drive, dir, file, ext) \
	  (void)strcpy(path, drive);		\
	  strcat(path, dir);			\
	  strcat(path, file);			\
	  strcat(path, ext);
#else /* __ZTC */
error -- unknown MS-DOS compiler.
#endif /* __ZTC */
#endif /* __MSC__ */
#endif /* __TURBOC__ */
#endif /* __WIN32__ */


#if __WIN32__
static void truncate_msdos_filename(char *filename)
/*
Truncate the base and extension parts of an MSDOS filename so that it fits into
the maximum specified by "__MAXFILE__" and "__MAXEXT__".  Note that these
lengths include the null terminator.  This routine can truncate the front of
the base if necessary.
*/
{
  char drive[__MAXDRIVE__];
  char dir[__MAXDIR__];
  char file[128];
  char ext[128];

  /* Split the name into its parts. */
  split_path(filename, drive, dir, file, ext);
  /* See if the file name ends with the suffix used for C output. */
  { char *C_file_suffix = GEN_C_FILE_SUFFIX;
    if (strlen(filename) > strlen(C_file_suffix) &&
        strcmp(filename + strlen(filename) - strlen(C_file_suffix),
               C_file_suffix) == 0) {
      sizeof_t IL_pre_suffix_len = strchr(C_file_suffix, '.') - C_file_suffix;
      /* See if the suffix was truncated. */
      if (memcmp(file+strlen(file)-IL_pre_suffix_len, C_file_suffix,
                 size_t_arg(IL_pre_suffix_len)) != 0) {
        /* Yes, it was truncated.  Re-truncate so as to preserve the "_int"
           part of the base name.  For example, "abcdef_int" is truncated to
           "abcd_int". */
        (void)memcpy(file+__MAXFILE__-1-IL_pre_suffix_len,
                     C_file_suffix, size_t_arg(IL_pre_suffix_len));
        file[__MAXFILE__-1] = '\0';
      }  /* if */
    }  /* if */
  }
  /* Truncate the filename part at the end (this may be unnecessary). */
  file[__MAXFILE__-1] = '\0';
  /* Truncate the extension part at the end (this may be unnecessary). */
  ext[__MAXEXT__-1] = '\0';
  /* Put the file name back together.  The resulting name is never longer than
     the original. */
  merge_path(filename, drive, dir, file, ext);
}  /* truncate_msdos_filename */


#ifdef NEED_SIZE_T_ARG_ERROR

/* The extern declaration for this routine is in basics.h. */
true_size_t size_t_arg_error(void)
/*
Called from the macro size_t_arg to abort the front end if a size is being
truncated.  This is used on machines that have a small size_t (say, 16 bits)
when we choose to make sizeof_t something longer.
*/
{
  fprintf(stderr, "edgcc: size_t_arg_error called.\n");
  error_exit();
}  /* size_t_arg_error */
#endif /* ifdef NEED_SIZE_T_ARG_ERROR */

#endif /* __WIN32__ */


static char *end_of_directory_name(a_const_char *file_name)
/*
Return a pointer to the end of the directory part of the indicated file
name, or NULL if there is no directory part.
Note that the following must work for FILE_NAME_FOR_STDIN, which is
used to represent stdin; it must return  NULL.
*/
{
  char *last_slash;
#if __WIN32__
  char *last_backslash;
#endif /* __WIN32__ */

  if (strcmp(file_name, FILE_NAME_FOR_STDIN) == 0) {
    /* Special pseudo-name used for stdin; no directory. */
    last_slash = NULL;
  } else {
#if __VMS__
    /* VMS -- Check for "[]" for directory, or ":" for disk or node name. */
    last_slash = strrchr(file_name, ']');
    if (last_slash == NULL) last_slash = strrchr(file_name, ':');
#else /* !__VMS__ */
    /* UNIX-like system -- check for last slash. */
    last_slash = strrchr(file_name, '/');
#if __WIN32__
    /* MSDOS -- Allow backslash as an alternative to "/", and check for ":"
       of disk name. */
    last_backslash = strrchr(file_name, '\\');
    if (last_slash == NULL || last_backslash > last_slash) {
      last_slash = last_backslash;
    }  /* if */
    if (last_slash == NULL && strlen(file_name) >= 2 && file_name[1] == ':') {
      /* Disk name is specified, as in "c:abc". */
      last_slash = file_name+1;
    }  /* if */
#endif /* __WIN32__ */
#endif /* __VMS__ */
  }  /* if */
  return(last_slash);
}  /* end_of_directory_name */


char *derived_name(a_const_char *file_name,
                   a_const_char *suffix)
/*
Return a string that is the base name of file_name with the given suffix
appended.  The string is allocated in general storage, NOT in an
intermediate language memory region, so it must be copied if it is to
be passed to the back end.
*/
{
  a_const_char *last_slash, *last_dot, *name_start, *name_end;
  sizeof_t     der_name_length, suffix_length, base_name_length;
  char         *der_name;

  /* Find the base name by removing the directory and suffix. */
  last_slash = end_of_directory_name(file_name);
  if (last_slash == NULL) {
    /* No directory name, start of file name is start of base name. */
    name_start = file_name;
  } else {
    /* Start of base name is after the directory name. */
    name_start = last_slash + 1;
  }  /* if */
  /* Find suffix, if any. */
  if ((last_dot = strrchr(name_start, '.')) == NULL) {
    /* No suffix, end of base name is the same as end of file name. */
    name_end = name_start + strlen(name_start) - 1;
  } else {
    /* End of base name is before the suffix. */
    name_end = last_dot - 1;
  }  /* if */
  /* Copy the base name and suffix into the derived name. */
  suffix_length = strlen(suffix);
  base_name_length = name_end - name_start + 1;
  der_name_length = base_name_length + suffix_length;
  der_name = (char *)alloc_general(der_name_length+1);
  (void)memcpy(der_name, name_start, size_t_arg(base_name_length));
  (void)memcpy(&der_name[base_name_length], suffix, size_t_arg(suffix_length));
  der_name[der_name_length] = '\0';
#if __WIN32__
  /* Check for and truncate file names that are too long for MSDOS 
     to handle. */
  truncate_msdos_filename(der_name);
#endif /* __WIN32__ */
  return(der_name);
}  /* derived_name */

#if 0
static a_boolean file_exists(char *file_name)
/*
Returns TRUE if a file exists; otherwise FALSE.
*/
{
  a_boolean	result = FALSE;
  FILE*		file;

  file = fopen(file_name, "r");
  if (file != NULL) {
    result = TRUE;
    fclose(file);
  }  /* if */
  return result;
}  /* file_exists */
#endif /* 0 */


static int rename_file(char *from,
		       char *to)
/*
Rename a file from "from" to "to".
*/
{
  int status;
  if (debug) {
    fprintf(stderr, "Renaming %s to %s\n", from, to);
  }  /* if */
#if __WIN32__
  remove(to);
  status = rename(from, to);
#else /* !__WIN32__ */
  unlink(to);
  status = link(from, to);
  unlink(from);
#endif /* __WIN32__ */
  if (status != 0) {
    fprintf(stderr, "edgcc: could not rename %s to %s.\n", from, to);
  }  /* if */
  return status;
}  /* rename_file */


static void remove_file(a_const_char *file)
/*
Remove a file.
*/
{
#if __WIN32__
  remove(file);
#else /* !__WIN32__ */
  unlink(file);
#endif /* __WIN32__ */
  if (debug) {
    fprintf(stderr, "%s removed\n", file);
  }  /* if */
}  /* remove_file */


a_boolean read_input_line(FILE* input_file)
/*
Reads a line of input from input_file.  Returns TRUE if a line of
input is being returned.  Returns FALSE at end-of-file.  Sets "line_size"
to the number of characters read not including the trailing null character.
*/
{
  register char*    buffer_pos = &string_buffer[0];
  register int      size = 0;
  register int      ch;
  a_boolean         result;

  while ((ch = getc(input_file)), ch != EOF && ch != '\n') {
    if (++size > STRING_BUFFER_SIZE) {
      fprintf(stderr, "edgcc: read_input_line: input line too long.");
      error_exit();
    }  /* if */
    *buffer_pos++ = ch;
  }  /* while */
  
  /* Terminate string with a null character. */
  *buffer_pos++ = '\0';

  /* Determine whether to return end-of-file (FALSE). */
  result = TRUE;
  if (ch == EOF && size == 0) result = FALSE;

  return (result);
}  /* read_input_line */


static a_const_char *get_suffix(a_const_char *file_name)
/*
Return a pointer to the suffix part of the file name.
*/
{
  a_const_char *ptr;
#if __WIN32__
  a_const_char *ptr2;
#endif /* __WIN32__ */
  a_const_char *suffix;
  /* Find the file name part of the path name.  This is the part after
     any directory name.  In MS-DOS it is also the part after a drive name. */
  ptr = strrchr(file_name, '/');
  if (ptr == NULL) ptr = file_name;
#if __WIN32__
  ptr2 = strrchr(file_name, '\\');
  if (ptr2 > ptr) ptr = ptr2;
  ptr2 = strrchr(file_name, ':');
  if (ptr2 > ptr) ptr = ptr2;
#endif /* __WIN32__ */
  suffix = strrchr(ptr, '.');
  /* Return the position of the character after the ".". */
  if (suffix != NULL) suffix++;
  return suffix;
}  /* get_suffix */


int execute_command(a_command_line_ptr clp)
/*
Execute the command in "string_buffer" and return the resulting status.
*/
{
  int			status;
  a_cl_argument_ptr	clap;
  a_cl_argument_ptr	prev_clap;
  char			*to;
  if (clp->length >= STRING_BUFFER_SIZE) {
    fprintf(stderr, "edgcc: command line too long.\n");
    error_exit();
  }  /* if */
  to = string_buffer;
  clap = clp->args;
  while (clap != NULL) {
    a_const_char *from = clap->str;
    while (*from) *to++ = *from++;
    *to++ = ' ';
    prev_clap = clap;
    clap = clap->next;
    free_cl_argument(prev_clap);
  }  /* while */
  *to = '\0';
  if (debug) {
    fprintf(stderr, "Executing %s\n", string_buffer);
  }  /* if */
  status = system(string_buffer);
  return status;
}  /* execute_command */


static void wrapup(void)
/*
Termination cleanup routine.
*/
{
#if 0
  remove_file(temp_file);
#endif /* 0 */
}  /* wrapup */


/*ARGSUSED*/ /* <-- Because "sig" is not used. */
static void term_on_signal(int sig)
/*
Routine set up as a signal handler, called to terminate compilation on
receipt of a signal.
*/
{
  wrapup();
}  /* term_on_signal */


#if __WIN32__

static long scan_opt_arg_number(char *optstr)
/*
Scan an argument option as a decimal number, and return its value.
*/
{
  char *arg_ptr;
  long result = 0;
  int  digit;

  for (arg_ptr = optstr; *arg_ptr != '\0'; arg_ptr++) {
    if (!isdigit((unsigned char)*arg_ptr)) goto number_error;
    digit = *arg_ptr - '0';
    if (result > LONG_MAX / 10) goto number_error;
    result *= 10;
    if (result > LONG_MAX-digit) goto number_error;
    result += digit;
  }  /* for */
  goto return_point;
number_error:
  fprintf(stderr, "edgcc: invalid numeric argument: %s\n", optstr);
  error_exit();
return_point:
  return result;
}  /* scan_opt_arg_number */

#endif /* __WIN32__ */

static void init(char *command_name)
/*
Startup initialization.
*/
{
  char	*str;

#if 0
  /* Get the name of a temporary file for use by the driver. */
  (void)tmpnam(temp_file);
#endif /* 0 */
  /* Get the current directory name. */
  get_curr_dir_name();
  /* Get the name of the EDG base directory which contains the
     bin and lib directories. */
  edg_base = getenv("EDG_BASE");
  if (edg_base == NULL) edg_base = DEFAULT_EDG_BASE;
  /* Get the name of the front end executable. */
  cpfe_command = getenv("EDG_CPFE");
  if (cpfe_command == NULL) cpfe_command = CPFE_COMMAND;
  /* Build $EDG_BASE/lib. */
  sprintf(string_buffer, "%s%slib", edg_base, PATH_DELIMITER);
  edg_lib = copy_of_string(string_buffer);
  /* Build $EDG_BASE/lib/edg_munch. */
  sprintf(string_buffer, "%s%s%s", edg_lib, PATH_DELIMITER, EDG_MUNCH);
  edg_munch = copy_of_string(string_buffer);
  /* Get any options that should be passed to the prelinker. */
  prelink_default_options = getenv("EDG_PRELINK_DEFAULT_OPTIONS");
#if __WIN32__
  { /* Get the Microsoft C compiler version being used. */
    char	*version;
    version = getenv("EDG_MSVC_VERSION");
    if (version == NULL) {
      msvc_target_version = DEFAULT_MSVC_TARGET_VERSION;
    } else {
      msvc_target_version = scan_opt_arg_number(version);
    }  /* if */
    /* Munch is needed for versions of MSVC prior to 7.0. */
    if (msvc_target_version < 1300) use_munch = TRUE;
  }
  /* Build $EDG_BASE/lib/munch_nm. */
  sprintf(string_buffer, "%s%s%s", edg_lib, PATH_DELIMITER, "munch_nm");
  munch_nm = copy_of_string(string_buffer);
  microsoft_include = getenv("EDG_MS_INCLUDE");
  if (microsoft_include == NULL) {
    microsoft_include = DEFAULT_MICROSOFT_INCLUDE;
  }  /* if */
  /* On Windows, don't use our include directory unless explicitly specified
     by an enviroment variable. */
  edg_include = getenv("EDG_INCLUDE");
#else /* !__WIN32__ */
  /* Build $EDG_BASE/include. */
  sprintf(string_buffer, "%s%sinclude", edg_base, PATH_DELIMITER);
  edg_include = copy_of_string(string_buffer);
#endif /* __WIN32__ */
  /* Build $EDG_BASE/lib/edg_prelink. */
  sprintf(string_buffer, "%s%s%s", edg_lib, PATH_DELIMITER, EDG_PRELINK);
  edg_prelink = copy_of_string(string_buffer);
  /* Build $EDG_BASE/lib/libC.a (or appropriate name). */
  sprintf(string_buffer, "%s%s%s", edg_lib, PATH_DELIMITER, LIBC_NAME);
  edg_libc = copy_of_string(string_buffer);
#if __WIN32__
  /* Build path name to the special prelinker nm command. */
  sprintf(string_buffer, "-c \"%s%s%s\"", edg_lib, PATH_DELIMITER, "pl_nm");
  prelink_options = copy_of_string(string_buffer);
#endif /* __WIN32__ */
  /* Create the start of the link command, just in case it is needed. */
  init_command_line(&link_command);
  /* Initialize the list of object files to be passed to the prelinker and
     linker. */
  init_command_line(&object_file_list);
  /* Initialize the file list. */
  init_command_line(&file_list);
  add_cl_argument(&link_command, C_COMMAND);
  /* Iniitalize the compiler options command line information. */
  init_command_line(&compile_options);
  /* Initialize the linker options. */
  init_command_line(&link_options);
  /* Initialize the C to object options. */
  init_command_line(&c_to_obj_options);
  if (edg_include != NULL) {
    /* Provide the location of the standard include files. */
    sprintf(string_buffer, "-I%s", edg_include);
    str = copy_of_string(string_buffer);
    add_cl_argument(&compile_options, str);
  }  /* if */
  /* Add default defines. */
  add_cl_argument(&compile_options, DEFAULT_DEFINES);
#if __WIN32__
  /* Add an argument to specify the MSVC target version being used. */
  sprintf(string_buffer, "--msvc_target_version=%ld", msvc_target_version);
  str = copy_of_string(string_buffer);
  add_cl_argument(&compile_options, str);
#endif /* __WIN32__ */
  /* Add the Microsoft target compiler version number, if any. */
  /* Initialize the instantiation command line. */
  init_command_line(&instantiation_command_line);
  add_cl_argument(&instantiation_command_line, command_name);
  /* Enable any signal handlers necessary to catch signals that may come
     up during execution (for example, SIGINT). */
  (void)signal(SIGINT, term_on_signal);
  (void)signal(SIGTERM, term_on_signal);
}  /* init */


static void update_template_info_file(a_const_char *file_name)
/*
If there is a .ti file, update it with the necessary driver information.
*/
{
  char			*ti_file_name;
  FILE			*ti_file;
  a_command_line	file_contents;
  a_cl_argument_ptr	clap;
  a_cl_argument_ptr	prev_clap;

  ti_file_name = derived_name(file_name, TEMPLATE_INFO_FILE_SUFFIX);
  ti_file = fopen(ti_file_name, "r");
  if (ti_file != NULL) {
    /* Read in the existing file. */
    init_command_line(&file_contents);
    while (read_input_line(ti_file)) {
      /* Use the command line as a linked list to store the instantiation
         list. */
      char	*name;
      name = copy_of_string(string_buffer);
      add_cl_argument(&file_contents, name);
    }  /* while */
    fclose(ti_file);
    ti_file = fopen(ti_file_name, "w");
    if (ti_file == NULL) {
      fprintf(stderr, "Could not reopen template information file.");
      error_exit();
    }  /* if */
    /* Write the command line to the file. */
    fputs("cmd:", ti_file);
    clap = instantiation_command_line.args;
    while (clap != NULL) {
      fputs(clap->str, ti_file);
      putc(' ', ti_file);
      clap = clap->next;
    }  /* while */
    fprintf(ti_file, "-c\n");
    /* Write the current directory. */
    fprintf(ti_file, "dir:%s\n", curr_dir_name);
    /* Write the name of the file being compiled. */
    fprintf(ti_file, "fnm:%s\n", file_name);
    /* Write the original information from the file. */
    clap = file_contents.args;
    while (clap != NULL) {
      fputs(clap->str, ti_file);
      putc('\n', ti_file);
      prev_clap = clap;
      clap = clap->next;
      /* Free the space used to store the string. */
      free((void *)prev_clap->str);
      free_cl_argument(prev_clap);
    }  /* while */
    fclose(ti_file);
  }  /* if */
}  /* update_template_info_file */



static int compile_file(a_const_char* file_name)
/*
Compile a file and generate an object file.
*/
{
  int			status;
  a_command_line	cl;
  char			*int_c_file_name;
  char			*int_obj_file_name;
  char			*obj_file_name;
#if __WIN32__
  char			*orig_int_c_file_name;
  char			*str;
#endif /* __WIN32__ */

  init_command_line(&cl);
  add_cl_argument(&cl, cpfe_command);
  /* Append any options extracted from the edgcc command line. */
  append_command_line(&cl, &compile_options);
#if __WIN32__
  /* Provide the location of the Microsoft include files. */
  sprintf(string_buffer, "-I%s", microsoft_include);
  str = copy_of_string(string_buffer);
  add_cl_argument(&cl, str);
  /* VC7 and beyond have some header files in a separate PlatformSDK
     directory and atlmfc directory. */
  sprintf(string_buffer, "-I%s/../PlatformSDK/include", microsoft_include);
  str = copy_of_string(string_buffer);
  add_cl_argument(&cl, str);
  sprintf(string_buffer, "-I%s/../atlmfc/include", microsoft_include);
  str = copy_of_string(string_buffer);
  add_cl_argument(&cl, str);
#endif /* __WIN32__ */
  add_cl_argument(&cl, file_name);
  /* In multiple translation unit mode, add the other file names to the
     command. */
  if (multi_trans_unit_mode) {
     a_cl_argument_ptr	file_name_arg;
    /* Add the remaining file names to the command (skip the first file,
       which we have already added). */
    for (file_name_arg = file_list.args->next; file_name_arg != NULL;
         file_name_arg = file_name_arg->next) {
      add_cl_argument(&cl, file_name_arg->str);
    }  /* for */
  }  /* if */
  status = execute_command(&cl);
  if (status == 0 && !fe_only && !preprocess_only) {
    /* Write the compilation command line into the instantiation
       information file. */
    update_template_info_file(file_name);
    /* Compile the generated C file. */
    int_c_file_name = derived_name(file_name, GEN_C_FILE_SUFFIX);
    init_command_line(&cl);
    add_cl_argument(&cl, C_COMMAND);
    add_cl_argument(&cl, "-c");
    append_command_line(&cl, &c_to_obj_options);
#if __WIN32__
    /* Temporary measure - use -Dregister="" to cause register keyword to
       be ignored. */
    add_cl_argument(&cl, "-Dregister=\"\"");
    add_cl_argument(&cl, "-Dsetjmp=\"_setjmp\"");
#endif /* __WIN32__ */
    int_obj_file_name = derived_name(file_name, GEN_C_OBJECT_FILE_SUFFIX);
    add_cl_argument(&cl, int_c_file_name);
    status = execute_command(&cl);
    if (!gflag) {
      remove_file(int_c_file_name);
    }  /* if */
    if (status == 0) {
      /* Add the object file name to the list of files to be linked. */
      obj_file_name = derived_name(file_name, OBJECT_FILE_SUFFIX);
      /* Rename the file from x.int.o to x.o. */
      status = rename_file(int_obj_file_name, obj_file_name);
    }  /* if */
    if (status == 0) {
      /* Add the object file name to the link command. */
      add_cl_argument(&object_file_list, obj_file_name);
    }  /* if */
  }  /* if */
  return status;
}  /* compile_file */


int link_executable(void)
/*
Link the object files into an executable and do any necessary processing
to handle static initialization.
*/
{
  int			status;
  a_command_line	cl;
  a_command_line	second_link_cl;

  /* Invoke the prelinker. */
  init_command_line(&cl);
  add_cl_argument(&cl, edg_prelink);
  if (prelink_options != NULL) add_cl_argument(&cl, prelink_options);
  if (prelink_default_options != NULL) {
    add_cl_argument(&cl, prelink_default_options);
  }  /* if */
  append_command_line(&cl, &object_file_list);
  (void)execute_command(&cl);
  /* Add the list of object files to the link command. */
  append_command_line(&link_command, &object_file_list);
#if __WIN32__
  /* Add the executable file name to the link command. */
  if (output_file_name == NULL) output_file_name = DEFAULT_OUTPUT_FILE_NAME;
  sprintf(string_buffer, "/Fe%s", output_file_name);
  add_cl_argument(&link_command, copy_of_string(string_buffer));
  add_cl_argument(&link_command, LINKER_OPTIONS);
  if (msvc_target_version < 1300) {
    /* A special linker option is needed to generate information for munch
       when using older MSVC versions. */
    add_cl_argument(&link_command, "/debugtype:both");
  } else {
    add_cl_argument(&link_command, "/NODEFAULTLIB:LIBC");
  }  /* if */
#else /* !__WIN32__ */
  /* Add the executable file name to the link command. */
  add_cl_argument(&link_command, "-o");
  add_cl_argument(&link_command, output_file_name);
#endif /* __WIN32__ */
  append_command_line(&link_command, &link_options);
  /* Make a copy of the command line before adding libC. */
  init_command_line(&second_link_cl);
  append_command_line(&second_link_cl, &link_command);
  /* Add libC to the original link line and execute the link. */
  add_cl_argument(&link_command, edg_libc);
  status = execute_command(&link_command);
  if (status == 0 && use_munch) {
    init_command_line(&cl);
    /* Run munch on the output. */
#if __WIN32__
    add_cl_argument(&cl, "link -dump -symbols");
    {
      char *exe_name;
      sprintf(string_buffer, "%s%s", output_file_name,
              EXECUTABLE_FILE_SUFFIX);
      exe_name = copy_of_string(string_buffer);
      add_cl_argument(&cl, exe_name);
    }
    add_cl_argument(&cl, "| ");
    add_cl_argument(&cl, munch_nm);
    add_cl_argument(&cl, "| ");
    add_cl_argument(&cl, edg_munch);
    add_cl_argument(&cl, "> ");
    add_cl_argument(&cl, MUNCH_C_FILE);
#else /* !__WIN32__ */
    add_cl_argument(&cl, "nm");
    add_cl_argument(&cl, output_file_name);
    add_cl_argument(&cl, "| ");
    add_cl_argument(&cl, edg_munch);
    add_cl_argument(&cl, "> ");
    add_cl_argument(&cl, MUNCH_C_FILE);
#endif /* __WIN32__ */
    execute_command(&cl);
    /* Compile the output file. */
    init_command_line(&cl);
    add_cl_argument(&cl, C_COMMAND);
    add_cl_argument(&cl, "-c");
    add_cl_argument(&cl, MUNCH_C_FILE);
#if 0
    /* Redirect the output to a temporary file. */
    add_cl_argument(&cl, " >");
    add_cl_argument(&cl, temp_file);
#endif /* 0 */
    if (execute_command(&cl) != 0) {
      fprintf(stderr,
              "edgcc: compilation of file generated by munch failed.\n");
      status = 1;
    } else {
      /* Do the link again, including the munch generated file. */
      add_cl_argument(&second_link_cl, MUNCH_OBJ_FILE);
      /* Add libC to the original link line and execute the link. */
      add_cl_argument(&second_link_cl, edg_libc);
      status = execute_command(&second_link_cl);
    }  /* if */
    /* Remove the files generated by munch. */
    remove_file(MUNCH_OBJ_FILE);
    remove_file(MUNCH_C_FILE);
  }  /* if */
  return status;
}  /* link_executable */


/*
Takes an argument of the form -Xarg or -X arg and returns the complete
in a string pointed to by str.
*/
#define make_arg_string()					\
  if (arg[2] == '\0') {						\
    /* The argument is in the next argument. */			\
    sprintf(string_buffer, "%s %s", arg, argv[optpos+1]);	\
    optpos++;							\
  } else {							\
    /* The option is part of this argument. */			\
    sprintf(string_buffer, "%s", arg);				\
  }  /* if */							\
  str = copy_of_string(string_buffer);


void proc_command_line(int argc, char *argv[])
/*
Process the command line options.
*/
{
  int		optpos;
  char		*str;
  char		optchar;

  for (optpos = 1; optpos < argc; optpos++) {
    char	*arg = argv[optpos];
    if (arg[0] != '-') {
      /* A file name. */
      add_cl_argument(&file_list, argv[optpos]);
    } else {
      /* An option that begins with -. */
      optchar = arg[1];
      switch (optchar) {
        case 'c':
          /* Compile to object file only. */
          cflag = TRUE;
          break;
        case 'g':
          /* Generate debugging information. */
          gflag = TRUE;
          break;
        case 'o':
          output_file_name = argv[++optpos];
          break;
        case 'E':
        case 'H':
        case 'M':
        case 'P':
          preprocess_only = TRUE;
          goto add_to_compile_options;
        case 'n':
        case 'N':
          /* Options that cause only the front end to be run. */
          fe_only = TRUE;
          goto add_to_compile_options;
        case 'a':
        case 'b':
        case 'm':
        case 'r':
        case 's':
        case 'u':
        case 'v':
        case 'w':
        case 'x':
        case 'A':
        case 'B':
        case 'C':
        case 'K':
        case 'T':
        case 'V':
add_to_compile_options:
          /* Options with no parameters to be passed to the compiler. */
          sprintf(string_buffer, "-%c", optchar);
          str = copy_of_string(string_buffer);
          add_cl_argument(&compile_options, str);
          add_cl_argument(&instantiation_command_line, str);
          break; 
        case 'd':
        case 'e':
        case 'i':
        case 't':
        case 'D':
        case 'I':
        case 'U':
        case 'X':
        case '-':
          /* Options with parameters to be passed to the compiler. */
          make_arg_string();
          add_cl_argument(&compile_options, str);
          add_cl_argument(&instantiation_command_line, str);
          break;
        case 'l':
        case 'L':
          /* Linker options. */
          make_arg_string();
          add_cl_argument(&link_command, str);
          break;
        case 'q':
          /* Multi-trans unit mode. */
          multi_trans_unit_mode = TRUE;
          break;
        case 'S':
          /* Run front end only, but produce a .int.c file. */
          fe_only = TRUE;
          break;
        case 'y':
          /* Add the specifier string as a C to object option. */
          add_cl_argument(&c_to_obj_options, argv[++optpos]);
          break;
        case 'z':
          /* Add the specifier string as a linker option. */
          add_cl_argument(&link_options, argv[++optpos]);
          break;
        case '#':
        case '=':
          /* Display driver debugging information. */
          debug = TRUE;
          break;
        default:
          fprintf(stderr, "edgcc: invalid option: %s\n", arg);
          error_exit();
      }  /* switch */
    }  /* if */
  }  /* while */
}  /* proc_command_line */


int main(int argc, char *argv[])
{
  a_boolean	any_errors = FALSE;
  int		status;
  a_cl_argument_ptr	clap;

  init(argv[0]);
  proc_command_line(argc, argv);
  if (file_list.elements == 0) {
    fprintf(stderr, "No file names were specified.\n");
    error_exit();
  }  /* if */
  /* Loop through the file names. */
  clap = file_list.args;
  while (clap != NULL) {
    a_const_char *file_name = clap->str;
    a_const_char *suffix = get_suffix(file_name);
    if (suffix != NULL &&
        (strcmp(suffix, "c") == 0 || strcmp(suffix, "C") == 0 ||
         strcmp(suffix, "cpp") == 0 || strcmp(suffix, "CPP") == 0 ||
         strcmp(suffix, "cxx") == 0 || strcmp(suffix, "CXX") == 0 ||
         strcmp(suffix, "cc") == 0)) {
      status = compile_file(file_name);
      if (status != 0) any_errors = TRUE;
    } else {
      /* Anything else is assumed to be an object file name or library. */
      add_cl_argument(&object_file_list, file_name);
    }  /* if */
    /* Stop after the first file in multi-trans-unit mode. */
    if (multi_trans_unit_mode) break;
    clap = clap->next;
  }  /* while */
  if (!cflag && !any_errors && !fe_only && !preprocess_only) {
    /* Link the executable file. */
    status = link_executable();
    if (status != 0) any_errors = TRUE;
  }  /* if */
  wrapup();
  return_status = any_errors;
  exit(return_status);
}



