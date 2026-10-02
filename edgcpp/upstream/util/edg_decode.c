/*
Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
Exceptions.
See https://edgcpp.org/LICENSE.txt for license information.
SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
*/

/*
edg_decode.c -- Name demangler for C++ (standalone program)

This program reads input from stdin, writes output to stdout.
Things that look like mangled names in the input are demangled.
Everything else is passed through unchanged.

The -u option reverses the default setting for whether external
names have added leading underscores.

The demangling is intended to work only on names of external entities.
There is some name mangling done for internal entities, or by the
C-generating back end, that this program does not try to decode.
*/
#include "basics.h"
#include "host_envir.h"
#include "targ_def.h"
#include "getopt.h"

/*
If the EDG namespace is being used, make the names there visible.
*/
USING_NAMESPACE_EDG

#include "decode.h"

/*
TRUE if external names have an extra underscore prefix.  Can be
modified by a command line option.
*/
static a_boolean
		skip_underscore_prefix =
                                      TARG_EXTERNAL_NAMES_GET_UNDERSCORE_ADDED;
#if IA64_ABI
/*
TRUE if the bugs in the g++ 3.2 implementation of the IA-64 ABI should
be emulated.  Can be changed by a command line option.  Initialized
in decode.c.
*/
extern a_boolean
		emulate_gnu_abi_bugs;
#endif /* IA64_ABI */

static int	ch;	/* Current input character. */

#define MAX_ID_LENGTH 15000
			/* Maximum size of an identifier. */
static char	orig_id[MAX_ID_LENGTH];
			/* Identifier being processed currently, as read. */
static char	*demangled_id;
			/* Demangled form of the current identifier,
			   dynamically allocated. */
static sizeof_t	demangled_id_size = MAX_ID_LENGTH;
			/* The size of demangled_id (initially the same size
			   as the input buffer, but may change. */


/*
Return TRUE if the given character is one that can start an identifier.
The set includes "$" as an extension.  This is a macro, and it evaluates
its argument more than once.
*/
#define is_id_start_char(ch)                                          \
  ((ch) != EOF &&                                                     \
   (isalpha((unsigned char)(ch)) || (ch) == '_' || (ch) == '$'))

/*
Return TRUE if the given character is one that can be part of an identifier
after the first character.  This is a macro, and it evaluates its argument
more than once.
*/
#define is_id_following_char(ch)                                      \
  (ch != EOF && (is_id_start_char(ch) || isdigit((unsigned char)(ch))))


static void process_identifier(void)
/*
The current input character is the beginning of an identifier.  Read the
identifier, process it, and output it.  On return, the current character
is the one following the identifier.
*/
{
  a_boolean     is_mangled_name = FALSE, too_long_err = FALSE;
  unsigned long i, orig_id_len;

  orig_id_len = 1;
  orig_id[0] = ch;
  /* Accumulate the identifier. */
  for (;;) {
    /* Get another character, stop on a non-identifier character. */
    ch = getchar();
    if (!is_id_following_char(ch)) break;
    if (orig_id_len >= MAX_ID_LENGTH-1) {
      /* Identifier is too long. */
      if (!too_long_err) {
        /* First time through. */
        /* Dump the identifier (the part seen so far) in original form. */
        for (i = 0; i < orig_id_len; i++) putchar(orig_id[i]);
        too_long_err = TRUE;
      }  /* if */
      /* Keep going to the end of the identifier, passing through the rest
         of the characters. */
      putchar(ch);
    } else {
      /* Add the character to the current identifier. */
      orig_id[orig_id_len] = ch;
#if !IA64_ABI
      /* Keep track of whether "__" appears in the name. */
      if (ch == '_' && orig_id_len > 0 && orig_id[orig_id_len-1] == '_') {
        is_mangled_name = TRUE;
      }  /* if */
#endif /* !IA64_ABI */
      orig_id_len++;
    }  /* if */
  }  /* for */
  /* The identifier has been accumulated. */
  if (too_long_err) {
    /* It was too long (it's already been copied unchanged to the output). */
  } else {
    char *id = orig_id;
    /* The identifier was not too long.  Add a terminating null. */
    orig_id[orig_id_len] = '\0';
    /* If external names are supposed to begin with an underscore, drop the
       underscore.  Furthermore, an identifier that does not begin with an
       underscore cannot be an external name, so it shouldn't be demangled. */
    if (skip_underscore_prefix) {
      if (id[0] == '_') {
        id++;
        orig_id_len--;
      } else {
        is_mangled_name = FALSE;
      }  /* if */
    }  /* if */
#if IA64_ABI
    /* An IA-64 mangled name begins with "_Z". */
    if (orig_id_len > 2 && id[0] == '_' && id[1] == 'Z') {
      is_mangled_name = TRUE;
    } else if (orig_id_len > 4 &&
               id[0] == '_' && id[1] == '_' &&
               (id[2] == 'b' || id[2] == 'v') &&
               id[3] == '_') {
      /* Also demangle fabricated field names (that start with __b_ or __v_).*/
      is_mangled_name = TRUE;
    }  /* if */
#endif /* IA64_ABI */
    if (is_mangled_name) {
      a_boolean err, buffer_overflow_err;
      sizeof_t  required_buffer_size;
      do {
        /* Demangle the identifier. */
        decode_identifier(id, demangled_id, demangled_id_size,
                          &err, &buffer_overflow_err, &required_buffer_size);
        if (err && buffer_overflow_err) {
          /* The demangled name doesn't fit in the output buffer; increase
             the size and try again. */
          if (required_buffer_size <= demangled_id_size) {
            /* Buffers should only get larger. */
            fprintf(stderr, "Request to allocate smaller buffer\n");
            exit(RC_CATASTROPHE);
          }  /* if */
          /* Double the size of the buffer (making sure that it's at least
             large enough to handle the demangled name in question). */
          demangled_id_size *= 2;
          if (demangled_id_size < required_buffer_size) {
            demangled_id_size = required_buffer_size;
          }  /* if */
          /* Re-allocate the buffer (no need to save its contents). */
          free(demangled_id);
          demangled_id = (char*)malloc(demangled_id_size);
          if (demangled_id == NULL) {
            perror(NULL);
            exit(RC_CATASTROPHE);
          }  /* if */
        }  /* if */
      } while (buffer_overflow_err);
      /* On an error, force output of the original form of the name. */
      if (err) is_mangled_name = FALSE;
    }  /* if */
    if (!is_mangled_name) {
      /* Output the original form of the identifier. */
      fputs(orig_id, stdout);
    } else {
      /* Output the demangled form. */
      fputs(demangled_id, stdout);
    }  /* if */
  }  /* if */
#undef MAX_ID_LENGTH
}  /* process_identifier */


int main(int argc, char *argv[])
/*
edg_decode utility program -- demangles names for C++.
*/
{
  int optchar;

  /* Process command-line options. */
  /* Suppress getopt's error on non-recognized option. */
  opterr = 0;
#if IA64_ABI
#define OPTION_LIST "ug"
#else /* !IA64_ABI */
#define OPTION_LIST "u"
#endif /* IA64_ABI */
  /* Allocate the output buffer (initially the same size as the input buffer
     but can grow). */
  demangled_id = (char*)malloc(demangled_id_size);
  if (demangled_id == NULL) {
    perror(NULL);
    return RC_CATASTROPHE;
  }  /* if */
  while ((optchar = getopt(argc, argv, OPTION_LIST)) != EOF) {
    switch (optchar) {
      case 'u':
        /* Specify whether names have an extra underscore that should
           be ignored.  The option selects the opposite of the default. */
        skip_underscore_prefix = !TARG_EXTERNAL_NAMES_GET_UNDERSCORE_ADDED;
        break;
#if IA64_ABI
      case 'g':
        /* Specify whether g++ 3.2 bugs in the implementation of the IA-64
           ABI should be emulated. */
        emulate_gnu_abi_bugs = !DEFAULT_EMULATE_GNU_ABI_BUGS;
        break;
#endif /* IA64_ABI */
      default:
        if (optind >= argc) optind = argc-1;
        optarg = argv[optind];
        fprintf(stderr, "Unrecognized option: %s\n", optarg);
        return RC_ERROR;
    }  /* switch */
  }  /* while */
  /* Read and echo characters until end of file.  When the start of an
     identifier is encountered, process it specially. */
  while ((ch = getchar()) != EOF) {
    /* Look for the start of an identifier. */
    if (is_id_start_char(ch)) {
      process_identifier();
      /* If the identifier runs into the end of file without a preceding
         newline, end the loop. */
      if (ch == EOF) break;
    }  /* if */
    putchar(ch);
  }  /* while */
  return RC_NORMAL;
}  /* main */


