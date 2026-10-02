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

class AConstantTemplateParamPrinter:
  _tpck_to_variant = {
    'tpck_param': ('coordinates', None),
    'tpck_expression': ('expr', None)
  }

  # Configure a variant resolver with an explicitly provided variant field
  # mapping to handle some special cases, along with a prefix length
  # adjustment to properly compute name names.
  _variant_resolver = VariantResolver(
    kind_field_enum_prefix_length = 4,
    variant_field_mapping = _tpck_to_variant
  )

  def __init__(self, value):
    self.value = value

  def children(self):
    with TemporaryInspection(self.value):
      variant_resolver = AConstantTemplateParamPrinter._variant_resolver
      variant_resolver.register_information(self.value)

      for field in self.value.type.fields():
        f_name = field.name
        yield f_name, self.value[f_name]

class AConstantPrinter:
  # Use none to fall back to the default printer
  _ck_to_variant = {
    'ck_integer': ('integer_value', None),
    'ck_fixed_point': ('fixed_point_value', None),
    'ck_float': ('float_value', None),
    'ck_complex': ('complex_value', None),
    'ck_template_param': ('template_param', AConstantTemplateParamPrinter)
  }

  # Configure a variant resolver with an explicitly provided variant field
  # mapping to handle some special cases.
  _variant_resolver = VariantResolver(variant_field_mapping = _ck_to_variant)

  def __init__(self, value):
    self.value = value

  def children(self):
    with TemporaryInspection(self.value):
      variant_resolver = AConstantPrinter._variant_resolver
      variant_resolver.register_information(self.value)

      for field in self.value.type.fields():
        f_name = field.name
        yield f_name, self.value[f_name]

  # FIXME: The printing function seem to print directly, we need
  # something that gives us a cstring object
  #
  # def to_string(self):
  #   pass

def register_constants(ppr: RegexpCollectionPrettyPrinter):
  ppr.add_printer('a_constant', '^(edg::|)a_constant$', AConstantPrinter)
