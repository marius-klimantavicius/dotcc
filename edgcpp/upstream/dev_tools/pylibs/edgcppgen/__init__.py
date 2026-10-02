# Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
# Exceptions.
# See https://edgcpp.org/LICENSE.txt for license information.
# SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception

import re

from typing import Any, List, Optional, TextIO, Tuple

TAB_SIZE = 8

def block_indent(num_indents: int) -> str:
  return num_indents * '  '

def tab_indent(num_tabs: int) -> str:
  return num_tabs * (' ' * TAB_SIZE)

def to_camel_case(name: str) -> str:
  '''Convert a snake_case name to a CamelCase name.'''
  parts = name.split('_')
  parts[:] = [f"{p[0].upper()}{p[1:]}" for p in parts]
  return ''.join(parts)

_UPPER_CASE_PREFIX = re.compile('([A-Z]+)([A-Z][a-z]+)')
_UPPER_CASE_WORD = re.compile('([A-Z]+)')
_USELESS_UNDER_SCORE = re.compile('(^|_)_')

def to_snake_case(name: str) -> str:
  '''Convert a CamelCase name to a snake_case name.'''
  # Handle cases like FOOBarBaz -> FOO_BarBaz.
  result = _UPPER_CASE_PREFIX.sub(r'\1_\2', name)
  # Handle FOO_BarBaz -> _FOO__Bar_Baz.
  result = _UPPER_CASE_WORD.sub(r'_\1', result)
  # Handle _FOO__Bar_Baz -> FOO_Bar_Baz.
  result = _USELESS_UNDER_SCORE.sub(r'\1', result)
  # Handle FOO_Bar_Baz -> foo_bar_baz.
  return result.lower()

def to_acronym(word: str) -> str:
  acronym = ''
  parts = word.split('_')
  for part in parts:
    acronym += part[0]
  return acronym

def pointer_to_reference(expr: str) -> str:
  '''Given a pointer typed expression, convert it to a reference.

  Expressions that don't start with the addressof operator ("&") are assumed to
  be pointers to the storage.

  Thus this function does the following transformations:
    foo    -> *(foo)
    &foo   -> foo
    &(foo) -> foo
  '''
  if not expr.startswith('&'):
    return f"*({expr})"

  if expr.startswith('&('):
    return expr[2:-1]
  return expr[1:]

def _fill_bits(num_bits: int) -> int:
  bitmask = 0
  while num_bits > 0:
    bitmask <<= 1
    bitmask |= 1
    num_bits -= 1
  return bitmask

_UINT_BYTE_INFO = {
  1: (_fill_bits(8), 'uint8_t'),
  2: (_fill_bits(16), 'uint16_t'),
  4: (_fill_bits(32), 'uint32_t'),
  8: (_fill_bits(64), 'uint64_t')
}

_UINT_BYTE_INFO_LIST = list(_UINT_BYTE_INFO.items())

def min_bytes_for_n(num_elements: int) -> int:
  for byte_count, (max_bit_value, _) in _UINT_BYTE_INFO_LIST:
    if num_elements <= max_bit_value:
      return byte_count

  assert False, 'The number of elements doesn\'t fit in any native type'

def size_to_uint_or_none(byte_size: int) -> Optional[str]:
  '''Convert a given byte size to a C++ appropriately sized unsigned integer.

  If no appropriately sized type exists, return None.
  '''
  if byte_size in _UINT_BYTE_INFO:
    return _UINT_BYTE_INFO[byte_size][1]
  return None

def size_to_uint(byte_size: int) -> str:
  '''Convert a given byte size to a C++ appropriately sized unsigned integer
  type.

  If no appropriately sized type exists, an AssertionError will be thrown.
  '''
  native_size_type = size_to_uint_or_none(byte_size)
  err_msg = f"{byte_size} does not correspond to a native type"
  assert native_size_type is not None, err_msg
  return native_size_type

def size_to_byte_arr(byte_size: int) -> str:
  '''Convert a given byte size to a C++ byte array type of the given size.'''
  return f"uint8_t[{byte_size}]"

def size_to_uint_or_byte_arr(byte_size: int) -> str:
  '''Convert a given byte size to a C++ appropriately sized unsigned integer
  type.

  If no appropriately sized type exists, return a byte array type of the given
  size.
  '''
  native_size_type = size_to_uint_or_none(byte_size)
  if native_size_type is not None:
    return native_size_type
  else:
    return size_to_byte_arr(byte_size)

def to_bool(value: bool) -> str:
  '''Convert a python boolean value to the equivalent EDG TRUE/FALSE macro.'''
  if value:
    return 'TRUE'
  else:
    return 'FALSE'

def _visible_len(text: str) -> int:
  '''Calculate the length of text up until a newline character.

  This function is named "visible_len" to capture this is about visible
  character lengths, even though the only character this is a problem for
  currently is the newline character.
  '''
  size = 0
  for char in text:
    if char == '\n':
      break
    size += 1
  return size

def write_comment(text: str, output_stream: TextIO, *,
                  large_block = False, indent = '') -> None:
  '''Write an automatically reflowed comment from the given text.

  /*
  If large_block is specified the comment will be written like this.
  */

  /* Otherwise it will be written like this. */

  Indent specifies the number of characters to indent the comment by.

  The text can contain some special "words" to help with formatting. Currently,
  supported options are:

    %BREAK% - Specifies that '\n\n' should be used as a whitespace between
              the surrounding words during formatting.
  '''
  content_indent = ''
  inline_terminator = False
  if large_block:
    output_stream.write(indent)
    output_stream.write('/*\n')
    output_stream.write(indent)
    content_indent = indent
  else:
    output_stream.write(indent)
    output_stream.write('/* ')
    content_indent = f"{indent}   "
    inline_terminator = True

  # Setup variables for indent tracking
  content_indent_len = len(content_indent)
  curr_indent = content_indent_len

  # Break the text into words for reflowing.
  words = text.split()
  last_idx = len(words) - 1

  next_needs_sentence_whitespace = False
  for idx, word in enumerate(words):
    if word == '%BREAK%':
      output_stream.write('\n\n')
      curr_indent = 0
      continue

    # Default the whitespace to a single space unless this is the first word in
    # which case, no whitespace should precede the word.
    prefix_whitespace = ' ' if idx != 0 else ''
    # Determine if the last word was the end of the sentence, and we
    # now need to consider using a double space.
    if next_needs_sentence_whitespace:
      prefix_whitespace = '  '
      next_needs_sentence_whitespace = False

    # Check to see if this word ends a sentence and the next word may
    # need extra padding if there's not a line wrap performed.
    if word[-1] == '.':
      next_needs_sentence_whitespace = True

    # If this is an inline terminated (not large block) comment, we
    # need to add the " */" terminator to the word for length consideration.
    formatted_word = word
    formatted_word_len = len(word)
    if inline_terminator and idx == last_idx:
      formatted_word = f"{formatted_word} */\n"
      # Optimization to minimize counting, instead of using a _visible_len call
      # below, which is more expensive.
      formatted_word_len += 3

    # Check to see if we need to start a new line, and do so if necessary.
    prefix_whitespace_len = len(prefix_whitespace)
    if curr_indent + prefix_whitespace_len + formatted_word_len > 79:
      output_stream.write('\n')
      curr_indent = 0

    # Check to see if this is the start of a new line, if it is, we don't
    # need to prefix the words.
    if curr_indent == 0:
      output_stream.write(content_indent)
      curr_indent += content_indent_len
    else:
      output_stream.write(prefix_whitespace)
      curr_indent += prefix_whitespace_len
    output_stream.write(formatted_word)
    curr_indent += formatted_word_len

  if large_block:
    output_stream.write(f"\n{indent}*/\n")

class CommentWriter:
  def __init__(self, text: str, **fw_kw_args):
    self._text = text
    self._fw_kw_args = fw_kw_args

  def requires_block_in_switch(self) -> bool:
    return False

  def write(self, output_stream: TextIO, *, indent = '',
            existing_indent = 0) -> None:
    write_comment(
      self._text,
      output_stream,
      indent = indent,
      **self._fw_kw_args
    )

def _transform_var_type_and_name(var_type: str,
                                 var_name: str) -> Tuple[str, str]:
  var_last_char = var_type[-1]
  if var_last_char == '*':
    var_type = var_type[:-1]
    var_name = f"*{var_name}"
  elif var_last_char == '&':
    var_type = var_type[:-1]
    var_name = f"&{var_name}"
  return (var_type, var_name)

def _write_function_decl(name: str, return_type: str,
                         parameter_tup_list: List[Tuple[str, str]],
                         output_stream: TextIO, *, specifiers: List[str],
                         indent: str, terminator: str) -> None:
  # Rebuild the var decl tuples with the '*' and '&' placed on the var name
  # instead of the type to match EDG style.
  rebuilt_parameter_tup_list = []
  for param_type, param_name in parameter_tup_list:
    transformed = _transform_var_type_and_name(param_type, param_name)
    rebuilt_parameter_tup_list.append(transformed)
  parameter_tup_list = rebuilt_parameter_tup_list

  # Form part_1 roughly "<specifiers> <return type> <name>("
  part_1_components = []
  specifiers_str = ' '.join(specifiers)
  if len(specifiers_str) > 0:
    part_1_components.append(specifiers_str)
  if len(return_type) > 0:
    part_1_components.append(return_type)
  part_1_components.append(f"{name}(")
  part_1 = ' '.join(part_1_components)
  part_1_len = len(part_1)
  num_params = len(parameter_tup_list)
  if (part_1_len > 79 or
      ((part_1_len + len(terminator)) > 79 and num_params == 0)):
    # For now assume that breaking out the specifiers and return type give back
    # enough space. (This is hopefully true in all cases we care about anyways)
    output_stream.write(indent)
    if len(specifiers_str) > 0:
      output_stream.write(specifiers_str)
    if len(return_type) > 0:
      # If we wrote specifiers add a space to separate the specifiers.
      if len(specifiers_str) > 0:
        output_stream.write(' ')
      output_stream.write(return_type)
    output_stream.write('\n')
    part_1 = f"{name}("
    part_1_len = len(part_1)

  # Try to be greedy and put everything on one line.
  # Count ', ' (2 chars) n - 1 times
  # Count ' '  (1 char)  n times
  inline_param_len = max((num_params * 3) - 2, 0)
  for param_type, param_name in parameter_tup_list:
    inline_param_len += len(param_type)
    inline_param_len += len(param_name)

  # This is checking that the entire line fits:
  #
  # ... function_name(param_1_ty param_1, param_2_ty param_2)
  # ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
  #
  # This is composed of the following:
  #
  # part_1
  #   ... function_name(param_1_ty param_1, param_2_ty param_2)
  #   ^^^^^^^^^^^^^^^^^^
  #
  # inline_param_len
  #   ... function_name(param_1_ty param_1, param_2_ty param_2)
  #                     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
  # terminator
  #   ... function_name(param_1_ty param_1, param_2_ty param_2)
  #                                                           ^
  if (len(indent) + part_1_len + inline_param_len + len(terminator)) <= 79:
    output_stream.write(indent)
    output_stream.write(part_1)
    # Write the parameters.
    first = True
    for param_type, param_name in parameter_tup_list:
      if not first:
        output_stream.write(', ')

      output_stream.write(param_type)
      output_stream.write(' ')
      output_stream.write(param_name)
      first = False
    # Parameters have been written, terminate.
    output_stream.write(terminator)
  else:
    # Putting things on one line failed, switch to a multi-line mode.
    output_stream.write(indent)
    output_stream.write(part_1)

    largest_param_type = 0
    largest_param_name = 0
    for param_type, param_name in parameter_tup_list:
      param_type_len = len(param_type)
      if param_type_len > largest_param_type:
        largest_param_type = param_type_len

      param_name_len = len(param_name)
      if param_name_len > largest_param_name:
        largest_param_name = param_name_len

    # Calculate the length of the largest possible  type and name
    # pair.
    #
    # ... function_name(big_parameter_type parameter_name,
    #                   ^^^^^^^^^^^^^^^^^^
    #                                     ^
    #                   parameter_type     big_parameter_name)
    #                                      ^^^^^^^^^^^^^^^^^^^
    #
    aligned_param_len = largest_param_type + 1 + largest_param_name

    # Calculate the length of the line if the first parameter line is inlined.
    #
    # ... function_name(parameter_type_1 parameter_name_1,
    # ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    #
    # This is composed of the following:
    #
    # part_1
    #   ... function_name(parameter_type_1 parameter_name_1,
    #   ^^^^^^^^^^^^^^^^^^
    #
    # aligned_param_len
    #   ... function_name(parameter_type_1 parameter_name_1,
    #                     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    #
    # terminator is additionally used in this calculation. This is typically
    # equivalent to counting the comma:
    #
    #   ... function_name(parameter_type_1 parameter_name_1,
    #                                                      ^
    # However, terminator is used instead of a fixed "+ 1" as the
    # terminator contributes to the "first line length" overflow (if this is
    # still confusing, consider the carets to be the "overflowing portion"):
    #
    #   ... function_name(parameter_type_1 parameter_name_1) = delete;
    #                                                      ^^^^^^^^^^^
    # This is still true on multi-line examples:
    #
    #   ... function_name(parameter_type_1 parameter_name_1,
    #                     parameter_type_2 parameter_name_2) = delete;
    #                                                      ^^^^^^^^^^^
    inline_first_parameter_length = (
      len(indent) + len(part_1) + aligned_param_len + len(terminator)
    )

    # Calculate the amount of whitespace all alignments are off from
    # a perfect wrap.
    #
    # ... function_name(parameter_type_1 parameter_name_1,
    #                   parameter_type_2 parameter_name_2)
    #                                                     ^^^^^^^^^^^^^^^^^^^^^
    perfect_wrap_miss = 79 - inline_first_parameter_length

    # If we missed by a negative amount, the initial line overflows, and
    # cannot be inlined. Immediately insert a newline.
    if perfect_wrap_miss < 0:
      output_stream.write('\n')

    first = True
    for param_type, param_name in parameter_tup_list:
      if not first:
        output_stream.write(',\n')
      next_part = f"{param_type:<{largest_param_type}} {param_name}"

      # Calculate the unnecessary whitespace needed for any larger
      # parameter_names but not this parameter.
      #
      # ... function_name(parameter_type_1 parameter_name,
      #                                                   ^^^^^^^
      #                   parameter_type_2 bigger_parameter_name)
      #
      param_extra_padding = (largest_param_name - (len(param_name) + 1))

      # Get the initial adjusted line length, subtracting the extra
      # whitespace we don't want to manually write out for this specific
      # line.
      padding = 79 - param_extra_padding - len(terminator)

      # Subtract out the extra whitespace we don't want to manually write
      # out on every line (i.e., the "perfect_wrap_miss").
      #
      # ... function_name(parameter_type_1 parameter_name_1,
      #                   parameter_type_2 parameter_name_2)
      #                                                     ^^^^^^^^^^^^^^^^^^^
      if perfect_wrap_miss > 0:
        padding -= perfect_wrap_miss
      # If this is the first line, and there was a perfect wrap miss, this
      # line will be inline, so we want to skip the part_1 length so the
      # parameter is written to the same line.
      if first and perfect_wrap_miss >= 0:
        padding -= len(part_1)
      # Subtract 1 for the missing , or ) that's added after each iteration.
      padding -= 1
      output_stream.write(f"{next_part:>{padding}}")
      first = False

    output_stream.write(terminator)

def write_function_decl(name: str, return_type: str,
                        parameter_tup_list: List[Tuple[str, str]],
                        output_stream: TextIO, *, specifiers = ['inline'],
                        indent = '', terminator = ')') -> None:
  _write_function_decl(
    name,
    return_type,
    parameter_tup_list,
    output_stream,
    specifiers = specifiers,
    indent = indent,
    terminator = terminator
  )
  output_stream.write('\n')

def write_constructor_decl(type_name: str,
                           parameter_tup_list: List[Tuple[str, str]],
                           output_stream: TextIO, *,
                           constructor_name = None,
                           specifiers = ['inline'],
                           indent = '', terminator = ')') -> None:
  constructor_name = (
    type_name if constructor_name is None else constructor_name
  )
  name = f"{type_name}::{constructor_name}"
  _write_function_decl(
    name,
    '',
    parameter_tup_list,
    output_stream,
    specifiers = specifiers,
    indent = indent,
    terminator = terminator
  )
  output_stream.write('\n')

def write_function_template_decl(
                                template_parameter_list: List[Tuple[str, str]],
                                name: str, return_type: str,
                                parameter_tup_list: List[Tuple[str, str]],
                                output_stream: TextIO, *,
                                specifiers = ['inline'], indent = '',
                                terminator = ')') -> None:
  output_stream.write('template<')
  first = True
  for arg_type, arg_name in template_parameter_list:
    if not first:
      output_stream.write(', ')
    first = False
    output_stream.write(f"{arg_type} {arg_name}")
  output_stream.write('>\n')
  _write_function_decl(
    name,
    return_type,
    parameter_tup_list,
    output_stream,
    specifiers = specifiers,
    indent = indent,
    terminator = terminator
  )
  output_stream.write('\n')


def write_function_template_inst(name: str, template_argument_list: List[str],
                                 return_type: str,
                                 parameter_tup_list: List[Tuple[str, str]],
                                 output_stream: TextIO, *,
                                 specifiers = ['inline'], indent = '',
                                 terminator = ')') -> None:
  output_stream.write('template\n')

  name += '<'
  first = True
  for arg_type in template_argument_list:
    if not first:
      name += ', '
    first = False
    name += arg_type
  name += '>'

  _write_function_decl(
    name,
    return_type,
    parameter_tup_list,
    output_stream,
    specifiers = specifiers,
    indent = indent,
    terminator = terminator
  )
  output_stream.write('\n')


def write_conversion_operator_decl(operator_name: str,
                                   parameter_tup_list: List[Tuple[str, str]],
                                   output_stream: TextIO, *,
                                   specifiers = ['inline'], indent = '  ',
                                   terminator = ');') -> None:
  _write_function_decl(
    operator_name,
    'operator',
    parameter_tup_list,
    output_stream,
    specifiers = specifiers,
    indent = indent,
    terminator = terminator
  )
  output_stream.write('\n')

def write_function_def_end(function_name: str, output_stream: TextIO):
  '''Write the terminating brace and comment of a function definition by
  with given function name.
  '''
  output_stream.write(f"}}  /* {function_name} */\n")

def write_operator_decl(operator_name: str, return_type: str,
                        parameter_tup_list: List[Tuple[str, str]],
                        output_stream: TextIO, *, specifiers = ['inline'],
                        indent = '', terminator = ')') -> None:
  _write_function_decl(
    f"operator{operator_name}",
    return_type,
    parameter_tup_list,
    output_stream,
    specifiers = specifiers,
    indent = indent,
    terminator = terminator
  )
  output_stream.write('\n')

def write_operator_def_end(operator_name: str, output_stream: TextIO) -> None:
  write_function_def_end(
    f"operator{operator_name}",
    output_stream
  )

def _form_line(parts: List[str], indent: str, existing_indent: int) -> str:
  existing_indent += len(indent) + len(parts[0])
  contents = [indent, parts.pop(0)]
  while len(parts) > 0:
    part_len = len(parts[0])
    if (existing_indent + part_len) > 79:
      break
    existing_indent += part_len
    contents.append(parts.pop(0))
  return ''.join(contents)

def write_splitable(parts: List[str], output_stream: TextIO, *,
                    indent: str = '', existing_indent: int = 0) -> None:
  first_line = True
  parts = parts.copy()
  while len(parts) > 0:
    line = _form_line(parts, indent, existing_indent)
    if first_line:
      output_stream.write(line)
      output_stream.write('\n')
      first_line = False
      existing_indent = 0
    else:
      output_stream.write(f"{line:>{79}}\n")

def write_right_split(left: str, right: str, output_stream: TextIO, *,
                      indent = '', inline_join = ' ',
                      existing_indent = 0) -> None:
  output_stream.write(indent)
  output_stream.write(left)
  total_indent_len = existing_indent + len(indent)
  if (total_indent_len + len(left) + len(inline_join) + len(right)) <= 79:
    output_stream.write(inline_join)
    output_stream.write(right)
  else:
    output_stream.write('\n')
    output_stream.write(f"{right:>{79}}")
  output_stream.write('\n')

class SplittableLine:
  def __init__(self, lhs: str, rhs: str, inline_join = ' '):
    self._lhs = lhs
    self._rhs = rhs
    self._inline_join = inline_join

  def requires_block_in_switch(self) -> bool:
    return False

  def write(self, output_stream: TextIO, *, indent = '',
            existing_indent = 0) -> None:
    write_right_split(
      self._lhs,
      self._rhs,
      output_stream,
      inline_join = self._inline_join,
      indent = indent,
      existing_indent = existing_indent
    )

def write_variable_block(var_decl_tuples: List[Tuple[str, str, str]],
                         output_stream: TextIO, *, indent = '  ',
                         existing_indent = 0,
                         include_trailing_line = True) -> None:
  # Rebuild the var decl tuples with the '*' and '&' placed on the var name
  # instead of the type to match EDG style.
  rebuilt_var_decl_tuples = []
  for var_type, var_name, var_init in var_decl_tuples:
    var_type, var_name = _transform_var_type_and_name(var_type, var_name)
    rebuilt_var_decl_tuples.append((var_type, var_name, var_init))
  var_decl_tuples = rebuilt_var_decl_tuples

  largest_type = 0

  for var_type, var_name, _ in var_decl_tuples:
    var_type_len = len(var_type)
    if var_type_len > largest_type:
      largest_type = var_type_len

  # If there's existing indent, the block is written using the existing
  # indent, and there is no local indent for the first line.
  is_inline = existing_indent > 0
  for var_type, var_name, var_init in var_decl_tuples:
    local_indent = '' if is_inline else indent
    var_decl = f"{var_type:<{largest_type}} {var_name}"
    if var_init is not None:
      write_right_split(
        f"{local_indent}{var_decl} =", f"{var_init};",
        output_stream,
        existing_indent = existing_indent
      )
    else:
      output_stream.write(local_indent)
      output_stream.write(var_decl)
      output_stream.write(';\n')
    is_inline = False

  if include_trailing_line:
    output_stream.write('\n')

class VariableBlockWriter:
  def __init__(self, var_decl_tuples: List[Tuple[str, str, str]], *,
               include_trailing_line = True, include_preceding_line = False):
    self._var_decl_tuples = var_decl_tuples
    self._include_trailing_line = include_trailing_line
    self._include_preceding_line = include_preceding_line

  def requires_block_in_switch(self) -> bool:
    return True

  def write(self, output_stream: TextIO, *, indent = '',
            existing_indent = 0) -> None:
    if self._include_preceding_line:
      assert existing_indent == 0
      output_stream.write('\n')

    write_variable_block(
      self._var_decl_tuples,
      output_stream,
      indent = indent,
      existing_indent = existing_indent,
      include_trailing_line = self._include_trailing_line
    )

def write_field(type_name: str, name: str, output_stream: TextIO,
                indent = block_indent(1),
                comment_writer = None) -> None:
  type_string = f"{indent}{type_name}"
  output_stream.write(type_string)

  # Compute the amount of space remaining before we should write the name.
  #
  # This allows us to detect the extra space here:
  #
  #   a_type
  #         ^^^^^^^^^^name;
  #
  # So it can then be "corrected" to:
  #
  #   a_type          name;
  #
  # Or conversely, if the type is really long, it can be split:
  #
  #   a_type_that_is_really_long;
  #                   name;
  #
  type_string_len = len(type_string)
  name_indent = TAB_SIZE * 2
  amount_til_name_indent = name_indent - type_string_len
  if amount_til_name_indent > 0:
    output_stream.write(' ' * amount_til_name_indent)
    output_stream.write(name)
  else:
    output_stream.write('\n')
    output_stream.write(tab_indent(2))
    output_stream.write(name)
  output_stream.write(';\n')
  if comment_writer is not None:
    comment_writer.write(output_stream, indent = tab_indent(3))

def write_variant_field(control_field: str,
                        subfield_tup_list: List[Tuple[
                          str, str, str, CommentWriter
                        ]],
                        output_stream: TextIO, *,
                        type_name: str = '') -> None:
  output_stream.write(f"{block_indent(1)}union ")
  if len(type_name) > 0:
    output_stream.write(f"{type_name} ")
  output_stream.write('{\n')
  for field_type_name, name, enumeration, comment_writer in subfield_tup_list:
    write_comment(
      f"When {control_field} == {enumeration}:",
      output_stream,
      indent = block_indent(2)
    )
    write_field(
      field_type_name,
      name,
      output_stream,
      indent = block_indent(2),
      comment_writer = comment_writer
    )
  if len(type_name) > 0:
    output_stream.write(f"{block_indent(2)}{type_name}() {{}}\n")
  output_stream.write(f"{block_indent(1)}}} variant;\n")

class FunctionCallExprWriter:
  def __init__(self, function_name: str, args: List[str], terminator = ');\n'):
    self._function_name = function_name
    self._args = args
    # Convert to a brace init syntax if the terminator is specified to be a
    # curly brace rather than a paren. This allows this writer to be used for
    # brace init constructors.
    self._is_brace_init = terminator.startswith('}')
    self._terminator = terminator

  def requires_block_in_switch(self):
    return False

  def _write_opening_token(self, output_stream):
    if self._is_brace_init:
      output_stream.write('{')
    else:
      output_stream.write('(')

  def write(self, output_stream: TextIO, *, indent = '',
            existing_indent = 0) -> None:
    joined_args = ", ".join(self._args)

    # This is checking that the entire line fits:
    #
    # ... foo(a, b, c)
    # ^^^^^^^^^^^^^^^^
    #
    # This is composed of the following:
    #
    # indent
    #   ... foo(a, b, c);
    #   ^^^^
    #
    # function_name
    #   ... foo(a, b, c);
    #       ^^^^
    #
    # " + 1"
    #   ... foo(a, b, c);
    #          ^
    #
    # joined_args
    #   ... foo(a, b, c);
    #           ^^^^^^^
    #
    # terminator
    #   ... foo(a, b, c);
    #                  ^^
    #
    function_call_intro_len = len(self._function_name) + 1
    passed_indent_len = len(indent)
    argument_start_indent_len = passed_indent_len + function_call_intro_len
    terminator_len = _visible_len(self._terminator)
    call_length = (
      argument_start_indent_len + len(joined_args) + terminator_len
    )
    # FIXME: The indent handling here is a hack, this should be revised to be
    # more proper/rethought.
    if call_length < 79:
      if existing_indent != passed_indent_len:
        output_stream.write(indent)
      output_stream.write(self._function_name)
      self._write_opening_token(output_stream)
      output_stream.write(joined_args)
      output_stream.write(self._terminator)
    else:
      # Putting things on one line failed, switch to a multi-line mode.
      if existing_indent != passed_indent_len:
        output_stream.write(indent)
      output_stream.write(self._function_name)
      self._write_opening_token(output_stream)

      curr_indent = argument_start_indent_len

      last_idx = len(self._args) - 1
      for idx, arg in enumerate(self._args):
        # This doesn't occur much in the EDG Python code, but we're relying on
        # the fact that local variables are scoped at the function level, this
        # allows these conditions to be somewhat simplified for performance
        # reasons.
        if idx == 0:
          prefix_whitespace = ''
          prefix_whitespace_len = 0
        else:
          prefix_whitespace = ' '
          prefix_whitespace_len = 1
        if idx != last_idx:
          formatted_arg = f"{arg},"
          formatted_arg_len = len(formatted_arg)
        else:
          formatted_arg = f"{arg}{self._terminator}"
          formatted_arg_len = len(arg) + terminator_len

        if (curr_indent + prefix_whitespace_len + formatted_arg_len) < 79:
          output_stream.write(prefix_whitespace)
          output_stream.write(formatted_arg)
          curr_indent += prefix_whitespace_len + formatted_arg_len
        else:
          output_stream.write('\n')
          total_line_length = argument_start_indent_len + formatted_arg_len
          # Calculate a relative indent for cases where the argument simple
          # can't fit on one line without backtracking the indent a bit.
          #
          #   ... foo(
          #        realllllyyyyyyyyyyyyy_longgggggggggg_arrggggguummmennttttt);
          #        ^^^
          relative_indent = function_call_intro_len
          if total_line_length > 79:
            relative_indent -= (total_line_length - 79)
          output_stream.write(indent)
          output_stream.write(' ' * relative_indent)
          output_stream.write(formatted_arg)
          curr_indent = passed_indent_len + relative_indent + formatted_arg_len

def _stmt_needs_blocking(stmt_seq: List[Any]) -> bool:
  """Given a statement sequence, return TRUE when the sequence should be
  enclosed in a block, FALSE otherwise.
  """

  stmt_count = len(stmt_seq)
  if stmt_count <= 1:
    return False

  inner_stmt = stmt_seq[0]
  if isinstance(inner_stmt, str):
    return False

  if isinstance(inner_stmt, list):
    return True

  return inner_stmt.requires_block_in_switch()

def _write_stmt_seq(stmt_seq: List[Any], output_stream: TextIO, *,
                    inline = False, indent = '', existing_indent = 0) -> None:
  for idx, line in enumerate(stmt_seq):
    indent_suppressed = idx == 0 and existing_indent > 0

    true_indent = indent
    forced_indent = f"{indent}  "
    if not inline:
      true_indent = forced_indent
    if isinstance(line, str):
      if not indent_suppressed and len(line) != 0:
        output_stream.write(true_indent)
      output_stream.write(line)
      output_stream.write('\n')
    elif isinstance(line, list):
      if not indent_suppressed:
        output_stream.write(true_indent)
      output_stream.write('{\n')
      _write_stmt_seq(line, output_stream, indent = forced_indent)
      output_stream.write(true_indent)
      output_stream.write('}\n')
    else:
      line.write(
        output_stream,
        indent = true_indent,
        existing_indent = existing_indent
      )

    existing_indent = 0

def write_stmt_seq(stmt_seq: List[Any], output_stream: TextIO) -> None:
  _write_stmt_seq(stmt_seq, output_stream)

class BranchWriter:
  def __init__(self, *, closing_comment = '  /* if */\n',
               always_a_block = False):
    self._conditional_branches = []
    self._fallback_branch = None
    self._closing_comment = closing_comment
    self._always_a_block = always_a_block

  def add_branch(self, condition: Any, branch: List[Any]) -> None:
    self._conditional_branches.append((condition, branch))

  def add_fallback(self, branch: List[Any]) -> None:
    assert self._fallback_branch is None
    self._fallback_branch = branch

  def make_final_case_else(self) -> None:
    self.add_fallback(self._conditional_branches.pop()[1])

  def requires_block_in_switch(self) -> bool:
    return False

  def write(self, output_stream: TextIO, *, indent = '  ',
            existing_indent = 0) -> None:
    first = True
    for condition, branch in self._conditional_branches:
      if_type = 'if' if first else '} else if'
      output_stream.write(indent)
      output_stream.write(if_type)
      output_stream.write(' (')
      if isinstance(condition, FunctionCallExprWriter):
        full_indent_len = len(indent) + len(if_type) + 2
        condition.write(
          output_stream,
          indent = (' ' * full_indent_len),
          existing_indent = full_indent_len
        )
      else:
        output_stream.write(condition)
        output_stream.write(') {')
      output_stream.write('\n')
      _write_stmt_seq(branch, output_stream, indent = indent)

      first = False

    if self._fallback_branch is not None:
      write_inline = True
      forced_block = False
      if not first:
        output_stream.write(indent)
        output_stream.write('} else {\n')
        write_inline = False
      elif self._always_a_block:
        write_inline = False
        forced_block = True
        output_stream.write(indent)
        output_stream.write('{\n')

      _write_stmt_seq(
        self._fallback_branch,
        output_stream,
        indent = indent,
        inline = write_inline
      )

      if forced_block:
        output_stream.write(indent)
        output_stream.write('}\n')

    if not first:
      output_stream.write(indent)
      output_stream.write('}')
      output_stream.write(self._closing_comment)

class PreProcessorBranchWriter:
  def __init__(self, condition: Any, true_branch: List[Any]):
    self._condition = condition
    self._true_branch = true_branch
    self._false_branch: Optional[List[Any]] = None

  def add_else(self, branch: List[Any]) -> None:
    assert self._false_branch is None
    self._false_branch = branch

  def requires_block_in_switch(self) -> bool:
    return False

  def write(self, output_stream: TextIO, *, indent = '  ',
            existing_indent = 0) -> None:
    assert existing_indent == 0

    output_stream.write(f"#if {self._condition}\n")
    _write_stmt_seq(
      self._true_branch,
      output_stream,
      indent = indent,
      inline = True
    )

    if self._false_branch is not None:
      output_stream.write(f"#else /* !{self._condition} */\n")
      _write_stmt_seq(
        self._false_branch,
        output_stream,
        indent = indent,
        inline = True
      )

    output_stream.write(f"#endif /* {self._condition} */\n")

class SwitchWriter:
  def __init__(self, value_ref: Any, *, closing_comment = '  /* switch */\n'):
    self._value_ref = value_ref
    self._cases: List[Tuple[List[str], List[Any]]] = []
    self._default_case: Optional[List[Any]] = None
    self._closing_comment = closing_comment

  def add_case(self, enumerator: str, stmt_seq: List[Any]) -> None:
    self._cases.append(([enumerator], stmt_seq))

  def add_cases(self, enumerators: List[str], stmt_seq: List[Any]) -> None:
    self._cases.append((enumerators, stmt_seq))

  def add_default_case(self, stmt_seq: List[Any]):
    assert self._default_case is None
    self._default_case = stmt_seq

  def requires_block_in_switch(self) -> bool:
    return False

  def _write_cases(self, labels: List[str], stmt_seq: List[Any],
                   output_stream: TextIO, *, indent: str,
                   template = '{}') -> None:
    for label in labels:
      output_stream.write(indent)
      output_stream.write('  ')
      output_stream.write(template.format(label))
      output_stream.write(':\n')
    needs_blocking = _stmt_needs_blocking(stmt_seq)
    if needs_blocking:
      output_stream.write(indent)
      intro_brace = '    { '
      output_stream.write(intro_brace)
      _write_stmt_seq(
        stmt_seq,
        output_stream,
        indent = f"{indent}    ",
        existing_indent = len(indent) + len(intro_brace)
      )
      output_stream.write(indent)
      output_stream.write('    }\n')
    else:
      _write_stmt_seq(
        stmt_seq,
        output_stream,
        indent = f"{indent}  "
      )
    output_stream.write(indent)
    output_stream.write('    break;\n')

  def write(self, output_stream: TextIO, *, indent = '  ',
            existing_indent = 0) -> None:
    assert existing_indent == 0
    output_stream.write(indent)
    output_stream.write('switch (')
    output_stream.write(self._value_ref)
    output_stream.write(') {\n')

    for enumerators, stmt_seq in self._cases:
      self._write_cases(
        enumerators,
        stmt_seq,
        output_stream,
        indent = indent,
        template = "case {}"
      )

    if self._default_case is not None:
      self._write_cases(
        ['default'],
        self._default_case,
        output_stream,
        indent = indent
      )
    else:
      output_stream.write(indent)
      output_stream.write('  default_is_unexpected();\n')

    output_stream.write(indent)
    output_stream.write('}')
    output_stream.write(self._closing_comment)

def _write_table_break(column_width_table: List[int], output_stream: TextIO, *,
                       column_sep = '-|-') -> None:
  output_stream.write('  |-')
  first = True
  for column_width in column_width_table:
    if not first:
      output_stream.write(column_sep)
    first = False
    output_stream.write('-' * column_width)
  output_stream.write('-|\n')

def _write_table_cells(row: List[str], column_width_table: List[int],
                       output_stream: TextIO) -> None:
  output_stream.write('  | ')
  first = True
  for idx, cell in enumerate(row):
    if not first:
      output_stream.write(' | ')
    first = False
    output_stream.write(f"{cell:<{column_width_table[idx]}}")
  output_stream.write(' |\n')

def write_table(header_row: List[str], content_rows: List[List[str]],
                output_stream: TextIO, *, title = '') -> None:
  column_width_table = []
  for header_cell in header_row:
    column_width_table.append(len(header_cell))

  for content_row in content_rows:
    if content_row is None:
      continue
    for idx, content_cell in enumerate(content_row):
      cell_width = len(content_cell)
      if cell_width > column_width_table[idx]:
        column_width_table[idx] = cell_width

  column_count = len(column_width_table)
  # Add the ' | ' spacing
  full_content_width = 3 * (column_count - 1)
  for column_width in column_width_table:
    # Add the element width
    full_content_width += column_width

  # Correct the table in an aesthetically pleasing way if the title is
  # over-sized.
  title_width = len(title)
  if title_width > full_content_width:
    space_needed = title_width - full_content_width
    even_space = space_needed // column_count
    extra_first_space = space_needed % column_count

    for idx in range(column_count):
      if idx == 0:
        column_width_table[idx] += extra_first_space
      column_width_table[idx] += even_space

    # Add the space needed/we just added back to the full content width.
    full_content_width += space_needed

  _write_table_break(column_width_table, output_stream, column_sep = '---')

  if len(title) > 0:
    output_stream.write(f"  | {title:^{full_content_width}} |\n")

    _write_table_break(column_width_table, output_stream)

  _write_table_cells(header_row, column_width_table, output_stream)

  _write_table_break(column_width_table, output_stream)

  for content_row in content_rows:
    # Create a table break when None is provided as a row.
    if content_row is None:
      _write_table_break(column_width_table, output_stream)
      continue

    _write_table_cells(content_row, column_width_table, output_stream)

  _write_table_break(column_width_table, output_stream)

def write_enumeration(name: str, enumerators: List[Tuple[str, str]],
                      output_stream: TextIO, *, scoped = False,
                      size_type = None) -> None:
  classifier = 'enum class' if scoped else 'enum'
  if size_type is None:
    size_type = size_to_uint(max(4, min_bytes_for_n(len(enumerators))))

  output_stream.write(f"{classifier} {name} : {size_type} {{\n")

  largest_name = 0
  for enumerator, _ in enumerators:
    len_enumerator = len(enumerator)
    if len_enumerator > largest_name:
      largest_name = len_enumerator

  first = True
  for enumerator, initializer in enumerators:
    if not first:
      output_stream.write(',\n')
    first = False

    if initializer is not None:
      output_stream.write(
        f"  {enumerator:<{largest_name}} = {initializer}"
      )
    else:
      output_stream.write(
        f"  {enumerator}"
      )
  output_stream.write(f"\n}};  /* {name} */\n")
