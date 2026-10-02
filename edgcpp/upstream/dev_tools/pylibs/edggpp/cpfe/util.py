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

from .support import evaluate_member_function, TemporaryInspection

from gdb.printing import RegexpCollectionPrettyPrinter

class AllocatedStringPrinter:
  def __init__(self, value):
    self.value = value

  def display_hint(self):
    return 'string'

  def to_string(self):
    str_value = self.value['backing_array']['elems']
    str_len = self.value['backing_array']['n_elems']

    return str_value.string('utf-8', length = str_len)

class DynArrayPrinter:
  def __init__(self, value):
    self.value = value

  def display_hint(self):
    return 'array'

  def num_children(self):
    return self.value['n_elems']

  def child(self, elem_idx):
    with TemporaryInspection(self.value):
      return self.value['elems'][elem_idx]

  def children(self):
    num_elements = self.value['n_elems']

    with TemporaryInspection(self.value):
      for elem_idx in range(num_elements):
        yield 'value', self.value['elems'][elem_idx]

  def to_string(self):
    used = self.value['n_elems']
    total = self.value['n_allocated']

    return f"{used}/{total} array elements"

class PtrMapPrinter:
  def __init__(self, value):
    self.value = value

  def display_hint(self):
    return 'map'

  def children(self):
    total_elements = self.value['hash_mask'] + 1

    with TemporaryInspection(self.value):
      for elem_idx in range(total_elements):
        contains_elem = evaluate_member_function(
          self.value,
          'has_value_at',
          elem_idx
        )
        if not contains_elem:
          continue

        yield 'key', self.value['table'][elem_idx]['stored_key']
        yield 'value', self.value['table'][elem_idx]['stored_value']

  def to_string(self):
    used = self.value['n_elements']
    total = self.value['hash_mask'] + 1

    return f"{used}/{total} map elements"

def register_util_types(ppr: RegexpCollectionPrettyPrinter):
  ppr.add_printer(
    'Allocated_string',
    '^(edg::|)Allocated_string<.*>$',
    AllocatedStringPrinter
  )
  ppr.add_printer(
    'Dyn_array',
    '^(edg::|)Dyn_array<.*>$',
    DynArrayPrinter
  )
  ppr.add_printer(
    'Ptr_map',
    '^(edg::|)Ptr_map<.*>$',
    PtrMapPrinter
  )
