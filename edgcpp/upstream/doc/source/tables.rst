================
Table Management
================




For a description of the intermediate language itself, see
:ref:`intermediate-language`.

The source file ``il_alloc.c`` contains routines to allocate and clear the
intermediate language tables, and ``il.c`` contains routines to debug-print
them.  These routines should always be used, so that new tables are always set
to the proper default values and problems with undefined fields are avoided.
``il_def.h``, ``il_alloc.h``, and ``il.h`` contain associated declarations.

Note that some of these routines should not be called from a back end, because
the front end data structures they need are no longer set up properly at that
point.  In general, routines that allocate new storage or new entries should
not be called, and routines that answer questions about existing entries are
okay to call.  The

.. code:: c++

   #if !STANDALONE_UTILITY_PROGRAM

guards indicate routines that should not be called by back ends.

Storage Allocation
------------------

``alloc_cil`` has an interface like ``malloc``; it allocates space in the
current intermediate language memory region.  ``new_il_region`` begins a new
intermediate language memory region.  ``switch_il_region`` switches to a
previously-created region.  ``alloc_il`` allocates space in the file-scope
intermediate language memory region.

When the alternate IL file format is used, space is reserved preceding each IL
entry so that the IL entry number can be stored there.

Constant Entries
----------------

Constant entries (type ``a_constant``) are usually allocated by
``alloc_constant``; ``fs_constant`` is similar to it but allocates the constant
entry at the file scope.  ``make_zero_of_proper_type`` makes a zero constant of
a given type.  ``clear_constant`` and ``set_constant_kind`` are called to
re-initialize existing constant entries, and ``set_error_constant`` changes an
existing constant entry to an error constant.

In most cases, a constant entry need not be unique -- the same entry can be
used for all occurrences of a given literal constant.  For these cases, one
should call ``alloc_shareable_constant`` instead of ``alloc_constant``.  (The
interface is slightly different; one passes a filled-in constant entry rather
than just a kind.) ``alloc_shareable_constant`` maintains
``shareable_constants_table``, a hash table that is searched by hashing a
constant entry to a bucket number (routine ``hash_constant``), then searching a
linked list of constants in that bucket to see if there is a match (comparison
is done by ``eq_constants``).  If an entry is found, it is reused.  If none is
found, one is entered into the table.

In cases where the constant entry cannot be shared (as when it must be linked
into an initializer list), ``alloc_unshared_constant`` should be called
instead.  It basically just calls ``alloc_constant``, but adjusts a few things
in the constant entry.

Type Entries
------------

Type entries (type ``a_type``) are allocated by ``alloc_type``.

``integer_type``, ``fixed_point_type``, ``float_type``, ``string_type``,
``void_type``, and ``error_type`` can be called to create type entries for the
most common types; these routines will reuse the entries once they have created
them.  ``make_pointer_type`` creates a pointer type pointing to a given base
type.  It uses the routine ``get_based_type`` to determine if a pointer to the
type has already been created, and it calls ``add_based_type_list_member`` to
record the fact that a pointer type has been allocated.
``make_reference_type`` is the similar routine for (lvalue) reference types,
and ``make_rvalue_reference_type`` is the similar routine for rvalue reference
types.  ``ptr_to_member_type`` is the similar routine for pointer-to-member
types.

``make_qualified_type`` is used to add type qualifiers to an existing type;
``make_unqualified_type`` is used to remove them.

``unknown_type`` creates an unknown type entry; such an entry is used as a
place-holder within the front end and the references to it must be eliminated
before the back end is called.  Another type that is normally not seen by the
back end (unless prototype instantiations are recorded in the IL) is the
template parameter type, which is used in processing template declarations.

There are several routines that allocate various pieces of the description of a
class type:

* | ``alloc_derivation_step``
* | ``alloc_base_class_derivation``
* | ``alloc_overriding_virtual_function``
* | ``alloc_base_class``
* | ``alloc_list_entry_for_class``
* | ``alloc_class_type_supplement``

Other Declarative Entries
-------------------------

``alloc_variable``, ``alloc_field``, ``alloc_label``, and ``alloc_routine``
allocate variable, field, label, and routine entries.  They do nothing terribly
interesting.  ``remove_from_routines_list`` can be used to remove a routine
from the routines list so that it can be added again at the position where its
actual definition appears.

Routines that allocate other IL entities resulting from declarations include:

* | ``alloc_param_type`` allocates a param-entry pointed to from the routine
    type supplement of a ``tk_routine`` type entry.
* | ``alloc_template``, ``alloc_template_arg`` and
    ``alloc_template_param_type_descr`` are used in template processing.
* | ``alloc_namespace`` allocates an entry to represent either a namespace or a
    namespace alias.
* | ``alloc_using_decl`` allocates an entry to represent a ``using``-directive,
    a member ``using``-declaration, or a nonmember ``using``-declaration.
* | ``alloc_macro`` allocates an entry representing a macro definition (when
    ``RECORD_MACROS_IN_IL`` is set).
* | ``alloc_pragma`` allocates an entry representing a ``#pragma`` declaration.
* | ``alloc_asm_entry`` allocates an entry representing an ``asm`` declaration.
    To support certain extensions (i.e., when ``ASM_FUNCTION_ALLOWED`` is set)
    ``alloc_asm_function_body`` is also available.
* | ``alloc_scope`` allocates a scope entry.
* | ``alloc_ctor_init`` allocates a constructor-initializer entry, a list of
    which is recorded in the scope entry associated with a constructor.
* | ``alloc_temporary_variable`` allocates a variable entry for a temporary.
* | ``alloc_dynamic_init`` allocates a dynamic initialization entry, used
    (among other things) to represent dynamic initialization on variable
    declarations.
* | ``alloc_local_static_variable_init`` allocates an entry used to represent
    the initialization of a function-local variable with static storage class.
* | ``alloc_list_entry_for_routine`` allocates an entry used to record a list
    of routine entries (used to support ``friend`` declarations).
* | ``alloc_exception_specification`` allocates an entry that represents an
    exception specification in a function declaration.
* | ``alloc_accessible_base_class`` allocates an accessible base class entry
    (unused in implementations in which ``ABI_CHANGES_FOR_RTTI`` is set).
* | ``alloc_hidden_name`` allocates a hidden-name entry (for use by the
    C++-generating back end, when ``RECORD_HIDDEN_NAMES_IN_IL`` is set).

Expressions
-----------

``alloc_expr_node`` allocates an expression node.

``set_expr_node_kind`` and ``clear_expr_node`` re-initialize existing entries.
``make_operator_node`` creates an expression node for an operation.

``set_node_operator`` changes the operator or type in an existing operation
expression node.

``error_node`` makes an error expression node (a new one on each call, since
executable constructs cannot be shared).

There are several utility routines that help in constructing expressions:

* | ``alloc_node_for_constant`` builds an expression node that points to a
    constant entry.
* | ``node_for_integer_constant`` builds an expression node for an integer
    constant.
* | ``var_lvalue_expr`` builds an expression node for a variable as an lvalue.
* | ``var_rvalue_expr`` builds an expression node for a variable as an rvalue.
* | ``var_addr_expr`` builds an expression node for the address of a variable.
    The ``address_taken`` flag is set.
* | ``function_lvalue_expr`` builds an expression node for a function as an
    lvalue.
* | ``function_rvalue_node`` builds an expression node for a function as an
    rvalue, i.e., for the address of the function.  This is suitable for
    calling the function, but not for the "``&``" operator applied to the
    function, because the ``address_taken`` flag is not set.
* | ``function_addr_expr`` builds an expression node for the address of a
    function.  The ``address_taken`` flag is set.
* | ``rvalue_expr_for_lvalue`` converts an lvalue expression to an rvalue.  It
    also works for function lvalues, but not for arrays.
* | ``add_indirection_to_node`` adds an indirection ("``*``" operator) to an
    expression.
* | ``add_ref_indirection_to_node`` adds the reference equivalent of
    "``*``" to an expression.
* | ``add_address_of_to_node`` adds a "``&``" operator to an expression.
* | ``add_reference_to_to_node`` adds the reference equivalent of "``&``" to an
    expression.  (Note that that is not a typo; the name really does have
    "``to_to_``" in it.)
* | ``field_lvalue_selection_expr`` builds an expression for a field selection
    that produces an lvalue.
* | ``field_rvalue_selection_expr`` builds an expression for a field selection
    that produces an rvalue.

``copy_expr_tree`` makes a copy of an expression tree.  When it does so, it can
optionally "clone" any temporary variables found within the tree so that the
copy uses different but equivalent temporaries.

In addition, ``alloc_lowered_eh_construct_node`` allocates an expression node
to represent one of several (partially) lowered exception handling constructs
(used only when IL lowering is enabled and ``DO_FULL_PORTABLE_EH_LOWERING`` is
configured to FALSE.

Statements
----------

``alloc_statement`` allocates a statement (which may allocate a supplement of
type ``a_for_loop``, ``a_switch_stmt_descr``, etc.).
``alloc_switch_case_entry`` allocates the entry used to represent one case
(possibly, the "``default:``" case) of a ``switch`` statement.

Object Lifetimes
----------------

``alloc_object_lifetime`` allocates an object lifetime entry, and
``bind_object_lifetime`` binds it to a specified IL entry (a scope, an
expression, a dynamic init, etc.).

A dynamic init entry with a destructor is added to the destructions list of an
object lifetime by ``record_end_of_lifetime_destruction``, which calls
``add_to_destructions_list``.  Conversely, it is removed from the list by
``remove_from_destructions_list``.

During front-end processing object lifetime entries are pushed onto the object
lifetime stack (see ``push_object_lifetime`` and global variable
``curr_object_lifetime``, which is the top of the stack) and popped off the
stack when appropriate (see ``pop_object_lifetime``).

If, when it is popped off the object lifetime stack, the entry is not actually
required in the IL (see ``is_useless_object_lifetime``), it is unbound from the
IL entry with which it is associated (see ``unbind_object_lifetime``) and
placed on an available list for reuse (see ``free_object_lifetime``).

Source Sequence Entries
-----------------------

Source-sequence entries are created only when
``GENERATE_SOURCE_SOURCE_SEQUENCE_LISTS`` is configured to TRUE.  See
:ref:`intermediate-language` for additional information.

``f_update_source_sequence_list`` creates new source sequence entries and adds
them to the appropriate list.

* | For declarations it is called by ``sym_update_source_sequence_list`` (in
    ``symbol_ref.c``), which also calls ``alloc_src_seq_secondary_decl`` when
    the declaration is not a primary declaration.
* | For statements it is called by ``stmt_update_source_sequence_list`` (in
    ``statements.c``).
* | In certain other cases -- e.g., for the declaration of unnamed ``enum``
    types, for which there is no symbol -- it is called via macros
    ``update_source_sequence_list`` and ``add_to_source_sequence_list``.

Other functions called to create and manage source sequence lists include:

* | ``alloc_source_sequence_entry`` allocates and initializes a source-sequence
    entry.
* | ``add_source_sequence_entry_to_list`` is called to add entries to the end
    of the list of the current scope stack entry (see ``source_sequence_list``
    in ``a_scope_stack_entry``).  When ``pop_scope`` is called, the list for a
    given scope stack entry is either appended to the list of the enclosing
    scope stack entry or, for function scopes and the file scope, moved to the
    associated IL scope (see ``source_sequence_list`` in ``a_scope).``
* | ``add_empty_source_sequence_entry`` creates and adds a source sequence
    entry that does not yet point to an IL entry.  Such entries are
    placeholders to reserve a position in the list for a declarator when the
    particular IL entry it represents is not yet known; they do not persist
    beyond the front end.
* | ``add_end_of_construct_source_sequence_entry`` creates a source sequence
    entry to mark the end of a construct for a given entity (see
    ``alloc_src_seq_end_of_construct)`` and adds it to the current source
    sequence list.  (See also :ref:`src-seq-lists`.)
* | ``make_source_sequence_secondary_decl`` creates a source sequence entry to
    represent a non-defining declaration (see ``a_src_seq_secondary_decl``),
    and ``set_src_seq_secondary_decl_fields`` can be called to locate the entry
    on the current source sequence list (see
    ``last_matching_source_sequence_entry``) and to update various of its
    pointers and flags.
* | When either ``CLASS``- or
    ``NONCLASS_TEMPLATE_INSTANTIATIONS_IN_SOURCE_SEQUENCE_LISTS`` is TRUE,
    ``add_source_sequence_entry_for_partial_instantiation`` is called to enter
    a secondary declaration entry to represent the declaration of a
    compiler-generated template specialization that stands for a template
    instantiation, and when the body of such a specialization is generated, its
    source sequence list is added by ``insert_instantiation_src_seq_list``; in
    both cases, ``find_instantiation_insert_scope`` is called to locate the
    point at which the instantiation should be inserted in the source sequence
    list (since it is not necessarily at the end of the list for the current
    scope stack entry).
* | ``insert_src_seq_list`` inserts a list into another list at a specified
    point; it is also called to append a list to another list.
* | ``remove_from_source_sequence_list`` removes a given source sequence entry
    from its list; see also macros ``unlink_src_seq_entries`` and
    ``unlink_src_seq_entry`` (which operate on source sequence lists and single
    entries, respectively).
* | ``fixup_function_scope_source_sequence_list`` creates both a sublist header
    (see ``alloc_src_seq_sublist``) and the source sequence entry that points
    to it.

Lists of Entries
----------------

The routines

* | ``add_to_constants_list``,
* | ``add_to_types_list``,
* | ``add_to_variables_list``,
* | ``add_to_parameters_list``,
* | ``add_to_dynamic_inits_list``,
* | ``add_to_routines_list``, and
* | ``add_to_labels_list``
* | ``add_to_namespaces_list``
* | ``add_to_using_decls_list``
* | ``add_to_asm_entries_list``
* | ``add_to_templates_list``
* | ``add_to_macros_list``
* | ``add_to_pragma_list``

add entries of the indicated type to end of the list of similar entries under a
scope entry.  The scope will vary depending on the type of entry and what the
caller specifies.  In many cases it is either the current scope or the
innermost namespace scope that is used (e.g., variables, constants, types).
Routines that are not class members are added to the list for the file scope or
a namespace scope.  Labels are always added to an ``sck_function`` scope.  And
so forth.

There is also an ``add_to_scopes_list``; it is called by ``create_block_scope``
when a scope for a statement block is created.  Special handling is required
because the IL scope for a block is not created until there is some declaration
in that block.

Orphaned Entries
----------------

Orphans are entries in the file scope memory region that are referenced only
from function scope memory regions.  When the parents of such entries are
written out and then removed from memory, the entries are orphaned because they
are not attached to the rest of the file-scope IL tree.

The orphan mechanism makes lists of such entries so that they can be found
during traversal of the file-scope IL.  In the file scope, each entry is
preceded by space for an orphan-list pointer.  When a potential orphan entry is
encountered, for example when an entry in a function scope memory region points
to an entry in the file scope memory region,
``add_orphaned_file_scope_il_entry`` is called.  It adds the potential orphan
to a list of entries of its type headed by the proper element of the array
``orphaned_file_scope_il_entries`` and linked by the hidden pointer.  An entry
is ignored if it is already on the list (which can be discerned from the fact
that the orphan-list pointer is already non-NULL or the fact that the entry
address matches the last-on-list pointer for that entry type).

There is also a special mechanism for keeping track of the lists of local
static variables and types in function and block scopes.  These are lists that
are entirely in the file scope memory region but originate with a pointer that
is in a function scope memory region.  To keep track of these, entries of type
``a_scope_orphaned_list_header`` are allocated in the file scope memory region
and attached to the ``il_header``.  Each entry contains a copy of the static
variables and types pointers from a function or block scope, so that the lists
can be visited in the file scope memory region even if the associated function
scope memory region is no longer in memory.  See
``add_scope_orphaned_il_lists``.

Source File Information
-----------------------

``record_start_of_source_file`` and ``record_end_of_source_file`` build
information about the correspondence between source files and sequence numbers.
Basically, this is a tree that shows the nesting of include files, with added
entries to show the information provided by ``#line`` directives.  These
routines are called from ``push_input_stack`` and ``pop_input_stack`` in
``lexical.c``, and ``proc_line`` in ``preproc.c``.
``conv_seq_to_file_and_line`` uses that data structure to map sequence numbers
into file names and line numbers.

Debug Print Routines
--------------------

The following debug-print routines can be used to display intermediate language
constructs for debugging purposes:

* | ``db_name``
* | ``db_type_name``
* | ``db_name_linkage``
* | ``db_type``
* | ``db_abbreviated_type``
* | ``db_access_control``
* | ``db_field``
* | ``db_static_data_member``
* | ``db_member_function``
* | ``db_virtual_function_info``
* | ``db_constant``
* | ``db_variable``
* | ``db_expression``
* | ``db_dynamic_initializer``
* | ``db_initializer``
* | ``db_statement``
* | ``db_statement_list``
* | ``db_object_lifetime``
* | ``db_object_lifetime_stack``
* | ``db_scope``
* | ``db_source_sequence_entry``

Types
=====

The routines and data structures described in this section are defined in
``types.c`` and ``types.h``.  These are routines that manipulate intermediate
language types in various ways.  They answer questions about them, and do
various language transformations on them.

Typerefs
--------

In type trees, typedefs, type qualifiers (e.g., const and volatile), type
operators such as decltype, and various other types are represented by an entry
called a typeref.  It indicates a reference to an existing type.  For qualified
pointer types, the ``typeref`` indicating the qualifier is on top of the
pointer type: for ``int *const q`` the type is a ``const``\ ``typeref``
pointing to a pointer type entry, which points to an entry for ``int``.  Macros
``get_type_qualifiers`` and ``get_top_level_type_qualifiers`` (the former may
check for qualifiers on the underlying array element type, the latter does not)
call ``f_get_type_qualifiers`` when appropriate, returning a bit set (see
``a_type_qualifier_set``).

It is often necessary for code that deals with types to access the target of a
typeref or chain of typerefs.  Various convenience functions are provided for
this purpose, including skip_typerefs, skip_typerefs_not_typedefs,
skip_lexical_typerefs, etc.

Asking Questions About Types
----------------------------

There are many predicates that determine whether or not a given type is of a
given kind.  Having predicate functions (instead of doing the testing by direct
coding) is necessary because of the possibility of there being ``typeref``
entries on a type.  These predicates are implemented as functions, although
they could be implemented as macros for any cases where it seems desirable.
Within ``types.c``, macros are used so as to avoid having one predicate routine
call a half-dozen others to get an answer.  The predicate macros have the same
names as the corresponding predicate functions, with the final ``_type`` of the
name removed (i.e., ``is_integral`` corresponds to ``is_integral_type``).  They
do not call ``skip_typerefs``.

The predicate functions are

* | ``is_error_type``
* | ``is_function_type``
* | ``is_incomplete_type``
* | ``is_object_type``
* | ``is_void_type``
* | ``is_integral_type``
* | ``is_signed_integral_type``
* | ``is_enum_typeis_integral_or_enum_type``
* | ``is_character_type``
* | ``is_fixed_point_type``
* | ``is_floating_type``
* | ``is_arithmetic_or_enum_type``
* | ``is_pointer_type``
* | ``is_reference_type``
* | ``is_lvalue_reference_type``
* | ``is_rvalue_reference_type``
* | ``is_ptr_or_ref_type``
* | ``is_scalar_type``
* | ``is_array_type``
* | ``is_char_array_type``
* | ``is_class_struct_union_type``
* | ``is_complete_class_struct_union_type``
* | ``is_union_type``
* | ``is_aggregate_or_union_type``
* | ``is_polymorphic_class_type``
* | ``is_ptr_to_member_type``
* | ``is_template_class_type``
* | ``is_template_param_type``
* | ``is_nullptr_type``

Macros that may be called to get information about the type (when
``skip_typerefs`` is not to be called) include

* | ``is_immediate_error_type``
* | ``is_immediate_class_type``
* | ``is_qualified_type``
* | ``is_const_qualified_type``
* | ``is_volatile_qualified_type``
* | ``is_top_level_qualified_type``
* | ``is_top_level_const_qualified_type``
* | ``is_top_level_volatile_qualified_type``

(Among those that check type qualifiers, the ``is_top_level_``\ *xxx* versions
differ from the others in not checking for qualifiers on underlying array
element types.) Other type qualifiers besides ``const`` and ``volatile`` may be
recognized; for instance, when support for ``restrict`` is enabled,
``is_restrict_qualified_type`` and ``is_top_level_restrict_qualified_type`` are
also defined.

``is_illegal_abstract_class_type`` returns TRUE if the type is that of an
abstract class, struct, or union, or if it is an array of abstract class
objects, or if it is pointer or reference to an array of abstract class
objects.

``int_kind_is_signed`` determines whether a given integer kind is a signed
type.

There are in addition some predicate functions that examine an entire type
tree, if necessary, to return information about a type:

* | ``is_or_contains_local_type``
* | ``is_or_contains_unnamed_or_local_type``
* | ``is_or_contains_template_param``
* | ``is_or_contains_specific_template_param``

These routines call ``traverse_type_tree``, a generic type-walking routine; it
is passed the address of a service routine that examines a node in the type
tree, and its detailed behavior is governed by setting some combination of
flags (see ``a_type_tree_traversal_flag_set``).

Another set of predicates tests C++/CLI types and can be used only inside
``#if`` guards for ``MICROSOFT_EXTENSIONS_ALLOWED:``

* | ``is_handle_type``
* | ``is_managed_nullptr_type``
* | ``is_ref_class_type``
* | ``is_value_class_type``
* | ``is_managed_class_type``
* | ``is_standard_class_type``
* | ``is_cli_interface_type``
* | ``is_tracking_reference_type``
* | ``is_handle_or_tracking_ref_type``
* | ``is_interior_ptr_type``
* | ``is_pin_ptr_type``
* | ``is_cli_array_type``
* | ``is_handle_to_cli_array_type``
* | ``is_cli_generic_param_type``
* | ``is_cli_generic_constraint_type``
* | ``is_boxable_type``
* | ``is_delegate_type``
* | ``is_cli_system_object_type``
* | ``is_cli_system_string_type``

To ease pointer/reference/handle tests in code that might have to deal with
C++/CLI types, the following test certain general categories of pointers and
references:

* | ``is_any_reference_type``
* | ``is_any_ptr_or_ref_type``
* | ``is_pointer_or_handle_type``
* | ``types_are_both_pointers_or_both_handles``

Taking Apart Types
------------------

Given an array type, ``array_element_type`` and
``underlying_array_element_type`` return the type of an element; the second
deals with multidimensional arrays.  ``type_pointed_to`` is used to get the
base type from a pointer or reference type.  ``pm_member_type`` and
``pm_class_type`` extract the base types from pointer-to-member types.
``underlying_type_of_derived_type`` extracts the underlying type for all
derived types (pointer, pointer-to-member, array, etc.).

Given a C++/CLI array type, ``cli_array_element_type`` returns the type of the
element.  ``cli_array_rank_constant`` and ``cli_array_rank`` return the rank
(number of dimensions) of the array in different forms.

Setting Sizes and Alignments of Types
-------------------------------------

The size and alignment for a type are recorded in the type entry (except for
``typeref``\ s, which have their sizes and alignments dictated by the type they
refer to).  These values, however, are not set all over the front end, but only
by isolated routines.  The main routine is ``set_type_size``, which will set
the size of an arbitrary type.  The type must be completely built by the time
this routine is called.  ``set_type_size`` calls ``set_array_type_size`` for
arrays.  Size and alignment for class types are set by code in ``layout.c``.

Modifying Type Trees
--------------------

``set_used_in_exception_or_rtti_flag`` is called for types that appear as the
type of a throw object, in an exception specification list, as the type of a
handler, or in a ``typeid`` operator.  It sets the type entry's
``used_in_exception_or_rtti`` flag.  To assure that such types have external
linkage, it also calls ``set_force_external_linkage_flag``, which in turn calls
``traverse_type_tree`` to mark all the components of the type tree that have
linkage.

``strip_local_typedefs`` is called to remove all local ``typedef`` components
of a type tree.  It calls ``traverse_and_modify_type_tree`` to help accomplish
this task.  If a subtree of the type tree is modified, a new type tree is built
to accommodate the change.

Getting Information About Base Classes
--------------------------------------

``find_base_class_of`` returns a pointer to a base class entry if a given class
type is represented on the base classes list of another class type.  Similar
functionality, but with the implied restrictions, is provided by
``find_direct_base_class_of`` and ``find_direct_or_virtual_base_class_of``.
``related_class_pointers`` will do the same for pointers to classes.

``corresponding_base_class``, given a base class belonging to one class,
searches the base classes of another class, finds the corresponding entry, and
returns a pointer to it.

``is_on_any_derivation_of`` returns TRUE if a given base class is on the
derivation of another base class.

Routines that Implement Language Concepts
-----------------------------------------

The routines described in this section implement concepts of the C or C++
languages.

Two routines handle type promotions: ``type_after_integral_promotion``
determines the type that results from integral promotion done in an expression,
and ``default_argument_promotion`` determines the type of an argument to a
function with old-style parameter declarations after default promotion.  See
also ``type_after_bit_field_integral_promotion`` in ``exprutil.c`` for a
special case involving bit-fields.

Several routines compare two types to see if they are compatible in some way.
``identical_types`` determines whether the two types are structurally identical
(they don't have to be the same pointer).  ``il_identical_types`` sees if the
two types are identical from an IL perspective (e.g., ``int`` and ``long``
might be identical if the front end is configured that way).
``types_are_compatible`` sees if two types are compatible (C standard,
3.1.2.6).  (Those three names are actually macros which call
``f_identical_types`` and ``f_types_are_compatible`` only if the type pointers
are not exactly the same.) ``interchangeable_types`` sees if two types are
interchangeable as arguments to functions (basically, this is true if they are
compatible except for signedness issues).

``impl_pointer_conversion`` determines whether or not an implicit conversion
from a given type to a given pointer type is valid, with some special cases for
pointers to ``void`` and for null pointer constants.

``impl_conversion_possible`` determines whether or not an implicit conversion
from a given type to another is valid.  ``impl_ptr_to_member_conversion``
handles the subcase of implicit conversion of one pointer-to-member type to
another.

``expl_conversion_possible`` determines whether or not an explicit conversion
(cast) from a given type to another is valid.  It relies on two subroutines for
the major subcases:

* | ``static_cast_conversion_possible`` and
* | ``reinterpret_cast_conversion_possible``.

``composite_type`` develops a composite type from two types (C standard,
3.1.2.6).

``overload_distinguishable`` determines whether or not two function types are
sufficiently different that they can be distinguished by overload processing
(and therefore routines with those types can be overloaded).

``impl_handle_conversion`` determines whether or not an implicit conversion
from a given type to a given C++/CLI handle type is valid.

``boxing_conversion_possible`` determines whether a conversion from one given
type to another is a C++/CLI boxing conversion.  The cases accepted convert a
value type to a handle to that value type.

``unboxing_conversion_possible`` determines whether a conversion from one given
type to another is a C++/CLI unboxing conversion.  The cases accepted convert a
handle type to a value type.

``system_type_from_fundamental_type`` and ``fundamental_type_from_system_type``
are a pair of functions that convert back and forth between fundamental types
and the corresponding C++/CLI value class types (e.g., between ``int`` and
``System::Int32``).
