/*
Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
Exceptions.
See https://edgcpp.org/LICENSE.txt for license information.
SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
*/

/*

EDG equivalent of the AT&T munch utility.

This program takes the output of the "nm" command and outputs a C
program that initializes an array of static constructors and destructors
to be called before and after the execution of the main program.

The input is expected to be of the form:

xxxxxxxx a nnnn....

"xxxxxxxx" is the symbol's "value" in "nm" terminology.  "a" is the symbol
type, and "nnnn..." is the symbol name.

This program looks for entries where "a" is "T" and "nnnn..." is "__sti__*" or
"__std__*".

*/

#include "basics.h"
#include <stdio.h>
#include <ctype.h>
#include "host_envir.h"
#include "targ_def.h"
#include "edg_munch.h"
#if !__ANSIC__
#include <malloc.h>
#endif /* !__ANSI__ */

/*
If the EDG namespace is being used, make the names from the namespace
visible.
*/
USING_NAMESPACE_EDG

/*
The getopt.h include file will provide either the declarations needed
to use the system getopt routine or, if no system version is available,
the body of our own version of the getopt routine.
*/
#include "getopt.h"

typedef struct a_list_entry*  a_list_entry_ptr;

typedef struct a_list_entry {
  a_list_entry_ptr      next;
  char*                 name;
} a_list_entry;


/*
Lines from standard input are read into this buffer for analysis.
*/
#define INPUT_LINE_SIZE 32767
static char   input_line_buffer[INPUT_LINE_SIZE];
static int    line_size;
                /* Number of characters in the input line not including
                   the final null. */

/* TRUE if external names have an extra underscore prefix.  Can be
   modified by a command line option. */
static a_boolean		skip_underscore_prefix
                                   = TARG_EXTERNAL_NAMES_GET_UNDERSCORE_ADDED;

/*
Simple macro to find the end of the identifier.  Right now it just looks
for white space or the end of the line.  This can be made fancier if
needed.
*/
#define is_id_char(c) (((c) != ' ') && ((c) != '\t') && ((c) != '\0'))


static void error_util(a_const_char *error_string)
/*
Prints an error message and exits with an error exit status.
*/
{
  fprintf(stderr, "edg_munch: %s\n", error_string);
  exit (RC_ERROR);
}


static void internal_error_util(a_const_char *error_string)
/*
Prints an internal error message and exits with a catastrophic error
exit status.
*/
{
  fprintf(stderr, "edg_munch: %s\n", error_string);
  exit (RC_CATASTROPHE);
}


static a_void_ptr malloc_with_check(sizeof_t size)
/*
Interface to malloc that allocates "size" bytes.  Checks for failure of
allocation and generates a catastrophic error.
*/
{
  a_void_ptr ptr;

  if ((ptr = (a_void_ptr)malloc(size)) == NULL) {
    error_util("out of memory");
  }  /* if */
  return (ptr);
}  /* malloc_with_check */


static int read_input_line(void)
/*
Reads a line of input from standard input.  Returns TRUE if a line of
input is being returned.  Returns FALSE at end-of-file.  Sets "line_size"
to the number of characters read not including the trailing null character.
*/
{
  char*  buffer_pos = &input_line_buffer[0];
  int    size = 0;
  int    ch;
  int    result;

  while (ch = getchar(), ch != EOF && ch != '\n') {
    if (++size > INPUT_LINE_SIZE) {
      internal_error_util("read_input_line: input line too long.");
    }  /* if */
    *buffer_pos++ = ch;
  }  /* while */
  
  /* Terminate string with a null character. */
  *buffer_pos++ = '\0';

  /* Determine whether to return end-of-file (FALSE). */
  result = TRUE;
  if (ch == EOF && size == 0) result = FALSE;

  /* Save the number of characters read. */
  line_size = size;
  return (result);
}  /* read_input_line */


static int check_type_and_get_name(char**    name_pos,
                                   int       *name_length,
                                   a_boolean *is_ctor)
/*
This routine looks at the input line, determines whether it is a
static constructor or destructor that should be included in
static construction and destruction.  Returns TRUE if the name
should be processed and returns the starting position and
length of the name.
*/
{
  int             result = FALSE;
  char*           pos = &input_line_buffer[0];
  char            ch;
  char*           local_name_pos;
  char            type;
  static sizeof_t ctor_prefix_length = 0;
  static sizeof_t dtor_prefix_length = 0;

  /* Check for one-time initialization of ctor and dtor prefix lengths. */
  if (ctor_prefix_length == 0) {
    ctor_prefix_length = strlen(CTOR_PREFIX);
    dtor_prefix_length = strlen(DTOR_PREFIX);
  }  /* if */

  /* Less that 12 characters of input must be invalid. */
  if (line_size < 12) goto invalid_input;

  /* Skip over the first field which is expected to contain the
     value field.  Skip to a blank. */
  while((ch = *pos), ch != ' ' && ch != '\0') pos++;

  /* Look for blank after value. */
  if (*pos++ != ' ') goto invalid_input;

  /* Now look for a nonblank. */
  while (*pos == ' ') pos++;

  /* Get the type code. */
  type = *pos++;
  if (!isalpha((unsigned char)type)) goto invalid_input;

  /* Look for blank after type. */
  if (*pos++ != ' ') goto invalid_input;

  /* Now look for a nonblank. */
  while (*pos == ' ') pos++;

  /* Skip past extra underscore at the start of every symbol if an
     underscore is present.  */
  if (skip_underscore_prefix && *pos == '_') pos++;

  /* Save the position of the start of the name. */
  local_name_pos = pos;

  /* Check type and name prefix. */
  if (type == EXTERN_TYPE) {
    /* It has the right type -- now check to see if the beginning of the
       name is what we are looking for. */
    if (strncmp(pos, CTOR_PREFIX, ctor_prefix_length) == 0) {
      result = TRUE;
      *is_ctor = TRUE;
    } else if (strncmp(pos, DTOR_PREFIX, dtor_prefix_length) == 0) {
      result = TRUE;
      *is_ctor = FALSE;
    }  /* if */
    
    /* We have found an entry that needs processing. */
    if (result) {
      /* Set name length. */
      int length = 0;
      while (ch = *pos++, is_id_char(ch)) ++length;
      *name_length = length;
      /* Return the starting position of the name. */
      *name_pos = local_name_pos;
    }  /* if */
  }   /* if */

  return (result);

invalid_input:
  error_util("invalid input format");
  /*NOTREACHED*/
  return 0;
}  /* check_type_and_get_name */



static void create_output(a_list_entry_ptr list_ptr,
                          a_const_char     *array_name)
/*
Generate the output for this list of functions.
*/
{
  a_list_entry_ptr    entry;

  /* Constructor declarations. */
  entry = list_ptr;
  while (entry) {
    printf("%s %s();\n", FUNCTION_RETURN_TYPE, entry->name);
    entry = entry->next;
  }  /* while */
  printf("\n");

  /* Array definition. */
  printf("%s %s[] = {\n", FUNCTION_PTR_TYPEDEF_NAME, array_name);
  entry = list_ptr;
  while (entry) {
    printf("\t%s,\n", entry->name);
    entry = entry->next;
  }  /* while */

  /* Terminate array with a null entry and closing brace. */
  printf("\t0\n};\n");
}  /* create_output */



int main(int argc, char *argv[])
{
  a_list_entry_ptr     ctor_list = NULL;
  a_list_entry_ptr     dtor_list = NULL;
  a_list_entry_ptr     entry;
  a_list_entry_ptr     last_entry = NULL;
  char*                name_pos;
  char*                name_string;
  int                  name_length;
  a_boolean            is_ctor;
  int		       optchar;
  int		       lines_to_skip = 0;

  /* Process command-line options. */
  /* Suppress getopt's error on non-recognized option. */
  opterr = 0;
#define OPTION_LIST "ui:"
  while ((optchar = getopt(argc, argv, OPTION_LIST)) != EOF) {
    switch (optchar) {
      case 'i':
        /* Ignore the first N lines of the input.  This is used to
           ignore the header information put out by nm commands like
           the one on Solaris. */
        lines_to_skip = atoi(optarg);
        break;
      case 'u':
        /* Specify whether names have an extra underscore that should
           be ignored.  The option selects the opposite of the default. */
        skip_underscore_prefix = !TARG_EXTERNAL_NAMES_GET_UNDERSCORE_ADDED;
        break;
      default:
        if (optind >= argc) optind = argc-1;
        optarg = argv[optind];
        fprintf(stderr, "Unrecognized option: %s\n", optarg);
        error_util("command line error");
        break;
    }  /* switch */
  }  /* while */
  /* Skip over any lines that are supposed to be ignored */
  for (; lines_to_skip > 0; lines_to_skip--) read_input_line();
  while (read_input_line()) {
    /* Skip empty lines. */
    if (line_size == 0) continue;
    if (check_type_and_get_name(&name_pos, &name_length, &is_ctor)) {
      /* If the name is the same as the last entry, skip this one.  Some
         systems put out duplicate entries for the sti and std routines. */
      if (last_entry != NULL &&
          name_length == (int)strlen(last_entry->name) &&
          strncmp(last_entry->name, name_pos, name_length) == 0) continue;
      /* Make a copy of the routine name. */
      name_string = (char *)malloc_with_check(size_t_arg(name_length + 1));
      strncpy(name_string, name_pos, name_length);
      /* Add null to name string. */
      name_string[name_length] = '\0';
      /* Make a link entry for this routine. */
      entry = (a_list_entry_ptr)malloc_with_check(sizeof(a_list_entry));
      entry->name = name_string;
      entry->next = NULL;
      last_entry = entry;
      /* Add to appropriate list.  The lists are built backwards (new items
         put on the front).  This matches the output from the AT&T munch
         utility.  */
      if (is_ctor) {
        /* Add to front of constructor list. */
        entry->next = ctor_list;
        ctor_list = entry;
      } else {
        /* Add to front of destructor list. */
        entry->next = dtor_list;
        dtor_list = entry;
      }  /* if */
    }  /* if */
  }  /* while */

  /* Generate output.  The output consists of the following:

     1. Typedef of a pointer to a function returning void.
     2. Declarations of the constructor functions.
     3. Definition of the constructor pointer array.
     4. Declarations of the destructor functions.
     5. Definition of the destructor pointer array.
  */

  /* Output typedef. */
  printf("typedef %s (*%s)();\n", FUNCTION_RETURN_TYPE,
         FUNCTION_PTR_TYPEDEF_NAME);

  /* Constructor declarations and array definition. */
  if (ctor_list) create_output(ctor_list, CTORS_ARRAY_NAME);
  printf("\n");

  /* Destructor declarations and array definition. */
  if (dtor_list) create_output(dtor_list, DTORS_ARRAY_NAME);

  return (RC_NORMAL);
}  /* main */


