# This file is part of edggpp.
#
# edggpp is free software: you can redistribute it and/or modify it under the
# terms of the GNU General Public License as published by the Free Software
# Foundation, either version 3 of the License, or (at your option) any later
# version.
#
# edggpp is distributed in the hope that it will be useful, but WITHOUT ANY
# WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR
# A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
#
# You should have received a copy of the GNU General Public License along with
# edggpp.  If not, see <https://www.gnu.org/licenses/>.

from .support import (
  get_address,
  read_cstr,
  forward_exec_only,
  DynamicVariable,
  TemporaryInspection
)

import gdb

from contextlib import ExitStack
from gdb.printing import RegexpCollectionPrettyPrinter

class ASourcePositionPrinter:
  def __init__(self, value):
    self.value = value

  def children(self):
    with TemporaryInspection(self.value):
      for field in self.value.type.fields():
        f_name = field.name
        yield f_name, self.value[f_name]

  @forward_exec_only
  def to_string(self):
    # Allocate variables for the output parameters
    with ExitStack() as stack:
      file_name_var = stack.enter_context(DynamicVariable('a_const_char*'))
      full_name_var = stack.enter_context(DynamicVariable('a_const_char*'))
      line_num_var = stack.enter_context(DynamicVariable('a_line_number'))
      end_of_src_var = stack.enter_context(DynamicVariable('a_boolean'))
      use_lookup_table = stack.enter_context(DynamicVariable('a_boolean'))
      try:
        # Disable the sequence number lookup table, it causes problems.
        gdb.parse_and_eval(
          f"*({use_lookup_table}) = okay_to_use_seq_number_lookup_table"
        )
        gdb.parse_and_eval(
          f"okay_to_use_seq_number_lookup_table = 0"
        )

        # Evaluate conv_seq_to_file_and_line to extract the required values.
        seq_address = get_address(self.value['seq'])
        seq_num_ref = f"*static_cast<a_seq_number*>({seq_address})"

        conv_seq_to_file_expr = (
          'conv_seq_to_file_and_line('
          f"{seq_num_ref},"
          f"{file_name_var},"
          f"{full_name_var},"
          f"{line_num_var},"
          f"{end_of_src_var})"
        )
        gdb.parse_and_eval(conv_seq_to_file_expr)

        # Convert the extracted values into a nice string for output.
        is_file_needed_expr = f"seq_is_in_include_file({seq_num_ref})"

        result_str = ''
        if gdb.parse_and_eval(is_file_needed_expr):
          result_str += f"{read_cstr(file_name_var.get_gdb_value())} - "

        if end_of_src_var.get_gdb_value():
          result_str += 'end of source'
        else:
          line_num_val = line_num_var.get_gdb_value()
          result_str += f"line {line_num_val}, column {self.value['column']}"

        return result_str
      finally:
        # Restore the sequence number lookup table setting.
        gdb.parse_and_eval(
          f"okay_to_use_seq_number_lookup_table = *({use_lookup_table})"
        )

def register_srcpos(ppr: RegexpCollectionPrettyPrinter):
  ppr.add_printer(
    'a_source_position',
    '^(edg::|)a_source_position$',
    ASourcePositionPrinter
  )
