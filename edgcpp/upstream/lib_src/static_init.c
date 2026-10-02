/*
Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
Exceptions.
See https://edgcpp.org/LICENSE.txt for license information.
SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
*/

/*

static_init.c -- called by _main to handle calling of static constructors
                 and destructors. 

*/

#include "basics.h"
#include "runtime.h"
#pragma hdrstop
#include "main.h"
#include "static_init.h"
#include "dtor_list.h"


/*
Indicates whether the executable is set up to use the "patch" method
of static initialization.  If this is FALSE then it is assumed that
the "munch" method is being used.
*/
static int use_patch_info = TRUE;


void __call_dtors()
/*
Call functions to perform static destruction of objects.

This routine deals with three different means of destroying static
objects.  The primary means is by use of a list of needed destructions
that is built as objects are constructed.  To allow "new" object
files that use the needed destruction list to be linked with
older object files, the "patch" and "munch" style destruction lists
are also supported.  If a file contains both a list of needed
destructions and a "patch" or "munch" list, the "patch"/"munch" list
is processed first, followed by the needed destruction list.  This
ensures that any new needed destructions created while processing
the "patch"/"munch" destructions will be handled.
*/
{
#ifndef __EDG_IA64_ABI
  struct __linkl	*link_ptr;
  static a_boolean	dtors_done = FALSE;

  if (!dtors_done) {
    dtors_done = TRUE;
    if (use_patch_info) {
      /* Walk through the linked list of constructor/destructor function
         pointers, which are now in reverse order, and call each termination
         routine, if defined.  The global variable __head points to the
         linked list. */
      for (link_ptr = __head; link_ptr != NULL; link_ptr = link_ptr->next) {
        if (link_ptr->dtor != NULL) {
          (*(link_ptr->dtor))();
        }  /* if */
      }  /* for */
    } else {
      /* Using munch information.  Call the destructors in the _dtors
         array in reverse order. */
      int   pos = 0;
      while (_dtors[pos]) pos++;
      while (pos--) (_dtors[pos])();
    }  /* if */
  }  /* if */
  /* Do the destructions specified by the needed destructions list.
     This is not done unconditionally (i.e., not based on dtors_done),
     because when using EH it is actually possible for exit to be
     called more than once, and there is no ill effect of calling
     __process_needed_destructions multiple times.  */
  __process_needed_destructions();
#else /* defined(__EDG_IA64_ABI) */
  ABI_NAMESPACE::__cxa_finalize((a_dso_handle)NULL);
#endif /* defined(__EDG_IA64_ABI) */
}  /* __call_dtors */


#if !defined(sun) && USE_ATEXIT == 0
/* Declare low-level routine or system call that exits a process. */
extern "C" void _exit(int status);


void exit(int status)
/*
This is used on systems that don't support a protocol such as the 
ANSI C "atexit" or the SunOS "on_exit" functions.  This version of exit
should be used instead of the normal OS exit routine.  Which means that
the library containing this routine needs to be linked in ahead of
the standard C library (such as libc.a).  Using this version of exit
means that some processing done by special versions of exit (such as the
profiling version) will not be done.
*/
{
  __call_dtors();
  _exit(status);
}  /* exit */
#endif /* !defined(sun) && USE_ATEXIT == 0 */


/*
The kind of function pointer accepted by atexit and on_exit.  Note
that the WP has a problem with the definition of atexit (as of 7/96),
because it is unspecified whether atexit has C or C++ linkage.  With
the recent change in which linkage is now part of a routine's type,
that means it is unspecified whether the argument passed to atexit
must have C or C++ linkage.
*/
extern "C" typedef void (*void_c_function_ptr)();

#if defined(sun) && USE_ATEXIT == 0
/* Used to register a function to be called by exit to do wrapup
   processing. */
extern "C" void on_exit(void_c_function_ptr, char *);
#endif /* defined(sun) && USE_ATEXIT == 0 */


void __register_finalization_routine(void)
/*
Register the function that handles static destruction so that it will be
called when the program exits.  May be called multiple times (and is
effectively a no-op on invocations after the first).
*/
{
  static a_boolean already_registered = FALSE;
  if (!already_registered) {
    already_registered = TRUE;
#if USE_ATEXIT
    atexit((void_c_function_ptr)__call_dtors);
#elif defined(sun)
    on_exit((void_c_function_ptr)__call_dtors, (char *)NULL);
#endif /* USE_ATEXIT */
  }  /* if */
}  /* __register_finalization_routine */


void __call_ctors()
/*
Call functions to perform static construction of objects.  This routine
first determines whether the executable has been processed using the
"patch" or "munch" utility and then uses the appropriate method to
call the static initializer functions.
*/
{
  struct __linkl	*link_ptr;
  struct __linkl	*reverse_ptr;
  struct __linkl	*next_ptr;

  /* Figure out whether we should use "patch" initialization or "munch"
     initialization.  "patch" uses a tool to link together a list of
     pointers to static constructors and destructors.  Munch generates
     an array of pointers to constructors and destructors.  The array
     is linked into the executable. */
  use_patch_info = (__head != NULL);
  if (use_patch_info) {
    /* Walk through the linked list of constructor/destructor function
       pointers and call each initialization (constructor) function.
       The list is pointed to by the global variable __head.  As each __linkl
       structure is visited, reverse the order of the linked list. */
    for (link_ptr = __head, reverse_ptr = NULL;
         link_ptr != NULL;
         link_ptr = next_ptr ) {

      /* Save the pointer to the next item on the list. */
      next_ptr = link_ptr->next;

      /* If the initialization routine pointer is non-NULL, call the 
         initialization function. */
      if (link_ptr->ctor != NULL) {
        (*(link_ptr->ctor))();
      }  /* if */

      /* Reverse the order of the linked list.  The first will now point 
         nowhere (NULL).  The second will point to the first, etc.. */
      link_ptr->next = reverse_ptr;
      reverse_ptr = link_ptr;
    }  /* for */

    /* Point to the new head of the list. */
    __head = reverse_ptr;
  } else {
    /* Using munch information.  Call the constructors in the _ctors
       array. */
    int   pos = 0;
    while (_ctors[pos]) (*_ctors[pos++])();
  }  /* if */
}  /* __call_ctors */

