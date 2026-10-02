/*
Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
Exceptions.
See https://edgcpp.org/LICENSE.txt for license information.
SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
*/

/*

func_def.h -- Declarations related to func_def.c (having to do with
              processing for function definitions).

*/

/* Avoid including these declarations more than once. */
#ifndef FUNC_DEF_H
#define FUNC_DEF_H 1

#ifndef DECLS_H
#include "decls.h"
#endif /* ifndef DECLS_H */

/* Conditionally open the "edg" namespace. */
BEGIN_EDG_NAMESPACE

/* Constants defining bits in the input bit vector used in calls to
   scan_function_body. */
#define SFB_NO_FLAGS (a_decl_flag_set)(0x0)
#define SFB_IMPLICITLY_DECLARED_RETURN_TYPE (a_decl_flag_set)(0x1)
			/* If this bit is set the return type was not
			   explicitly declared (and is "int" by default). */
#define SFB_NO_CLASS_REACTIVATION (a_decl_flag_set)(0x2)
			/* If this bit is set the scope for the parent
			   class of a member function has already been
			   reactivated. */
#define SFB_NEW_STRUCT_STMT_STACK_REQUIRED (a_decl_flag_set)(0x4)
			/* If this bit is set the function definition may be
			   within a statement context -- e.g., an inline
			   member function of a local class or an inline
			   template function being instantiated "on demand".
			   In such cases the structured statement stack should
			   be reinitialized, and then restored once the
			   function definition is complete. */
#define SFB_IS_INSTANTIATION (a_decl_flag_set)(0x8)
			/* If this bit is set the definition is being generated
			   by the compiler based on a template. */
#define SFB_PRAGMA_PACK_IS_LOCAL (a_decl_flag_set)(0x10)
			/* This bit means the function whose body is being
			   scanned is one in which a "#pragma pack" directive
			   has effect only within the function and does not
			   persist once the function body has terminated. */
#define SFB_INLINE_NAMESPACE_SPECIALIZATION (a_decl_flag_set)(0x20)
			/* This bit is set when scanning the body of an
			   explicit specialization of a function template or
			   member function of a class template in cases where
			   the function being explicitly specialized was
			   declared in an inline namespace. */

extern void adjust_member_routine_type(a_type_ptr	rout_type,
				       a_type_ptr	prev_type);

extern void scan_function_body(a_routine_ptr      rout_ptr,
                               a_func_info_block  *func_info,
                               a_decl_flag_set    flags);

extern void define_lambda_conversion_function(a_routine_ptr  conv_op);

extern a_boolean check_function_return_type(
                                     a_type_ptr         return_type,
                                     a_source_position  *err_pos,
                                     a_boolean          is_expr_use,
                                     a_boolean          evaluated,
                                     a_boolean          incomplete_return_okay,
                                     a_routine_ptr      rout_ptr);

extern void scan_defaulted_or_deleted_definition(
                                            a_decl_parse_state    *dps,
                                            a_func_info_block     *func_info);

extern void function_definition(a_symbol_locator      *locator,
                                a_decl_parse_state    *dps,
                                a_func_info_block     *func_info,
                                a_decl_pos_block_ptr  decl_pos_block);

extern void force_definition_of_compiler_generated_routine(a_routine_ptr rp);

extern void generate_required_virtual_destructor_bodies(a_scope_ptr  scope);

extern void require_definitions_of_virtual_functions_in_class(
							a_type_ptr class_type);

extern a_coroutine_descr_ptr get_coroutine_descr(a_routine_ptr rp);

/* Conditionally close the "edg" namespace. */
END_EDG_NAMESPACE

#endif /* FUNC_DEF_H */

