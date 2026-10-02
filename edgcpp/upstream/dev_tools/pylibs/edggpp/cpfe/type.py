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

from .support import evaluate_name, VariantResolver, TemporaryInspection

from gdb.printing import RegexpCollectionPrettyPrinter

class ATypePrinter:
  _unused_variant_tks = {'tk_nullptr', 'tk_error', 'tk_unknown', 'tk_void'}
  _tk_to_variant = {
    'tk_class': ('class_struct_union', None),
    'tk_struct': ('class_struct_union', None),
    'tk_union': ('class_struct_union', None)
  }

  # Configure a variant resolver with a set of type kinds where the variant is
  # unused as well as an explicitly provided variant field mapping to handle
  # some special cases.
  _variant_resolver = VariantResolver(
    unused_variant_kinds = _unused_variant_tks,
    variant_field_mapping = _tk_to_variant
  )

  def __init__(self, value):
    self.value = value

  def children(self):
    with TemporaryInspection(self.value):
      variant_resolver = ATypePrinter._variant_resolver
      variant_resolver.register_information(self.value)

      for field in self.value.type.fields():
        f_name = field.name

        # This is better represented by our to_string method which prints the
        # type name as part of the inspection.
        if f_name == 'source_corresp':
          continue

        yield f_name, self.value[f_name]

  def to_string(self):
    return evaluate_name(self.value['source_corresp'], 'iek_type')

def register_types(ppr: RegexpCollectionPrettyPrinter):
  ppr.add_printer('a_type', '^(edg::|)a_type$', ATypePrinter)
