# Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
# Exceptions.
# See https://edgcpp.org/LICENSE.txt for license information.
# SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception

import hashlib
import io
import json
import pkgutil
import struct
import sys
import uuid

from enum import Enum
from typing import Any, Collection, Dict, List

# Fundamental Node Structure

class IFCField:
  def __init__(self, name, ifc_type):
    self._name = name
    self._ifc_type = ifc_type

  def name(self) -> str:
    '''Return the name of the field.'''
    return self._name

  def type_name(self) -> str:
    '''Return the name of the field's type.'''
    return self._ifc_type.name()

  def subfields(self) -> 'List[IFCField]':
    '''Return a list of the field's subfields.'''
    if hasattr(self._ifc_type, 'fields'):
      return self._ifc_type.fields()
    return []

  def parse(self, input_stream: io.BufferedIOBase) -> Any:
    '''Parse the field's value from the given input stream.'''
    return self._ifc_type.parse(input_stream)

  def size(self) -> int:
    '''Return the size of the field in terms of bytes.'''
    return self._ifc_type.size()

class IFCNodeTacitFieldProvider:
  def __init__(self):
    self._supported_fields = {}
    self._supported_fields_arr = []

  def add_support(self, tacit_field: 'IFCTacitField') -> None:
    '''Add support for the given tacit field to this tacit field provider.'''
    field_name = tacit_field.name()
    err_msg = f"double registration, {field_name} is already a tacit field"
    assert field_name not in self._supported_fields, err_msg

    self._supported_fields[tacit_field.name()] = tacit_field
    self._supported_fields_arr.append(tacit_field)

  def has_field_named(self, field_name: str) -> bool:
    '''Return True if this tacit field provider can provide a tacit field with
    the given name.'''
    return field_name in self._supported_fields

  def fields(self) -> 'List[IFCTacitField]':
    '''Return a list of tacit fields this provider provides.'''
    return self._supported_fields_arr

  def field_named(self, field_name: str) -> 'IFCTacitField':
    '''Return the tacit field this provider provides with the given name.'''
    return self._supported_fields[field_name]

class IFCNodeType:
  def __init__(self, name: str, fields: List[IFCField], padding: int,
               tacit_provider: IFCNodeTacitFieldProvider, inst_type: Any):
    '''Initialize a new IFC node type with the given name and fields list.
    Padding is the number of insignificant padding bytes following the node.
    The tacit provider is the tacit field provider associated with this node
    that can be used to retroactively add additional "tacit" information.  The
    inst type is the type that nodes of type will be created with when parsed.
    '''
    self._name = name
    self._fields = fields
    self._padding = padding
    self._tacit_provider = tacit_provider
    self._inst_type = inst_type

    self._name_mapped_fields = self._compute_name_mapped(fields)
    self._field_offsets = self._compute_offset_map(fields)

  def _compute_name_mapped(self,
                           fields: List[IFCField]) -> Dict[str, IFCField]:
    '''Given a list of fields return a dictionary mapping each field name to
    its field object.'''
    named_map = {}
    for field in fields:
      named_map[field.name()] = field
    return named_map

  def _compute_offset_map(self, fields: List[IFCField]) -> Dict[str, int]:
    '''Given a list of fields return a dictionary mapping each field name to
    its byte offset relative to the start of the node.
    '''
    offset_map = {}
    current_offset = 0
    for field in fields:
      offset_map[field.name()] = current_offset
      current_offset += field.size()
    return offset_map

  def name(self) -> str:
    '''Return the name of the node.'''
    return self._name

  def has_field_named(self, field_name: str) -> bool:
    '''Return True if this node has a field with the given name.  This does not
    include tacit fields.
    '''
    return field_name in self._name_mapped_fields

  def fields(self) -> List[IFCField]:
    '''Return a list of this node's fields. This does not include tacit fields.
    '''
    return self._fields

  def field_offset_of(self, field_name: str) -> int:
    '''Return the field offset for the field with the given name. This does not
    include tacit fields (they do not have real positions).
    '''
    return self._field_offsets[field_name]

  def field_named(self, field_name: str) -> IFCField:
    '''Return the field object for the given field name.'''
    return self._name_mapped_fields[field_name]

  def has_tacit_field_named(self, field_name: str) -> bool:
    '''Return True if this node has a tacit field with the given name.'''
    return self._tacit_provider.has_field_named(field_name)

  def tacit_fields(self) -> 'List[IFCTacitField]':
    '''Return a list of this node's fields.'''
    return self._tacit_provider.fields()

  def tacit_field_named(self, field_name: str) -> 'IFCTacitField':
    '''Return the tacit field object for the given field name.'''
    return self._tacit_provider.field_named(field_name)

  def padding(self) -> int:
    '''Return the node padding (i.e., the number of insignificant padding bytes
    following the node).
    '''
    return self._padding

  def parse(self, input_stream: io.BufferedIOBase) -> Any:
    '''Parse the node's value from the given input stream.'''
    result = {}
    for field in self._fields:
      result[field.name()] = field.parse(input_stream)

    if self._padding > 0:
      input_stream.read(self._padding)

    return self._inst_type(result)

  def size(self) -> int:
    '''Return the total size of this node including all fields and any
    padding.
    '''
    size = self._padding
    for field in self._fields:
      size += field.size()
    return size

class IFCIEEELEFloatType:
  def name(self):
    '''Return the name of this type.'''
    return 'IEEELEFloat'

  def parse(self, input_stream: io.BufferedIOBase) -> Any:
    '''Parse the IEEE float value from the input stream.'''
    # Decode using Python's struct lib to a "double".
    float_bytes = input_stream.read(self.size())
    return struct.unpack('d', float_bytes)[0]

  def size(self):
    '''Return the size of an IEEE float.'''
    return 8

# Fundamental Type Generators

def mk_fundamental_type(meta_name: str, meta_size: int,
                        meta_parse_fn: Any) -> Any:
  '''Create a new IFC fundamental type with the given name, size, and parse
  function.
  '''
  def name(self):
    return meta_name

  def size(self):
    return meta_size

  return type(
    f"IFC{meta_name}Type",
    (),
    {
      'name': name,
      'parse': meta_parse_fn,
      'size': size
    }
  )

def mk_type_raw_bytes(meta_name: str, meta_num_bytes: int) -> Any:
  '''Create a new IFC fundamental raw-bytes type with the given name and byte
  size.
  '''
  def parse(self, input_stream):
    return input_stream.read(meta_num_bytes)
  return mk_fundamental_type(meta_name, meta_num_bytes, parse)

def mk_type_uint(meta_name: str, meta_num_bytes: int) -> Any:
  '''Create a new IFC fundamental unsigned int type with the given name and
  byte size.
  '''
  def parse(self, input_stream):
    return int.from_bytes(input_stream.read(meta_num_bytes), 'little')
  return mk_fundamental_type(meta_name, meta_num_bytes, parse)

def _mk_value_getter(value_name: str) -> Any:
  '''Create a new value getter function that returns the value
  self._values[value_name] from its associated object.
  '''
  def get_value(self):
    return self._values[value_name]
  return get_value

class BadFieldError(Exception):
  def __init__(self, meta_name: str, field_name: str,
               known_fields: List[str]):
    '''Initialize a new bad field error for a node with the given name, the
    name of the field that was requested, and a list of known field names.
    '''
    known_fields_str = ', '.join(known_fields)
    super().__init__(
      f"Type \"{meta_name}\" has no field: {field_name}. "
      f"Known: {known_fields_str}"
    )

class BadTacitFieldError(Exception):
  def __init__(self, meta_name, field_name, known_fields):
    '''Initialize a new bad tacit field error for a node with the given name,
    the name of the tacit field that was requested, and a list of known tacit
    field names.
    '''
    known_fields_str = ', '.join(known_fields)
    super().__init__(
      f"Type \"{meta_name}\" has no tacit field: {field_name}. "
      f"Known: {known_fields_str}"
    )

def mk_node_inst(meta_name, tacit_field_provider, type_field_keys):
  def constructor(self, values):
    self._values = values

  def get_by_name(self, name):
    try:
      return self._values[name]
    except KeyError:
      raise BadFieldError(meta_name, name, type_field_keys) from None

  def get_tacit_by_name(self, retrieval_state, stream_reader, name):
    try:
      tacit_field = tacit_field_provider.field_named(name)
      return tacit_field.retrieve_value(retrieval_state, stream_reader, self)
    except KeyError:
      # Check to see if we tried to access a field we don't provide and give a
      # better exception, otherwise raise the original exception as something
      # else went wrong.
      if not tacit_field_provider.has_field_named(name):
        bad_field_error = BadTacitFieldError(
          meta_name,
          name,
          tacit_field_provider.fields()
        )
        raise bad_field_error from None
      else:
        raise

  def get_tacit_or_direct_by_name(self, retrieval_state, stream_reader, name):
    if tacit_field_provider.has_field_named(name):
      return self.get_tacit(retrieval_state, stream_reader, name)
    return self.get(name)

  # Setup the class structure.
  field_dict = {
    '__init__': constructor,
    'get': get_by_name,
    'get_tacit': get_tacit_by_name,
    'get_potentially_tacit': get_tacit_or_direct_by_name
  }
  for meta_key in type_field_keys:
    field_dict[meta_key] = _mk_value_getter(meta_key)

  # Construct a class for instances of this type.
  return type(
    f"IFC{meta_name}InstanceType",
    (),
    field_dict
  )

class IFCUnsupportedValue:
  def __init__(self, description):
    self.description = description

# IFC Tacit Field Support

class IFCTacitField:
  def __init__(self, name, ifc_type, proxy_impl):
    self._name = name
    self._ifc_type = ifc_type
    self._proxy_impl = proxy_impl

  def name(self):
    return self._name

  def type_name(self):
    return self._ifc_type.name()

  def subfields(self):
    if hasattr(self._ifc_type, 'fields'):
      return self._ifc_type.fields()
    return []

  def proxy(self):
    return self._proxy_impl

  def retrieve_value(self, retrieval_state, stream_reader, ifc_object):
    return self._proxy_impl.retrieve_value(
      retrieval_state,
      stream_reader,
      ifc_object
    )

class IFCReusableResult:
  def __init__(self, proxy):
    self._proxy = proxy
    self._key = uuid.uuid4()

  def proxy(self):
    return self._proxy

  def key(self):
    return self._key

  def uses_foreign_values(self):
    return self._proxy.uses_foreign_values()

  def retrieve_value(self, retrieval_state, stream_reader, ifc_object):
    # If this result has not yet been calculated, calculate it now, otherwise
    # use the previously computed result.
    if self._key not in retrieval_state:
      retrieval_state[self._key] = self._proxy.retrieve_value(
        retrieval_state,
        stream_reader,
        ifc_object
      )
    return retrieval_state[self._key]

class IFCConditionalDelegate:
  def __init__(self, condition_proxy, result_type_name, true_value_proxy,
               false_value_proxy):
    self._result_type_name = result_type_name
    self._condition_proxy = condition_proxy
    self._true_value_proxy = true_value_proxy
    self._false_value_proxy = false_value_proxy

  def result_type_name(self):
    return self._result_type_name

  def condition_proxy(self):
    return self._condition_proxy

  def true_value_proxy(self):
    return self._true_value_proxy

  def false_value_proxy(self):
    return self._false_value_proxy

  def uses_foreign_values(self):
    return (self._condition_proxy.uses_foreign_values() or
            self._true_value_proxy.uses_foreign_values() or
            self._false_value_proxy.uses_foreign_values())

  def retrieve_value(self, retrieval_state, stream_reader, ifc_object):
    result = self._condition_proxy.retrieve_value(
      retrieval_state,
      stream_reader,
      ifc_object
    )
    if result:
      return self._true_value_proxy.retrieve_value(
        retrieval_state,
        stream_reader,
        ifc_object
      )
    else:
      return self._false_value_proxy.retrieve_value(
        retrieval_state,
        stream_reader,
        ifc_object
      )

class IFCTestSortMatch:
  def __init__(self, registry, test_value_source, index_name, desired_matches):
    self._test_value_source = test_value_source
    self._index_name = index_name
    self._desired_matches = desired_matches
    self._is_sort_match = self._make_predicate(
      registry,
      index_name,
      desired_matches
    )

  def _find_encoded_sort_values(self, index_reg, desired_matches):
    encoded_values = set()
    for encoded_value, label in index_reg.values.items():
      if label in desired_matches:
        encoded_values.add(encoded_value)

    if len(encoded_values) != len(desired_matches):
      found_labels = set()
      for encoded_value, label in index_reg.values.items():
        if label in desired_matches:
          found_labels.add(label)

      missing_sorts = desired_matches - found_labels
      missing_sort_str = ', '.join(missing_sorts)
      assert False, f"Desired sorts were not found: {missing_sort_str}"
    return encoded_values

  def _make_predicate(self, registry, index_name, desired_matches):
    index_reg = registry.get_index_sort_registration(index_name)
    target_values = self._find_encoded_sort_values(index_reg, desired_matches)

    return lambda value: index_reg.get_tag_encoding(value) in target_values

  def test_value_source(self):
    return self._test_value_source

  def index_name(self):
    return self._index_name

  def desired_matches(self):
    return self._desired_matches

  def uses_foreign_values(self):
    return self._test_value_source.uses_foreign_values()

  def retrieve_value(self, retrieval_state, stream_reader, ifc_object):
    value = self._test_value_source.retrieve_value(
      retrieval_state,
      stream_reader,
      ifc_object
    )
    return self._is_sort_match(value)

class IFCFieldDelegate:
  def __init__(self, field_path):
    self._field_path = field_path.split('.')

  def field_path(self):
    return self._field_path

  def uses_foreign_values(self):
    return True

  def retrieve_value(self, retrieval_state, stream_reader, ifc_object):
    value = ifc_object
    for field in self._field_path:
      value = value.get_potentially_tacit(
        retrieval_state,
        stream_reader,
        field
      )
    return value

class IFCDirectDelegate:
  def __init__(self, field_path):
    self._field_path = field_path.split('.')

  def field_path(self):
    return self._field_path

  def uses_foreign_values(self):
    return False

  def retrieve_value(self, retrieval_state, stream_reader, ifc_object):
    value = ifc_object
    for field in self._field_path:
      value = value.get(field)
    return value

class IFCEncodedDelegate:
  def __init__(self, encoded_type_name, value_proxy):
    self._encoded_type_name = encoded_type_name
    self._value_proxy = value_proxy

  def encoded_type_name(self):
    return self._encoded_type_name

  def value_proxy(self):
    return self._value_proxy

  def uses_foreign_values(self):
    return self._value_proxy.uses_foreign_values()

  def retrieve_value(self, retrieval_state, stream_reader, ifc_object):
    # The python implementation does not do any kind of decoding, so
    # this is a noop.
    return self._value_proxy.retrieve_value(
      retrieval_state,
      stream_reader,
      ifc_object
    )

class IFCRemoteDelegate:
  '''Used when retrieving a value in a referenced node via an index based
  reference.  '''
  def __init__(self, registry, local_value_proxy, local_value_index_name,
               remote_type_name, remote_field_name):
    self._local_value_proxy = local_value_proxy
    self._local_value_index_name = local_value_index_name
    self._remote_type_name = remote_type_name
    self._remote_field_name = remote_field_name

    self._is_correct_sort = self._calc_sort_pred(
      registry,
      local_value_index_name,
      remote_type_name
    )

  def _find_encoded_value(self, index_reg, required_match):
    for encoded_value, label in index_reg.values.items():
      if label == required_match:
        return encoded_value
    assert False, f"{required_value} value not found"

  def _calc_sort_pred(self, registry, index_name, required_match):
    index_reg = registry.get_index_sort_registration(index_name)
    target_value = self._find_encoded_value(index_reg, required_match)

    return lambda value: index_reg.get_tag_encoding(value) == target_value

  def local_value_proxy(self):
    return self._local_value_proxy

  def local_value_index_name(self):
    return self._local_value_index_name

  def remote_type_name(self):
    return self._remote_type_name

  def remote_field_name(self):
    return self._remote_field_name

  def uses_foreign_values(self):
    return self._local_value_proxy.uses_foreign_values()

  def retrieve_value(self, retrieval_state, stream_reader, ifc_object):
    # Retrieve the initial starting "local" value.
    local_value = self._local_value_proxy.retrieve_value(
      retrieval_state,
      stream_reader,
      ifc_object
    )

    # Check the sort.
    if not self._is_correct_sort(local_value):
      return None

    # Retrieve the "remote" object value.
    index_name = self._local_value_index_name
    remote_value = stream_reader.read_by_index(index_name, local_value)

    # Retrieve the delegated value.
    return remote_value.get(self._remote_field_name)

class IFCOffsetDelegate:
  '''Used when retrieving a value in a referenced node via an offset based
  reference.
  '''
  def __init__(self, registry, local_value_proxy, remote_type_name,
               remote_field_name):
    self._local_value_proxy = local_value_proxy
    self._remote_type_name = remote_type_name
    self._remote_field_name = remote_field_name
    self._partition_kind = registry.get_partition_for_type_name(
      remote_type_name
    )

  def local_value_proxy(self):
    return self._local_value_proxy

  def remote_type_name(self):
    return self._remote_type_name

  def remote_field_name(self):
    return self._remote_field_name

  def offset_partition(self):
    return self._partition_kind

  def uses_foreign_values(self):
    return self._local_value_proxy.uses_foreign_values()

  def retrieve_value(self, retrieval_state, stream_reader, ifc_object):
    # Retrieve the initial starting "local" value.
    field_value = self._local_value_proxy.retrieve_value(
      retrieval_state,
      stream_reader,
      ifc_object
    )

    # Retrieve the "remote" object value.
    remote_value = stream_reader.read(self._partition_kind, field_value)

    # Retrieve the delegated value.
    return remote_value.get(self._remote_field_name)

class IFCForeignDelegate:
  """Represents a value that's from another module."""

  def __init__(self, registry, module_unit_value_source,
               module_local_value_source):
    self._registry = registry
    self._module_unit_value_source = module_unit_value_source
    self._module_local_value_source = module_local_value_source

  def module_unit_value_source(self):
    return self._module_unit_value_source

  def module_local_value_source(self):
    return self._module_local_value_source

  def uses_foreign_values(self):
    return True

  def retrieve_value(self, retrieval_state, stream_reader, ifc_object):
    # FIXME: Python _could_ implement this for ifc-qt but that's a stretch/long
    # term goal.
    return IFCUnsupportedValue('Foreign module value')

class IFCVisitorDelegate:
  def __init__(self, desired_field_name, desired_field_type_name, index_name,
               value_source):
    self._desired_field_name = desired_field_name
    self._desired_field_type_name = desired_field_type_name
    self._index_name = index_name
    self._value_source = value_source

  def desired_field_name(self):
    return self._desired_field_name

  def desired_field_type_name(self):
    return self._desired_field_type_name

  def index_name(self):
    return self._index_name

  def value_source(self):
    return self._value_source

  def uses_foreign_values(self):
    return True

  def retrieve_value(self, retrieval_state, stream_reader, ifc_object):
    visitor_idx = self._value_source.retrieve_value(
      retrieval_state,
      stream_reader,
      ifc_object
    )

    destination_value = stream_reader.read_by_index(
      self._index_name,
      visitor_idx
    )
    return destination_value.get_potentially_tacit(
      retrieval_state,
      stream_reader,
      self._desired_field_name
    )

class IFCComposedCategory:
  def __init__(self, sort_field, value_field_tups, desired_category):
    self._sort_field = sort_field
    self._value_field_tups = value_field_tups
    self._desired_category = desired_category

  def sort_field(self):
    return self._sort_field

  def value_fields(self):
    return list(map(lambda x: x[0], self._value_field_tups))

  def value_shifts(self):
    return list(map(lambda x: x[1], self._value_field_tups))

  def desired_category(self):
    return self._desired_category

  def uses_foreign_values(self):
    return False

  def retrieve_value(self, retrieval_state, stream_reader, ifc_object):
    result = 0

    for value_field, value_field_shift in reversed(self._value_field_tups):
      category_value = ifc_object.get(value_field)
      result |= category_value
      result <<= value_field_shift

    sort_value = ifc_object.get(self._sort_field)
    result |= sort_value

    return result

# IFC Registry Structures

def _make_bitmask(bitwidth):
  '''Create a bitmask of the specified bitwidth.'''
  bitmask = 0
  while bitwidth > 0:
    bitwidth -= 1
    # Increase the bitmask width by 1.
    bitmask <<= 1
    bitmask |= 1
  return bitmask

class IFCRegisteredIndexSort:
  def __init__(self, sort_type_name, bitwidth, values, *, non_null):
    self.sort_type_name = sort_type_name
    self.bitshift = bitwidth
    self.bitmask = _make_bitmask(bitwidth)
    self.values = values
    self.non_null = non_null

  def get_tag_encoding(self, value):
    return self.bitmask & value

  def get_tag(self, value):
    try:
      return self.values[self.get_tag_encoding(value)]
    except KeyError:
      decoded_value = self.get_tag_encoding(value)
      print(f"Unknown {hex(decoded_value)}, known values:")
      for value_key, value_label in self.values.items():
        print(f" - {hex(value_key)}: {value_label}")
      raise

  def get_index(self, value):
    return value >> self.bitshift

class IFCRegisteredOffset:
  def __init__(self, backing_type_name: str, *, non_null: bool) -> None:
    self.backing_type_name = backing_type_name
    self.non_null = non_null

class IFCRegisteredValueCategory:
  def __init__(self, bitwidth, discriminating_sort, sort_map,
               resolved_category_dict):
    self.bitshift = bitwidth
    self.bitmask = _make_bitmask(bitwidth)
    self.discriminating_sort = discriminating_sort
    self.sort_map = sort_map
    self.resolved_category_dict = resolved_category_dict

  def get_tag_encoding(self, value):
    return self.bitmask & value

  def get_sort(self, value):
    return self.sort_map[self.get_tag_encoding(value)]

  def get_item_type(self, value):
    return self.resolved_category_dict[self.get_sort(value)][1]

  def get_item_type_name(self, value):
    return self.resolved_category_dict[self.get_sort(value)][0]

  def get_item(self, value):
    category_value_type = self.get_item_type(value)

    # Form a byte stream to be parsed.
    interpreted_int_value = value >> self.bitshift
    interpreted_byte_value = interpreted_int_value.to_bytes(
      category_value_type.size(),
      byteorder='little'
    )
    byte_stream = io.BytesIO(interpreted_byte_value)

    # Parse the value.
    return category_value_type.parse(byte_stream)

class IFCRegistryTypeRole(Enum):
  # A node which has a corresponding partition.
  PARTITION_NODE      = 1
  # An interior node which has no corresponding partition.
  BASIC_NODE          = 2
  # A node which exists as part of the file header.
  HEADER_NODE         = 3
  # An enum like structure that maps a single value to a name.
  SIMPLE_SORT         = 4
  # A combination of a simple sort where the names indicate different
  # PARTITION_NODE types and an index into the associated node type's
  # partition.
  INDEX_SORT          = 5
  # A specialization of INDEX_SORT where the type corresponds to exactly one
  # PARTITION_NODE type.
  NODE_OFFSET         = 6
  # An offset into the string table.
  STRING_TABLE_OFFSET = 7
  # A bitset like structure that holds one or more bitflags.
  BITFIELD            = 8
  # A raw number with no special meaning.
  RAW_NUMERIC         = 9
  # A raw byte array with no special meaning.
  RAW_BYTES           = 10
  # A discriminated union like type.
  VALUE_CATEGORY      = 11
  # An index referencing a value in a different IFC file.
  FOREIGN_INDEX       = 12
  UNKNOWN             = 13

def is_node_role(role):
  return (role == IFCRegistryTypeRole.PARTITION_NODE or
          role == IFCRegistryTypeRole.BASIC_NODE or
          role == IFCRegistryTypeRole.HEADER_NODE)

def is_sort_role(role):
  return (role == IFCRegistryTypeRole.SIMPLE_SORT or
          role == IFCRegistryTypeRole.INDEX_SORT)

class IFCRegistry:
  '''A high level type abstracting the registration of IFC types for different
  IFC versions.
  '''

  def __init__(self):
    self._type_registry = {}
    self._node_registry = {}
    self._simple_sort_registry = {}
    self._index_sort_registry = {}
    self._foreign_index_registry = set()
    self._node_offset_registry = {}
    self._string_table_offset_registry = {}
    self._bitfield_mask_registry = {}
    self._value_category_registry = {}
    self._partition_to_type_name = {}
    self._partition_to_type = {}
    self._node_type_to_tacit_fields = {}
    self._type_to_partition = {}
    self._type_roles = {}
    self._type_name_to_tacit_provider = {}

  def _register(self, instance):
    type_name = instance.name()
    assert type_name not in self._type_registry, (
      f"{type_name} already registered"
    )
    self._type_registry[type_name] = instance
    return instance

  def register(self, instance, role = IFCRegistryTypeRole.UNKNOWN):
    self._register(instance)
    self._type_roles[instance.name()] = role
    return instance

  def register_uint(self, type_name, num_bytes):
    self._type_roles[type_name] = IFCRegistryTypeRole.RAW_NUMERIC
    return self._register(mk_type_uint(type_name, num_bytes)())

  def register_raw_bytes(self, type_name, num_bytes):
    self._type_roles[type_name] = IFCRegistryTypeRole.RAW_BYTES
    return self._register(mk_type_raw_bytes(type_name, num_bytes)())

  def register_node(self, type_name, field_dict, *, padding = 0):
    fields_arr = []
    for field_name, field_type in field_dict.items():
      fields_arr.append(IFCField(field_name, self._type_registry[field_type]))

    # Register the tacit field provider so tacit fields can be
    # registered to this type.
    tacit_field_provider = IFCNodeTacitFieldProvider()
    self._type_name_to_tacit_provider[type_name] = tacit_field_provider

    # Setup the node registration.
    node = self._register(IFCNodeType(
      type_name,
      fields_arr,
      padding,
      tacit_field_provider,
      mk_node_inst(type_name, tacit_field_provider, field_dict.keys())
    ))
    self._type_roles[type_name] = IFCRegistryTypeRole.BASIC_NODE
    self._node_registry[type_name] = node

    return node

  def register_tacit_field(self, type_name, field_name, field_type,
                           proxy_impl):
    assert type_name in self._type_name_to_tacit_provider

    tacit_field_provider = self._type_name_to_tacit_provider[type_name]
    tacit_field_provider.add_support(IFCTacitField(
      field_name,
      self._type_registry[field_type],
      proxy_impl
    ))

  def register_header_node(self, type_name, field_dict):
    self.register_node(type_name, field_dict)
    assert self._type_roles[type_name] == IFCRegistryTypeRole.BASIC_NODE
    self._type_roles[type_name] = IFCRegistryTypeRole.HEADER_NODE

  def register_simple_sort(self, sort_name, sort_dict):
    self._simple_sort_registry[sort_name] = sort_dict
    self._type_roles[f"{sort_name}Sort"] = IFCRegistryTypeRole.SIMPLE_SORT

    type_size = self._type_registry[f"{sort_name}Sort"].size()

    # Implicitly create an "Encoded<Sort>Sort" type.
    self.register_uint(f"Encoded{sort_name}Sort", type_size)
    self._type_roles[f"Encoded{sort_name}Sort"] = (
      IFCRegistryTypeRole.RAW_NUMERIC
    )

  def register_index_sort(self, sort_name, sort_bitwidth, sort_dict, *,
                          non_null):
    '''Register an index sort.

    sort_name should be the name of the sort without the 'Sort' postfix.
    psort_bitwidth should be the number of low order bits are reserved for the
    sort. sort_dict should be the key value mapping of numeric value to label.
    '''
    sort_reg = IFCRegisteredIndexSort(
      f"{sort_name}Sort",
      sort_bitwidth,
      sort_dict,
      non_null = non_null
    )
    self._index_sort_registry[sort_name] = sort_reg
    self._type_roles[f"{sort_name}Index"] = IFCRegistryTypeRole.INDEX_SORT

    type_size = self._type_registry[f"{sort_name}Index"].size()

    # Implicitly create an "Encoded<Sort>Index" type.
    self.register_uint(f"Encoded{sort_name}Index", type_size)
    self._type_roles[f"Encoded{sort_name}Index"] = (
      IFCRegistryTypeRole.RAW_NUMERIC
    )

    # Implicitly create an "<Sort>Sort" type.
    self.register_uint(f"{sort_name}Sort", type_size)
    self.register_simple_sort(sort_name, sort_dict)

  def register_node_offset(self, offset_name, backing_type_name, *, non_null):
    offset_reg = IFCRegisteredOffset(
      backing_type_name,
      non_null = non_null
    )
    self._node_offset_registry[offset_name] = offset_reg
    self._type_roles[f"{offset_name}Offset"] = IFCRegistryTypeRole.NODE_OFFSET

    type_size = self._type_registry[f"{offset_name}Offset"].size()

    # Implicitly create an "Encoded<Offset>Offset" type.
    self.register_uint(f"Encoded{offset_name}Offset", type_size)
    self._type_roles[f"Encoded{offset_name}Offset"] = (
      IFCRegistryTypeRole.RAW_NUMERIC
    )

  def register_string_table_offset(self, offset_name, backing_type_name):
    self._string_table_offset_registry[offset_name] = backing_type_name
    self._type_roles[f"{offset_name}Offset"] = (
      IFCRegistryTypeRole.STRING_TABLE_OFFSET
    )

  def register_foreign_index(self, sort_name):
    self._foreign_index_registry.add(sort_name)
    self._type_roles[f"{sort_name}ForeignIndex"] = (
      IFCRegistryTypeRole.FOREIGN_INDEX
    )

  def register_bitfield(self, bitfield_name, empty_name, index_dict,
                        *, extra = None):
    # Default values are initialized only once, as we modify the dict if
    # present, use None instead of {} as the default value, then adjust here.
    mask_dict = extra or {}
    mask_dict[0b0] = empty_name
    for index, value in index_dict.items():
      mask_dict[0b1 << index] = value
    self._bitfield_mask_registry[bitfield_name] = mask_dict
    self._type_roles[f"{bitfield_name}Bitfield"] = IFCRegistryTypeRole.BITFIELD

  def register_value_category(self, category_name, category_bitwidth,
                              discriminating_sort, category_dict):
    sort_map = self._simple_sort_registry[discriminating_sort]

    valid_keys = set(sort_map.values())
    resolved_category_dict = {}
    for category_key, category_type in category_dict.items():
      # Run a couple of sanity/setup checks.
      valid_key_err = (
        f"Unknown sort value {category_key} registering {category_name}."
      )
      assert category_key in valid_keys, valid_key_err
      double_reg_err = (
        f"Double registration of {category_key} registering {category_name}."
      )
      assert category_key not in resolved_category_dict, double_reg_err

      # Create the resolved mapping information.
      resolved_category_dict[category_key] = (
        category_type,
        self.get_type(category_type)
      )

    # Create a registry mapping between the category and all subsorts.
    self._value_category_registry[category_name] = IFCRegisteredValueCategory(
      category_bitwidth,
      discriminating_sort,
      sort_map,
      resolved_category_dict
    )
    self._type_roles[f"{category_name}Category"] = (
      IFCRegistryTypeRole.VALUE_CATEGORY
    )

  def register_partition(self, partition_name, type_name):
    # This relationship between type and partition should by one-to-one not
    # one-to-many or many-to-one (consider making types for each partition,
    # see ModuleReference for an example).
    assert partition_name not in self._partition_to_type_name
    assert type_name not in self._type_to_partition

    self._partition_to_type_name[partition_name] = type_name
    self._partition_to_type[partition_name] = self._type_registry[type_name]
    self._type_to_partition[type_name] = partition_name
    # The type should be registered to a basic node role first, then registered
    # to a partition after one is assigned.
    assert self._type_roles[type_name] == IFCRegistryTypeRole.BASIC_NODE
    self._type_roles[type_name] = IFCRegistryTypeRole.PARTITION_NODE

  def get_all_partitions(self):
    return self._partition_to_type_name.keys()

  def has_partition(self, partition_name):
    return partition_name in self._partition_to_type_name

  def get_all_type_names(self):
    return self._type_registry.keys()

  def has_type(self, type_name):
    return type_name in self._type_registry

  def get_type(self, type_name):
    return self._type_registry[type_name]

  def get_type_role(self, type_name):
    if type_name in self._type_roles:
      return self._type_roles[type_name]
    return IFCRegistryTypeRole.UNKNOWN

  def get_sort_tag(self, sort_name, sort_value):
    return self._simple_sort_registry[sort_name][sort_value]

  def get_simple_sort_registration(self, sort_name):
    return self._simple_sort_registry[sort_name]

  def get_simple_sort_registration_by_type(self, type_name):
    # Drop the Sort suffix for lookup
    sort_name = type_name[:-4]
    return self._simple_sort_registry[sort_name]

  def get_index_sort_registration(self, sort_name):
    return self._index_sort_registry[sort_name]

  def get_index_sort_registration_by_type(self, type_name):
    # Drop the Index suffix for lookup
    sort_name = type_name[:-5]
    return self.get_index_sort_registration(sort_name)

  def get_sort_dict_by_type(self, type_name):
    type_role = self.get_type_role(type_name)
    if type_role == IFCRegistryTypeRole.SIMPLE_SORT:
      return self.get_simple_sort_registration_by_type(type_name)
    if type_role == IFCRegistryTypeRole.INDEX_SORT:
      index_reg = self.get_index_sort_registration_by_type(type_name)
      return index_reg.values
    assert False, f"Bad type role: {type_role}"

  def get_node_offset_backing_type_name(self, offset_name: str) -> str:
    return self._node_offset_registry[offset_name].backing_type_name

  def get_node_offset_backing_type_name_by_type(self, type_name: str) -> str:
    offset_name = type_name[:-6]
    return self.get_node_offset_backing_type_name(offset_name)

  def get_node_offset_registration(self, offset_name: str
                                   ) -> IFCRegisteredOffset:
    return self._node_offset_registry[offset_name]

  def get_node_offset_registration_by_type(self, type_name: str
                                           ) -> IFCRegisteredOffset:
    offset_name = type_name[:-6]
    return self.get_node_offset_registration(offset_name)

  def get_tagged_bitshift_by_type(self, type_name):
    type_role = self.get_type_role(type_name)
    if type_role == IFCRegistryTypeRole.INDEX_SORT:
      index_reg = self.get_index_sort_registration_by_type(type_name)
      return index_reg.bitshift
    if type_role == IFCRegistryTypeRole.VALUE_CATEGORY:
      category_reg = self.get_value_category_registration_by_type(type_name)
      return category_reg.bitshift
    assert False, f"Bad type role: {type_role}"

  def get_tagged_bitmask_by_type(self, type_name):
    type_role = self.get_type_role(type_name)
    if type_role == IFCRegistryTypeRole.INDEX_SORT:
      index_reg = self.get_index_sort_registration_by_type(type_name)
      return index_reg.bitmask
    if type_role == IFCRegistryTypeRole.VALUE_CATEGORY:
      category_reg = self.get_value_category_registration_by_type(type_name)
      return category_reg.bitmask
    assert False, f"Bad type role: {type_role}"

  def get_bitfield_registration(self, bitfield_name):
    return self._bitfield_mask_registry[bitfield_name]

  def get_bitfield_registration_by_type(self, type_name):
    # Drop the Bitfield suffix for lookup
    bitfield_name = type_name[:-8]
    return self._bitfield_mask_registry[bitfield_name]

  def get_value_category_registration(self, category_name):
    return self._value_category_registry[category_name]

  def get_value_category_registration_by_type(self, type_name):
    # Drop the Category suffix for lookup
    category_name = type_name[:-8]
    return self._value_category_registry[category_name]

  def get_type_for_partition_name(self, partition_name):
    return self._partition_to_type[partition_name]

  def get_type_name_for_partition_name(self, partition_name):
    return self._partition_to_type_name[partition_name]

  def has_partition_for_type_name(self, type_name):
    return type_name in self._type_to_partition

  def get_partition_for_type_name(self, type_name):
    return self._type_to_partition[type_name]

  def get_all_partition_types(self):
    return self._type_to_partition.keys()

  def get_all_simple_sorts(self):
    return self._simple_sort_registry.keys()

  def get_all_index_sorts(self):
    return self._index_sort_registry.keys()

  def get_all_foreign_indexes(self):
    return self._foreign_index_registry

  def get_all_bitfields(self):
    return self._bitfield_mask_registry.keys()

  def get_all_value_catagories(self):
    return self._value_category_registry.keys()

  def get_all_nodes(self):
    return self._node_registry.keys()

class IFCVersionInfo:
  def __init__(self, major_version, minor_version):
    self.major = major_version
    self.minor = minor_version

  def is_at_least(self, major_version, minor_version):
    return self >= IFCVersionInfo(major_version, minor_version)

  def __eq__(self, other):
    if other is None:
      return False
    if self.major != other.major:
      return False
    if self.minor != other.minor:
      return False
    return True

  def __lt__(self, other):
    if self.major < other.major:
      return True
    if self.major == other.major and self.minor < other.minor:
      return True
    else:
      return False

  def __le__(self, other):
    return self < other or self == other

  def __gt__(self, other):
    return not (self < other) and self != other

  def __ge__(self, other):
    return self > other or self == other

  def __str__(self):
    return f"{self.major}.{self.minor}"

  def __hash__(self):
    return hash((self.major, self.minor))

def _has_home_scope_decl(type_info):
  if type_info.has_field_named('home_scope'):
    home_scope_field = type_info.field_named('home_scope')
    if home_scope_field.type_name() == 'DeclIndex':
      return True
  return False

def _is_scope_abstracted(registry, source_value):
  return IFCTestSortMatch(
    registry,
    source_value,
    'Decl',
    {'DeclSpecialization'}
  )

def _scope_abstraction_skipper(registry, source_value):
  # Cache the source value as its retrieval can be quite complicated.
  source_value = IFCReusableResult(source_value)

  # Setup a logic equivalent to (in pseudo code):
  #
  #  if (decl_sort(source_value) == DeclSpecializationSort) {
  #    DeclSpecialization spec = load(source_value);
  #    return spec.decl;
  #  } else {
  #    return source_value;
  #  }
  #
  return IFCConditionalDelegate(
    _is_scope_abstracted(registry, source_value),
    'DeclIndex',
    IFCRemoteDelegate(
      registry,
      source_value,
      'Decl',
      'DeclSpecialization',
      'decl'
    ),
    source_value
  )

def _named_by_enclosing_class(registry, version):
  if version.is_at_least(0, 41):
    return IFCConditionalDelegate(
      _is_scope_abstracted(registry, IFCDirectDelegate('home_scope')),
      'NameIndex',
      IFCVisitorDelegate(
        'name',
        'NameIndex',
        'Decl',
        IFCDirectDelegate('home_scope')
      ),
      IFCVisitorDelegate(
        'name',
        'NameIndex',
        'Decl',
        IFCFieldDelegate('home_scope')
      )
    )
  else:
    return IFCVisitorDelegate(
      'name',
      'NameIndex',
      'Decl',
      IFCFieldDelegate('home_scope')
    )

def _init_node_tacit_fields(registry, version, type_name):
  type_info = registry.get_type(type_name)
  if version.is_at_least(0, 41):
    # Check to see if the type has a direct home_scope field, and doesn't
    # already have a tacit field definition.
    if (_has_home_scope_decl(type_info) and
        not type_info.has_tacit_field_named('home_scope')):
      registry.register_tacit_field(
        type_name,
        'home_scope',
        'DeclIndex',
        _scope_abstraction_skipper(registry, IFCDirectDelegate('home_scope'))
      )

def _maybe_wrap_in_scope_abstraction_skipper(registry, version, proxy):
  if version.is_at_least(0, 41):
    return _scope_abstraction_skipper(registry, proxy)
  return proxy

def _init_tacit_fields(registry, version):
  registry.register_tacit_field(
    'DeclConstructor',
    'name',
    'NameIndex',
    _named_by_enclosing_class(registry, version)
  )
  registry.register_tacit_field(
    'DeclDestructor',
    'name',
    'NameIndex',
    _named_by_enclosing_class(registry, version)
  )
  registry.register_tacit_field(
    'DeclEnumerator',
    'home_scope',
    'DeclIndex',
    _maybe_wrap_in_scope_abstraction_skipper(
      registry,
      version,
      IFCRemoteDelegate(
        registry,
        IFCDirectDelegate('type'),
        'Type',
        'TypeDesignated',
        'decl'
      )
    )
  )
  registry.register_tacit_field(
    'DeclProperty',
    'home_scope',
    'DeclIndex',
    _maybe_wrap_in_scope_abstraction_skipper(
      registry,
      version,
      IFCRemoteDelegate(
        registry,
        IFCDirectDelegate('member'),
        'Decl',
        'DeclField',
        'home_scope'
      )
    )
  )
  registry.register_tacit_field(
    'DeclReference',
    'index',
    'DeclIndex',
    IFCForeignDelegate(
      registry,
      IFCDirectDelegate('unit'),
      IFCDirectDelegate('local_index')
    )
  )
  registry.register_tacit_field(
    'DeclPartialSpecialization',
    'home_scope',
    'DeclIndex',
    IFCVisitorDelegate(
      'home_scope',
      'DeclIndex',
      'Decl',
      IFCDirectDelegate(
        'entity.decl'
      )
    )
  )
  registry.register_tacit_field(
    'DeclPartialSpecialization',
    'primary_template',
    'DeclIndex',
    IFCOffsetDelegate(
      registry,
      IFCDirectDelegate('form'),
      'FormSpec',
      'primary_template'
    )
  )
  registry.register_tacit_field(
    'DeclPartialSpecialization',
    'name',
    'NameIndex',
    IFCVisitorDelegate(
      'name',
      'NameIndex',
      'Decl',
      IFCFieldDelegate('primary_template')
    )
  )
  registry.register_tacit_field(
    'DeclTemplate',
    'name',
    'NameIndex',
    IFCConditionalDelegate(
      IFCTestSortMatch(
        registry,
        IFCDirectDelegate('entity.decl'),
        'Decl',
        {'DeclConstructor', 'DeclDestructor'}
      ),
      'NameIndex',
      IFCVisitorDelegate(
        'name',
        'NameIndex',
        'Decl',
        IFCDirectDelegate(
          'entity.decl'
        )
      ),
      IFCDirectDelegate('name')
    )
  )

  registry.register_tacit_field(
    'SourceWord',
    'category',
    'WordCategory',
    IFCComposedCategory(
      'sort',
      [('value', 8), ('index', 16)],
      'Word'
    )
  )
  registry.register_tacit_field(
    'NestableWord',
    'category',
    'WordCategory',
    IFCComposedCategory(
      'sort',
      [('value', 8), ('index', 16)],
      'Word'
    )
  )

  trait_type_names = [
    # Generic
    'TraitAliasTemplate',
    'TraitAttribute',
    'TraitDeductionGuide',
    'TraitDeprecated',
    'TraitFunctionDefinition',
    'TraitFriend',
    'TraitRequires',
    'TraitSpecialization',
    # MSVC Specific
    'TraitMsvcDeclAttrs',
    'TraitMsvcFuncParams',
    'TraitMsvcUuid',
    'TraitMsvcVendorTrait',
    # EDG Specific
    'EdgTraitClassTemplateDefinition',
    'EdgTraitFunctionDefinition'
  ]
  for trait_type_name in trait_type_names:
    if not registry.has_type(trait_type_name):
      continue
    registry.register_tacit_field(
      trait_type_name,
      'encoded_decl',
      'EncodedDeclIndex',
      IFCEncodedDelegate(
        'EncodedDeclIndex',
        IFCDirectDelegate('decl')
      )
    )

  if version.is_at_least(0, 41):
    registry.register_tacit_field(
      'DeclSpecialization',
      'home_scope',
      'DeclIndex',
      IFCVisitorDelegate(
        'home_scope',
        'DeclIndex',
        'Decl',
        IFCDirectDelegate(
          'decl'
        )
      )
    )
    registry.register_tacit_field(
      'DeclSpecialization',
      'locus',
      'SourceLocation',
      IFCVisitorDelegate(
        'locus',
        'SourceLocation',
        'Decl',
        IFCDirectDelegate(
          'decl'
        )
      )
    )
    registry.register_tacit_field(
      'DeclSpecialization',
      'primary_template',
      'DeclIndex',
      IFCOffsetDelegate(
        registry,
        IFCDirectDelegate('form'),
        'FormSpec',
        'primary_template'
      )
    )
    registry.register_tacit_field(
      'DeclSpecialization',
      'name',
      'NameIndex',
      IFCVisitorDelegate(
        'name',
        'NameIndex',
        'Decl',
        IFCFieldDelegate('primary_template')
      )
    )

  # Register a convenience accessor to provide an interface more in line with
  # the IFC specification. Shadow the wrapped value to prevent API dependence
  # on it.
  registry.register_tacit_field(
    'TypePlaceholder',
    'basis',
    'TypeBasisSort',
    IFCDirectDelegate('basis.value')
  )

  for type_name in registry.get_all_type_names():
    role = registry.get_type_role(type_name)
    if is_node_role(role):
      _init_node_tacit_fields(registry, version, type_name)

def _register_special_case_primitives(registry: IFCRegistry) -> None:
  registry.register(IFCIEEELEFloatType(), IFCRegistryTypeRole.RAW_BYTES)

def _register_integer_primitives(registry: IFCRegistry,
                                 json_content: Any) -> None:
  for name, byte_size in json_content['primitives']['integer'].items():
    registry.register_uint(name, byte_size)

def _register_byte_primitives(registry: IFCRegistry,
                              json_content: Any) -> None:
  for name, byte_size in json_content['primitives']['byte'].items():
    registry.register_raw_bytes(name, byte_size)

def _deserialize_hex_keys(json_dict: Dict[str, str]) -> Dict[int, str]:
  return {int(k, 16): v for k, v in json_dict.items()}

def _register_simple_sorts(registry: IFCRegistry, json_content: Any) -> None:
  for name, info in json_content['simple_sorts'].items():
    try:
      # Register an underlying primitive integer type.
      byte_size = info['size']
      registry.register_uint(f"{name}Sort", byte_size)

      # Register the sort information.
      value_dict = _deserialize_hex_keys(info['values'])
      registry.register_simple_sort(name, value_dict)
    except Exception as e:
      if sys.version_info >= (3, 11):
        e.add_note(
          f"during processing of simple sort: \"{name}\""
        )
      raise

def _register_index_sorts(registry: IFCRegistry, json_content: Any) -> None:
  for name, info in json_content['index_sorts'].items():
    try:
      # Register an underlying primitive integer type.
      byte_size = info['size']
      registry.register_uint(f"{name}Index", byte_size)

      # Register the sort information.
      bitwidth = info['bitwidth']
      value_dict = _deserialize_hex_keys(info['values'])
      non_null = info['non_null']
      registry.register_index_sort(
        name,
        bitwidth,
        value_dict,
        non_null = non_null
      )
    except Exception as e:
      if sys.version_info >= (3, 11):
        e.add_note(
          f"during processing of index sort: \"{name}\""
        )
      raise

def _register_foreign_indexes(registry: IFCRegistry,
                              json_content: Any) -> None:
  for name, byte_size in json_content['foreign_indexes'].items():
    # Register an underlying primitive integer type.
    registry.register_uint(f"{name}ForeignIndex", byte_size)
    registry.register_foreign_index(name)

def _register_node_offsets(registry: IFCRegistry, json_content: Any) -> None:
  for name, info in json_content['node_offsets'].items():
    # Register an underlying primitive integer type.
    byte_size = info['size']
    registry.register_uint(f"{name}Offset", byte_size)

    backing_type_name = info['type']
    non_null = info['non_null']
    registry.register_node_offset(name, backing_type_name, non_null = non_null)

def _register_string_table_offsets(registry: IFCRegistry,
                                   json_content: Any) -> None:
  for name, info in json_content['string_table_offsets'].items():
    # Register an underlying primitive integer type.
    byte_size = info['size']
    registry.register_uint(f"{name}Offset", byte_size)

    backing_type_name = info['type']
    registry.register_string_table_offset(name, backing_type_name)

def _deserialize_position_mapping(json_dict: Dict[str, str]) -> Dict[int, str]:
  return {int(k): v for k, v in json_dict.items()}

def _register_bitfields(registry: IFCRegistry, json_content: Any) -> None:
  for name, info in json_content['bitfields'].items():
    # Register an underlying primitive integer type.
    byte_size = info['size']
    registry.register_uint(f"{name}Bitfield", byte_size)

    # Register the bitfield information.
    empty_name = info['empty']
    position_mapping = info['position']
    special_values = _deserialize_hex_keys(info['special_values'])
    registry.register_bitfield(
      name,
      empty_name,
      _deserialize_position_mapping(position_mapping),
      extra = special_values
    )

def _register_value_categories(registry: IFCRegistry,
                               json_content: Any) -> None:
  for name, info in json_content['value_categories'].items():
    # Register an underlying primitive integer type.
    byte_size = info['size']
    registry.register_uint(f"{name}Category", byte_size)

    # Register the bitfield information.
    bitwidth = info['bitwidth']
    discriminating_sort = info['sort']
    category_dict = info['variants']
    registry.register_value_category(
      name,
      bitwidth,
      discriminating_sort,
      category_dict
    )

def _all_field_types_in_registry(registry: IFCRegistry,
                                 field_types: Collection[str]) -> bool:
  for field_type in field_types:
    if not registry.has_type(field_type):
      return False
  return True

def _register_nodes(registry: IFCRegistry, json_content: Any) -> None:
  node_dict = json_content['nodes']

  # Handle the special case of the FileHeader and Partition nodes which edgifc
  # handles with a distinct role and registration function.
  for name in ['FileHeader', 'Partition']:
    info = node_dict.pop(name)
    fields = info['fields']
    registry.register_header_node(name, fields)

  # This code assumes that for most nodes their field types are already going
  # to be registered. Thus, in the happy path the last element of the list is
  # repeatedly popped, and the list only needs shifted in the rare case that
  # something is missing and the node registration should happen later.
  revisit_queue = list(node_dict.keys())
  prev_revisited = set()
  while len(revisit_queue) > 0:
    name = revisit_queue.pop()
    info = node_dict[name]

    fields = info['fields']
    if not _all_field_types_in_registry(registry, fields.values()):
      if name not in prev_revisited:
        revisit_queue.insert(0, name)
        prev_revisited.add(name)
      else:
        print(f"Could not resolve all fields for: {name}, missing:")
        for field_type in fields.values():
          if not registry.has_type(field_type):
            print(f" - {field_type}")
        sys.exit(1)
      continue

    padding = info['padding']
    registry.register_node(name, fields, padding = padding)

def _register_partitions(registry: IFCRegistry, json_content: Any) -> None:
  for partition_name, type_name in json_content['partitions'].items():
    registry.register_partition(partition_name, type_name)

def _load_json_registry(json_content: Any) -> IFCRegistry:
  registry = IFCRegistry()
  _register_special_case_primitives(registry)
  _register_integer_primitives(registry, json_content)
  _register_byte_primitives(registry, json_content)
  _register_simple_sorts(registry, json_content)
  _register_index_sorts(registry, json_content)
  _register_foreign_indexes(registry, json_content)
  _register_node_offsets(registry, json_content)
  _register_string_table_offsets(registry, json_content)
  _register_bitfields(registry, json_content)
  _register_value_categories(registry, json_content)
  _register_nodes(registry, json_content)
  _register_partitions(registry, json_content)
  return registry

def _get_json_spec_contents(name: str) -> bytes:
  result = pkgutil.get_data(__name__, f"spec_files/{name}.json")
  assert result is not None
  return result

def _load_fundamental_registry() -> IFCRegistry:
  spec_json = json.loads(_get_json_spec_contents('fundamental'))
  return _load_json_registry(spec_json)

class UnsupportedIFCVersionError(Exception):
  def __init__(self, bad_version: IFCVersionInfo):
    self.bad_version = bad_version

  def __str__(self):
    return f"Unsupported IFC version: {self.bad_version}"

class BadChecksumError(Exception):
  def __init__(self):
    pass

  def __str__(self):
    return 'The file checksum was not valid'

SUPPORTED_VERSIONS = [
  IFCVersionInfo(0, 33),
  IFCVersionInfo(0, 41),
  IFCVersionInfo(0, 42),
  IFCVersionInfo(0, 43),
  IFCVersionInfo(0, 44)
]

def get_registry_version(version: IFCVersionInfo) -> IFCRegistry:
  if version not in SUPPORTED_VERSIONS:
    raise UnsupportedIFCVersionError(version)
  spec_json = json.loads(_get_json_spec_contents(str(version)))
  registry = _load_json_registry(spec_json)
  _init_tacit_fields(registry, version)
  return registry

class StreamReader:
  def __init__(self, ifc_reader):
    self._ifc_reader = ifc_reader

  def _get_input_stream(self):
    return self._ifc_reader._input_stream

  def _get_header(self):
    return self._ifc_reader._file_header

  def _get_registry(self):
    return self._ifc_reader._registry

  def _lookup_partition(self, partition_name):
    return self._ifc_reader._partitions[partition_name]

  def read_text(self, offset):
    file_header = self._get_header()

    input_stream = self._get_input_stream()
    input_stream.seek(file_header.string_table_bytes() + offset)

    chars = []
    while True:
      char = input_stream.read(1).decode('utf-8')
      if char == '\0':
        break
      chars.append(char)

    return ''.join(chars)

  def read(self, partition_name, index = 0):
    partition = self._lookup_partition(partition_name)

    input_stream = self._get_input_stream()
    input_stream.seek(partition.offset() + (partition.entry_size() * index))

    registry = self._get_registry()
    ifc_type = registry.get_type_for_partition_name(partition_name)
    return ifc_type.parse(input_stream)

  def read_by_index(self, index_name, encoded_value):
    registry = self._get_registry()
    index_reg = registry.get_index_sort_registration(index_name)

    type_name = index_reg.get_tag(encoded_value)
    index = index_reg.get_index(encoded_value)

    return self.read(registry.get_partition_for_type_name(type_name), index)

  def read_by_offset(self, offset_name, encoded_value):
    registry = self._get_registry()
    offset_reg = registry.get_node_offset_registration(index_name)

    type_name = offset_reg.backing_type_name

    return self.read(
      registry.get_partition_for_type_name(type_name),
      encoded_value
    )

  def read_next(self, type_name):
    registry = self._get_registry()
    input_stream = self._get_input_stream()
    return registry.get_type(type_name).parse(input_stream)

  def read_header(self):
    self._get_input_stream().seek(4)
    return self.read_next('FileHeader')

  def read_partitions(self):
    file_header = self._get_header()

    input_stream = self._get_input_stream()
    input_stream.seek(file_header.toc())

    partitions = []
    for idx in range(file_header.partition_count()):
      partitions.append(self.read_next('Partition'))
    return partitions

MICROSOFT_IFC_MAGIC_NUMBERS = b'\x54\x51\x45\x1A'
EDG_IFC_MAGIC_NUMBERS       = b'\x54\x51\x45\x2C'

class IFCDialect(Enum):
  UNKNOWN      = 1
  EDG          = 2
  MICROSOFT    = 3

class IFCReader:
  def __init__(self, input_stream):
    self._input_stream = input_stream

    self._registry = None
    self._file_header = None
    self._partitions_list = []
    self._partitions = {}
    self._partition_indexes = {}
    self._dialect = self._detect_dialect()

  def _get_version_info(self):
    return IFCVersionInfo(
      self._file_header.major_version(),
      self._file_header.minor_version()
    )

  def _detect_dialect(self):
    # Identify the IFC file based on its "magic numbers."
    dialect_bytes = self._input_stream.read(4)
    if dialect_bytes == EDG_IFC_MAGIC_NUMBERS:
      return IFCDialect.EDG
    if dialect_bytes == MICROSOFT_IFC_MAGIC_NUMBERS:
      return IFCDialect.MICROSOFT
    return IFCDialect.UNKNOWN

  def _check_checksum(self):
    # Make sure we're past the file's "magic numbers."
    self._input_stream.seek(4)
    # Read the checksum for the file.
    expected_checksum = self._input_stream.read(32)
    # Compute the checksum for the file based on its current state.
    hash_state = hashlib.sha256()
    reusable_buffer = bytearray(100_000_000)
    while True:
      count = self._input_stream.readinto(reusable_buffer)
      if count == 0:
        break

      if count != len(reusable_buffer):
        hash_state.update(reusable_buffer[:count])
      else:
        hash_state.update(reusable_buffer)

    # Compare the computed checksum to the read checksum.
    if hash_state.digest() != expected_checksum:
      raise BadChecksumError()

  def _process_header(self):
    # Setup a bare bones registry and read the file header
    self._registry = _load_fundamental_registry()
    stream_reader = StreamReader(self)
    self._file_header = stream_reader.read_header()

    # Read the partition table
    for idx, partition in enumerate(stream_reader.read_partitions()):
      partition_name = stream_reader.read_text(partition.name())
      self._partitions[partition_name] = partition
      self._partition_indexes[partition_name] = idx
      self._partitions_list.append((partition_name, partition))

  def _load_full_registry(self):
    self._registry = get_registry_version(self._get_version_info())

  def init_file(self) -> None:
    '''Initialize the file, this operation may throw exceptions for malformed
    or unprocessable files.
    '''
    self._check_checksum()
    self._process_header()
    self._load_full_registry()

  def get_dialect(self) -> IFCDialect:
    return self._dialect

  def get_header(self):
    return self._file_header

  def get_header_type(self):
    return self._registry.get_type('FileHeader')

  def get_partitions(self):
    return self._partitions_list

  def get_partition_names(self):
    return list(map(lambda part_tup: part_tup[0], self._partitions_list))

  def get_partition_index(self, name):
    return self._partition_indexes[name]

  def get_partition_by_index(self, index):
    return self.get_partitions()[index]

  def get_partition_name_by_index(self, index):
    return self.get_partitions()[index][0]

  def supports_partition(self, name):
    return self._registry.has_partition(name)

  def supports_partition_at_index(self, index):
    partition_name = self.get_partition_name_by_index(index)
    return self.supports_partition(partition_name)

  def get_type_name_for_partition_name(self, partition_name):
    return self._registry.get_type_name_for_partition_name(partition_name)

  def get_type_for_partition(self, partition_name):
    return self._registry.get_type_for_partition_name(partition_name)

  def has_partition_for_type(self, type_name):
    return self._registry.has_partition_for_type_name(type_name)

  def get_partition_name_by_type(self, type_name):
    return self._registry.get_partition_for_type_name(type_name)

  def get_all_registered_partition_types(self):
    return self._registry.get_all_partition_types()

  def get_all_registered_simple_sorts(self):
    return self._registry.get_all_simple_sorts()

  def get_all_registered_index_sorts(self):
    return self._registry.get_all_index_sorts()

  def get_all_registered_bitfields(self):
    return self._registry.get_all_bitfields()

  def get_type_role(self, type_name: str) -> IFCRegistryTypeRole:
    return self._registry.get_type_role(type_name)

  def get_sort_tag(self, sort_name, sort_value):
    return self._registry.get_sort_tag(sort_name, sort_value)

  def _decompose_index_sort(self, sort_reg, sort_value):
    return (
      sort_reg.get_tag(sort_value),
      sort_reg.get_index(sort_value)
    )

  def get_decomposed_index_sort(self, sort_name, sort_value):
    sort_reg = self._registry.get_index_sort_registration(sort_name)
    return self._decompose_index_sort(sort_reg, sort_value)

  def get_decomposed_index_sort_by_type(self, sort_name, sort_value):
    sort_reg = self._registry.get_index_sort_registration_by_type(sort_name)
    return self._decompose_index_sort(sort_reg, sort_value)

  def _decompose_index_sort_values(self, sort_reg, sort_value):
    return (
      sort_reg.get_tag_encoding(sort_value),
      sort_reg.get_index(sort_value)
    )

  def get_decomposed_index_sort_values(self, sort_name, sort_value):
    sort_reg = self._registry.get_index_sort_registration(sort_name)
    return self._decompose_index_sort_values(sort_reg, sort_value)

  def get_decomposed_index_sort_values_by_type(self, type_name, sort_value):
    sort_reg = self._registry.get_index_sort_registration_by_type(sort_name)
    return self._decompose_index_sort_values(sort_reg, sort_value)

  def is_node_offset_non_null(self, offset_name: str) -> bool:
    offset_reg = self._registry.get_node_offset_registration(offset_name)
    return offset_reg.non_null

  def is_node_offset_non_null_by_type(self, type_name: str) -> bool:
    offset_reg = self._registry.get_node_offset_registration_by_type(type_name)
    return offset_reg.non_null

  def get_node_offset_backing_type_name(self, offset_name: str) -> str:
    offset_reg = self._registry.get_node_offset_registration(offset_name)
    return offset_reg.backing_type_name

  def get_node_offset_backing_type_name_by_type(self, type_name: str) -> str:
    offset_reg = self._registry.get_node_offset_registration_by_type(type_name)
    return offset_reg.backing_type_name

  def get_bitfield_values(self, bitfield_name, bitfield_value):
    bitfield_reg = self._registry.get_bitfield_registration(bitfield_name)
    # Seach for matching bitmasks
    zero_match = None
    values = []
    for mask, value in bitfield_reg.items():
      # The bitmask must be exactly to the spec of the mask, because Microsoft
      # has masks that use multiple bits for some bitfields.
      if (bitfield_value & mask) == mask:
        # If this is the all-zero/empty/none mask save it for later.
        if mask == 0:
          zero_match = value
        else:
          values.append(value)
    # Fallback to zero match if it exists and nothing else was found.
    if len(values) == 0 and zero_match is not None:
      values.append(zero_match)
    return values

  def render_python_value(self, value: Any, type_name: str) -> str:
    '''This function provides a simple, common rendering function for the raw
    values of various roles.
    '''
    try:
      type_role = self.get_type_role(type_name)
      if type_role == IFCRegistryTypeRole.BITFIELD:
        return bin(value)
      if type_role == IFCRegistryTypeRole.INDEX_SORT:
        return hex(value)
      if type_role == IFCRegistryTypeRole.RAW_BYTES:
        if type_name == 'IEEELEFloat':
          # This is a special case for the IEEE little-endian float type.
          #
          # The Python EDG IFC library parses this out into a Python floating
          # point type (and thus even though it's "role" is raw-bytes we can do
          # better).
          return str(value)
        if sys.version_info >= (3, 8):
          return f"0x{value.hex('-', 4)}"
        return f"0x{value.hex()}"

      return str(value)
    except Exception as e:
      if sys.version_info >= (3, 11):
        if type_role is not None:
          e.add_note(f"for type name: \"{type_name}\" ({type_role})")
        else:
          e.add_note(f"for type name: \"{type_name}\"")
      raise


