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
  get_address, VariantResolver, InspectionCacheManager, TemporaryInspection
)

from gdb import parse_and_eval
from gdb.printing import RegexpCollectionPrettyPrinter

class ATemplatePrinter:
  # Configure a variant resolver with an explicitly provided variant field
  # mapping, prefix length adjustment, and set of ignored tempks to handle some
  # special cases.
  _tempk_to_variant = {
    'templk_function': ('routine', None),
    'templk_member_function': ('routine', None),
    'templk_class': ('type', None),
    'templk_member_class': ('type', None),
    'templk_member_enum': ('type', None),
    'templk_static_data_member': ('variable', None),
    'templk_variable': ('variable', None),
    'templk_concept': ('constraint', None)
  }
  _unused_variant_templks = {
    'templk_none',
    'templk_template_template_param'
  }
  _variant_resolver = VariantResolver(
    variant_field_name = 'prototype_instantiation',
    variant_field_mapping = _tempk_to_variant,
    kind_field_enum_prefix_length = 7,
    unused_variant_kinds = _unused_variant_templks
  )

  def __init__(self, value):
    self.value = value

  def children(self):
    with TemporaryInspection(self.value):
      variant_resolver = ATemplatePrinter._variant_resolver
      variant_resolver.register_information(self.value)

      for field in self.value.type.fields():
        f_name = field.name
        yield f_name, self.value[f_name]


class ATemplateArgPrinter:
  # Compute the value of is_array_bound_of_unknown_type
  def _is_array_bound_of_unknown_type(value):
    expression_template = (
      "static_cast<a_template_arg_ptr>({})->is_array_bound_of_unknown_type"
    )
    object_address = get_address(value)
    expression = expression_template.format(object_address)
    return int(parse_and_eval(expression))

  def _get_nontype_kind(value):
    if ATemplateArgPrinter._is_array_bound_of_unknown_type(value):
      return 'integer_value'
    else:
      return 'constant'

  def __init__(self, value):
    self.value = value

    # Configure the variant resolver, for this class we need to do this on
    # object init, rather than on class init, as the configuration depends on
    # the value we're working with.
    nontype_variant_field = ATemplateArgPrinter._get_nontype_kind(value)
    self.variant_resolver = VariantResolver(
      variant_field_mapping = {
        'tak_template': ('templ', None),
        'tak_nontype': (nontype_variant_field, None)
      },
      kind_field_enum_prefix_length = 4,
      unused_variant_kinds = {'tak_start_of_pack_expansion'}
    )

  def children(self):
    with TemporaryInspection(self.value):
      self.variant_resolver.register_information(self.value)

      for field in self.value.type.fields():
        f_name = field.name
        yield f_name, self.value[f_name]

class ATemplateParameterPrinter:
  # Configure a variant resolver with a prefix length adjustment to properly
  # compute names.
  _unused_variant_tpks = {'tpk_error'}
  _variant_resolver = VariantResolver(
    kind_field_enum_prefix_length = 4,
    unused_variant_kinds = _unused_variant_tpks
  )

  def __init__(self, value):
    self.value = value

  def children(self):
    with TemporaryInspection(self.value):
      variant_resolver = ATemplateParameterPrinter._variant_resolver
      variant_resolver.register_information(self.value)

      for field in self.value.type.fields():
        f_name = field.name
        yield f_name, self.value[f_name]

class ATemplateParamPrinter:
  # Configure a variant resolver with an indirect path and special case
  # mappings.
  _variant_resolver = VariantResolver(
    kind_field_path = 'param_symbol.kind',
    variant_field_mapping = {
      'sk_class_template': ('templ', None)
    }
  )
  # Configure a variant resolver with an indirect path and special case
  # mappings for the default argument variant.
  _default_arg_resolver = VariantResolver(
    kind_field_path = 'param_symbol.kind',
    variant_field_name = 'default_arg',
    variant_field_mapping = {
      'sk_class_template': ('templ', None)
    }
  )

  def __init__(self, value):
    self.value = value

  def children(self):
    with TemporaryInspection(self.value):
      variant_resolver = ATemplateParamPrinter._variant_resolver
      variant_resolver.register_information(self.value)

      default_arg_resolver = ATemplateParamPrinter._default_arg_resolver
      default_arg_resolver.register_information(self.value)

      for field in self.value.type.fields():
        f_name = field.name
        yield f_name, self.value[f_name]

def register_templates(ppr: RegexpCollectionPrettyPrinter):
  ppr.add_printer(
    'a_template',
    '^(edg::|)a_template$',
    ATemplatePrinter
  )
  ppr.add_printer(
    'a_template_arg',
    '^(edg::|)a_template_arg$',
    ATemplateArgPrinter
  )
  ppr.add_printer(
    'a_template_param',
    '^(edg::|)a_template_param$',
    ATemplateParamPrinter
  )
  ppr.add_printer(
    'a_template_parameter',
    '^(edg::|)a_template_parameter$',
    ATemplateParameterPrinter
  )
