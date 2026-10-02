/*
Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
Exceptions.
See https://edgcpp.org/LICENSE.txt for license information.
SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
*/

/*

getopt.h -- command line option processing.

*/

#ifndef GETOPT_H
#define GETOPT_H 1

/*
Flag that is TRUE if the parameters of getopt are not declared as
const.
*/
#ifndef GETOPT_PARAMS_ARE_NOT_CONST
#define GETOPT_PARAMS_ARE_NOT_CONST FALSE
#endif /* !defined(GETOPT_PARAMS_ARE_NOT_CONST) */

#if __SYSV__ && !__VMS__

/* External declarations for getopt.  On some systems the last two
   parameters are const and on some they are not.  By default we
   assume that they are const.  On systems for which this is
   not correct, the configuration flag GETOPT_PARAMS_ARE_NOT_CONST
   must be defined. */
#if GETOPT_PARAMS_ARE_NOT_CONST
int getopt(int argc, char ** argv, char *optstring);
#else /* !GETOPT_PARAMS_ARE_NOT_CONST */
int getopt(int argc, char * const * argv, const char *optstring);
#endif /* GETOPT_PARAMS_ARE_NOT_CONST */
extern char *optarg;
extern int optind, opterr;

#else /* __ANSIC__ || __BSD__ || __VMS__ */

/*
Define a getopt-equivalent for systems that don't have one.
*/
char		*optarg;
			/* Returned from getopt -- Pointer to the current
			   option argument. */
int		optind = 1;
			/* Index of the current option in argv. */
int		opterr = 1;
			/* If non-zero, produce an error message on
			   a bad option. */

#if GETOPT_PARAMS_ARE_NOT_CONST
int getopt(int argc, char ** argv, char *optstring)
#else /* !GETOPT_PARAMS_ARE_NOT_CONST */
int getopt(int argc, char * const * argv, const char *optstring)
#endif /* GETOPT_PARAMS_ARE_NOT_CONST */
/*
Fetch a command-line option.  This routine is a functional analogue of
the System V getopt routine (see the SVID, getopt(BA_LIB)).  argc and
argv are the count of command-line arguments and the array containing
the command-line argument strings.  *optstring is a string of
recognized option letters; for those options that take an argument,
the letter is followed by a ":".  One option letter (with optarg
pointing to the option value if appropriate) is returned on each call.
When there are no more options (i.e., the next thing does not begin with
a "-", or it is "--" or "-"), getopt returns EOF.  optind at that
point indicates the argv index of the non-option argument.  If an
invalid option is used, getopt will return a "?"; if opterr is non-zero,
getopt will also output an error message.
*/
{
  int         return_value;
  char        *optpos;
  static char *optchar = NULL;
			/* The character position containing the
			   next option letter to be examined, or NULL
			   if a new argument should be begun. */

  /* See if a new argument must be begun (i.e., there is not
     part of an existing option to finish). */
  if (optchar == NULL) {
start_new_argument:
    if (optind >= argc) {
      /* No more arguments. */
      return_value = EOF;
      goto end_of_routine;
    } else {
      optchar = argv[optind];
      if (*optchar != '-') {
        /* The argument string does not begin with a "-". */
        return_value = EOF;
        goto end_of_routine;
      } else if (*(optchar+1) == '-') {
        if (*(optchar+2) == '\0') {
          /* The argument is "--", which marks the end of the options.
             Swallow this argument. */
          optind++;
          return_value = EOF;
        } else {
          /* This is an argument that begins with "--" that we don't handle. */
          return_value = '?';
        }  /* if */
        goto end_of_routine;
      } else if (*(optchar+1) == '\0') {
        /* The argument is "-", which is used to indicate stdin as a
           file name.  Return without swallowing this argument. */
        return_value = EOF;
        goto end_of_routine;
      }  /* if */
      /* We have the start of a new option.  Advance past the "-". */
      optchar++;
    }  /* if */
  }  /* if */
  /* Here, optchar points to the next option character.  See if the
     current option letter list has been exhausted. */
  if (*optchar == '\0') {
    /* Start the next argument. */
    optind++;
    goto start_new_argument;
  }  /* if */
  /* See if the option letter appears in the string of legal options. */
  optpos = strchr((char *)optstring, *optchar);
  if (optpos == NULL) {
    /* Bad option letter. */
    if (opterr) fprintf(stderr, "%s: illegal option -- %c\n", argv[0],
                                *optchar);
    return_value = '?';
    goto end_of_routine;
  }  /* if */
  /* Valid option letter, return it. */
  return_value = *optchar;
  /* See if the option takes an argument. */
  if (*(optpos+1) == ':') {
    if (*(optchar+1) == '\0') {
      /* The option letter is the last thing in the argument, so use the
         next argument as the option value, as in "-I xxx". */
      optind++;
      if (optind >= argc) {
        /* There are no remaining arguments, so the option argument is
           missing. */
        if (opterr) fprintf(stderr, "%s: option requires an argument -- %c\n",
                                    argv[0], *optchar);
        return_value = '?';
        goto end_of_routine;
      }  /* if */
      optarg = argv[optind];
    } else {
      /* The option argument is the remainder of the current argument,
         as in "-Ixxx". */
      optarg = optchar+1;
    }  /* if */
    /* In either case, take no more characters of the current argument. */
    optchar = NULL;
    optind++;
  } else {
    /* The option does not take an argument. */
    optchar++;
    optarg = NULL;
  }  /* if */
end_of_routine:
  return(return_value);
}  /* getopt */
#endif /* else of __SYSV__ */

#endif /* ifndef GETOPT_H */

