/*
Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
Exceptions.
See https://edgcpp.org/LICENSE.txt for license information.
SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
*/

/*

fixed_pt.h -- Declarations for fixed_pt.c (having to do with manipulation of
              internal fixed-point quantities).

*/

/* Avoid including these declarations more than once: */
#ifndef FIXED_PT_H
#define FIXED_PT_H 1

/* Conditionally open the "edg" namespace. */
BEGIN_EDG_NAMESPACE

#define cmp_fixed_point_constants(cp1, cp2)  \
  cmp_integer_constants((cp1), (cp2))

extern a_fixed_point_type_descr *fxp_descr_for_constant(a_constant_ptr	cp);

extern void fxp_init_value(a_fixed_point_value  *value);

extern a_boolean fxp_value_is_zero(a_fixed_point_value  *value);

extern void fxp_string_to_fixed_point(a_fixed_point_type_descr  *fxp_descr,
                                      a_const_char              *str,
                                      a_fixed_point_value       *value,
                                      a_boolean                 *err);

extern void fxp_hex_string_to_fixed_point(a_fixed_point_type_descr  *fxp_descr,
                                          a_const_char              *str,
                                          a_fixed_point_value       *value,
                                          a_boolean                 *err,
                                          a_boolean                 *inexact);

extern
void conv_integer_to_fixed_point(a_constant_ptr		old_constant,
			         a_constant_ptr		new_constant,
			         an_error_code		*err_code,
			         an_error_severity	*err_severity);

extern void conv_fixed_point_to_fixed_point(
				a_constant_ptr		old_constant,
				a_constant_ptr		new_constant,
				an_error_code		*err_code,
				an_error_severity	*err_severity);

extern
void conv_fixed_point_to_integer(a_constant_ptr		old_constant,
			         a_constant_ptr		new_constant,
			         an_error_code		*err_code,
			         an_error_severity	*err_severity);

extern 
void conv_fixed_point_to_float(a_constant_ptr		old_constant,
			       a_constant_ptr		new_constant,
			       an_error_code		*err_code,
			       an_error_severity	*err_severity);

extern
void conv_float_to_fixed_point(a_constant_ptr		old_constant,
			       a_constant_ptr		new_constant,
			       an_error_code		*err_code,
			       an_error_severity	*err_severity);

extern a_number_buffer fxp_to_string(a_fixed_point_type_descr  *fxp_descr,
                                     a_fixed_point_value       *value);

extern
void fxp_shift(a_constant		*constant,
	       int			shift_count,
	       a_constant		*result,
	       a_boolean		shift_right,
	       a_boolean		*err);

extern
void fxp_add(a_constant		*constant_1,
	     a_constant		*constant_2,
	     a_constant		*result,
	     a_boolean		*did_not_fold,
	     a_boolean		*err);

extern
void fxp_subtract(a_constant	*constant_1,
		  a_constant	*constant_2,
		  a_constant	*result,
		  a_boolean	*did_not_fold,
		  a_boolean	*err);

extern
void fxp_multiply(a_constant	*constant_1,
		  a_constant	*constant_2,
		  a_constant	*result,
		  a_boolean	*did_not_fold,
		  a_boolean	*err);

extern
void fxp_divide(a_constant	*constant_1,
		a_constant	*constant_2,
		a_constant	*result,
		a_boolean	*did_not_fold,
		a_boolean	*err);

extern
void fxp_negate(a_fixed_point_value      *value,
                a_fixed_point_type_descr *fxp_descr,
                a_fixed_point_value      *result,
                a_fixed_point_type_descr *fxp_descr_result,
                a_boolean                *err);

extern
int fxp_compare(a_constant	*constant_1,
		a_constant	*constant_2);

extern a_hash_value fxp_hash(a_fixed_point_value *value);

extern a_targ_size_t non_fractional_bits_for_fixed_point(
                                          a_fixed_point_type_descr *fxp_descr);

/* Conditionally close the "edg" namespace. */
END_EDG_NAMESPACE

#endif /* ifndef FIXED_PT_H */

