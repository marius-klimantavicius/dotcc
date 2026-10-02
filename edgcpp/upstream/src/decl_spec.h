/*
Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
Exceptions.
See https://edgcpp.org/LICENSE.txt for license information.
SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
*/

/*

decl_spec.h -- Declarations related to decl_spec.c (having to with
               scanning of declaration specifiers).

*/

/* Avoid including these declarations more than once: */
#ifndef DECL_SPEC_H
#define DECL_SPEC_H 1

#ifndef DECLS_H
#include "decls.h"
#endif /* ifndef DECLS_H */

/* Conditionally open the "edg" namespace. */
BEGIN_EDG_NAMESPACE

typedef struct an_extended_decl_info_block {
  a_type_qualifier_set
		qualifiers;
  a_decl_modifiers_block
		decl_modifiers;
#if MICROSOFT_EXTENSIONS_ALLOWED
  an_inheritance_kind
		inheritance_kind;
  a_source_position
		inheritance_kind_pos;
#endif /* MICROSOFT_EXTENSIONS_ALLOWED */
} an_extended_decl_info_block;

#if MICROSOFT_EXTENSIONS_ALLOWED
#define clear_extended_decl_info_block(block)                           \
  { (block).qualifiers = TQ_NONE;                                       \
    clear_decl_modifiers_block(&((block).decl_modifiers));              \
    (block).inheritance_kind = (an_inheritance_kind)ihk_none;           \
    (block).inheritance_kind_pos = null_source_position;                \
  }
#else /* !MICROSOFT_EXTENSIONS_ALLOWED */
#define clear_extended_decl_info_block(block)                           \
  { (block).qualifiers = TQ_NONE;					\
    clear_decl_modifiers_block(&((block).decl_modifiers));              \
  }
#endif /* MICROSOFT_EXTENSIONS_ALLOWED */

#if MICROSOFT_EXTENSIONS_ALLOWED || NEAR_AND_FAR_ALLOWED
extern void scan_extended_decl_modifiers(
                             an_extended_decl_info_block  *extended_decl_info,
                             an_attribute_ptr             *p_attr,
                             an_attribute_location        syn_loc,
                             a_boolean                    is_enum_decl);

extern void scan_and_discard_extended_decl_modifiers(void);
#endif /* MICROSOFT_EXTENSIONS_ALLOWED || NEAR_AND_FAR_ALLOWED */

#if MICROSOFT_EXTENSIONS_ALLOWED
extern void scan_and_record_event_interface_declaration(
                                                a_decl_parse_state *dps,
                                                a_type_ptr         class_type);

extern an_assembly_visibility scan_cli_visibility_specifier_if_any(
                                                     a_source_position  *pos);

extern void set_cli_visibility(a_type_ptr              type,
                               an_assembly_visibility  declared_visibility,
                               a_source_position_ptr   diag_pos,
                               a_boolean               is_definition);

extern void update_dll_info_for_class(a_type_ptr          class_type,
                                      a_decl_modifier_set flags,
                                      a_boolean           explicit_inst,
                                      a_boolean           adjust_template_base,
                                      a_source_position   *err_pos);

extern a_boolean record_uuid_for_class(a_type_ptr         class_type,
                                       a_const_char       *uuid_string,
                                       a_source_position  *err_pos);

extern a_hash_value hash_unresolved_type_map_key(a_void_ptr  key_ptr);

extern a_boolean compare_for_unresolved_type_map(a_void_ptr  type_ptr,
                                                 a_void_ptr  key_ptr);
#endif /* MICROSOFT_EXTENSIONS_ALLOWED */

#if DECL_MODIFIERS_IN_USE || NEAR_AND_FAR_ALLOWED
extern void update_extended_decl_info_for_class(
                            a_type_ptr                   class_type,
                            an_extended_decl_info_block  *extended_decl_info,
                            a_boolean                    explicit_inst,
                            a_source_position            *err_pos);
#endif /* DECL_MODIFIERS_IN_USE || NEAR_AND_FAR_ALLOWED */

#if MICROSOFT_EXTENSIONS_ALLOWED
extern a_boolean convert_GUID_string_literal(a_constant_ptr  strcon,
                                             char            **pstr);

extern char *scan_GUID_string(void);

extern void check_inheritance_kind(a_type_ptr           class_type,
                                   an_inheritance_kind  inheritance_kind,
                                   a_source_position    *err_pos);

extern void add_flags_from_dll_attributes(a_decl_modifier_set  *p_flags,
                                          an_attribute_ptr     ap);
#endif /* MICROSOFT_EXTENSIONS_ALLOWED */

extern a_boolean scan_enumerator_constant(a_constant_ptr constant,
                                          a_type_ptr     enum_type,
                                          a_boolean      *is_template_param,
                                          a_source_range *source_range);

extern void scan_enumerator_list(a_type_ptr           enum_type,
                                 a_decl_parse_state   *dps,
                                 a_decl_flag_set      dsi_flags,
                                 an_ms_attribute_ptr  *p_ms_attributes,
                                 a_type_ptr           class_of_which_a_member,
                                 a_boolean            *declares_something,
                                 a_decl_pos_block     *decl_pos_block);

extern void enum_specifier(a_decl_parse_state   *dps,
                           a_decl_flag_set      dsi_flags,
                           a_boolean            vacuous_decl_allowed,
                           a_boolean            is_enum_template_definition,
                           a_type_ptr           *type_ptr,
                           an_ms_attribute_ptr  *p_ms_attributes,
                           a_boolean            *declares_something,
                           a_boolean            *defines_something,
                           a_decl_pos_block     *decl_pos_block);

extern void typename_specifier(a_type_ptr            *type_ptr,
			       a_symbol_ptr	     *type_sym,
                               a_boolean             within_using_decl,
                               a_boolean             is_decl_specifier,
                               a_decl_parse_state    *dps,
                               a_decl_pos_block_ptr  decl_pos_block);

extern a_boolean is_constructor_decl(a_type_ptr          class_type,
                                     a_decl_parse_state  *dps);

extern void check_for_rescannable_alias(a_decl_parse_state  *dps);

extern void decl_specifiers(a_decl_flag_set             input_flags,
                            a_decl_parse_state          *state,
                            a_decl_pos_block_ptr        decl_pos_block);

#if MICROSOFT_EXTENSIONS_ALLOWED

extern a_type_ptr scan_unresolved_metadata_type(void);

extern void scan_microsoft_secondary_decl_specifiers(
                                 a_decl_flag_set            input_flags,
                                 a_decl_parse_state         *state,
                                 a_decl_pos_block_ptr       decl_pos_block);
#endif /* MICROSOFT_EXTENSIONS_ALLOWED */

extern void set_name_linkage_for_type(a_type_ptr  tp);

extern void update_membership_of_class(a_symbol_ptr       tag_sym,
                                       a_boolean          def_or_vacuous_decl,
                                       a_boolean          is_event_interface,
                                       a_scope_depth      decl_level,
                                       a_source_position  *diag_pos);

extern void attach_tag_attributes(an_attribute_ptr    attributes,
                                  a_type_ptr          type,
                                  a_decl_parse_state  *dps,
                                  a_boolean           is_definition,
                                  a_boolean           is_forward_decl,
                                  a_boolean           ignore_gnu_attributes);

extern void diagnose_std_attribute_on_explicit_instantiation(
                                                        an_attribute_ptr  ap);

/*
Macro that is TRUE when class modifiers (denoted by context-sensitive keywords
"final"/"sealed" or "abstract") are permitted.
*/
#define class_modifiers_allowed()                                           \
  (cpp11_mode ||                                                            \
   (!C_mode() &&                                                            \
    ((ms_extensions &&                                                      \
      (microsoft_version >= 1400 || cli_or_cx_enabled)) ||                  \
     (gnu_mode && gnu_version >= 40700))))
    
extern void check_for_class_modifiers(a_token_kind  *next_tok,
                                      a_token_kind  body_start,
                                      a_boolean     tag_name_first);

extern void scan_class_modifiers(a_type_kind  type_kind,
                                 a_boolean    *p_is_final,
                                 a_boolean    *p_is_abstract,
                                 a_boolean    *p_is_sealed);

extern void apply_class_modifiers(a_type_ptr  class_type,
                                  a_boolean   is_final,
                                  a_boolean   is_abstract,
                                  a_boolean   is_sealed);

extern a_type_ptr enclosing_class_type(void);

extern void cache_attributes(a_token_cache  *cache);

extern void decl_spec_init(void);

extern void decl_spec_one_time_init(void);

/* Constants defining bits in the input bit vector used in calls to
   decl_specifiers. */
#define DSI_NO_INPUT_FLAGS ((a_decl_flag_set)0x0)
#define DSI_STORAGE_CLASS_SPECIFIER_ALLOWED ((a_decl_flag_set)0x1)
			/* If this bit is set the declaration specifiers may
			   include a storage class keyword. */
#define DSI_TYPE_SPECIFIER_ALLOWED ((a_decl_flag_set)0x2)
			/* If this bit is set the declaration specifiers may
			   include a type specifier. */
#define DSI_IS_MEMBER_DECLARATION ((a_decl_flag_set)0x4)
			/* If this bit is set the declaration is that of a
			   class member, inside the class (this includes field
			   declarations in C mode). */
#define DSI_IS_PARAMETER ((a_decl_flag_set)0x8)
			/* If this bit is set the declaration specifiers are
			   part of the declaration of a parameter. */
#define DSI_EMPTY_DECL_SPECIFIERS_ALLOWED ((a_decl_flag_set)0x10)
			/* If this bit is set suppress an error when an
			   non-type-name identifier is found before the first
			   specifier.  Simply set the default type and return
			   a flag signaling that there are no specifiers. */
#define DSI_INLINE_ALLOWED ((a_decl_flag_set)0x20)
			/* If this bit is set allow an inline specifier. */
#define DSI_IS_NEW_TYPE_NAME ((a_decl_flag_set)0x40)
			/* If this bit is set the declaration specifiers are
			   part of the type declaration associated with a
			   "new" operator. */
#define DSI_VACUOUS_TAG_DECL_ALLOWED ((a_decl_flag_set)0x80)
			/* If this bit is set a class, struct, union, or enum
			   declaration with no associated definition may be
			   interpreted as introducing a new tag name, not
			   referring to an existing one from outer scope. */
#define DSI_IS_TEMPLATE_PARAMETER ((a_decl_flag_set)0x100)
			/* If this bit is set decl_specifiers is called for
			   a template parameter declaration. */
#define DSI_IS_TEMPLATE_DECLARATION ((a_decl_flag_set)0x200)
			/* If this bit is set decl_specifiers is called for
			   a template class or template function
                           declaration. */
#define DSI_COLLECT_DECLARATOR_TYPE_QUALIFIERS ((a_decl_flag_set)0x400)
			/* If this bit is set decl_specifiers is called to
			   scan a list of type qualifiers in the
			   context of a pointer declarator.  When a token
			   other than a type qualifier is seen, return
			   immediately, without issuing any diagnostics. */
#define DSI_CHECK_FOR_DANGLING_TYPE_SPECIFIER ((a_decl_flag_set)0x800)
			/* If this bit is set decl_specifiers will do special
			   checking for a "dangling type specifier" -- an
			   identifier that may belong to a type specifier
			   of a subsequent declaration because a ";" is
			   missing. */
#define DSI_IS_OLD_STYLE_PARAM_DECL ((a_decl_flag_set)0x1000)
			/* If this bit is set decl_specifiers is being called
			   for an old-style parameter declaration.  Some error
			   checking is affected. */
#define DSI_ASM_ALLOWED ((a_decl_flag_set)0x2000)
			/* If this bit is set "asm" is recognized as a decl-
			   specifier.  Used only when ASM_FUNCTION_ALLOWED is
			   TRUE. */
#define DSI_IS_LINKAGE_SPEC_DECL ((a_decl_flag_set)0x4000)
			/* If this bit is set the declaration belongs to
			   a non-brace-enclosed linkage specification. */
#define DSI_IS_CONDITION_DECL ((a_decl_flag_set)0x8000)
			/* If this bit is set the declaration is that of a
			   C++ condition in an if, switch, for, or while
			   statement. */
#define DSI_IS_EXPLICIT_INSTANTIATION ((a_decl_flag_set)0x10000)
			/* If this bit is set the declaration is that of a
			   C++ explicit template instantiation directive. */
#define DSI_IS_SPECIALIZATION ((a_decl_flag_set)0x20000)
			/* If this bit is set the declaration is that of a
			   C++ template specialization. */
#define DSI_MARKED_AS_GNU_EXTENSION ((a_decl_flag_set)0x40000)
			/* If this bit is set the declaration was preceded by
			   the GNU keyword __extension__. */
#define DSI_NO_REAL_DECLARATOR ((a_decl_flag_set)0x80000)
			/* If this bit is set, the specifiers cannot be
			   followed by a real declarator (e.g., in a cast). */
#define DSI_MICROSOFT_SECONDARY_SPECIFIERS ((a_decl_flag_set)0x100000)
			/* If this bit is set, secondary specifiers (a
			   Microsoft extension/bug) are scanned. */
#define DSI_MICROSOFT_ATTRIBUTES_ALLOWED ((a_decl_flag_set)0x200000)
			/* If this bit is set, Microsoft attributes are valid
			   declaration specifiers. */
#define DSI_GNU_ATTRIBUTES_ALLOWED ((a_decl_flag_set)0x400000)
			/* If this bit is set, GNU attributes are valid
			   declaration specifiers. */
#define DSI_REGISTER_ID_ALLOWED ((a_decl_flag_set)0x800000)
			/* If this bit is set, Embedded C register names are
			   valid declaration specifiers. */
#define DSI_NO_TAG_DEFINITION ((a_decl_flag_set)0x1000000)
			/* If this bit is set, a tag definition is not allowed
			   and not considered.  For example, when parsing the
			   return type in the C++11 lambda "[]()->enum E {}"
			   decl_specifiers is called with the upcoming token
			   sequence "enum E {" and the "{" should not taken to
			   introduce an enum definition (instead, it is the
			   beginning of the lambda body). */
#define DSI_STD_ATTRIBUTES_ALLOWED ((a_decl_flag_set)0x8000000)
			/* If this bit is set, standard attributes are valid
			   declaration specifiers. */
#define DSI_LAST DSI_STD_ATTRIBUTES_ALLOWED
			/* Last bit in the bit vector that is in use. */

/* Constants defining bits in the output bit vector returned from
   decl_specifiers. */
#define DSO_NO_OUTPUT_FLAGS ((a_decl_flag_set)0x0)
#define DSO_HAS_EXPLICIT_TYPE_SPECIFIER	\
				((a_decl_flag_set)0x1)
			/* If this bit is set the declaration specifiers
			   were found to have at least one type specifier. */
#define DSO_INLINE 		((a_decl_flag_set)0x2)
			/* If this bit is set the function specifier "inline"
			   was found. */
#define DSO_VIRTUAL 		((a_decl_flag_set)0x4)
			/* If this bit is set the function specifier "virtual"
			   was found. */
#define DSO_FRIEND		((a_decl_flag_set)0x8)
			/* If this bit is set the declaration specifier
			   "friend" was found. */
#define DSO_DECLARES_SOMETHING	((a_decl_flag_set)0x10)
			/* If this bit is set the declaration specifiers
			   actually declare something (a tag or enumeration
			   members). */
#define DSO_DEFINES_SOMETHING	((a_decl_flag_set)0x20)
			/* If this bit is set the declaration specifiers
			   actually define something (a class, struct, union,
			   or enumeration). */
#define DSO_JUST_VOID 		((a_decl_flag_set)0x40)
			/* If this bit is set the keyword "void" was found,
			   and nothing else. */
#define DSO_DANGLING_TYPE_SPECIFIER	\
				((a_decl_flag_set)0x80)
			/* If this bit is set a malformed type specification
			   was detected, probably caused by a missing
			   semicolon following an class, struct, union, or
			   enum declaration.  Error reporting is left to the
			   caller in such cases. */
#define DSO_NO_DECL_SPECIFIERS	((a_decl_flag_set)0x100)
			/* If this bit is set then no declaration specifiers
			   were found before the first non-type-name
			   identifier was encountered. */
#define DSO_ELABORATED_TYPE_SPECIFIER	\
				((a_decl_flag_set)0x200)
                        /* If this bit is set the declaration specifiers
                           consist of (1) a keyword class, struct, union, or
                           enum and (2) an identifier (and optionally (3) the
                           keyword friend). */
#define DSO_CONSTRUCTOR 	((a_decl_flag_set)0x400)
			/* If this bit is set the declaration is for a
			   constructor, in which case the type returned from
			   decl_specifiers is tk_void. */
#define DSO_DESTRUCTOR 		((a_decl_flag_set)0x800)
			/* If this bit is set the declaration appears to be
                           that of a destructor (a "~" was seen, and the
                           specifiers, if any, are consistent with those
			   allowed on a destructor declaration), and so a type
                           of tk_void was returned. */
#define DSO_MUTABLE		((a_decl_flag_set)0x1000)
			/* If this bit is set the storage class "mutable" was
			   found. */
#define DSO_EXPLICIT		((a_decl_flag_set)0x2000)
			/* If this bit is set the specifier "explicit" was
			   found. */
#define DSO_LINKAGE_SPEC_DECL	((a_decl_flag_set)0x4000)
                        /* If this bit is set the decl-specifiers included a
                           linkage specifier; this is only accepted in
                           Microsoft mode (and only under restricted
                           circumstances). */
#if UPC_EXTENSIONS_ALLOWED
#define DSO_UPC_SHARED_LAYOUT	((a_decl_flag_set)0x8000)
			/* If this bit is set, a UPC "shared" specifier was
			   found with an explicit block size. */
#endif /* UPC_EXTENSIONS_ALLOWED */
#define DSO_TYPENAME		((a_decl_flag_set)0x10000)
			/* This bit is set if and only if the keyword typename
			   introduced an elaborated type specifier (i.e.,
			   DSO_ELABORATED_TYPE_SPECIFIER must also be set). */
#define DSO_STATIC_CONSTRUCTOR	((a_decl_flag_set)0x20000)
			/* If this bit is set the declaration is for a C++/CLI
			   static constructor, in which case the type returned
			   from decl_specifiers is tk_void.  This bit is
			   mutually exclusive w.r.t. DSO_CONSTRUCTOR. */
#define DSO_FINALIZER 		((a_decl_flag_set)0x40000)
                        /* If this bit is set the declaration appears to be
                           that of a C++/CLI finalizer (a "!" was seen, and
                           the specifiers, if any, are consistent with those
                           allowed on a finalizer declaration), and so a type
                           of tk_void was returned. */
#define DSO_CONSTEXPR 		((a_decl_flag_set)0x80000)
			/* If this bit is set the specifier "constexpr" was
			   found. */
#define DSO_CONSTEVAL 		((a_decl_flag_set)0x100000)
			/* If this bit is set the specifier "consteval" was
			   found. */
#define DSO_CONSTINIT 		((a_decl_flag_set)0x200000)
			/* If this bit is set the specifier "constinit" was
			   found. */
#define DSO_THREAD_LOCAL 	((a_decl_flag_set)0x400000)
			/* If this bit is set the storage class specifier
			   "local_thread" was found. */
#define DSO_LAST DSO_THREAD_LOCAL
			/* Last bit in the bit vector that is in use. */

#define DSO_STORAGE_CLASS_SPECIFIERS (DSO_MUTABLE | DSO_THREAD_LOCAL)
                        /* Set of bits that are lexically considered to
                           be storage-class-specifiers. */

inline void check_for_c23_deprecation(a_const_char  *old_string,
                                      an_error_code error_code)
/*
If we're in C23 mode and the current token matches old_string, give a
diagnostic indicating that the keyword (as spelled by old_string) is an
obsolescent feature (and that there's a new spelling).  Issue the diagnostic
only once and suppress any diagnostic altogether in system header files.  Note
that the use of a separate error_code (rather than the same error code with
fill-ins) is to allow each instance of this error to be issued only once.
*/
{
  if (c23_mode && locator_for_curr_id.symbol_header != NULL) {
    sizeof_t old_len = strlen(old_string);
    if (locator_for_curr_id.symbol_header->identifier_length == old_len &&
        strncmp(locator_for_curr_id.symbol_header->identifier, old_string,
                old_len) == 0 &&
        !seq_is_in_system_header(pos_curr_token.seq)) {
      /* Valid C17 code when compiled in C23 mode may see many of these
         diagnostics, so make this a remark except in strict mode where it's
         a warning. */
      pos_diagnostic(strict_ansi_mode ? es_warning : es_remark, error_code,
                     &pos_curr_token);
      (void)set_severity_for_error_number((int)error_code, es_once,
                                          /*make_default=*/FALSE);
    }  /* if */
  }  /* if */
}  /* check_for_c23_deprecation */


/* Conditionally close the "edg" namespace. */
END_EDG_NAMESPACE

#endif /* DECL_SPEC_H */

