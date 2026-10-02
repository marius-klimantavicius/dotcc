/*
Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
Exceptions.
See https://edgcpp.org/LICENSE.txt for license information.
SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
*/

/*

Declarations relating to error.c - runtime error handling.

*/

#ifndef ERROR_H
#define ERROR_H 1

enum an_error_code {
  ec_none,
  ec_abort_header,
  ec_terminate_called,
  ec_terminate_returned,
  ec_already_marked_for_destruction,
  ec_main_called_more_than_once,  /* No longer used. */
  ec_pure_virtual_called,
  ec_bad_cast,
  ec_bad_typeid,
  ec_array_not_from_vec_new,
  ec_terminate_called_more_than_once,
  ec_negative_vla_size,
  ec_vla_allocation_failed,
  ec_deleted_virtual_called,
  ec_thread_registration_failed,
  ec_last
};

EXTERN_C NORETURN void __abort_execution(an_error_code err_code);

#endif /* ERROR_H */

