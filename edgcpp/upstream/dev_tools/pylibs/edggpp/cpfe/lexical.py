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

import gdb

from .support import InspectionCacheManager, TemporaryInspection

from gdb.printing import RegexpCollectionPrettyPrinter

class StopTokenArrayPrinter:
  def __init__(self, value):
    self.value = value

  def display_hint(self):
    return 'array'

  def children(self):
    num_to_test = gdb.parse_and_eval('((unsigned)tok_last)')
    with TemporaryInspection(self.value):
      for idx in range(num_to_test):
        element = self.value[idx]
        if element != 0:
          yield 'value', gdb.parse_and_eval(f"(a_token_kind){idx}")

  def to_string(self):
    num_to_test = gdb.parse_and_eval('((unsigned)tok_last)')
    num_used = 0
    with TemporaryInspection(self.value):
      for idx in range(num_to_test):
        element = self.value[idx]
        if element != 0:
          num_used += 1

    return f"{num_used} stop tokens"

class StopTokenStackEntryPrinter:
  def __init__(self, value):
    self.value = value

  def children(self):
    with TemporaryInspection(self.value):
      stop_tokens = self.value['stop_tokens']
      cache_manager = InspectionCacheManager.inst()
      cache_manager.register_object(stop_tokens, StopTokenArrayPrinter)

      for field in self.value.type.fields():
        f_name = field.name
        yield f_name, self.value[f_name]

def register_lexical_types(ppr: RegexpCollectionPrettyPrinter):
  ppr.add_printer(
    'a_stop_token_stack_entry',
    '^(edg::|)a_stop_token_stack_entry$',
    StopTokenStackEntryPrinter
  )
