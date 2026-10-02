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
  VariantResolver, TemporaryInspection
)

from gdb.printing import RegexpCollectionPrettyPrinter

class ACachedTokenPrinter:
  _unused_variant_teiks = {'teik_none'}
  _teik_to_variant = {
    'teik_identifier': ('locator', None),
    'teik_pragma': ('pragmas', None),
    'teik_pp_token': ('pp_token_descr', None),
    'teik_extracted_body': ('extracted_template', None)
  }

  # Configure a variant resolver with a set of token extra info kinds where the
  # variant is unused as well as an explicitly provided variant field mapping
  # to handle some special cases.
  _variant_resolver = VariantResolver(
    kind_field_path = 'extra_info_kind',
    kind_field_enum_prefix_length = 5,
    unused_variant_kinds = _unused_variant_teiks,
    variant_field_mapping = _teik_to_variant
  )

  def __init__(self, value):
    self.value = value

  def children(self):
    with TemporaryInspection(self.value):
      variant_resolver = ACachedTokenPrinter._variant_resolver
      variant_resolver.register_information(self.value)

      for field in self.value.type.fields():
        f_name = field.name

        # This is better represented by our to_string method which prints the
        # token name as part of the inspection.
        if f_name == 'token':
          continue

        yield f_name, self.value[f_name]

  def to_string(self):
    return self.value['token']

def register_tokens(ppr: RegexpCollectionPrettyPrinter):
  ppr.add_printer(
    'a_cached_token',
    '^(edg::|)a_cached_token$',
    ACachedTokenPrinter
  )
