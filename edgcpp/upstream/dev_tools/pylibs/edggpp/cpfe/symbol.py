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

from .support import (
  InspectionCacheManager,
  TemporaryInspection,
  VariantResolver,

  get_address
)

from gdb.printing import RegexpCollectionPrettyPrinter

# FIXME: There might be a refactor with DereferencePrinter possible here
class ATemplateSymbolSupplementPrinter:
  _sk_to_variant = {
    'sk_function_template': ('function', None),
    'sk_member_function': ('function', None),
    'sk_variable_template': ('variable', None),
    'sk_static_data_member': ('variable', None)
  }

  # Configure a variant resolver with an explicitly provided variant field
  # mapping to handle some special cases.
  _variant_resolver = VariantResolver(variant_field_mapping = _sk_to_variant)

  def __init__(self, value, *, parent):
    self.value = value.referenced_value()
    self.parent = parent

  def children(self):
    with TemporaryInspection(self.value):
      variant_resolver = ATemplateSymbolSupplementPrinter._variant_resolver
      variant_resolver.register_information(
        self.value, kind_field_delegate = self.parent
      )

      for field in self.value.type.fields():
        f_name = field.name
        yield f_name, self.value[f_name]

class ASymbolPrinter:
  def __init__(self, value):
    self.value = value

    # Configure the variant resolver, for this class we need to this object
    # inint, rather than class init, as the configuration depends on the value
    # we're working with.
    #
    # Namely, we need to tell ATemplateSymbolSupplementPrinter about our
    # current value (its parent), so that it can determine how to decode the
    # variant.
    template_symbol_supplement = (
      'template_info',
      ATemplateSymbolSupplementPrinter,
      { 'parent': self.value }
    )
    self.variant_resolver = VariantResolver(
      variant_field_mapping = {
        'sk_class_or_struct_tag': ('class_struct_union', None),
        'sk_union_tag': ('class_struct_union', None),
        'sk_member_function': ('routine', None),
        'sk_extern_variable': ('extern_symbol_descr', None),
        'sk_extern_routine': ('extern_symbol_descr', None),
        'sk_parameter': ('param_id', None),
        'sk_class_template': template_symbol_supplement,
        'sk_function_template': template_symbol_supplement,
        'sk_variable_template': template_symbol_supplement,
        'sk_namespace': ('namespace_info', None),
        'sk_module': ('module_info', None),
        'sk_property_set': ('property_info', None)
      }
    )

  def children(self):
    with TemporaryInspection(self.value):
      self.variant_resolver.register_information(self.value)

      for field in self.value.type.fields():
        f_name = field.name
        yield f_name, self.value[f_name]

  def to_string(self):
    self_address = get_address(self.value)
    self_ref = f"*static_cast<a_symbol*>({self_address})"

    kind_name_val = gdb.parse_and_eval(f"symbol_kind_names[({self_ref}).kind]")
    kind = kind_name_val.string()
    name = gdb.parse_and_eval(f"({self_ref}).header.identifier").string()
    return f"<{kind}> \"{name}\""

class ASymbolLocatorPrinter:
  def __init__(self, value):
    self.value = value

  def children(self):
    for field in self.value.type.fields():
      f_name = field.name
      yield f_name, self.value[f_name]

  def to_string(self):
    self_address = get_address(self.value)
    self_ref = f"*static_cast<a_symbol_locator*>({self_address})"

    return gdb.parse_and_eval(
      f"({self_ref}).symbol_header.identifier"
    ).string()

def register_symbols(ppr: RegexpCollectionPrettyPrinter):
  ppr.add_printer('a_symbol', '^(edg::|)a_symbol$', ASymbolPrinter)
  ppr.add_printer(
    'a_symbol_locator',
    '^(edg::|)a_symbol_locator$',
    ASymbolLocatorPrinter
  )
