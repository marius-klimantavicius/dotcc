/*
Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
Exceptions.
See https://edgcpp.org/LICENSE.txt for license information.
SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
*/

/*

dtor_list.c -- destruction list processing.

*/

#include "basics.h"
#include "runtime.h"
#pragma hdrstop
#include "static_init.h"
#include "dtor_list.h"

#if !defined(__EDG_IA64_ABI) || !SYSTEM_RUNTIME_HAS_IA64_SUPPORT || \
    !SYSTEM_RUNTIME_HAS_IA64_ATEXIT

/*
The list of static objects that require destruction.  An entry is
added to the front of this list each time a new destructible static
object is created.
*/
static a_needed_destruction_ptr
		needed_destruction_head /* = NULL*/;

#endif /* !defined(__EDG_IA64_ABI) || !SYSTEM_RUNTIME_HAS_IA64_SUPPORT
          !SYSTEM_RUNTIME_HAS_IA64_ATEXIT */

#if RUNTIME_DOES_THREAD_LOCAL_DESTRUCTIONS

/*
The list of thread_local objects that require destruction.  An entry is
added to the front of this list each time a new destructible thread_local
object is created.
*/
thread_local a_needed_destruction_ptr
		__thread_needed_destruction_head /* = NULL*/;

#endif /* RUNTIME_DOES_THREAD_LOCAL_DESTRUCTIONS */


#ifndef __EDG_IA64_ABI

void __process_destruction_list(a_needed_destruction_ptr *destruction_list)
/*
Go through the specified destructions list and perform the required
destructions.
*/
{
  a_needed_destruction_ptr	ndp;
  while (*destruction_list != NULL) {
    void	*object_ptr;
    /* Note that the value of *destruction_list may change
       during the execution of the destructor.  Consequently, the
       current entry is removed from the list before the destructor
       routine is called. */
    ndp = *destruction_list;
    *destruction_list = (*destruction_list)->next;
    object_ptr = ndp->object;
    /* Choose between a simple and complex destruction based on whether
       or not the object pointer is NULL. */
    if (object_ptr != NULL) {
      a_destructor_ptr	dp;
      /* Destroy the object by calling a destructor.  The flag value of 2
         indicates the object should be destroyed, but operator delete
         should not be called. */
      dp = (a_destructor_ptr)ndp->destruction_routine;
      (dp)(object_ptr, 2);
    } else {
      /* Destroy the object by calling a special function that will do the
         destruction of this specific object. */
      (ndp->destruction_routine)();
    }  /* if */
  }  /* while */
}  /* __process_destruction_list */


void __process_needed_destructions(void)
/*
Go through the needed destructions lists and perform the required
destructions (for the current thread as well as the process).
*/
{
#if RUNTIME_DOES_THREAD_LOCAL_DESTRUCTIONS
  /* thread_local destructions are performed first. */
  __process_destruction_list(&__thread_needed_destruction_head);
#endif /* RUNTIME_DOES_THREAD_LOCAL_DESTRUCTIONS */
  __process_destruction_list(&needed_destruction_head);
}  /* __process_needed_destructions */


EXTERN_C void __already_marked_for_destruction()
/*
This routine is called when a needed destruction is recorded more than
once.  It simply calls abort.  The name is intended to describe the nature
of the problem to the user.
*/
{
  __abort_execution(ec_already_marked_for_destruction);
}

void __record_destruction_on_list(a_needed_destruction_ptr *destruction_list,
                                  a_needed_destruction_ptr ndp)
/*
Add ndp to the list pointed to by *destruction_list.
*/
{
  /* If the entry has already been put on the list, terminate the execution. */
  if (ndp->next != NULL || ndp == *destruction_list) {
    __already_marked_for_destruction();
  }  /* if */
  ndp->next = *destruction_list;
  *destruction_list = ndp;
}  /* __record_destruction_on_list */


EXTERN_C void __record_needed_destruction(a_needed_destruction_ptr ndp)
/*
Called when a static object has been constructed to register a
destruction that must be done at program termination.  ndp points to
a needed destruction entry that is to be added to the front of the
list of needed destructions.
*/
{
  if (needed_destruction_head == NULL) {
    /* If this is the first static destruction, register to be called when
       the program exits so that destructions can be properly handled. */
    __register_finalization_routine();
  }  /* if */
  __record_destruction_on_list(&needed_destruction_head, ndp);
}  /* __record_needed_destruction */


#if CFRONT_COMPATIBILITY_MODE
EXTERN_C void __std__needed_destruction_list(void)
/*
This routine is provided for use with a cfront runtime library, including
cfront startup and termination code.  This routine will be called by
the cfront static destruction routines and will ensure that any needed
destructions get done.  The sequence of the destructions will not
be standard conforming, but there is no way to get standard conforming
behavior when using the cfront termination routines.

This routine will only be used when munch is being used.  When patch
is being used, the link structure defined below will result in a call to
__process_needed_destructions.
*/
{
  __process_needed_destructions();
}  /* __std__needed_destruction_list */


/*
Define a link structure that will be used when patch is being used.
*/
struct a_link {
  a_link	*next;
  a_void_function_ptr
		ctor;
  a_void_function_ptr
		dtor;
};

static a_link __link = {(a_link*)NULL,
                        (a_void_function_ptr)NULL,
                        (a_void_function_ptr)__process_needed_destructions};


/*
This declaration is used to force the static definition of __link to be
put out (i.e., to make the compiler think that __link is actually used).
*/
a_link* __dummy_variable_used_to_force_definition_of__link = &__link;
#endif /* CFRONT_COMPATIBILITY_MODE */

#else /* defined(__EDG_IA64_ABI) */

#if !SYSTEM_RUNTIME_HAS_IA64_SUPPORT
/* Current versions of Linux already define these routines in the C runtime
   library, and if we attempt to redefine them here we end up with multiple
   versions of these symbols. */

/* The DSO handle.  Initialization to NULL indicates that this is the handle
   for the main program.  */
a_dso_handle __dso_handle;

#endif /* !SYSTEM_RUNTIME_HAS_IA64_SUPPORT */

#if !SYSTEM_RUNTIME_HAS_IA64_ATEXIT || !SYSTEM_RUNTIME_HAS_IA64_THREAD_ATEXIT

void __finalize_destructions(a_needed_destruction_ptr *destruction_list,
                             a_dso_handle             dso_handle)
/*
Go through the specified destructions list and perform the required
destructions for the DSO indicated by dso_handle, or all destructions if
dso_handle is NULL.
*/
{
  a_needed_destruction_ptr *ndpp, ndp, old_head;
  
  ndpp = destruction_list;
  while (*ndpp != NULL) {
    ndp = *ndpp;
    /* Skip destructions that do not apply to this DSO. */
    if (dso_handle != NULL && ndp->dso_handle != dso_handle) {
      ndpp = &ndp->next;
      continue;
    }  /* if */
    /* Note that the value pointed to by destruction_list may change
       during the execution of the destructor.  Consequently, the
       current entry is removed from the list before the destructor
       routine is called. */
    *ndpp = ndp->next;
    old_head = *destruction_list;
    /* Call the routine. */
    (*ndp->destruction_routine)(ndp->object);
    /* Deallocate the entry. */
    free(ndp);
    /* If the head has changed, start at the beginning of the list 
       again so that we can process the newly added destruction. */
    if (*destruction_list != old_head) {
      ndpp = destruction_list;
    }  /* if */
  }  /* while */
}  /* __finalize_destructions */


int __add_destruction_to_list(a_needed_destruction_ptr *destruction_list,
                              a_cxa_dtor_ptr           destruction_routine,
                              void                     *object,
                              a_dso_handle             dso_handle)
/*
Register an action to be taken at a later time (program termination,
DSO unload, or thread termination) by queuing the action (a call
to destruction_routine with object as its argument) on the specified
destruction list.  Return zero if the registration is successful, or non-zero
otherwise.
*/
{
  int                      success = TRUE;
  a_needed_destruction_ptr ndp;

  ndp = (a_needed_destruction_ptr)malloc(sizeof(a_needed_destruction));
  if (ndp == NULL) {
    success = FALSE;
  }  else {
    ndp->object = object;
    ndp->destruction_routine = (a_destructor_ptr)destruction_routine;
    ndp->dso_handle = dso_handle;
    ndp->next = *destruction_list;
    *destruction_list = ndp;
  }  /* if */
  return !success;
}  /* __add_destruction_to_list */

#endif /* !SYSTEM_RUNTIME_HAS_IA64_ATEXIT || !SYSTEM_RUNTIME_HAS_IA64_... */
#if !SYSTEM_RUNTIME_HAS_IA64_ATEXIT

void ABI_NAMESPACE::__cxa_finalize(a_dso_handle dso_handle)
/*
Go through the needed destructions lists and perform the required
destructions for the DSO indicated by dso_handle, or all destructions if
dso_handle is NULL.
*/
{
#if RUNTIME_DOES_THREAD_LOCAL_DESTRUCTIONS
  /* Do thread_local destructions for the current thread first. */
  __finalize_destructions(&__thread_needed_destruction_head, dso_handle);
#endif /* RUNTIME_DOES_THREAD_LOCAL_DESTRUCTIONS */
  __finalize_destructions(&needed_destruction_head, dso_handle);
}  /* __cxa_finalize */


int ABI_NAMESPACE::__cxa_atexit(a_cxa_dtor_ptr   destruction_routine,
                                void             *object,
                                a_dso_handle     dso_handle)
/* 
Register an action to be taken at program termination (or DSO unload) time.
The action is the calling of destruction_routine with the object parameter.
If dso_handle is non-NULL, the action will be taken when __cxa_finalize
is called for the specific dso_handle value, or when __cxa_finalize
is called to process all objects (i.e., the __cxa_finally dso_handle has
a NULL value).  If dso_handle is NULL, the action will be taken only when
__cxa_finalize is called to process all objects.

This version of __cxa_atexit is only intended to be used when the
system's runtime does not include its own version of __cxa_atexit.  When
the system's runtime includes such a function, the system is responsible
for seeing that __cxa_finalize is invoked at program termination.  When
using our own version of this routine, we need to ensure that __cxa_finalize
is invoked at program termination.  This is accomplished by calling
__register_finalization_routine on the first invocation of this function.
Return zero if the registration is successful, or non-zero otherwise.
*/
{
  if (needed_destruction_head == NULL) __register_finalization_routine();
  return __add_destruction_to_list(&needed_destruction_head,
                                   destruction_routine,
                                   object, dso_handle);
}  /* __cxa_atexit */

#endif /* !SYSTEM_RUNTIME_HAS_IA64_ATEXIT */

#endif /* defined(__EDG_IA64_ABI) */

