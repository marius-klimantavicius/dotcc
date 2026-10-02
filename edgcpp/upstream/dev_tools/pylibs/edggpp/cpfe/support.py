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

import random
import re
import string

import gdb

from gdb.types import make_enum_dict

def _get_true_type_code(value: gdb.Value):
  return value.type.strip_typedefs().code

def has_struct_type(value: gdb.Value):
  return _get_true_type_code(value) == gdb.TYPE_CODE_STRUCT

def has_indirect_type(value: gdb.Value):
  '''Return True if the type of the given value is a pointer or reference type.
  '''
  type_code = _get_true_type_code(value)
  if type_code == gdb.TYPE_CODE_PTR:
    return True
  if type_code == gdb.TYPE_CODE_REF:
    return True
  if type_code == gdb.TYPE_CODE_RVALUE_REF:
    return True
  return False

def has_field(value: gdb.Value, name):
  if not has_struct_type(value):
    return False

  for field in value.type.fields():
    if field.name == name:
      return True

  return False

def forward_exec_only(func):
  '''A decorator for recovering from reverse mode errors.

  This decorator exists so that "to_string" functions that depend on the
  execution of functions can recover more gracefully.
  '''
  def wrapper(*args, **kw_args):
    try:
      return func(*args, **kw_args)
    except gdb.error as e:
      if 'reverse mode' in str(e):
        return '(pretty print error -- reverse mode)'
  return wrapper

ADDRESS_HEX_REGEX = re.compile(r'(0x[0-9A-Za-z]+)')

def get_address(gdb_value):
  address = str(gdb_value.address)
  try:
    return ADDRESS_HEX_REGEX.search(address).group(1)
  except:
    raise Exception(f"Couldn't extract address from \"{address}\".")

GDB_CSTR_DECODE_REGEX = re.compile(r'0x[0-9A-Za-z]+ "(.*)"')

def read_cstr(gdb_value):
  raw_result = str(gdb_value)
  try:
    return GDB_CSTR_DECODE_REGEX.search(raw_result).group(1)
  except:
    raise Exception(f"Couldn't extract c-string text from \"{raw_result}\".")


def evaluate_name(scp_gdb_value, il_entry_kind_name):
  '''Function to compute a name via callout to db_name_str.'''
  # Form an expression which returns the type's name as a string
  expression_template = (
    "db_name_str(static_cast<a_source_correspondence*>({}), {})"
  )
  object_address = get_address(scp_gdb_value)
  expression = expression_template.format(object_address, il_entry_kind_name)

  # Execute the expression and return the result as a string
  return str(gdb.parse_and_eval(expression))

def evaluate_member_function(gdb_value, func_name, *args):
  '''Function to evaluate the given member function with the given arguments
  on the given object.'''
  object_address = get_address(gdb_value)
  object_type = str(gdb_value.type)

  func_args = ','.join([str(arg) for arg in args])
  expression = (
    f"static_cast<{object_type}*>({object_address})->{func_name}({func_args})"
  )
  return gdb.parse_and_eval(expression)

def convert_to_enum_name(enum_tag_name, value):
  '''Function to convert an enum numeric value to a human readable label, given
  the name of the corresponding enum tag type.
  '''
  enum_type = gdb.lookup_type(enum_tag_name)
  enum_dict = make_enum_dict(enum_type)

  for key, it_value in enum_dict.items():
    if value == it_value:
      return key

  return None

class DynamicVariableInst:
  def __init__(self, var_name, var_type, gdb_value):
    self._var_name = var_name
    self._var_type = var_type
    self._gdb_value = gdb_value

  def get_var_name(self):
    return self._var_name

  def get_var_type(self):
    return self._var_type

  def get_gdb_value(self):
    expression_template = '*static_cast<{}*>({})'

    expression = expression_template.format(
      self._var_type,
      self._gdb_value
    )
    return gdb.parse_and_eval(expression)

  def __str__(self):
    return self._var_name

# GDB does not have a way to release convenience variables. As we randomly grab
# convenience vars for various operations, pool them so as not to waste GDB
# resources.
_INACTIVE_VAR_POOL = []
_ALL_CONVENIENCE_VARS = set()

def _grab_gdb_var():
  '''Obtain a new GDB convenience variable name.'''
  # Reuse the existing name.
  if len(_INACTIVE_VAR_POOL) > 0:
    return _INACTIVE_VAR_POOL.pop()

  # Loop until a unique new name can be found.
  chars = string.ascii_lowercase
  while True:
    var_name = ''.join(random.choice(chars) for i in range(10))
    if var_name in _ALL_CONVENIENCE_VARS:
      continue

    _ALL_CONVENIENCE_VARS.add(var_name)
    return var_name

def _release_gdb_var(var_name):
  '''Release a GDB convenience variable name for reuse.'''
  _INACTIVE_VAR_POOL.append(var_name)

class DynamicVariable:
  def __init__(self, var_type):
    self._var_name = _grab_gdb_var()
    self._var_type = var_type

  def __enter__(self):
    gdb_value = gdb.parse_and_eval(
      f"({self._var_type}*)malloc(sizeof({self._var_type}))"
    )
    gdb.set_convenience_variable(self._var_name, gdb_value)
    return DynamicVariableInst(f"${self._var_name}", self._var_type, gdb_value)

  def __exit__(self, exc_type, exc_value, traceback):
    gdb.execute(f"call (void)free(${self._var_name})")
    _release_gdb_var(self._var_name)

class InspectionCacheManager:
  '''This handles pretty printing based off of memory address.

  It's registered once in the initialization of edggpp's cpfe code
  (edggpp/cpfe/__init__.py). This registers lookup_printer to resolve
  printers based on the address of the value.

  Values are registered for specialized printing via their address in
  register_object.
  '''

  _inst = None

  def inst():
    if not InspectionCacheManager._inst:
      InspectionCacheManager._inst = InspectionCacheManager()

    return InspectionCacheManager._inst

  def __init__(self):
    self.inspection_cache = {}

  def __contains__(self, value):
    lookup_address = get_address(value)
    return lookup_address in self.inspection_cache

  def get_extra_info(self, value):
    lookup_address = get_address(value)
    if lookup_address in self.inspection_cache:
      _, extra_info = self.inspection_cache[lookup_address]
      return extra_info

    return None

  def lookup_printer(self, value):
    '''Called by GDB to lookup the printer for the given value.'''

    # If there is no address, there's nothing to do here.
    if value.address is None:
      return None

    lookup_address = get_address(value)
    if lookup_address in self.inspection_cache:
      printer, extra_info = self.inspection_cache[lookup_address]
      return printer(value, **extra_info)

    return None

  def register_object(self, value, printer, **extra_info):
    '''Called to register the given value by its address for pretty printing
    with the given printer type, and any extra named arguments for the printer.

    The printer's constructor will be called as "printer(value, **extra_info)"
    when resolved via lookup_printer.
    '''
    lookup_address = get_address(value)
    self.inspection_cache[lookup_address] = (printer, extra_info)

  def unregister_object(self, value):
    '''Removes a given address from the cache. This is generally done via the
    TemporaryInspection class when there's a conflict with a previously
    registered value.
    '''
    lookup_address = get_address(value)
    del self.inspection_cache[lookup_address]

class TemporaryInspection:
  '''Used for creating a temporary inspection cache for printing children of
  the given value.

  This works to ensure that the value's first field is controlled by the
  printer printing the child, rather than the address resolution resulting in
  the printer attempting to print the first field with the class's printer.

  If this seems counter intuitive, this occurs as the address of the first
  field and the address of the class are the same. So we need to temporarily
  override the rules about the current address so that printing of that field
  doesn't return to the class.
  '''

  def __init__(self, target_value):
    self.registered_information = []
    self.cache_manager = InspectionCacheManager.inst()
    self.target_value = target_value

  def __enter__(self):
    # Swap the cache out for a temporary copy
    self.original_cache = self.cache_manager.inspection_cache
    self.cache_manager.inspection_cache = self.original_cache.copy()

    # Unregister the target value.
    #
    # Whatever created this TemporaryInspection should be driving control of
    # interpretation of the data at the target value's address -- not any
    # parents.
    if self.target_value in self.cache_manager:
      self.cache_manager.unregister_object(self.target_value)

  def __exit__(self, exc_type, exc_value, traceback):
    self.cache_manager.inspection_cache = self.original_cache

class DereferencePrinter:
  '''Used for printing a pointer or reference as an in place (dereferenced)
  object.
  '''

  def __init__(self, value):
    self.value = value.referenced_value()

  def children(self):
    with TemporaryInspection(self.value):
      for field in self.value.type.fields():
        f_name = field.name
        yield f_name, self.value[f_name]

class UnusedPrinter:
  '''Used for <unused> instead of a large nonsensical value (e.g. an unused
  variant).
  '''

  def __init__(self, value):
    self.value = value

  def to_string(self):
    return '<unused>'

class VariantContainerPrinter:
  '''Used to print the variant field's currently active variant.

  For instance, this is registered on the union type representing the variant,
  then prints the "child" that's the currently active field within said union.
  '''

  def __init__(self, value, *, variant_name, variant_printer,
               variant_printer_args):
    self.value = value
    self.variant_name = variant_name
    self.variant_printer = variant_printer
    self.variant_printer_args = variant_printer_args

  def children(self):
    with TemporaryInspection(self.value):
      variant_value = self.value[self.variant_name]

      cache_manager = InspectionCacheManager.inst()
      if self.variant_printer:
        cache_manager.register_object(
          variant_value,
          self.variant_printer,
          **self.variant_printer_args
        )

      # If we see an extra info field in a variant, just go ahead and expand it
      if has_field(variant_value, 'extra_info'):
        cache_manager.register_object(
          variant_value['extra_info'],
          DereferencePrinter
        )

      yield self.variant_name, variant_value

class VariantResolver:
  '''Used as a high level API to direct to register address resolutions to
  correctly handle a given value's variants and discriminating enum.
  '''

  def __init__(self, *,
               kind_field_path = 'kind',
               kind_field_enum = None,
               kind_field_enum_prefix_length = 3,
               variant_field_name = 'variant',
               variant_field_mapping = {},
               unused_variant_kinds = set()):
    '''Construct a variant resolver.

    Kind field path is a period separated path (though it's typically just
    a single element, the immediate field). This path sees through pointers
    so code that would normally be "a->b.c" in C++ is just "a.b.c".

    Kind field enum is dynamically resolved from the type of the object
    discovered via traversal of the kind field path. However, it can be
    manually overridden as well (this is particularly useful when dealing with
    kind fields represented as bitfields).

    Kind field enum prefix length is the number of characters that server as
    the prefix; for most kind enums in the front end, the default of 3 is
    correct (e.g., for the enumerator "sk_type", "sk_" is the prefix).

    Variant field name is the name of the variant field that we're modifying
    the printing of; for most types in the front end, the default of "variant"
    is correct.

    Variant field mapping is an optional dict of kind field enumerators to a
    tuple.  The first value in the tuple is the name of variable within the
    variant field to use. The second value is either None or a type to use a
    printer for the variable. A third value can be optionally provided and is a
    dict of keyword (named) arguments used to construct the printer.

    Unused variant kinds is a set of enumerators that don't correspond to a
    variant (if any).
    '''
    self.kind_field_path = kind_field_path
    self.kind_field_enum = kind_field_enum
    self.kind_field_enum_prefix_length = kind_field_enum_prefix_length
    self.variant_field_name = variant_field_name
    self.variant_field_mapping = variant_field_mapping
    self.unused_variant_kinds = unused_variant_kinds

  def _get_variant_info(self, kind):
    variant = None
    printer = None
    args = {}

    if kind in self.variant_field_mapping:
      entry = self.variant_field_mapping[kind]
      # Deconstruct based on argument count
      if len(entry) == 2:
        variant, printer = entry
      else:
        variant, printer, args = entry

    # If not present variant falls back to conversion from the kind by removing
    # the n character prefix.
    variant = variant or kind[self.kind_field_enum_prefix_length:]
    return variant, printer, args

  def _get_kind_field_value(self, object_value):
    for subobject_name in self.kind_field_path.split('.'):
      object_value = object_value[subobject_name]
      if has_indirect_type(object_value):
        object_value = object_value.referenced_value()
    return object_value

  def _get_kind_field_enum(self, object_value):
    # If we've been explicitly told what the kind field enum is,
    # use that, otherwise calculate it.
    if self.kind_field_enum:
      return self.kind_field_enum

    # Unwrap to get the field's type name.
    field_value_inst = self._get_kind_field_value(object_value)
    field_type = field_value_inst.type
    field_type_name = field_type.name
    return field_type_name

  def _get_kind_info(self, object_value):
    '''Given the object holding the kind field, return the value of the field
    and the type of the field.
    '''
    return (
      self._get_kind_field_enum(object_value),
      self._get_kind_field_value(object_value)
    )

  def _register_variant_printer(self, value, kind):
      # Resolve variant information
      variant_name, variant_printer, variant_printer_args = (
        self._get_variant_info(kind)
      )
      variant_container = value[self.variant_field_name]

      # Inform the cache manager about these object details
      cache_manager = InspectionCacheManager.inst()
      cache_manager.register_object(
        variant_container,
        VariantContainerPrinter,
        variant_name = variant_name,
        variant_printer = variant_printer,
        variant_printer_args = variant_printer_args
      )

  def _register_unused_variant_printer(self, value):
      # Resolve variant information
      variant_container = value[self.variant_field_name]

      # Inform the cache manager it shouldn't print anything for this variant
      # field.
      cache_manager = InspectionCacheManager.inst()
      cache_manager.register_object(variant_container, UnusedPrinter)

  def register_information(self, value: gdb.Value, *,
                           kind_field_delegate: gdb.Value = None):
    # Resolve the kind information
    kind_enum, kind_field_value = self._get_kind_info(
      kind_field_delegate or value
    )
    kind = convert_to_enum_name(kind_enum, kind_field_value)

    # Register a printer for the variant information if the variant is not in
    # the set of kinds where the variant is unused.
    if kind not in self.unused_variant_kinds:
      self._register_variant_printer(value, kind)
    else:
      self._register_unused_variant_printer(value)
