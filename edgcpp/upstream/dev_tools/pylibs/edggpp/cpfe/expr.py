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

class AnExprNodePrinter:
  _unused_variant_enks = {'enk_error', 'enk_address_of_ellipsis'}
  _enk_to_variant = {
    'enk_temp_init': ('init', None),
    'enk_lambda': ('init', None),
    'enk_gcnew': ('gcnew_info', None),
    'enk_throw': ('throw_info', None),
    'enk_typeid': ('typeid_info', None),
    'enk_sizeof': ('sizeof_info', None),
    'enk_alignof': ('sizeof_info', None),
    'enk_reuse_value': ('reuse_value_init', None),
    'enk_lowered_eh_construct': ('lowered_eh', None),
    'enk_vla_dealloc': ('vla_variable', None),
    'enk_await': ('await_info', None),
    'enk_yield': ('await_info', None),
    'enk_requires': ('requires_expr', None)
  }

  # Configure a variant resolver with a set of expr extra info kinds where the
  # variant is unused as well as an explicitly provided variant field mapping
  # to handle some special cases.
  _variant_resolver = VariantResolver(
    kind_field_enum = 'an_expr_node_kind',
    kind_field_enum_prefix_length = 4,
    unused_variant_kinds = _unused_variant_enks,
    variant_field_mapping = _enk_to_variant
  )

  def __init__(self, value):
    self.value = value

  def children(self):
    with TemporaryInspection(self.value):
      variant_resolver = AnExprNodePrinter._variant_resolver
      variant_resolver.register_information(self.value)

      for field in self.value.type.fields():
        f_name = field.name
        yield f_name, self.value[f_name]

class AnInitComponentPrinter:
  _ick_to_variant = {
    'ick_expression': ('expr', None),
    'ick_continued': ('continued', None)
  }

  # Configure a variant resolver with an explicitly provided variant field
  # mapping to handle some special cases.
  _variant_resolver = VariantResolver(
    kind_field_enum = 'an_init_component_kind',
    kind_field_enum_prefix_length = 4,
    variant_field_mapping = _ick_to_variant
  )

  def __init__(self, value):
    self.value = value

  def children(self):
    with TemporaryInspection(self.value):
      variant_resolver = AnInitComponentPrinter._variant_resolver
      variant_resolver.register_information(self.value)

      for field in self.value.type.fields():
        f_name = field.name
        yield f_name, self.value[f_name]

def register_exprs(ppr: RegexpCollectionPrettyPrinter):
  ppr.add_printer('an_expr_node', '^(edg::|)an_expr_node$', AnExprNodePrinter)
  ppr.add_printer(
    'an_init_component',
    '^(edg::|)an_init_component$',
    AnInitComponentPrinter
  )
