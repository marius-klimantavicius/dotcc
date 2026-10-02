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

from gdb.printing import RegexpCollectionPrettyPrinter

class AnIndexEntityPrinter:
  def __init__(self, value):
    self.value = value

  def children(self):
    yield 'file', self.value['file']
    yield 'sort', self.value['sort']
    yield 'value', self.value['value']

def register_ifc_entities(ppr: RegexpCollectionPrettyPrinter):
  ppr.add_printer(
    'Index_entity',
    '^(edg::|)an_ifc_[a-z]+_index$',
    AnIndexEntityPrinter
  )
