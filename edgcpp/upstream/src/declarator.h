/*
Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
Exceptions.
See https://edgcpp.org/LICENSE.txt for license information.
SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
*/

/*

declarator.h -- Declarations related to declarator.c (having to with
                scanning of declarators).

*/

/* Avoid including these declarations more than once: */
#ifndef DECLARATOR_H
#define DECLARATOR_H 1

#ifndef DECLS_H
#include "decls.h"
#endif /* ifndef DECLS_H */

/* Conditionally open the "edg" namespace. */
BEGIN_EDG_NAMESPACE

/*
Macro that is TRUE if the current token is the start of a Microsoft
calling convention.
*/
#if MICROSOFT_EXTENSIONS_ALLOWED
#define is_microsoft_calling_convention(tok)                          \
  (ms_extensions &&                                                   \
   ((tok) == tok_cdecl ||                                             \
    (tok) == tok_fastcall ||                                          \
    (tok) == tok_stdcall ||                                           \
    (tok) == tok_thiscall ||                                          \
    (tok) == tok_vectorcall ||                                        \
    (tok) == tok_clrcall))
#else /* MICROSOFT_EXTENSIONS_ALLOWED */
/* When Microsoft keywords are not allowed simply return FALSE. */
#define is_microsoft_calling_convention(tok) /*lint --e(506)*/FALSE
#endif /* MICROSOFT_EXTENSIONS_ALLOWED */

/*
Macro that is TRUE if the current token is "near" or "far".
*/
#if NEAR_AND_FAR_ALLOWED
#define is_near_or_far() (curr_token == tok_near || curr_token == tok_far)
#else /* !NEAR_AND_FAR_ALLOWED */
#define is_near_or_far() /*lint --e(506)*/FALSE
#endif /* NEAR_AND_FAR_ALLOWED */

/*
Macro to test whether the current token is "near" or "far".  Includes an "||"
at the beginning.
*/
#if NEAR_AND_FAR_ALLOWED
#define or_is_near_or_far() || is_near_or_far()
#else /* !NEAR_AND_FAR_ALLOWED */
#define or_is_near_or_far() /* Nothing */
#endif /* NEAR_AND_FAR_ALLOWED */


/*
Macros to test whether the current token is one of the extension keywords that
can appear in a declarator in Microsoft mode and whether the current token is
a declarator operator for a C++/CLI handle ("^") or tracking reference ("%").
Includes a logical or ("||") at the beginning for convenient use in the macros
is_declarator_start and is_abstract_declarator_start.
*/
#if MICROSOFT_EXTENSIONS_ALLOWED
#define or_is_microsoft_declarator_keyword(tok) ||                     \
  (is_microsoft_calling_convention(tok) || (tok) == tok_based)
#define or_is_cli_declarator_operator(tok)                             \
  || (cli_or_cx_enabled &&                                             \
      ((tok) == tok_excl_or || (tok) == tok_caret_caret ||             \
       (tok) == tok_remainder))
#else /* !MICROSOFT_EXTENSIONS_ALLOWED */
#define or_is_microsoft_declarator_keyword(tok) /* Nothing */
#define or_is_cli_declarator_operator(tok) /* Nothing */
#endif /* MICROSOFT_EXTENSIONS_ALLOWED */

/*
Macro that is TRUE if the current token is the start of a real (as opposed
to "abstract") declarator.
*/
#define is_declarator_start()                                               \
  (curr_token == tok_identifier ?                                           \
     (C_mode() || !identifier_is_template_id()) :                           \
     (curr_token == tok_star || curr_token == tok_lparen                    \
      or_is_microsoft_declarator_keyword(curr_token) or_is_near_or_far() || \
      (C_dialect == C_dialect_cplusplus &&                                  \
       (curr_token == tok_ampersand ||                                      \
        (rvalue_references_enabled && curr_token == tok_and_and) ||         \
        (struct_bindings_enabled && curr_token == tok_lbracket)             \
        or_is_cli_declarator_operator(curr_token)                 ||        \
        curr_token == tok_operator))))


/*
Macro that is TRUE if the current identifier token is the start of a
pointer-to-member declarator (class-name :: *).  is_decl_qualified_name_start
calls is_generalized_identifier_start, which sets curr_token to
tok_ptr_to_member and returns FALSE if a pointer to member is found.
*/
#define is_ptr_to_member_declarator_start()				\
  (!is_decl_qualified_name_start() && curr_token == tok_ptr_to_member)

/*
Macro that is TRUE if the current token is the start of an abstract
declarator.
*/
#define is_abstract_declarator_start()                                   \
  (curr_token == tok_star || curr_token == tok_lbracket ||               \
   curr_token == tok_lparen                                              \
   or_is_microsoft_declarator_keyword(curr_token) or_is_near_or_far() || \
   (C_dialect == C_dialect_cplusplus &&                                  \
    (is_ptr_to_member_declarator_start() ||                              \
     curr_token == tok_ampersand ||                                      \
     (rvalue_references_enabled && curr_token == tok_and_and)            \
     or_is_cli_declarator_operator(curr_token))))

/*
Macro that is TRUE if the current token is the start of either an
abstract or real declarator.
*/
#define is_abstract_or_real_declarator_start()                           \
  (is_declarator_start() || curr_token == tok_lbracket                   \
   or_is_microsoft_declarator_keyword(curr_token) or_is_near_or_far() || \
   (C_dialect == C_dialect_cplusplus &&                                  \
    is_ptr_to_member_declarator_start()))


/* Constants defining bits in the input bit vector used in calls to
   declarator. */
#define DI_NO_INPUT_FLAGS ((a_decl_flag_set)0x0)
#define DI_REAL_DECLARATOR_ALLOWED ((a_decl_flag_set)0x1)
			/* If this bit is set the entity may be scanned as an
			   declarator (rather than an abstract declarator). */
#define DI_ABSTRACT_DECLARATOR_ALLOWED ((a_decl_flag_set)0x2)
			/* If this bit is set the entity may be scanned as an
			   abstract declarator. */
#define DI_QUALIFIED_NAME_ALLOWED ((a_decl_flag_set)0x4)
			/* If this bit is set, a qualified name is allowed
			   in the declarator (e.g., the flag is not set for a
			   formal parameter or a typedef declaration). */
#define DI_PARENTHESIZED_INITIALIZER_ALLOWED ((a_decl_flag_set)0x8)
			/* If this bit is set a declarator may be followed
			   by an initializer using the "(expr-list)"
			   notation (ARM 8.4). */
#define DI_NONSTATIC_MEMBER ((a_decl_flag_set)0x10)
			/* If this bit is set the declarator is for a class
			   member declared within a class definition without
			   a "static" type specifier.  If the name turns out
			   to be a function name, it will thus be that of
			   a nonstatic member function.  This is of importance
			   to function_declarator in creating the implicit
			   this param type entry for such functions. */
#define DI_IS_CONSTRUCTOR ((a_decl_flag_set)0x20)
			/* If this bit is set decl_specifiers has determined
			   that the declaration is that of a constructor. */
#define DI_DIMENSION_EXPRESSION_ALLOWED ((a_decl_flag_set)0x40)
			/* If this bit is set the first dimension of an array
			   declarator may be a nonconstant expression. */
#define DI_IS_TEMPLATE_DECLARATION ((a_decl_flag_set)0x80)
			/* If this bit is set declarator is called for a
			   declaration of a template function or a template
			   static data member. */
#define DI_IS_TYPEDEF_DECLARATION ((a_decl_flag_set)0x100)
			/* If this bit is set a storage class of "typedef" has
			   been encountered. */
#define DI_OPERATOR_NAME_ALLOWED ((a_decl_flag_set)0x200)
			/* If this bit is set an operator name (e.g.,
			   "operator+" or "operator int") is allowed as the
			   declarator identifier. */
#define DI_IS_FRIEND_DECL ((a_decl_flag_set)0x400)
			/* If this bit is set the declarator is part of a
			   friend declaration. */
#define DI_IS_PARAMETER_DECL ((a_decl_flag_set)0x800)
			/* If this bit is set the declarator is part of a
			   function parameter declaration. */
#define DI_IS_SPECIALIZATION ((a_decl_flag_set)0x1000)
			/* If this bit is set the declarator appears in a
			   template specialization.  When
			   DI_IS_TEMPLATE_DECLARATION is also set, the
			   specialization declares a template that is a
			   specialization of the original template; otherwise,
			   it is a full specialization. */
#define DI_IS_EXPLICIT_INSTANTIATION ((a_decl_flag_set)0x2000)
			/* If this bit is set the declaration is that of a
			   C++ explicit template instantiation directive. */
#define DI_VLA_ALLOWED ((a_decl_flag_set)0x4000)
			/* If this bit is set a VLA type is allowed. */
#define DI_VLA_ASTERISK_ALLOWED ((a_decl_flag_set)0x8000)
			/* If this bit is set "[*]" is allowed to specify
			   a VLA of unknown size in a function prototype. */
#define DI_NO_TYPE_SPECIFIERS ((a_decl_flag_set)0x10000)
			/* If this bit is set no type specifiers appeared
			   among the declaration specifiers. */
#define DI_IS_TEMPLATE_PARAM_DECL ((a_decl_flag_set)0x20000)
			/* If this bit is set the declaration is that of a
			   template parameter. */
#define DI_VARIABLY_MODIFIED_DECL_ALLOWED ((a_decl_flag_set)0x40000)
			/* If this bit is set and DI_VLA_ALLOWED is not, then
			   nonconstant bounds are only allowed if the final
			   type is not that of a VLA.  E.g., "(a[3])[n]" would
			   not be allowed, but "(*a)[n]" would be okay. */
#define DI_EXPLICIT_TEMPLATE_ARGS_ALLOWED ((a_decl_flag_set)0x80000)
			/* If this bit is set, an explicit template argument
			   list is allowed.  An explicit argument list might
			   also be allowed as the result of other flags in
			   some cases. */
#define DI_IS_DEDUCTION_GUIDE ((a_decl_flag_set)0x100000)
			/* If this bit is set the declaration is for a C++17
			   deduction guide. */
#define DI_IS_TEMPLATE_PARAM_PACK_EXPANSION  ((a_decl_flag_set)0x200000)
			/* This bit is set for a template nontype parameter
			   declaration in which the type is an expansion of
			   an enclosing template parameter. */
#define DI_LAST DI_IS_TEMPLATE_PARAM_PACK_EXPANSION
			/* Last bit in the bit vector that is in use. */
/* Constants defining bits in the output bit vector used in calls to
   declarator. */
#define DO_NO_OUTPUT_FLAGS ((a_decl_flag_set)0x0)
#define DO_PARENTHESIZED_INITIALIZER ((a_decl_flag_set)0x1)
			/* If this bit was set the declarator is followed by
			   a parenthesized initializer.  The left parenthesis
                           has been consumed by the call to "declarator". */
#define DO_REAL_DECLARATOR_SCANNED ((a_decl_flag_set)0x2)
			/* If this bit was set a name was scanned, indicating
			   a real, not abstract, declarator. */
#define DO_CFRONT_MEMBER_FUNCTION_TYPEDEF ((a_decl_flag_set)0x4)
			/* If this bit was set a nonstandard member-function
			   typedef declaration was seen; these are recognized
			   in cfront-compatibility mode only. */
#define DO_SCOPE_DEACTIVATION_REQUIRED ((a_decl_flag_set)0x8)
			/* If this bit was set a class scope was reactivated
			   or a namespace extension scope was pushed to handle
			   (respectively) a class or namespace member
			   declaration.  This flag lets the caller know that
			   the scope needs to be popped. */
#define DO_IS_CONSTRUCTOR ((a_decl_flag_set)0x10)
			/* This bit is set if a constructor declarator was
			   scanned.  It will certainly be set if the input
			   flag corresponding to DI_IS_CONSTRUCTOR was set,
			   but even when that is not the case -- presumably
			   because the declarator was parenthesized -- this
			   bit may become set.  When handling nested
			   declarators, this flag may be set for
			   nonconstructors in cases that are not legal in
			   strict mode; e.g., "struct X { int (*X)(); };". */
#define DO_IS_DESTRUCTOR ((a_decl_flag_set)0x20)
			/* This bit is set if a destructor declarator was
			   scanned.  It will certainly be set if the input
			   flag corresponding to DI_IS_DESTRUCTOR was set,
			   but even when that is not the case -- presumably
			   because the declarator was parenthesized -- this
			   bit may become set. */
#define DO_HAS_PTR_OR_REF_COMPONENT ((a_decl_flag_set)0x40)
			/* This bit is set if the declarator has a pointer or
			   reference component (including pointer-to-members).
			   E.g., it is set for "(*x[3])[4]" but not for
			   "y[3][4]"). */
#define DO_HAS_PTR_TO_MEMBER_COMPONENT ((a_decl_flag_set)0x80)
			/* This bit is set if a pointer-to-member declarator
			   was scanned.  DO_HAS_PTR_OR_REF_COMPONENT is always
			   set when this bit is set. */
#define DO_RPAREN_IN_NEW_DECLARATOR ((a_decl_flag_set)0x100)
			/* This bit is set in GNU C++ mode when a right
			   parenthesis has been seen inside a new-declarator
			   (which is only possible due to a GNU bug). */
#define DO_IS_STATIC_CONSTRUCTOR ((a_decl_flag_set)0x200)
			/* This bit is set if a C++/CLI static constructor
			   declarator was scanned.  It is FALSE when
			   DO_IS_CONSTRUCTOR is TRUE and vice versa. */
#define DO_IS_FINALIZER ((a_decl_flag_set)0x400)
			/* This bit is set if a finalizer declarator was
			   scanned.  It will certainly be set if the input
			   flag corresponding to DI_IS_FINALIZER was set,
			   but even when that is not the case -- presumably
			   because the declarator was parenthesized -- this
			   bit may become set. */
#define DO_LAST DO_IS_FINALIZER
			/* Last bit in the bit vector that is in use. */

/*
Structure used to represent the information about a calling convention
specifier.
*/
typedef struct a_call_conv_descr *a_call_conv_descr_ptr;
typedef struct a_call_conv_descr {
  a_calling_convention
		call_conv;
			/* The calling convention specified. */
  a_source_position
		position;
			/* The source position of the calling convention
			   specifier. */
} a_call_conv_descr;

/*
Clear a calling convention description.
*/
#define clear_call_conv_descr(call_conv_descr)                        \
  ((call_conv_descr)->call_conv = (a_calling_convention)cc_default)


extern void function_declarator(a_decl_parse_state  *state,
                                a_decl_flag_set     di_flags,
                                a_type_ptr          *new_type_ptr,
                                a_func_info_block   *func_info,
                                a_symbol_locator    *locator,
                                a_type_ptr          parent_type,
                                a_boolean           is_nonstatic_member,
                                a_boolean           is_constructor,
                                a_boolean           is_static_constructor,
                                a_boolean           is_destructor,
                                a_boolean           is_finalizer,
                                a_boolean           disallow_default_args,
                                a_boolean           disallow_exception_spec,
                                a_decl_pos_block    *decl_pos_block);

extern
void declarator(a_decl_flag_set             input_flags,
                a_decl_parse_state          *state,
                a_type_ptr                  member_parent_type,
                a_symbol_locator            *locator,
                a_func_info_block           *func_info,
                a_decl_pos_block_ptr        decl_pos_block);

extern
a_type_ptr pointer_declarator(
                      a_type_ptr            specifiers_type,
                      a_decl_parse_state    *state,
                      a_boolean   	    reference_allowed,
                      a_call_conv_descr_ptr left_calling_convention,
                      a_call_conv_descr_ptr unbound_calling_convention,
                      a_type_qualifier_set  *left_qualifiers,
                      a_type_qualifier_set  *unbound_qualifiers,
                      a_boolean             *ptr_to_member_scanned,
                      a_decl_pos_block_ptr  decl_pos_block);

extern
void array_declarator(a_decl_parse_state    *dps,
                      a_type_ptr            *new_type_ptr,
                      a_boolean             nonconstant_dimension_allowed,
                      a_boolean             vla_allowed,
                      a_boolean             vla_asterisk_allowed,
                      a_boolean             threads_dimension_allowed,
                      a_boolean             top_level_field_decl,
                      a_boolean             top_level_param_decl,
                      a_decl_pos_block_ptr  decl_pos_block);

extern a_boolean restrict_qualifier_is_allowed(a_type_ptr         type,
                                               a_source_position  *error_pos);

extern
a_boolean check_nullability_qualifiers(a_type_qualifier_set  nullability,
                                       a_type_ptr            type,
                                       a_source_position     *diag_pos);

extern a_boolean is_cfront_member_function_typedef(a_type_ptr   type_ptr,
                                                   a_type_ptr   *rout_type,
                                                   a_type_ptr   *class_type,
                                                   a_symbol_ptr *sym);

extern a_type_qualifier_set collect_type_qualifiers(
                                       a_decl_pos_block_ptr  decl_pos_block,
                                       a_upc_block_size      *upc_block_size);

#if GENERATE_SOURCE_SEQUENCE_LISTS
extern a_type_ptr form_declared_type(a_type_ptr             type_ptr,
                                     a_func_info_block_ptr  func_info);
#endif /* GENERATE_SOURCE_SEQUENCE_LISTS */

extern void add_to_derived_type_list(a_type_ptr          new_type_ptr,
                                     a_type_ptr          *derived_type,
                                     a_type_ptr          *bottom_derived_type,
                                     a_decl_parse_state  *dps,
                                     a_boolean           parameter_type);

extern void report_bad_return_type_qualifier(a_type_ptr          type,
                                             a_decl_parse_state  *dps,
                                             a_source_position   *diag_pos,
                                             a_boolean           *err);

extern a_boolean check_return_type(a_type_ptr          type,
                                   a_decl_parse_state  *dps,
                                   a_source_position   *diag_pos);

extern void resolve_pending_mapped_exc_spec(a_symbol_ptr                sym,
                                            an_exception_specification  *esp);

extern void delayed_scan_of_exception_spec(
                                       a_routine_ptr              rp,
                                       a_reusable_token_cache     tokens,
                                       an_exception_specification *esp = NULL);

#if MICROSOFT_EXTENSIONS_ALLOWED
extern a_boolean f_check_cli_or_cx_type_pointed_to(
                                                 a_type_ptr         tp,
                                                 a_boolean          is_ref,
                                                 a_boolean          is_handle,
                                                 a_source_position  *pos);

extern a_boolean check_param_array_type(a_param_type_ptr   ptp,
                                        a_source_position  *diag_pos);

#define check_cli_or_cx_type_pointed_to(tp, is_ref, is_handle, pos)          \
  (!cli_or_cx_enabled ||                                                     \
   f_check_cli_or_cx_type_pointed_to((tp), (is_ref), (is_handle), (pos)))
#else /* !MICROSOFT_EXTENSIONS_ALLOWED */
/*lint -esym(2666,check_cli_or_cx_type_pointed_to)*/
/*lint -emacro(506,check_cli_or_cx_type_pointed_to)*/
#define check_cli_or_cx_type_pointed_to(tp, is_ref, is_handle, pos) TRUE
#endif /* MICROSOFT_EXTENSIONS_ALLOWED */

extern
void diagnose_invalid_class_templ_arg_deduction(a_decl_parse_state  *dps);

extern void check_type_with_placeholder_specifier(a_decl_parse_state  *state);

extern
void report_incomplete_function_return_type(a_type_ptr         return_type,
                                            a_source_position  *pos,
                                            a_routine_ptr      rp);

extern a_symbol_header* sym_hdr_for_capture(a_lambda_capture  *lcp);

extern void scan_lambda_declarator(a_decl_parse_state  *dps,
                                   a_func_info_block   *func_info,
                                   a_tmpl_decl_state   *templ_state,
                                   a_decl_pos_block    *decl_pos_block);


extern
a_param_type_ptr scan_requires_expr_parameters(a_decl_parse_state  *dps);

extern void make_param_syms_invisible(a_boolean  is_invisible);

extern void declarator_one_time_init(void);

extern void declarator_init(void);

/* Conditionally close the "edg" namespace. */
END_EDG_NAMESPACE

#endif /* DECLARATOR_H */

