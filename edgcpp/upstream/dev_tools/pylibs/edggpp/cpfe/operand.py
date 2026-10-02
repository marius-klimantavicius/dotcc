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

from .support import VariantResolver, TemporaryInspection

from gdb.printing import RegexpCollectionPrettyPrinter

class AnOperandNodePrinter:
  _unused_variant_oks = {
    'ok_error',
    'ok_indefinite_function',
    'ok_sym_for_member',
    'ok_undefined_symbol'
  }

  # Configure a variant resolver with a set of expr extra info kinds where the
  # variant is unused.
  _variant_resolver = VariantResolver(
    unused_variant_kinds = _unused_variant_oks
  )

  def __init__(self, value):
    self.value = value

  def children(self):
    with TemporaryInspection(self.value):
      variant_resolver = AnOperandNodePrinter._variant_resolver
      variant_resolver.register_information(self.value)

      for field in self.value.type.fields():
        f_name = field.name
        yield f_name, self.value[f_name]

def register_operands(ppr: RegexpCollectionPrettyPrinter):
  ppr.add_printer('an_operand', '^(edg::|)an_operand$', AnOperandNodePrinter)
