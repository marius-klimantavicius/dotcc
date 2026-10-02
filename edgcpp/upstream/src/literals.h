/*
Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
Exceptions.
See https://edgcpp.org/LICENSE.txt for license information.
SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
*/

/*

literals.h -- Declarations relating to literals.c (having to do with 
              conversion of literal constants to and from internal form).

*/

/* Avoid including these declarations more than once. */
#ifndef LITERALS_H
#define LITERALS_H 1

#ifndef ERROR_H
#include "error.h"
#endif /* ifndef ERROR_H */
#ifndef IL_H
#include "il.h"
#endif /* ifndef IL_H */
#ifndef LEXICAL_H
#include "lexical.h"
#endif /* ifndef LEXICAL_H */

/* Conditionally open the "edg" namespace. */
BEGIN_EDG_NAMESPACE

extern void conv_integer_literal(int           radix,
                                 an_error_code *err_code,
                                 a_const_char  **err_pos,
                                 a_boolean     potential_ud_literal);
#if FIXED_POINT_ALLOWED
extern void conv_fixed_point_literal(a_boolean      is_hexadecimal,
                                     an_error_code  *err_code,
                                     a_const_char   **err_pos);
#endif /* FIXED_POINT_ALLOWED */
extern void conv_float_literal(a_boolean         is_hexadecimal,
                               an_error_code     *err_code,
                               a_const_char      **err_pos,
                               an_error_severity *severity);

/*
Structure to maintain the current state of the processing of
conv_single_char.
*/
typedef struct a_char_conversion_state *a_char_conversion_state_ptr;
typedef struct a_char_conversion_state {
  a_const_char	**next_token_char;
			/* Points to a pointer to the next character in the
			   token string to be processed.  This can point
			   within a multibyte character (both native and
			   UTF-8) unless we are translating from UTF-8 to
			   multibyte characters; in that case, this will
			   point to the next complete UTF-8 character in
			   the token string. */
  an_orig_line_modif_ptr
		next_orig_line_modif;
			/* When processing a raw string literal, points to
			   the next original line modification (trigraph or
			   line splice) that must be reversed in creating
			   the value of the string.  NULL for literals that
			   are not raw strings or if there is no such
			   modification. */
  int		remaining_char_count;
			/* Number of bytes left in the current multibyte
			   character.  conv_single_char is called multiple
			   times for a multibyte character, each call
			   returning one byte of the result.  Also used to
			   indicate that the second code unit of a
			   surrogate pair is pending and should be returned
			   by conv_single_char instead of reading a new
			   character from the token, in which case
			   next_mbc_char will be NULL. */
  unsigned char	*next_mbc_char;
			/* When translating from UTF-8 to multibyte
			   characters and for universal-character-names
			   (except when create_surrogate_pairs is TRUE), if
			   remaining_char_count is nonzero, points to the
			   next byte from translated_char to be returned.
			   NULL for normal multibyte character processing
			   (indicating multibyte characters will be fetched
			   directly from the token string) and when
			   pending_surrogate_pair contains the next code
			   unit.  Also used for the original form of
			   trigraphs and line splices when reversing
			   trigraph and line splice translation in raw
			   string literals. */
  unsigned long	pending_surrogate_pair;
			/* When create_surrogate_pairs is TRUE and a
			   character or universal-character-name is
			   encountered that requires a surrogate pair, the
			   second code unit of the pair is saved here and
			   remaining_char_count is set to 1 so that the
			   next call to conv_single_char will return it
			   instead of reading another character from the
			   token. */
  a_byte_boolean
		translate_utf8_to_mbc;
			/* If TRUE, Unicode characters (in Unicode-encoded
			   source files and, depending on the emulation
			   mode and other options, from
			   universal-character-names) appearing in the
			   token string will be replaced in the converted
			   output by their corresponding multibyte
			   character in the system default locale. */
  a_byte_boolean
		create_surrogate_pairs;
			/* If TRUE, the target data type is such that a
			   character designating a code point > 0xffff must
			   be represented as a surrogate pair.
			   pending_surrogate_pair and remaining_char_count
			   are used to enable conv_single_char to return
			   the second code unit of the pair in a subsequent
			   call. */
  a_byte_boolean
		warn_on_invalid_conversion;
			/* If TRUE, a warning will be issued for invalid
			   multibyte sequences; otherwise, invalid
			   sequences will be reported as discretionary
			   errors. */
  a_byte_boolean
		force_utf8;
			/* If TRUE, input is assumed to be encoded as
			   UTF-8, regardless of the setting of
			   curr_file_unicode_source_kind. */
  a_byte_boolean
		is_rescan;
			/* If TRUE, the source is a string literal constant
			   that is being rescanned, e.g., to change its
			   literal kind during string literal
			   concatenation. */
  unsigned char	translated_char[MAX_MULTIBYTE_CHAR_LENGTH];
			/* When translating from UTF-8 to multibyte
			   characters and for universal-character-names,
			   contains the translated version of the current
			   character.  Also used to reverse translated
			   trigraphs and line splices in raw string
			   literals. */
} a_char_conversion_state;

#define clear_char_conversion_state(state, ptr, translate_utf8) \
  { (state)->next_token_char = ptr;                             \
    (state)->next_orig_line_modif = NULL;                       \
    (state)->remaining_char_count = 0;                          \
    (state)->next_mbc_char = NULL;                              \
    (state)->translate_utf8_to_mbc = translate_utf8;            \
    (state)->create_surrogate_pairs = FALSE;                    \
    (state)->warn_on_invalid_conversion = FALSE;                \
    (state)->force_utf8 = FALSE;                                \
    (state)->is_rescan = FALSE;                                 \
  }  /* clear_char_conversion_state */

inline a_const_char *skip_embedded_null_escapes(a_const_char *loc,
                                                a_const_char *end_loc)
/*
MSVC ignores embedded null characters in character and string literals, not
including them in the value of the literal nor counting them in the length.
To emulate that behavior, advance loc over any LE_NULL lexical escapes that
occur preceding end_loc and return the adjusted value of loc.
*/
{
  while (loc <= end_loc - LE_ESCAPE_LEN && loc[0] == LE_ESCAPE &&
         loc[1] == LE_NULL) {
    loc += LE_ESCAPE_LEN;
  }  /* while */
  return loc;
}  /* skip_embedded_null_escapes */


extern void conv_single_char(a_char_conversion_state_ptr state,
                             a_boolean                   process_escapes,
                             unsigned long               *ch,
                             unsigned long               centity_mask,
                             a_boolean                   narrow_literal,
                             a_boolean                   utf8_literal);
extern void conv_char_literal(unsigned long num_chars,
                              an_error_code *err_code,
                              a_const_char  **err_pos);
extern void conv_string_literal(
                         a_const_char                  *start_of_string_value,
                         a_const_char                  *end_of_string_value,
                         a_string_or_char_literal_kind lit_kind,
                         unsigned long                 num_chars,
                         an_error_code                 *err_code,
                         a_const_char                  **err_pos,
                         a_boolean                     is_rescan = FALSE);

extern void concat_string_literals(a_token_cache_ptr      cache,
                                   a_character_kind       kind,
                                   a_token_cache_iterator *first_token = NULL);

extern void literals_one_time_init(void);

/* Conditionally close the "edg" namespace. */
END_EDG_NAMESPACE

#endif /* ifndef LITERALS_H */

