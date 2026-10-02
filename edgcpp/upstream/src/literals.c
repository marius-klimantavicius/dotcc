/*
Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
Exceptions.
See https://edgcpp.org/LICENSE.txt for license information.
SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
*/

/*

literals.c -- Literal constant conversion to and from internal form.

*/

/* Header files common to all files. */
#include "fe_common.h"

#ifdef PCH_PRAGMA_GUARD
/* Mark the end of the sequence of headers subject to precompiled header
   processing. */
#pragma hdrstop
#endif /* ifdef PCH_PRAGMA_GUARD */

/* Additional header files. */
#include "literals.h"
#include "preproc.h"

/* Conditionally open the "edg" namespace. */
BEGIN_EDG_NAMESPACE


STATIC_THREAD a_text_buffer_ptr
		token_buffer;
			/* A text buffer used to hold the spelling of a
			   token after removal of digit separators. */

static a_const_char *remove_digit_separators(a_const_char *first_char,
                                             a_const_char *last_char)
/*
Make a copy of the characters from first_char to last_char, inclusive, but
excluding any apostrophe (C++14 digit separator) characters, and return a
pointer to the first character of the resulting null-terminated string.
*/
{
  a_const_char *p;

  if (token_buffer == NULL) {
    /* Allocate a buffer for the copy. */
    token_buffer = alloc_text_buffer(64);
  }  /* if */
  reset_text_buffer(token_buffer);
  for (p = first_char; p <= last_char; ++p) {
    if (*p != '\'') {
      add_char_to_text_buffer(token_buffer, *p);
    }  /* if */
  }  /* if */
  add_char_to_text_buffer(token_buffer, '\0');
  return token_buffer->buffer;
}  /* remove_digit_separators */

#if MICROSOFT_EXTENSIONS_ALLOWED

static void trim_integer_value_to_kind(an_integer_value  *p_value,
                                       an_integer_kind   kind)
/*
Mask off the bits on the left of the most-significant bit of *p_value assuming
it represents an integer of the given kind.  That might mean that
sign-extension might be needed later on.
*/
{
  a_targ_size_t     size;
  a_targ_alignment  alignment;
  an_integer_value  mask;

  get_integer_size_and_alignment(kind, &size, &alignment);
  make_integer_value_mask(&mask, size_t_arg(size * targ_char_bit));
  and_integer_values(p_value, &mask);
}  /* trim_integer_value_to_kind */

#endif /* MICROSOFT_EXTENSIONS_ALLOWED */

static a_boolean is_bit_precise_literal_suffix(a_const_char  *first_char,
                                               a_const_char  *last_char,
                                               a_const_char  **suffix_start,
                                               a_boolean     *is_unsigned)
/*
Return TRUE if the characters ending at last_char form a bit-precise integer
literal suffix.  The "wb" portion must be consistently lowercase or uppercase.
*/
{
  a_boolean result = FALSE;

  if (last_char - first_char >= 1 &&
      ((last_char[-1] == 'w' && last_char[0] == 'b') ||
       (last_char[-1] == 'W' && last_char[0] == 'B'))) {
    a_const_char  *start = last_char - 1;
    *is_unsigned = FALSE;
    if (last_char - first_char >= 2 &&
        (last_char[-2] == 'u' || last_char[-2] == 'U')) {
      start = last_char - 2;
      *is_unsigned = TRUE;
    }  /* if */
    if (prefixed_bit_precise_literal_suffix_allowed() &&
        start - first_char >= 2 && start[-2] == '_' && start[-1] == '_') {
      *suffix_start = start - 2;
      result = TRUE;
    } else if (standard_bit_precise_literal_suffix_allowed()) {
      *suffix_start = start;
      result = TRUE;
    }  /* if */
  }  /* if */
  return result;
}  /* is_bit_precise_literal_suffix */


static size_t bits_required_to_represent_unsigned_value(
                                                    an_integer_value *value)
/*
Return the number of bits required to represent the indicated unsigned value.
*/
{
  a_constant con;

  clear_constant(&con, (a_constant_repr_kind)ck_integer);
  con.type = integer_type((an_integer_kind)ik_unsigned_long);
  con.variant.integer_value = *value;
  return bits_required_to_represent_integer_constant(&con);
}  /* bits_required_to_represent_unsigned_value */


void conv_integer_literal(int                  radix,
                          an_error_code        *err_code,
                          a_const_char         **err_pos,
                          ARG_UNUSED a_boolean potential_ud_literal)
/*
Convert an integer of base indicated by radix (2, 8, 10, or 16) from
external form to internal form.  start_of_curr_token and end_of_curr_token
point to the two ends of the external form.  The internal form is placed in
const_for_curr_token.  If there is no error, *err_code is set to
ec_no_error (which is 0); otherwise, *err_code is set to an appropriate
error code and *err_pos is set to the character position of the error.  A
zero-length number is converted as zero.  Other than the zero-length
pathology, the input number is guaranteed to be syntactically correct
(except for digits 8 and 9 in octal constants or digits above 1 for binary
constants).  The number may have a "u" or "l" suffix, or both. (Or an "ll"
or "ull" suffix, if long long is allowed.) (Or a suffix like "i32", if
Microsoft extensions are enabled.)  Apostrophes (C++14 digit separators)
within the token are unconditionally ignored, since they will only be part
of the token if digit separators are enabled.  If potential_ud_literal is
TRUE, the integer literal might be part of a user-defined literal, which
affects the handling of some overflow cases.
*/
{
  an_integer_value number, ten, digit, mask;
  a_boolean        has_u_suffix = FALSE;
  a_boolean        has_l_suffix = FALSE;
  a_boolean        has_z_suffix = FALSE;
  a_boolean        has_bit_precise_suffix = FALSE;
  a_boolean        bit_precise_suffix_is_unsigned = FALSE;
  a_const_char     *bit_precise_suffix_start = NULL;
#if LONG_LONG_ALLOWED
  a_boolean        has_ll_suffix = FALSE;
  char		   l_char_used = '\0';
#endif /* LONG_LONG_ALLOWED */
  a_const_char     *temp_ptr;
  a_boolean        err, ovflo = FALSE, do_sign_extension = FALSE;
  a_boolean        non_arith = (radix != 10);
  a_const_char     *real_end_pos = end_of_curr_token;
  unsigned long    intdigit;
  an_integer_kind  kind;
#if MICROSOFT_EXTENSIONS_ALLOWED
  an_integer_kind  isuffix_kind = (an_integer_kind)ik_none;
#endif /* MICROSOFT_EXTENSIONS_ALLOWED */

  *err_code = ec_no_error;
  /* Locate and logically remove the suffix, if any.  The suffix is an
     optional "u/U" for unsigned and/or "l/L" for long, "ll/LL" for long
     long (if LONG_LONG_ALLOWED is TRUE), or "z/Z" for the size_t
     signed/unsigned type (if size_suffix_enabled is TRUE); these can
     appear in either order. */
  if (real_end_pos >= start_of_curr_token) {
    if (is_bit_precise_literal_suffix(start_of_curr_token, real_end_pos,
                                      &bit_precise_suffix_start,
                                      &bit_precise_suffix_is_unsigned)) {
      has_bit_precise_suffix = TRUE;
      real_end_pos = bit_precise_suffix_start - 1;
    }  /* if */
#if MICROSOFT_EXTENSIONS_ALLOWED
    if (ms_extensions) {
      /* The Microsoft compiler allows a suffix like "i32" indicating a
         32-bit integer.  "ui32" indicates an unsigned 32-bit integer. */
      /* Look for an "i" or "I" anywhere in the number. */
      /*lint --e{850} temp_ptr modified in loop */
      for (temp_ptr = start_of_curr_token;
           temp_ptr <= real_end_pos;
           temp_ptr++) {
        if (*temp_ptr == 'i' || *temp_ptr == 'I') {
          /* Yes, we have a suffix like "i32". */
          /* scan_number has ensured that there is at least one digit
             following the "i". */
          a_const_char  *suffix_loc = temp_ptr;
          unsigned long isuffix = 0;
          unsigned long ndigits = 0;

          real_end_pos = temp_ptr-1;
          temp_ptr++;
          /* Accumulate the size. */
          do {
            isuffix *= 10;
            isuffix += (unsigned long)(*temp_ptr++) - (unsigned long)'0';
            ndigits++;
          } while (isdigit((unsigned char)(*temp_ptr)));
          /* Check that the size is valid. */
          if (ndigits <= 3) {
            if (isuffix == 8 &&
                targ_int8_int_kind != (an_integer_kind)ik_none) {
              isuffix_kind = targ_int8_int_kind;
            } else if (isuffix == 16 &&
                       targ_int16_int_kind != (an_integer_kind)ik_none) {
              isuffix_kind = targ_int16_int_kind;
            } else if (isuffix == 32 &&
                       targ_int32_int_kind != (an_integer_kind)ik_none) {
              isuffix_kind = targ_int32_int_kind;
            } else if (isuffix == 64 &&
                       targ_int64_int_kind != (an_integer_kind)ik_none) {
              isuffix_kind = targ_int64_int_kind;
            }  /* if */
          }  /* if */
          if (isuffix_kind == (an_integer_kind)ik_none) {
            /* Bad size. */
            *err_pos = suffix_loc;
            *err_code = ec_bad_suffix;
          }  /* if */
        }  /* if */
      }  /* for */
    }  /* if */
#endif /* MICROSOFT_EXTENSIONS_ALLOWED */
    for (;;) {
      if (*real_end_pos == 'u' || *real_end_pos == 'U') {
        has_u_suffix = TRUE;
        --real_end_pos;
      } else if (*real_end_pos == 'l' || *real_end_pos == 'L') {
#if LONG_LONG_ALLOWED
        if (has_l_suffix) {
          has_l_suffix = FALSE;
          has_ll_suffix = TRUE;
          if (*real_end_pos != l_char_used && strict_ansi_mode) {
            /* An invalid suffix such as "Ll" or "lL".  Give an error but
               still treat it as a long long. */
            *err_pos = real_end_pos;
            *err_code = ec_bad_suffix;
          }  /* if */
        } else
#endif /* LONG_LONG_ALLOWED */
        {
          has_l_suffix = TRUE;
#if LONG_LONG_ALLOWED
          l_char_used = *real_end_pos;
#endif /* LONG_LONG_ALLOWED */
        }  /* if */
        --real_end_pos;
      } else if (size_suffix_enabled &&
                 (*real_end_pos == 'z' || *real_end_pos == 'Z')) {
        has_z_suffix = TRUE;
        --real_end_pos;
      } else {
        /* Not an "l" or "z" or "u"; exit loop. */
        break;
      }  /* if */
    }  /* for */
    if (has_bit_precise_suffix &&
        (has_u_suffix || has_l_suffix || has_z_suffix
#if LONG_LONG_ALLOWED
         || has_ll_suffix
#endif /* LONG_LONG_ALLOWED */
                                             )) {
      *err_pos = bit_precise_suffix_start;
      *err_code = ec_bad_suffix;
    }  /* if */
#if MICROSOFT_EXTENSIONS_ALLOWED
    if (ms_extensions && isuffix_kind != (an_integer_kind)ik_none &&
        has_u_suffix) {
      /* The number has a suffix like "ui32".  Adjust the kind to the
         corresponding unsigned integral kind. */
      isuffix_kind = unsigned_int_kind_of[(int)isuffix_kind];
    }  /* if */
#endif /* MICROSOFT_EXTENSIONS_ALLOWED */
  }  /* if */

  /* Evaluate the literal as an unsigned long. */
  if (radix == 10) {
    /* Decimal. */
    if (!number_contains_digit_separator &&
        ((sizeof(a_host_large_unsigned) >= 16 &&
          real_end_pos - start_of_curr_token <= 36) ||
         (sizeof(a_host_large_unsigned) >= 8 &&
          real_end_pos - start_of_curr_token <= 18) ||
         (sizeof(a_host_large_unsigned) >= 4 &&
          real_end_pos - start_of_curr_token <= 7))) {
      /* The value of the literal can be represented without overflow in
         a_host_large_unsigned, so we can use a more efficient loop
         accumulating the literal value. */
      a_host_large_unsigned lit_val =
                                   (unsigned char)(*start_of_curr_token) - '0';
      for (temp_ptr = start_of_curr_token + 1;
           temp_ptr <= real_end_pos; ++temp_ptr) {
        lit_val = 10 * lit_val + (unsigned char)(*temp_ptr) - '0';
      }  /* for */
      set_unsigned_integer_value(&number, lit_val);
    } else {
      /* For literals that might overflow a host integer or that contain
         embedded digit separators, use the constant integer routines to
         calculate the value of the literal. */
      set_unsigned_integer_value(&ten, (a_host_large_unsigned)10);
      intdigit = (unsigned char)(*start_of_curr_token) - '0';
      set_unsigned_integer_value(&number, (a_host_large_unsigned)intdigit);
      for (temp_ptr = start_of_curr_token+1;
           temp_ptr <= real_end_pos; temp_ptr++) {
        if (*temp_ptr == '\'') {
          /* Digit separator -- ignore. */
        } else {
          intdigit = (unsigned char)(*temp_ptr) - '0';
          /* Multiply previous value by 10, checking for overflow. */
          multiply_integer_values(&number, &ten, /*is_signed=*/FALSE, &err);
          if (err) ovflo = TRUE;
          /* Add in digit, checking for overflow. */
          set_unsigned_integer_value(&digit, (a_host_large_unsigned)intdigit);
          add_integer_values(&number, &digit, /*is_signed=*/FALSE, &err);
          if (err) ovflo = TRUE;
        }  /* if */
      }  /* for */
    }  /* if */
  } else if (radix == 8) {
    /* Octal.*/
    set_unsigned_integer_value(&number, (a_host_large_unsigned)0);
    for (temp_ptr = start_of_curr_token+1;
         temp_ptr <= real_end_pos; temp_ptr++) {
      if (*temp_ptr == '\'') {
        /* Digit separator -- ignore. */
      } else {
        intdigit = (unsigned char)(*temp_ptr) - '0';
        if (C_dialect != C_dialect_pcc && (intdigit >= 8)) {
          /* Digits 8 and 9 are allowed by K&R/pcc, but not by ANSI. */
          *err_pos = temp_ptr;
          *err_code = ec_bad_octal_digit;
          goto wrapup;
        }  /* if */
        /* Multiply previous value by 8, checking for overflow. */
        shift_left_integer_value(&number, 3, &err);
        if (err) ovflo = TRUE;
        /* Or in digit. */
        set_unsigned_integer_value(&digit, (a_host_large_unsigned)intdigit);
        or_integer_values(&number, &digit);
      }  /* if */
    }  /* for */
  } else if (radix == 2) {
    /* Binary.*/
    set_unsigned_integer_value(&number, (a_host_large_unsigned)0);
    for (temp_ptr = start_of_curr_token+2;
         temp_ptr <= real_end_pos; temp_ptr++) {
      if (*temp_ptr == '\'') {
        /* Digit separator -- ignore. */
      } else {
        intdigit = (unsigned char)(*temp_ptr) - '0';
        if (intdigit >= 2) {
          /* Digits over 1 are not allowed. */
          *err_pos = temp_ptr;
          *err_code = ec_bad_binary_digit;
          goto wrapup;
        }  /* if */
        /* Multiply previous value by 2, checking for overflow. */
        shift_left_integer_value(&number, 1, &err);
        if (err) ovflo = TRUE;
        /* Or in digit. */
        set_unsigned_integer_value(&digit, (a_host_large_unsigned)intdigit);
        or_integer_values(&number, &digit);
      }  /* if */
    }  /* for */
  } else {
    /* radix == 16 (hexadecimal). */
    set_unsigned_integer_value(&number, (a_host_large_unsigned)0);
    for (temp_ptr = start_of_curr_token+2;
         temp_ptr <= real_end_pos; temp_ptr++) {
      if (*temp_ptr == '\'') {
        /* Digit separator -- ignore. */
      } else {
        intdigit = hexvalue((unsigned char)(*temp_ptr));
        /* Multiply previous value by 16, checking for overflow. */
        shift_left_integer_value(&number, 4, &err);
        if (err) ovflo = TRUE;
        /* Or in digit. */
        set_unsigned_integer_value(&digit, (a_host_large_unsigned)intdigit);
        or_integer_values(&number, &digit);
      }  /* if */
    }  /* for */
  }  /* if */
  if (has_bit_precise_suffix) {
    /* Bit-precise integer literals use the smallest _BitInt type that can
       represent the accumulated value. */
    if (!ovflo && *err_code == ec_no_error) {
      a_targ_size_t width = (a_targ_size_t)
                         bits_required_to_represent_unsigned_value(&number);
      if (!bit_precise_suffix_is_unsigned) {
        width++;
        if (width < 2) width = 2;
      } else if (width < 1) {
        width = 1;
      }  /* if */
      if (width > bitint_maxwidth_value) {
        a_source_position pos;
        conv_line_loc_to_source_pos(start_of_curr_token, &pos);
        pos_num2_diagnostic(es_error, ec_bitint_width_too_large, &pos,
                            (int32_t)width,
                            (int32_t)bitint_maxwidth_value);
        set_error_constant(&const_for_curr_token);
        goto wrapup;
      }  /* if */
      clear_constant(&const_for_curr_token,
                     (a_constant_repr_kind)ck_integer);
      const_for_curr_token.type = bit_precise_integer_type(
                                      width, bit_precise_suffix_is_unsigned,
                                      /*explicitly_signed=*/FALSE);
      const_for_curr_token.variant.integer_value = number;
      const_for_curr_token.non_arithmetic = non_arith;
      const_for_curr_token.is_simple_zero = FALSE;
      goto wrapup;
    }  /* if */
    if (*err_code != ec_no_error) {
      goto wrapup;
    }  /* if */
    check_assertion(ovflo);
    goto bit_precise_literal_done;
  } else if (in_pp_if_expression && (c99_mode || gnu_mode)) {
    /* Determine the type based on the value and the suffixes. */
    /* C99 was amended with DR 265 to the effect that the conversion of an
       integer literal in a #if control expression should treat all integer
       types as having the same representation as intmax_t or uintmax_t
       (depending on their signedness). */
    if (has_u_suffix) {
      /* A "u" suffix is always mapped onto an unsigned type. */
      kind = targ_uintmax_kind;
    } else if (radix == 10 ||
               le_max_integer_value_of_kind(&number, /*is_signed=*/FALSE,
                                      targ_intmax_kind)) {
      /* Decimal literals without a "u" suffix are always signed.  Nondecimal
         literals are signed if they can be represented by the signed type. */
      kind = targ_intmax_kind;
    } else {
      kind = targ_uintmax_kind;
    }  /* if */
    goto kind_established;
  }  /* if */
  /* In pcc compatibility mode, overflow is ignored, and the constant is
     either int or long (see K&R, reference manual section, 2.4.1 and 2.4.2).
     Since the "u" suffix does not exist in pcc C, treat constants with that
     suffix according to the ANSI rules. */
#if LONG_LONG_ALLOWED
  /* Likewise for "ll". */
#endif /* LONG_LONG_ALLOWED */
  if (C_dialect == C_dialect_pcc && !has_u_suffix
#if LONG_LONG_ALLOWED
      && !has_ll_suffix
#endif /* LONG_LONG_ALLOWED */
                                                 ) {
    /* Non-ANSI (pcc) checking. */
    if (has_l_suffix) goto pcc_l_check;
    if (radix == 10 &&
        le_max_integer_value_of_kind(&number, /*is_signed=*/FALSE,
                                     (an_integer_kind)ik_int)) {
      /* A decimal constant that is no larger than the largest signed int
         is an int. */
      kind = (an_integer_kind)ik_int;
      goto pcc_kind_established;
    }  /* if */
    if (radix != 10 &&
        le_max_integer_value_of_kind(&number, /*is_signed=*/FALSE,
                                     (an_integer_kind)ik_unsigned_int)) {
      /* A hexadecimal or octal constant that is no larger than the largest
         unsigned int is treated as an int (there are no unsigned int
         constants in K&R/pcc). */
      kind = (an_integer_kind)ik_int;
      do_sign_extension = TRUE;
      goto pcc_kind_established;
    }  /* if */
pcc_l_check:
    if (le_max_integer_value_of_kind(&number, /*is_signed=*/FALSE,
                                     (an_integer_kind)ik_unsigned_long)) {
      /* A constant that is no larger than the largest unsigned long is
         treated as a long (there are no unsigned long constants in
         K&R/pcc). */
      kind = (an_integer_kind)ik_long;
      do_sign_extension = TRUE;
      /* A decimal constant that is greater than the largest long is considered
         a long, but tagged as non-arithmetic because the source looks
         positive but the internal value is negative.  This helps in
         avoiding an error when converting the smallest integer. */
      if (!non_arith &&
          !le_max_integer_value_of_kind(&number, /*is_signed=*/FALSE,
                                        (an_integer_kind)ik_long)) {
        non_arith = TRUE;
      }  /* if */
      goto pcc_kind_established;
    }  /* if */
#if LONG_LONG_ALLOWED
    if (le_max_integer_value_of_kind(&number, /*is_signed=*/FALSE,
                                     (an_integer_kind)ik_unsigned_long_long)) {
      /* long long. */
      kind = (an_integer_kind)ik_long_long;
      do_sign_extension = TRUE;
      /* A decimal constant that is larger than LONG_LONG_MAX is tagged as
         non-arithmetic because the source looks positive but the internal
         value is negative.  This helps in avoiding an error when
         converting the smallest integer. */
      if (!non_arith &&
          !le_max_integer_value_of_kind(&number, /*is_signed=*/FALSE,
                                        (an_integer_kind)ik_long_long)) {
        non_arith = TRUE;
      }  /* if */
      goto pcc_kind_established;
    }  /* if */
#endif /* LONG_LONG_ALLOWED */
    /* Doesn't fit in target integers.  This can only happen when the
       host representation for integer values can hold values larger
       than the largest target integer. */
    ovflo = TRUE;
#if LONG_LONG_ALLOWED
    kind = (an_integer_kind)ik_long_long;
#else /* !LONG_LONG_ALLOWED */
    kind = (an_integer_kind)ik_long;
#endif /* LONG_LONG_ALLOWED */
pcc_kind_established:
    if (ovflo) {
      /* A warning is generated for overflow, but the overflow is then
         ignored.  The conversions above produce the same value that pcc
         does. */
      /* Convert the character position into an error position. */
      conv_line_loc_to_source_pos(start_of_curr_token, &error_position);
      pos_warning(ec_integer_too_large, &error_position);
      non_arith = TRUE;
      do_sign_extension = int_kind_is_signed[kind];
      /* Mask off any bits past the end of the largest target integer. */
      make_integer_value_mask(&mask,
                              size_t_arg(targ_sizeof_largest_integer *
                                         targ_char_bit));
      and_integer_values(&number, &mask);
      ovflo = FALSE;
    }  /* if */
  } else if (!ovflo) {
    /* Non-pcc-mode constant checking. */
#if MICROSOFT_EXTENSIONS_ALLOWED
    if (ms_extensions && isuffix_kind != (an_integer_kind)ik_none) {
      /* The number has a suffix like "i32".  The kind has already been
         determined. */
      kind = isuffix_kind;
      /* If necessary, truncate the constant to the size specified. */
      if (!le_max_integer_value_of_kind(&number, /*is_signed=*/FALSE, kind)) {
        /* The constant doesn't fit in the integer kind. */
        /* Convert the character position into an error position. */
        conv_line_loc_to_source_pos(start_of_curr_token, &error_position);
        pos_warning(ec_integer_too_large, &error_position);
        non_arith = TRUE;
        /* Mask off any bits past the end of the integer. */
        trim_integer_value_to_kind(&number, kind);
        do_sign_extension = int_kind_is_signed[kind];
      }  /* if */
      goto kind_established;
    }  /* if */
#endif /* MICROSOFT_EXTENSIONS_ALLOWED */
    /* ISO C/C++ constant checking. */
    if (has_z_suffix) {
      /* The constant will have the type std::size_t or the corresponding
         signed type.  This literal can only be one of those specific
         types; unlike other integer literals, it cannot be implicitly
         interpreted as a larger type, per core issue 2698.  Note that the
         following code relies on these types having adjacent kinds, with
         the unsigned type immediately following the signed type. */
      an_integer_kind signed_kind, unsigned_kind;
      if (int_kind_is_signed[(int)targ_size_t_int_kind]) {
        signed_kind = targ_size_t_int_kind;
        unsigned_kind = (an_integer_kind)(targ_size_t_int_kind + 1);
      } else {
        signed_kind = (an_integer_kind)(targ_size_t_int_kind - 1);
        unsigned_kind = targ_size_t_int_kind;
      }  /* if */
      if (has_u_suffix) {
        /* UZ (and similar combinations) always leads to type size_t. */
        kind = unsigned_kind;
        ovflo = !le_max_integer_value_of_kind(&number, /*is_signed=*/FALSE,
                                              kind);
      } else {
        /* Z without U produces the signed counterpart of size_t if (a) the
           literal is decimal or (b) if the value fits in that type.
           Otherwise, the type corresponds to size_t.  GCC appears to have a
           bug where literals with the Z suffix but not the U suffix are
           always signed. */
        if (radix == 10 || gnu_version_is(any_version)) {
          kind = signed_kind;
          if (radix == 10) {
            ovflo = !le_max_integer_value_of_kind(&number, /*is_signed=*/TRUE,
                                                  kind);
          } else {
            ovflo = !le_max_integer_value_of_kind(&number, /*is_signed=*/FALSE,
                                                  unsigned_kind);
          }  /* if */
        } else if (le_max_integer_value_of_kind(&number, /*is_signed=*/FALSE,
                                                signed_kind)) {
          kind = signed_kind;
        } else {
          kind = unsigned_kind;
          ovflo = !le_max_integer_value_of_kind(&number, /*is_signed=*/FALSE,
                                                kind);
        }  /* if */;
      }  /* if */
      do_sign_extension = kind == signed_kind;
      goto kind_established;
    }  /* if */
#if LONG_LONG_ALLOWED
    if (has_ll_suffix) goto ll_check;
#endif /* LONG_LONG_ALLOWED */
    if (has_l_suffix) goto l_check;
    if (!has_u_suffix &&
        le_max_integer_value_of_kind(&number, /*is_signed=*/FALSE,
                                     (an_integer_kind)ik_int)) {
      kind = (an_integer_kind)ik_int;
      goto kind_established;
    } else if ((has_u_suffix || radix != 10) &&
               le_max_integer_value_of_kind(&number, /*is_signed=*/FALSE,
                                           (an_integer_kind)ik_unsigned_int)) {
      kind = (an_integer_kind)ik_unsigned_int;
      goto kind_established;
#if LONG_LONG_ALLOWED
    } else if (!has_u_suffix && microsoft_mode && radix == 10 &&
               (microsoft_version < 1924 ? TRUE :
                microsoft_version < 1928 ? ms_permissive
                                         : ms_permissive && !cpp20_mode) &&
               targ_sizeof_long == targ_sizeof_int && long_long_is_standard &&
               targ_sizeof_long < targ_sizeof_long_long &&
               le_max_integer_value_of_kind(
                                          &number, /*is_signed=*/FALSE,
                                          (an_integer_kind)ik_unsigned_long)) {
      /* Ordinarily, literals in this category (unsuffixed decimal, fitting in
         unsigned long but not signed long) have type long long (see, e.g.,
         [lex.icon]/3 in N4901).  However, earlier versions of MSVC appear to
         use the unsigned long type instead.  MSVC 19.24 fixes that in non-
         permissive modes.  MSVC 19.28 also fixes it in "c++latest" mode.
         For example:
           template<typename T1, typename T2> struct is_same;
           template<typename T> struct is_same<T, T> { enum { value = 1 }; };
           constexpr auto x = 0x80000000;
           static_assert(is_same<decltype(x), unsigned const>::value, "");
           constexpr auto y = 2147483648;
           static_assert(is_same<decltype(y), unsigned long const>::value, "");
           constexpr auto z = 4294967296;
           static_assert(is_same<decltype(z), const long long>::value, "");
         The first and third assertions are standard and always accepted by
         MSVC (as of 19.30).  The second is nonstandard and is usually accepted
         except in non-permissive mode starting with MSVC 19.24, and in
         "c++latest" mode starting with MSVC 19.28.  (In the Microsoft ABI,
         long and int always have the same size (32 bits) and are always
         smaller than long long.  We don't emulate this behavior in Microsoft
         modes paired with ABIs when long is larger than int. */
      kind = (an_integer_kind)ik_unsigned_long;
      goto kind_established;
#endif /* LONG_LONG_ALLOWED */
    }  /* if */
l_check:
    if (!has_u_suffix &&
        le_max_integer_value_of_kind(&number, /*is_signed=*/FALSE,
                                     (an_integer_kind)ik_long)) {
      kind = (an_integer_kind)ik_long;
      goto kind_established;
    } else if ((has_u_suffix || radix != 10 || !long_long_promotion_allowed) &&
               le_max_integer_value_of_kind(&number, /*is_signed=*/FALSE,
                                          (an_integer_kind)ik_unsigned_long)) {
      /* When long long is not a standard type (including the case when long
         long does not exist) a signed constant promotes to unsigned long
         before (possibly) considering long long. */
      kind = (an_integer_kind)ik_unsigned_long;
      goto kind_established;
    }  /* if */
#if LONG_LONG_ALLOWED
ll_check:
    if (strict_ansi_mode && !long_long_is_standard && !has_ll_suffix) {
      /* long long is not supported, so skip these range checks to force
         a constant-too-large error.  Don't skip if there is an explicit
         "ll" suffix, to avoid two errors. */
    } else if (!has_u_suffix &&
        le_max_integer_value_of_kind(&number, /*is_signed=*/FALSE,
                                     (an_integer_kind)ik_long_long)) {
      kind = (an_integer_kind)ik_long_long;
      goto kind_established;
    } else if ((!c99_mode || has_u_suffix || radix != 10) &&
               le_max_integer_value_of_kind(
                                     &number, /*is_signed=*/FALSE,
                                     (an_integer_kind)ik_unsigned_long_long)) {
      /* Note that in C99 a constant that is too large for long long
         but is decimal with no U suffix does not get unsigned long long
         type (that's designed to allow C99 implementations to have
         integral types larger than long long). */
      kind = (an_integer_kind)ik_unsigned_long_long;
      goto kind_established;
    } else if (c99_mode &&
#if INT128_EXTENSIONS_ALLOWED
               !int128_extensions_enabled &&
#endif /* INT128_EXTENSIONS_ALLOWED */
               le_max_integer_value_of_kind(
                                     &number, /*is_signed=*/FALSE,
                                     (an_integer_kind)ik_unsigned_long_long)) {
      /* In C99 mode, give the kind of constant described above
         unsigned long long type, with a warning.  Note that if the
         implementation has extended integer types beyond unsigned long long
         this test should be eliminated.  This should be an error in
         strict mode, but both Plum Hall and Perennial have constants
         like this; a warning counts as a "diagnostic" so it's a
         reasonable compromise until the C committee rules on it. */
      conv_line_loc_to_source_pos(start_of_curr_token, &error_position);
      pos_warning(ec_c99_constant_in_unsigned_long_long_range,
                  &error_position);
      non_arith = TRUE;
      kind = (an_integer_kind)ik_unsigned_long_long;
      goto kind_established;
    }  /* if */
#endif /* LONG_LONG_ALLOWED */
#if INT128_EXTENSIONS_ALLOWED
    if (int128_extensions_enabled) {
      /* 128-bit integers are supported. */
      if (clang_mode || gnu_mode) {
        if (potential_ud_literal) {
          /* An overflowing integer is allowed if it will be used with a
             raw user-defined literal operator or a user-defined literal
             template.  Pass the potential error back to the caller, where
             it will be ignored if this is determined to be part of a valid
             user-defined literal. */
          *err_pos = start_of_curr_token;
          *err_code = ec_integer_too_large;
        } else {
          /* Although Clang and GCC accept types like __int128, they do not
             allow literals of those types.  Clang appears to fall back to
             unsigned long long (after issuing an error), whereas GCC falls
             back to int (after issuing a warning). */
          pos_diagnostic(clang_mode ? es_discretionary_error : es_warning,
                         ec_integer_too_large, &error_position);
        }  /* if */
        kind = clang_mode ? ik_unsigned_long_long
                          : ik_int;
        goto kind_established;
      }  /* if */
      if (!has_u_suffix &&
          le_max_integer_value_of_kind(&number, /*is_signed=*/FALSE,
                                       (an_integer_kind)ik_int128)) {
        kind = (an_integer_kind)ik_int128;
        goto kind_established;
      } else if (le_max_integer_value_of_kind(
                                       &number, /*is_signed=*/FALSE,
                                       (an_integer_kind)ik_unsigned_int128)) {
        kind = (an_integer_kind)ik_unsigned_int128;
        goto kind_established;
      }  /* if */
    }  /* if */
#endif /* INT128_EXTENSIONS_ALLOWED */
    /* Doesn't fit in target integers.  This can only happen when the
       host representation for integer values can hold values larger
       than the largest target integer. */
    ovflo = TRUE;
#if LONG_LONG_ALLOWED
    kind = (an_integer_kind)ik_long_long;
#else /* !LONG_LONG_ALLOWED */
    kind = (an_integer_kind)ik_long;
#endif /* LONG_LONG_ALLOWED */
kind_established:;
  }  /* if */
bit_precise_literal_done:
  if (ovflo) {
    *err_pos = start_of_curr_token;
    *err_code = ec_integer_too_large;
  } else {
    /* Build a constant with the right type and value. */
    clear_constant(&const_for_curr_token, (a_constant_repr_kind)ck_integer);
#if MICROSOFT_EXTENSIONS_ALLOWED
    if (ms_extensions && microsoft_version == 1200 &&
        isuffix_kind != (an_integer_kind)ik_none) {
      const_for_curr_token.type = microsoft_sized_integer_type(kind);
    } else
#endif /* MICROSOFT_EXTENSIONS_ALLOWED */
    /* Do not insert code here. */
    {
      const_for_curr_token.type = integer_type(kind);
    }  /* if */
    /* For values that might be negative (possible in pcc mode), do
       sign extension. */
    if (do_sign_extension) {
      sign_extend_integer_value(&number,
                                const_for_curr_token.type->size *
                                                                targ_char_bit);
    }  /* if */
    const_for_curr_token.variant.integer_value = number;
    const_for_curr_token.non_arithmetic        = non_arith;
    /* is_simple_zero is TRUE if the constant is simply "0".  It's useful to
       know that when the constant is used in a virtual function pure specifier
       in C++. */
    const_for_curr_token.is_simple_zero        = (start_of_curr_token ==
                                                  end_of_curr_token &&
                                                  *start_of_curr_token == '0');
  }  /* if */
wrapup:
  if (*err_code != ec_no_error) {
    /* Return an error constant. */
    set_error_constant(&const_for_curr_token);
  }  /* if */
}  /* conv_integer_literal */

#if FIXED_POINT_ALLOWED

void conv_fixed_point_literal(a_boolean      is_hexadecimal,
                              an_error_code  *err_code,
                              a_const_char   **err_pos)
/*
Convert a fixed-point constant from external form to internal form.
start_of_curr_token and end_of_curr_token point to the two ends of the
external form.  is_hexadecimal is TRUE if the external form is specified
as a hexadecimal value.

The internal form is placed in const_for_curr_token.  If there is no
error, *err_code is set to ec_no_error (which is 0); otherwise,
*err_code is set to an appropriate error code and *err_pos is set to
the character position of the error.

This function is modeled after conv_float_literal (see below).
*/
{
  a_fixed_point_type_descr
               fxp_descr;
  a_fixed_point_value
               value;
  a_const_char *actual_end = end_of_curr_token;
  char         old_next_char = '\0';
  a_boolean    err;
  a_boolean    inexact = FALSE;
  a_const_char *token_no_separators;

  *err_code = ec_no_error;
  /* Check the suffixes. */
  check_assertion(*actual_end == 'r' || *actual_end == 'R' ||
                  *actual_end == 'k' || *actual_end == 'K');
  if (*actual_end == 'r' || *actual_end == 'R') {
    /* "R" suffix, indicates "_Fract" fixed-point type. */
    fxp_descr.is_fract_type = TRUE;
  } else {
    /* "K" suffix, indicates "_Accum" fixed-point type. */
    fxp_descr.is_fract_type = FALSE;
  }  /* if */
  --actual_end;
  fxp_descr.precision = (a_fixed_point_precision)fpp_default;
  fxp_descr.is_unsigned = FALSE;
  fxp_descr.saturating = FALSE;
  for (;;) {
    if (*actual_end == 'u' || *actual_end == 'U') {
      fxp_descr.is_unsigned = TRUE;
    } else if (*actual_end == 'h' || *actual_end == 'H') {
      fxp_descr.precision = (a_fixed_point_precision)fpp_short;
    } else if (*actual_end == 'l' || *actual_end == 'L') {
      fxp_descr.precision = (a_fixed_point_precision)fpp_long;
    } else {
      /* Not a suffix: This should be the actual end of the number. */
      break;
    }  /* if */
    --actual_end;
  }  /* for */
  if (number_contains_digit_separator) {
    /* We must remove the separators before converting. */
    token_no_separators = remove_digit_separators(start_of_curr_token,
                                                  actual_end);
  } else {
  /* Use the token spelling directly.  Place a null after the number to
     guarantee stopping at the right point.  */
    token_no_separators = start_of_curr_token;
    old_next_char = *(actual_end+1);
    *(char *)(actual_end+1) = '\0';
  }  /* if */
  /* Do the conversion. */
  if (is_hexadecimal) {
    fxp_hex_string_to_fixed_point(&fxp_descr, token_no_separators, &value,
                                  &err, &inexact);
  } else {
    fxp_string_to_fixed_point(&fxp_descr, token_no_separators, &value, &err);
  }  /* if */
  if (!number_contains_digit_separator) {
    /* Restore the character that was replaced by a null. */
    *(char *)(actual_end+1) = old_next_char;
  }  /* if */
  if (err) {
    *err_code = ec_bad_fixed_point_value;
    *err_pos = start_of_curr_token;
  } else {
    /* Build a constant with the right type and value. */
    clear_constant(&const_for_curr_token,
                   (a_constant_repr_kind)ck_fixed_point);
    const_for_curr_token.type = fixed_point_type(fxp_descr);
    const_for_curr_token.variant.fixed_point_value = value;
    if (inexact) {
      /* The hex value could not be exactly represented in the specified
         fixed-point format. */
      a_source_position	pos;
      conv_line_loc_to_source_pos(start_of_curr_token, &pos);
      pos_warning(ec_inexact_fxp_conversion, &pos);
    }  /* if */
  }  /* if */
  if (*err_code != ec_no_error) {
    /* Return an error constant. */
    set_error_constant(&const_for_curr_token);
  }  /* if */
}  /* conv_fixed_point_literal */

#endif /* FIXED_POINT_ALLOWED */

void conv_float_literal(a_boolean         is_hexadecimal,
			an_error_code     *err_code,
                        a_const_char      **err_pos,
                        an_error_severity *severity)
/*
Convert a floating constant from external form to internal form.
start_of_curr_token and end_of_curr_token point to the two ends of the
external form.  is_hexadecimal is TRUE if the external form is specified
as a hexadecimal value.

The internal form is placed in const_for_curr_token.  If there is no
error, *err_code is set to ec_no_error (which is 0); otherwise,
*err_code is set to an appropriate error code, *err_pos is set to
the character position of the error, and *severity is set to the severity
of the resulting diagnostic.
*/
{
  a_float_kind kind;
  an_internal_float_value
               number;
  a_const_char *actual_end = end_of_curr_token;
  a_const_char *last_conversion_char;
  char         old_next_char, old_next2_char;
  a_boolean    err;
  a_boolean    inexact = FALSE;
  a_const_char *token_no_separators;
#if GNU_EXTENSIONS_ALLOWED || C99_IL_EXTENSIONS_SUPPORTED
  a_boolean    is_imaginary_literal = FALSE;
#endif /* GNU_EXTENSIONS_ALLOWED || C99_IL_EXTENSIONS_SUPPORTED */

  *err_code = ec_no_error;
  /* See if there is a suffix (or two). */
#if GNU_EXTENSIONS_ALLOWED
  if (*actual_end == 'i' || *actual_end == 'I' ||
      *actual_end == 'j' || *actual_end == 'J') {
    /* GNU accepts imaginary literals like "1.0i", "2.0fj", and "3.0jL".
       So we have to check for a 'i', 'I', 'j', or 'J' both here and
       after a potential "precision suffix" like 'f' or 'L'. */
    is_imaginary_literal = TRUE;
    --actual_end;
  }  /* if */
#endif /* GNU_EXTENSIONS_ALLOWED */
  if (*actual_end == 'f' || *actual_end == 'F') {
    /* "F" suffix, indicates float type. */
    kind = (a_float_kind)fk_float;
    --actual_end;
  } else if (*actual_end == 'l' || *actual_end == 'L') {
    /* "L" suffix, indicates long double. */
    kind = (a_float_kind)fk_long_double;
    --actual_end;
  } else if (float80_enabled && (*actual_end == 'w' || *actual_end == 'W')) {
    /* "W" suffix, indicates __float80. */
    kind = (a_float_kind)float_kind_for_float80;
    --actual_end;
  } else if (float128_enabled && (*actual_end == 'q' || *actual_end == 'Q')) {
    /* "Q" suffix, indicates __float128. */
    kind = (a_float_kind)float_kind_for_float128;
    --actual_end;
  } else if (extended_float_types || gcc_version_is(>= 70000) ||
             gpp_version_is(>= 130000) || clang_version_is(>= 150000)) {
    /* Default to double if none of the extended float suffixes match. */
    kind = fk_double;
    if (actual_end > start_of_curr_token + 3 &&
        (actual_end[-3] == 'f' || actual_end[-3] == 'F')) {
      if (strncmp(actual_end - 2, "128", 3) == 0) {
        actual_end -= 4;
        if (float128_enabled) {
          kind = fk_std_float128;
        } else {
          *err_code = ec_std_float128_not_supported;
          *err_pos = actual_end - 3;
          *severity = strict_ansi_mode ? strict_ansi_error_severity
                                       : es_warning;
          kind = fk_std_float64;
        }  /* if */
      } else if (strncmp(actual_end - 2, "32x", 3) == 0) {
        kind = fk_float32x;
        actual_end -= 4;
      } else if (strncmp(actual_end - 2, "64x", 3) == 0) {
        kind = fk_float64x;
        actual_end -= 4;
      } else if (is_hexadecimal) {
        /* Presumably the 'F'/'f' is a hexadecimal digit and not part of
           the suffix.  Leave the type as double, and don't back up over
           the nonexistent suffix. */
      } else {
        unexpected_condition();
      }  /* if */
    } else if (actual_end > start_of_curr_token + 2 &&
               (actual_end[-2] == 'f' || actual_end[-2] == 'F')) {
      if ((actual_end[-3] == 'b' || actual_end[-3] == 'B') &&
          actual_end[-1] == '1' && *actual_end == '6') {
        kind = fk_std_bfloat16;
        actual_end -= 4;
      } else {
        switch (actual_end[-1]) {
          case '1':
            if (*actual_end == '6') {
              if (cpp23_mode || gnu_version_is(>= 130000) ||
                  clang_version_is(>= 150000)) {
                /* Recent versions of gcc and clang make _Float16 and
                   std::float16_t synonyms. */
                kind = fk_std_float16;
              } else {
                /* Use the pre-C++23 _Float16 type. */
                kind = fk_float16;
              }  /* if */
              actual_end -= 3;
            }  /* if */
            break;
          case '3':
            if (*actual_end == '2') {
              kind = fk_std_float32;
              actual_end -= 3;
            }  /* if */
            break;
          case '6':
            if (*actual_end == '4') {
              kind = fk_std_float64;
              actual_end -= 3;
            }  /* if */
            break;
          default:
            /* Already fk_double. */
            break;
        }  /* switch */
      }  /* if */
    }  /* if */
  } else if (float16_enabled && actual_end > start_of_curr_token + 2 &&
             (actual_end[-2] == 'f' || actual_end[-2] == 'F') &&
             actual_end[-1] == '1' && *actual_end == '6') {
    kind = fk_float16;
    actual_end -= 3;
  } else {
    /* No suffix.  Default is double. */
    kind = (a_float_kind)fk_double;
  }  /* if */
#if GNU_EXTENSIONS_ALLOWED
  if (*actual_end == 'i' || *actual_end == 'I' ||
      *actual_end == 'j' || *actual_end == 'J') {
    /* Check for a suffix indicating an imaginary literal appearing after a
       suffix denoting the floating-point precision.  Note that scan_number
       will have diagnosed cases where two "imaginary literal" suffixes
       appeared. */
    is_imaginary_literal = TRUE;
    --actual_end;
  }  /* if */
#endif /* GNU_EXTENSIONS_ALLOWED */
  if (microsoft_bugs &&
      start_of_curr_token[0] == '.' &&
      isdigit((unsigned char)start_of_curr_token[1]) &&
      start_of_curr_token[2] == '.') {
    /* Microsoft accepts constants like .1.234, and ignores the second
       decimal point and everything after it. */
    actual_end = start_of_curr_token+1;
  }  /* if */
  /* Place a null after the number to guarantee stopping at the right
     point.  If the number has a missing exponent, place a zero exponent
     at the end (this is for the pcc case).  There's always room for at
     least two characters after the floating number, because the number 
     is always followed by at least a newline and null. */
  old_next_char = *(actual_end+1);
  old_next2_char = *(actual_end+2);
  if (*actual_end == 'E' || *actual_end == 'e' ||
      ((*actual_end == '+' || *actual_end == '-') &&
       actual_end != start_of_curr_token &&
       (*(actual_end-1) == 'E' || *(actual_end-1) == 'e'))) {
    /* Missing exponent digits (pcc case); add 0 exponent. */
    *(char *)(actual_end+1) = '0';
    *(char *)(actual_end+2) = '\0';
    last_conversion_char = actual_end + 1;
  } else {
    *(char *)(actual_end+1) = '\0';
    last_conversion_char = actual_end;
  }  /* if */
  if (number_contains_digit_separator) {
    /* We must remove the separators before converting. */
    token_no_separators = remove_digit_separators(start_of_curr_token,
                                                  last_conversion_char);
  } else {
  /* Use the token spelling directly. */
    token_no_separators = start_of_curr_token;
  }  /* if */
  /* Do the conversion. */
  if (is_hexadecimal) {
    fp_hex_string_to_float(kind, token_no_separators, &number, &err, &inexact);
  } else {
    fp_string_to_float(kind, token_no_separators, &number, &err);
  }  /* if */
  *(char *)(actual_end+1) = old_next_char;
  *(char *)(actual_end+2) = old_next2_char;
  if (err) {
    *err_code = ec_bad_float_value;
    *err_pos = start_of_curr_token;
    *severity = es_error;
  } else {
    /* Build a constant with the right type and value. */
#if C99_IL_EXTENSIONS_SUPPORTED
    if (is_imaginary_literal) {
      clear_constant(&const_for_curr_token, (a_constant_repr_kind)ck_complex);
      const_for_curr_token.type = complex_type(kind);
      fp_host_large_integer_to_float(
                     kind, (a_host_large_integer)0,
                     &const_for_curr_token.variant.complex_value->real, &err);
      const_for_curr_token.variant.complex_value->imag = number;
    } else
#endif /* C99_IL_EXTENSIONS_SUPPORTED */
    /* Do not insert code here. */
    {
      clear_constant(&const_for_curr_token, (a_constant_repr_kind)ck_float);
      const_for_curr_token.type = float_type(kind);
      const_for_curr_token.variant.float_value = number;
    }  /* if */
    if (inexact) {
      /* The hex value could not be exactly represented in the specified
         floating point format. */
      a_source_position	pos;
      conv_line_loc_to_source_pos(start_of_curr_token, &pos);
      pos_warning(ec_inexact_fp_conversion, &pos);
    }  /* if */
  }  /* if */
  if (*err_code != ec_no_error && *severity > es_warning) {
    /* Return an error constant. */
    set_error_constant(&const_for_curr_token);
  }  /* if */
}  /* conv_float_literal */


static unsigned long conv_unicode_literal_char(
                                      a_char_conversion_state_ptr state,
                                      unsigned long               unicode_char,
                                      a_boolean                   utf8_literal)
/*
Convert the Unicode character unicode_char to the appropriate
representation in a literal.  Return the first byte of the converted
character and set up state for scanning through the second and following
bytes (if any).  If utf8_literal is TRUE, the character is part of a UTF-8
string literal and is to be converted to UTF-8 rather than being truncated
to a Latin-1 byte.
*/
{
  unsigned      translated_len;
#if UNICODE_SOURCE_SUPPORTED
  a_boolean     is_unicode_source =
                                   (curr_file_unicode_source_kind != usk_none);
#else /* !UNICODE_SOURCE_SUPPORTED */
  a_boolean     is_unicode_source = FALSE;
#endif /* UNICODE_SOURCE_SUPPORTED */

#if NATIVE_MULTIBYTE_CHARS_SUPPORTED_WITH_UNICODE
  if (state->translate_utf8_to_mbc ||
      (!gnu_mode && !microsoft_mode && !is_unicode_source && !utf8_literal)) {
    /* If the emulation (such as some Microsoft modes) requires it, we
       translate Unicode characters to the system default multibyte
       character set.  Except in GNU and Microsoft modes, we also do that
       translation if the source is not Unicode (so that a
       universal-character-name will have the same encoding as the
       surrounding native characters). */
    a_boolean err;
    translated_len = unicode_to_multibyte_char(unicode_char,
                                               state->translated_char, &err);
    if (err) {
      /* The code point could not be converted to a suitable
         representation.  Issue a diagnostic. */
      a_number_buffer num_buf(unicode_char);

      conv_line_loc_to_source_pos(*state->next_token_char, &error_position);
      pos_warning(ec_bad_unicode_char_in_string, &error_position,
                  num_buf.as_temp_characters());
    }  /* if */
  } else
#endif /* NATIVE_MULTIBYTE_CHARS_SUPPORTED_WITH_UNICODE */
  /* Do not insert code here. */
  if (gnu_mode || is_unicode_source || utf8_literal) {
    /* Translate the Unicode character into UTF-8. */
    translated_len = unicode_to_utf8(unicode_char, state->translated_char);
  } else {
    /* Assuming that the target character set is Latin-1, which shares the
       first 256 code points with Unicode, we truncate the character to the
       low-order eight bits and issue a warning if the character is not
       Latin-1. */
    translated_len = 1;
    state->translated_char[0] = (unsigned char)unicode_char;
    if (unicode_char > 0xff) {
      conv_line_loc_to_source_pos(*state->next_token_char, &error_position);
      pos_warning(ec_character_not_latin_1, &error_position);
    }  /* if */
  }  /* if */
  /* Set up for scanning the remaining translated characters and return the
     first. */
  state->remaining_char_count = (int)(translated_len - 1);
  state->next_mbc_char = state->translated_char + 1;
  return state->translated_char[0];
}  /* conv_unicode_literal_char */


static unsigned long create_surrogate_pair(unsigned long               ch,
                                           a_char_conversion_state_ptr state)
/*
If ch is a valid Unicode character requiring a surrogate pair in its UTF-16
encoding, set up *state to buffer the second code unit of the pair and
return the first; otherwise, return ch and leave *state unmodified.
*/
{
  unsigned short encoding[2];
  int            num_code_units;

  num_code_units = ucn_to_utf16(ch, encoding);
  if (num_code_units == 2) {
    /* The character was valid Unicode and resulted in a surrogate pair.
       Return the first code unit now and set up to return the second one
       as the next character.  (If the value was invalid, an error was
       already reported when the character was scanned, so we just return
       the original value.) */
    state->pending_surrogate_pair = encoding[1];
    state->next_mbc_char = NULL;
    state->remaining_char_count = 1;
    ch = encoding[0];
  }  /* if */
  return ch;
}  /* create_surrogate_pair */


void conv_single_char(a_char_conversion_state_ptr state,
                      a_boolean                   process_escapes,
                      unsigned long               *ch,
                      unsigned long               centity_mask,
                      a_boolean                   narrow_literal,
                      a_boolean                   utf8_literal)
/*
Fetch one character of a character constant or string literal.  The current
position in the token is *state->next_token_char (it is incremented
appropriately for what is taken).  Escapes (beginning with "\") are
recognized and processed if process_escapes is TRUE.  The character gotten
is returned (not sign-extended) in ch.  centity_mask defines the size of
the character entity into which this character is going (char, char8_t,
wchar_t, char16_t, or char32_t); narrow_literal is TRUE for
narrow-character string and character literals, and utf8_literal is TRUE
for UTF-8 string and character literals.  When multibyte characters are
enabled and for universal-character-names, each byte of the multibyte
character is returned on a separate call of this routine.
state->remaining_char_count is set to the number of characters remaining to
be extracted on subsequent calls, and serves to disable recognition of
escapes, etc., on bytes after the first in a multibyte character.  The
caller must set state->remaining_char_count to zero before the first call
of this routine in a given string, even if multibyte characters are not
enabled.  When NATIVE_MULTIBYTE_CHARS_SUPPORTED_WITH_UNICODE and
state->translate_utf8_to_mbc are TRUE, the bytes returned for a UTF-8
character will be those of the corresponding character in the system
default locale.  When state->create_surrogate_pairs is TRUE and a character
or universal-character-name is encountered that requires a surrogate pair,
the first code unit is returned by this call and the second code unit is
saved in state->pending_surrogate_pair to be returned on the next call.
When processing C++11 raw string literals, state->next_orig_line_modif may
be non-NULL; if it points to a modification for the current position, the
original character(s) are returned instead of the modified version and
state->next_orig_line_modif is advanced to point to the next modification.
*/
{
  unsigned long targ_ch;
  unsigned char tch;
  a_const_char  *lptr;
  unsigned      digit;
  a_boolean     range_error = FALSE;
  a_boolean     numeric_escape = FALSE;
  a_boolean     unrecognized;
  a_boolean     malformed_err = FALSE;

  lptr = *state->next_token_char;
  if (state->remaining_char_count != 0) {
    /* We are in the middle of a multibyte character sequence or surrogate
       pair started on a previous call of this routine.  Return another
       character (or the second code unit) and decrement the count of
       remaining characters. */
    if (state->next_mbc_char != NULL) {
      /* The Unicode character that was seen was translated into a
         multibyte character, or a trigraph or line splice was reverted in
         a raw string literal; state->next_mbc_char points to the
         translated or reverted byte to return on this call. */
      targ_ch = (unsigned char)*state->next_mbc_char;
      if (state->remaining_char_count == 1) {
        /* This is the last translated/reverted character.  Set the buffer
           pointer to NULL in case a multibyte character follows, which
           will set remaining_char_count to a non-zero value but needs to
           fetch characters from the token, not the next_mbc_char
           buffer. */
        state->next_mbc_char = NULL;
      } else {
        ++state->next_mbc_char;
      }  /* if */
    } else if (state->create_surrogate_pairs) {
      /* The previous call returned the first code unit of a surrogate
         pair.  Return the second code unit now. */
      check_assertion(state->remaining_char_count == 1);
      targ_ch = state->pending_surrogate_pair; /*lint !e530*/
    } else {
      targ_ch = (unsigned char)*lptr;
      lptr++;
    }  /* if */
    --state->remaining_char_count;
    goto return_point;
  }  /* if */
get_another:
  targ_ch = (unsigned char)*lptr;
  if (state->next_orig_line_modif != NULL &&
      state->next_orig_line_modif->line_loc == lptr) {
    /* This is a character that must be restored to its original form
       because it appeared in a raw string literal. */
    an_orig_line_modif_ptr olmp = state->next_orig_line_modif;
    state->next_orig_line_modif = olmp->next;
    olmp->in_raw_string_literal = TRUE;
    switch (olmp->kind) {
      case olm_trigraph:
        /* Reconstruct the original trigraph.  The first '?' will be
           returned on this call, while the remaining two characters are
           put into the translated_char array for future calls. */
        targ_ch = '?';
        state->remaining_char_count = 2;
        state->translated_char[0] = '?';
        state->translated_char[1] = olmp->variant.orig_char;
        state->next_mbc_char = state->translated_char;
        if (olmp->next == NULL || olmp->next->line_loc > lptr) {
          /* Advance the current location only if the next modification
             entry is not at the current location (to allow for cases where
             the backslash in a line splice is represented by a trigraph,
             which will result in an olm_trigraph followed by an
             olm_line_splice at the same location). */
          ++lptr;
        }  /* if */
        break;
      case olm_line_splice:
        /* A line splice is not represented in the source string
           characters, so we don't increment lptr, but we return the '\'
           now and the newline on a subsequent call.. */
        targ_ch = '\\';
        if (olmp->next == NULL || olmp->next->kind != olm_splice_whitespace) {
          /* Only return the newline after any whitespace characters
             following the backslash. */
          state->remaining_char_count = 1;
          state->translated_char[0] = TARG_NEWLINE_CHAR;
          state->next_mbc_char = state->translated_char;
        }  /* if */
        break;
      case olm_multiline_string_splice:
        /* scan_multiline_string inserted the two characters '\' and 'n'
           into the source string to represent the newline.  Skip over
           those characters and just return a newline character. */
        targ_ch = TARG_NEWLINE_CHAR;
        lptr += 2;
        break;
      case olm_null:
        /* A null (0) character in the source was replaced by an LE_NULL
           lexical escape.  Skip over it and just return the null, except
           in Microsoft mode, where the character is ignored. */
        lptr += LE_ESCAPE_LEN;
        if (microsoft_mode) {
          goto get_another;
        } else {
          targ_ch = 0;
        }  /* if */
        break;
      case olm_splice_whitespace:
        /* A whitespace character following the backslash of a line splice
           is not represented in the source string characters, so we don't
           increment lptr, but we return the whitespace character. */
        targ_ch = olmp->variant.orig_char;
        if (olmp->next == NULL || olmp->next->kind != olm_splice_whitespace) {
          /* Return the newline after the last whitespace character
             following the backslash. */
          state->remaining_char_count = 1;
          state->translated_char[0] = TARG_NEWLINE_CHAR;
          state->next_mbc_char = state->translated_char;
        }  /* if */
        break;
      default:
        unexpected_condition();
    }  /* switch */
  } else if (targ_ch == LE_ESCAPE && !state->is_rescan) {
    check_assertion(lptr[1] == LE_NULL);
    /* Null (zero) character, represented as an escape. */
    targ_ch = 0;
    lptr += LE_ESCAPE_LEN;
    /* In Microsoft mode, such characters are thrown away. */
    if (microsoft_mode) goto get_another;
  } else if (targ_ch != '\\' || !process_escapes) {
    /* Normal character (not escaped). */
#if MULTIBYTE_CHARS_IN_SOURCE_SUPPORTED
    if (multibyte_chars_in_source_enabled) {
      /* Determine the size of the multibyte character sequence that begins at
         the current character. */
      a_boolean     err;
      unsigned long wc;
      int           numch = lex_mbc_to_wide_char(lptr, &wc, &err);
      if (err) {
        /* Invalid multibyte character sequence.  Report the error, skip
           over the invalid sequence, and return '?' in place of the bad
           character. */
        conv_line_loc_to_source_pos(lptr, &error_position);
        diagnostic(state->warn_on_invalid_conversion ? es_warning
                                                     : es_discretionary_error,
                   ec_bad_multibyte_char);
        lptr += (a_ptrdiff)numch - 1;
        targ_ch = '?';
#if NATIVE_MULTIBYTE_CHARS_SUPPORTED_WITH_UNICODE
      } else if (curr_file_unicode_source_kind != usk_none &&
                 state->translate_utf8_to_mbc) {
        /* The UTF-8 character in the token must be translated to the system
           default multibyte character set.  Get the wide character Unicode
           character corresponding to the UTF-8 bytes and pass it to
           conv_unicode_literal_char, which will convert the Unicode
           to the appropriate character set and set up the conversion state
           to return subsequent bytes of the multibyte character. */
        (void)mbc_to_wide_char(lptr, &wc, (a_boolean *)NULL,
                               /*is_native=*/FALSE);
        lptr += (a_ptrdiff)numch - 1;
        targ_ch = conv_unicode_literal_char(state, wc, /*utf8_literal=*/FALSE);
#endif /* NATIVE_MULTIBYTE_CHARS_SUPPORTED_WITH_UNICODE */
      } else if (utf8_literal) {
        /* This is a character in a UTF-8 literal, which could be a
           multibyte character, either UTF-8 or a native character set:
           convert it to Unicode and then to UTF-8, returning the first (or
           only) byte. */
        lptr += (a_ptrdiff)numch - 1;
        targ_ch = conv_unicode_literal_char(state, wc, /*utf8_literal=*/TRUE);
      } else {
        /* No translation required, just set up *state to return the bytes
           of the character one at a time directly from the input. */
        state->remaining_char_count = numch - 1;
      }  /* if */
    } else
#endif /* MULTIBYTE_CHARS_IN_SOURCE_SUPPORTED */
    /* Do not insert code here. */
    if (utf8_literal) {
      /* This is a Latin-1 character (one byte) in a UTF-8 literal.
         Convert it to UTF-8 and return the first (or only) byte. */
      targ_ch = conv_unicode_literal_char(state, targ_ch,
                                          /*utf8_literal=*/TRUE);
    }  /* if */
    lptr++;
  } else {
    /* Backslash, escaped character.  Can be an octal escape, a hexadecimal
       escape, a simple escape sequence (like \n), a
       universal-character-name, or something unrecognized, in which case
       the character is left alone. */
    a_const_char *start_of_escape = lptr++;
    unrecognized = FALSE;
    switch ((int)(tch = (unsigned char)*(lptr++))) {
      case 'a':
        if (C_dialect == C_dialect_pcc) {
          /* pcc does not recognize \a. */
          unrecognized = TRUE;
        } else {
          targ_ch = (unsigned char)TARG_ALERT_CHAR;
        }  /* if */
        break;
      case 'b':
        targ_ch = (unsigned char)TARG_BACKSPACE_CHAR;
        break;
#if GNU_EXTENSIONS_ALLOWED
      case 'e':
      case 'E':
        if (gnu_mode) {
          /* GNU mode \e or \E stands for the ASCII "ESC" character. */
          targ_ch = (unsigned char)TARG_ESC_CHAR;
        } else {
          /* Most modes do not recognize this escape sequence. */
          unrecognized = TRUE;
        }  /* if */
        break;
#endif /* GNU_EXTENSIONS_ALLOWED */
      case 'f':
        targ_ch = (unsigned char)TARG_FORM_FEED_CHAR;
        break;
      case 'n':
        targ_ch = (unsigned char)TARG_NEWLINE_CHAR;
        break;
      case 'r':
        targ_ch = (unsigned char)TARG_CARR_RETURN_CHAR;
        break;
      case 't':
        targ_ch = (unsigned char)TARG_HORIZ_TAB_CHAR;
        break;
      case 'v':
        /* \v is not in K&R, but is recognized by pcc. */
        targ_ch = (unsigned char)TARG_VERT_TAB_CHAR;
        break;
      case 'u':
      case 'U':
        /* A universal character name.  Back the pointer up to the position
           of the backslash. */
        /* Universal characters are allowed in C++ and C99. */
        if (!universal_character_names_allowed) goto other_chars;
        lptr = start_of_escape;
        targ_ch = scan_universal_character(&lptr,
                                           /*is_identifier=*/FALSE,
                                           /*is_identifier_start=*/FALSE,
                                           /*issue_diagnostics=*/TRUE,
                                           &malformed_err);
        if (malformed_err) {
          /* There was a malformed escape sequence.  If a delimited literal
             was empty, add a null character; otherwise, skip the '\' and
             treat the rest of the sequence as having no special
             significance. */
          if (lptr != start_of_escape + 4 || targ_ch != 0) {
            targ_ch = (unsigned char)start_of_escape[1];
            lptr = start_of_escape + 2;
          }  /* if */
        }  /* if */
        if (!narrow_literal) {
          /* This is for a wide character or wide string literal.  Return
             the value directly, subject to the range constraints implied
             by centity_mask. */
          goto range_check;
        } else {
          /* Convert the Unicode character specified by the
             universal-character-name to either UTF-8 or the system default
             multibyte character set as appropriate and set up the
             conversion state to return subsequent bytes of the resulting
             character. */
          targ_ch = conv_unicode_literal_char(state, targ_ch, utf8_literal);
        }  /* if */
        break;
      case 'N':
        /* A named Unicode character.  Move the pointer back to the start
           of the construct, i.e., to the '\' in "\N{". */
        if (!named_unicode_chars_allowed) goto other_chars;
        lptr = start_of_escape;
        targ_ch = scan_named_unicode_char(&lptr,
                                          /*is_identifier=*/FALSE,
                                          /*is_identifier_start=*/FALSE,
                                          /*issue_diagnostics=*/TRUE,
                                          /*update_pos_on_error=*/FALSE);
        if (targ_ch > MAX_UNICODE_VAL) {
          /* An error occurred and lptr was not updated.  Treat the 'N' and
             following as ordinary characters. */
          ++lptr;
          goto other_chars;
        }  /* if */
        if (!narrow_literal) {
          /* This is for a wide character or wide string literal.  Return
             the value directly, subject to the range constraints implied
             by centity_mask. */
          goto range_check;
        } else {
          /* Convert the named Unicode character to either UTF-8 or the
             system default multibyte character set as appropriate and set
             up the conversion state to return subsequent bytes of the
             resulting character. */
          targ_ch = conv_unicode_literal_char(state, targ_ch, utf8_literal);
        }  /* if */
        break;
      case 'o':
        if (*lptr != '{' || !delimited_escape_seqs_allowed) {
          /* A plain "\o" has no special meaning. */
          goto other_chars;
        }  /* if */
        /* Octal escape, which has an unlimited number of octal characters,
           terminated by a '}'. */
        targ_ch = 0;
        while (isdigit((unsigned char)*++lptr) &&
               *lptr != '8' && *lptr != '9') {
          if (targ_ch > (ULONG_MAX>>3)) {
            /* Error will be processed below.  We must keep going and take
               all the digits. */
            range_error = TRUE;
          }  /* if */
          targ_ch = (targ_ch << 3) | (unsigned char)(*lptr - '0');
        }  /* while */
        if (*lptr == '}') {
          /* Normal termination.  Skip over the closing '}'. */
          numeric_escape = TRUE;
          ++lptr;
          goto range_check;
        } else {
          /* Unterminated delimited escape sequence.  The error will have
             already been reported.  Treat the malformed sequence as an
             ordinary sequence of characters beginning with the '\'. */
          targ_ch = (unsigned char)'\\';
          lptr = start_of_escape + 1;
        }  /* if */
        break;
      case 'x':
        /* Hexadecimal escape.  There can be many digits, but there must be
           at least one.  If not, treat as just "x". */
        if (delimited_escape_seqs_allowed && *lptr == '{') {
          /* A delimited escape sequence, which has an unlimited number of
             hexadecimal characters, terminated by a '}'. */
          targ_ch = 0;
          while (isxdigit(*++lptr)) {
            if (targ_ch > (ULONG_MAX>>4)) {
              /* Error will be processed below.  We must keep going and take
                 all the digits. */
              range_error = TRUE;
            }  /* if */
            targ_ch = (targ_ch << 4) | hexvalue((unsigned char)(*lptr));
          }  /* while */
          if (*lptr == '}') {
            /* Normal termination.  Skip over the closing '}'. */
            numeric_escape = TRUE;
            ++lptr;
            goto range_check;
          } else {
            /* Unterminated delimited escape sequence.  The error will have
               already been reported.  Treat the malformed sequence as an
               ordinary sequence of characters beginning with the '\'. */
            targ_ch = (unsigned char)'\\';
            lptr = start_of_escape + 1;
          }  /* if */
        } else if (!isxdigit((unsigned char)*lptr)) {
          conv_line_loc_to_source_pos(*state->next_token_char+2,
                                      &error_position);
          if (C_dialect == C_dialect_pcc || SVR4_C_mode) {
            pos_warning(ec_bad_hex_digit, &error_position);
          } else {
            pos_error(ec_bad_hex_digit, &error_position);
          }  /* if */
          targ_ch = (unsigned char)'x';
        } else {
          numeric_escape = TRUE;
          targ_ch = hexvalue((unsigned char)(*lptr));  /* First digit. */
          while (tch = (unsigned char)(*(++lptr)), isxdigit(tch)) {
            if (targ_ch > (ULONG_MAX>>4)) {
              /* Error will be processed below.  We must keep going and take
                 all the digits. */
              range_error = TRUE;
            }  /* if */
            digit = hexvalue(tch);
            targ_ch = (targ_ch << 4) | digit;
          }  /* while */
          goto range_check;
        }  /* if */
        break;
      case '0':  case '1':  case '2':  case '3':
      case '4':  case '5':  case '6':  case '7':
        /* Octal escape.  Note that neither ANSI nor pcc recognizes 8 and 9
           as "octal" in this context.  Up to three octal digits may appear.
           Note that there is code in accum_quoted_string that must match
           this code. */
        numeric_escape = TRUE;
        targ_ch = tch - '0';  /* First digit. */
        tch = (unsigned char)*lptr;
        if (isdigit(tch) && tch != '8' && tch != '9') {
          /* Second digit. */
          targ_ch = (targ_ch << 3) | (tch - '0');
          lptr++;
          tch = (unsigned char)*lptr;
          if (isdigit(tch) && tch != '8' && tch != '9') {
            /* Third digit. */
            lptr++;
            targ_ch = (targ_ch << 3) | (tch - '0');
          }  /* if */
        }  /* if */
        goto range_check;
      default:
other_chars:
        /* Other characters, left alone.  Specifically, standard requires
           that \', \", \?, and \\ be reduced to just the escaped character. */
        if (tch == '\'' || tch == '"' || tch == '?' || tch == '\\') {
          targ_ch = tch;
        } else if (tch == 'N' && named_unicode_chars_allowed) {
          /* There was already an error issued for an incorrect named
             Unicode character escape; a warning about an unrecognized
             escape would be superfluous. */
          targ_ch = tch;
        } else {
          unrecognized = TRUE;
        }  /* if */
        break;
    }  /* switch */
    /* Unrecognized escapes cause a warning but translate to the escaped
       character. */
    if (unrecognized) {
      conv_line_loc_to_source_pos(*state->next_token_char, &error_position);
      pos_warning(ec_unrecognized_char_escape, &error_position);
      targ_ch = tch;
    }  /* if */
  }  /* if */
return_point:
  /* Drop out-of-range bits. */
  targ_ch &= centity_mask;
  *ch = targ_ch;
  *state->next_token_char = lptr;
  return;

range_check:
  /* Check that the value of c is representable in the target character
     type. */
  if (!range_error) {
    /* The comparison here is always done as unsigned, even if char or
       wchar_t are signed.  That's because octal and hexadecimal escapes
       are always treated as unsigned.  See 3.1.3.4 constraints. */
    /* Use masking for the check so that this will work when the target
       char is larger than the host.  In that case, with the current limited
       implementation, there can be "holes" in the middle of wide character
       constants, and those holes shouldn't contain any "1" bits. */
    if ((targ_ch & ~centity_mask) != 0) {
      if (state->create_surrogate_pairs && !numeric_escape) {
        /* The target character type is such that an overflow should be
           handled by creating a UTF-16 surrogate pair rather than as a
           warning or error. */
        targ_ch = create_surrogate_pair(targ_ch, state);
      } else {
        range_error = TRUE;
      }  /* if */
    }  /* if */
  }  /* if */
  if (range_error) {
    /* A range error occurring in an octal or hexadecimal escape is
       classified as an error by the C Standard and, after adoption of
       paper P1854R4, by the C++ Standard as well, although that is not
       enforced by gcc. */
   an_error_severity sev =
                          gnu_version_is(any_version) ? es_warning
                                                      : es_discretionary_error;
    conv_line_loc_to_source_pos(*state->next_token_char, &error_position);
    if (narrow_literal) {
      /* Register the overflow instead of reporting it immediately, since
         it might be acceptable if string concatenation changes the literal
         kind. */
      register_char_overflow(sev, ec_bad_character_value, &error_position);
    } else {
      diagnostic(sev, ec_bad_character_value);
    }  /* if */
  }  /* if */
  goto return_point;
}  /* conv_single_char */


static void conv_single_wide_char(a_char_conversion_state_ptr state,
                                  a_boolean                   process_escapes,
                                  unsigned long               *ch,
                                  unsigned long               centity_mask)
/*
Fetch one wide character of a wide character constant or string literal
(here, a "wide character" can be a wchar_t, a char16_t, or a char32_t).
The current position in the token is *state->next_token_char (it is
incremented appropriately for what is taken).  More than one source
character may be taken to produce one wide character as output.  The wide
character gotten is returned (not sign-extended) in ch.  centity_mask
defines the size of character.
*/
{
#if !MULTIBYTE_CHARS_IN_SOURCE_SUPPORTED
  /* Simple version: no multibyte characters to consider. */
  conv_single_char(state, process_escapes, ch, centity_mask,
                   /*narrow_literal=*/FALSE, /*utf8_literal=*/FALSE);
#else /* MULTIBYTE_CHARS_IN_SOURCE_SUPPORTED */
  /* Multibyte character processing may be needed. */
  if ((!multibyte_chars_in_source_enabled && !state->force_utf8) ||
      (process_escapes && **state->next_token_char == '\\') ||
      **state->next_token_char == LE_ESCAPE ||
      state->remaining_char_count > 0 ||
      (state->next_orig_line_modif != NULL &&
       state->next_orig_line_modif->line_loc == *state->next_token_char)) {
    /* Use simple routine if multibyte characters are disabled or if
       the character is an escape or the result of a modification. */
    conv_single_char(state, process_escapes, ch, centity_mask,
                     /*narrow_literal=*/FALSE, /*utf8_literal=*/FALSE);
  } else {
    unsigned  long wc;
    int       numch;
    a_boolean err;
    a_boolean is_native;
    if (state->force_utf8) {
      is_native = FALSE;
    } else {
#if UNICODE_SOURCE_SUPPORTED
      is_native = curr_file_unicode_source_kind == usk_none;
#else /* !UNICODE_SOURCE_SUPPORTED */
      is_native = FALSE;
#endif /* UNICODE_SOURCE_SUPPORTED */
    }  /* if */
    /* Convert a multibyte character sequence to a wide character. */
    numch = mbc_to_wide_char(*state->next_token_char, &wc, &err, is_native);
    if (err) {
      if (state->is_rescan) {
        /* This is a rescan of a previously-processed string literal, so
           presumably this character resulted from an octal or hexadecimal
           escape, not an actual extended character or universal character
           name.  Just take the single character. */
        wc = (unsigned char)**state->next_token_char;
        numch = 1;
      } else {
        /* Invalid multibyte character sequence.  Report an error and
           replace the character with '?'. */
        conv_line_loc_to_source_pos(*state->next_token_char, &error_position);
        diagnostic(state->warn_on_invalid_conversion ? es_warning
                                                     : es_discretionary_error,
                   ec_bad_multibyte_char);
        wc = L'?';
      }  /* if */
    }  /* if */
    if ((wc & ~centity_mask) != 0 && state->create_surrogate_pairs) {
      /* The character does not fit into a single code unit.  Create a
         UTF-16 surrogate pair, buffering the second code unit in *state
         and returning the first as the result of this call. */
      wc = create_surrogate_pair(wc, state);
    }  /* if */
    *ch = wc;
    *state->next_token_char += numch;
  }  /* if */
#endif /* !MULTIBYTE_CHARS_IN_SOURCE_SUPPORTED */
}  /* conv_single_wide_char */


void conv_char_literal(unsigned long num_chars,
                       an_error_code *err_code,
                       a_const_char  **err_pos)
/*
Convert a character constant from external form to internal form.
start_of_curr_token and end_of_curr_token point to the two ends of the
external form.  The internal form is placed in const_for_curr_token.  If
there is no error, *err_code is set to ec_no_error (which is 0); otherwise,
*err_code is set to an appropriate error code and *err_pos is set to the
character position of the error.  (Note that conv_single_char may report
errors using register_char_overflow, so the caller should call
report_char_overflows after calling conv_char_literal.)  num_chars
indicates the number of characters contained within the quotes (after
escape processing, and in wide characters if the constant is wide).  If the
literal contains a UCN, the actual number of converted characters may be
less than num_chars.  */
{
  unsigned long           i, ch;
  an_integer_value        number, ch_int_val;
  a_const_char            *temp_ptr;
  a_boolean               err, too_many_chars = FALSE, bad_character = FALSE;
  a_type_ptr              con_type = NULL;
  unsigned int            char_size;
  unsigned long           centity_mask;
  a_boolean               centity_is_signed = FALSE;
  unsigned                centity_bits = 0;
  int                     encoding_length;
  a_character_kind        character_kind = (a_character_kind)ck_last;
  a_char_conversion_state conv_state;
  a_boolean               utf8_literal = FALSE;
  a_boolean               char_too_wide_for_rep = FALSE;
  a_const_char            *char_start;
  a_const_char            *mbc_loc = NULL;

  /* Determine the constant type as follows:
       Single-character constant     ('x'): int in C, char in C++
       UTF-8 character constant    (u8'x'): char (C++17) or char8_t (C++20)
                                            or unsigned char (C23)
       Multi-character constant     ('xy'): int
       Wide character constant      (L'x'): wchar_t
       char16_t character constant  (u'x'): char16_t
       char32_t character constant  (U'x'): char32_t
     Multi-character wide-character literals don't really make sense, but we
     do allow them (with a warning) for wchar_t literals because the C
     standard says it is implementation-defined, and several test suites have
     something like L'ab' in them.
  */
  switch (*start_of_curr_token) {
    case '\'':
      /* Normal character literal (single or multi). */
      character_kind = (a_character_kind)chk_char;
      char_size = 1;
      centity_bits = targ_char_bit;
      centity_is_signed = targ_has_signed_chars; 
      temp_ptr = start_of_curr_token+1;
      if (C_mode() || num_chars > 1) {
        /* Character constants in C have type int, as do multi-character
           literals in C++.  In the case of a C++ literal containing a UCN
           that translates to a single character, the type will be adjusted
           to char below after the value is known. */
        con_type = integer_type((an_integer_kind)ik_int);
      } else {
        /* A single-character constant in C++. */
        con_type = integer_type((an_integer_kind)ik_char);
      }  /* if */
      break;
    case 'L':
      /* Wide character literal. */
      character_kind = (a_character_kind)chk_wchar_t;
      char_size = (unsigned int)targ_sizeof_wchar_t;
      centity_bits = char_size*targ_char_bit;
      centity_is_signed = int_kind_is_signed[(int)targ_wchar_t_int_kind];
      con_type = eff_wchar_t_type();
      temp_ptr = start_of_curr_token+2;
      break;
    case 'U':
      /* char32_t character literal. */
      character_kind = (a_character_kind)chk_char32_t;
      char_size = (unsigned int)targ_sizeof_char32_t;
      centity_bits = char_size*targ_char_bit;
      centity_is_signed = FALSE; 
      con_type = eff_char32_t_type();
      temp_ptr = start_of_curr_token+2;
      break;
    case 'u':
      if (start_of_curr_token[1] == '8') {
        /* UTF-8 character literal. */
        utf8_literal = TRUE;
        char_size = 1;
        centity_bits = targ_char_bit;
        temp_ptr = start_of_curr_token + 3;
        if (c23_mode) {
          /* C23 UTF-8 character literals have type unsigned char. */
          character_kind = chk_char;
          centity_is_signed = FALSE;
          con_type = integer_type(ik_unsigned_char);
        } else if (char8_t_enabled) {
          /* C++20 UTF-8 character literals have type char8_t. */
          character_kind = chk_char8_t;
          centity_is_signed = FALSE;
          con_type = eff_char8_t_type();
        } else {
          /* C++17 UTF-8 character literals have type char. */
          character_kind = chk_char;
          centity_is_signed = targ_has_signed_chars;
          con_type = integer_type(ik_char);
        }  /* if */
      } else {
        /* char16_t character literal. */
        character_kind = (a_character_kind)chk_char16_t;
        char_size = (unsigned int)targ_sizeof_char16_t;
        /* Do not use a mask for char16_t characters at this time.  Any masking
           operation is the responsibility of the encoding (invoked through the
           encode_in_char16_t macro). */
        centity_bits = sizeof(unsigned long)*CHAR_BIT;
        centity_is_signed = FALSE; 
        con_type = eff_char16_t_type();
        temp_ptr = start_of_curr_token+2;
      }  /* if */
      break;
    default:
      unexpected_condition();
  }  /* switch */
  centity_mask = (unsigned long)1 << (centity_bits-1);
  centity_mask = centity_mask | (centity_mask - 1);
  /* UTF-8 characters should be translated to multibyte characters only
     for narrow-character literals in Microsoft mode in non-Unicode source
     files. */
  a_boolean translate_utf8_to_mbc =
                               (character_kind == (a_character_kind)chk_char &&
                                microsoft_mode
#if UNICODE_SOURCE_SUPPORTED
                                && curr_file_unicode_source_kind == usk_none
#endif /* UNICODE_SOURCE_SUPPORTED */
                                                                            );
  clear_char_conversion_state(&conv_state, &temp_ptr, translate_utf8_to_mbc);
#if MULTIBYTE_CHARS_IN_SOURCE_SUPPORTED
  /* Initialize for scanning multibyte characters in the string. */
  mbc_scan_init_if_multibyte_chars_in_source_enabled();
#endif /* MULTIBYTE_CHARS_IN_SOURCE_SUPPORTED */
  set_unsigned_integer_value(&number, (a_host_large_unsigned)0);
  if (microsoft_mode && temp_ptr <= end_of_curr_token - LE_ESCAPE_LEN &&
      *temp_ptr == LE_ESCAPE) {
    temp_ptr = skip_embedded_null_escapes(temp_ptr, end_of_curr_token);
  }  /* if */
  /* Accumulate the characters.  A wide literal with no characters (L'')
     is possible in Microsoft mode and must produce a zero value. */
  /*lint -e{440}*/
  for (i = 0;
       temp_ptr < end_of_curr_token || conv_state.remaining_char_count > 0;
       ++i) {
    /* Convert one character of the char constant. */
    switch (character_kind) {
      case chk_char:
      case chk_char8_t:
        char_start = temp_ptr;
        conv_single_char(&conv_state, /*process_escapes=*/TRUE, &ch,
                         centity_mask, /*narrow_literal=*/TRUE, utf8_literal);
        if (!utf8_literal && conv_state.remaining_char_count != 0 &&
            !char_too_wide_for_rep && !gnu_version_is(any_version) &&
            !ms_version_is(any_version)) {
          /* An ordinary narrow character literal cannot contain a
             multi-byte character, even if it fits in an int. */
          char_too_wide_for_rep = TRUE;
          mbc_loc = char_start;
        }  /* if */
        if ((i >= targ_sizeof_int && !gnu_version_is(any_version) &&
             !clang_version_is(any_version)) ||
            (utf8_literal && i != 0 && !clang_version_is(any_version))) {
          /* GNU and clang compilers accept overlong literals, simply
             discarding any leading characters that do not fit.  Otherwise
             (including the case of a UTF-8 literal with more than a single
             converted character, except in clang mode), flag this as an
             error. */
          too_many_chars = TRUE;
        }  /* if */
        break;
      case chk_wchar_t:
        conv_single_wide_char(&conv_state, /*process_escapes=*/TRUE, &ch,
                              centity_mask);
        if (i != 0) {
          if (C_mode()) {
            /* The value of a multi-character L'...' literal is truncated
               to the first character in C. */
            continue;
          } else if (gnu_version_is(any_version) ||
                     clang_version_is(<140000) || ms_version_is(any_version)) {
            /* Accepted with a warning by gcc, MSVC, and older versions of
               clang, with the value being the last character in the
               literal. */
          } else {
            too_many_chars = TRUE;
          }  /* if */
        }  /* if */
        break;
      case chk_char16_t:
        char_start = temp_ptr;
        conv_single_wide_char(&conv_state, /*process_escapes=*/TRUE, &ch,
                              centity_mask);
        if (i != 0 && !C_mode()) {
          too_many_chars = TRUE;
        } else {
          unsigned short char16_t_vals[MAX_CHAR16_T_ENCODING_LENGTH];
          encoding_length = encode_in_char16_t(ch, char16_t_vals);
          if (encoding_length == 1) {
            /* Normal case. */
            ch = (unsigned long)char16_t_vals[0];
          } else {
            /* ch contained a character code that cannot be encoded in a
               single char16_t character. */
            if (gnu_version_is(any_version) && char_start[0] == '\\' &&
                char_start[1] == 'x') {
              /* Hexadecimal escapes with too many characters are accepted
                 by gcc with a warning. */
            } else {
              bad_character = TRUE;
            }  /* if */
            if (encoding_length > 1) {
              /* Use the low-order code unit. */
              ch = (unsigned long)char16_t_vals[encoding_length - 1];
            }  /* if */
          }  /* if */
        }  /* if */
        break;
      case chk_char32_t:
        conv_single_wide_char(&conv_state, /*process_escapes=*/TRUE, &ch,
                              centity_mask);
        if (i != 0 && !C_mode() && !gnu_version_is(<100000)) {
          too_many_chars = TRUE;
        }  /* if */
        break;
      default:
        unexpected_condition();
    }  /* switch */
    if (i != 0 && (character_kind == chk_wchar_t ||
                   character_kind == chk_char16_t ||
                   character_kind == chk_char32_t)) {
      /* Ignore any preceding characters and just take the last one. */
      set_unsigned_integer_value(&number, (a_host_large_unsigned)0);
    }  /* if */
    /* Put the character in the right place. */
    set_unsigned_integer_value(&ch_int_val, (a_host_large_unsigned)ch);
    if (targ_char_constant_first_char_most_significant) {
      /* 'ab' == 0x6162. */
      /* Do sign extension if necessary, but only on the first character. */
      if (i == 0 && centity_is_signed) {
        sign_extend_integer_value(&ch_int_val, centity_bits);
      }  /* if */
      shift_left_integer_value(&number, (int)centity_bits, &err);
    } else {
      /* 'ab' == 0x6261. */
      /* Do sign extension on the new character if necessary. */
      if (centity_is_signed) {
        sign_extend_integer_value(&ch_int_val, centity_bits);
      }  /* if */
      if (i != 0) {
        /* Drop any sign extension on the previous value if this isn't the
           first character. */
        if (centity_is_signed) {
          an_integer_value mask;
          make_integer_value_mask(&mask, i * centity_bits);
          and_integer_values(&number, &mask);
        }  /* if */
        shift_left_integer_value(&ch_int_val, (int)(i * centity_bits), &err);
      }  /* if */
    }  /* if */
    or_integer_values(&number, &ch_int_val);
    if (microsoft_mode && temp_ptr <= end_of_curr_token - LE_ESCAPE_LEN &&
        *temp_ptr == LE_ESCAPE) {
      temp_ptr = skip_embedded_null_escapes(temp_ptr, end_of_curr_token);
    }  /* if */
  }  /* for */
  if (character_kind != (a_character_kind)chk_char32_t &&
      num_chars > 1 && i == 1) {
    /* A universal-character-name might potentially represent a number of
       bytes, so num_chars was set conservatively to allow for that case.
       If it actually turned out to represent a single character, update
       the character count and constant type accordingly. */
    if (character_kind == (a_character_kind)chk_char && !C_mode()) {
      con_type = integer_type((an_integer_kind)ik_char);
    }  /* if */
    num_chars = 1;
  }  /* if */
  if (bad_character) {
    if (C_mode() || gnu_version_is(<100000)) {
      /* In C, a character that cannot be represented in one UTF-16 code
         unit has an implementation-defined value, typically the low-order
         16 bits.  Early versions of gcc followed the same rule.  Issue a
         warning about the truncation. */
      conv_line_loc_to_source_pos(start_of_curr_token, &error_position);
      pos_warning(ec_utf16_char_lit_too_long, &error_position);
      *err_code = ec_no_error;
      *err_pos = NULL;
    } else {
      /* An unrepresentable character is an error in C++. */
      *err_code = ec_no_char16_t_representation;
      *err_pos = start_of_curr_token + 2;
      /* Return an error constant. */
      set_error_constant(&const_for_curr_token);
    }  /* if */
  } else if (too_many_chars) {
    if (utf8_literal) {
      *err_code = ec_utf8_char_lit_too_long;
    } else {
      *err_code = ec_too_many_characters;
    }  /* if */
    *err_pos = start_of_curr_token;
    /* Return an error constant. */
    set_error_constant(&const_for_curr_token);
  } else {
    *err_code = ec_no_error;
    *err_pos = NULL;
    if (num_chars > 1) {
      /* An ordinary narrow character literal containing a multi-byte
         character is ill-formed, as is a wide character (wchar_t) literal
         in C23 mode containing more than a single character.  Otherwise, a
         character literal with more than one character produces an
         implementation-defined value.  Issue a warning in the well-formed
         cases.  The "too many characters" warning is used for wide
         characters as this is unlikely to produce a meaningful result. */
      an_error_code     wcode;
      an_error_severity sev;

      if (char_too_wide_for_rep) {
        wcode = ec_char_too_wide_for_rep;
        sev = es_discretionary_error;
        conv_line_loc_to_source_pos(mbc_loc, &error_position);
      } else {
        wcode = (character_kind != chk_char) ? ec_too_many_characters
                                             : ec_multi_char_literal;
        sev = (character_kind == chk_wchar_t && cpp23_mode &&
               strict_ansi_mode) ? strict_ansi_discretionary_severity
                                 : es_warning;
        conv_line_loc_to_source_pos(start_of_curr_token, &error_position);
      }  /* if */
      if (!char_too_wide_for_rep && gnu_mode && i > targ_sizeof_int) {
        /* Truncate the value and warn about discarded characters. */
        an_integer_value int_mask;
        make_integer_value_mask(&int_mask, size_t_arg(targ_sizeof_int *
                                                      targ_char_bit));
        and_integer_values(&number, &int_mask);
        wcode = ec_leading_character_ignored_in_char_literal;
      }  /* if */
      pos_diagnostic(sev, wcode, &error_position);
    }  /* if */
  }  /* if */
  if (*err_code == ec_no_error) {
    clear_constant(&const_for_curr_token, (a_constant_repr_kind)ck_integer);
    const_for_curr_token.type = con_type;
    const_for_curr_token.variant.integer_value = number;
    const_for_curr_token.character_kind = character_kind;
  }  /* if */
}  /* conv_char_literal */


static void put_wide_char_into_string(unsigned long  ch,
                                      char           **pstr,
                                      unsigned int   char_size)
/*
Put the wide character (wchar_t, char16_t, or char32_t) ch into the string
pointed to by *pstr, and increment *pstr by the proper amount.  char_size
specifies the number of bytes in a wide character.
*/
{
  unsigned int  i;
  char          *p = *pstr;

  /* This is basically a copy of an integer to an array of characters;
     we must allow for the target endian-ness. */
  if (targ_little_endian) {
    for (i = 0; i < char_size; i++) {
      *p++ = (char)(ch & UCHAR_MAX);
      ch >>= targ_char_bit;
    }  /* for */
  } else {
    for (i = 0; i < char_size; i++) {
      *p++ = (char)((ch >> ((char_size - i - 1) * targ_char_bit)) & UCHAR_MAX);
    }  /* for */
  }  /* if */
  *pstr = p;
}  /* put_wide_char_into_string */


template<a_string_or_char_literal_kind a_Prefix_kind>
static inline void conv_string_literal_chars(
                        ARG_UNUSED char           **result_str_start,
                        char                      **result_str_next_ch,
                        a_const_char              **string_next_char,
                        a_const_char              **end_of_string_value,
                        const unsigned int        &char_size,
                        ARG_UNUSED const sizeof_t &constant_size,
                        a_char_conversion_state   &conv_state,
                        unsigned long             centity_mask,
                        a_boolean                 process_escapes,
                        a_boolean                 is_raw_string,
                        int                       raw_str_trigraph_delim_chars)
/*
This function exists as an implementation detail of conv_string_literal; it
should not be used directly outside of the aforementioned function.

The optimization here works by reducing loop complexity for prefix kinds other
than the one this function was instantiated with (effectively generating an
optimized version of the conversion loop for each character kind).

The start and end of the result string are represented by *result_str_start and
*result_str_next_ch respectively.  Similarly, the start and end of the string
being converted to a string literal are represented by *string_next_char and
*end_of_string_value respectively.  char_size is the size of a character for
the current prefix.  constant_size is the maximum number of bytes from
result_str_start available to write into.  conv_state is the preinitialized
character conversion state.  centity_mask and processing_escapes are forwarded
to conv_single_char (see that function for more information).  is_raw_string
should be TRUE if the full literal kind specified SCLK_RAW_STRING_LITERAL.
raw_str_trigraph_delim_chars is used to account for extra characters in the
input string (if any).
*/
{
  a_boolean inside_char = FALSE;
  /* Check if the loop below should consider character width errors ahead of
     time as an optimization. */
  a_boolean consider_char_width = (a_Prefix_kind == SCLK_ORDINARY_LITERAL &&
                                   !is_raw_string && strict_ansi_mode);

  /* Accumulate the characters.  Loop until we reach the indicated end of the
     string value.  The loop is extended while characters are pending, either
     because a multibyte character is in process, or because of the
     pathological ']' trigraph case mentioned in conv_string_literal, or
     because a raw string literal ended with a line splice that must be
     expanded. */
  while ((*string_next_char < (*end_of_string_value +
                               raw_str_trigraph_delim_chars)) ||
         conv_state.remaining_char_count > raw_str_trigraph_delim_chars ||
         (conv_state.next_orig_line_modif != NULL &&
          (conv_state.next_orig_line_modif->kind == olm_line_splice ||
           conv_state.next_orig_line_modif->kind == olm_splice_whitespace) &&
          conv_state.next_orig_line_modif->line_loc == *string_next_char)) {
    unsigned long ch;
    check_assertion(*result_str_next_ch < *result_str_start + constant_size);
    /* Convert one character of the string literal. */
    /* coverity[switch_selector_expr_is_constant] */
    switch (a_Prefix_kind) {
      case SCLK_ORDINARY_LITERAL:
      case SCLK_UTF8_LITERAL:
        { a_const_char *char_start = *string_next_char;

          conv_single_char(
                &conv_state, process_escapes, &ch, centity_mask,
                /*narrow_literal=*/TRUE, (a_Prefix_kind == SCLK_UTF8_LITERAL));
          /* Coverity: coverity picks up on the dead code elimination that is
             the goal of the optimization described above. */
          /* coverity[dead_error_line] */ /* coverity[dead_error_condition] */
          if (consider_char_width && conv_state.remaining_char_count != 0 &&
              !inside_char) {
            /* The character is too wide to fit in a single char. */
            a_source_position pos;
            conv_line_loc_to_source_pos(char_start, &pos);
            register_char_overflow(es_discretionary_error,
                                   ec_char_too_wide_for_rep, &pos);
          }  /* if */
          inside_char = (conv_state.remaining_char_count != 0);
          *(*result_str_next_ch)++ = (char)ch;
        }
        break;
      case SCLK_WIDE_LITERAL:
      case SCLK_CHAR16_T_LITERAL:
      case SCLK_CHAR32_T_LITERAL:
        conv_single_wide_char(&conv_state, process_escapes, &ch, centity_mask);
        put_wide_char_into_string(ch, result_str_next_ch, char_size);
        break;
      default:
        unexpected_condition();
    }  /* switch */
    if (microsoft_mode &&
        *string_next_char <=
         *end_of_string_value + raw_str_trigraph_delim_chars - LE_ESCAPE_LEN &&
        **string_next_char == LE_ESCAPE) {
      *string_next_char = skip_embedded_null_escapes(
                          *string_next_char,
                          *end_of_string_value + raw_str_trigraph_delim_chars);
    }  /* if */
  }  /* while */
  /* Add the final null. */
  check_assertion(*result_str_next_ch < *result_str_start + constant_size);
  /* coverity[switch_selector_expr_is_constant] */
  switch (a_Prefix_kind) {
    case SCLK_ORDINARY_LITERAL:
    case SCLK_UTF8_LITERAL:
      /* Narrow string literal. */
      *((*result_str_next_ch)++) = '\0';
      break;
    case SCLK_WIDE_LITERAL:
    case SCLK_CHAR16_T_LITERAL:
    case SCLK_CHAR32_T_LITERAL:
      /* L"...", u"...", or U"...": */
      { unsigned long ch = 0;

        put_wide_char_into_string(ch, result_str_next_ch, char_size);
      }
      break;
    default:
      unexpected_condition();
  }  /* switch */
}  /* conv_string_literal_chars */


void conv_string_literal(a_const_char                  *start_of_string_value,
                         a_const_char                  *end_of_string_value,
                         a_string_or_char_literal_kind lit_kind,
                         unsigned long                 num_chars,
                         an_error_code                 *err_code,
                         a_const_char                  **err_pos,
       /* Defaulted: */  a_boolean                     is_rescan)
/*
Convert a string literal from external form to internal form.
start_of_string_value and end_of_string_value point to the first character
of the value and to the terminating character of the external form (i.e.,
following the opening quote and to the closing quote in an ordinary string
literal, or following the '(' and to the ')' in a raw string literal), and
lit_kind describes the kind of literal.  The internal form is placed in
const_for_curr_token.  num_chars indicates the number of characters
contained within the quotes (after escape processing, and in wide
characters if the string is wide).  If the string is a char16_t string of
the form u"...", num_chars may be larger (but not smaller) than the number
of characters needed to represent the string.  if is_rescan is TRUE, this
call is scanning a previously-processed narrow character string value as a
different literal kind, as when concatenating an unprefixed string literal
with one that has a prefix.  In this case, escapes will have already been
recognized and translated, and an escaped backslash should not be
considered to introduce another escape.  Also, universal character names
will have been replaced by their UTF-8 encodings, so the rescan as a wider
UTF encoding may result in fewer characters than the number of bytes in the
UTF-8 encoding.

If there is no error, *err_code is set to ec_no_error (which is 0);
otherwise, *err_code is set to an appropriate error code and *err_pos is
set to the character position of the error.  (Not all errors are reported
this way.  Because string concatenation can change the literal kind,
constructs that would be an error in an ordinary narrow string literal but
accepted in a wide or UTF string literal are reported using
register_char_overflow, to be processed or discarded once the literal kind
is finally known.)
*/
{
  unsigned long                 i, centity_mask;
  unsigned int                  char_size = 0;
  a_character_kind              character_kind = (a_character_kind)ck_last;
  a_char_conversion_state       conv_state;
  int                           raw_str_trigraph_delim_chars = 0;
  a_string_or_char_literal_kind prefix_kind =
                                             literal_encoding_prefix(lit_kind);
  a_boolean                     is_raw_string =
                                     (lit_kind & SCLK_RAW_STRING_LITERAL) != 0;
  a_boolean                     process_escapes = !is_rescan;

  /* Set the character kind and size. */
  check_assertion(lit_kind & SCLK_STRING_LITERAL);
  switch (prefix_kind) {
    case SCLK_ORDINARY_LITERAL:
      character_kind = (a_character_kind)chk_char;
      char_size = 1;
      break;
    case SCLK_UTF8_LITERAL:
      character_kind = char8_t_enabled ? (a_character_kind)chk_char8_t
                                       : (a_character_kind)chk_char;
      char_size = 1;
      break;
    case SCLK_WIDE_LITERAL:
      character_kind = (a_character_kind)chk_wchar_t;
      char_size = (unsigned int)targ_sizeof_wchar_t;
      break;
    case SCLK_CHAR32_T_LITERAL:
      character_kind = (a_character_kind)chk_char32_t;
      char_size = (unsigned int)targ_sizeof_char32_t;
      break;
    case SCLK_CHAR16_T_LITERAL:
      character_kind = (a_character_kind)chk_char16_t;
      char_size = (unsigned int)targ_sizeof_char16_t;
      break;
    default:
      unexpected_condition();
  }  /* switch */
  /* Build a mask used to mask individual characters. */
  centity_mask = (unsigned long)1 << (targ_host_string_char_bit-1);
  centity_mask = centity_mask | (centity_mask-1);

  /* Determine the size of the string literal array (for char16_t strings, this
     may be an overestimate that's corrected following processing). */
  sizeof_t constant_size = (sizeof_t)(num_chars + 1);
  if (char_size != 1) {
    constant_size *= char_size;
    /* Replicate the mask for one character as many times as there are chars
       in the wide character.  This "inefficient" method is used because it
       works right even when the target character is larger than the host
       character.  In that case, there are "holes" in the bit pattern where a
       "1" bit cannot be represented. */
    for (i = 1; i < char_size; ++i) {
      centity_mask |= (centity_mask << targ_char_bit);
    }  /* for */
#if NATIVE_MULTIBYTE_CHARS_SUPPORTED_WITH_UNICODE
  } else if (curr_file_unicode_source_kind != usk_none && microsoft_mode) {
    /* UTF-8 characters will be translated to multibyte characters, so
       there is no fixed correspondence between the number of bytes in the
       token and the number of bytes in the constant.  In the worst case,
       each single-byte Unicode character could require
       MAX_MULTIBYTE_CHAR_LENGTH bytes in the translated character, so we
       assume that constant size to be safe.  (The actual length of the
       constant will be calculated below based on the number of bytes in
       the translated string.) */
    constant_size *= MAX_MULTIBYTE_CHAR_LENGTH;
#endif /* NATIVE_MULTIBYTE_CHARS_SUPPORTED_WITH_UNICODE */
  }  /* if */

  /* Allocate enough space to hold the final string based on the constant_size
     calculation above. */
  char      *result_str_start = alloc_text_of_string_literal(constant_size);
  char      *result_str_next_ch = result_str_start;
  /* UTF-8 characters should be translated to multibyte characters only for
     narrow-character literals in Microsoft mode in non-Unicode source files
     and only when the literal does not represent a function-name string like
     __FUNCTION__. */
  a_boolean translate_utf8_to_mbc =
                                (prefix_kind == SCLK_ORDINARY_LITERAL &&
                                 (lit_kind & SCLK_FUNCTION_NAME) == 0 &&
                                 microsoft_mode
#if UNICODE_SOURCE_SUPPORTED
                                 && curr_file_unicode_source_kind == usk_none
#endif /* UNICODE_SOURCE_SUPPORTED */
                                                                             );
  clear_char_conversion_state(&conv_state, &start_of_string_value,
                              translate_utf8_to_mbc);
  conv_state.create_surrogate_pairs = (prefix_kind == SCLK_WIDE_LITERAL ||
                                       prefix_kind == SCLK_CHAR16_T_LITERAL);
  conv_state.force_utf8 = gnu_mode && is_rescan;
  conv_state.is_rescan = is_rescan;
#if MULTIBYTE_CHARS_IN_SOURCE_SUPPORTED
  /* Initialize for scanning multibyte characters in the string. */
  mbc_scan_init_if_multibyte_chars_in_source_enabled();
#endif /* MULTIBYTE_CHARS_IN_SOURCE_SUPPORTED */
  if (is_raw_string) {
    process_escapes = FALSE;
    /* Set up to reverse any original line modifications (trigraphs, line
       splices) that appear in the raw string. */
    conv_state.next_orig_line_modif = orig_line_modif_list;
    while (conv_state.next_orig_line_modif != NULL &&
           conv_state.next_orig_line_modif->line_loc < start_of_string_value) {
      conv_state.next_orig_line_modif = conv_state.next_orig_line_modif->next;
    }  /* while */
    if (*end_of_string_value == ']') {
      /* This is the pathological case in which the two characters
         preceding the terminating ')' of the raw string literal were both
         '?' characters, which was interpreted as a trigraph for ']'.  Set
         up the loop control accordingly. */
      raw_str_trigraph_delim_chars = 1;
    }  /* if */
  }  /* if */
  if (microsoft_mode &&
      start_of_string_value <=
          end_of_string_value + raw_str_trigraph_delim_chars - LE_ESCAPE_LEN &&
      *start_of_string_value == LE_ESCAPE) {
    start_of_string_value = skip_embedded_null_escapes(
                           start_of_string_value,
                           end_of_string_value + raw_str_trigraph_delim_chars);
  }  /* if */
  switch (prefix_kind) {
    /* Define a macro to write the switch cases. */
#define ADD_SWITCH_CASE(prefix_kind)                                          \
    case prefix_kind:                                                         \
      conv_string_literal_chars<prefix_kind>(&result_str_start,               \
                                             &result_str_next_ch,             \
                                             &start_of_string_value,          \
                                             &end_of_string_value,            \
                                             char_size,                       \
                                             constant_size,                   \
                                             conv_state,                      \
                                             centity_mask,                    \
                                             process_escapes,                 \
                                             is_raw_string,                   \
                                             raw_str_trigraph_delim_chars);   \
      break;
    /* Add the switch cases. */
    ADD_SWITCH_CASE(SCLK_ORDINARY_LITERAL);
    ADD_SWITCH_CASE(SCLK_UTF8_LITERAL);
    ADD_SWITCH_CASE(SCLK_WIDE_LITERAL);
    ADD_SWITCH_CASE(SCLK_CHAR32_T_LITERAL);
    ADD_SWITCH_CASE(SCLK_CHAR16_T_LITERAL);
    default:
      unexpected_condition();
#undef ADD_SWITCH_CASE
  }  /* switch */
  /* Recalculate the actual final size of the converted constant, which can
     be smaller than the original calculated size because of translation of
     universal character names into UTF-8, translation of UTF-8 to a wider
     UTF-encoding or native multibyte characters, etc. */
  constant_size = (sizeof_t)(result_str_next_ch - result_str_start);

  a_targ_size_t num_elems = (a_targ_size_t)(constant_size / char_size);
  /* Make the constant entry for the string. */
  clear_constant(&const_for_curr_token, ck_string);
  const_for_curr_token.type = string_literal_type(character_kind, num_elems);
  const_for_curr_token.variant.string.length = (a_targ_size_t)constant_size;
  const_for_curr_token.variant.string.value = result_str_start;
  const_for_curr_token.character_kind = character_kind;
  /* Currently, no error is returned through err_code or err_pos. */
  *err_code = ec_no_error;
  *err_pos = NULL;  /* To make lint happy. */
}  /* conv_string_literal */


static void add_string_literal_concat_part(a_constant     *con,
                                           a_constant_ptr *p_first_con,
                                           a_constant_ptr *p_last_con)
/*
Add a copy of con to the list of constants for a dependent string literal
concatenation.
*/
{
  a_constant_ptr new_con = alloc_unshared_constant(con);

  if (*p_last_con == NULL) {
    *p_first_con = new_con;
  } else {
    (*p_last_con)->next = new_con;
  }  /* if */
  *p_last_con = new_con;
}  /* add_string_literal_concat_part */


void concat_string_literals(a_token_cache_ptr      cache,
                            a_character_kind       character_kind,
          /* Defaulted: */  a_token_cache_iterator *first_token)
/*
Concatenate two or more string literals contained in the indicated token cache,
and replace the constant in the first cached string token with the constant for
the concatenation.  If first_token is non-NULL, it points to an element of the
given cache and that is where concatenation starts; otherwise, concatenation
starts with the first token in the given cache.  (The rest of the cached tokens
are left as they are; the caller removes and frees them.)  The result string
will have characters of the given kind.  Some of the constants may be error
constants if there were malformed string literals in the input; in that case,
the output is an error constant.  Some of the entries in the token cache may be
for pragmas; they are ignored.  This routine implements the lexical
concatenation of section 2.1.1.2, phase 6, of the C standard.  The nulls from
the initial strings are discarded in doing the concatenation, and the one from
the last string is copied as the final null of the concatenated string; see
ANSI C 3.1.4.  The cached strings either all have the given character kind, or
a mix of the given kind and chk_char.
*/
{
  a_targ_size_t                 total_len = 0, str_len, null_len;
  a_token_cache_iterator        first_string_token = cache->end();
  a_boolean                     produce_error_constant = FALSE;
  char                          *new_str;
  a_const_char                  *saved_curr_char_loc = curr_char_loc;
  a_string_or_char_literal_kind lit_kind;
  a_boolean                     dependent_concat = FALSE;

  db_enter(4, "concat_string_literals");
  if (character_kind != chk_char) {
    /* We may be rescanning normal narrow character string literals as
       a different literal kind, so any previous pending reports of char
       overflow should be discarded. */
    clear_char_overflows();
  }  /* if */

  a_token_cache_iterator tok_it;
  a_token_cache_iterator tok_it_end = cache->end();
  if (first_token != NULL) {
    tok_it = *first_token;
  } else {
    tok_it = cache->begin();
  }  /* if */
  /* Determine the length of the terminating null on strings.  It's usually 1,
     but it may be bigger for wide string literals. */
  null_len = character_size[character_kind];
  /* Determine the literal kind associated with the character kind in
     case ordinary string literals must be rescanned to match the result
     kind. */
  lit_kind = char_kind_to_str_literal_kind(character_kind);
  /* Determine the length of the concatenation. */
  for (; tok_it != tok_it_end; ++tok_it) {
    const a_shared_token &tok = *tok_it;

    /* Ignore pragma entries. */
    if (tok->is_pragma()) continue;
    check_assertion_str(tok->is_string_literal(),
                       "concat_string_literals: cached token is not a string");
    if (first_string_token == tok_it_end) {
      first_string_token = tok_it;
    }  /* if */

    const a_constant *con = tok->get_constant(), *str_con = con;
    if (is_error_constant(con)) {
      /* If any constant is an error constant, the overall concatenation
         will be an error constant. */
      produce_error_constant = TRUE;
      break;
    } else {
      /* String constant. */
      if (constant_is(con, ck_template_param) &&
          tpck_is(con, tpck_dependent_constant)) {
        dependent_concat = TRUE;
        str_con = con->variant.template_param.variant.constant;
      }  /* if */
      check_assertion_str(constant_is(str_con, ck_string),
                          "concat_string_literals: constant not ck_string");
      /* Determine the length of this string literal. */
      str_len = str_con->variant.string.length;
      if (str_con->character_kind != character_kind) {
        if (str_con->character_kind != chk_char) {
          /* An attempt to concatenate two different string kinds, neither of
             which is a plain (narrow) string.  This is an error. */
          produce_error_constant = TRUE;
        } else {
          /* This string will need widening. */
          str_len *= null_len;
        }  /* if */
      }  /* if */
      /* Except on the last constant, subtract out the space for the
         final null in the string. */
      if (tok_it + 1 != tok_it_end) str_len -= null_len;
      /* Add the length of this string to the accumulated length. */
      total_len += str_len;
    }  /* if */
  }  /* for */
  /* Here, we either have the length of the concatenation in total_len, or
     produce_error_constant is set. */
  /* Build the concatenation and record it in the constant in the first
     string token in the cache. */
  check_assertion(first_string_token != tok_it_end);

  /* Create a copy of the token and concatenate into the new associated
     constant. */
  a_cached_token new_tok = **first_string_token;
  a_constant     *concat_con = new_tok.get_constant();
  if (produce_error_constant) {
    /* There is at least one error constant in the concatenation or the
       strings were of incompatible kinds (e.g., L"a" U"b"), so return
       an error constant. */
    set_error_constant(concat_con);
  } else if (dependent_concat) {
    a_constant_ptr first_con = NULL, last_con = NULL;
    a_token_cache_iterator con_tok_it = first_string_token;
    for (; con_tok_it != tok_it_end; ++con_tok_it) {
      const a_shared_token &tok = *con_tok_it;

      if (tok->is_pragma()) continue;
      add_string_literal_concat_part(tok->get_constant(),
                                     &first_con, &last_con);
    }  /* for */
    clear_constant(concat_con, ck_template_param);
    set_template_param_constant_kind(concat_con, tpck_concat_string_literals);
    concat_con->type = string_literal_type(character_kind, total_len/null_len);
    concat_con->character_kind = character_kind;
    concat_con->variant.template_param.variant.string_literal_list = first_con;
  } else {
    /* No error constants, so do the concatenation. */
    /* Allocate enough space for the concatenation. */
    new_str = alloc_text_of_string_literal((sizeof_t)total_len);
    total_len = 0;

    /* Copy the constants into the concatenation. */
    a_token_cache_iterator con_tok_it = first_string_token;
    for (; con_tok_it != tok_it_end; ++con_tok_it) {
      const a_shared_token &tok = *con_tok_it;

      /* Ignore pragma entries. */
      if (tok->is_pragma()) {
        continue;
      }  /* if */

      const a_constant *con = tok->get_constant();
      if (con->character_kind != character_kind) {
        /* A string like "xyz" in L"abc" "xyz" must be rescanned as the
           correct kind of literal, effectively resulting in L"abc" L"xyz".
           const_for_curr_token will replace the cached constant. */
        a_const_char                  *old_val = con->variant.string.value;
        an_error_code                 err_code;
        a_const_char                  *err_loc;
        a_string_or_char_literal_kind this_lit_kind = lit_kind;
        check_assertion(con->character_kind == (a_character_kind)chk_char);
        conv_string_literal(old_val, old_val + con->variant.string.length - 1,
                            this_lit_kind,
                            (unsigned long)con->variant.string.length - 1,
                            &err_code, &err_loc, /*is_rescan=*/TRUE);
        con = &const_for_curr_token;
      }  /* if */
      /* Determine the length of this string literal. */
      str_len = con->variant.string.length;
      /* Except on the last constant, subtract out the space for the final
         null in the string. */
      if ((con_tok_it + 1) != tok_it_end) {
        str_len -= null_len;
      }  /* if */
      /* Copy the string text (including the final null, if that's
         appropriate). */
      (void)memcpy(new_str+total_len, con->variant.string.value,
                   size_t_arg(str_len));
      /* Keep track of the total length so far, which is also the offset for
         storing into the concatenation. */
      total_len += str_len;
    }  /* for */
    /* Overwrite the first string with the concatenation.  Note this is
       done late because the information in the first string is used in the
       concatenation loop above. */
    /* The string currently associated with the first constant (and, for that
       matter, the strings for all the constants) are just lost. */
    /* Get rid of any information specific to the old constant; in particular,
       get rid of its source correspondence (possible when the first constant
       comes from a macro). */
    clear_constant(concat_con, (a_constant_repr_kind)ck_string);
    concat_con->variant.string.length = total_len;
    concat_con->variant.string.value  = new_str;
    /* Adjust the constant type to match the new length. */
    concat_con->type = string_literal_type(character_kind,
                                           (a_targ_size_t)total_len/null_len);
    concat_con->character_kind = character_kind;
  }  /* if */

  /* Replace the token with the modified copy. */
  *first_string_token = move_from(&new_tok);
  /* The constants have been concatenated into the first constant in the
     token cache (which might not be the first entry in the cache, if there
     are pragma entries first).  Discard the token cache entries for the
     string literal tokens after that first one. */
  cache->remove_non_pragma_tokens_after(first_string_token);
  curr_char_loc = saved_curr_char_loc;
  db_exit();
}  /* concat_string_literals */


void literals_one_time_init(void)
/*
Do one-time (per call, in MAKE_FRONT_END_CALLABLE configurations)
initialization of variables related to processing of literals.
*/
{
  token_buffer = NULL;
}  /* literals_one_time_init */

/* Conditionally close the "edg" namespace. */
END_EDG_NAMESPACE


