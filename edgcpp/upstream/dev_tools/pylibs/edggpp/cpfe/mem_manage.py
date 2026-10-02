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

class ATextBufferPrinter:
  def __init__(self, value):
    self.value = value

  def display_hint(self):
    return 'string'

  def to_string(self):
    str_value = self.value['buffer']
    str_len = self.value['size']

    return str_value.string('utf-8', length = str_len)

def register_mem_management(ppr: RegexpCollectionPrettyPrinter):
  ppr.add_printer(
    'a_text_buffer',
    '^(edg::|)a_text_buffer$',
    ATextBufferPrinter
  )
