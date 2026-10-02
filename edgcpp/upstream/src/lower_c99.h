/*
Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
Exceptions.
See https://edgcpp.org/LICENSE.txt for license information.
SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
*/

/*

lower_c99.h -- Declarations related to lower_c99.c.

*/

/* Avoid including these declarations more than once. */
#ifndef LOWER_C99_H
#define LOWER_C99_H 1
#if DO_IL_LOWERING

#include "il.h"

/* Conditionally open the "edg" namespace. */
BEGIN_EDG_NAMESPACE

/* These macros are used for historical purposes. */
#define lower_any_c99_expr(expr)  lower_c99_expr(expr)

#define lower_any_cpp_expr(expr)  lower_expr(expr)

/*
Macro to lower an expression as either a C or C++ expression as appropriate.
*/
#define lower_any_expr(expr)                                                \
  if (C_mode()) {                                                           \
    lower_any_c99_expr(expr);                                               \
  } else {                                                                  \
    lower_any_cpp_expr(expr);                                               \
  }  /* if */

/*
Macro to lower an expression as either a C or C++ expression as appropriate.
Used in cases where it is possible to know whether or not the given expression
can result in a non-null value.
*/
#define lower_any_expr_full(expr, assume_expr_is_non_null)                  \
  if (C_mode()) {                                                           \
    lower_any_c99_expr(expr);                                               \
  } else {                                                                  \
    lower_expr_full(expr, assume_expr_is_non_null);                         \
  }  /* if */

/*
Macro to lower a boolean controlling expression as either a C or C++ expression
as appropriate.
*/
#define lower_any_boolean_controlling_expr(expr, is_full_expr)              \
  if (C_mode()) {                                                           \
    lower_c99_boolean_controlling_expr(expr, is_full_expr);                 \
  } else {                                                                  \
    lower_boolean_controlling_expr(expr, is_full_expr);                     \
  }  /* if */

extern void lower_runtime_sizeof(an_expr_node_ptr expr);

extern void lower_vla_dimension_expression(a_vla_dimension_ptr  vdp);

extern void lower_type_of_vla_cast_if_necessary(an_expr_node_ptr expr);

#if LOWER_VARIABLE_LENGTH_ARRAYS

extern void record_vla_component_types_for_lowering(a_type_ptr  tp);

extern void prepare_to_lower_variably_modified_typedef(a_type_ptr  type);

extern an_expr_node_ptr lower_vla_dimensions(a_type_ptr  tp);

extern void lower_vla_types(void);

extern void lower_vla_variable_types(a_variable_ptr variable_list);

extern void lower_vla_variable_types_in_scope(a_scope_ptr scope);

extern void lower_vla_decl(a_statement_ptr  stmt);

extern void lower_set_vla_size(a_statement_ptr  stmt);

extern void lower_vla_pointer_integer_arithmetic(an_expr_node_ptr  expr);

extern void lower_vla_pointer_difference(an_expr_node_ptr  expr);

extern void lower_vla_variable_lvalue(an_expr_node_ptr  expr);

extern void lower_vla_dealloc(an_expr_node_ptr  expr);

extern void lower_vla_operations_before_operands_are_lowered(
                                                        an_expr_node_ptr expr);
#else /* !LOWER_VARIABLE_LENGTH_ARRAYS */

extern void create_dimension_variable(a_statement_ptr  stmt);

extern void create_element_count_variable_for_vla(a_statement_ptr  stmt);

#endif /* LOWER_VARIABLE_LENGTH_ARRAYS */

extern an_expr_node_ptr vla_dimension_expr_for_type(a_type_ptr type);

#if LOWER_COMPLEX

extern void lower_c99_nonreal_float_types(void);

extern void lower_c99_complex_constant(a_constant_ptr  constant);

extern void lower_c99_complex_cast(an_expr_node_ptr  expr);

void lower_c99_xnegate(an_expr_node_ptr  expr);

void lower_c99_xadd(an_expr_node_ptr  expr);

void lower_c99_xsubtract(an_expr_node_ptr  expr);

void lower_c99_xmultiply(an_expr_node_ptr  expr);

void lower_c99_xdivide(an_expr_node_ptr  expr);

void lower_c99_xeq(an_expr_node_ptr  expr);

void lower_c99_xne(an_expr_node_ptr  expr);

void lower_c99_xincr_decr(an_expr_node_ptr expr);

#if C99_IL_EXTENSIONS_SUPPORTED

void lower_xconj(an_expr_node_ptr  expr);

void lower_complex_projection(an_expr_node_ptr  expr);

#endif /* C99_IL_EXTENSIONS_SUPPORTED */

extern void lower_c99_complex_aggregate_constant(a_constant_ptr constant);

extern a_type_ptr lowered_complex_type(a_float_kind fkind);

#endif /* LOWER_COMPLEX */
extern a_boolean c99_il_lowering_needed(void);

#if LOWER_FIXED_POINT
extern a_type_ptr lowered_integer_type_for_fixed_point_type(
                                                           a_type_ptr fx_type);
#endif /* LOWER_FIXED_POINT */

extern void lower_c99_cast(an_expr_node_ptr expr);

extern void lower_c99_constant(a_constant_ptr constant);

extern void lower_c99_constant_expr(an_expr_node_ptr expr);

extern void lower_c99_operator(an_expr_node_ptr expr);

extern void lower_c99_expr(an_expr_node_ptr expr);

extern void lower_c99_full_expr(an_expr_node_ptr expr);

extern void lower_c99_boolean_controlling_expr(an_expr_node_ptr expr,
                                               a_boolean        is_full_expr);

extern void lower_c99_il_memory_region(a_memory_region_number region_number);

extern void lower_c99_statement(a_statement_ptr statement);

extern void lower_c99_ne_0_if_needed(an_expr_node_ptr expr);

extern void lower_c99_one_time_init(void);

extern void lower_c99_trans_unit_init(void);

extern void lower_c99_init(void);

extern a_float_kind map_extended_float_kinds(a_float_kind fkind);

/* Conditionally close the "edg" namespace. */
END_EDG_NAMESPACE

#endif /* DO_IL_LOWERING */
#endif /* #ifndef LOWER_C99_H */

