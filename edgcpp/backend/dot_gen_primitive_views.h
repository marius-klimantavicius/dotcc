/*
Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
Exceptions. See https://edgcpp.org/LICENSE.txt for license information.
SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
*/
// Read-only backend primitive views. The stock C backend does not call frontend
// primitive factories. Those factories depend on private cross-TU caches that
// are intentionally absent from serialized IL. Views have stable map-node
// lifetime for one back_end invocation and never enter EDG's IL ownership lists.
std::map<an_integer_kind,a_type> source_primitive_views;
void reset_source_primitive_views() { source_primitive_views.clear(); }
a_type_ptr source_integer_view(an_integer_kind kind) {
  auto found=source_primitive_views.find(kind);
  if(found!=source_primitive_views.end())return &found->second;
  check_assertion(!is_bit_precise_kind(kind));
  a_type value={};value.kind=tk_integer;value.variant.integer.int_kind=kind;
  a_targ_alignment alignment;
  get_integer_size_and_alignment(kind,&value.size,&alignment);
  value.alignment=alignment;
  return &source_primitive_views.emplace(kind,value).first->second;
}

// Promotion decisions below are copied from the pinned stock EDG source:
// types.c::type_after_integral_promotion and exprutil.c's three node/bitfield
// promotion routines. Only the names and primitive factory are redirected.
// The bitfield-only prvalue conversion uses skip_typerefs: bitfield types are
// integral, so its immediate skip_typerefs already discards all cv-qualifiers.
// tests/il_replay_verify.py checks this adaptation against the stock bodies.

// types.c's local predicate (including the scoped-enum exclusion).
#define source_is_integer_or_unscoped_enum(tp) \
  ((tp)->kind == (a_type_kind)tk_integer && \
   !(tp)->variant.integer.is_scoped_enum)

static a_boolean source_is_bitfield_extract_node(an_expr_node_ptr node)
/*
Return TRUE if the given expression node is a bit-field extraction.
*/
{
  a_boolean is_bit_field_extract = FALSE;

  if (is_operation_node(node) &&
      (node_operator_is((node), eok_dot_field) ||
       node_operator_is((node), eok_points_to_field)) &&
      node_field((node)->variant.operation.operands->next)->is_bit_field) {
    is_bit_field_extract = TRUE;
  } else if (is_variable_node(node)) {
    a_variable_ptr  vp = node_variable(node);
    if (vp->init_kind == (an_init_kind)initk_binding) {
      is_bit_field_extract =
                        source_is_bitfield_extract_node(vp->initializer.bound_expr);
    }  /* if */
  }  /* if */
  return is_bit_field_extract;
}  /* source_is_bitfield_extract_node */

a_type_ptr source_type_after_integral_promotion(a_type_ptr type)
/*
Determine the type that would result from applying the integral promotions
to type.  Return the promoted type, which may be the same as the original
type.  See also source_bitfield_promoted_type for integral
promotions for bit field expressions.  Note that this routine expects to
receive an rvalue type.
*/
{
  a_type_ptr promoted_type = type;
  a_type_ptr unqual_type = skip_typerefs(type);

  db_enter(5, "source_type_after_integral_promotion");

  if (source_is_integer_or_unscoped_enum(unqual_type)) {
    an_integer_kind ikind = unqual_type->variant.integer.int_kind;
    if (is_bit_precise_kind(ikind)) {
      /* Bit-precise integer types are not subject to integral promotion. */
    } else if (unqual_type->variant.integer.bool_type) {
      /* bool always promotes to int. */
      promoted_type = source_integer_view((an_integer_kind)ik_int);
    } else if (!C_mode() &&
                (unqual_type->variant.integer.enum_type ||
                 unqual_type->variant.integer.wchar_t_type ||
                 unqual_type->variant.integer.char16_t_type ||
                 unqual_type->variant.integer.char32_t_type) &&
                targ_sizeof_int == targ_sizeof_long &&
               (ikind == (an_integer_kind)ik_long ||
                ikind == (an_integer_kind)ik_unsigned_long)) {
      /* In C++, enums, wchar_t, char16_t, and char32_t (when  keywords) go
         through the normal promotion processing for the underlying type.  If
         the type is unchanged, it will be converted to the corresponding plain
         integral type (see below).  However, there's one anomaly:
         if long and int are the same size, enums, wchar_t, char16_t, and
         char32_t of size long promote to int or unsigned int. */
      if (ikind == (an_integer_kind)ik_long) {
        promoted_type = source_integer_view((an_integer_kind)ik_int);
      } else { /* ikind == (an_integer_kind)ik_unsigned_long */
        promoted_type = source_integer_view((an_integer_kind)ik_unsigned_int);
      }  /* if */
    } else {
      switch (ikind) {
        case ik_char:
          if (targ_has_signed_chars &&
              !unqual_type->variant.integer.char8_t_type) goto do_signed_char;
          goto do_unsigned_char;
        case ik_unsigned_char:
do_unsigned_char:
          if (C_dialect == C_dialect_pcc) {
            /* In pcc mode, unsigned char is promoted to unsigned int. */
            promoted_type = source_integer_view((an_integer_kind)ik_unsigned_int);
          } else {
            /* In ANSI mode, unsigned char is promoted to int if all values
               of type unsigned char can be represented in an int; otherwise
               unsigned char is promoted to unsigned int. */
            if (targ_sizeof_int > 1) {
              /* All values of type unsigned char can fit in an int, so
                 unsigned char is promoted to int. */
              promoted_type = source_integer_view((an_integer_kind)ik_int);
            } else {
              /* int and char are the same size, so unsigned char is promoted
                 to unsigned int. */
              promoted_type = source_integer_view((an_integer_kind)ik_unsigned_int);
            }  /* if */
          }  /* if */
          break;
        case ik_signed_char:
do_signed_char:;
          FALLTHROUGH
        case ik_short:
          /* Signed char and signed short are promoted to int. */
          promoted_type = source_integer_view((an_integer_kind)ik_int);
	  break;
        case ik_unsigned_short:
          if (C_dialect == C_dialect_pcc) {
            /* In pcc mode, unsigned short is promoted to unsigned int. */
            promoted_type = source_integer_view((an_integer_kind)ik_unsigned_int);
          } else {
            /* In ANSI mode, unsigned short is promoted to int if all values
               of type unsigned short can be represented in an int; otherwise
               unsigned short is promoted to unsigned int. */
            if (targ_sizeof_int > targ_sizeof_short) {
              /* All values of type unsigned short can fit in an int, so
                 unsigned short is promoted to int. */
              promoted_type = source_integer_view((an_integer_kind)ik_int);
            } else {
              /* int and short are the same size, so unsigned short is promoted
                 to unsigned int. */
              promoted_type = source_integer_view((an_integer_kind)ik_unsigned_int);
            }  /* if */
          }  /* if */
          break;
        case ik_int:
#if MICROSOFT_EXTENSIONS_ALLOWED
          if (unqual_type->variant.integer.microsoft_sized_int_type) {
            /* __int32 promotes to int. */
            promoted_type = source_integer_view((an_integer_kind)ik_int);
          }  /* if */
#endif /* MICROSOFT_EXTENSIONS_ALLOWED */
          break;
        case ik_unsigned_int:
        case ik_long:
        case ik_unsigned_long:
#if LONG_LONG_ALLOWED
        case ik_long_long:
        case ik_unsigned_long_long:
#endif /* LONG_LONG_ALLOWED */
#if INT128_EXTENSIONS_ALLOWED
        case ik_int128:
        case ik_unsigned_int128:
#endif /* INT128_EXTENSIONS_ALLOWED */
          /* These are deliberately left as they are; they are not supposed
             to be promoted. */
          break;
        default:
          unexpected_condition_str(
                                "source_type_after_integral_promotion: bad int kind");
      }  /* switch */
      if (C_dialect == C_dialect_cplusplus) {
        /* enums, wchar_t, char16_t, and char32_t get promoted to the
           corresponding integral type (and lose their special properties) if
           they were not promoted above. */
        unqual_type = skip_typerefs(promoted_type);
        if (unqual_type->variant.integer.enum_type ||
            unqual_type->variant.integer.wchar_t_type ||
            unqual_type->variant.integer.char16_t_type ||
            unqual_type->variant.integer.char32_t_type) {
          /* Make a "plain" version of this type, i.e., the same underlying
             integral type but not tagged as an enum, wchar_t, char16_t, or
             char32_t. */
          promoted_type = source_integer_view(unqual_type->variant.integer.int_kind);
        }  /* if */
      }  /* if */
    }  /* if */
  }  /* if */

  db_exit();
  return promoted_type;
}  /* source_type_after_integral_promotion */

static
a_type_ptr source_bitfield_promoted_type(an_expr_node_ptr node)
/*
node is a bit-field selection operation (glvalue or prvalue).  Determine the
type that would result from applying the integral promotions to the selection
type.  Return the promoted type, which may be the same as the original type.
The node is not actually promoted; it is up to the caller to do the cast if
desired.
*/
{
  a_type_ptr       type, promoted_type;
  a_field_ptr      field;
  an_integer_kind  ikind, orig_ikind;
  unsigned int     field_size;
  an_expr_node_ptr field_node;

  db_enter(4, "source_bitfield_promoted_type");
  check_assertion(is_operation_node(node));
  field_node = node->variant.operation.operands->next;
  check_assertion(field_node != NULL &&
                  field_node->kind == (an_expr_node_kind)enk_field);
  field = node_field(field_node);
  field_size = field->bit_size;
  type = node->type;
  if (!is_glvalue_node(node)) type = skip_typerefs(type);
  promoted_type = skip_typerefs(type);
  if (is_template_param_type(promoted_type) ||
      is_scoped_enum_type(promoted_type) ||
      is_error_type(promoted_type)) {
    goto end_of_routine;
  }  /* if */
  /* The type of a bit-field should be integral. */
  check_assertion_str(
            type_is(promoted_type, tk_integer),
            "source_bitfield_promoted_type: bit-field not integral");
  orig_ikind = ikind = promoted_type->variant.integer.int_kind;
  if (is_bit_precise_kind(ikind)) {
    /* Bit-precise integer types are not subject to integral promotion. */
    goto end_of_routine;
  }  /* if */
  check_assertion_str(
      field_size <= (unsigned int)(targ_sizeof_largest_integer*targ_char_bit),
      "source_bitfield_promoted_type: bit-field too big");
  if (!C_mode()) {
    /* In C++, the size of a bit field can be larger than the size of
       the underlying type.  The value representation only includes
       the number of bits in the underlying type, however.  Base the
       promotion rules on that.  See [class.bit]p1 in the C++ standard. */
    a_targ_size_t    int_size;
    a_targ_alignment int_alignment;
    get_integer_size_and_alignment(ikind, &int_size, &int_alignment);
    int_size *= targ_char_bit;
    if (field_size > int_size) field_size = (unsigned int)int_size;
  }  /* if */

  if (is_immediate_enum_type(promoted_type) &&
      ikind >= (an_integer_kind)ik_int) {
    /* Enum bit fields keep their underlying type when being promoted, unless
       this underlying type is smaller than an int. */
  } else if (gcc_version_is(< 40000) &&
      field->bit_size == (unsigned int)(targ_sizeof_long*targ_char_bit)) {
    /* gcc before 4.0 considers bit fields that are exactly as long as "long"
       to retain their types. */
  } else if ((microsoft_mode || gpp_version_is(< 40200)) &&
             ikind >= (an_integer_kind)ik_int) {
    /* MSVC++ and g++ before 4.2.0 consider int and larger bit fields to
       retain their types even if the bit field size is less than the size of
       int. */
  } else if (field->bit_field_is_signed) {
    /* Bit-field is signed, so it is promoted to int if all its values will
       fit in int. */
    if (field_size <= (unsigned int)(targ_sizeof_int*targ_char_bit)) {
      ikind = (an_integer_kind)ik_int;
    } else {
      /* Bit-fields larger than int keep the original type. */
    }  /* if */
  } else {
    /* Bit-field is unsigned, so it is promoted to the first of int or
       unsigned int into which all its values will fit. */
    if (clang_mode && field->declared_bit_size ==
                              (unsigned int)(targ_sizeof_int*targ_char_bit)) {
      /* Clang promotes over-sized unsigned bitfields with the width of an
         int to be unsigned ints, not ints.  E.g., "unsigned char c:32"
         promotes to unsigned int. */
      ikind = (an_integer_kind)ik_unsigned_int;
    } else if (field_size < (unsigned int)(targ_sizeof_int*targ_char_bit)) {
      ikind = (an_integer_kind)ik_int;
    } else if (field_size == (unsigned int)(targ_sizeof_int*targ_char_bit)) {
      ikind = (an_integer_kind)ik_unsigned_int;
#if LONG_LONG_ALLOWED
    } else if (gcc_mode && gnu_version == 40000 &&
               ikind == (an_integer_kind)ik_unsigned_long_long &&
               field_size <
                         (unsigned int)(targ_sizeof_long_long*targ_char_bit)) {
      /* gcc 4.0 promotes unsigned long long bit fields of less than 64 bits
         to signed long long.  This doesn't seem to conform to the C99
         standard (6.3.1.1p2). */
      ikind = (an_integer_kind)ik_long_long;
#endif /* LONG_LONG_ALLOWED */
    } else {
      /* Bit-fields larger than unsigned int keep the original type. */
    }  /* if */
  }  /* if */
  if (ikind != orig_ikind || is_immediate_enum_type(promoted_type)) {
    /* If the integer kind changed, update the promoted type to the promoted
       kind.  Even if it didn't change, return the underlying type in the case
       of an enum type (unscoped; scoped enum types don't get here). */
    promoted_type = source_integer_view(ikind);
  }  /* if */
end_of_routine:
  db_exit();
  return promoted_type;
}  /* source_bitfield_promoted_type */

static a_boolean source_requires_bitfield_promotion(an_expr_node_ptr  node,
                                              a_type_ptr        *p_type)
/*
A bit-field expression is a direct access to a bit field, or such an access
under operations that maintain the bit-field nature of the access.  The exact
meaning of this is unfortunately a little unclear in both the C and C++
standards.  Still, if p->bf is a direct access to a bit field that is smaller
than an int, the usual interpretation is that expressions like ++p->bf,
(x, p->bf), and p->bf = 1 are still bit-field expressions and that if the
accessed bit field is smaller than an int, the promoted type of the expression
is int (even if the bit field itself has, e.g., unsigned int type).

This function returns TRUE if the given node is a bit-field expression (except
in modes where bit-field expressions have no special treatment), and in that
case it sets *p_type to the bit-field's promoted type.
*/
{
  a_boolean    result = TRUE;
  a_field_ptr  fp = NULL;

  if (microsoft_mode || C_dialect == C_dialect_pcc) {
    /* pcc and MSVC don't treat bit fields in a special way. */
    result = FALSE;
    goto loop_done;
  } else if (!bit_field_promotion_applies_to_some_operations) {
    /* Just check the case of a direct access to a bit field. */
    if (source_is_bitfield_extract_node(node)) {
      if (is_variable_node(node)) {
        a_variable_ptr vp = node_variable(node);
        check_assertion(vp->init_kind == (an_init_kind)initk_binding);
        node = vp->initializer.bound_expr;
      }
      *p_type = source_bitfield_promoted_type(node);
    } else {
      result = FALSE;
    }  /* if */
    goto loop_done;
  }  /* if */
  for (;;) {
    if (is_operation_node(node)) {
      switch (node->variant.operation.kind) {
        case eok_dot_field:
        case eok_points_to_field:
          fp = node_field(node->variant.operation.operands->next);
          if (fp->is_bit_field) {
            /* The given expression was a bit-field expression.  Determine its
               promoted type. */
            *p_type = source_bitfield_promoted_type(node);
          } else {
            result = FALSE;
          }  /* if */
          goto loop_done;
        case eok_parens:
        case eok_assign:
        case eok_add_assign:
        case eok_subtract_assign:
        case eok_multiply_assign:
        case eok_divide_assign:
        case eok_remainder_assign:
        case eok_shiftl_assign:
        case eok_shiftr_assign:
        case eok_and_assign:
        case eok_or_assign:
        case eok_xor_assign:
        case eok_padd_assign:
        case eok_psubtract_assign:
          /* Operations that preserve the bit-field nature of their first
             operand. */
          node = node->variant.operation.operands;
          break;
        case eok_pre_incr:
        case eok_pre_decr:
          if (clang_mode) {
            /* Clang doesn't promote the prefix increment/decrement
               operations. */
            result = FALSE;
            goto loop_done;
          }  /* if */
          node = node->variant.operation.operands;
          break;
        case eok_post_decr:
        case eok_post_incr:
          if ((!C_mode() && !gpp_mode) || clang_mode) {
            /* C++ doesn't make a postfix increment/decrement a bit field even
               if the operand is one, but GCC does.  (Clang doesn't in any
               mode.) */
            result = FALSE;
            goto loop_done;
          }  /* if */
          node = node->variant.operation.operands;
          break;
        case eok_comma:
          /* The comma operator preserves the bit-field nature of its second
             operand. */
          node = node->variant.operation.operands->next;
          break;
        default:
          result = FALSE;
          goto loop_done;
      }  /* switch */
    } else if (is_variable_node(node)) {
      a_variable_ptr vp = node_variable(node);
      if (vp->init_kind != (an_init_kind)initk_binding) {
        result = FALSE;
        break;
      }  /* if */
      node = vp->initializer.bound_expr;
    } else {
      result = FALSE;
      break;
    }  /* if */
  }  /* for */
loop_done:
  return result;
}  /* source_requires_bitfield_promotion */

a_type_ptr source_promoted_type(an_expr_node_ptr node)
/*
Determine the type that would result from applying the integral promotions
to the indicated expression.  Return the promoted type, which may be the
same as the original type.  The expression is an rvalue.
*/
{
  a_type_ptr promoted_type;

  /* Check for bit-field accesses, which require special handling.
     The special processing is not done in pcc and Microsoft modes. */
  if (!source_requires_bitfield_promotion(node, &promoted_type)) {
    promoted_type = source_type_after_integral_promotion(node->type);
  }  /* if */
  return promoted_type;
}  /* source_promoted_type */

#undef source_is_integer_or_unscoped_enum
