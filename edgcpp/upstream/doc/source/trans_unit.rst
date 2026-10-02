.. _multi-tu:

==========================
Multiple Translation Units
==========================

A *translation unit* is a set of source files translated as a unit, comprising
a primary source file and all of the source files included by that file.
Sometimes, the front end has to deal with more than one translation unit at a
time:

* | When exported templates are used, the source file in which a template is
    defined may be different than the one in which is it used.  The front end
    has to read in a translation unit for the defining source file while
    retaining the translation unit for the referencing source file, and then
    perform the instantiation in a merged context that includes both
    translation units.  Additional translation units may be read in for
    instantiations of other templates.
* | When ``COMPILE_MULTIPLE_TRANSLATION_UNITS`` is TRUE, multiple files can be
    specified on the command line for the front end.  Each file is read in turn
    as the primary file of a translation unit.

In both these cases, the first translation unit read is called the *primary
translation unit*, and any translation units read after the first are called
*secondary translation units*.  The translation units are kept separate within
the front end.  They have separate intermediate language (IL) trees and
conceptually separate symbol tables.  After all translation units have been
processed, the IL for each secondary translation unit is merged into the IL for
the primary translation unit.  Duplicate copies of entities that appear in more
than one translation unit are eliminated during the merge, and errors are
issued for conflicts between entities that have incompatible definitions in
different translation units.  The final merged primary IL tree is then passed
to a back end, looking much like an IL tree from a simple one-translation-unit
compilation.

The process of translating a primary translation unit and zero or more
secondary translation units into an IL tree is called a *compilation*.

Note that when ``COMPILE_MULTIPLE_SOURCE_FILES`` [#f1]_ is TRUE and the front
end is given several source file names on the command line, each translation
unit is treated as a separate compilation.  It is read separately and its IL
tree is written to a separate IL file.  No merging is done on the translation
unit IL trees, and no checking for matching between translation units.  There
are no secondary translation units in this mode (except those that might be
brought in for exported templates).

The principal source files of the front end involved in
multiple-translation-unit processing are ``trans_unit.c``, ``trans_unit.h``,
``trans_corresp.c``, ``trans_corresp.h``, ``trans_copy.c``, and
``trans_copy.h``.

.. _multi-tu-dispatch:

Initialization, Termination, and Keeping Translation Units Separate
===================================================================

By and large, the code in the front end is written as if there is only one
translation unit.  Global variables and tables indicate the state at the
current point in the input, which is a particular point within a single
translation unit.  For the handling of multiple translation units, this global
state must be carefully managed, and updated when one switches between
translation units.

The process of initialization in the front end is broken down into three parts:

* | Initialization that is done only once in the entire front end invocation.
    Initialization routines in this class have names that have the suffix
    "``_one_time_init``".
* | Initialization that is redone for each compilation.  Initialization
    routines in this class have names that have the suffix "``_init``".
* | Initialization that is redone for each translation unit.  Initialization
    routines in this class have names that have the suffix
    "``_trans_unit_init``".

It can be seen that in ``COMPILE_MULTIPLE_TRANSLATION_UNITS`` mode, the front
end does one-time initialization, compilation initialization, and then, for
each source file specified on the command line, translation unit
initialization.  In ``COMPILE_MULTIPLE_SOURCE_FILES`` mode, on the other hand,
the front end does one-time initialization and then, for each source file
specified on the command line, compilation initialization and then
translation-unit initialization.  See the top-level routines in ``fe_init.c``.

Similarly, termination is done in several parts:

* | ``pop_scope`` for the file scope does necessary processing at the end of
    the file scope for a translation unit.
* | ``translation_unit_wrapup`` does necessary processing at the end of a
    translation unit.
* | ``fe_wrapup`` does necessary processing at the end of a compilation.  This
    includes calling ``template_and_inline_function_wrapup``, which does most
    template instantiation.
* | ``wrap_up_file_scopes`` finishes up each translation unit and merges the
    IL.

The instantiation process (during the third step above) can bring in additional
secondary translation units for exported templates, which go through their own
initialization, reading/parsing, and termination phases.  Eventually, all
necessary templates have been instantiated, which means all necessary
translation units have been created and processed, and ``wrap_up_file_scopes``
is called.

The source code for a secondary translation unit is read, and the corresponding
IL is generated, all at one time.  There is no switching between translation
units while the source code is being read.  Once a secondary translation unit
has been read, additional processing may be done in that translation unit that
will add more IL to it, but that is done through template instantiations, and
never by going back and reading more source code.  Likewise, instantiations can
add additional IL to the primary translation unit, but no additional source
code will be read into it.

A structure of type ``a_translation_unit`` contains information about a single
translation unit within a compilation, and the global variable
``curr_translation_unit`` points to the entry for the translation unit that is
currently active.  By calling ``switch_translation_unit``, one can switch to a
different translation unit, i.e., make another translation unit the "active"
one.  This is a relatively expensive process that changes the values of many
global variables to match the new translation unit.

How does ``switch_translation_unit`` know what to update? It updates the
variables recorded by calling ``register_trans_unit_variable`` during one-time
initialization.  Variables registered in that way are known as *trans unit
variables*, and their per-translation-unit values are recorded in a data
structure associated with the ``a_translation_unit`` structure: They are saved
when the translation unit becomes inactive, and restored when the translation
unit is reactivated.  Certain values that need to be accessed efficiently even
when the corresponding translation unit is not the active one are stored
directly in the ``a_translation_unit`` structure (e.g.,
``file_scope_pointers_block``), or are pointed to by fields in that structure
(e.g., ``module_id_ptr``; variables of this kind are registered via
``register_trans_unit_variable_with_field``).

``process_translation_unit`` is the driver for the reading/parsing of a single
translation unit.

Some data structures and numbering sequences are deliberately shared across
multiple translation units.  The source file data structure and source line
sequence numbers, for example, are shared, so that a source position consisting
of a sequence number and column number is unique across the entire compilation.
Likewise, the symbol table and scope number sequence is shared, which ensures
that scopes always have unique numbers, and therefore that one can do a lookup
in the symbol table by scope number and find only entities from a single scope
from a single translation unit.

Memory Regions and Intermediate Language
========================================

Each secondary translation unit has its own set of IL memory regions, one for
its file scope and one for each top-level function definition.  The global
variable ``file_scope_region_number`` indicates the region number of the file
scope of the current translation unit, whether it is a primary or secondary
translation unit.

The function ``alloc_primary_file_scope_il`` allocates space in the primary
translation unit IL even if the current translation unit is a secondary
translation unit.

The macro ``is_secondary_trans_unit`` returns true for an IL entry allocated in
a memory region that is part of a secondary translation unit.  The global array
``trans_unit_for_scope`` maps a scope number back to a pointer to the
translation unit that contains that scope.  Note that when IL is moved from
secondary translation units to the primary IL at the end of the compilation,
the value of ``is_secondary_trans_unit`` will change, but that of
``trans_unit_for_scope`` stays as it was.

The front end memory region is shared across all translation units.

Translation Unit Correspondences
================================

Each IL entry with external linkage is assigned to a correspondence set
containing all of the related IL entries from different translation units,
i.e., all the entries that represent the same entity.  So, for example, all
external global variables called "``i``" will be in one correspondence set.

A correspondence set is represented by an entry of type
``a_trans_unit_corresp``, which is allocated in front end memory (so it is not
really part of the IL).  Each entry that is a member of the correspondence set
points to the appropriate ``a_trans_unit_corresp`` entry via the
``trans_unit_corresp`` pointer in its ``source_corresp`` field.  These pointers
are set when secondary translation units are processed, and therefore in a
compilation consisting only of a single translation unit the pointers will be
NULL and the correspondence-set entries will not be allocated.  [#f2]_ For
entries without linkage, or with internal linkage, the ``trans_unit_corresp``
pointer is always NULL.

IL entries of type ``a_base_class`` are also matched up in correspondence sets
and therefore contain a ``trans_unit_corresp`` pointer (though it is not part
of a ``source_corresp`` field in that case).

.. _canonical-entries:

Canonical Entries
=================

In each correspondence set, one entry is chosen as the *canonical* entry, and
the ``canonical`` field in the ``a_trans_unit_corresp`` points to it.  The
entry chosen as the canonical entry is an instance that contains the most
detailed information available about the entity: one with a definition if a
definition is available, and a specialization if both specialized and
unspecialized versions of a template instance are available.  All other things
being e qual, an entry in the primary IL is chosen over one in a secondary
translation unit, because that means less copying later.

The canonical entry chosen can change as the compilation progresses.  The first
instance seen will generally be chosen as the canonical entry, but it may be
replaced afterwards by a better choice from some other secondary translation
unit scanned later.

Note that it is possible for a class or namespace not to be the canonical entry
in its correspondence set while at the same time some of its members are the
canonical entries in their sets.

The macro ``canonical_il_entry_of`` can be used to fetch the address of the
canonical entry corresponding to a given entry.  For an entry without linkage,
the entry itself is returned.

The Copy Address Pointer
========================

All IL entries have a prefix that contains various flags and numbers.  IL
entries in the file scope of a secondary translation unit have, in addition, a
copy address pointer, which is set during the copy process to indicate the
address in the primary IL to which the entry should be copied or remapped.

For entries in correspondence sets, generally the canonical entry will be
copied to the primary IL and its copy address pointer will be set to the
address of the copy.  The other members of the correspondence set will not be
copied, but their copy address pointers will be set to the same copy address so
that references to them can be remapped to the unique copy in the primary IL.

For entries not in correspondence sets, including non-declarative entries like
statement and expression nodes, no copy address is pre-assigned.  When the
entry is encountered during the copy process, it is copied to the primary IL
and the copy address pointer is set to the address of the copy.  All references
to the entry encountered thereafter are remapped to the established copy
address.

In some cases, entries in the secondary IL are "merged" into the primary IL.
This is necessary, for example, if the canonical entry for a correspondence set
is in a secondary translation unit but there is also a member of that
correspondence set in the primary IL.  The address in the primary IL must be
preserved (because there are references to it from elsewhere in the primary IL,
which will not be rewritten), so the canonical entity must be copied on top of
the existing primary IL entry.  In such cases, the copy address pointer is used
to form a two-element list: The copy address pointer of the entry in the
secondary translation unit points to allocated space for a copy (also in the
secondary translation unit IL), and the copy address pointer of that copy entry
points in turn to the address of the existing entry in the primary IL.  The
copy process copies the original entry to the intermediate copy, with pointer
remapping, and then later overwrites the primary IL entry, carefully preserving
certain information (e.g., next-on-list pointers).

The macro ``trans_unit_copy_address_of`` gives access to the correspondence
pointer of an IL entry (for both fetching and setting).  The macro
``checked_trans_unit_copy_address_of`` does the same thing, but also checks
that the entry pointer provided is actually in the file scope of a secondary
translation unit.

Correspondence Checking
=======================

The code in ``trans_corresp.c`` establishes correspondences between entities in
a secondary translation unit and similar entities in the primary translation
unit and in other secondary translation units.  It sets the correspondence
pointer of IL entries to indicate the correspondences found.  While
establishing these correspondences, it checks that linked entities are in fact
compatible, and issues errors when they are not.  The entities that have
correspondences set in this way are types, routines, variables, fields,
namespaces, base classes (i.e., ``a_base_class`` entries), using-declarations
appearing in class scopes, and templates (including template instances).

Correspondence checking can occur in three stages of the compilation:

* | Built-in types have their correspondences set by ``record_builtin_type`` as
    soon as they are created.
* | When a translation unit has been processed,
    ``set_trans_unit_correspondences`` is called from
    ``translation_unit_wrapup`` to establish correspondences for the entities
    that were created in that translation unit.  The global variable
    ``correspondence_checking_underway`` is TRUE during this stage (and only
    during this stage); after that ``correspondence_checking_done`` is TRUE.
* | The correspondence checking process is notified of every template
    instantiation by calls to ``record_instantiation``.  When this occurs in a
    translation unit after its ``translation_unit_wrapup`` call, the
    correspondence pointer is established for the new instantiation.  The
    instantiation process also notifies the correspondence checking process of
    template instantiations being completed, through calls to
    ``establish_class_instantiation_corresp``,
    ``establish_function_instantiation_corresp``,
    ``establish_variable_instantiation_corresp``,
    ``establish_block_extern_function_correspondence``,
    ``establish_block_extern_variable_correspondence`` and
    ``establish_friend_type_correspondence``.

Finding Corresponding Pairs
---------------------------

Several different mechanisms are used to find an entity in another translation
unit that corresponds to a given entity:

* | For named entities in namespace scope, the symbol table is used.  The
    search process simply traverses the list of symbols attached to the symbol
    header of the entity for which a correspondence is being searched.  This is
    done in functions with the prefix ``find_`` (e.g.,
    ``find_type_correspondence``).
* | For class members, we can rely on the fact that the lists of IL entities
    representing the members should have their elements correspond on a
    one-to-one basis.  Hence no search is needed, although care must be taken
    with compiler-generated members that may not appear in every translation
    unit.  This principle is implemented by
    ``establish_trans_unit_correspondences_for_class``.  A similar process can
    be used for enumeration constants.
* | Correspondences for instantiations of templates are usually established
    after the templates from which they are generated have been matched up.  To
    support the search for a matching instantiation, a field
    ``all_instantiations`` was added to ``a_template_symbol_supplement``.  This
    field is set only in the supplement for the canonical template entry and
    points to a list of all the instantiation symbols for the template,
    including those for corresponding templates in other translation units.
    Some of the key functions manipulating this list are ``add_instantiation``,
    ``find_class_template_instantiation`` and
    ``find_function_template_instantiation``.
* | Correspondences are also sometimes established between unnamed types.  This
    happens when matching up declarations of entities with linkage involving
    unnamed types.  For example:

  .. code:: c++

     struct { float a, b; } *p, q; // Could appear in two translation units

  | In such cases, the correspondence is first established for the entities
    with linkage (variables or routines), and the correspondence between the
    types is sought by comparing the two types using ``f_types_are_compatible``
    with the option ``TCF_SEEK_CORRESP``.

Ordering Dependencies
---------------------

Determining whether two entities from different translation units correspond
may require that other correspondences are known.  For example, suppose we have
two namespace scope functions ``f`` that are declared as

.. code:: c++

   extern R N::f();

then the two must correspond if the entities denoted by ``N`` correspond, and
if that is the case the entities denoted by ``R`` must correspond too.  The
ordering of corresponding checks must therefore be managed carefully.

To reduce the magnitude of this issue, correspondences are determined in two
steps.  In the first step, correspondences are *established*: Only those
components of the entity that uniquely identify it are compared (e.g., the
entity's name, its enclosing classes and namespaces, parameter types of
functions, etc.) and if they match the correspondence pointer is set.  Some
diagnostics may be issued at this time.  This process is mostly done in
functions with the prefix ``establish_``.  In the second step, which starts
after all the correspondences for the translation unit have been established,
the correspondences are *verified*.  This involves comparing the remaining
relevant attributes of entities that were found to correspond after the first
step; diagnostics are issued if the entities do not match.  Most of the
verification work is done in functions with the prefix ``verify_``.  This
separation into two steps makes sure that in our example above the
correspondence for ``R`` has been established by the time it is needed.

To deal with other ordering issues, such as the requirement that the
correspondence of ``N`` be established in our example, the correspondence
checking process may be stacked.  For example, while searching for a
correspondence for ``N::f`` above, another search for a correspondence for
``N`` may be initiated.  This is driven by calls to functions named
``canonical_``\ *entity*\ ``_entry_of`` where *entity* is one of
``namespace``, ``type``, ``field``, ``routine``, ``variable`` or ``template``.
If a correspondence was already established, these functions return the
requested canonical entry; if not, they first determine the correspondence by
calling ``determine_correspondence`` [#f3]_.

Other ordering issues sometimes arise because establishing a correspondence for
a template instantiation seemingly leads to infinite recursion.  Such
situations are resolved in part by placing appropriate markers in the
``all_instantiations`` list and in part by as much as possible establishing the
correspondences of all templates before attempting to match their
instantiations.  To that end, the front end maintains a list of instantiations
whose correspondences are yet to be determined (the list is pointed to by
``instantiations_to_process``).  That list is processed at appropriate times
(i.e., when the correspondences of the various templates have been found) by
calls to ``process_pending_instantiations``.

Finally, we should note that the verification process always compares an entity
with its canonical counterpart.  If a canonical entry (from an already
processed translation unit) is replaced by a new entry (from the current
translation unit) the former will need to be compared with the latter when all
the correspondences have been established in the current translation unit.
This is done in ``process_verification_list``.

Debugging Facilities
--------------------

When the preprocessor symbol ``DEBUG`` is TRUE,a few facilities are provided by
``trans_corresp.c`` to help identify problems during correspondence checking.
First, the command-line option "``-d-trans_corresp``" will cause a line of
output to be produced for each correspondence pointer being set (or cleared),
as well as for certain operations on the ``all_instantiations`` lists.  Second,
the function ``db_corresp`` can be called from a debugger to examine the
correspondence of a given entity.  Finally, the modification of a
correspondence pointer for a specific entity can easily be intercepted in a
debug environment where addresses are reproducible from run to run.  To enable
this, set breakpoints on ``main`` and on ``corresp_intercept``.  Rerun the
front end and at the stop in ``main``, set the static variable
``trace_corresp_ptr`` to the address of the entity whose correspondence value
should be tracked: ``corresp_intercept`` will be called (triggering the
breakpoint) when that value is modified.

.. _multi-tu-copy:

Copying from Secondary Translation Units
========================================

The code in ``trans_copy.c`` copies IL from a secondary translation unit to the
primary translation unit IL.  It is run after elimination of unneeded entries
in the secondary translation unit, so it copies only entries that are actually
"needed." It also runs after correspondence checking has established the
correspondences between externally-linked entities, so it does not copy entries
for which there is already an instance in the primary translation unit IL; for
such cases, all references to the entity are simply rewritten to point to the
primary IL instance.

The secondary translation unit IL consists of a file scope memory region and
zero or more function scope memory regions.  The file scope memory region is
actually copied into the primary IL, via a tree walk, with duplicates
eliminated as indicated above.  The function scope memory regions, on the other
hand, are not copied.  Instead, they are walked to update pointers
appropriately, and then the memory region numbers are simply reassigned to the
primary IL.

The copy process is handled by ``copy_secondary_trans_unit_IL_to_primary``,
which has five main phases:

* | ``prepare_for_trans_unit_copy`` walks through the declaration lists of the
    file scope of a secondary translation unit and its subscopes, and puts each
    entry into one of four categories: (a) entries that should be copied to the
    primary IL; (b) entries that should overwrite a corresponding entry in the
    primary IL (e.g., a function in the secondary translation unit that has a
    definition, where the corresponding function in the primary IL has only a
    declaration); (c) entries that are in a correspondence set but are not the
    canonical entry, which should be removed from the lists and discarded; and
    (d) entries in a correspondence set that provide additional (minor)
    information over what is present in the canonical entry, which should be
    merged into the corresponding entry.
* | ``copy_from_secondary_to_primary_IL`` uses the IL-walking routines to walk
    the IL tree, copy over entries that need to be copied, and update pointers
    so that the copied IL refers only to copies or to pre-existing entries in
    the primary IL.
* | ``copy_function_bodies_to_primary_IL`` processes the bodies of any
    functions that are to be moved over, by using the IL-walking routines to
    walk the IL tree and update pointers appropriately.
* | ``finish_trans_unit_copy`` walks through the declaration lists again, and
    links copied entries into the declaration lists of the primary IL.  A
    struct type copied over, for example, would be linked into the types list
    of the primary scope of the primary IL.  Variables, types, and routines
    that must be merged into the corresponding entries are also copied onto the
    corresponding entries at this point.  This part of the processing runs
    while switched into the primary translation unit.  See also
    ``finish_moved_entity_processing``, which is similar but reaches all copied
    or merged entities, even those from non-merged scopes.
* | ``finish_processing_for_function_bodies`` calls
    ``finish_function_body_processing`` for each copied function scope memory
    region, and kicks off IL lowering for the function if it's needed.  This
    part of the processing runs while switched into the primary translation
    unit.

For entries that are being merged into the primary IL, the copy process sets
the ``il_lowering`` flag in the IL entry prefix to indicate merging rather than
copying.  (The flag is available for this use because IL lowering is never done
on secondary translation units.) For merged entries, the copy process uses an
extra copy of the entity allocated in the secondary translation unit: The
entity's copy address pointer points to the copy, whose copy address pointer
points to the corresponding entity in the primary IL.  During the copy, the
original entry is copied to the copy space, and its pointers are remapped
there.  This allows ``finish_trans_unit_copy`` later to have access to both the
original entity in the primary IL and the entity from the secondary translation
unit as updated for the copy.  This is often important because some flags from
the two entries need to be merged.

The bodies of functions not copied over are eliminated, and likewise for the
initializers of variables.  See ``clear_body_for_routine`` and
``clear_variable_definition``.  Non-template functions and variables in
secondary translation units included only to get exported templates are turned
into external declarations, because the definition of those entities is put out
when the file is compiled as a primary translation unit.

The IL for secondary translation units can contain references to other
translation units.  In some ways, the IL trees for all secondary translation
units have to be viewed as one big somewhat interconnected tree.  For this
reason, each of the processing phases described above is done for all secondary
translation units before advancing to the next phase.

Part of the function of the copy process is to produce a primary IL tree that
contains no references to IL entries in secondary translation units, and this
is done by making sure that every entity copied over has only pointers that
refer to the primary IL.  Sometimes, however, the instantiation of a template
that is defined in the primary translation unit introduces exactly the kind of
mixed-translation-unit IL that we are trying to avoid.  To eliminate such
references, a pair of routines in ``trans_copy.c`` is called:

* | ``mark_secondary_trans_unit_IL_entities_used_from_primary_as_needed``
    sweeps the primary translation unit IL and looks for references to
    secondary translation unit entries.  When it finds them, it marks the
    secondary translation unit entries as needed so that they will be copied to
    the primary IL when the secondary translation unit is copied.
* | ``rewrite_secondary_trans_unit_IL_entity_pointers_used_in_primary`` sweeps
    the primary translation unit IL after the secondary translation units have
    been copied in, and looks for references to secondary translation unit
    entries.  When it finds them, it rewrites them either as references to the
    corresponding entity in the primary translation unit (if there is one) or
    it copies them and rewrites the pointer to the copy.

In some (relatively rare) cases, the merged IL produced by the copy process has
some type entries out of order on the type lists, where "out of order" is
defined according to the peculiar ordering required by the C-generating back
end so that it can generate compilable C code.
``fix_type_list_ordering_problems`` is called after IL lowering to examine the
file-scope types list to look for ordering problems and to move types to fix
those problems.  If there are no ordering problems (which is almost always the
case), the checking is done fairly quickly and efficiently.

Externalization of statics
==========================

When a source file includes exported templates, it is possible that the
exported templates will refer to static entities in the file.  Because the
template instances may be generated as part of the translation of other source
files, and be included in the object code for those files, it may be necessary
to refer to the static entities from another compilation.  Therefore, the front
end externalizes all statics when a compilation includes exported templates.
This gives the static functions and variables external linkage and special
mangled names.  This has a number of consequences, such as that static inline
functions become extern inline functions, which in some configurations makes
them instantiatable.  See ``externalize_statics_for_exported_templates``.

One-instantiation-per-object mode
=================================

In one-instantiation-per-object mode, each instantiated template is put into a
separate "slice" of the IL, so that a back end can put out each instantiation
in a separate object file.  The slices are defined by having a separate
"needed" flag for each slice (there's an array of them attached to each IL
entity), and setting the slice-specific needed flags so that each slice
contains only the types, variables, etc.  referenced by the template
instantiation in that slice.

When there are multiple translation units, the per-instantiation needed flags
are not maintained in the secondary translation unit IL.  Once the IL is copied
to the primary IL, the flags are built up and maintained appropriately there.

.. [#f1] Not ``COMPILE_MULTIPLE_TRANSLATION_UNITS``.
.. [#f2] There are some circumstances involving templates where the
         correspondence pointer is set even if there is only a primary
         translation unit.  The important point is simply that one shouldn't
         count on the pointer being set when there is only one translation
         unit.
.. [#f3] Note that this is different from ``canonical_il_entry_of`` which does
         not attempt to establish a correspondence.
