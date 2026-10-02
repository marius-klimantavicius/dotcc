=======
Modules
=======

The EDG modules implementation is based upon the Microsoft IFC specification.

The EDG front end has experimental support for both the IFC variant produced by
Microsoft's Visual Studio compiler and an EDG variant produced by the EDG front
end.

Roles
=====

To make the system work, every IFC type is understood in terms of a "role". The
type's role in effect defines what semantics it will be generated with.

The roles are as follows:

* :ref:`ifc-simple-sort-role` - Any kind of enumeration of named values
  unrelated to an index.
* :ref:`ifc-index-sort-role` - A "sort" that encodes a simple sort and an
  associated index (e.g., ``DeclIndex`` is the "Index Sort", with a "Simple
  Sort" ``DeclSort``).
* :ref:`ifc-bitfield-role` - Any IFC type that encodes things using bit flags.
* :ref:`ifc-raw-numeric-role` - Any IFC type that is an integer with no
  associated implicit interpretation (from a format perspective, it's just a
  "dumb" number).
* :ref:`ifc-raw-bytes-role` - Similar to Raw Numeric, anything that's just a
  raw byte array.
* :ref:`ifc-node-roles`
   * :ref:`ifc-partition-node-role` - An IFC Node that can be directly read
     from a partition, for instance a ``DeclFunction``.
   * :ref:`ifc-basic-node-role` - An IFC Node that only appears as a
     sub-node (doesn't have a partition of its own), for instance, a
     ``ModuleReference``.
   * :ref:`ifc-header-node-role` - Basically what it says, an IFC Node that's
     part of the header (``FileHeader`` and ``Partition``).
* :ref:`ifc-foreign-index-role` - A special role for IFC types that represent
  an Index in another module file.
* :ref:`ifc-value-category-role` - A role representing several levels of
  discriminated values (conceptually similar to an IFC ``std::variant``)

.. _ifc-simple-sort-role:

Simple Sort
-----------

Simple sorts are represented as an enum (sorted in alphabetical order):

.. code-block:: c++

   enum an_ifc_access_sort {
     ifc_as_none,
     ifc_as_private,
     ifc_as_protected,
     ifc_as_public
   };  /* an_ifc_access_sort */

Simple sorts can also be converted to a c-string of their respective EDG name
via a call to the respective ``str_for`` function. This can be useful for
debugging and diagnostic purposes.

.. _ifc-index-sort-role:

Index Sort
----------

Index sorts are represented as an index type (a struct with a module binding,
an associated :ref:`ifc-simple-sort-role` value, and the associated index
value):

.. code-block:: c++

   enum an_ifc_decl_sort {
     ifc_ds_decl_alias,
     ifc_ds_decl_barren,
     ifc_ds_decl_bitfield,
     ifc_ds_decl_concept,
     ifc_ds_decl_constructor,
     ifc_ds_decl_deduction_guide,
     ifc_ds_decl_destructor,
     ifc_ds_decl_enumeration,
     ifc_ds_decl_enumerator,
     ifc_ds_decl_expansion,
     ifc_ds_decl_explicit_instantiation,
     ifc_ds_decl_explicit_specialization,
     ifc_ds_decl_field,
     /* snip */
   };  /* an_ifc_decl_sort */

   /*
   The universal representation for an IFC DeclIndex.
   */
   struct an_ifc_decl_index {
     an_ifc_module*
                   mod;
                           /* The associated module. */
     an_ifc_decl_sort
                   sort;
                           /* The associated DeclSort value for this index. */
     uint32_t      value;
                           /* The index value into the associated partition of
                              "sort" for this index.  Represented as the
                               largest common underlying type. */
   };  /* an_ifc_decl_index */

.. _ifc-bitfield-role:

Bitfield
--------

Bitfields are represented as a bitfield type (a struct with a module binding
and the raw value pulled from the module file):

.. code-block:: c++

   /*
   The universal representation for an IFC FunctionTraitsBitfield.
   */
   struct an_ifc_function_traits_bitfield {
     an_ifc_module*
                   mod;
                           /* The associated module. */
     an_ifc_function_traits_bitfield_storage
                   value;
                           /* The raw bit value obtained from the module.
                              Represented as the largest common underlying
                              type. */
   };  /* an_ifc_function_traits_bitfield */

This is then paired with a (alphabetically sorted) set of bit queries.

.. code-block:: c++

   enum an_ifc_function_traits_bitfield_query : uint32_t {
     ifc_ftb_constexpr     = 1 << 0,
     ifc_ftb_constrained   = 1 << 1,
     ifc_ftb_defaulted     = 1 << 2,
     ifc_ftb_deleted       = 1 << 3,
     ifc_ftb_explicit      = 1 << 4,
     ifc_ftb_hidden_friend = 1 << 5,
     ifc_ftb_immediate     = 1 << 6,
     ifc_ftb_inline        = 1 << 7,
     ifc_ftb_no_return     = 1 << 8,
     ifc_ftb_none          = 1 << 9,
     ifc_ftb_pure_virtual  = 1 << 10,
     ifc_ftb_virtual       = 1 << 11
   };  /* an_ifc_function_traits_bitfield_query */

A bitmask can then before formed and tested on a bitfield like so:

.. code-block:: c++

    an_ifc_function_traits_bitfield bitfield = ...;

    if (test_bitmask<ifc_ftb_immediate | ifc_ftb_explicit>(bitfield)) {
      /* matched bitfield & (ifc_ftb_immediate | ifc_ftb_explicit) */
    } else {
      /* didn't match */
    }  /* if */

.. _ifc-raw-numeric-role:

Raw Numeric
-----------

Raw numerics are represented as a raw numeric type (a struct with a module
binding and the raw value pulled from the module file, along with a convenience
conversion operator):

.. code-block:: c++

   /*
   The universal representation for an IFC Index.
   */
   struct an_ifc_index {
     an_ifc_module*
                   mod;
                           /* The associated module. */
     an_ifc_index_storage
                   value;
                           /* The raw bit value obtained from the module.
                              Represented as the largest common underlying
                              type. */

     inline operator an_ifc_index_storage() const
       { return this->value; }
   };  /* an_ifc_index */

Thus, the value itself can be passed around safely without concern of changes
in the underlying type, and with access to the module, while being trivial to
work with as an unsigned number:

.. code-block:: c++

    an_ifc_index power_idx = ...;

    if (power_idx > 9000) {
      /* panic */
    }  /* if */

.. _ifc-raw-bytes-role:

Raw Bytes
---------

Types generated from the raw bytes role are derived from
:ref:`ifc-byte-buffers`.

.. code-block:: c++

   /*
   The universal representation of an IFC SHA256 node.

   The representation is declared as a derived struct of an_ifc_Byte_buffer
   rather than simply an alias of the instantiation to improve the debugger
   experience.
   */
   struct an_ifc_sha256 : an_ifc_Byte_buffer<an_ifc_sha256_storage> {
     using storage_type = an_ifc_sha256_storage;
     using base_type = an_ifc_Byte_buffer<storage_type>;
     using base_type::an_ifc_Byte_buffer;
   };  /* an_ifc_sha256 */

These types also include an additional table documenting the number of bytes
available (starting from a specific version). For example, if a type has
32 bytes starting with version 0.33, and that doubles to 64 bytes in 0.41
the following table will be written:

.. code-block:: text

    |----------------|
    | Version | Size |
    |---------|------|
    | 0.33    | 32   |
    | 0.41    | 64   |
    |---------|------|


.. _ifc-node-roles:

Nodes
-----

The node roles (:ref:`ifc-partition-node-role`, :ref:`ifc-basic-node-role`, and
:ref:`ifc-header-node-role`) are all very similar, differing only in their
construction.

The node types are -- similarly to :ref:`ifc-raw-bytes-role` types -- derived
from :ref:`ifc-byte-buffers`.

.. code-block:: c++

   /*
   The universal representation of an IFC DeclFunction node.

   The representation is declared as a derived struct of an_ifc_Byte_buffer
   rather than simply an alias of the instantiation to improve the debugger
   experience.
   */
   struct an_ifc_decl_function :
                             an_ifc_Byte_buffer<an_ifc_decl_function_storage> {
     using storage_type = an_ifc_decl_function_storage;
     using base_type = an_ifc_Byte_buffer<storage_type>;
     using base_type::an_ifc_Byte_buffer;
   };  /* an_ifc_decl_function */

Additionally, -- again similarly to :ref:`ifc-raw-bytes-role` types -- node
types include a descriptive table to aid in development:

.. code-block:: text

     |------------------------------------------------------------|
     |               DeclFunction - 0.33 (32 bytes)               |
     |-------------|-----------------------------|---------|------|
     | Name        | Type                        | Version | Size |
     |-------------|-----------------------------|---------|------|
     | name        | NameIndex                   | 0.33    | 4    |
     | locus       | SourceLocation              | 0.33    | 8    |
     | type        | TypeIndex                   | 0.33    | 4    |
     | home_scope  | DeclIndex                   | 0.33    | 4    |
     | chart       | ChartIndex                  | 0.33    | 4    |
     | traits      | FunctionTraitsBitfield      | 0.33    | 2    |
     | specifiers  | BasicSpecifiersBitfield     | 0.33    | 1    |
     | access      | AccessSort                  | 0.33    | 1    |
     | properties  | ReachablePropertiesBitfield | 0.33    | 1    |
     | __padding__ | uint8_t[3]                  |         | 3    |
     |-------------|-----------------------------|---------|------|

     |------------------------------------------------------------|
     |               DeclFunction - 0.41 (32 bytes)               |
     |-------------|-----------------------------|---------|------|
     | Name        | Type                        | Version | Size |
     |-------------|-----------------------------|---------|------|
     | name        | NameIndex                   | 0.33    | 4    |
     | locus       | SourceLocation              | 0.33    | 8    |
     | type        | TypeIndex                   | 0.33    | 4    |
     | home_scope  | DeclIndex                   | 0.41    | 4    |
     | chart       | ChartIndex                  | 0.33    | 4    |
     | traits      | FunctionTraitsBitfield      | 0.33    | 2    |
     | specifiers  | BasicSpecifiersBitfield     | 0.33    | 1    |
     | access      | AccessSort                  | 0.33    | 1    |
     | properties  | ReachablePropertiesBitfield | 0.33    | 1    |
     | __padding__ | uint8_t[3]                  |         | 3    |
     |-------------|-----------------------------|---------|------|
     | home_scope  | DeclIndex                   | 0.41    | TF   |
     |-------------|-----------------------------|---------|------|

Here, the first table describes ``DeclFunction`` from IFC version 0.33 up until
0.41 (non-inclusive) as a 32 byte structure. Each of the structure's fields are
listed with their name, return type, the interpretation version of the return
type, and the size of the field. Additionally, a ``__padding__`` field is
listed specifying there are 3 bytes of unused (or unknown) data padding the end
of the structure (which would otherwise be 29 bytes).

The second table describes ``DeclFunction`` for IFC version 0.41+
(inclusive).  This table is very similar to the structure describing the
earlier version of ``DeclFunction``. However, this table specifies that the
home_scope's ``DeclIndex`` has an updated value interpretation, and in the
bottom row of the table sized "TF" that a tacit field is being used to
define (in this case redefine) home_scope (see :ref:`ifc-tacit-field` for
more information on tacit fields).

The values of these fields can be retrieved via calling the ``get_ifc_``\ *X*
function where *X* is the name of the field (for information on the inner
workings of these functions, see :ref:`ifc-node-versioning`). For instance, to
retrieve the ``home_scope`` (independent of the IFC version and underlying
semantics) the following function can be used:

.. code-block:: c++

   an_ifc_decl_function func_decl = /* ... */;
   an_ifc_decl_index    home_scope = get_ifc_home_scope(func_decl);

The returned ``home_scope`` is pre-validated during the node's
construction, and ready for usage (for more information, see
:ref:`validation`).

.. _ifc-partition-node-role:

Partition Node
^^^^^^^^^^^^^^

To get a partition node, in effectively all front end code, ``construct_node``
should be used (for information on exceptional cases, see
:ref:`skipping-validation`). This looks something like:

.. code-block:: c++

   Opt<an_ifc_decl_function> opt_idf;

   construct_node(&opt_idf, index);

This will load the appropriate value found at the given index, run validation
(if necessary), and (if validation is successful) store the node in the ``Opt``
variable.

.. _ifc-basic-node-role:

Basic Node
^^^^^^^^^^

Basic nodes are only obtainable from a ``get_ifc_``\ *X* operation, and used to
represent nodes which are nested in other nodes. For instance, you might have:

.. code-block:: c++

   an_ifc_decl_reference decl_ref = ...;

   an_ifc_module_reference module_reference = get_ifc_unit(decl_ref);

Here the returned ``an_ifc_module_reference`` is a basic node. Note that this
is not wrapped in an optional, as the ``an_ifc_module_reference`` being a
sub-node of ``an_ifc_decl_reference`` was validated when
``an_ifc_decl_reference`` was constructed.

.. _ifc-header-node-role:

Header Node
^^^^^^^^^^^

Header nodes rarely need new code written for them, there are only two at the
time of writing (and for the foreseeable future):

* ``an_ifc_file_header``
* ``an_ifc_partition``

To get a header node, the low level ``construct_node_from_module`` is used
directly. Similarly, since header nodes are lower level, validate functions
need called explicitly.

The flow for interaction with these nodes is typically something like the
following (if not in practice, conceptually):

.. code-block:: c++

   Opt<an_ifc_file_header> get_module_header(an_ifc_module *mod)
   {
     Opt<an_ifc_file_header> result;
     an_ifc_file_header      raw_result;

     init_byte_buffer(4, mod->f_size - 4);
     raw_result = construct_node_from_module<an_ifc_file_header>(mod);
     if (validate(raw_result, /*parent=*/NULL)) {
       result = raw_result;
     }  /* if */
     return result;
   }  /* get_module_header */

This is conceptually very similar to what ``construct_node`` is doing behind
the scenes.

.. _ifc-foreign-index-role:

Foreign Index
-------------

Currently in the IFC spec there's a ``DeclSort::Reference`` structure:

+-----------------+---------------------+
| Field           | Type                |
+=================+=====================+
| ``unit``        | ``ModuleReference`` |
+-----------------+---------------------+
| ``local_index`` | ``DeclIndex``       |
+-----------------+---------------------+

The problem is that the ``local_index`` field uses a ``DeclIndex``, but
that ``DeclIndex`` is valid in a completely different module. This has
caused issues for the front end in the past where the index is accidentally
being used in the current module's code.

The "Foreign Index" role is used to replace the ``DeclIndex`` type with a
new ``DeclForeignIndex`` type.  This new type is more akin to a
:ref:`ifc-raw-numeric-role` than a :ref:`ifc-index-sort-role`.

The front end thus represents this as the following node structure:

+-----------------+----------------------+
| Field           | Type                 |
+=================+======================+
| ``unit``        | ``ModuleReference``  |
+-----------------+----------------------+
| ``local_index`` | ``DeclForeignIndex`` |
+-----------------+----------------------+

The ``DeclForeignIndex`` type in C++ code looks as follows:

.. code-block:: c++

   /*
   The universal representation for an IFC DeclForeignIndex.
   */
   struct an_ifc_decl_foreign_index {
     an_ifc_module*
                   mod;
                           /* The associated module. */
     an_ifc_decl_foreign_index_storage
                   value;
                           /* The raw bit value obtained from the module.
                              Represented as the largest common underlying
                              type. */
   };  /* an_ifc_decl_foreign_index */

This allows the index value to be imported into the current module in a
type safe way that can't be accidentally treated as a ``DeclIndex``. Thus,
interacting with the ``DeclReference`` you get something like:

.. code-block:: c++

   an_ifc_decl_reference decl_ref = ...;

   an_ifc_decl_foreign_index local_index = get_ifc_local_index(decl_ref);
   process_ifc_decl(local_index); /* type error */

In terms of correct usage, foreign indexes are typically deserialized via an
associated :ref:`ifc-tacit-field` which reconstructs them as the appropriate
:ref:`ifc-index-sort-role` in the context of their respective module.

As an example consider the DeclReference type:

.. code-block:: text

      |-------------------------------------------------|
      |         DeclReference - 0.33 (12 bytes)         |
      |-------------|------------------|---------|------|
      | Name        | Type             | Version | Size |
      |-------------|------------------|---------|------|
      | unit        | ModuleReference  | 0.33    | 8    |
      | local_index | DeclForeignIndex | 0.33    | 4    |
      |-------------|------------------|---------|------|
      | index       | DeclIndex        | ANY     | TF   |
      |-------------|------------------|---------|------|

Using the tacit field ``index``, ``local_index`` can be retrieved in the
context of the associated ``unit`` (i.e., IFC module) transparently:

.. code-block:: c++

    an_ifc_decl_reference ref = ...;
    an_ifc_decl_index     idx = get_ifc_index(ref);

.. _ifc-value-category-role:

Value Category
--------------

Value categories are roughly equivalent to a variant type. The categories have
a :ref:`ifc-simple-sort-role` which describes which variant is valid.  The
actual variants can be any role.

This allows for handling of nested hierarchies like IFC Operators where an
``OperatorSort`` selects the interpretation of the variant value which may
be one of several different :ref:`Simple Sorts<ifc-simple-sort-role>`.

Versioning
==========

.. _ifc-simple-sort-versioning:

Simple Sort
-----------

The :ref:`ifc-simple-sort-role` versioning system works by converting from a
"versioned sort" (i.e., enumeration) to a "universal sort" (i.e., enumeration).
Each IFC version gets its own unique, and appropriately sized enumeration, like
so:

.. code-block:: c++

    enum an_ifc_decl_sort_0_33 : uint32_t {
      ifc_0_33_ds_decl_vendor_extension        = 0,
      ifc_0_33_ds_decl_enumerator              = 1,
      ifc_0_33_ds_decl_variable                = 2,
      ifc_0_33_ds_decl_parameter               = 3,
      ifc_0_33_ds_decl_field                   = 4,
      ifc_0_33_ds_decl_bitfield                = 5,
      ifc_0_33_ds_decl_scope                   = 6,
      ifc_0_33_ds_decl_enumeration             = 7,
      /* snip */
    };  /* an_ifc_decl_sort_0_33 */

    enum an_ifc_decl_sort_0_41 : uint32_t {
      ifc_0_41_ds_decl_vendor_extension       = 0,
      ifc_0_41_ds_decl_enumerator             = 1,
      ifc_0_41_ds_decl_variable               = 2,
      ifc_0_41_ds_decl_parameter              = 3,
      ifc_0_41_ds_decl_field                  = 4,
      ifc_0_41_ds_decl_bitfield               = 5,
      ifc_0_41_ds_decl_scope                  = 6,
      ifc_0_41_ds_decl_enumeration            = 7,
      /* snip */
    };  /* an_ifc_decl_sort_0_41 */

Note that these versioned enumerators do not follow the typical naming
convention. This is done to reduce the amount of possible completions editor
auto complete has for ``ifc_ds_`` (and similar).

The universal sort representation by contrast takes all of the unique names,
from all the versioned sorts, and puts them in one enumeration:

.. code-block:: c++

    enum an_ifc_decl_sort : uint32_t {
      ifc_ds_decl_alias,
      ifc_ds_decl_barren,
      ifc_ds_decl_bitfield,
      ifc_ds_decl_concept,
      ifc_ds_decl_constructor,
      ifc_ds_decl_deduction_guide,
      ifc_ds_decl_destructor,
      ifc_ds_decl_enumeration,
      ifc_ds_decl_enumerator,
      ifc_ds_decl_expansion,
      ifc_ds_decl_explicit_instantiation,
      ifc_ds_decl_explicit_specialization,
      ifc_ds_decl_field,
      /* snip */
    };  /* an_ifc_decl_sort */

This enumeration is sorted alphabetically to make things easier to find and to
prevent the list from being "shuffled" when new IFC versions are adopted.

Functions are then added to convert between the versioned sort (read from the
module file) to the universal sort, they look something like this:

.. code-block:: c++

    inline an_ifc_decl_sort to_universal_sort(an_ifc_decl_sort_0_33 versioned)
    /*
    Given the versioned representation of DeclSort, return the corresponding
    universal representation.
    */
    {
      an_ifc_decl_sort result;

      switch (versioned) {
        case ifc_0_33_ds_decl_vendor_extension:
          result = ifc_ds_decl_vendor_extension;
          break;
        case ifc_0_33_ds_decl_enumerator:
          result = ifc_ds_decl_enumerator;
          break;
        case ifc_0_33_ds_decl_variable:
          result = ifc_ds_decl_variable;
          break;
      /* snip */

This allows for stability in all of our logic despite a number of possible
changes to the sort for a particular version (e.g., numbering changes, removal,
and addition). Effectively, sorts become unique on name rather than unique on
value.

.. _ifc-index-sort-versioning:

Index Sort
----------

As an :ref:`ifc-index-sort-role` is composed of a :ref:`ifc-simple-sort-role`
and an index value, its versioning relies upon the versioning of the associated
:ref:`ifc-simple-sort-role` (see :ref:`ifc-simple-sort-versioning`).

.. _ifc-bitfield-versioning:

Bitfield
--------

:ref:`ifc-bitfield-role` versioning works very similarly to
:ref:`ifc-simple-sort-versioning`.

There's similarly a versioned bitfield enum for each version, of appropriate
size, using the values that are correct for that version:

.. code-block:: c++

    enum an_ifc_basic_specifiers_bitfield_0_33 : uint8_t {
      ifc_0_33_bsb_cxx                        = 0,
      ifc_0_33_bsb_c                          = 1 << 0,
      ifc_0_33_bsb_internal                   = 1 << 1,
      ifc_0_33_bsb_vague                      = 1 << 2,
      ifc_0_33_bsb_external                   = 1 << 3,
      ifc_0_33_bsb_deprecated                 = 1 << 4,
      ifc_0_33_bsb_initialized_in_class       = 1 << 5,
      ifc_0_33_bsb_non_exported               = 1 << 6,
      ifc_0_33_bsb_is_member_of_global_module = 1 << 7
    };  /* an_ifc_basic_specifiers_bitfield_0_33 */

Unlike sorts, it's not super practical to try and normalize the representation
itself, so instead the value is captured with a module reference.

.. code-block:: c++

    /*
    The universal representation for an IFC BasicSpecifiersBitfield.
    */
    struct an_ifc_basic_specifiers_bitfield {
            an_ifc_module*
                    mod;
                            /* The associated module. */
            an_ifc_basic_specifiers_bitfield_storage
                    value;
                            /* The raw bit value obtained from the module.
                               Represented as the largest common underlying
                               type. */
    };  /* an_ifc_basic_specifiers_bitfield */

This is then paired with a universal query enum (similar to the universal sort)
that gives a unique bit value to each possible bitfield and a ``test_bitmask``
function.

.. code-block:: c++

   enum an_ifc_basic_specifiers_bitfield_query : uint32_t {
     ifc_bsb_c                          = 1 << 0,
     ifc_bsb_cxx                        = 1 << 1,
     ifc_bsb_deprecated                 = 1 << 2,
     ifc_bsb_external                   = 1 << 3,
     ifc_bsb_initialized_in_class       = 1 << 4,
     ifc_bsb_internal                   = 1 << 5,
     ifc_bsb_is_member_of_global_module = 1 << 6,
     ifc_bsb_non_exported               = 1 << 7,
     ifc_bsb_vague                      = 1 << 8
   };  /* an_ifc_basic_specifiers_bitfield_query */


   template<an_ifc_basic_specifiers_bitfield_query a_Query>
   inline a_boolean test_bitmask(
                            const an_ifc_basic_specifiers_bitfield &universal)
   /*
   Given the universal representation of BasicSpecifiersBitfield, return TRUE
   if the universal bitmask specified by a_Query matches; otherwise, return
   FALSE.
   */
   {
     a_boolean result;

     uint8_t mask = to_bitmask_0_33(a_Query);
     uint8_t test_value = universal.value & mask;

     result = test_value == mask;
     return result;
   }  /* test_bitmask */

At a high level, these functions generate per-version branches. Each of these
branches calls a function that converts the universal query, into a version
specific query (i.e., bitmask) for respective version (in this case there's
only one version of the bitfield, so there are no branches, just a call to
``to_bitmask_0_33``). These bitmask conversion functions look something like
the following:

.. code-block:: c++

    uint8_t to_bitmask_0_33(an_ifc_basic_specifiers_bitfield_query query)
    /*
    Given the universal representation of a BasicSpecifiersBitfield bitmask,
    return the corresponding IFC version 0.33 representation of the bitmask.
    */
    {
      uint8_t result = 0;

      if (query & ifc_bsb_cxx) {
        result |= ifc_bsb_0_33_cxx;
      }  /* if */
      if (query & ifc_bsb_c) {
        result |= ifc_bsb_0_33_c;
      }  /* if */
      /* snip */

The versioned specific bitmask is then tested against the version specific
value, returning the result.

This approach allows the front end to test "the same" bitmasks in a version
agnostic way, with a very small runtime performance overhead.

Bitwidth Limitations
^^^^^^^^^^^^^^^^^^^^

As bitfield versioning relies upon native integer types, there are limitations
imposed, namely there are a limited number of expressible bitfield queries
across all versions of the IFC.

If this becomes a problem in the future, two approaches are noted as promising.

The first (and simplest) being, the removal of the ability to create a bitmask
by joining two queries (bit-or) in favor of two (effectively bit-and)
tests. This would allow a much larger number of bitfield queries to be
represented as the bitfield query would no longer require its own bit position.

The second option being, the usage of a "big integer" type with overloaded
operators in place of a native integer type. As the translation process is
realistically performed at compile time (either via language level guarantees
if the front end moves to C++14 and these functions are made constexpr, or via
the optimizer), while this approach may on the surface seem more expensive, at
runtime it would *likely* be more efficient than the former.

If this problem is encountered, existing usage will guide the resolution (the
formation and usage of bit-or joined queries has thus far been limited, and may
not be useful long term).

Raw Values
----------

:ref:`ifc-raw-numeric-role` and :ref:`ifc-raw-bytes-role` values effectively
aren't versioned.  As raw values have no semantic meaning at the format level,
there's "nothing to be done."

.. _ifc-node-versioning:

Nodes
-----

:ref:`Node<ifc-node-roles>` versioning relies upon a number of "field
accessors" (i.e., ``get_ifc_``\ *X*) that read from the underlying byte
representation (abstracted by :ref:`ifc-byte-buffers`).  These accessors
additionally, abstract away the exact location and representation of various
field values within the node, allowing for complex changes to the node
representation to be "normalized."

These accessor functions look something like the following:

.. code-block:: c++

    template<>
    an_ifc_decl_index get_ifc_decl(
                                  const an_ifc_parameterized_entity &universal)
    /*
    Given the universal representation of ParameterizedEntity, return the
    universal representation of the field "decl".
    */
    {
      an_ifc_decl_index result;

      /* Ensure the decl field exists in the current module version. */
      check_assertion(has_ifc_decl(universal));
      if (is_at_least(universal.get_module(), 0, 41)) {
        /* version specific logic */
      } else {
        /* version specific logic */
      }  /* if */
      return result;
    }  /* get_ifc_decl */

For every version where the retrieval logic is different, we end up with a
separate branch gated on the lower bound of the version range. Newer values are
always checked first, as it's presumed newer IFC versions will be the common
case.

.. _ifc-node-stages:

Stages
^^^^^^

The exact logic in a given branch used to read a field value, is represented in
the generator as a sequence of "stages." These stages are setup via a "stage
builder" for the specific IFC node version. The stage builder allocates a
result variable for every stage (and can also allocate some additional
variables with extra name information if necessary).

In most cases the stages and the associated logic are fairly trivial. As an
example, consider:

.. code-block:: c++

    an_ifc_decl_index_0_41 stage_0;
    an_ifc_decl_index      stage_1;

    /* Copy the field (ParameterizedEntity::decl - DeclIndex) into
       version-specific storage. */
    static_assert(sizeof(stage_0) == 4,
                  "stage_0 is not properly sized storage!");
    copy_ifc_field(&stage_0, universal.get_storage(), /*offset=*/0);
    stage_1 = to_universal_index(universal.get_module(), stage_0);
    result = stage_1;

Here the raw value is copied (via ``copy_ifc_field``) from an offset into a
version specific variable (stage 0). Then, said version specific variable is
converted (via ``to_universal_index``) into a universal representation (stage
1). Finally, the resulting value is assigned to the ``result`` return
variable.

This is a trivial case of data retrieval where the data is directly stored as
part of the node, and only needs converted to a universal representation. For
more information about this process, and additional similar cases, see
:ref:`ifc-field-deserialization`.

Additionally, it's important to note that in some cases, more complicated
processing is similarly implemented via stages (for a variety of reasons). For
more information on this process, and advanced uses of the stage builder, see
:ref:`ifc-tacit-field`.

.. _ifc-field-presence-checking:

Field Presence Checking
^^^^^^^^^^^^^^^^^^^^^^^

The presence of an accessor sometimes depends on the IFC version. In
contexts where this is not obvious ``has_ifc_``\ *X* functions can be used to
check to see if the associated ``get_ifc_``\ *X* is valid. These are fairly
simple calls that return TRUE when field *X* is relevant to the node for
the corresponding IFC version, and FALSE otherwise.

Foreign Index
-------------

:ref:`Foreign Indexes<ifc-foreign-index-role>`, despite conceptually being an
:ref:`ifc-index-sort-role` are versioned more like :ref:`Raw
Numerics<ifc-raw-numeric-role>`.  This is done as foreign indexes have an
associated module which contains its own revision information.  Thus, using the
current (referencing) module's version information to convert the raw value to
a universal understanding would be unreliable at best.

Value Category
--------------

:ref:`Value Categories<ifc-value-category-role>` rely upon the fact that the
types they're composed of are themselves versioned.  Thus, while value
categories are versioned, they do not have any versioning rules specific to
them.

.. _ifc-deserialization:

Deserialization
===============

IFC deserialization is notable for intricacies introduced by supporting
optional memory mapped IFC reading and support for big-endian and little-endian
byte orders.

.. _ifc-byte-buffers:

Byte Buffers
------------

The ``an_ifc_Byte_buffer`` class template is the class that's fundamentally
responsible for abstracting away the location of the in memory representation
of a given byte sequences taken from the IFC file.

The most notable use the byte buffer abstraction being for the representation
of :ref:`ifc-node-roles` but some other IFC roles, such as
:ref:`ifc-raw-bytes-role`, also make use of byte buffers.

The byte buffer fulfills its duty of abstracting away the memory location for a
given byte sequence via a union. Said union contains either the bytes required
for storage, or a storage_ptr; this determination is made during construction
of the respective byte buffer.

.. _ifc-endianness:

Endianness Agnostic Reading
---------------------------

Endianness agnostic reading requires considerations for cases that are
and aren't memory mapped.

Without Memory Mapping
----------------------

When memory mapping isn't enabled, the front end resorts to calls to
``get_bytes`` for each field in each node.

Each call copies the appropriate number of bytes for the field into a
:ref:`Byte Buffer<ifc-byte-buffers>`, reversing the byte order for the bytes
stored in the field if the host and IFC endianness do not match.

For nodes that contain nested (i.e., :ref:`ifc-basic-node-role`) node
structures, the internal node's fields are copied individually rather than
copying the entire internal node as "one field."

With Memory Mapping
-------------------

When memory mapping is enabled, if there is an endianness mismatch, the
non-memory mapped approach described above is used.

Otherwise, the front end will use only a pointer to the start of the node
(within the broader memory mapped memory region). This pointer is then stored
and dereferenced when retrieving data in place of copying bytes into the
:ref:`Byte Buffer<ifc-byte-buffers>`.

There is also support for a-typical cases where the front end, even when
operating with memory mapping and matching endianness, explicitly requests an
explicit copy of the data. In practice, this isn't currently used by the front
end and may be removed in the future.

.. _ifc-field-deserialization:

Field Deserialization
---------------------

Fields are deserialized from the associated :ref:`Byte
Buffer<ifc-byte-buffers>` for the corresponding node.  This process makes use
of one of two ``copy_ifc_field`` field functions, and in some cases an adjusted
:ref:`Byte Buffer<ifc-byte-buffers>` pointer instead of a copy.

Implicit Bounds Copying
-----------------------

Copying with implicit bounds occurs when the destination object type is exactly
equivalent size. These copies additionally include a static assert as a
functional "sanity check" to ensure the code generation tool is right about the
size of the destination object (not just in theory, but in practice).

.. code-block:: c++

  an_ifc_keyword_sort_0_33 stage_0;
  an_ifc_keyword_sort      stage_1;

  /* Copy the field (KeywordSyntax::value - KeywordSort) into version-specific
     storage. */
  static_assert(sizeof(stage_0) == 4,
                "stage_0 is not properly sized storage!");
  copy_ifc_field(&stage_0, universal.get_storage(), /*offset=*/8);


Explicit Bounds Copying
-----------------------

Copying with explicit bounds occurs when the destination object type is a
buffer of at least equivalent size.

.. code-block:: c++

    enum an_ifc_source_location_part : uint8_t {};
    using an_ifc_source_location_storage = an_ifc_source_location_part[8];
    #if USE_MMAP_FOR_MODULES
    using an_ifc_source_location_bytes = const an_ifc_source_location_storage*;
    #else /* !USE_MMAP_FOR_MODULES */
    using an_ifc_source_location_bytes = an_ifc_source_location_storage;
    #endif /* USE_MMAP_FOR_MODULES */

    an_ifc_source_location_bytes stage_0;

    /* Copy the field (KeywordSyntax::locus - SourceLocation) into universal
       storage. */
    copy_ifc_field(&stage_0, universal.get_storage(), /*offset=*/0,
                   /*size=*/8);

Byte Buffer Adjustment
----------------------

:ref:`ifc-byte-buffers` are adjusted in place of a copy when the front end is
operating in memory mapped mode, and the resulting value is a byte buffer based
type. This process allows the front end to avoid unnecessary copies in memory
mapped modes when working with subobjects.

.. code-block:: c++

    enum an_ifc_source_location_part : uint8_t {};
    using an_ifc_source_location_storage = an_ifc_source_location_part[8];
    using an_ifc_source_location_bytes = const an_ifc_source_location_storage*;

    an_ifc_source_location_bytes stage_0;

    /* Update the universal storage pointer to the start of the field
       (KeywordSyntax::locus - SourceLocation). */
    stage_0 = (an_ifc_source_location_bytes)((*universal.get_storage()) + 0);

This optimization is only performed when memory mapped mode is available as
otherwise it creates an unsafe implicit dependence. In said case, the value
retrieved from a node via a field accessor depends on the lifetime of the
owning node's storage. In other words, without memory mapping, it's possible
for the memory that owns the subobject to go out of scope.

.. _field-copying:

Field Copying
-------------

The respective implementation of both overloads of ``copy_ifc_field`` perform a
``memcpy`` operation; this is an intentional choice over casting and
dereferencing the pointer.

This decision is also closely related to the usage of :ref:`ifc-byte-buffers`
over dedicated structs representing each version (which have debugging benefits
over the chosen approach).

In the case of both decisions, the driving force behind this choice is
alignment. In the former case attempting to dereference an misaligned pointer
is undefined behavior.  Thus, attempting to "avoiding the copy" via a
``reinterpret_cast`` paired with a deference could lead to severe portability
issues, and -- ironically -- degraded performance. However, the ``memcpy``
operation can be safely optimized and used, even for unaligned inputs. Thus,
the approach of usage a ``memcpy`` to copy data into known fixed size types
gives maximum performance without the associated portability concerns.

In the case of the latter decision specifically, the individual data members
within a struct will be padded differently on different host compilers,
operating systems, and different architectures (to comply with alignment
requirements). Thus, attempting to "overlay" a struct onto the underlying IFC
byte representation, results in similar severe portability issues.

.. _ifc-tacit-field:

Tacit Field
-----------

A "tacit field" is a field that isn't actually part of the official field list
for a node. Tacit fields are regularly used to expose information that's
(indirectly) always present. For instance, a node might have no "home_scope"
field but the home scope can always be resolved via a different field.

As an example, consider the following made up node "``FooDef``" (which for
the sake of the hypothetical, pairs a declaration with a syntactic
definition):

+-----------------+----------------------+
| Field           |   Type               |
+=================+======================+
| ``declaration`` | ``DeclIndex``        |
+-----------------+----------------------+
| ``definition``  | ``SyntaxIndex``      |
+-----------------+----------------------+

Continuing with the hypothetical, in practice for "``FooDef``" nodes the
``DeclIndex`` for declaration, when valid, *always* points to a node that has a
``home_scope`` field. A tacit field exposes this invariant explicitly. From
the front end's perspective, adding a tacit field for ``home_scope`` is
"as-if" the binary representation defined "``FooDef``" to be:

+-----------------+----------------------+
| Field           | Type                 |
+=================+======================+
| ``declaration`` | ``DeclIndex``        |
+-----------------+----------------------+
| ``definition``  | ``SyntaxIndex``      |
+-----------------+----------------------+
| ``home_scope``  | ``DeclIndex``        |
+-----------------+----------------------+

This also exposes this invariant to the validator, which will then make sure
declaration points to a node with a ``home_scope`` field containing a valid
``DeclIndex``.

Not all tacit fields perform this sort of "proxy retrieval" operation,
effectively *any* invariant can be exposed and validated in this way.

Impact on Versioning
^^^^^^^^^^^^^^^^^^^^

This conceptual breadth of functionality allows tacit fields play a large
role in supporting different IFC versions as well. Consider a later
revision of "``FooDef``" changes ``declaration`` so that the ``DeclIndex``
sometimes points to a new intermediary node "``FooProxyDecl``", which then
itself always points to a third node that has a ``home_scope``.

Tacit fields allow this change to be "invisible" to existing logic. The
existing ``home_scope`` tacit field can be updated to follow the new rules
and properly retrieve the ``home_scope`` as one might expect. However,
tacit fields can also "replace" (or "override") a field that's explicitly
part of the binary node representation (i.e., "explicitly declared") if
they share the same name.

In the context of this example, existing code might break -- badly -- when
it encounters the new intermediary "``FooProxyDecl``" node after a call to
the field accessor ``get_ifc_declaration(const an_ifc_foo_decl&)``. A new
``declaration`` tacit field can be added that automatically "sees through"
the wrapper, "as-if" it was never added. The existing logic then goes back
to functioning as it did before, as from its perspective nothing changed.

Code that then needs to observe the new "``FooProxyDecl``" can then be
accounted for by adding additional tacit fields that either expose the
"true" value of ``declaration`` or information from "``FooProxyDecl``."

The exact approach taken is situational, however, tacit fields are a powerful
tool for resolving versioning issues.

Tacit Field Implementation
^^^^^^^^^^^^^^^^^^^^^^^^^^

As noted in :ref:`ifc-node-versioning`, field accessors work via "stages"
constructed from a "stage builder" in the code generation tool.

These can get *exceptionally* complicated in terms of implemented
code. Consider the following real example, taken verbatim from the front end:

.. code-block:: c++

    template<>
    an_ifc_decl_index get_ifc_home_scope(
                                       const an_ifc_decl_enumerator &universal)
    /*
    Given the universal representation of DeclEnumerator, return the universal
    representation of the field "home_scope".
    */
    {
      an_ifc_decl_index result;

      /* Ensure the home_scope field exists in the current module version. */
      check_assertion(has_ifc_home_scope(universal));
      if (is_at_least(universal.get_module(), 0, 41)) {
        an_ifc_type_index_0_33 stage_0;
        an_ifc_type_index      stage_1;
        an_ifc_type_designated stage_2;
        an_ifc_decl_index_0_41 stage_3;
        an_ifc_decl_index      stage_4;
        a_boolean              stage_5;
        an_ifc_decl_index      stage_6;

        /* Copy the field (DeclEnumerator::type - TypeIndex) into
           version-specific storage. */
        static_assert(sizeof(stage_0) == 4,
                      "stage_0 is not properly sized storage!");
        copy_ifc_field(&stage_0, universal.get_storage(), /*offset=*/12);
        stage_1 = to_universal_index(universal.get_module(), stage_0);
        /* Use the obtained index to retrieve the appropriate instance of
           TypeDesignated (the type tag should have been prechecked by
           validation).  Then, retrieve and return the desired value held by
           the field decl. */
        construct_node_unchecked(&stage_2, stage_1);
        /* Copy the field (TypeDesignated::decl - DeclIndex) into
           version-specific storage. */
        static_assert(sizeof(stage_3) == 4,
                      "stage_3 is not properly sized storage!");
        copy_ifc_field(&stage_3, stage_2.get_storage(), /*offset=*/0);
        stage_4 = to_universal_index(stage_2.get_module(), stage_3);
        if (stage_4.sort == ifc_ds_decl_specialization) {
          stage_5 = TRUE;
        } else {
          stage_5 = FALSE;
        }  /* if */
        if (stage_5) {
          an_ifc_decl_index          stage_6_0;
          an_ifc_decl_specialization stage_6_1;
          an_ifc_decl_index_0_41     stage_6_2;
          an_ifc_decl_index          stage_6_3;

          stage_6_0 = stage_4;
          /* Use the obtained index to retrieve the appropriate instance of
             DeclSpecialization (the type tag should have been prechecked by
             validation).  Then, retrieve and return the desired value held by
             the field decl. */
          construct_node_unchecked(&stage_6_1, stage_6_0);
          /* Copy the field (DeclSpecialization::decl - DeclIndex) into
             version-specific storage. */
          static_assert(sizeof(stage_6_2) == 4,
                        "stage_6_2 is not properly sized storage!");
          copy_ifc_field(&stage_6_2, stage_6_1.get_storage(), /*offset=*/4);
          stage_6_3 = to_universal_index(stage_6_1.get_module(), stage_6_2);
          stage_6 = stage_6_3;
        } else {
          an_ifc_decl_index stage_6_0;

          stage_6_0 = stage_4;
          stage_6 = stage_6_0;
        }  /* if */
        result = stage_6;
      } else {
        an_ifc_type_index_0_33 stage_0;
        an_ifc_type_index      stage_1;
        an_ifc_type_designated stage_2;
        an_ifc_decl_index_0_33 stage_3;
        an_ifc_decl_index      stage_4;

        /* Copy the field (DeclEnumerator::type - TypeIndex) into
           version-specific storage. */
        static_assert(sizeof(stage_0) == 4,
                      "stage_0 is not properly sized storage!");
        copy_ifc_field(&stage_0, universal.get_storage(), /*offset=*/12);
        stage_1 = to_universal_index(universal.get_module(), stage_0);
        /* Use the obtained index to retrieve the appropriate instance of
           TypeDesignated (the type tag should have been prechecked by
           validation).  Then, retrieve and return the desired value held by
           the field decl. */
        construct_node_unchecked(&stage_2, stage_1);
        /* Copy the field (TypeDesignated::decl - DeclIndex) into
           version-specific storage. */
        static_assert(sizeof(stage_3) == 4,
                      "stage_3 is not properly sized storage!");
        copy_ifc_field(&stage_3, stage_2.get_storage(), /*offset=*/0);
        stage_4 = to_universal_index(stage_2.get_module(), stage_3);
        result = stage_4;
      }  /* if */
      return result;
    }  /* get_ifc_home_scope */

Stage Breakdown
~~~~~~~~~~~~~~~

This is an intimidating amount of logic, however it's composed of bite sized
pieces of principled logic coming together. At a high level, there are two
cases here, one for IFC version 0.41+ and one for earlier supported versions of
the IFC.

Consider the " earlier " code (i.e., the code for supporting IFC versions
prior to 0.41, which will be known from this point as the 0.33+ code):

.. code-block:: c++

    an_ifc_type_index_0_33 stage_0;
    an_ifc_type_index      stage_1;
    an_ifc_type_designated stage_2;
    an_ifc_decl_index_0_33 stage_3;
    an_ifc_decl_index      stage_4;

    /* Copy the field (DeclEnumerator::type - TypeIndex) into version-specific
       storage. */
    static_assert(sizeof(stage_0) == 4,
                  "stage_0 is not properly sized storage!");
    copy_ifc_field(&stage_0, universal.get_storage(), /*offset=*/12);
    stage_1 = to_universal_index(universal.get_module(), stage_0);
    /* Use the obtained index to retrieve the appropriate instance of
       TypeDesignated (the type tag should have been prechecked by validation).
       Then, retrieve and return the desired value held by the field decl. */
    construct_node_unchecked(&stage_2, stage_1);
    /* Copy the field (TypeDesignated::decl - DeclIndex) into version-specific
       storage. */
    static_assert(sizeof(stage_3) == 4,
                  "stage_3 is not properly sized storage!");
    copy_ifc_field(&stage_3, stage_2.get_storage(), /*offset=*/0);
    stage_4 = to_universal_index(stage_2.get_module(), stage_3);
    result = stage_4;

The code generation tool represents this as in effect, a directly constructed
expression AST, simplifying a bit:

.. code-block:: python

    IFCRemoteDelegate(
      IFCDirectDelegate('type'),
      'Type',
      'TypeDesignated',
      'decl'
    )

This is conceptually an "expression" representing the retrieval of the
value. The innermost operation ``IFCDirectDelegate`` is thus the first to be
evaluated.

While fully documenting the tacit field system within the code generation tool
is out of scope, for purposes of understanding, an ``IFCDirectDelegate`` simply
means "read the type field" from the node.

Similarly, ``IFCRemoteDelegate`` simply means "take the input value (the
type field value), it's a ``Type(Index)`` that must point to a
``TypeDesignated`` node, read said node's ``decl`` field."

The "stage builder" takes this expression and transforms it into a series of
"stages."

Stage 0
"""""""

Read the versioned value of the type field:

.. code-block:: c++

    an_ifc_type_index_0_33 stage_0;

    /* snip */
    /* Copy the field (DeclEnumerator::type - TypeIndex) into version-specific
       storage. */
    static_assert(sizeof(stage_0) == 4,
                  "stage_0 is not properly sized storage!");
    copy_ifc_field(&stage_0, universal.get_storage(), /*offset=*/12);

Stage 1
"""""""

Turn the versioned type field value into a universal value:

.. code-block:: c++

    an_ifc_type_index_0_33 stage_0;
    an_ifc_type_index      stage_1;

    /* snip */
    stage_1 = to_universal_index(universal.get_module(), stage_0);

Stage 2
"""""""

Load the ``TypeDesignated`` node:

.. code-block:: c++

    an_ifc_type_index      stage_1;
    an_ifc_type_designated stage_2;

    /* Use the obtained index to retrieve the appropriate instance of
       TypeDesignated (the type tag should have been prechecked by validation).
       Then, retrieve and return the desired value held by the field decl. */
    construct_node_unchecked(&stage_2, stage_1);


Stage 3
"""""""

Read the versioned value of the ``decl`` field from the ``TypeDesignated``
node:

.. code-block:: c++

    an_ifc_type_designated stage_2;
    an_ifc_decl_index_0_33 stage_3;

    /* Copy the field (TypeDesignated::decl - DeclIndex) into version-specific
       storage. */
    static_assert(sizeof(stage_3) == 4,
                  "stage_3 is not properly sized storage!");
    copy_ifc_field(&stage_3, stage_2.get_storage(), /*offset=*/0);

Stage 4
"""""""

Turn the versioned ``decl`` field value into a universal value:

.. code-block:: c++

    an_ifc_decl_index_0_33 stage_3;
    an_ifc_decl_index      stage_4;

    stage_4 = to_universal_index(stage_2.get_module(), stage_3);
    result = stage_4;

Stage 5 (Wrap-up)
"""""""""""""""""

Assign the result:

.. code-block:: c++

    an_ifc_decl_index      stage_4;

    result = stage_4;

The stage builder facilitates the creation of these stages from the expression,
creating a metaphorical "data retrieval pipeline." The usage of expression
nodes and this system of generating C++ stages from these nodes, allows tacit
fields to be quickly written via reusable building blocks.

Examination of the validation logic will reveal it's very similar to the logic
generated for field data retrieval, however rather than returning a result, it
performs a number of checks to make sure the would-be result and everything
required in-between is validated as necessary.

Advanced Stage Breakdown
~~~~~~~~~~~~~~~~~~~~~~~~

After establishing an understanding of the flow of the 0.33+ code, consider the
0.41+ code:

.. code-block:: c++

    an_ifc_type_index_0_33 stage_0;
    an_ifc_type_index      stage_1;
    an_ifc_type_designated stage_2;
    an_ifc_decl_index_0_41 stage_3;
    an_ifc_decl_index      stage_4;
    a_boolean              stage_5;
    an_ifc_decl_index      stage_6;

    /* Copy the field (DeclEnumerator::type - TypeIndex) into version-specific
       storage. */
    static_assert(sizeof(stage_0) == 4,
                  "stage_0 is not properly sized storage!");
    copy_ifc_field(&stage_0, universal.get_storage(), /*offset=*/12);
    stage_1 = to_universal_index(universal.get_module(), stage_0);
    /* Use the obtained index to retrieve the appropriate instance of
       TypeDesignated (the type tag should have been prechecked by validation).
       Then, retrieve and return the desired value held by the field decl. */
    construct_node_unchecked(&stage_2, stage_1);
    /* Copy the field (TypeDesignated::decl - DeclIndex) into version-specific
       storage. */
    static_assert(sizeof(stage_3) == 4,
                  "stage_3 is not properly sized storage!");
    copy_ifc_field(&stage_3, stage_2.get_storage(), /*offset=*/0);
    stage_4 = to_universal_index(stage_2.get_module(), stage_3);
    if (stage_4.sort == ifc_ds_decl_specialization) {
      stage_5 = TRUE;
    } else {
      stage_5 = FALSE;
    }  /* if */
    if (stage_5) {
      an_ifc_decl_index          stage_6_0;
      an_ifc_decl_specialization stage_6_1;
      an_ifc_decl_index_0_41     stage_6_2;
      an_ifc_decl_index          stage_6_3;

      stage_6_0 = stage_4;
      /* Use the obtained index to retrieve the appropriate instance of
         DeclSpecialization (the type tag should have been prechecked by
         validation).  Then, retrieve and return the desired value held by the
         field decl. */
      construct_node_unchecked(&stage_6_1, stage_6_0);
      /* Copy the field (DeclSpecialization::decl - DeclIndex) into
         version-specific storage. */
      static_assert(sizeof(stage_6_2) == 4,
                    "stage_6_2 is not properly sized storage!");
      copy_ifc_field(&stage_6_2, stage_6_1.get_storage(), /*offset=*/4);
      stage_6_3 = to_universal_index(stage_6_1.get_module(), stage_6_2);
      stage_6 = stage_6_3;
    } else {
      an_ifc_decl_index stage_6_0;

      stage_6_0 = stage_4;
      stage_6 = stage_6_0;
    }  /* if */
    result = stage_6;

Here stages 0 through 4 are identical to the 0.33+ code, however additional
stages have been added (i.e., the "pipeline" has been extended). Stage 6 is
particularly interesting as it works with substages.

To understand what's going on here, consider the following simplified code from
the code generation tool:

.. code-block:: python

    existing_logic = IFCRemoteDelegate(
      IFCDirectDelegate('type'),
      'Type',
      'TypeDesignated',
      'decl'
    )
    source_value = IFCReusableResult(existing_logic)

    IFCConditionalDelegate(
      IFCTestSortMatch(
        source_value,
        'Decl',
        {'DeclSpecialization'}
      ),
      'DeclIndex',
      IFCRemoteDelegate(
        source_value,
        'Decl',
        'DeclSpecialization',
        'decl'
      ),
      source_value
    )

The ``existing_logic``, the associated ``IFCRemoteDelegate``, and
``IFCDirectDelegate`` were already covered, so those won't be
revisited. However, ``IFCReusableResult``, ``IFCTestSortMatch``, and
``IFCConditionalDelegate`` are new.

``IFCReusableResult`` can be thought of like a lazily-evaluated, "memoization
instruction." This allows reuse of the same value without reevaluation.

``IFCTestSortMatch`` means "take the input value (the possibly memoized
``TypeDesignated decl`` field value), evaluate true if its ``Decl(Sort)``
value is ``DeclSpecialization``; otherwise, evaluate false."

``IFCConditionalDelegate`` means "compute a ``DeclIndex`` value one of two
ways, if the input value (the test sort match) evaluates as true use the
first sub-expression, otherwise use the second."

``IFCConditionalDelegate``\ s are particularly notable as they allow logic
to have branched evaluation. When these branches are translated to C++
code, these are implemented via substages (effectively stage builders that
inherent a prefix and their input from a parent stage builder's stage).

Note how this all composes to allow the existing logic to be sanely extended.
Looking at what's new, stage 5 is where things diverge.

Stage 5
"""""""

The resulting ``DeclIndex`` is check to see if its sort is a
``DeclSpecialization``, storing the result as a boolean value:

.. code-block:: c++

    an_ifc_decl_index      stage_4;
    a_boolean              stage_5;

    if (stage_4.sort == ifc_ds_decl_specialization) {
      stage_5 = TRUE;
    } else {
      stage_5 = FALSE;
    }  /* if */

Stage 6 (Wrap-up)
"""""""""""""""""

The previous result of the ``DeclSpecialization`` sort test is used, forking to
evaluate one of two sub-expressions before the result is assigned:

.. code-block:: c++

    if (stage_5) {
      /* substage content */
    } else {
      /* substage content */
    }  /* if */
    result = stage_6;

The first sub-expression (when a ``DeclSpecialization`` is used) unwraps
the ``decl`` field value from the ``DeclSpecialization`` node, storing the
result in stage 6:

.. code-block:: c++

    an_ifc_decl_index          stage_6_0;
    an_ifc_decl_specialization stage_6_1;
    an_ifc_decl_index_0_41     stage_6_2;
    an_ifc_decl_index          stage_6_3;

    stage_6_0 = stage_4;
    /* Use the obtained index to retrieve the appropriate instance of
       DeclSpecialization (the type tag should have been prechecked by
       validation).  Then, retrieve and return the desired value held by the
       field decl. */
    construct_node_unchecked(&stage_6_1, stage_6_0);
    /* Copy the field (DeclSpecialization::decl - DeclIndex) into
       version-specific storage. */
    static_assert(sizeof(stage_6_2) == 4,
                  "stage_6_2 is not properly sized storage!");
    copy_ifc_field(&stage_6_2, stage_6_1.get_storage(), /*offset=*/4);
    stage_6_3 = to_universal_index(stage_6_1.get_module(), stage_6_2);
    stage_6 = stage_6_3;

The second sub-expression (when a ``DeclSpecializtion`` is not used) passes
through the value as a no-op:

.. code-block:: c++

    an_ifc_decl_index stage_6_0;

    stage_6_0 = stage_4;
    stage_6 = stage_6_0;

Note that even though the second sub-expression ends up being generated as an
effective no-op case with no special processing of its own, a substage
(stage_6_0) is initialized. There is no special meaning to this, it's merely an
implementation detail that falls out from the semantics of the code generation
tool. This may be improved in future revisions, but it's largely a visual
defect that gets cleaned up during optimization.

.. _validation:

Validation
----------

The IFC validation system is designed to check all data read from the IFC file
while being "out of the way", performant, and extendable.

Point of Validation
^^^^^^^^^^^^^^^^^^^

Data is validated upon construction of a node, namely within the
``construct_node`` function (via a call to the ``validate`` function
corresponding to the node). This validation covers all ``get_ifc_``\ *X* return
values (and in the case of :ref:`ifc-tacit-field`, any intermediate data).

The validation system leverages the stage builder (for more information
about stages in general, see :ref:`ifc-node-stages`) system used to create
the ``get_ifc_``\ *X* functions themselves. The stage builder knows how to
create not only the typical "data retrieval pipeline", but one which
validates the intermediate steps, and the returned data.

The extent of the validation depends on the role associated with the type
being validated.

Simple Sort
~~~~~~~~~~~

:ref:`ifc-simple-sort-role` validation ensures that the input value corresponds
to a valid simple sort value for the current IFC version and simple sort type.

Index Sort
~~~~~~~~~~

:ref:`ifc-index-sort-role` validation first checks the associated
:ref:`ifc-simple-sort-role` for validation issues, then ensures that the
requested element exists in the associated partition.  This latter step can be
thought of as a sort of pointer validation, it doesn't ensure the validity of
the data being pointed to, but it does ensure the pointer itself is valid.

Nodes
~~~~~

:ref:`ifc-node-roles` sometimes appear within other nodes (for more
information, see :ref:`ifc-basic-node-role`).  In these cases, the validator
recurses into the sub-node and also ensures the validity of its fields.  This
reduces the number of calls required to work with sub-nodes, and improves
diagnostics (by providing extra context to work with).

Value Categories
~~~~~~~~~~~~~~~~

:ref:`ifc-value-category-role` validation similar to :ref:`ifc-index-sort-role`
validation first checks the associated :ref:`ifc-simple-sort-role` for
validation issues.  The respective variant is then validated as necessary.

Validation Cache
^^^^^^^^^^^^^^^^

The validation system is made more efficient by a bit-based caching mechanism.

Each partition position in the IFC has its validation status represented as two
bits that are part of a 32-bit integer. The higher 16 bits are used to
represent an "invalid" flag while the lower 16 bits are used to represent a
"validated" flag. The choice to represent "invalid" status instead of "valid"
status is deliberate to minimize the number of writes to the validation cache
for a valid IFC file.

These 32-bit integers are themselves part of an array that's allocated
per-partition representing all elements in the partition. The cache thus has
both high locality and high space efficiency.

When a node is constructed via ``construct_node`` the validation cache is
checked, and if the node has already been validated, the cached validation
status will be reused rather than rerunning the ``validate`` function for the
node.

The caching process also provides -- as an adverse affect -- a guarantee that
validation diagnostics are only emitted once.

.. _skipping-validation:

Skipping Validation
^^^^^^^^^^^^^^^^^^^

In general, data validation should not be skipped. However, in some cases it's
desirable to skip validation.

Pre-checked Data
~~~~~~~~~~~~~~~~

``construct_node_prechecked`` can be useful as an optimization in places where
it's known -- with certainty -- that the node being constructed has previously
been validated.

Note that when ``CHECKING`` is enabled this function will check the validation
cache to ensure that the node being constructed truly was previously validated.

Unchecked Data
~~~~~~~~~~~~~~

``construct_node_unchecked`` can be used as part of low level implementation
details that can safely ignored validation status (or code that takes special
care to ensure safe usage). It's important to emphasize, this function truly
bypasses all checks, and thus can introduce unsafe data or crashes if used
improperly.

An example of a time this function is useful is the ``find_trait``
function. Traits are found via a binary search, attempting to validate the data
during the search would significantly degrade performance, and might lead to a
large number of unnecessary diagnostics about data that's never used. The usage
is made safe as after the search completes the final result (if any) is
validated.

.. _ifc-reader-visitors:

Visitors
--------

The visitor system is designed to allow the use of ``get_ifc_``\ *X*
functions on a given :ref:`ifc-index-sort-role` value. As an example,
consider the following:

.. code-block:: c++

    an_ifc_decl_index decl_idx = ...;
    an_ifc_decl_index home_scope_idx = get_ifc_home_scope(decl_idx);

This is not a field accessor as described in the :ref:`ifc-node-roles` section.
Instead, this function will load the appropriate node for the index, and then
delegate to that node's field accessor of the same name.  It's thus in essence,
equivalent to writing:

.. code-block:: c++

    switch (decl_idx.sort) {
      case ifc_ds_decl_function:
        { an_ifc_decl_function universal;

          construct_node_prechecked(&universal, decl_idx);
          home_scope_idx = get_ifc_home_scope(universal);
        }
        break;
    }  /* switch */

Note that this has some limitations, namely, the sort has to point to a node
that has the given field name for the current IFC version, and the node has to
have been previously validated.

To accommodate these limitations additional visitors are exposed for the
corresponding ``has_ifc_``\ *X* function (for more information, see:
:ref:`ifc-field-presence-checking`) and for node validation.

Thus, in most cases the correct usage looks something like the following:

.. code-block:: c++

    an_ifc_decl_index decl_idx = ...;
    an_ifc_decl_index home_scope_idx;

    /* Check if a valid version of the node pointed to by this decl_idx would
       ever have a valid home_scope */
    if (has_ifc_home_scope(decl_idx)) {
      /* Make sure we're working with a valid node */
      if (validate(decl_idx)) {
        /* Guaranteed retrieval of a valid home scope index. */
        home_scope_idx = get_ifc_home_scope(decl_idx);
      }  /* if */
    }  /* if */

This is often just simplified to:

.. code-block:: c++

    an_ifc_decl_index decl_idx = ...;
    an_ifc_decl_index home_scope_idx;

    if (has_ifc_home_scope(decl_idx) && validate(decl_idx)) {
      home_scope_idx = get_ifc_home_scope(decl_idx);
    }  /* if */

Passthrough
^^^^^^^^^^^

In the case of certain nodes, a reasonable answer can be obtained, but is not
implicitly present. For these cases, an "implicit dereference" is added to the
code generation tool. These allow visitors to recurse using the "dereferenced"
value.

As an example, consider the following:

.. code-block:: python

    # Implicit dereferencing on the DeclIndex.
    'Decl': {
      # Implicitly dereference DeclReference via the get_ifc_index function.
      'DeclReference': 'get_ifc_index'
    }

This is an implicit dereference on the Decl :ref:`ifc-index-sort-role`, that
applies the requested traversal to the result of a DeclReference's
``get_ifc_index`` field accessor.

In generated code, this looks something like the following:

.. code-block:: c++

    case ifc_ds_decl_reference:
      { an_ifc_decl_reference universal;

        construct_node_prechecked(&universal, idx);

        an_ifc_decl_index remote_idx = get_ifc_index(universal);
        result = get_ifc_home_scope(remote_idx);
      }
      break;

This notably also applies to the ``has_ifc_``\ *X* function:

.. code-block:: c++

    case ifc_ds_decl_reference:
      { Opt<an_ifc_decl_reference> opt_universal;

        construct_node(&opt_universal, idx);
        if (opt_universal.has_value()) {
          an_ifc_decl_index remote_idx = get_ifc_index(*opt_universal);

          if (has_ifc_home_scope(remote_idx)) {
            result = TRUE;
          }  /* if */
        }  /* if */
      }
      break;

Thus, in contrast to an :ref:`ifc-tacit-field`, a visitor is much more lenient
form of data retrieval, often used both in instances of "may-have" as opposed
to instances of "must-have."

.. _visitor-implicit-conversion:

Implicit Conversion
^^^^^^^^^^^^^^^^^^^

Some visitors are additionally configured with implicit conversions. These
conversions allow the visitor to return one return type while the underlying
``get_ifc_``\ *X* field accessors return different types.

As an example, the IFC's ``TextOffset`` can be represented as a
NameIndex. Thus, to allow for one ``get_ifc_name`` visitor, a conversion is
added.

Limited Scope
^^^^^^^^^^^^^

As most fields don't need exposed by visitors, generating every possible
visitor could get quite expensive (both for the code generation tool and for
build times). Thus, instead, the code generation tool requires desired visitors
to be explicitly listed.

For instance, to define a simple visitor to expose the home_scope DeclIndex:

.. code-block:: python

    # Add a visitor over the Decl index for get_ifc_home_scope returning
    # an_ifc_decl_index with no conversions.
    (
      'Decl',
      'home_scope',
      'an_ifc_decl_index',
      {}
    )

In more advanced cases like the
:ref:`aforementioned<visitor-implicit-conversion>` ``get_ifc_name`` visitor,
conversions are specified like so:

.. code-block:: python

    # Add a visitor over the Decl index for get_ifc_name returning
    # an_ifc_name_index with a potential conversion from a an_ifc_text_offset
    # to an_ifc_name_index.
    (
      'Decl',
      'name',
      'an_ifc_name_index',
      {
        'conversions': {
          'an_ifc_text_offset': IFCTextOffsetToNameIndex()
        }
      }
    )

Partition Kinds
===============

The generation tool manages a list of partition kinds known to said tool (and
thus the front end).

These partition kinds are exposed via the ``an_ifc_partition_kind`` enum with
an additional ``ifc_pk_none`` value written to represent invalid states.

The total number of IFC partition kinds known to the front end (not including
``ifc_pk_none``) can be retrieved via the ``IFC_PARTITION_COUNT`` macro.

A alphabetically sorted mapping of IFC partition kind names to
``an_ifc_partition_kind`` values (not including ``ifc_pk_none``) named
``ifc_partition_map`` is also maintained to enable binary search based mapping
between a given IFC partition kind name and the corresponding front end value
used for its representation.

As this mapping is extensive (with the exception of ``ifc_pk_none``)
``ifc_partition_mapping[partition_kind - 1].name`` can be used to retrieve the
name of any ``an_ifc_partition_kind`` value in constant time.

Additionally, the associated relationships between partition kinds and other
roles is also captured by several utility functions.

``get_ifc_partition_kind``
--------------------------

Some operations require knowing the partition kind associated with a given node
type. The ``get_ifc_partition_kind<T>`` explicit template specializations
expose this information via template instantiation.

``get_ifc_partition_element_size``
----------------------------------

Some operations require knowing the expected element size for a given partition
at a given IFC version. The ``get_ifc_partition_element_size`` function
provides this functionality taking a module and partition kind and returning
the expected size.

``has_partition_kind``
----------------------

Some operations require knowing whether or not a given
:ref:`ifc-simple-sort-role` value has an associated partition kind.  The
``has_partition_kind`` functions take a simple sort and return TRUE if the
sort has an associated partition kind, otherwise they return FALSE.

``to/from_partition_kind``
---------------------------

Some operations require a transition from an :ref:`Index
Sort's<ifc-index-sort-role>` :ref:`ifc-simple-sort-role` to a partition sort or
vice versa.

The ``to_partition_kind`` functions take a :ref:`ifc-simple-sort-role` and
convert the sort to ``an_ifc_partition_kind``.

Conversely, the ``to_X`` sort functions take ``an_ifc_partition_kind`` and
convert it to the desired :ref:`ifc-simple-sort-role`.

Note that in both cases, any conversion failures will result in a crash.

Debugging
=========

Nodes
-----

As noted in the :ref:`field-copying` subsection, to provide a portable
implementation of :ref:`IFC Deserialization<ifc-deserialization>`, sacrifices
were made that negatively impact the debugger experience of working with IFC
:ref:`ifc-node-roles`.

To mitigate this issue, ``DEBUG`` builds of the front end include ``db_node``
functions which can be invoked to get a pretty-printed summary of the node's
contents from within the debugger.

Consider the following:

.. code-block:: c++

    an_ifc_decl_alias foo = ...;
    /* break point */

When debugging this code, the ``db_node(const an_ifc_decl_alias &universal)``
function (in most cases invoked simply as ``db_node(foo)``) can be used to
print the node contents.

This results in output similar to the following:

.. code-block:: none

    =============================== DeclAlias ================================
    access: AccessSort::None
    aliasee:
      sort: TypeSort::TypePointer
      value: 0
    home_scope: NULL
    locus:
      column: 22
      line: 2
    name: 189
    specifiers:
      - C
      - Cxx
      - IsMemberOfGlobalModule
    type:
      sort: TypeSort::TypeFundamental
      value: 0

Code Organization
=================

This system generates a *lot* of code, as a result, to keep the front end build
times down code has been carefully organized and separated to prevent any one
translation unit from having an excessive impact on the build time.

The code generated files currently include:

* ``ifc_map.h`` - A forward declaration of all types so they can be used in
  ``ifc_module.h`` without polluting ``ifc_module.h`` with a large number of
  functions.
* ``ifc_map_functions.h`` - The forward declarations of functions imported in
  files that need access to low level IFC reading operations.
* ``ifc_map_functions.c`` - The implementation of functions declared in
  ``ifc_map_functions.h`` that do not have a more specific implementation file.
* ``ifc_map_functions_acc.c`` - The implementation of all IFC field accessor
  functions.
* ``ifc_map_functions_dbg.c`` - The implementation of all IFC debug functions.
* ``ifc_map_functions_val.c`` - The implementation of all IFC validation
  functions.
* ``ifc_modules_inst.h`` - A list of macros imported in ``ifc_modules_templ.c``
  to manage explicit template instantiations.
* ``ifc_modules_spec.h`` - A list of macros imported in ``ifc_modules_templ.c``
  to manage explicit template specializations.

Note that :ref:`Visitor<ifc-reader-visitors>` implementations are currently
found in ``ifc_map_functions.c``.

The portion of the front end that handles deserialization is primarily found in
``ifc_modules_read.c``.  The portion of the front end that handles
serialization is primarily found in ``ifc_modules_write.c``.  Code that is
common between the two is found in ``ifc_modules_internal.h`` and
``ifc_modules.c``.

All supporting template definitions and instantiations (that used by a mix of
``ifc_modules.c``, ``ifc_modules_read.c``, `ifc_modules_write.c``, and the
``ifc_map_functions*.c`` files) are isolated to ``ifc_modules_templ.c``,
preventing multiple translation units from instantiating the same template.
