/*
Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
Exceptions.
See https://edgcpp.org/LICENSE.txt for license information.
SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
*/

/*

thread_dtor.c -- thread_local destruction list processing.

*/

#include "basics.h"
#include "runtime.h"
#pragma hdrstop
#include "static_init.h"
#include "dtor_list.h"

#if RUNTIME_DOES_THREAD_LOCAL_DESTRUCTIONS

#if USE_PTHREADS
#include <pthread.h>

extern "C" {

/*ARGSUSED*/ /* argument is unused */
static void __thread_terminated(void *unused)
/*
Invoked at pthread destruction time and used as a hook to perform all
thread_local destructions for the current thread.  Note that dso_handle
is not used here (though it could be for some implementations).
*/
{
#ifndef __EDG_IA64_ABI
  __process_destruction_list(&__thread_needed_destruction_head);
#else /* !__EDG_IA64_ABI */
  __finalize_destructions(&__thread_needed_destruction_head, NULL);
#endif /* __EDG_IA64_ABI */
}  /* __thread_terminated */

}

#endif /* USE_PTHREADS */

static int __thread_register_finalization_routine(void)
/*
Arrange for a callback when the current thread is about to terminate
(so the thread_local destructions can be executed).

This notification mechanism relies heavily on the underlying operating
system and its support for threading.  For operating systems that use
POSIX threads, one possible implementation is given here.

Returns zero on success.
*/
{
#if USE_PTHREADS
  static thread_local pthread_key_t pthread_key;
  int result =  pthread_key_create(&pthread_key, __thread_terminated);
  if (result == 0) {
    /* The key must have a non-NULL value in order for the destructor to be
       invoked later, so set it to a non-NULL value. */
    result = pthread_setspecific(pthread_key, &pthread_key);
  }  /* if */
  return result;
#else /* !USE_PTHREADS */
 #error Need an OS-specific hook for thread termination
#endif /* USE_PTHREADS */
}  /* __thread_register_finalization_routine */

#ifndef __EDG_IA64_ABI

EXTERN_C void __record_needed_thread_destruction(a_needed_destruction_ptr ndp)
/*
Called when a thread_local object has been constructed to register a
destruction that must be done at thread termination.  ndp points to
a needed destruction entry that is to be added to the front of the
list of destructions for the current thread.
*/
{
  if (__thread_needed_destruction_head == NULL) {
    /* Arrange to be notified when the thread terminates. */
    __register_finalization_routine();
    if (__thread_register_finalization_routine() != 0) {
      /* Abort the execution. */
      __abort_execution(ec_thread_registration_failed);
    }  /* if */
  }  /* if */
  __record_destruction_on_list(&__thread_needed_destruction_head, ndp);
}  /* __record_needed_thread_destruction */


#else /* defined(__EDG_IA64_ABI) */

#if !SYSTEM_RUNTIME_HAS_IA64_THREAD_ATEXIT

#if SYSTEM_RUNTIME_HAS_IA64_ATEXIT
/*ARGSUSED*/ /* argument is unused. */
static void finalize_current_thread_destructions(void *unused)
/*
In configurations where the EDG runtime library is recording the thread_local
destructions but the system runtime library maintains the static and
atexit destructions, the EDG runtime library has no "hook" to initiate
thread_local destructions when, e.g., a thread calls exit().  In such cases,
this routine is registered as an atexit handler with the system's library,
which ensures that the thread_local destructions (for the current thread) will
take place, albeit out-of-sequence (i.e., they should occur before static and
other atexit destructions).
*/
{
  __finalize_destructions(&__thread_needed_destruction_head, NULL);
}  /* finalize_current_thread_destructions */

#endif /* SYSTEM_RUNTIME_HAS_IA64_ATEXIT */

int ABI_NAMESPACE::__cxa_thread_atexit(a_cxa_dtor_ptr   destruction_routine,
                                       void             *object,
                                       a_dso_handle     dso_handle)
/* 
Register an action to be taken at thread termination (or DSO unload) time.
The action is the calling of destruction_routine with the object parameter.

This version of __cxa_thread_atexit is only intended to be used when the
system's runtime does not include its own version of __cxa_thread_atexit.  When
the system's runtime includes such a function, the system is responsible
for seeing that thread_local destructions are invoked at thread termination.
When using our own version of this routine, call
__thread_register_finalization_routine on the first invocation of this function
so that we'll be informed when the thread terminates.  Return zero if the
registration is successful, or non-zero otherwise.
*/
{
  int result = 0;

#if SYSTEM_RUNTIME_HAS_IA64_ATEXIT
  static int once = 0;
  if (!once) {
    /* The EDG runtime is keeping track of the thread_local destructions,
       but the system runtime is keeping track of objects with static
       storage.  Register a routine to be called atexit to run the
       thread_local destructions (even though they may be out-of-sequence). */
    once = 1;
    ABI_NAMESPACE::__cxa_atexit(finalize_current_thread_destructions,
                                NULL, NULL);
  }  /* if */
#endif /* SYSTEM_RUNTIME_HAS_IA64_ATEXIT */
  if (__thread_needed_destruction_head == NULL) {
    /* Arrange to be notified when the thread terminates. */
    if (__thread_register_finalization_routine() != 0) {
      result = 1;
    }  /* if */
  }  /* if */
  if (result == 0) {
    result = __add_destruction_to_list(&__thread_needed_destruction_head,
                                       destruction_routine,
                                       object, dso_handle);
  }  /* if */
  return result;
}  /* __cxa_thread_atexit */

#endif /* !SYSTEM_RUNTIME_HAS_IA64_THREAD_ATEXIT */

#endif /* defined(__EDG_IA64_ABI) */

#endif /* RUNTIME_DOES_THREAD_LOCAL_DESTRUCTIONS */

