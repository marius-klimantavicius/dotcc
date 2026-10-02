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

from .attribute import register_attributes
from .constant import register_constants
from .expr import register_exprs
from .ifc import register_ifc_entities
from .il_def import register_il_def
from .lexical import register_lexical_types
from .mem_manage import register_mem_management
from .operand import register_operands
from .srcpos import register_srcpos
from .support import InspectionCacheManager
from .symbol import register_symbols
from .template import register_templates
from .token import register_tokens
from .type import register_types
from .util import register_util_types

from gdb import current_objfile
from gdb.printing import RegexpCollectionPrettyPrinter, register_pretty_printer

def register():
  # Create a new pretty printer "pretty-cpfe"
  pretty_printer_registry = RegexpCollectionPrettyPrinter("pretty-cpfe")

  # Register the InspectionCacheManager, this is used to provide pretty
  # printing information based off a memory address.
  #
  # Thus, this facilitates one printer passing information about an object to
  # be printed to another printer.
  inspection_cache = InspectionCacheManager.inst()
  inspection_lookup = lambda value: inspection_cache.lookup_printer(value)
  register_pretty_printer(current_objfile(), inspection_lookup)

  # Register associated pretty printers for "pretty-cpfe"
  register_attributes(pretty_printer_registry)
  register_constants(pretty_printer_registry)
  register_exprs(pretty_printer_registry)
  register_ifc_entities(pretty_printer_registry)
  register_il_def(pretty_printer_registry)
  register_lexical_types(pretty_printer_registry)
  register_mem_management(pretty_printer_registry)
  register_operands(pretty_printer_registry)
  register_srcpos(pretty_printer_registry)
  register_symbols(pretty_printer_registry)
  register_templates(pretty_printer_registry)
  register_tokens(pretty_printer_registry)
  register_types(pretty_printer_registry)
  register_util_types(pretty_printer_registry)

  # Register the pretty printer with gdb
  #
  # FIXME: We should probably just register this for cpfe related binaries
  #        not _every_ binary.
  register_pretty_printer(current_objfile(), pretty_printer_registry)
