/*
Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
Exceptions.
See https://edgcpp.org/LICENSE.txt for license information.
SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
*/

/*

floating.c -- convert decimal strings to binary values and vice versa.

*/

/* Header files common to all files. */
#include "fe_common.h"

#ifdef PCH_PRAGMA_GUARD
/* Mark the end of the sequence of headers subject to precompiled header
   processing. */
#pragma hdrstop
#endif /* ifdef PCH_PRAGMA_GUARD */

#if !USE_HOST_FP_CONVERSION_ROUTINES || \
    (USE_FLOAT128_FOR_HOST_FP_VALUE && !USE_QUADMATH_LIBRARY)

/* Additional header files. */
#include <ctype.h>
#include <string.h>
#include "floating.h"

/* Conditionally open the "edg" namespace. */
BEGIN_EDG_NAMESPACE

/*
The routines contained herein implement floating-point decimal-to-binary and
binary-to-decimal conversions based on the techniques described in the
following articles:

  Steele, Guy M. & Jon L. White, "How to Print Floating-point Numbers
    Accurately", Proceedings of the ACM SIGPLAN '90 Conference on Programming
    Language Design and Implementation.  White Plains, New York, June 20-22,
    1990.

  Clinger, William D., "How to Read Floating-point Numbers Accurately",
    Proceedings of the ACM SIGPLAN '90 Conference on Programming Language
    Design and Implementation.  White Plains, New York, June 20-22, 1990.

  Gay, David M., "Correctly Rounded Binary-Decimal and Decimal-Binary
    Conversions", AT&T Bell Laboratories Numerical Analysis Manuscript 90-10,
    November 30, 1990.

*/

/*
For the purposes of unit testing, allow certain routines to be visible
externally.
*/

#ifndef FP_UNIT_TESTING
#define FP_UNIT_TESTING 0
#endif /* ifndef FP_UNIT_TESTING */

#if FP_UNIT_TESTING
#define STATIC /**/
#else /* !FP_UNIT_TESTING */
#define STATIC static
#endif /* FP_UNIT_TESTING */

static constexpr a_const_char *lc_inf = "infinity";
static constexpr a_const_char *uc_inf = "INFINITY";
static constexpr a_const_char *lc_nan = "nan";
static constexpr a_const_char *uc_nan = "NAN";


static int match(a_const_char *str,
                 a_const_char *end,
                 a_const_char *lc_tgt,
                 a_const_char *uc_tgt)
/*
Check whether an input string [str, end) matches the first three characters
of the target string or the entire target string; lc_tgt is a lowercase
version of the target string and uc_tgt is uppercase.  Any combination of
cases is a match.  Returns the length of the successful match, or 0 if the
match failed.
*/
{
  int len = 0;
  while (len < end - str &&
         (str[len] == lc_tgt[len] || str[len] == uc_tgt[len])) {
    ++len;
  }  /* while */
  return len == 3 || uc_tgt[len] == '\0' ? len : 0;
}  /* match */


static void parse_nan(an_fp_decimal_input *dec,
                      a_const_char        *str,
                      a_const_char        *end)
/*
Parse an input string [str, end) that has been tentatively identified as a
nan.
*/
{
  int       len = match(str, end, lc_nan, uc_nan);

  if (len == end - str) {
    dec->type = fpt_nan;
  } else if (len == 3 &&
             str[3] == '(' &&
             strchr(str + 4, ')') == end - 1) {
    dec->type = fpt_nan;
  } else {
    dec->type = fpt_invalid;
  }  /* if */
}  /* parse_nan */


static void parse_infinity(an_fp_decimal_input *dec,
                           a_const_char        *str,
                           a_const_char        *end)
/*
Parse an input string [str, end) that has been tentatively identified as an
infinity.
*/
{
  int       len = match(str, end, lc_inf, uc_inf);

  if (len == end - str) {
    dec->type = fpt_infinity;
  } else {
    dec->type = fpt_invalid;
  }  /* if */
}  /* parse_infinity */


STATIC void split_string(an_fp_decimal_input *dec,
                         a_const_char        *str,
                         a_const_char        *end)
/*
Split an input string [str, end) into parts.
*/
{
  int       precision = 0;
  a_boolean seen_digit = FALSE;

  dec->first_frac = dec->last_frac = str;
  dec->is_negative = FALSE;
  dec->type = fpt_number;
  /* Skip leading whitespace, handle '+' and '-', skip leading zeros. */
  while (*str && isspace((unsigned char)*str)) {
    ++str;
  }  /* while */
  if (*str == '-') {
    dec->is_negative = TRUE;
    ++str;
  } else if (*str == '+') {
    ++str;
  }  /* if */
  if (*str == 'n' || *str == 'N') {
    parse_nan(dec, str, end);
    goto end_of_routine;
  } else if (*str == 'i' || *str == 'I') {
    parse_infinity(dec, str, end);
    goto end_of_routine;
  }  /* if */
  while (*str == '0') {
    seen_digit = TRUE;
    ++str;
  }  /* while */
  /* Set dec->first_int to point to first non-zero digit to left of
     decimal point, and dec->last_int to next non-digit character.  This
     range can be empty. */
  dec->first_int = str;
  while (isdigit((unsigned char)*str) &&
         precision < BIGINT_MAX_EXP - 10) {
    ++precision;
    seen_digit = TRUE;
    ++str;
  }  /* while */
  dec->last_int = str;
  while (isdigit((unsigned char)*str)) {
    /* Skip digits beyond precision. */
    ++str;
  }  /* while */
  dec->exponent = (int)(str - dec->first_int);
  /* Set dec->first_frac to point to first digit of decimal fraction,
     and dec->last_frac to next non-digit character.  This range can be
     empty.  If there were no non-zero digits to the left of the decimal
     point, decimal fraction begins at the first non-zero digit after the
     decimal point. */
  if (*str == '.') {
    ++str;
    if (dec->first_int == dec->last_int) {
      while (*str == '0') {
        seen_digit = TRUE;
        ++str;
        --dec->exponent;
      }  /* while */
    }  /* if */
    dec->first_frac = str;
    while (*str && isdigit((unsigned char)*str) &&
           precision < BIGINT_MAX_EXP - 10) {
      ++precision;
      seen_digit = TRUE;
      ++str;
    }  /* while */
    dec->last_frac = str;
    /* Remove trailing zeros. */
    while (dec->last_frac != dec->first_frac && *(dec->last_frac - 1) == '0') {
      --precision;
      --dec->last_frac;
    }  /* while */
    while (*str && isdigit((unsigned char)*str)) {
      /* Skip digits beyond precision. */
      ++str;
    }  /* while */
  }  /* if */
  if (dec->last_frac == dec->first_frac) {
    while (dec->last_int != dec->first_int && *(dec->last_int - 1) == '0') {
      --precision;
      --dec->last_int;
    }  /* while */
  }  /* if */
  dec->precision = (int)(dec->last_int - dec->first_int) +
                   (int)(dec->last_frac - dec->first_frac);
  check_assertion(precision == dec->precision);
  if (precision == 0) {
    dec->type = fpt_zero;
  }  /* if */
  /* Scan exponent if present. */
  if (!seen_digit) {
    dec->type = fpt_invalid;
  } else if (*str == 'e' || *str == 'E') {
    char *str_end;
    long tmp;
    int neg = 0;

    ++str;
    if (*str == '-') {
      ++str;
      neg = 1;
    } else if (*str == '+') {
      ++str;
    }  /* if */
    if (!isdigit((unsigned char)*str)) {
      dec->type = fpt_invalid;
    } else {
      tmp = strtol(str, &str_end, 10);
      if (str_end == str) {
        /* No valid digits found in exponent. */
        dec->type = fpt_invalid;
      } else if (dec->type == fpt_zero) {
        /* Nothing to do. */
      } else if (INT_MAX <= tmp ||
                 (0 < dec->exponent && LONG_MAX - dec->exponent < tmp)) {
        /* Integer underflow or overflow occurred. */
        dec->type = neg ? fpt_underflow : fpt_overflow;
      } else {
        if (neg) {
          dec->exponent -= (int)tmp;
        } else {
          dec->exponent += (int)tmp;
        }  /* if */
      }  /* if */
      str = str_end;
    }  /* if */
  }  /* if */
  /* Truncate excessively long strings of digits. */
  if (dec->type == fpt_number && precision == 0) {
    dec->type = fpt_zero;
  }  /* if */
  if (dec->type != fpt_number) {
    /* Nothing to do. */
  } else if (BIGINT_MAX_EXP < dec->precision + dec->exponent) {
    dec->precision = BIGINT_MAX_EXP - dec->exponent;
  } else if (BIGINT_MAX_EXP < dec->precision - dec->exponent) {
    dec->precision = BIGINT_MAX_EXP + dec->exponent;
  }  /* if */
  if (str != end) {
    dec->type = fpt_invalid;
  }  /* if */
end_of_routine:;
}  /* split_string */

#if FP_USE_EMULATION || FP_UNIT_TESTING

STATIC void fp_frac_set_from_uint(unsigned char *frac,
                                  int           width,
                                  an_fp_uint    val)
/*
Copy the value in val into width bits in the byte array pointed to by frac,
right justified.  Note: sets all bits in the topmost byte, even if the
fraction occupies fewer bits.
*/
{
  int i;
  int bytes = BYTE_COUNT(width);

  for (i = 0; i < bytes && val != 0; ++i) {
    frac[i] = (unsigned char)val;
    val >>= BYTE_SIZE;
  }  /* for */
  for ( ; i < bytes; ++i) {
    frac[i] = 0;
  }  /* for */
}  /* fp_frac_set_from_uint */


STATIC void fp_frac_copy(unsigned char *left,
                         unsigned char *right,
                         int           width)
/*
Copy low bytes holding width bits from byte array pointed to by
left to byte array pointed to by right.
*/
{
  memcpy(left, right, (size_t)BYTE_COUNT(width));
}  /* fp_frac_copy */


STATIC a_boolean fp_frac_lt(unsigned char *left,
                            unsigned char *right,
                            int           width)
/*
Return TRUE if value in low bytes holding width bits in byte arrays pointed
to by left is less than value in right, otherwise FALSE.  Note: bits beyond
width in the topmost byte must be 0.
*/
{
  int       i;
  a_boolean res = FALSE;

  for (i = BYTE_COUNT(width); 0 < i; ) {
    --i;
    if (left[i] != right[i]) {
      res = (a_boolean)(left[i] < right[i]);
      break;
    }  /* if */
  }  /* for */
  return res;
}  /* fp_frac_lt */

#endif /* FP_USE_EMULATION || FP_UNIT_TESTING */

STATIC void fp_frac_set_to_min(unsigned char *frac,
                               int           width)
/*
Set the byte array pointed to by frac to represent the minimum normalized
fraction for a floating-point value of width bits.  That is, the topmost
bit is set to 1 and all other bits are set to 0.  Note: sets all bits in
the topmost byte, even if the fraction occupies fewer bits.
*/
{
  int i;
  int bytes = BYTE_COUNT(width);

  for (i = 0; i < bytes - 1; ++i) {
    frac[i] = 0;
  }  /* for */
  frac[i] = (unsigned char)HIGH_BIT(width);
}  /* fp_frac_set_to_min */


STATIC void fp_frac_set_to_max(unsigned char *frac,
                               int           width)
/*
Set the byte array pointed to by frac to represent the maximum normalized
fraction for a floating-point value of width bits.  That is, all the bits
are set to 1.
*/
{
  int i;
  int bytes = BYTE_COUNT(width);

  for (i = 0; i < bytes - 1; ++i) {
    frac[i] = HIGH_BYTE_BITS(BYTE_SIZE);
  }  /* for */
  frac[i] = HIGH_BYTE_BITS(width);
}  /* fp_frac_set_to_max */


STATIC a_boolean fp_frac_eq_min_frac(unsigned char *frac,
                                     int           width)
/*
Return TRUE if frac holds minimum fraction value for width bits, i.e., all
bits 0 except bit at (width - 1).  Note: bits beyond width in the topmost
byte must be 0.
*/
{
  int       i;
  int       bytes = BYTE_COUNT(width);
  a_boolean res;

  for (i = 0; i < bytes - 1; ++i) {
    if (frac[i] != 0) {
      res = FALSE;
      goto end_of_routine;
    }  /* if */
  }  /* for */
  if (frac[i] == HIGH_BIT(width)) {
    res = TRUE;
  } else {
    res = FALSE;
  }  /* if */
end_of_routine:
  return res;
}  /* fp_frac_eq_min_frac */


STATIC int fp_frac_low_zero_bits(unsigned char *frac,
                                 int           width)
/*
Return the number of low-order zero bits in the width bits at the low
end of frac.
*/
{
  int           i;
  unsigned char the_byte = 0;
  int           zbits = 0;

  for (i = 0; i < BYTE_COUNT(width) && (the_byte = frac[i]) == 0; ++i) {
    zbits += BYTE_SIZE;
  }  /* for */
  if (the_byte != 0) {
#if 8 < BYTE_SIZE
#if 16 < BYTE_SIZE
#if 32 < BYTE_SIZE
    while ((the_byte & 0xffffffff) == 0) {
      zbits += 32;
      the_byte >>= 32;
    }  /* while */
#endif  /* 32 < BYTE_SIZE */
    if ((the_byte & 0xffff) == 0) {
      zbits += 16;
      the_byte >>= 16;
    }  /* if */
#endif  /* 16 < BYTE_SIZE */
    if ((the_byte & 0xff) == 0) {
      zbits += 8;
      the_byte >>= 8;
    }  /* if */
#endif  /* 8 < BYTE_SIZE */
    if ((the_byte & 0x0f) == 0) {
      zbits += 4;
      the_byte >>= 4;
    }  /* if */
    if ((the_byte & 0x03) == 0) {
      zbits += 2;
      the_byte >>= 2;
    }  /* if */
    if ((the_byte & 0x01) == 0) {
      ++zbits;
    }  /* if */
  }  /* if */
  return zbits;
}  /* fp_frac_low_zero_bits */


STATIC int fp_frac_high_zero_bits(unsigned char *frac,
                                  int           width)
/*
Return the number of high-order zero bits in the width bits at the low
end of frac.
*/
{
  int           zbits;
  unsigned char *pos;
  unsigned char the_byte;

  pos = frac + BYTE_COUNT(width);
  zbits = width % BYTE_SIZE;
  if (zbits != 0) {
    /* Nothing to do. */
  } else if (pos == frac) {
    zbits = 0;
    goto end_of_routine;
  } else {
    zbits = BYTE_SIZE;
  }  /* if */
  the_byte = *(pos - 1) & (unsigned char)MASK_BITS(zbits);
  while (pos != frac && *--pos == 0) {
    zbits += BYTE_SIZE;
    if (pos != frac) {
      /* Prevent reading byte before buffer. */
      the_byte = *(pos - 1);
    }  /* if */
  }  /* while */
  if (*pos == 0) {
    /* All bits are zero, and we've overestimated the value. */
    zbits -= BYTE_SIZE;
  } else {
    /* *pos is the highest non-zero value; adjust for its bits. */
#if 8 < BYTE_SIZE
#if 16 < BYTE_SIZE
#if 32 < BYTE_SIZE
#error unsigned char too large
#endif /* 32 < BYTE_SIZE */
    if ((the_byte & 0xFFFF0000) != 0) {
      the_byte >>= 16;
      zbits -= 16;
    }  /* if */
#endif /* 16 < BYTE_SIZE */
    if ((the_byte & 0xFF00) != 0) {
      the_byte >>= 8;
      zbits -= 8;
    }  /* if */
#endif /* 8 < BYTE_SIZE */
    if ((the_byte & 0xF0) != 0) {
      the_byte >>= 4;
      zbits -= 4;
    }  /* if */
    if ((the_byte & 0x0C) != 0) {
      the_byte >>= 2;
      zbits -= 2;
    }  /* if */
    if ((the_byte & 0x02) != 0) {
      the_byte >>= 1;
      zbits -= 1;
    }  /* if */
    if (the_byte != 0) {
      --zbits;
    }  /* if */
  }  /* if */
end_of_routine:
  return zbits;
}  /* fp_frac_high_zero_bits */


static a_boolean has_one_bits_below(unsigned char *frac,
                                    int           bit_pos)
/*
Return FALSE if there are no 1 bits below bit_pos in the byte array pointed to
by frac, otherwise TRUE.
*/
{
  a_boolean res = FALSE;
  int       byte_pos = bit_pos / BYTE_SIZE;
  unsigned  bit_mask = MASK_BITS(bit_pos % BYTE_SIZE);

  if ((frac[byte_pos] & bit_mask) != 0) {
    res = TRUE;
  }  /* if */
  while (res == 0 && byte_pos != 0) {
    if (frac[--byte_pos] != 0) {
      res = TRUE;
    }  /* if */
  }  /* while */
  return res;
}  /* has_one_bits_below */


static int fp_frac_cmp_tail_to_half(unsigned char *frac,
                                    int           width,
                                    int           low_bit_pos)
/*
Return -1 if tail < .5, 0 if tail == .5, 1 if tail > .5.
*/
{
  int res = 0;

  if (low_bit_pos <= 0) {
    res = 0; /* Empty tail. */
  } else if (BIT_AT(frac, width, low_bit_pos - 1) == 0) {
    res = -1;  /* Tail < .5. */
  } else if (has_one_bits_below(frac, low_bit_pos - 1)) {
    res = 1;  /* Tail > .5. */
  }  /* if */
  return res;
}  /* fp_frac_cmp_tail_to_half */

#if FP_USE_EMULATION || FP_UNIT_TESTING

STATIC a_boolean fp_frac_should_round(unsigned char *frac,
                                      int           width,
                                      int           low_bit_pos)
/*
Return TRUE if discarding the bits below low_bit_pos in the byte array pointed
to by frac would require rounding, otherwise FALSE.  Applies round to even.
*/
{
  int res = FALSE;

  if (low_bit_pos <= 0) {
    /* No tail, no rounding. */
  } else if (BIT_AT(frac, width, low_bit_pos - 1) == 0) {
    /* Tail < .5, no rounding. */
  } else if (has_one_bits_below(frac, low_bit_pos - 1)) {
    res = TRUE; /* Tail > .5, round up. */
  } else if (width == low_bit_pos) {
    res = FALSE; /* Tail == .5, no high bit (i.e., high bit is 0). */
  } else if (BIT_AT(frac, width, low_bit_pos) != 0) {
    res = TRUE; /* Tail == .5, low bit is 1, round up (to even). */
  }  /* if */
  return res;
}  /* fp_frac_should_round */

#endif /* FP_USE_EMULATION || FP_UNIT_TESTING */

STATIC void do_shift_right(unsigned char *frac,
                           int           width,
                           int           shift,
                           a_boolean     should_round)
/*
Shift the width-bit value pointed to by frac right by shift bits, rounding
if should_round is TRUE.  Pads with zeros.
*/
{
  int bytes = BYTE_COUNT(width);

  check_assertion(0 <= shift);
  if (width < shift) {
    int pos;
    for (pos = 0; pos < bytes; ++pos) {
      frac[pos] = 0;
    }  /* for */
  } else if (shift != 0) {
    int byte_shift = shift / BYTE_SIZE;
    int bit_shift = shift % BYTE_SIZE;
    int src_pos = byte_shift;
    int tgt_pos = 0;

    for ( ; src_pos < bytes; ++src_pos, ++tgt_pos) {
      frac[tgt_pos] = frac[src_pos] >> bit_shift;
      if (src_pos < bytes - 1) {
        frac[tgt_pos] += (unsigned char)(frac[src_pos + 1] <<
                                         (BYTE_SIZE - bit_shift));
      }  /* if */
      if (!should_round) {
        /* Nothing to do. */
      } else if (frac[tgt_pos] == BYTE_MAX) {
        frac[tgt_pos] = 0;
      } else {
        ++frac[tgt_pos];
        should_round = FALSE;
      }  /* if */
    }  /* for */
    if (should_round) {
      frac[tgt_pos++] = 1;
    }  /* if */
    for ( ; tgt_pos < bytes; ++tgt_pos) {
      frac[tgt_pos] = 0;
    }  /* for */
  }  /* if */
}  /* do_shift_right */


static void fp_frac_shift_right_raw(unsigned char *frac,
                                    int           width,
                                    int           shift)
/*
Shift the width-bit value pointed to by frac right by shift bits, ignoring
low bits. Pads with zeros.
*/
{
  do_shift_right(frac, width, shift, 0);
}  /* fp_frac_shift_right_raw */

#if FP_USE_EMULATION || FP_UNIT_TESTING

STATIC void fp_frac_shift_right(unsigned char *frac,
                                int           width,
                                int           shift)
/*
Shift the width-bit value pointed to by frac right by shift bits, rounding
as needed.  Pads with zeros.
*/
{
  a_boolean should_round = fp_frac_should_round(frac, width, shift);
  do_shift_right(frac, width, shift, should_round);
}  /* fp_frac_shift_right */

#endif /* FP_USE_EMULATION || FP_UNIT_TESTING */

static void fp_frac_shift_right_with_high_bit(unsigned char *frac,
                                              int           width)
/*
Shift the width-bit value pointed to by frac right by 1 bit, ignoring low bit.
Pads with 1.
*/
{
  int bytes = BYTE_COUNT(width);
  int pos = 0;
  an_fp_uint tmp;

  tmp = frac[pos];
  frac[pos] = (unsigned char)(tmp >> 1);
  ++pos;
  for ( ; pos < bytes; ++pos) {
    frac[pos - 1] |= (frac[pos] & 0x01) << (BYTE_SIZE - 1);
    tmp = frac[pos];
    frac[pos] = (unsigned char)(tmp >> 1);
  }  /* for */
  frac[pos - 1] |= (unsigned char)HIGH_BIT(width);
}  /* fp_frac_shift_right_with_high_bit */


STATIC void fp_frac_shift_left(unsigned char *frac,
                               int           width,
                               int           shift)
/*
Shift the width-bit value pointed to by frac left by shift bits.  Fills with
zeros.
*/
{
  int bytes = BYTE_COUNT(width);

  check_assertion(0 <= shift);
  if (width <= shift) {
    int pos;
    for (pos = 0; pos < bytes; ++pos) {
      frac[pos] = 0;
    }  /* for */
  } else if (shift != 0) {
    int byte_shift = shift / BYTE_SIZE;
    int bit_shift = shift % BYTE_SIZE;
    int src_pos = bytes - byte_shift - 1;
    int tgt_pos = bytes - 1;

    frac[tgt_pos] = (unsigned char)(frac[src_pos] << bit_shift);
    for ( ; src_pos != 0; ) {
      --src_pos;
      --tgt_pos;
      frac[tgt_pos + 1] |= frac[src_pos] >> (BYTE_SIZE - bit_shift);
      if (tgt_pos >= 0) {
        frac[tgt_pos] = (unsigned char)(frac[src_pos] << bit_shift);
      }  /* if */
    }  /* for */
    for ( ; tgt_pos != 0; ) {
      --tgt_pos;
      frac[tgt_pos] = 0;
    }  /* for */
    frac[bytes - 1] &= HIGH_BYTE_BITS(width);
  }  /* if */
}  /* fp_frac_shift_left */


STATIC a_boolean fp_frac_add_int(unsigned char *frac,
                                 int           width,
                                 int           val)
/*
Add the value val to the width-bit value pointed to by frac.  Returns TRUE if
the result overflowed, otherwise FALSE.
*/
{
  int       i;
  int       bytes = BYTE_COUNT(width);
  int       carry = val;

  for (i = 0; i < bytes - 1 && carry != 0; ++i) {
    carry += frac[i];
    frac[i] = (unsigned char)carry;
    carry /= (1 << BYTE_SIZE);
  }  /* for */
  if (carry != 0) {
    carry += HIGH_BYTE_BITS(width) & frac[bytes - 1];
    frac[bytes - 1] = (unsigned char)(carry & HIGH_BYTE_BITS(width));
    carry &= ~HIGH_BYTE_BITS(width);
  }  /* if */
  return carry != 0 ? TRUE : FALSE;
}  /* fp_frac_add_int */


static a_boolean fp_frac_add_one_at_pos(unsigned char *frac,
                                        int           width,
                                        int           pos)
/*
Add (1 << pos) to the width_bit value pointed to by frac.  Return TRUE if
the result overflowed, otherwise FALSE.
*/
{
  int i;
  int bytes = BYTE_COUNT(width);
  int carry = 1 << (pos % BYTE_SIZE);

  for (i = pos / BYTE_SIZE; i < bytes - 1 && carry != 0; ++i) {
    carry += frac[i];
    frac[i] = (unsigned char)carry;
    carry /= (1 << BYTE_SIZE);
  }  /* for */
  if (carry != 0) {
    carry += HIGH_BYTE_BITS(width) & frac[bytes - 1];
    frac[bytes - 1] = (unsigned char)(carry & HIGH_BYTE_BITS(width));
    carry &= ~HIGH_BYTE_BITS(width);
  }  /* if */
  return carry != 0 ? TRUE : FALSE;
}  /* fp_frac_add_one_at_pos */


STATIC int fp_frac_sub_int(unsigned char *frac,
                           int           width,
                           int           val)
/*
Subtract the value val (assumed to be small) from the width-bit
value pointed to by frac.  If the result underflowed the result is
not correct.  Returns the magnitude of the underflow if
the result underflowed, otherwise 0.
*/
{
  int i;
  int bytes = BYTE_COUNT(width);
  int borrow = val;
  int res = 0;

  check_assertion(0 <= val && val < (1 << BYTE_SIZE));
  for (i = 0; i < bytes && borrow != 0; ++i) {
    int tmp = frac[i] - borrow;
    if (0 <= tmp) {
      borrow = 0;
    } else {
      tmp += (1 << BYTE_SIZE);
      borrow = 1;
    }  /* if */
    frac[i] = (unsigned char)tmp;
  }  /* for */
  if (borrow != 0 || (HIGH_BIT(width) & frac[bytes - 1]) == 0) {
    res = (1 << BYTE_SIZE) - frac[0];
  }  /* if */
  return res;
}  /* fp_frac_sub_int */

#if FP_USE_EMULATION || FP_UNIT_TESTING

static void fp_frac_sub(unsigned char *left,
                        unsigned char *right,
                        int           width)
/*
Compute *left -= *right.  Assumes that *right <= *left.
*/
{
  int i;
  int tmp = 0;
  int bytes = BYTE_COUNT(width);

  for (i = 0; i < bytes; ++i) {
    int borrow = 0;
    tmp += left[i];
    tmp -= right[i];
    if (tmp < 0) {
      tmp += BYTE_MAX + 1;
      borrow = -1;
    }  /* if */
    left[i] = (unsigned char)tmp;
    tmp = borrow;
  }  /* for */
}  /* fp_frac_sub */


static void fp_frac_div_int(unsigned char *frac,
                            int           width,
                            int           val)
/*
Divide the width-bit value pointed to by frac by the non-negative value val.
*/
{
  int i;
  int tmp = 0;
  int bytes = BYTE_COUNT(width);

  for (i = bytes; 0 < i; ) {
    --i;
    tmp += frac[i];
    frac[i] = (unsigned char)(tmp / val);
    tmp <<= BYTE_SIZE;
  }  /* for */
  check_assertion(tmp == 0);
}  /* fp_frac_div_int */


STATIC void fp_frac_mult_int(unsigned char *frac,
                             int           width,
                             int           val)
/*
Multiply the width-bit value pointed to by frac by the non-negative value
val.  Does not handle overflow sensibly.
*/
{
  int i;
  int carry = 0;

  check_assertion(0 <= val);
  for (i = 0; i < BYTE_COUNT(width); ++i) {
    carry += frac[i] * val;
    frac[i] = (unsigned char)carry;
    carry /= (1 << BYTE_SIZE);
  }  /* for */
  check_assertion(i < BYTE_COUNT(width) || carry == 0);
}  /* fp_frac_mult_int */

#endif /* FP_USE_EMULATION || FP_UNIT_TESTING */

/* a_bigint objects */
STATIC_THREAD a_bigint
                bigints[NUM_BIGINTS];
                        /* A statically allocated list of a_bigint temporary
                           locations used for the conversion routines. */
STATIC_THREAD a_bigint
                *bigint_head;
                        /* A list of bigints that are "free". */


STATIC void delete_bigint(a_bigint *ptr)
/*
Put a_bigint object pointed to by ptr back onto the free list.
*/
{
  check_assertion(ptr != 0);
  check_assertion(&bigints[0] <= ptr && ptr < &bigints[NUM_BIGINTS]);
  ptr->next = bigint_head;
  bigint_head = ptr;
}  /* delete_bigint */


STATIC a_bigint *new_bigint(void)
/*
Return a pointer to a currently unused bigint object, whose value is set to 0.
*/
{
  a_bigint *res = 0;

  /* Return head of free list. */
  res = bigint_head;
  check_assertion(bigint_head != 0);
  bigint_head = bigint_head->next;
  res->num_words = 0;
  return res;
}  /* new_bigint */


STATIC void init_bigints(void)
/*
Allocate bigint objects as necessary.
*/
{
  /* Verify the configuration. */
  int i;
  check_assertion(sizeof(a_bigint_word) >= BIGINT_WORD_BYTES);   /*lint !e506*/
  check_assertion(sizeof(a_bigint_uint) >= 2*BIGINT_WORD_BYTES); /*lint !e506*/
  check_assertion(sizeof(a_bigint_uint) == sizeof(a_bigint_int));/*lint !e506*/
  check_assertion(sizeof(an_fp_uint) > BIGINT_WORD_BYTES);       /*lint !e506*/
  /* Initialize free list. */
  for (i = 0; i < NUM_BIGINTS; ++i) {
    delete_bigint(&bigints[i]);
  }  /* for */
}  /* init_bigints */


STATIC void bigint_from_bigint(a_bigint *tgt,
                               a_bigint *val)
/*
Copy *val to *tgt.
*/
{
  tgt->num_words = val->num_words;
  memcpy(tgt->words, val->words, val->num_words * sizeof(tgt->words[0]));
}  /* bigint_from_bigint */


STATIC void bigint_from_uint(a_bigint   *tgt,
                             an_fp_uint val)
/*
Set *tgt to value of val.
*/
{
  tgt->num_words = 0;
  while (val != 0) {
    check_assertion(tgt->num_words < BIGINT_WORDS);
    tgt->words[tgt->num_words] = (a_bigint_word)(val & BIGINT_WORD_MASK);
    ++tgt->num_words;
    val >>= BIGINT_WORD_BITS;
  }  /* while */
}  /* bigint_from_uint */


STATIC void bigint_from_fp_int(a_bigint      *tgt,
                               unsigned char *val,
                               int           width)
/*
Set *tgt to width-bit value pointed to by val.  Typically used to copy
mantissa of a binary floating-point value into a bigint object.
*/
{
  a_bigint_uint tmp = 0;
  int           cur_word = 0;
  int           cur_byte;

  for (cur_byte = 0; cur_byte < BYTE_COUNT(width); ) {
    tmp += (a_bigint_uint)val[cur_byte] <<
                     (CHAR_BIT * ((unsigned int)cur_byte % BIGINT_WORD_BYTES));
    if ((unsigned int)++cur_byte % BIGINT_WORD_BYTES == 0) {
      tgt->words[cur_word++] = (a_bigint_word)tmp;
      if (tmp != 0) {
        tgt->num_words = (unsigned int)cur_word;
      }  /* if */
      tmp = 0;
    }  /* if */
  }  /* for */
  if (tmp != 0) {
    tgt->words[cur_word++] = (a_bigint_word)tmp;
    tgt->num_words = (unsigned int)cur_word;
  }  /* if */
  check_assertion(tgt->num_words < BIGINT_WORDS);
}  /* bigint_from_fp_int */


STATIC void bigint_mult_int(a_bigint *tgt,
                            int      mult)
/*
Compute *tgt *= mult.  Requires: value of mult must fit in one word of
bigint's internal representation.
*/
{
  unsigned int  i;
  unsigned int  tnum = tgt->num_words;
  a_bigint_word *twords = tgt->words;
  a_bigint_uint tmp = 0;

  check_assertion(0 <= mult &&
		  (sizeof(mult) <= sizeof(a_bigint_word) ||
                   (unsigned)mult <= BIGINT_WORD_MAX)); /*lint !e506 !e685*/
  if (mult == 0) {
    tnum = 0;
  } else if (mult == 1) {
    /* Nothing to do */
  } else {
    for (i = 0; i < tnum; ++i) {
      tmp += twords[i] * (a_bigint_uint)mult;
      twords[i] = (a_bigint_word)(tmp & BIGINT_WORD_MASK);
      tmp >>= BIGINT_WORD_BITS;
    }  /* for */
    if (tmp != 0) {
      check_assertion(tnum < BIGINT_WORDS);
      twords[tnum] = (a_bigint_word)tmp;
      ++tnum;
    }  /* if */
  }  /* if */
  tgt->num_words = tnum;
}  /* bigint_mult_int */


STATIC void bigint_add_int(a_bigint      *tgt,
                           a_bigint_word add)
/*
Compute *tgt += add.
*/
{
  unsigned int  i;
  unsigned int  tnum = tgt->num_words;
  a_bigint_word *twords = tgt->words;
  a_bigint_uint tmp = add;

  for (i = 0; i < tnum && tmp != 0; ++i) {
    tmp += twords[i];
    twords[i] = (a_bigint_word)(tmp & BIGINT_WORD_MASK);
    tmp >>= BIGINT_WORD_BITS;
  }  /* for */
  if (tmp != 0) {
    check_assertion(tnum < BIGINT_WORDS);
    twords[tnum] = (a_bigint_word)tmp;
    ++tnum;
  }  /* if */
  tgt->num_words = tnum;
}  /* bigint_add_int */


STATIC void bigint_from_dec_mant(a_bigint            *tgt,
                                 an_fp_decimal_input *dec)
/*
Set *tgt to the value of the mantissa of *dec.
*/
{
  a_bigint_word   dec_fraction = 0;
  int             mult = 1;
  a_const_char    *cur;
  int             digits = dec->precision;

  /* Gather digits from integral part into tgt, accumulating into
     dec_fraction to avoid large integer math as much as possible. */
  for (cur = dec->first_int;
       cur != dec->last_int && digits != 0;
       ++cur, --digits, mult *= 10) {
    if (BIGINT_WORD_MAX / 10 <= (unsigned int)mult) {
      bigint_mult_int(tgt, mult);
      bigint_add_int(tgt, dec_fraction);
      dec_fraction = 0;
      mult = 1;
    }  /* if */
    dec_fraction *= 10;
    dec_fraction += (a_bigint_word)(*cur - '0');
  }  /* for */
  /* Gather digits from fraction part into tgt, accumulating into
     dec_fraction to avoid large integer math as much as possible. */
  for (cur = dec->first_frac;
       cur != dec->last_frac && digits!= 0;
       ++cur, --digits, mult *= 10) {
    if (BIGINT_WORD_MAX / 10 <= (unsigned int)mult) {
      bigint_mult_int(tgt, mult);
      bigint_add_int(tgt, dec_fraction);
      dec_fraction = 0;
      mult = 1;
    }  /* if */
    dec_fraction *= 10;
    dec_fraction += (a_bigint_word)(*cur - '0');
  }  /* for */
  if (mult != 1) {
    bigint_mult_int(tgt, mult);
    bigint_add_int(tgt, dec_fraction);
  }  /* if */
}  /* bigint_from_dec_mant */


STATIC int bigint_cmp(a_bigint *left,
                      a_bigint *right)
/*
Return <0, 0, >0 according to whether
*left < *right, *left == *right, *left > *right.
*/
{
  int res = (int)left->num_words - (int)right->num_words;

  if (res == 0 && left->num_words != 0) {
    a_bigint_word *lbegin = left->words;
    a_bigint_word *lend = lbegin + left->num_words;
    a_bigint_word *rend = right->words + right->num_words;
    while (lend != lbegin && res == 0) {
      if (*--lend != *--rend) {
        res = *lend < *rend ? -1 : 1;
      }  /* if */
    }  /* while */
  }  /* if */
  return res;
}  /* bigint_cmp */


STATIC a_boolean bigint_is_zero(a_bigint *val)
/*
Return TRUE if *val is zero, otherwise FALSE.
*/
{
  return (a_boolean)(val->num_words == 0);
}  /* bigint_is_zero */


static void small_shift_left(a_bigint *tgt,
                             int      shift,
                             int      pos)
/*
Shift bits starting at tgt->words[pos] left by shift bits.
Requires: shift is non-negative and less than word size.
*/
{
  check_assertion(0 <= shift && shift < BIGINT_WORD_BITS);
  check_assertion((unsigned int)pos < tgt->num_words);
  check_assertion(shift == 0 || pos + 1 <= BIGINT_WORDS);
  if (shift != 0 && tgt->num_words != 0) {
    a_bigint_uint tmp = 0;
    while ((unsigned int)pos < tgt->num_words) {
      tmp |= (a_bigint_uint)tgt->words[pos] << shift;
      tgt->words[pos] = (a_bigint_word)(tmp & BIGINT_WORD_MASK);
      tmp >>= BIGINT_WORD_BITS;
      ++pos;
    }  /* while */
    if (tmp != 0) {
      check_assertion(tgt->num_words < BIGINT_WORDS);
      tgt->words[tgt->num_words] = (a_bigint_word)tmp;
      ++tgt->num_words;
    }  /* if */
  }  /* if  */
}  /* small_shift_left */


STATIC void bigint_shift_left(a_bigint *tgt,
                              int      shift)
/*
Shift *tgt left by shift bits.  Requires: shift is non-negative.
*/
{
  if (shift != 0 && tgt->num_words != 0) {
    int offset = shift / BIGINT_WORD_BITS;
    check_assertion(tgt->num_words + (unsigned int)offset <= BIGINT_WORDS);
    if (offset != 0) {
      memmove(&tgt->words[offset], &tgt->words[0],
        tgt->num_words * BIGINT_WORD_STORAGE_BYTES);
      memset(&tgt->words[0], 0, (size_t)offset * BIGINT_WORD_STORAGE_BYTES);
      tgt->num_words += (unsigned int)offset;
    }  /* if */
    small_shift_left(tgt, shift % BIGINT_WORD_BITS, offset);
  }  /* if */
}  /* bigint_shift_left */


static void small_shift_right(a_bigint *tgt,
                              int      shift)
/*
Shift right by shift bits.
Requires: shift is non-negative and less than word size.
*/
{
  check_assertion(0 <= shift && shift < BIGINT_WORD_BITS);
  if (shift != 0 && tgt->num_words != 0) {
    a_bigint_uint tmp = 0;
    unsigned int  pos = tgt->num_words;
    while (pos != 0) {
      a_bigint_uint tmp0;
      --pos;
      tmp |= tgt->words[pos] >> shift;
      tmp0 = tgt->words[pos];
      tgt->words[pos] = (a_bigint_word)(tmp & BIGINT_WORD_MASK);
      tmp = (tmp0 << (BIGINT_WORD_BITS - shift)) & BIGINT_WORD_MASK;
    }  /* while */
    if (tgt->words[tgt->num_words - 1] == 0) {
      --tgt->num_words;
    }  /* if */
  }  /* if */
}  /* small_shift_right */


STATIC void bigint_shift_right(a_bigint *tgt,
                               int      shift)
/*
Shift *tgt right by shift bits.  Requires: shift is non-negative.
*/
{
  if (shift != 0 && tgt->num_words != 0) {
    int offset = shift / BIGINT_WORD_BITS;
    check_assertion((unsigned int)offset < tgt->num_words);
    if (offset != 0) {
      memmove(&tgt->words[0], &tgt->words[offset],
        (tgt->num_words - (unsigned)offset) * BIGINT_WORD_STORAGE_BYTES);
      tgt->num_words -= (unsigned)offset;
      while (tgt->num_words != 0 && tgt->words[tgt->num_words -1] == 0) {
        --tgt->num_words;
      }  /* while */
    }  /* if */
    small_shift_right(tgt, shift % BIGINT_WORD_BITS);
  }  /* if */
}  /* bigint_shift_right */


static int do_bigint_mult(a_bigint_word *twords,
                          a_bigint_word *lwords,
                          int           lnum,
                          a_bigint_word *rwords,
                          int           rnum,
                          int           tsize)
/*
Compute *twords = *lwords * *rwords.
lnum is the number of significant words in lwords; rnum is the number of
significant words in rwords; tsize is the size of the array that twords points
to.  Returns the number of significant words in *twords.
*/
{
  /* Using pointers is about 5% faster than indexing with gcc. */
  a_bigint_word *lbegin = lwords;
  a_bigint_word *lend = lbegin + lnum;
  int           tnum = lnum + rnum;

  check_assertion(tnum <= tsize);
  memset(twords, 0, (size_t)tnum * BIGINT_WORD_STORAGE_BYTES);
  while (lbegin != lend) {
    a_bigint_uint fact = *lbegin;
    if (fact != 0) {
      a_bigint_uint tmp = 0;
      a_bigint_word *rbegin = rwords;
      a_bigint_word *rend = rbegin + rnum;
      a_bigint_word *cur = twords + (lbegin - lwords);
      while (rbegin != rend) {
        tmp += *cur + fact * *rbegin;
        *cur = (a_bigint_word)(tmp & BIGINT_WORD_MASK);
        tmp >>= BIGINT_WORD_BITS;
        ++cur;
        ++rbegin;
      }  /* while */
      if (tmp != 0) {
        if (cur - twords == tnum) {
          check_assertion(tnum < tsize);
          ++tnum;
          *cur = 0;
        }  /* if */
        tmp += *cur;
        *cur = (a_bigint_word)(tmp & BIGINT_WORD_MASK);
        check_assertion((tmp >> BIGINT_WORD_BITS) == 0);
      }  /* if */
    }  /* if */
    ++lbegin;
  }  /* while */
  while (tnum != 0 && twords[tnum -1] == 0) {
    --tnum;
  }  /* while */
  return tnum;
}  /* do_bigint_mult */


static a_bigint *bigint_mult(a_bigint *left,
                             a_bigint *right)
/*
Return *left * *right.
*/
{
  a_bigint *res = new_bigint();

  if (left->num_words == 0 || right->num_words == 0) {
    /* Nothing to do.  Result is 0. */
  } else {
    if (left->num_words < right->num_words) {
      a_bigint *tmp = left;
      left = right;
      right = tmp;
    }  /* if */
    res->num_words = (unsigned int)do_bigint_mult(res->words,
                                                  left->words,
                                                  (int)left->num_words,
                                                  right->words,
                                                  (int)right->num_words,
                                                  BIGINT_WORDS);
  }  /* if */
  return res;
}  /* bigint_mult */


/* Storage for large powers of 5, for use in bigint_5_2_n. */
STATIC_THREAD a_bigint
                large_fives[FIVE_TWO_CACHE_SIZE];
                        /* A cache for 5^2^n values.  These values are
                           computed when needed and stored here for future
                           use. */
STATIC_THREAD int
                large_fives_inited;
                        /* The number of entries in large_fives that have
                           valid values. */
STATIC_THREAD a_bigint
                large_fives_static;
                        /* A static a_bigint object returned by bigint_5_2_n
                           when the requested value is larger than one
                           found in the cache. */


static a_bigint *bigint_5_2_n(int n)
/*
Return 5^2^(n+2).  Caches up to FIVE_TWO_CACHE_SIZE previously-computed
values.  The value returned is a pointer to a statically allocated object
and the caller must be careful that its value doesn't change (i.e., if
this routine is called subsequently).
*/
{
  a_bigint *res = 0;
  a_bigint *tmp;

  check_assertion(0 <= n);
  if (large_fives_inited == 0) {
    bigint_from_uint(&large_fives[0], (an_fp_uint)5*5*5*5);
    large_fives_inited = 1;
  }  /* if */
  while (large_fives_inited < FIVE_TWO_CACHE_SIZE && large_fives_inited <= n) {
    tmp = bigint_mult(&large_fives[large_fives_inited - 1],
                      &large_fives[large_fives_inited - 1]);
    bigint_from_bigint(&large_fives[large_fives_inited], tmp);
    delete_bigint(tmp);
    ++large_fives_inited;
  }  /* while */
  if (n < large_fives_inited) {
    res = &large_fives[n];
  } else {
    /* n is larger than table size, so compute by hand. */
    bigint_from_bigint(&large_fives_static,
                       &large_fives[large_fives_inited - 1]);
    n -= large_fives_inited - 1;
    while (n-- != 0) {
      tmp = bigint_mult(&large_fives_static, &large_fives_static);
      bigint_from_bigint(&large_fives_static, tmp);
      delete_bigint(tmp);
    }  /* while */
    res = &large_fives_static;
  }  /* if */
  check_assertion(res != 0);
  return res;
}  /* bigint_5_2_n */


/* Small powers of 5, for use in bigint_do_mult_pow5. */
static constexpr int pow5s[] =
{
  1,
  5,
  5*5,
  5*5*5
};


static a_bigint *bigint_do_mult_pow5(a_bigint *val,
                                     int      pow5)
/*
Return a pointer to an a_bigint object that holds val*5^pow5.
Caller should delete the returned object.
*/
{
  int      large_five;
  a_bigint *res = new_bigint();

  bigint_from_bigint(res, val);
  if ((pow5 & 0x03) != 0) {
    bigint_mult_int(res, pow5s[pow5 & 0x03]);
  }  /* if */
  pow5 >>= 2;
  large_five = 0;
  for ( ; pow5 != 0; pow5 >>= 1, ++large_five) {
    if (pow5 & 1) {
      a_bigint *tmp = bigint_mult(res, bigint_5_2_n(large_five));
      delete_bigint(res);
      res = tmp;
    }  /* if */
  }  /* for */
  return res;
}  /* bigint_do_mult_pow5 */


STATIC void bigint_mult_pow5(a_bigint *tgt,
                             int      pow5)
/*
Compute *tgt *= 5^pow5 without using fives cache.
*/
{
  if (pow5 != 0) {
    a_bigint *res = bigint_do_mult_pow5(tgt, pow5);
    bigint_from_bigint(tgt, res);
    delete_bigint(res);
  }  /* if */
}  /* bigint_mult_pow5 */


STATIC void bigint_add(a_bigint *tgt,
                       a_bigint *val)
/*
Compute *tgt += *val.
*/
{
  unsigned int  i;
  a_bigint_uint tmp = 0;

  check_assertion(tgt != val);
  if (tgt->num_words < val->num_words) {
    memset(&tgt->words[tgt->num_words], 0,
           (val->num_words - tgt->num_words) * BIGINT_WORD_STORAGE_BYTES);
    tgt->num_words = val->num_words;
  }  /* if */
  for (i = 0; i < val->num_words; ++i) {
    tmp += (a_bigint_uint)tgt->words[i] + val->words[i];
    tgt->words[i] = (a_bigint_word)(tmp & BIGINT_WORD_MASK);
    tmp >>= BIGINT_WORD_BITS;
  }  /* for */
  for ( ; i < tgt->num_words; ++i) {
    tmp += tgt->words[i];
    tgt->words[i] = (a_bigint_word)(tmp & BIGINT_WORD_MASK);
    tmp >>= BIGINT_WORD_BITS;
  }  /* for */
  if (tmp != 0) {
    check_assertion(tgt->num_words < BIGINT_WORDS);
    tgt->words[tgt->num_words] = (a_bigint_word)tmp;
    ++tgt->num_words;
  }  /* if */
}  /* bigint_add */


STATIC void bigint_sub(a_bigint *tgt,
                       a_bigint *sub)
/*
Compute *tgt -= *sub.
Requires: *sub <= *tgt.
*/
{
  unsigned int i;
  a_bigint_int tmp = 0;
  int          borrow;

  check_assertion(tgt != sub);
  check_assertion(sub->num_words <= tgt->num_words);
  for (i = 0; i < sub->num_words; ++i) {
    borrow = 0;
    tmp += tgt->words[i];
    tmp -= sub->words[i];
    if (tmp < 0) {
      /* Handle underflow by borrowing. */
      tmp += (a_bigint_int)(BIGINT_WORD_MAX + 1);
      borrow = -1;
    }  /* if */
    check_assertion(0 <= tmp);
    tgt->words[i] = (a_bigint_word)tmp & BIGINT_WORD_MASK;
    tmp >>= BIGINT_WORD_BITS;
    tmp += borrow;
  }  /* for */
  for ( ; tmp != 0; ++i) {
    borrow = 0;
    check_assertion(i < tgt->num_words);
    tmp += tgt->words[i];
    if (tmp < 0) {
      /* Handle underflow by borrowing. */
      tmp += (a_bigint_int)(BIGINT_WORD_MAX + 1);
      borrow = -1;
    }  /* if */
    check_assertion(0 <= tmp);
    tgt->words[i] = (a_bigint_word)tmp & BIGINT_WORD_MASK;
    tmp >>= BIGINT_WORD_BITS;
    tmp += borrow;
  }  /* for */
  /* Remove high-order zeros. */
  while (tgt->num_words != 0 && tgt->words[tgt->num_words - 1] == 0) {
    --tgt->num_words;
  }  /* while */
}  /* bigint_sub */


STATIC int bigint_abs_diff(a_bigint *tgt,
                           a_bigint *left,
                           a_bigint *right)
/*
Compute *tgt = abs(*left - *right).
Returns <0, 0, >0 according to whether
*left < *right, *left == *right, *left > *right.
*/
{
  int sgn = bigint_cmp(left, right);

  if (sgn < 0) {
    bigint_from_bigint(tgt, right);
    bigint_sub(tgt, left);
  } else if (0 < sgn) {
    bigint_from_bigint(tgt, left);
    bigint_sub(tgt, right);
  } else {
    tgt->num_words = 0;
  }  /* if */
  return sgn;
}  /* bigint_abs_diff */


STATIC int bigint_divmod(a_bigint *tgt,
                         a_bigint *divisor)
/*
Compute *tgt / *divisor, returning quotient and putting remainder in *tgt.
Assumes that quotient will be a small non-negative integer value.
*/
{
  unsigned int  i;
  a_bigint_word trial_quotient;
  a_bigint_int  tmp = 0;

  check_assertion(tgt != divisor);
  check_assertion(divisor->num_words != 0);  /* Don't divide by 0. */
  if (tgt->num_words < divisor->num_words ||
      (tgt->num_words == divisor->num_words &&
       tgt->words[tgt->num_words - 1] <
                                     divisor->words[divisor->num_words - 1])) {
    trial_quotient = 0;
    goto end_of_routine;
  }  /* if */
  check_assertion(tgt->num_words - divisor->num_words < 2);
  /* Compute trial quotient that is no larger than actual quotient,
     adjust later if it's too small. */
  trial_quotient =
    tgt->words[tgt->num_words - 1] /
                                  (divisor->words[divisor->num_words - 1] + 1);
  if (trial_quotient != 0) {
    /* *tgt -= trial_quotient * *divisor */
    tmp = 0;
    for (i = 0; i < divisor->num_words; ++i) {
      int borrow = 0;
      tmp += (a_bigint_int)tgt->words[i] -
             (a_bigint_int)divisor->words[i] * trial_quotient;
      if (tmp < 0) {
        borrow = -1;
        tmp += (a_bigint_int)(BIGINT_WORD_MAX + 1);
      }  /* if */
      tgt->words[i] = (a_bigint_word)tmp & BIGINT_WORD_MASK;
      tmp >>= BIGINT_WORD_BITS;
      tmp += borrow;
    }  /* for */
  }  /* if */
  check_assertion(tmp == 0);
  /* Remove high-order zeros. */
  while (tgt->num_words != 0 && tgt->words[tgt->num_words - 1] == 0) {
    --tgt->num_words;
  }  /* while */
  /* Adjust if trial quotient was too small. */
  while (bigint_cmp(tgt, divisor) >= 0) {
    ++trial_quotient;
    bigint_sub(tgt, divisor);
  }  /* while */
  /* Don't need to remove high-order zeros here; main loop did it, or
     bigint_sub did it.  */
end_of_routine:
  return (int)trial_quotient;
}  /* bigint_divmod */


STATIC a_boolean fp_frac_eq_zero(unsigned char *frac,
                                 int           width)
/*
Return TRUE if frac holds zeros in all width bits.
*/
{
  return (a_boolean)(fp_frac_high_zero_bits(frac, width) == width);
}  /* fp_frac_eq_zero */

#if FP_USE_EMULATION
/*
The floating-point emulator provides a task-oriented set of operations; it is
not a full emulator, but provides only the operations needed by the rest of the
library.  It operates on values of type an_fp_binary rather than floating-point
values to avoid dependencies on the representation of floating-point values.
*/

/*
Emulator holds enough bits to multiply two values of size MAX_FRAC_BYTES plus
one pad byte for use by fp_frac_shift_right.
*/
#define ACCUM_BYTES (MAX_FRAC_BYTES * 2 + 1)


static void copy_accum_and_normalize(an_fp_binary  *tgt,
                                     unsigned char *accum,
                                     int           acc_bits)
/*
Copy the fraction in accum, consisting of acc_bits bits, into the
fraction in *tgt, rounding to fit if necessary, and adjusting the exponent
in *tgt if rounding changed it.
*/
{
  int hi_z_bits = fp_frac_high_zero_bits(accum, acc_bits);
  int low_bit_pos = acc_bits - hi_z_bits - tgt->precision;
  int adjust = hi_z_bits;

  if (acc_bits == hi_z_bits) {
    tgt->type = fpt_zero;
  } else if (low_bit_pos < 0) {
    fp_frac_shift_left(accum, acc_bits, -low_bit_pos);
  } else if (0 < low_bit_pos) {
    fp_frac_shift_right(accum, acc_bits, low_bit_pos);
    if (BIT_AT(accum, acc_bits, tgt->precision) != 0) {
      /* Rounding up produced overflow; adjust down to fix.
         All low bits are zero, so just need to set the top bit. */
      if (tgt->precision % BYTE_SIZE == 0) {
        accum[(tgt->precision - 1) / BYTE_SIZE] = MASK_BIT(BYTE_SIZE - 1);
      } else {
        accum[tgt->precision / BYTE_SIZE] >>= 1;
      }  /* if */
      --adjust;
    }  /* if */
  }  /* if */
  fp_frac_copy(tgt->frac, accum, tgt->precision);
  tgt->exponent -= adjust;
}  /* copy_accum_and_normalize */


STATIC void fp_emul_set_to_zero(an_fp_binary *bin)
/*
Set *bin to 0.
*/
{
  bin->type = fpt_zero;
}  /* fp_emul_set_to_zero */


STATIC void fp_emul_copy(an_fp_binary *left,
                         an_fp_binary *right)
/*
Copy *right to *left.
*/
{
  memcpy(left, right, sizeof(*left));
}  /* fp_emul_copy */


STATIC a_boolean fp_emul_lt(an_fp_binary *left,
                            an_fp_binary *right)
/*
Return TRUE if *left < *right, otherwise FALSE.  Returns FALSE when either
value is NaN.
*/
{
  a_boolean res;

  if (right->type == fpt_zero) {
    res = FALSE;
  } else if (left->type == fpt_zero) {
    res = TRUE;
  } else if (left->exponent != right->exponent) {
    res = (a_boolean)(left->exponent < right->exponent);
  } else {
    res = (a_boolean)(fp_frac_lt(left->frac, right->frac, left->precision));
  }  /* if */
  return res;
}  /* fp_emul_lt */


STATIC a_boolean fp_emul_is_zero(an_fp_binary *bin)
/*
Return TRUE if *bin is zero, otherwise FALSE.
*/
{
  return (a_boolean)(bin->type == fpt_zero);
}  /* fp_emul_is_zero */


STATIC void fp_emul_abs(an_fp_binary *bin)
/*
Set *bin to |*bin|.
*/
{
  bin->is_negative = FALSE;
}  /* fp_emul_abs */


STATIC void fp_emul_negate(an_fp_binary *bin)
/*
Set *bin = -*bin;
*/
{
  bin->is_negative = !bin->is_negative;
}  /* fp_emul_negate */


static void fp_emul_scale_and_add(an_fp_binary *tgt,
                                  an_fp_binary *left,
                                  an_fp_binary *right)
/*
Add *right to value in *left.
Exponent of *left is never less than exponent of *right.
*/
{
  int           rbytes = BYTE_COUNT(right->precision);
  int           lbytes = BYTE_COUNT(left->precision);
  int           abytes = rbytes + lbytes;
  unsigned char accum[ACCUM_BYTES + 2] = { 0 };
  unsigned char shifted_left[MAX_FRAC_BYTES] = { 0 };
  unsigned char *left_frac;
  unsigned      tmp;
  int           i;

  for (i = 0; i < rbytes; ++i) {
    accum[lbytes + i] = right->frac[i];
  }  /* for */
  fp_frac_shift_right(accum, abytes * BYTE_SIZE,
                      left->exponent - right->exponent);
  if (left->precision % BYTE_SIZE == 0) {
    left_frac = left->frac;
  } else {
    for (i = 0; i < lbytes; ++i) {
      shifted_left[i] = left->frac[i];
    }  /* for */
    fp_frac_shift_left(shifted_left, lbytes * BYTE_SIZE,
                       BYTE_SIZE - left->precision % BYTE_SIZE);
    fp_frac_shift_left(accum, abytes * BYTE_SIZE,
                       BYTE_SIZE - left->precision % BYTE_SIZE);
    left_frac = shifted_left;
  }  /* if */
  tmp = 0;
  for (i = 0; i < lbytes; ++i) {
    tmp += accum[rbytes + i];
    tmp += left_frac[i];
    accum[rbytes + i] = tmp & BYTE_MASK;
    tmp >>= BYTE_SIZE;
  }  /* for */
  tgt->exponent = left->exponent;
  if (tmp != 0) {
    accum[abytes] = tmp & BYTE_MASK;
    ++abytes;
    tgt->exponent += BYTE_SIZE;
    check_assertion((tmp >> BYTE_SIZE) == 0);
  }  /* if */
  copy_accum_and_normalize(tgt, accum, abytes * BYTE_SIZE);
}  /* fp_emul_scale_and_add */


STATIC void fp_emul_add(an_fp_binary *left,
                        an_fp_binary *right)
/*
*left += *right;
*/
{
  if (right->type == fpt_zero) {
    /* Nothing to do. */
  } else if (left->type == fpt_zero) {
    fp_emul_copy(left, right);
  } else if (left->exponent < right->exponent) {
    fp_emul_scale_and_add(left, right, left);
  } else {
    fp_emul_scale_and_add(left, left, right);
  }  /* if */
}  /* fp_emul_add */


STATIC void fp_emul_add_int(an_fp_binary  *left,
                            unsigned long right)
/*
*left += right
*/
{
  if (right != 0) {
    an_fp_binary  bin = {};
    int           i;
    unsigned long high_bits_mask = ~MASK_BITS(left->precision);
    unsigned long over;

    bin.type = fpt_number;
    bin.is_negative = FALSE;
    bin.precision = left->precision;
    fp_frac_set_from_uint(bin.frac, bin.precision,
                          (an_fp_uint)right);
    if (bin.precision < (int)(CHAR_BIT * sizeof(right)) &&
        right >= (1ul << bin.precision)) {
      /* By definition, there are no leading zero bits if the value is
         larger than the precision. */
      i = 0;
    } else {
      i = fp_frac_high_zero_bits(bin.frac, bin.precision);
    }  /* if */
    if (i == 0 && bin.precision < (int)(CHAR_BIT * sizeof(right)) &&
        (over = (right & high_bits_mask) >> bin.precision) != 0) {
      /* The value being added occupies more bits than allowed by the
         precision.  Count the number of excess bits and increment the
         exponent to reflect the eventual adjustment (right shift) that
         will be done to represent the value in the number of bits of
         precision. */
      while (over != 0) {
	++i;
	over >>= 1;
      }  /* while */
      /* The -1 in the adjustment to the exponent reflects the implicit
         high-order "1" bit in the fraction.  E.g., if over is initially
         the two bits "10", the precision will be incremented by 1, since
         the initial "1" in the fraction will be implicit. */
      bin.exponent = bin.precision + i - 1;
    } else {
      /* Decrement the exponent and shift the value to normalize the value
         so that the high-order bit of the fraction is a "1". */
      bin.exponent = bin.precision - i;
      fp_frac_shift_left(bin.frac, bin.precision, i);
    }  /* if */
    fp_emul_add(left, &bin);
  }  /* if */
}  /* fp_emul_add_int */


STATIC void fp_emul_sub(an_fp_binary *left,
                        an_fp_binary *right)
/*
Subtract *right from *left and store the result in *left.
Assumes that *right <= *left.
*/
{
  int           rbytes = BYTE_COUNT(right->precision);
  int           lbytes = BYTE_COUNT(left->precision);
  int           abytes = rbytes + lbytes;
  unsigned char accum[ACCUM_BYTES + 2] = { 0 };
  unsigned char sub[ACCUM_BYTES + 2] = { 0 };
  int           i;
  unsigned      tmp;

  if (right->type != fpt_zero) {
    for (i = 0; i < rbytes; ++i) {
      sub[lbytes + i] = right->frac[i];
    }  /* for */
    fp_frac_shift_right(sub, (lbytes + i) * BYTE_SIZE,
                        left->exponent - right->exponent);

    for (i = 0; i < lbytes; ++i) {
      accum[rbytes + i] = left->frac[i];
    }  /* for */
    if (left->precision % BYTE_SIZE != 0) {
      fp_frac_shift_left(accum, abytes * BYTE_SIZE,
                         BYTE_SIZE - left->precision % BYTE_SIZE);
      fp_frac_shift_left(sub, abytes * BYTE_SIZE,
                         BYTE_SIZE - left->precision % BYTE_SIZE);
    }  /* if */
    tmp = 0;
    for (i = 0; i < ACCUM_BYTES + 2; ++i) {
      tmp += sub[i];
      if (tmp <= accum[i]) {
        accum[i] -= (unsigned char)tmp;
        tmp = 0;
      } else {
        accum[i] -= (unsigned char)tmp;
        tmp = 1;
      }  /* if */
    }  /* for */
    check_assertion(tmp == 0);
    copy_accum_and_normalize(left, accum, (lbytes + rbytes) * BYTE_SIZE);
  }  /* if */
}  /* fp_emul_sub */


STATIC void fp_emul_mult_int(an_fp_binary  *left,
                             unsigned      right)
/*
Multiply *left by right and store the result in *left.
*/
{
  if (right == 0) {
    left->type = fpt_zero;
  } else if (left->type == fpt_zero) {
    /* Nothing to do. */
  } else {
    int           i;
    unsigned char accum[MAX_FRAC_BYTES + 1] = { 0 };
    int           bytes = BYTE_COUNT(left->precision);
    unsigned int  tmp = 0;

    for (i = 0; i < bytes; ++i) {
      tmp += left->frac[i] * right;
      accum[i] =  tmp & BYTE_MASK;
      tmp >>= BYTE_SIZE;
    }  /* for */
    accum[i] = (unsigned char)tmp;
    left->exponent += BYTE_SIZE;
    if (left->precision % BYTE_SIZE != 0)
      left->exponent += BYTE_SIZE - (left->precision % BYTE_SIZE);
    copy_accum_and_normalize(left, accum, (bytes + 1) * BYTE_SIZE);
  }  /* if */
}  /* fp_emul_mult_int */


STATIC void fp_emul_mult(an_fp_binary *left,
                         an_fp_binary *right)
/*
*left *= *right;
*/
{
  unsigned char accum[ACCUM_BYTES] = { 0 };
  int           lbytes = BYTE_COUNT(left->precision);
  int           rbytes = BYTE_COUNT(right->precision);
  int           i;
  int           j;
  unsigned int  tmp;

  if (fp_emul_is_zero(left)) {
    /* Nothing to do. */
  } else if (fp_emul_is_zero(right)) {
    fp_emul_set_to_zero(left);
  } else {
    for (i = 0; i < lbytes; ++i) {
      if (left->frac[i] != 0) {
        tmp = 0;
        for (j = 0; j < rbytes; ++j) {
          tmp += accum[i + j] + (unsigned int)(left->frac[i] * right->frac[j]);
          accum[i + j] = tmp & BYTE_MASK;
          tmp >>= BYTE_SIZE;
        }  /* for */
        accum[i + j] = tmp & BYTE_MASK;
        check_assertion((tmp >> BYTE_SIZE) == 0);
      }  /* if */
    }  /* for */
    left->exponent += right->exponent;
    copy_accum_and_normalize(left, accum, left->precision + right->precision);
  }  /* if */
}  /* fp_emul_mult */


static int fp_emul_get_quotient_digit(unsigned char *u,
                                      unsigned char *v,
                                      int           j,
                                      int           n)
/*
Divide *accum by *v, leaving the remainder in *accum and returning the
integral quotient.

Part of implementation of Knuth's Algorithm D, from "The Art of Computer
Programming", Volume 2, Seminumerical Algorithms (3rd edition).
*/
{
  int           qh;
  int           rh;
  int           b = BYTE_MAX + 1;
  unsigned char intermediate[MAX_FRAC_BYTES + 1] = { 0 };

  qh = (u[j + n] * b + u[j + n - 1]) / v[n - 1];
  rh = (u[j + n] * b + u[j + n - 1]) % v[n - 1];
rpt:
  if (qh == b || b * rh + u[j + n - 2] < qh * v[n - 2]) {
    --qh;
    rh += v[n - 1];
    if (rh < b) {
      goto rpt;
    }  /* if */
  }  /* if */
  /* D4: Multiply and subtract. */
  /* D5: Test remainder. */
  fp_frac_copy(intermediate, v, n * BYTE_SIZE);
  fp_frac_mult_int(intermediate, (n + 1) * BYTE_SIZE, qh);
  if (fp_frac_lt(u + j, intermediate, (n + 1) * BYTE_SIZE)) {
    /* D6: Add back. */
    --qh;
    fp_frac_sub(intermediate, v, (n + 1) * BYTE_SIZE);
  }  /* if */
  fp_frac_sub(u + j, intermediate, (n + 1) * BYTE_SIZE);
  return qh;
}  /* fp_emul_get_quotient_digit */


STATIC void fp_emul_div(an_fp_binary *num,
                        an_fp_binary *den)
/*
Compute *num /= *den.

Uses Knuth's Algorithm D, from "The Art of Computer Programming", Volume 2,
Seminumerical Algorithms (3rd edition).
*/
{
  int           j;
  unsigned char u[ACCUM_BYTES + 1] = { 0 };
  unsigned char result[MAX_FRAC_BYTES + 2] = { 0 };
  int           dbytes = BYTE_COUNT(den->precision);
  int           rwidth = (dbytes + 2) * BYTE_SIZE;
  int           d;
  int           b = BYTE_MAX + 1;
  unsigned char *v = den->frac;
  unsigned char shifted_den[MAX_FRAC_BYTES + 1] = { 0 };
  int           m = BYTE_COUNT(num->precision) + 1;
  int           n = dbytes;

  fp_frac_copy(u + n + 1, num->frac, num->precision);
  if (den->precision % BYTE_SIZE != 0) {
    for (j = 0; j < n; ++j) {
      shifted_den[j] = den->frac[j];
    }  /* for */
    fp_frac_shift_left(u, (m + n) * BYTE_SIZE,
                       BYTE_SIZE - den->precision % BYTE_SIZE);
    fp_frac_shift_left(shifted_den, n * BYTE_SIZE,
                       BYTE_SIZE - den->precision % BYTE_SIZE);
    v = shifted_den;
  }  /* if */
  /* D1: Normalize. */
  d = b / (v[n - 1] + 1);
  if (1 < d) {
    fp_frac_mult_int(u, m + n - 1, d);
    fp_frac_mult_int(v, n, d);
  }  /* if */
  /* D2: Initialize. */
  for (j = m; 0 <= j; --j) {
    result[j] = (unsigned char)fp_emul_get_quotient_digit(u, v, j, n);
  /* D7: Loop on j. */
  }  /* for */
  if (fp_frac_eq_zero(u, (m + n - 1) * BYTE_SIZE) == FALSE &&
      (result[0] == BIT_MASK(BYTE_SIZE - 1) || result[0] == 0)) {
    /* Ensure that round-to-even rounds up when result[0] has only high
       bit set but there is a remainder.  */
    ++result[0];
  }  /* if */
  /* D8: Un-normalize. */
  if (1 < d) {
    fp_frac_div_int(v, n, d);
  }  /* if */
  num->exponent -= den->exponent;
  num->exponent += BYTE_SIZE;
  copy_accum_and_normalize(num, result, rwidth);
}  /* fp_emul_div */


static int fp_emul_to_int(an_fp_binary *val)
/*
Extract the integer value from val.  Assumes that the result is small, i.e.,
less than 10.
*/
{
  int res = 0;
  if (val->exponent <= 0) {
    /* Nothing to do. */
  } else if (val->exponent <= HIGH_BYTE_BIT_COUNT(val->precision)) {
    /* Extract high bits from high byte. */
    res = val->frac[BYTE_COUNT(val->precision) - 1] >>
          (HIGH_BYTE_BIT_COUNT(val->precision) - val->exponent);
  } else {
    /* Piece together bits from high byte with high bits of
       next byte. */
    res = val->frac[BYTE_COUNT(val->precision) - 1] <<
                         (val->exponent - HIGH_BYTE_BIT_COUNT(val->precision));
    res += val->frac[BYTE_COUNT(val->precision) - 2] >>
           (BYTE_SIZE - (val->exponent - HIGH_BYTE_BIT_COUNT(val->precision)));
  }  /* if */
  return res;
}  /* fp_emul_to_int */

#else /* !FP_USE_EMULATION */

/* Use non-emulation versions. */
#define fp_emul_set_to_zero(val)      (*(val) = 0)
#define fp_emul_copy(left, right)     (*(left) = *(right))
#define fp_emul_lt(left, right)       (*(left) < *(right))
#define fp_emul_is_zero(val)          (*(val) == 0)
#define fp_emul_abs(val)              ((*(val) = *(val) < 0 ? -*(val): *(val)))
#define fp_emul_negate(val)           (*(val) = -*(val))
#define fp_emul_add_int(left, right)  (*(left) += (right))
#define fp_emul_sub(left, right)      (*(left) -= *(right))
#define fp_emul_mult(left, right)     (*(left) *= *(right))
#define fp_emul_mult_int(left, right) (*(left) *= (right))
#define fp_emul_div(left, right)      (*(left) /= *(right))
#define fp_emul_to_int(val)           ((int)*(val))

#endif  /* FP_USE_EMULATION */

static int do_round(int  idx,
                    char *digits)
/*
Round up, adjusting characters in the range [digits, digits + idx).
Returns new past-the-end index.
Special case: if all digits were 9, sets digits[0] to '1'
and returns 0.  This signals caller to increase decimal exponent by 1
and use 1 as past-the-end index.
*/
{
  while (1 <= idx && digits[idx - 1]++ == '9' ) {
    --idx;
  }  /* while */
  if (idx == 0) {
    digits[0] = '1';
  }  /* if */
  return idx;
}  /* do_round */


STATIC void bigint_div_ceil(a_bigint     *tgt,
                            unsigned int divisor)
/*
Compute *tgt /= divisor, rounded up.
*/
{
  unsigned int  i;
  a_bigint_uint tmp = 0;

  check_assertion(divisor != 0);
  for (i = tgt->num_words; i != 0; ) {
    --i;
    tmp |= tgt->words[i];
    tgt->words[i] = (a_bigint_word)(tmp / /*lint --e(414)*/divisor);
    tmp = (tmp % /*lint --e(414)*/divisor) << BIGINT_WORD_BITS;
  }  /* for */
  /* Round up: */
  if (tmp != 0) {
    bigint_add_int(tgt, 1);
  }  /* if */
  /* Remove high-order zeros: */
  while (tgt->num_words != 0 && tgt->words[tgt->num_words - 1] == 0) {
    --tgt->num_words;
  }  /* while */
}  /* bigint_div_ceil */


STATIC void bin2dec(an_fp_decimal *dec,
                    an_fp_binary  *bin,
                    int           ndigits,
                    int           scale)
/*
Convert broken-down binary floating-point value in bin to broken-down decimal
floating-point value in dec.  When ndigits is FP_SHORTEST, generates shortest
(but no less than 2 digits for Java) digit sequence that will distinguish the
value from its two neighbors.  Otherwise, generates up to ndigits digits, using
bankers rounding if the tail of the value is not zero.  When scale is non-zero,
effectively reduces the precision of the result by 2^scale; this produces
proper shortest digit strings for subnormal values.  Assumes that the value
being converted is not a zero, an infinity, or a NaN.
*/
{
  int      digit;
  int      bin_exp;
  int      conv_finished = 0;
  long     result_scale = 0;
  int      dig_pos = 0;
  int      adjust_upper_delta = 0;
  long     r2 = 0, r5 = 0, s2 = 0, s5 = 0;
  int      min_digits = 0;
  a_bigint *residual = new_bigint();
  a_bigint *divisor = new_bigint();
  a_bigint *delta = new_bigint();
  a_bigint *temp0 = new_bigint();
  a_bigint *temp1 = new_bigint();
  a_bigint *tmp = 0;

  check_assertion(!fp_frac_eq_zero(bin->frac, bin->precision));
  /* Initialize factors for residual and divisor so that residual/divisor ==
     bin. */
  bin_exp = bin->exponent - bin->precision;
  if (bin_exp == 0) {
    /* Nothing to do. */
  } else if (bin_exp < 0) {
    /* divisor = divisor * 2^-bin_exp */
    s2 += -bin_exp;
  } else {
    /* residual = residual * 2^bin_exp */
    r2 += bin_exp;
    if (bin->precision == 11) {
      /* The exponent of a _Float16 value is greater than the precision,
         indicating a large integer value.  Ensure that all the digits are
         represented in the result. */
      min_digits = (bin->exponent > 13) ? 5 : 4;
    }  /* if */
  }  /* if */
  /* Normalize, so that 0.1 <= residual/divisor < 1.0 and residual/divisor ==
     bin / 10^result_scale.  */
  result_scale = LOG10_2times(bin->exponent);
  if (result_scale == 0) {
    /* Nothing to do. */
  } else if (result_scale < 0) {
    r2 += -result_scale;
    r5 += -result_scale;        /* residual = residual * 10^-result_scale */
  } else {
    s2 += result_scale;
    s5 += result_scale;         /* divisor = divisor * 10^result_scale */
  }  /* if */
  /* Remove common factors of 2 from residual and divisor. */
  if (r2 == s2) {
    /* Nothing to do. */
  } else if (r2 < s2) {
    s2 -= r2;
    r2 = 0;
  } else {
    r2 -= s2;
    s2 = 0;
  }  /* if */
  /* Upper delta is equal to delta except when fraction is exactly .1 binary.
     Then the gap between fraction and the next higher value is twice
     as large as the gap between fraction and the next lower value, so we have
     to adjust the upper delta value and the residual and divisor values. */
  if (fp_frac_eq_min_frac(bin->frac, bin->precision)) {
    adjust_upper_delta = 1;
  }  /* if */
  /* Calculate delta = 1/2 ULP.  */
  bigint_from_uint(delta, (an_fp_uint)1);
  bigint_mult_pow5(delta, (int)r5);
  bigint_shift_left(delta, (int)r2);
  /* Calculate residual = the floating-point value, scaled to match the value
     that we're about to compute for divisor.  Since we've already done the
     powers of ten for delta, we compute this as bin->fraction * delta * 2. */
  bigint_from_fp_int(residual, bin->frac, bin->precision);
  tmp = bigint_mult(residual, delta);
  bigint_shift_left(tmp, 1 + adjust_upper_delta);
  delete_bigint(residual);
  residual = tmp;
  /* Calculate divisor. */
  bigint_from_uint(divisor, (an_fp_uint)1);
  bigint_mult_pow5(divisor, (int)s5);
  bigint_shift_left(divisor, (int)(s2 + 1 + adjust_upper_delta));
  /* While residual < divisor/10, adjust residual and lower_limit. */
  bigint_from_bigint(temp0, divisor);
  bigint_div_ceil(temp0, 10);
  while (bigint_cmp(residual, temp0) < 0) {
    --result_scale;
    bigint_mult_int(residual, 10);
    bigint_mult_int(delta, 10);
  }  /* while */
  /* While divisor <= residual, adjust divisor. */
  while (bigint_cmp(divisor, residual) <= 0) {
    ++result_scale;
    bigint_mult_int(divisor, 10);
  }  /* while */
  /* Adjust delta for subnormal values. */
  if (scale != 0) {
    bigint_shift_left(delta, scale);
  }  /* if */
  /* Generate digits: residual *= 10, digit = residual/divisor,
     residual %= divisor. */
  while (!conv_finished) {
    /* Extract and store digit, adjust residual and limits. */
    bigint_mult_int(residual, 10);
    digit = bigint_divmod(residual, divisor);
    check_assertion(dig_pos < MAX_DIGITS && digit >= 0 && digit <= 9);
    dec->digits[dig_pos++] = '0' + (char)digit;
    /* Check for termination. */
    if (ndigits == FP_SHORTEST) {
      /* Check for shortest string that correctly rounds to binary value. */
      int cmp;
      bigint_mult_int(delta, 10);
      cmp = bigint_cmp(residual, delta);
#if JAVA_STYLE_SHORTEST
      if (dig_pos == 1) {
        /* Don't stop with 1 digit. */
      } else
#endif  /* JAVA_STYLE_SHORTEST */
      /* Do not insert code here. */
      {
        if (cmp < 0 || (cmp == 0 && bin->frac[0] % 2 == 0)) {
          conv_finished = (dig_pos >= min_digits);
          if (!bigint_is_zero(residual)) {
            bigint_from_bigint(temp0, residual);
            bigint_shift_left(temp0, 1);
            /* temp0 holds 2 * residual. */
            cmp = bigint_cmp(divisor, temp0);
            if (cmp < 0 || (cmp == 0 && digit % 2 != 0)) {
              dig_pos = do_round(dig_pos, dec->digits);
            }  /* if */
          }  /* if */
        } else {
          bigint_from_bigint(temp0, residual);
          bigint_add(temp0, delta);
          if (adjust_upper_delta) {
            bigint_add(temp0, delta);
          }  /* if */
          /* temp0 holds residual + upper delta. */
          cmp = bigint_cmp(divisor, temp0);
          if (cmp < 0 || (cmp == 0 && bin->frac[0] % 2 == 0)) {
            conv_finished = 1;
            if (!bigint_is_zero(residual)) {
              dig_pos = do_round(dig_pos, dec->digits);
            }  /* if */
          }  /* if */
        }  /* if */
      }  /* if */
    } else if (dig_pos == ndigits + 1) {
      /* Round to ndigits. */
      conv_finished = 1;
      --dig_pos;
      if (dec->digits[dig_pos] < '5') {
        /* Nothing to do. */
      } else if ('5' < dec->digits[dig_pos] ||
                 !bigint_is_zero(residual) ||
                 (dec->digits[dig_pos-1] - '0') % 2 != 0) {
        dig_pos = do_round(dig_pos, dec->digits);
      }  /* if */
    }  /* if */
  }  /* while */
  if (dig_pos == 0) {
    dig_pos = 1;
    ++result_scale;
  }  /* if */
  /* Suppress trailing zeros. */
  while (1 < dig_pos && dec->digits[dig_pos - 1] == '0') {
    --dig_pos;
  }  /* while */
  /* Fill in remaining values. */
  dec->is_negative = bin->is_negative;
  dec->exponent = (int)result_scale;
  dec->digits[dig_pos] = '\0';
  dec->ndigits = dig_pos;
  delete_bigint(temp1);
  delete_bigint(temp0);
  delete_bigint(delta);
  delete_bigint(divisor);
  delete_bigint(residual);
}  /* bin2dec */


static void adjust_down(an_fp_binary *bin,
                        a_bigint     *err,
                        a_bigint     *eps,
                        int          scale)
/*
Binary exceeds decimal by value in *err; reduce value of bin.  *eps holds
value of epsilon (i.e., 1/2 ULP) for value in *bin.  Subnormal values have
been multiplied by 2^scale; this factor affects rounding, and has to be
removed here.
*/
{
  int adjust = bigint_divmod(err, eps);
  int underflow = fp_frac_sub_int(bin->frac, bin->precision, adjust / 2);

  if (underflow != 0) {
    /* Subtraction underflowed.  Set correct fraction, and leave exponent
       adjustment for later.

       underflow holds the number of ULPs that aren't accounted for when
       we adjust the fraction down to zero.

       But when we subtract from the zero fraction, each ULP reduces
       the value by half as much as the ULP if we hadn't underflowed, so
       we need to adjust by twice the calculated number of ULPs. */
    adjust = 2 * underflow + adjust % 2;
    /* Also, epsilon for the new fraction is half of what it was for the
       old fraction, so we need to adjust epsilon and the remaining error. */
    bigint_shift_right(eps, 1);
    if (0 < bigint_cmp(err, eps)) {
      bigint_sub(err, eps);
      ++adjust;
    }  /* if */
    fp_frac_set_to_max(bin->frac, bin->precision);
    /* The new fraction has been reduced by one ULP,
       so we reduce by adjust-1. */
    (void)fp_frac_sub_int(bin->frac, bin->precision, adjust - 1);
    adjust = 0;
  }  /* if */
  if (scale != 0) {
    int tail_to_half = fp_frac_cmp_tail_to_half(bin->frac, bin->precision,
                                                scale);
    if (0 < tail_to_half ||
        (tail_to_half == 0 &&
         bigint_is_zero(err) &&
         adjust % 2 == 0 &&
         BIT_AT(bin->frac, bin->precision, scale) != 0)) {
      if (scale == bin->precision) {
        fp_frac_set_to_max(bin->frac, bin->precision);
        underflow = 1;
      } else if (fp_frac_add_one_at_pos(bin->frac, bin->precision, scale)) {
        fp_frac_shift_right_with_high_bit(bin->frac, bin->precision);
        --scale;
      }  /* if */
    }  /* if */
  } else if (fp_frac_eq_min_frac(bin->frac, bin->precision)) {
    int error_to_eps;
    bigint_shift_left(err, 1);
    error_to_eps = bigint_cmp(err, eps);
    if (adjust % 2 != 0 || 0 < error_to_eps) {
      fp_frac_set_to_max(bin->frac, bin->precision);
      --bin->exponent;
      if (adjust % 2 != 0 && 0 <= error_to_eps) {
        --bin->frac[0];  /* Round to even. */
      }  /* if */
    }  /* if */
  } else if (BIT_AT(bin->frac, bin->precision, 0) == 0) {
    if (adjust % 2 != 0 && !bigint_is_zero(err)) {
      if (fp_frac_sub_int(bin->frac, bin->precision, 1)) {
        fp_frac_set_to_max(bin->frac, bin->precision);
        underflow = 1;
      }  /* if */
    }  /* if */
  } else if (adjust % 2 != 0) {
    --bin->frac[0];
  }  /* if */
  if (underflow == 0) {
    /* Nothing to do. */
  } else if (scale != 0) {
    --scale;
  } else {
    --bin->exponent;
  }  /* if */
  bin->exponent -= scale;
}  /* adjust_down */


static void adjust_up(an_fp_binary *bin,
                      a_bigint     *err,
                      a_bigint     *eps,
                      int          scale)
/*
Decimal exceeds binary by value in *err; increase value of bin.  *eps holds
value of epsilon (i.e., 1/2 ULP) for value in *bin.  Subnormal values have
been multiplied by 2^scale; this factor affects rounding, and has to be
removed here.
*/
{
  int tail_to_half;
  int adjust = bigint_divmod(err, eps);
  int overflow = (int)fp_frac_add_int(bin->frac, bin->precision, adjust / 2);

  /* If overflow != 0, addition overflowed; bin->frac holds low bits.
     Handle later. */
  if (scale != 0) {
    if (overflow != 0 && bin->precision < scale) {
      tail_to_half = fp_frac_eq_zero(bin->frac, bin->precision) ? 0 : 1;
    } else {
      tail_to_half =
                    fp_frac_cmp_tail_to_half(bin->frac, bin->precision, scale);
    }  /* if */
    if (tail_to_half == 0 && adjust % 2 != 0) {
      tail_to_half = 1;
    }  /* if */
  } else if (overflow != 0) {
    tail_to_half =
                 fp_frac_cmp_tail_to_half(bin->frac, bin->precision, overflow);
    if (tail_to_half == 0 && adjust % 2 != 0) {
      tail_to_half = 1;
    }  /* if */
  } else if (adjust %2 == 0) {
    tail_to_half = -1;
  } else {
    tail_to_half = 0;
  }  /* if */
  if (0 < tail_to_half ||
      (tail_to_half == 0 &&
       (!bigint_is_zero(err) ||
        BIT_AT(bin->frac, bin->precision, scale + overflow) != 0))) {
    /* Inclusion of "overflow" in the BIT_AT calculation has the effect of
       rounding ties to the nearest even value; omitting "overflow" would
       result in the "ties away from zero" rounding mode. */
    if (bin->precision <= scale) {
      fp_frac_set_to_min(bin->frac, bin->precision);
      --scale;
    } else if (fp_frac_add_one_at_pos(bin->frac, bin->precision, scale)) {
      ++overflow;
    }  /* if */
  }  /* if */
  while (overflow-- != 0) {
    /* overflow will never be greater than 2 and only rarely equal to 2,
       so we can just shift once or twice as needed. */
    fp_frac_shift_right_with_high_bit(bin->frac, bin->precision);
    ++bin->exponent;
  }  /* while */
  bin->exponent -= scale;
}  /* adjust_up */


STATIC void dec2bin(an_fp_binary        *bin,
                    an_fp_decimal_input *dec,
                    int                 scale)
/*
Adjust floating-point value in bin to best approximation of decimal value
in dec.  Assumes that bin is close to the correct value (fast_dec2bin_double
supposedly produces value within 6.01 ULPs).  Subnormal binary values have
been multiplied by 2^scale; thus, the decimal value must also be multiplied
by 2^scale; the final adjustments in adjust_up and adjust_down take this
multiplier into account in rounding, and then remove it.
*/
{
  int      exp;
  int      decimal_to_binary;
  int      error_to_eps;
  int      b2 = 0, b5 = 0, d2 = scale, d5 = 0;
  a_bigint *tmp;
  a_bigint *decimal = new_bigint();
  a_bigint *binary = new_bigint();
  a_bigint *eps = new_bigint();
  a_bigint *err = new_bigint();

  /* Figure out scale factors. */
  exp = dec->exponent - dec->precision;
  if (exp < 0) {
    b2 = b5 = -exp;
  } else if (0 < exp) {
    d2 += exp;
    d5 += exp;
  }  /* if */
  exp = bin->exponent - bin->precision;
  if (exp < 0) {
    d2 += -exp;
  } else if (0 < exp) {
    b2 += exp;
    if (bin->precision == 11 && dec->exponent == dec->precision) {
      d2 += exp;
    }  /* if */
  }  /* if */
  /* Remove common factors of 2. */
  if (b2 <= d2) {
    d2 -= b2;
    b2 = 0;
  } else {
    b2 -= d2;
    d2 = 0;
  }  /* if */
  /* Multiply binary and decimal by 2 so that eps represents half an ULP. */
  ++b2;
  ++d2;
  /* Set large integer values. */
  bigint_from_uint(eps, (an_fp_uint)1);
  bigint_mult_pow5(eps, b5);
  bigint_shift_left(eps, b2 - 1); /* eps = 1 * 5^b5*2^(b2-1) */
  bigint_from_fp_int(binary, bin->frac, bin->precision);
  bigint_shift_left(binary, 1);
  tmp = bigint_mult(binary, eps); /* binary = bin->fraction * 5^b5*2^b2 */
  delete_bigint(binary);
  binary = tmp;
  tmp = 0;
  bigint_from_dec_mant(decimal, dec);
  bigint_mult_pow5(decimal, d5);
  bigint_shift_left(decimal, d2); /* decimal = dec->[fraction] * 5^d5*2^d2 */
  /* Compare abs(decimal - binary) to eps to decide what to do next. */
  decimal_to_binary = bigint_abs_diff(err, decimal, binary);
  error_to_eps = bigint_cmp(err, eps);
  if (error_to_eps < 0 && scale == 0) {
    /* Error is less than half an ULP.  Check for special
       case: fraction is a power of 2 and decimal < binary. */
    if (decimal_to_binary < 0 &&
        fp_frac_eq_min_frac(bin->frac, bin->precision) != 0) {
      /* At lower end of binary fraction range.  Need to check lower range,
         where eps is half of what it is here.  We can compensate by doubling
         the value of err and checking again. */
      bigint_shift_left(err, 1);
      if (0 < bigint_cmp(err, eps)) {
        fp_frac_set_to_max(bin->frac, bin->precision);
        --bin->exponent;
      }  /* if */
    }  /* if */
  } else {
    /* Error is greater than half an ULP.  floor(err/eps) gives us
       the number of eps's that we need to adjust by.  (An ULP is 2 eps's.)
       Note that this differs from Gay's implementation: he uses floating-point
       to compute an approximation to the adjustment, then goes back through
       the large integer stuff again. */
    if (decimal_to_binary < 0) {
      adjust_down(bin, err, eps, scale);
    } else if (0 < decimal_to_binary) {
      adjust_up(bin, err, eps, scale);
    } else {
      bin->exponent -= scale;
    }  /* if */
  }  /* if */
  /* Clean up. */
  delete_bigint(err);
  delete_bigint(eps);
  delete_bigint(binary);
  delete_bigint(decimal);
}  /* dec2bin */


static an_fp_return_type n_strcpy(char         *tgt,
                                  a_const_char *src,
                                  size_t       size)
/*
Copy no more than size characters from src to tgt.
Returns: fp_ret_valid if successful, fp_ret_too_small if size is too small.
*/
{
  char              *last = tgt + size;
  an_fp_return_type res;

  while (tgt < last && *src != '\0') {
    *tgt++ = *src++;
  }  /* while */
  if (tgt < last) {
    *tgt = '\0';
    res = fp_ret_valid;
  } else {
    res = fp_ret_too_small;
  }  /* if */
  return res;
}  /* n_strcpy */


static constexpr a_const_char
                zeros[] = "00000000";
                        /* Used for formatting below. */


STATIC an_fp_return_type format(char          *tgt,
                                size_t        size,
                                an_fp_decimal *dec)
/*
Format contents of dec (ignoring sign, which has already been done) into tgt,
which is an array of at least size chars.  Emulates Java's default
floating-point format, to make cross-checking easier.
Returns fp_ret_valid if successful, fp_ret_too_small if the output
buffer is too small.
*/
{
  an_fp_return_type res = fp_ret_valid;

  if (dec->exponent <= 0 && -3 < dec->exponent) {
    /* 0.xxx */
    a_number_buffer tmp_buff;

    tmp_buff.reset_to("0.", &zeros[8 + dec->exponent], dec->digits);
    if (tmp_buff.length() + 1 < size) {
      tmp_buff.write_to_buffer(tgt, size);
    } else {
      res = fp_ret_too_small;
    }  /* if */
  } else if (0 < dec->exponent && dec->exponent < 8) {
    /* xxxxxxx.xx */
    long nchars;
    if (dec->ndigits <= dec->exponent) {
      /* Need room for max(dec->ndigits, dec->exponent) characters
         plus decimal point plus 0 digit after decimal point
         plus terminating 0. */
      nchars = dec->exponent + 3;
    } else {
      /* Need room for max(dec->ndigits, dec->exponent) characters
         plus decimal point plus terminating 0. */
      nchars = dec->ndigits + 2;
    }  /* if */
    if (size < (size_t)nchars) {
      res = fp_ret_too_small;
    } else {
      int pos;
      for (pos = 0; pos < dec->exponent && pos < dec->ndigits; ++pos) {
        *tgt++ = dec->digits[pos];
      }  /* for */
      for ( ; pos < dec->exponent; ++pos) {
        *tgt++ = '0';
      }  /* for */
      *tgt++ = '.';
      if (dec->ndigits <= pos) {
        *tgt++ = '0';
        ++pos;
      }  /* if */
      for ( ; pos < dec->ndigits; ++pos) {
        *tgt++ = dec->digits[pos];
      }  /* for */
      *tgt = '\0';
    }  /* if */
  } else {
    /* x.xxxxEyy */
    a_number_buffer tmp_buff;

    if (dec->ndigits == 1) {
      tmp_buff.reset_to(a_string_view(dec->digits, 1), ".0E",
                        dec->exponent - 1);
    } else {
      tmp_buff.reset_to(a_string_view(dec->digits, 1),
                        ".", &dec->digits[1], "E",
                        dec->exponent - 1);
    }  /* if */
    if (tmp_buff.length() + 1 < size) {
      tmp_buff.write_to_buffer(tgt, size);
    } else {
      res = fp_ret_too_small;
    }  /* if */
  }  /* if */
  return res;
}  /* format */


static an_fp_return_type convert_and_format(char         *tgt,
                                            size_t       size,
                                            an_fp_binary *bin)
/*
Convert simple values represented in broken-down form by *bin into
decimal representation and formats the result in *tgt (whose size is
specified by "size").  Returns fp_ret_valid, fp_ret_nan, fp_ret_neg_infinity,
or fp_ret_pos_infinity if successful, fp_ret_too_small if size is too small,
fp_ret_invalid if the value is invalid, and fp_ret_not_formatted if the value
was too complicated to handle here.  The caller is responsible for emitting any
"-" sign if necessary.
*/
{
  an_fp_return_type res = fp_ret_valid;
  if (bin->type == fpt_invalid) {
    res = n_strcpy(tgt, "<invalid floating-point value>", size);
    if (res == fp_ret_valid) {
      res = fp_ret_invalid;
    }  /* if */
  } else if (bin->type == fpt_nan) {
    res = n_strcpy(tgt, "NaN", size);
    if (res == fp_ret_valid) res = fp_ret_nan;
  } else if (bin->type == fpt_infinity) {
    res = n_strcpy(tgt, "Infinity", size);
    if (res == fp_ret_valid) {
      if (bin->is_negative) {
        res = fp_ret_neg_infinity;
      } else {
        res = fp_ret_pos_infinity;
      }  /* if */
    }  /* if */
  } else if (bin->type == fpt_zero) {
    res = n_strcpy(tgt, "0.0", size);
  } else {
    res = fp_ret_not_formatted;
  }  /* if */
  return res;
}  /* convert_and_format */


/*
Floating-point routines that are type-specific are generated by including
float_type.h with one of FPT_FLOAT, FPT_DOUBLE, FPT_LONG_DOUBLE, etc., defined
to indicate the desired floating-point routines.
*/
#define FPT_FLOAT16
END_EDG_NAMESPACE  /* Conditionally close the "edg" namespace. */
#include "float_type.h"
BEGIN_EDG_NAMESPACE  /* Conditionally open the "edg" namespace. */
#undef FPT_FLOAT16

#define FPT_BFLOAT16
END_EDG_NAMESPACE  /* Conditionally close the "edg" namespace. */
#include "float_type.h"
BEGIN_EDG_NAMESPACE  /* Conditionally open the "edg" namespace. */
#undef FPT_BFLOAT16

#define FPT_FLOAT
END_EDG_NAMESPACE  /* Conditionally close the "edg" namespace. */
#include "float_type.h" /*lint !e451 included more than once. */
BEGIN_EDG_NAMESPACE  /* Conditionally open the "edg" namespace. */
#undef FPT_FLOAT

#define FPT_DOUBLE
END_EDG_NAMESPACE  /* Conditionally close the "edg" namespace. */
#include "float_type.h" /*lint !e451 included more than once. */
BEGIN_EDG_NAMESPACE  /* Conditionally open the "edg" namespace. */
#undef FPT_DOUBLE

#if FP_HAS_LONG_DOUBLE
#define FPT_LONG_DOUBLE
END_EDG_NAMESPACE  /* Conditionally close the "edg" namespace. */
#include "float_type.h" /*lint !e451 included more than once. */
BEGIN_EDG_NAMESPACE  /* Conditionally open the "edg" namespace. */
#undef FPT_LONG_DOUBLE
#endif /* FP_HAS_LONG_DOUBLE */

#if FLOAT80_ENABLING_POSSIBLE
#define FPT_FLOAT80
END_EDG_NAMESPACE  /* Conditionally close the "edg" namespace. */
#include "float_type.h" /*lint !e451 included more than once. */
BEGIN_EDG_NAMESPACE  /* Conditionally open the "edg" namespace. */
#undef FPT_FLOAT80
#endif /* FLOAT80_ENABLING_POSSIBLE */

#if FLOAT128_ENABLING_POSSIBLE
#define FPT_FLOAT128
END_EDG_NAMESPACE  /* Conditionally close the "edg" namespace. */
#include "float_type.h" /*lint !e451 included more than once. */
BEGIN_EDG_NAMESPACE  /* Conditionally open the "edg" namespace. */
#undef FPT_FLOAT128
#endif /* FLOAT128_ENABLING_POSSIBLE */

void floating_one_time_init(void)
/*
Do one-time initialization of variables related to floating-point.
*/
{
#if FP_UNIT_TESTING && DEBUG
  /* Direct output to stderr when performing stand-alone tests. */
  f_debug = stderr;
#endif /* FP_UNIT_TESTING && DEBUG */
  init_bigints();
  large_fives_inited = 0;
  initialize_tens_float16();
  initialize_tens_bfloat16();
  initialize_tens_float();
  initialize_tens_double();
#if FP_HAS_LONG_DOUBLE
  initialize_tens_long_double();
#endif /* FP_HAS_LONG_DOUBLE */
#if FLOAT80_ENABLING_POSSIBLE
  initialize_tens_float80();
#endif /* FLOAT80_ENABLING_POSSIBLE */
#if FLOAT128_ENABLING_POSSIBLE
  initialize_tens_float128();
#endif /* FLOAT128_ENABLING_POSSIBLE */
  floating_init_called = TRUE;
}  /* floating_one_time_init */

/* Conditionally close the "edg" namespace. */
END_EDG_NAMESPACE

#endif /* !USE_HOST_FP_CONVERSION_ROUTINES || (USE_FLOAT128...) */

