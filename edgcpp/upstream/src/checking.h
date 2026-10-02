/*
Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
Exceptions.
See https://edgcpp.org/LICENSE.txt for license information.
SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
*/

/*

checking.h -- Assertion checking fundamental declarations.

*/

#ifndef EDG_CHECKING_H
#define EDG_CHECKING_H 1

/* Conditionally open the "edg" namespace. */
BEGIN_EDG_NAMESPACE

#if CHECKING
/*lint -sem(assertion_failed, r_no)*/
NORETURN extern void assertion_failed(a_const_char *filename,
                                      int          line_number,
                                      a_const_char *function,
                                      a_const_char *string1,
                                      a_const_char *string2);

/* Macro to test an assertion and generate an internal error if
   the condition is not TRUE.  The macro expands to nothing when checking
   code is not being used. */

/*lint -emacro(774 506, check_assertion)*/
#define check_assertion(test)                                           \
  (((test)) ? (void)0 :                                                 \
    assertion_failed(__FILE__, __LINE__, __EDG_func__,                  \
                     (char *)NULL, (char *)NULL))

/*lint -emacro(774 506, check_assertion_str)*/
#define check_assertion_str(test, string)                               \
  if (!(test))                                                          \
    assertion_failed(__FILE__, __LINE__, __EDG_func__, string, (char *)NULL)

/* Macro that generates an assertion failed internal error.  Intended to
   be used in the else clause of an if statement or the default case of a
   switch statement that is not intended to be reached. */
#define unexpected_condition()                                          \
  assertion_failed(__FILE__, __LINE__, __EDG_func__, (char *)NULL,      \
                   (char *)NULL)

#define unexpected_condition_str(string)                                \
  assertion_failed(__FILE__, __LINE__, __EDG_func__, string, (char *)NULL)

/* Macros that are the same as above except that two strings are provided.
   This is simply done to make it easier to use long strings as arguments. */
/*lint -emacro(774 506, check_assertion_str2)*/
#define check_assertion_str2(test, string1, string2)                    \
  if (!(test))                                                          \
    assertion_failed(__FILE__, __LINE__, __EDG_func__, string1, string2)

#define unexpected_condition_str2(string1, string2)                     \
  assertion_failed(__FILE__, __LINE__, __EDG_func__, string1, string2)
#else /* !CHECKING */
/* Terminate the compilation without a signoff message when encountering an
   unreachable condition. */
NORETURN extern void exit_unrecoverable_compilation();

/* check_assertion must produce a void result. */
#define check_assertion(test) ((void)0)

#define check_assertion_str(test, string) /* Nothing */

#define unexpected_condition() exit_unrecoverable_compilation()

#define unexpected_condition_str(string) exit_unrecoverable_compilation()

#define check_assertion_str2(test, string1, string2) /* Nothing */

#define unexpected_condition_str2(string1, string2) \
  exit_unrecoverable_compilation()

#endif /* CHECKING */

/* Macros that can be used in place of default labels for switch statements
   where it should not be possible to reach the default case (e.g., exhaustive
   coverage via case labels or code structure/assumptions). */
#if CHECK_SWITCH_DEFAULT_UNEXPECTED
/* Check for omitted cases at run-time. */
#define default_is_unexpected() default: unexpected_condition()
#define default_is_unexpected_str(x) default: unexpected_condition_str(x)
#else /* !CHECK_SWITCH_DEFAULT_UNEXPECTED */
/* Enable static analysis detection of missing cases. */
#define default_is_unexpected() /* Nothing */
#define default_is_unexpected_str(x) /* Nothing */
#endif /* CHECK_SWITCH_DEFAULT_UNEXPECTED */

/* Conditionally close the "edg" namespace. */
END_EDG_NAMESPACE

#endif /* ifndef EDG_CHECKING_H */

