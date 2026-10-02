=======================
C++-Generating Back End
=======================

The C++-generating back end is a "back end" that turns C or C++ intermediate
language back into source text in C or C++.  It is designed for
source-to-source-transformation applications, i.e., processors that read in
source code, make some modifications to its meaning (e.g., adding profiling
code), and then write it out as modified.

The C++-generating back end handles C++, ANSI/ISO C, and K&R C [#f1]_.  The
output is in the same language as the input, and means the same thing, but
is not expected to be identical.  At the very least, comments are
eliminated and preprocessing is done, which means ``#include`` files are
merged in, macros are expanded, and ``#if``\ s and the like are processed.
The output *will* match the input exactly with regard to entity names and
source line numbers (the latter by use of ``#line`` directives, if
necessary), and in the order of declarations and statements in the source.
In short, it should be possible to compile the output file with a C or C++
compiler, execute it to get the same results as the original program, and
debug it using the original source text.

The source code for the C++-generating back end is in ``cp_gen_be.c``;
``cp_gen_be.h`` contains the associated declarations.

Source Sequence Lists
=====================

The C++-generating back end requires the source sequence list, which is a
construct that lists all declarations and statements of the source program in
the order of their appearance.  It is optional, and must be enabled by the
configuration flag ``GENERATE_SOURCE_SEQUENCE_LISTS``.  The source sequence
list stands alongside the "normal" tree-structured intermediate language, as a
sort of table of contents for the IL tree.  That is, for a program fragment
that declares variable ``i``, function ``f``, and variable ``j``, the source
sequence list would be a list of three entries, pointing to the IL entries for
``i``, ``f``, and ``j``, and the tree-structured information under those
entries would provide full information on the entities.

The list is conceptually a single list that runs through the whole compilation
unit, though in actuality it is implemented by a somewhat more complicated data
structure that snakes between the file scope memory region and the function
scope memory regions.  That is, however, merely the low-level representation
dictated by the restrictions on pointers between memory regions, and is hidden
by access functions, e.g., ``adv_to_signif_source_sequence_entry``.

Some entities in the source program may be declared or defined more than once.
For example, a function can be declared and then later defined.  For such
cases, one of the occurrences (the definition if there is one) is selected as
the primary declaration, and the source sequence entry for that declaration
points directly at the IL entry for the entity.  For all the other
declarations, the source sequence entry on the list points to a source sequence
secondary declaration entry, which points to the IL entry for the entity and
also gives information about that particular declaration, e.g., the source
position and the specific type used (which may differ from that of the
definition by use of typedefs, array bounds, etc.).

The C++-generating back end operates by traversing the list and generating code
for each construct it encounters.  A program is considered to be made up of an
alternation of declarative regions and executable regions.

In declarative regions, the C++-generating back end steps through the
entries on the source sequence list, each of which points to a declared
entity, and generates declarations.  Source sequence entries for class,
struct, union, and enum types that are declared as part of some other
declaration (e.g., "``struct A {int i;} x;``"), called *non-*\ autonomous
declarations, are skipped over without generating a declaration for the
type, but the type is marked to be put out when encountered while putting
out the type of the containing entity (e.g., "``x``" above).  All other
declarative source sequence entries are processed immediately.

In executable regions, the C++-generating back end steps through the source
sequence entries (a flat list) and the IL statement entries (a tree) in tandem,
and generates code for the statements.

Declarations
============

The top-level routine for declaration processing is ``gen_declaration``.  It
calls routines like

* | ``gen_variable_decl``,
* | ``gen_routine_decl``,
* | ``gen_namespace``,
* | ``gen_type_decl``,
* | ``gen_template``,
* | ``gen_using_directive_or_declaration``, and
* | ``gen_instantiation_directive``.

``gen_type_decl`` breaks the various type-declaration cases down as calls to

* | ``gen_class_definition``,
* | ``gen_enum_definition``, or
* | ``gen_typedef_decl``.

``gen_declaration_using_type`` and ``gen_type`` output types in general, by
calling routines in ``il_to_str.c``.

``gen_secondary_decl`` handles secondary declaration source sequence entries.

``gen_initializer`` handles initializers on variables.  It calls
``gen_dynamic_init`` to handle dynamic initialization.

``gen_class_definition`` uses normal declaration routines like
``gen_routine_decl`` to put out code for the class members, and also the
following, which handle special kinds of members:

* | ``gen_field_decl``,
* | ``gen_member_constant_decl``, and
* | ``gen_using_declaration.``

``gen_type_decl``, ``gen_variable_decl`` and ``gen_routine_decl`` include
special code to deal with declarations within class definitions.  In
particular, they deal with friend declarations.

Executable Code
===============

The top-level routine for executable code processing is ``gen_statement``.
Most statements are straightforward, but the following routines handle the more
complicated cases:

* | ``gen_block_statement``,
* | ``gen_try_block_statement``,
* | ``gen_switch_statement``, and
* | ``gen_for_statement``.

Expressions are handled by ``gen_expr``, with some more complicated cases
handled by:

* | ``gen_new_delete`` and
* | ``gen_temp_init``

The expression routines always add parentheses around expressions if there is
any chance of precedence confusion.

Constants
=========

Constants are put out by ``gen_constant``, which calls routines in
``il_to_str.c`` to do the actual output.

``gen_initializer_constant`` handles initializer constants, which include
aggregate constants, ``ck_dynamic_init`` constants, and cases where a reference
is initialized from a constant, as well as the usual kinds of constants.

Templates and Macros
====================

Templates and macros are strange cases in the IL.  Both are macro-like, in
that the source provides a prototype that is expanded by the front end on
each use of the construct.  That means that template and macro references
are largely invisible in the IL -- one sees only the expanded form and
little evidence that it came from a template or macro reference.

It is nevertheless desirable to be able to put out the template and macro
definitions in some way so that template instantiation and symbolic debugging
can be done on the generated code.  To accommodate this need, the IL optionally
contains entries for templates and macros.  These contain normalized textual
representations of the template or macro (e.g., a string like "``#define x(a)
a+1``").  The C++-generating back just puts those out verbatim.  (The macro
entries are all put out at the end of the output file, so they won't affect the
already-macro-expanded code that is put out, but they will still be available
for symbolic debuggers.) Note that macro entries are also used for ``#undef``
directives.  See ``gen_template`` and ``gen_macro``.

Source Output
=============

The effective position for source output is set by calling
``set_output_position``.  That will result in generation of ``#line``
directives as necessary, but not until some text is actually output, at which
point ``adjust_output_position`` is called.  Note that while line number
correspondence is maintained in the output, there is no attempt to maintain
column number correspondence, or even to do appropriate indentation.

Actual text output is done through

* | ``write_ch``,
* | ``m_write_ch``,
* | ``write_space``,
* | ``m_write_space``,
* | ``write_tok_ch``,
* | ``m_write_tok_ch``,
* | ``write_tok_str``,
* | ``m_write_tok_str``,
* | ``write_unsigned_num``, and
* | ``write_num``,

The names that begin with "``m_``" are macros and are intended for those cases
where speed is important.  The others are functions.  The names that include
"``_tok_``" are used to write complete tokens or sequences of tokens; the
low-level routines are given permission to break long output lines before or
after such text to avoid overly-long lines.

Troublesome Cases
=================

Names
-----

Names are, in general, put out exactly as they appear in the IL.  In some
cases, unnamed entities are given generated names.  Members of classes are in
general put out as qualified names (e.g., ``A::x``), but sometimes special care
is required:

* | The declaration of a member within its class must not use a qualified name.
    To handle this, a name context stack is maintained (see
    ``push_name_context`` and ``pop_name_context``), tracking the current
    scope.  When immediately within a class definition, the class qualifier for
    that class is suppressed.  The same thing is done for namespaces.
* | A call of a virtual function with a qualified name suppresses the
    virtual-ness of the function, so a qualified name must be used only if that
    behavior is desired.
* | In references to nonstatic members of classes (data or function), the
    choice of name can have an effect on access control (e.g., for protected
    members) and can disambiguate an otherwise ambiguous member name.
    ``gen_simple_field_selection`` and ``gen_bound_function`` handle such
    references for data and function cases, and call
    ``optimized_expr_for_selection`` to determine the class to be used in
    naming the member (that's done by examining any implicit casts used in the
    member selection).
* | Sometimes, a prefix "``::``" or an elaborated type specifier (e.g., ``class
    X``) must be used to distinguish one name from another.  One would think
    that always generating those would be the appropriate (and easy) thing to
    do.  It turns out, however, that various compilers have bugs in that area,
    and won't accept some valid code generated in that mechanical way.  As a
    consequence, the C++-generating back end uses the hidden-name information
    developed by the front end to generate prefix "``::``", qualified names in
    general, and elaborated type specifiers only when they are required.  See
    ``alloc_hidden_name_fixup`` et al.  This processing has an additional
    benefit: it handles external variables and functions that are declared
    ``extern`` within a function.  References to such entities must not use a
    "``::``" prefix, because although the entity named is external, and thus in
    some sense in the global scope, the name used for it is declared local to
    the function.

See

* | ``gen_name``,
* | ``gen_unqualified_name``,
* | ``gen_decl_name``,
* | ``gen_class_qualifier``, and
* | ``gen_namespace_qualifier``.

References
----------

Reference types, or rather uses of entities with such types, require some
special attention.  In expressions, ``eok_reference_to`` and
``eok_ref_indirect`` occur in order to make explicit the indirection implicit
in the uses of the reference types; these operators are simply ignored when
generating the expressions.

More subtle is the fact that certain kinds of constants implicitly contain an
"address-of" operation.  The ordinary processing of these by ``form_constant``
adds an ``&`` to the output before writing the constant's value; this ``&``
must be suppressed when the constant has a reference type (or, more generally,
is used in an lvalue context).  This special processing is provided by
``handle_lvalue_constant_node``.

Prototype Scopes
----------------

Prototype scopes (scopes associated with a prototyped function declarator, in
C) cause a particular set of headaches, because while such types cannot be
named outside of the prototype scope, they do nevertheless show up in the types
of objects outside those scopes.  For example:

.. code:: c++

   void f(struct A {int i;} *p);
   void g(enum E {e1});
   main() {
     f(0);   /* Really f((struct A *)0) */
     g(1);   /* Really g((enum E)1) */
     return 0;
   }

The C++-generating back end deals with these by noting such cases and
suppressing the explicit casts in those cases.  The conversion is then once
again implicit in the output program.

See ``is_implicitly_cast_integral_constant``.

Rewritten Property References
-----------------------------

References to Microsoft properties are rewritten by the front end.  Something
like "``p+1``", referring to a property ``p``, might have been rewritten as
"call the get accessor function for ``p``, add ``1``\ to it, and store the
resulting value by calling the set accessor for ``p``." The C++-generating back
end recovers the original source form.  This is based on a field in expression
nodes called ``rewritten_property_reference_kind``, which is set by the front
end to special values to identify certain key nodes in the expansion.  See
``gen_prop_event_or_op_synth_call``.  Event references are similarly
reconstituted.

.. [#f1] It cannot be used, however, to generate C from the output of IL
         lowering.
