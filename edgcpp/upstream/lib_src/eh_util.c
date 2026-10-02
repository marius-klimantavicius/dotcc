/*
Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
Exceptions.
See https://edgcpp.org/LICENSE.txt for license information.
SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
*/

/*

C++ functions to support exception handling.

*/

#include "basics.h"
#include "runtime.h"
#include "eh.h"

#if EXCEPTION_HANDLING
static a_boolean
		terminate_called_by_runtime = FALSE;
			/* Set to TRUE when terminate() is called by the
			   EH runtime.  This is used by uncaught_exception()
			   to determine whether terminate() has been called. */

static a_boolean
		terminate_called = FALSE;
			/* Set to TRUE when terminate() is called by the
			   either explicitly by the user or by the EH
			   runtime. */

/*
If the runtime should be defined in the std namespace, open
the std namespace.
*/
#ifdef __EDG_RUNTIME_USES_NAMESPACES
namespace std {
#endif /* ifdef __EDG_RUNTIME_USES_NAMESPACES */


NORETURN void terminate() THROW_NOTHING()
/*
The default terminate routine.
*/
{
  /* Detect an attempt for a terminate routine to call itself recursively. */
  if (terminate_called) __abort_execution(ec_terminate_called_more_than_once);
  terminate_called = TRUE;
  if (__default_terminate_routine != NULL) __default_terminate_routine();
  __abort_execution(ec_terminate_returned);
}  /* terminate */


EXTERN_C void __default_terminate(void)
/*
The default terminate routine, which is just a wrapup around abort().
*/
{
  __abort_execution(ec_terminate_called);
}  /* __default_terminate */


STD_NAMESPACE::terminate_handler set_terminate(
                     STD_NAMESPACE::terminate_handler new_func) THROW_NOTHING()
/*
Set the terminate routine pointer to the value passed by the caller
and return the old value.
*/
{
  STD_NAMESPACE::terminate_handler	old_func = __default_terminate_routine;
  __default_terminate_routine = new_func;
  return old_func;
}  /* set_terminate */


STD_NAMESPACE::terminate_handler get_terminate(void) THROW_NOTHING()
/*
Return a pointer to the terminate routine currently installed.
*/
{
  return __default_terminate_routine;
}  /* get_terminate */


NORETURN void unexpected() THROW_NOTHING()
/*
The default unexpected routine.  This routine calls terminate.
*/
{
  if (__default_unexpected_routine != NULL) __default_unexpected_routine();
  terminate();
}  /* unexpected */


a_void_function_ptr set_unexpected(a_void_function_ptr new_func)
THROW_NOTHING()
/*
Set the unexpected routine pointer to the value passed by the caller
and return the old value.
*/
{
  a_void_function_ptr	old_func = __default_unexpected_routine;
  __default_unexpected_routine = new_func;
  return old_func;
}  /* set_unexpected */


int uncaught_exceptions() THROW_NOTHING()
/*
Returns the number of uncaught exception objects in the current process.
*/
{
  an_eh_stack_entry_ptr	ehsep;
  int			result = 0;

  /* Return 0 if terminate() has been called by the implementation. */
  if (!terminate_called_by_runtime) {
    /* This function is used instead of simply using __curr_eh_stack_entry
       because of a problem using this variable in code that also uses
       it via generated EH code. */
    ehsep = __get_curr_eh_stack_entry();
    for (; ehsep != NULL; ehsep = ehsep->next) {
      if (ehsep->kind == ehsek_throw_processing_marker) {
        /* We are processing a throw.  An exception cannot be thrown here
           without resulting in a call to terminate().  Note that this is
           TRUE even if a try block is nested inside the throw processing
           marker. */
        result++;
      }  /* if */
    }  /* for */
  }  /* if */
  return result;
}  /* uncaught_exceptions */


__bool uncaught_exception() THROW_NOTHING()
/*
Return TRUE if an exception is in the process of being thrown.
*/
{
  return uncaught_exceptions() > 0;
}  /* uncaught_exception */

/*
If the runtime should be defined in the std namespace, close
the std namespace.
*/
#ifdef __EDG_RUNTIME_USES_NAMESPACES
}  /* namespace std */
#endif /* ifdef __EDG_RUNTIME_USES_NAMESPACES */


EXTERN_C void __call_unexpected(void)
/*
Used by the EH runtime when unexpected() needs to be called.  When
unexpected() exits by throwing an exception the exception must not
violate the exception specification that caused unexpected() to be
called in the first place.  If it does violate that exception
specification, std::bad_exception is thrown provided it is permitted
by the violated exception specification.  If it is not permitted,
terminate() is called.

*/
{
#if ABI_CHANGES_FOR_RTTI
  try {
    STD_NAMESPACE::unexpected();
  }  /* try */
  catch (...) {
    a_type_info_impl_ptr	thrown_type;
    an_ETS_flag_set		thrown_flags;
    an_ETS_flag_set		*thrown_ptr_flags;
    __type_of_thrown_object(&thrown_type, &thrown_flags, &thrown_ptr_flags);
    if (__can_throw_type(thrown_type, thrown_flags, thrown_ptr_flags)) {
      /* If the thrown type is permitted, rethrow it so that it will be
         handled by an enclosing try block (if any). */
      throw;
    } else {
      a_type_info_impl_ptr	bad_exception_type;
      bad_exception_type =
                   (a_type_info_impl_ptr)&typeid(STD_NAMESPACE::bad_exception);
      if (__can_throw_type(bad_exception_type,
                           (an_ETS_flag_set)ETS_NO_FLAGS,
                           (an_ETS_flag_set*)NULL)) {
        /* The thrown type is not allowed, but bad_exception is.  Throw
           bad_exception. */
        throw STD_NAMESPACE::bad_exception();
      } else {
        /* Neither the originally thrown type not bad_exception is permitted.
           Call terminate. */
        __call_terminate();
      }  /* if */
    }  /* if */
  }  /* catch */
#else /* !ABI_CHANGES_FOR_RTTI */
 /* When RTTI is not used, the old semantics of unexpected() are used in
    which the type of an object thrown by unexpected is not checked against
    the violated exception specification. */
  STD_NAMESPACE::unexpected();
#endif /* ABI_CHANGES_FOR_RTTI */
  /* It should not be possible to get here. */
  abort();
}  /* __call_unexpected */


EXTERN_C void __call_terminate(void)
/*
Used by the EH runtime when terminate needs to be called.  Ensures
that terminate does not return.
*/
{
  terminate_called_by_runtime = TRUE;
  STD_NAMESPACE::terminate();
  /* It should not be possible to get here. */
  abort();
}  /* __call_terminate */


#endif /* EXCEPTION_HANDLING */

