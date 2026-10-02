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

from .support import TemporaryInspection, VariantResolver

from gdb.printing import RegexpCollectionPrettyPrinter

class ADynamicInitPrinter:
  _unused_variant_diks = {'dik_none', 'dik_zero'}
  _dik_to_variant = {
    'dik_nonconstant_aggregate': ('constant', None),
    'dik_lambda': ('constant', None),
    'dik_class_result_via_ctor': ('expression', None),
  }
  # Configure a variant resolver with a prefix length adjustment to properly
  # compute names and an explicitly provided variant field mapping to handle
  # some special cases.
  _variant_resolver = VariantResolver(
    kind_field_enum_prefix_length = 4,
    variant_field_mapping = _dik_to_variant,
    unused_variant_kinds = _unused_variant_diks
  )

  def __init__(self, value):
    self.value = value

  def children(self):
    with TemporaryInspection(self.value):
      variant_resolver = ADynamicInitPrinter._variant_resolver
      variant_resolver.register_information(self.value)

      for field in self.value.type.fields():
        f_name = field.name
        yield f_name, self.value[f_name]

def register_il_def(ppr: RegexpCollectionPrettyPrinter):
  ppr.add_printer(
    'a_dynamic_init',
    '^(edg::|)a_dynamic_init$',
    ADynamicInitPrinter
  )
