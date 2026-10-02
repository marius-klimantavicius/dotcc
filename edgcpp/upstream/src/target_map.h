/*
Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
Exceptions.
See https://edgcpp.org/LICENSE.txt for license information.
SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
*/

/*

target_map.h -- Mapping of target-specific configuration macros to global
                variables

*/

/*
It must be possible to include this file more than once, so it intentionally
does not have an include guard.
*/

/* Conditionally open the "edg" namespace. */
BEGIN_EDG_NAMESPACE

/* Define a shorthand notation for TARGET_CONFIGURATION. */
#define _TC TARGET_CONFIGURATION
/*lint -esym(755,_TC)*/

#if FIXED_POINT_ALLOWED
/*
The run-time information for fixed point types is stored in three
multi-dimensional arrays (targ_alignof_fixed_point, targ_sizeof_fixed_point,
and targ_fractional_bits_for_fixed_point).  Use these local macros to
index properly into those arrays.
*/
#define _SIGNED_ACCUM     [/*is_unsigned=*/0][(int)fpp_default][/*is_fract=*/0]
/*lint -esym(755,_SIGNED_ACCUM)*/
#define _SIGNED_FRACT     [/*is_unsigned=*/0][(int)fpp_default][/*is_fract=*/1]
/*lint -esym(755,_SIGNED_FRACT)*/
#define _SIGNED_LONG_ACCUM   [/*is_unsigned=*/0][(int)fpp_long][/*is_fract=*/0]
/*lint -esym(755,_SIGNED_LONG_ACCUM)*/
#define _SIGNED_LONG_FRACT   [/*is_unsigned=*/0][(int)fpp_long][/*is_fract=*/1]
/*lint -esym(755,_SIGNED_LONG_FRACT)*/
#define _SIGNED_SHORT_ACCUM [/*is_unsigned=*/0][(int)fpp_short][/*is_fract=*/0]
/*lint -esym(755,_SIGNED_SHORT_ACCUM)*/
#define _SIGNED_SHORT_FRACT [/*is_unsigned=*/0][(int)fpp_short][/*is_fract=*/1]
/*lint -esym(755,_SIGNED_SHORT_FRACT)*/
#define _UNSIGNED_ACCUM   [/*is_unsigned=*/1][(int)fpp_default][/*is_fract=*/0]
/*lint -esym(755,_UNSIGNED_ACCUM)*/
#define _UNSIGNED_FRACT   [/*is_unsigned=*/1][(int)fpp_default][/*is_fract=*/1]
/*lint -esym(755,_UNSIGNED_FRACT)*/
#define _UNSIGNED_LONG_ACCUM [/*is_unsigned=*/1][(int)fpp_long][/*is_fract=*/0]
/*lint -esym(755,_UNSIGNED_LONG_ACCUM)*/
#define _UNSIGNED_LONG_FRACT [/*is_unsigned=*/1][(int)fpp_long][/*is_fract=*/1]
/*lint -esym(755,_UNSIGNED_LONG_FRACT)*/
#define _UNSIGNED_SHORT_ACCUM \
                            [/*is_unsigned=*/1][(int)fpp_short][/*is_fract=*/0]
/*lint -esym(755,_UNSIGNED_SHORT_ACCUM)*/
#define _UNSIGNED_SHORT_FRACT \
                            [/*is_unsigned=*/1][(int)fpp_short][/*is_fract=*/1]
/*lint -esym(755,_UNSIGNED_SHORT_FRACT)*/
#endif /* FIXED_POINT_ALLOWED */

static void TARGET_MAP_ROUTINE_NAME(TARGET_CONFIGURATION)
/*
Generic function to invoke the specified TARGET_MAP_MACRO for each
configuration macro that makes up a target configuration.
TARGET_MAP_ROUTINE_NAME and TARGET_MAP_MACRO are defined by the caller.

The minimum criteria for a configuration macro to be included in this list are:

  - The corresponding configuration macro should not be used in any #if
    directives (other than for --dump_configuration purposes).
  - There must be a global variable that has a one-to-one correspondence with
    the configuration macro.
  - The global variable must not be used (other than set to a default value)
    before the target configuration has been set (in both the front end
    and any back ends when STANDALONE_UTILITY_PROGRAM is TRUE).
*/
{
  TARGET_MAP_MACRO(TARG_ALIGNOF_DOUBLE, targ_alignof_double, _TC)
#if NEAR_AND_FAR_ALLOWED
  TARGET_MAP_MACRO(TARG_ALIGNOF_FAR_POINTER, targ_alignof_far_pointer, _TC)
#endif /* NEAR_AND_FAR_ALLOWED */
  TARGET_MAP_MACRO(TARG_ALIGNOF_FLOAT, targ_alignof_float, _TC)
  TARGET_MAP_MACRO(TARG_ALIGNOF_FLOAT128, targ_alignof_float128, _TC)
  TARGET_MAP_MACRO(TARG_ALIGNOF_FLOAT80, targ_alignof_float80, _TC)
  TARGET_MAP_MACRO(TARG_ALIGNOF_INT, targ_alignof_int, _TC)
#if INT128_EXTENSIONS_ALLOWED
  TARGET_MAP_MACRO(TARG_ALIGNOF_INT128, targ_alignof_int128, _TC)
#endif /* INT128_EXTENSIONS_ALLOWED */
  TARGET_MAP_MACRO(TARG_ALIGNOF_LONG, targ_alignof_long, _TC)
  TARGET_MAP_MACRO(TARG_ALIGNOF_LONG_DOUBLE, targ_alignof_long_double, _TC)
#if LONG_LONG_ALLOWED
  TARGET_MAP_MACRO(TARG_ALIGNOF_LONG_LONG, targ_alignof_long_long, _TC)
#endif /* LONG_LONG_ALLOWED */
#if NEAR_AND_FAR_ALLOWED
  TARGET_MAP_MACRO(TARG_ALIGNOF_NEAR_POINTER, targ_alignof_near_pointer, _TC)
#endif /* NEAR_AND_FAR_ALLOWED */
  TARGET_MAP_MACRO(TARG_ALIGNOF_POINTER, targ_alignof_pointer, _TC)
  TARGET_MAP_MACRO(TARG_ALIGNOF_PTR_TO_DATA_MEMBER,
                   targ_alignof_ptr_to_data_member, _TC)
  TARGET_MAP_MACRO(TARG_ALIGNOF_PTR_TO_MEMBER_FUNCTION,
                   targ_alignof_ptr_to_member_function, _TC)
  TARGET_MAP_MACRO(TARG_MICROSOFT_PTR_TO_MEMBER_SIZING,
                   targ_microsoft_ptr_to_member_sizing, _TC)
#if !IA64_ABI
  TARGET_MAP_MACRO(TARG_ALIGNOF_PTR_TO_VIRTUAL_BASE_CLASS,
                   targ_alignof_ptr_to_virtual_base_class, _TC)
#endif /* !IA64_ABI */
  TARGET_MAP_MACRO(TARG_ALIGNOF_SHORT, targ_alignof_short, _TC)
#if FIXED_POINT_ALLOWED
  TARGET_MAP_MACRO(TARG_ALIGNOF_SIGNED_ACCUM,
                   targ_alignof_fixed_point _SIGNED_ACCUM, _TC)
  TARGET_MAP_MACRO(TARG_ALIGNOF_SIGNED_FRACT,
                   targ_alignof_fixed_point _SIGNED_FRACT, _TC)
  TARGET_MAP_MACRO(TARG_ALIGNOF_SIGNED_LONG_ACCUM,
                   targ_alignof_fixed_point _SIGNED_LONG_ACCUM, _TC)
  TARGET_MAP_MACRO(TARG_ALIGNOF_SIGNED_LONG_FRACT,
                   targ_alignof_fixed_point _SIGNED_LONG_FRACT, _TC)
  TARGET_MAP_MACRO(TARG_ALIGNOF_SIGNED_SHORT_ACCUM,
                   targ_alignof_fixed_point _SIGNED_SHORT_ACCUM, _TC)
  TARGET_MAP_MACRO(TARG_ALIGNOF_SIGNED_SHORT_FRACT,
                   targ_alignof_fixed_point _SIGNED_SHORT_FRACT, _TC)
  TARGET_MAP_MACRO(TARG_ALIGNOF_UNSIGNED_ACCUM,
                   targ_alignof_fixed_point _UNSIGNED_ACCUM, _TC)
  TARGET_MAP_MACRO(TARG_ALIGNOF_UNSIGNED_FRACT,
                   targ_alignof_fixed_point _UNSIGNED_FRACT, _TC)
  TARGET_MAP_MACRO(TARG_ALIGNOF_UNSIGNED_LONG_ACCUM,
                   targ_alignof_fixed_point _UNSIGNED_LONG_ACCUM, _TC)
  TARGET_MAP_MACRO(TARG_ALIGNOF_UNSIGNED_LONG_FRACT,
                   targ_alignof_fixed_point _UNSIGNED_LONG_FRACT, _TC)
  TARGET_MAP_MACRO(TARG_ALIGNOF_UNSIGNED_SHORT_ACCUM,
                   targ_alignof_fixed_point _UNSIGNED_SHORT_ACCUM, _TC)
  TARGET_MAP_MACRO(TARG_ALIGNOF_UNSIGNED_SHORT_FRACT,
                   targ_alignof_fixed_point _UNSIGNED_SHORT_FRACT, _TC)
#endif /* FIXED_POINT_ALLOWED */
  TARGET_MAP_MACRO(TARG_ALIGNOF_VIRTUAL_FUNCTION_INFO,
                   targ_alignof_virtual_function_info, _TC)
  TARGET_MAP_MACRO(TARG_ALL_POINTERS_SAME_SIZE,
                   targ_all_pointers_same_size, _TC)
  TARGET_MAP_MACRO(TARG_BIT_FIELD_AFFECTS_UNION_ALIGNMENT,
                   targ_bit_field_affects_union_alignment, _TC)
  TARGET_MAP_MACRO(TARG_BIT_FIELD_CONTAINER_SIZE,
                   targ_bit_field_container_size, _TC)
  TARGET_MAP_MACRO(TARG_BOOL_INT_KIND, targ_bool_int_kind, _TC)
  TARGET_MAP_MACRO(TARG_C_BOOL_INT_KIND, targ_c_bool_int_kind, _TC)
  TARGET_MAP_MACRO(TARG_CHAR16_T_INT_KIND, targ_char16_t_int_kind, _TC)
  TARGET_MAP_MACRO(TARG_CHAR32_T_INT_KIND, targ_char32_t_int_kind, _TC)
  TARGET_MAP_MACRO(TARG_CHAR_BIT, targ_char_bit, _TC)
  TARGET_MAP_MACRO(TARG_CHAR_CONSTANT_FIRST_CHAR_MOST_SIGNIFICANT,
                   targ_char_constant_first_char_most_significant, _TC)
  TARGET_MAP_MACRO(TARG_DBL_MANT_DIG, targ_dbl_mant_dig, _TC)
  TARGET_MAP_MACRO(TARG_DBL_MAX_EXP, targ_dbl_max_exp, _TC)
  TARGET_MAP_MACRO(TARG_DBL_MIN_EXP, targ_dbl_min_exp, _TC)
  TARGET_MAP_MACRO(TARG_DEFAULT_NEW_ALIGNMENT, targ_default_new_alignment, _TC)
#if DO_IL_LOWERING
  TARGET_MAP_MACRO(TARG_DELTA_INT_KIND, targ_delta_int_kind, _TC)
#endif /* DO_IL_LOWERING */
  TARGET_MAP_MACRO(TARG_DOUBLE_FIELD_ALIGNMENT,
                   targ_double_field_alignment, _TC)
  TARGET_MAP_MACRO(TARG_DUAL_ALIGNMENTS_FOR_BUILTIN_TYPES,
                   targ_dual_alignments_for_builtin_types, _TC)
  TARGET_MAP_MACRO(TARG_ENUM_BIT_FIELDS_ARE_ALWAYS_UNSIGNED,
                   targ_enum_bit_fields_are_always_unsigned, _TC)
  TARGET_MAP_MACRO(TARG_ENUM_TYPES_CAN_BE_SMALLER_THAN_INT,
                   targ_enum_types_can_be_smaller_than_int, _TC)
  TARGET_MAP_MACRO(TARG_FIELD_ALLOC_SEQUENCE_EQUALS_DECL_SEQUENCE,
                   targ_field_alloc_sequence_equals_decl_sequence, _TC)
  TARGET_MAP_MACRO(TARG_FLT_MANT_DIG, targ_flt_mant_dig, _TC)
  TARGET_MAP_MACRO(TARG_FLT_MAX_EXP, targ_flt_max_exp, _TC)
  TARGET_MAP_MACRO(TARG_FLT_MIN_EXP, targ_flt_min_exp, _TC)
  TARGET_MAP_MACRO(TARG_FLOAT_FIELD_ALIGNMENT, targ_float_field_alignment, _TC)
  TARGET_MAP_MACRO(TARG_FLOAT128_FIELD_ALIGNMENT,
                   targ_float128_field_alignment, _TC)
  TARGET_MAP_MACRO(TARG_FLOAT80_FIELD_ALIGNMENT, targ_float80_field_alignment,
                   _TC)
  TARGET_MAP_MACRO(TARG_FLT128_MANT_DIG, targ_flt128_mant_dig, _TC)
  TARGET_MAP_MACRO(TARG_FLT128_MAX_EXP, targ_flt128_max_exp, _TC)
  TARGET_MAP_MACRO(TARG_FLT128_MIN_EXP, targ_flt128_min_exp, _TC)
  TARGET_MAP_MACRO(TARG_FLT80_MANT_DIG, targ_flt80_mant_dig, _TC)
  TARGET_MAP_MACRO(TARG_FLT80_MAX_EXP, targ_flt80_max_exp, _TC)
  TARGET_MAP_MACRO(TARG_FLT80_MIN_EXP, targ_flt80_min_exp, _TC)
  TARGET_MAP_MACRO(TARG_FORCE_ONE_BIT_BIT_FIELD_TO_BE_UNSIGNED,
                   targ_force_one_bit_bit_field_to_be_unsigned, _TC)
#if FIXED_POINT_ALLOWED
  TARGET_MAP_MACRO(TARG_FRACTIONAL_BITS_FOR_SIGNED_ACCUM,
                   targ_fractional_bits_for_fixed_point _SIGNED_ACCUM, _TC)
  TARGET_MAP_MACRO(TARG_FRACTIONAL_BITS_FOR_SIGNED_FRACT,
                   targ_fractional_bits_for_fixed_point _SIGNED_FRACT, _TC)
  TARGET_MAP_MACRO(TARG_FRACTIONAL_BITS_FOR_SIGNED_LONG_ACCUM,
                   targ_fractional_bits_for_fixed_point _SIGNED_LONG_ACCUM,
                   _TC)
  TARGET_MAP_MACRO(TARG_FRACTIONAL_BITS_FOR_SIGNED_LONG_FRACT,
                   targ_fractional_bits_for_fixed_point _SIGNED_LONG_FRACT,
                   _TC)
  TARGET_MAP_MACRO(TARG_FRACTIONAL_BITS_FOR_SIGNED_SHORT_ACCUM,
                   targ_fractional_bits_for_fixed_point _SIGNED_SHORT_ACCUM,
                   _TC)
  TARGET_MAP_MACRO(TARG_FRACTIONAL_BITS_FOR_SIGNED_SHORT_FRACT,
                   targ_fractional_bits_for_fixed_point _SIGNED_SHORT_FRACT,
                   _TC)
  TARGET_MAP_MACRO(TARG_FRACTIONAL_BITS_FOR_UNSIGNED_ACCUM,
                   targ_fractional_bits_for_fixed_point _UNSIGNED_ACCUM, _TC)
  TARGET_MAP_MACRO(TARG_FRACTIONAL_BITS_FOR_UNSIGNED_FRACT,
                   targ_fractional_bits_for_fixed_point _UNSIGNED_FRACT, _TC)
  TARGET_MAP_MACRO(TARG_FRACTIONAL_BITS_FOR_UNSIGNED_LONG_ACCUM,
                   targ_fractional_bits_for_fixed_point _UNSIGNED_LONG_ACCUM,
                   _TC)
  TARGET_MAP_MACRO(TARG_FRACTIONAL_BITS_FOR_UNSIGNED_LONG_FRACT,
                   targ_fractional_bits_for_fixed_point _UNSIGNED_LONG_FRACT,
                   _TC)
  TARGET_MAP_MACRO(TARG_FRACTIONAL_BITS_FOR_UNSIGNED_SHORT_ACCUM,
                   targ_fractional_bits_for_fixed_point _UNSIGNED_SHORT_ACCUM,
                   _TC)
  TARGET_MAP_MACRO(TARG_FRACTIONAL_BITS_FOR_UNSIGNED_SHORT_FRACT,
                   targ_fractional_bits_for_fixed_point _UNSIGNED_SHORT_FRACT,
                   _TC)
#endif /* FIXED_POINT_ALLOWED */
  TARGET_MAP_MACRO(TARG_HAS_SIGNED_CHARS, targ_has_signed_chars, _TC)
  TARGET_MAP_MACRO(TARG_HOST_STRING_CHAR_BIT, targ_host_string_char_bit, _TC)
#if DO_IL_LOWERING && IA64_ABI
  TARGET_MAP_MACRO(TARG_IA64_ABI_USE_GUARD_ACQUIRE_RELEASE,
                   targ_ia64_abi_use_guard_acquire_release, _TC)
  TARGET_MAP_MACRO(TARG_IA64_ABI_USE_INT_STATIC_INIT_GUARD,
                   targ_ia64_abi_use_int_static_init_guard, _TC)
  TARGET_MAP_MACRO(TARG_IA64_ABI_USE_VARIANT_ARRAY_COOKIES,
                   targ_ia64_abi_use_variant_array_cookies, _TC)
  TARGET_MAP_MACRO(TARG_IA64_ABI_USE_VARIANT_PTR_TO_MEMBER_FUNCTION_REPR,
                   targ_ia64_abi_use_variant_ptr_to_member_function_repr, _TC)
  TARGET_MAP_MACRO(TARG_IA64_ABI_VARIANT_CTORS_AND_DTORS_RETURN_THIS,
                   targ_ia64_abi_variant_ctors_and_dtors_return_this, _TC)
  TARGET_MAP_MACRO(TARG_IA64_ABI_VARIANT_KEY_FUNCTION,
                   targ_ia64_abi_variant_key_function, _TC)
  TARGET_MAP_MACRO(TARG_IA64_VTABLE_ENTRY_INT_KIND,
                   targ_ia64_vtable_entry_int_kind, _TC)
#endif /* DO_IL_LOWERING && IA64_ABI */
  TARGET_MAP_MACRO(TARG_INT_FIELD_ALIGNMENT, targ_int_field_alignment, _TC)
#if INT128_EXTENSIONS_ALLOWED
  TARGET_MAP_MACRO(TARG_INT128_FIELD_ALIGNMENT,
                   targ_int128_field_alignment, _TC)
#endif /* INT128_EXTENSIONS_ALLOWED */
#if DO_IL_LOWERING && DO_FULL_PORTABLE_EH_LOWERING
  TARGET_MAP_MACRO(TARG_JMP_BUF_ELEMENTS_ARE_FLOAT,
                   targ_jmp_buf_elements_are_float, _TC)
  TARGET_MAP_MACRO(TARG_JMP_BUF_ELEMENT_FLOAT_KIND,
                   targ_jmp_buf_element_float_kind, _TC)
  TARGET_MAP_MACRO(TARG_JMP_BUF_ELEMENT_INT_KIND,
                   targ_jmp_buf_element_int_kind, _TC)
  TARGET_MAP_MACRO(TARG_JMP_BUF_NUM_ELEMENTS,
                   targ_jmp_buf_num_elements, _TC)
  TARGET_MAP_MACRO(TARG_SETJMP_FUNC, targ_setjmp_func, _TC)
#endif /* DO_IL_LOWERING && DO_FULL_PORTABLE_EH_LOWERING */
  TARGET_MAP_MACRO(TARG_LDBL_MANT_DIG, targ_ldbl_mant_dig, _TC)
  TARGET_MAP_MACRO(TARG_LDBL_MAX_EXP, targ_ldbl_max_exp, _TC)
  TARGET_MAP_MACRO(TARG_LDBL_MIN_EXP, targ_ldbl_min_exp, _TC)
#if GNU_EXTENSIONS_ALLOWED
  TARGET_MAP_MACRO(TARG_LIBGCC_CMP_RETURN_MODE,
                   targ_libgcc_cmp_return_mode, _TC)
  TARGET_MAP_MACRO(TARG_LIBGCC_SHIFT_COUNT_MODE,
                   targ_libgcc_shift_count_mode, _TC)
#endif /* GNU_EXTENSIONS_ALLOWED */
  TARGET_MAP_MACRO(TARG_LITTLE_ENDIAN, targ_little_endian, _TC)
  TARGET_MAP_MACRO(TARG_LONG_DOUBLE_FIELD_ALIGNMENT,
                   targ_long_double_field_alignment, _TC)
  TARGET_MAP_MACRO(TARG_LONG_FIELD_ALIGNMENT, targ_long_field_alignment, _TC)
#if LONG_LONG_ALLOWED
  TARGET_MAP_MACRO(TARG_LONG_LONG_FIELD_ALIGNMENT,
                   targ_long_long_field_alignment, _TC)
#endif /* LONG_LONG_ALLOWED */
  TARGET_MAP_MACRO(TARG_MAXIMUM_INTRINSIC_ALIGNMENT,
                   targ_maximum_intrinsic_alignment, _TC)
  TARGET_MAP_MACRO(TARG_MAX_BASE_CLASS_OFFSET, targ_max_base_class_offset, _TC)
  TARGET_MAP_MACRO(TARG_MAX_CLASS_OBJECT_SIZE, targ_max_class_object_size, _TC)
  TARGET_MAP_MACRO(TARG_MICROSOFT_BIT_FIELD_ALLOCATION,
                   targ_microsoft_bit_field_allocation, _TC)
  TARGET_MAP_MACRO(TARG_MINIMUM_STRUCT_ALIGNMENT,
                   targ_minimum_struct_alignment, _TC)
  TARGET_MAP_MACRO(TARG_NONNEGATIVE_ENUM_BIT_FIELD_IS_UNSIGNED,
                   targ_nonnegative_enum_bit_field_is_unsigned, _TC)
  TARGET_MAP_MACRO(TARG_OPTIMIZE_EMPTY_BASE_CLASS_LAYOUT,
                   targ_optimize_empty_base_class_layout, _TC)
  TARGET_MAP_MACRO(TARG_PAD_BIT_FIELDS_LARGER_THAN_BASE_TYPE,
                   targ_pad_bit_fields_larger_than_base_type, _TC)
  TARGET_MAP_MACRO(TARG_PLAIN_INT_BIT_FIELD_IS_UNSIGNED,
                   targ_plain_int_bit_field_is_unsigned, _TC)
#if GNU_EXTENSIONS_ALLOWED && TARG_ALL_POINTERS_SAME_SIZE
  TARGET_MAP_MACRO(TARG_POINTER_MODE, targ_pointer_mode, _TC)
#endif /* GNU_EXTENSIONS_ALLOWED && TARG_ALL_POINTERS_SAME_SIZE */
  TARGET_MAP_MACRO(TARG_PTRDIFF_T_INT_KIND,
                   targ_ptrdiff_t_int_kind, _TC)
#if DO_IL_LOWERING && GENERATE_EH_TABLES
  TARGET_MAP_MACRO(TARG_REGION_NUMBER_INT_KIND,
                   targ_region_number_int_kind, _TC)
  TARGET_MAP_MACRO(TARG_ETS_FLAG_TYPE_INT_KIND,
                   targ_ets_flag_type_int_kind, _TC)
#endif /* DO_IL_LOWERING && GENERATE_EH_TABLES */
#if IA64_ABI
  TARGET_MAP_MACRO(TARG_REUSE_TAIL_PADDING, targ_reuse_tail_padding, _TC)
#endif /* IA64_ABI */
  TARGET_MAP_MACRO(TARG_RIGHT_SHIFT_IS_ARITHMETIC,
                   targ_right_shift_is_arithmetic, _TC)
#if !IA64_ABI
  TARGET_MAP_MACRO(TARG_RUNTIME_ELEM_COUNT_INT_KIND,
                   targ_runtime_elem_count_int_kind, _TC)
#endif /* !IA64_ABI */
  TARGET_MAP_MACRO(TARG_SHORT_FIELD_ALIGNMENT, targ_short_field_alignment, _TC)
  TARGET_MAP_MACRO(TARG_SIZEOF_DOUBLE, targ_sizeof_double, _TC)
#if NEAR_AND_FAR_ALLOWED
  TARGET_MAP_MACRO(TARG_SIZEOF_FAR_POINTER, targ_sizeof_far_pointer, _TC)
#endif /* NEAR_AND_FAR_ALLOWED */
  TARGET_MAP_MACRO(TARG_SIZEOF_FLOAT, targ_sizeof_float, _TC)
  TARGET_MAP_MACRO(TARG_SIZEOF_FLOAT128, targ_sizeof_float128, _TC)
  TARGET_MAP_MACRO(TARG_SIZEOF_FLOAT80, targ_sizeof_float80, _TC)
  TARGET_MAP_MACRO(TARG_SIZEOF_INT, targ_sizeof_int, _TC)
#if INT128_EXTENSIONS_ALLOWED
  TARGET_MAP_MACRO(TARG_SIZEOF_INT128, targ_sizeof_int128, _TC)
#endif /* INT128_EXTENSIONS_ALLOWED */
#if FIXED_POINT_ALLOWED
  TARGET_MAP_MACRO(TARG_SIZEOF_LARGEST_FIXED_POINT,
                   targ_sizeof_largest_fixed_point, _TC)
#endif /* FIXED_POINT_ALLOWED */
  TARGET_MAP_MACRO(TARG_SIZEOF_LONG, targ_sizeof_long, _TC)
  TARGET_MAP_MACRO(TARG_SIZEOF_LONG_DOUBLE, targ_sizeof_long_double, _TC)
#if LONG_LONG_ALLOWED
  TARGET_MAP_MACRO(TARG_SIZEOF_LONG_LONG, targ_sizeof_long_long, _TC)
#endif /* LONG_LONG_ALLOWED */
#if GNU_EXTENSIONS_ALLOWED
  TARGET_MAP_MACRO(TARG_SIZEOF_LARGEST_ATOMIC, targ_sizeof_largest_atomic, _TC)
#endif /* GNU_EXTENSIONS_ALLOWED */
#if NEAR_AND_FAR_ALLOWED
  TARGET_MAP_MACRO(TARG_SIZEOF_NEAR_POINTER, targ_sizeof_near_pointer, _TC)
#endif /* NEAR_AND_FAR_ALLOWED */
  TARGET_MAP_MACRO(TARG_SIZEOF_POINTER, targ_sizeof_pointer, _TC)
  TARGET_MAP_MACRO(TARG_SIZEOF_PTR_TO_DATA_MEMBER,
                   targ_sizeof_ptr_to_data_member, _TC)
  TARGET_MAP_MACRO(TARG_SIZEOF_PTR_TO_MEMBER_FUNCTION,
                   targ_sizeof_ptr_to_member_function, _TC)
#if !IA64_ABI
  TARGET_MAP_MACRO(TARG_SIZEOF_PTR_TO_VIRTUAL_BASE_CLASS,
                   targ_sizeof_ptr_to_virtual_base_class, _TC)
#endif /* !IA64_ABI */
  TARGET_MAP_MACRO(TARG_SIZEOF_SHORT, targ_sizeof_short, _TC)
#if FIXED_POINT_ALLOWED
  TARGET_MAP_MACRO(TARG_SIZEOF_SIGNED_ACCUM,
                   targ_sizeof_fixed_point _SIGNED_ACCUM, _TC)
  TARGET_MAP_MACRO(TARG_SIZEOF_SIGNED_FRACT,
                   targ_sizeof_fixed_point _SIGNED_FRACT, _TC)
  TARGET_MAP_MACRO(TARG_SIZEOF_SIGNED_LONG_ACCUM,
                   targ_sizeof_fixed_point _SIGNED_LONG_ACCUM, _TC)
  TARGET_MAP_MACRO(TARG_SIZEOF_SIGNED_LONG_FRACT,
                   targ_sizeof_fixed_point _SIGNED_LONG_FRACT, _TC)
  TARGET_MAP_MACRO(TARG_SIZEOF_SIGNED_SHORT_ACCUM,
                   targ_sizeof_fixed_point _SIGNED_SHORT_ACCUM, _TC)
  TARGET_MAP_MACRO(TARG_SIZEOF_SIGNED_SHORT_FRACT,
                   targ_sizeof_fixed_point _SIGNED_SHORT_FRACT, _TC)
  TARGET_MAP_MACRO(TARG_SIZEOF_UNSIGNED_ACCUM,
                   targ_sizeof_fixed_point _UNSIGNED_ACCUM, _TC)
  TARGET_MAP_MACRO(TARG_SIZEOF_UNSIGNED_FRACT,
                   targ_sizeof_fixed_point _UNSIGNED_FRACT, _TC)
  TARGET_MAP_MACRO(TARG_SIZEOF_UNSIGNED_LONG_ACCUM,
                   targ_sizeof_fixed_point _UNSIGNED_LONG_ACCUM, _TC)
  TARGET_MAP_MACRO(TARG_SIZEOF_UNSIGNED_LONG_FRACT,
                   targ_sizeof_fixed_point _UNSIGNED_LONG_FRACT, _TC)
  TARGET_MAP_MACRO(TARG_SIZEOF_UNSIGNED_SHORT_ACCUM,
                   targ_sizeof_fixed_point _UNSIGNED_SHORT_ACCUM, _TC)
  TARGET_MAP_MACRO(TARG_SIZEOF_UNSIGNED_SHORT_FRACT,
                   targ_sizeof_fixed_point _UNSIGNED_SHORT_FRACT, _TC)
#endif /* FIXED_POINT_ALLOWED */
  TARGET_MAP_MACRO(TARG_SIZEOF_VIRTUAL_FUNCTION_INFO,
                   targ_sizeof_virtual_function_info, _TC)
  TARGET_MAP_MACRO(TARG_SIZE_T_INT_KIND, targ_size_t_int_kind, _TC)
  TARGET_MAP_MACRO(TARG_SIZE_T_MAX, targ_size_t_max, _TC)
#if GNU_EXTENSIONS_ALLOWED
  TARGET_MAP_MACRO(TARG_SSIZE_T_INT_KIND, targ_ssize_t_int_kind, _TC)
#endif /* GNU_EXTENSIONS_ALLOWED */
  TARGET_MAP_MACRO(TARG_SUPPORTS_ARM32, targ_supports_arm32, _TC)
  TARGET_MAP_MACRO(TARG_SUPPORTS_ARM64, targ_supports_arm64, _TC)
  TARGET_MAP_MACRO(TARG_SUPPORTS_RISCV32, targ_supports_riscv32, _TC)
  TARGET_MAP_MACRO(TARG_SUPPORTS_RISCV64, targ_supports_riscv64, _TC)
  TARGET_MAP_MACRO(TARG_SUPPORTS_X86_64, targ_supports_x86_64, _TC)
  TARGET_MAP_MACRO(TARG_TOO_LARGE_SHIFT_COUNT_IS_TAKEN_MODULO_SIZE,
                   targ_too_large_shift_count_is_taken_modulo_size, _TC)
  TARGET_MAP_MACRO(TARG_UNNAMED_BIT_FIELD_AFFECTS_STRUCT_ALIGNMENT,
                   targ_unnamed_bit_field_affects_struct_alignment, _TC)
#if GNU_EXTENSIONS_ALLOWED
  TARGET_MAP_MACRO(TARG_UNWIND_WORD_MODE, targ_unwind_word_mode, _TC)
#endif /* GNU_EXTENSIONS_ALLOWED */
  TARGET_MAP_MACRO(TARG_USER_CONTROL_OF_STRUCT_PACKING_AFFECTS_BASE_CLASSES,
                   packing_applies_to_base_classes, _TC)
  TARGET_MAP_MACRO(TARG_USER_CONTROL_OF_STRUCT_PACKING_AFFECTS_BIT_FIELDS,
                   targ_user_control_of_struct_packing_affects_bit_fields, _TC)
#if DO_IL_LOWERING && GENERATE_EH_TABLES
  TARGET_MAP_MACRO(TARG_VAR_HANDLE_INT_KIND, targ_var_handle_int_kind, _TC)
#endif /* DO_IL_LOWERING && GENERATE_EH_TABLES */
#if DO_IL_LOWERING
  TARGET_MAP_MACRO(TARG_VIRTUAL_FUNCTION_INDEX_INT_KIND,
                   targ_virtual_function_index_int_kind, _TC)
#endif /* DO_IL_LOWERING */
  TARGET_MAP_MACRO(TARG_WCHAR_T_INT_KIND, targ_wchar_t_int_kind, _TC)
  TARGET_MAP_MACRO(TARG_WINT_T_INT_KIND, targ_wint_t_int_kind, _TC)
#if GNU_EXTENSIONS_ALLOWED
  TARGET_MAP_MACRO(TARG_WORD_MODE, targ_word_mode, _TC)
#endif /* GNU_EXTENSIONS_ALLOWED */
  TARGET_MAP_MACRO(TARG_ZERO_WIDTH_BIT_FIELD_AFFECTS_STRUCT_ALIGNMENT,
                   targ_zero_width_bit_field_affects_struct_alignment, _TC)
  TARGET_MAP_MACRO(TARG_ZERO_WIDTH_BIT_FIELD_ALIGNMENT,
                   targ_zero_width_bit_field_alignment, _TC)
}  /* TARGET_MAP_ROUTINE_NAME */

/* #undef the local macros defined above. */
#undef _TC
#if FIXED_POINT_ALLOWED
#undef _SIGNED_ACCUM
#undef _SIGNED_FRACT
#undef _SIGNED_LONG_ACCUM
#undef _SIGNED_LONG_FRACT
#undef _SIGNED_SHORT_ACCUM
#undef _SIGNED_SHORT_FRACT
#undef _UNSIGNED_ACCUM
#undef _UNSIGNED_FRACT
#undef _UNSIGNED_LONG_ACCUM
#undef _UNSIGNED_LONG_FRACT
#undef _UNSIGNED_SHORT_ACCUM
#undef _UNSIGNED_SHORT_FRACT
#endif /* FIXED_POINT_ALLOWED */

/* #undef macros that were set by the #includer. */
#undef TARGET_MAP_ROUTINE_NAME
#undef TARGET_MAP_MACRO

/* Conditionally close the "edg" namespace. */
END_EDG_NAMESPACE

