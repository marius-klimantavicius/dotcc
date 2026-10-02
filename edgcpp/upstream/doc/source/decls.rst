============
Declarations
============

Declaration scanning is shared among a number of files.  What is specific to
class member declarations is found in ``class_decl.c`` and is discussed in the
next chapter.  The remainder of declaration processing is found in ``decls.c``,
which contains the top level routines and a number of utility functions;
``decl_spec.c``, which handles declaration specifiers; ``declarator.c``, which
scans declarators; ``func_def.c``, which contains the code to process function
definitions; and ``decl_inits.c``, which contains the code to scan
initializers.  ``decls.h``, ``decl_spec.h``, ``declarator.h``, ``func_def.h``,
and ``decl_inits.h`` contain associated declarations.  In addition, code for
disambiguating C++ declarations and expressions is found in ``disambig.c``,
with associated declarations in ``disambig.h``.

Overview
========

The top level routine in declaration scanning, and in fact in the overall
compilation process, is ``translation_unit``, which scans a series of
declarations and stops at end of file.  The routine it calls is
``scan_nonmember_declaration`` (via the older ``declaration`` interface), which
is also called for local declarations that appear within functions or blocks
(including old-style parameter declarations).  (The only declarations that do
not pass through ``declaration`` are class member declarations and new-style
parameter declarations.)

Since a typical declaration looks like this:

  | *decl-specifiers* *declarator-list* ``;``

two principal elements in the structure of ``scan_nonmember_declaration`` are

* | a call to ``decl_specifiers``, which scans the type specifier, storage
    class keyword, and so forth; and
* | a loop to handle a comma-separated list of declarators with successive
    calls to ``declarator``.

There are many details, including a lot of error checking and some special
cases.  But in general, after the return from ``declarator``, one of several
routines is called (though not called directly from ``declaration``) to do
additional processing:

* | When a function body is present ``function_definition`` is called and the
    loop is exited.
* | ``decl_variable`` is called for declarations of variables.
* | ``decl_routine`` is called for declarations of non-member functions without
    bodies (it is also called via ``function_definition`` when a body is
    present).
* | ``decl_typedef`` is called for ``typedef`` declarations.
* | ``decl_parameter`` is called for old-style parameter list declarations.
* | ``define_static_data_member`` is called for C++ static data member
    definitions.

Finally, if an initializer is present in a variable or static data member
declaration, it is scanned by a call to ``initializer``; in cases where default
initialization may be required, ``def_initializer`` is called.

Other sorts of declarations are handled by the following (via
``check_special_declaration_form``):

* | ``template_directive_or_declaration`` is called for template declarations,
    template instantiation directives, and template specializations (in other
    words, declarations that begin with the keyword ``template``).
* | ``namespace_declaration`` is called for namespace and namespace alias
    declarations.
* | ``nonmember_using_declaration`` is called for all ``using``-declarations
    except those appearing in a class definition.
* | ``using_directive`` is called for ``using``-directives.
* | ``asm_declaration`` is called for declarations that begin with the keyword
    ``asm``.
* | ``alias_declaration`` is called for C++11-style alias declarations.
* | ``scan_and_record_cli_delegate_definition`` is called for C++/CLI delegate
    definitions.

Declaration Parse State
-----------------------

The outline above just lists a few of the many functions involved in parsing
declarations, and many of those functions need to share various bits of state.
The principal structure tracking this state through this process have type
``a_decl_parse_state``.  This keeps track of types (declared type, effective
type, etc.), positions (start of declaration, declarator, certain specifier
keywords, etc.), additional declarative attributes (GNU, Microsoft, etc.), and
so forth.

For declarations involving a declarator, the declared entity's symbol, once
available, is pointed to by ``dps->sym`` where ``dps`` is a pointer to the
declaration parse state for that declaration.

In most cases the declaration parse state is initialized using the
``init_decl_parse_state`` macro, but when multiple comma-separated declarators
are handled, the state is recycled for each secondary declarator by calling
``start_secondary_declarator`` (which preserves and even restores state
associated with the common declaration specifiers).

``a_decl_parse_state`` includes a field of type ``an_init_state`` to describe
the processing of any initializer associated with the declaration (although
``an_init_state`` variables can also exist independent of a specific
declaration; see also :ref:`init-components-and-state`).

The ``a_decl_parse_state`` stucture also maintains a list of "actions"
(pointers to functions accepting a pointer to that state) that should be run at
the end of a declaration; this can be useful when a local declaration element
is encountered that cannot be checked until the declaration has been more or
less completely processed.  New actions can be registered by calls to
``add_end_of_parse_action`` (they are eventually run by a call to
``run_end_of_parse_actions``).

The ``a_decl_parse_state`` structure is not only used by declarations handled
by ``scan_nonmember_declaration``, but also for class member declarations,
parameter declarations, and even when parsing type names in contexts such as
casts and template arguments (through function ``type_name_full``), which isn't
really declaration parsing at all.

.. _cppcli-assembly-decls:

Declarations from C++/CLI Assembly Metadata
-------------------------------------------

In C++/CLI mode, the front end is able to import assembly metadata files.  This
is achieved by first transforming the metadata into equivalent C++/CLI source
code, and then parsing that code.  Whether metadata is imported implicitly
(because it is the core library file), explicitly through a "preusing"
command-line toption, or explicitly through a ``#using`` directive, the root
function doing the importing is ``import_metadata`` (in ``preproc.c``).  This
function sequences the following operations:

* | It prepares a file for importing by calling ``make_cli_metadata_file`` and
    ``import_metadata_file``.
* | If all went well, it calls ``generate_top_level_metadata_code``, which
    creates a string containing C++/CLI source representing the
    declarations -- but not the definitions -- of the top-level entities
    (which are managed class types) in the metadata file.  The actual work
    for this is done by the "metadata reader" implemented in
    ``ms_metadata.cpp`` (a C++ file meant to be compiled only in a
    Microsoft environment).
* | Finally, the generated string is passed to
    ``scan_top_level_metadata_declarations``, which (much like
    ``translation_unit``) repeatedly calls ``declaration`` to internalize the
    generated code.

The definitions of the generated entities [#f1]_ aren't imported until (and
unless) they are needed.  When that happens, a process somewhat similar to that
of ``import_metadata`` is performed by ``get_definition_of_class``, but this
time routines to process definitions are called instead of ``declaration``:
``scan_cli_generic_class_definition_from_assembly_import`` for generic entities
(including generic delegates),
``scan_cli_delegate_definition_from_assembly_import`` for non-generic delegate
definitions, and ``scan_class_definition`` for other non-generic class type
definition.
An imported assembly is assigned a number (the "assembly index") and that
number is recorded in the IL representation of class and enum types loaded from
the assembly.
While processing code generated from metadata (top-level or needed
definitions), the global variable ``scanning_generated_code_from_metadata`` is
TRUE.  This is used, for example, to suppress certain diagnostics that do not
apply to code generated from metadata.

Linkage Specifications
======================

In C++ the first thing ``declaration`` must check for is the token ``extern``
followed by a string literal (e.g., ``extern "C"``).  This introduces a linkage
specification, which may control a single declaration or a brace-enclosed block
of declarations.  In either case ``linkage_specification`` is called.  It sets
global variable ``def_external_linkage`` to describe the kind of linkage that
is to be the default for subsequent declarations, and then it calls
``declaration`` again.

Declarations *vs.* Expressions
==============================

Inside a function body it is sometimes difficult to determine whether something
is a declaration or an executable statement.  In C the presence of a
declaration specifier keyword or a type name is sufficient to disambiguate the
cases, but in C++ it can be more complicated.  Similar disambiguation problems
appear in other contexts as well.  Here are some examples.

* | a statement *vs.* a declaration:

  .. code:: c++

     typedef int I;
     I(i);              // declaration (= I i);
     I(i)++;            // cast i to I, then increment
* | with ``operator new``, a parenthesized type *vs.* a placement expression:

  .. code:: c++

     new (int(1.5)) A   // placement
     new (int(*  ))     // type
* | a parenthesized initializer *vs.* a parameter declaration

  .. code:: c++

     struct A { /* ... */ };
     A a(int(1));   // declare variable a (initialized by
                    //    calling A::A() with arg 1)
     A f(int(i));   // declare function f to take an integer
                    //    argument and return an A

The function ``is_decl_not_expr`` is called in cases where disambiguation is
required.  This may in turn use the routines ``prescan_declaration``,
``prescan_decl_specifiers``, ``prescan_declarator``, and
``prescan_function_declarator``.  In general, if a sequence of tokens looks
like a declaration, then it is a declaration, even if it could also be an
expression.  The technique requires looking ahead as many tokens as necessary
to confirm or disprove the assumption that the token sequence is a declaration;
tokens are cached so that they can be rescanned by the caller.

Thus, in statement processing, when ``is_decl_not_expr`` returns TRUE the
tokens are scanned as a declaration (``declaration`` is called); otherwise, the
tokens are scanned assuming that they constitute a statement.

Declaration Scanning
====================

Declaration Specifiers
----------------------

The principal part of the code for processing declaration specifiers appears in
``decl_spec.c``.

``decl_specifiers`` scans a list of type specifiers, type qualifiers, and/or
storage class specifiers, and returns (through a ``a_decl_parse_state`` object)
a pointer to a type tree.  It is passed a vector of input flags (telling it
that a storage class specifier is allowed, that the declaration is of a class
member, etc.) and it returns additional information about the results of the
scan (reporting that ``inline`` was scanned, that an explicit type specifier
was present, etc.) via the ``a_decl_parse_state`` object that was passed in.

The specifiers are scanned in a loop.  The simple ones are handled directly.
When ``enum`` is seen, ``enum_specifier`` is called.  When ``class``,
``struct``, or ``union`` is scanned, ``class_specifier`` is called; the
processing entailed in scanning a class definition (see
``scan_class_definition`` in ``class_decl.c``) is described in the chapter on
Class Declarations.

A ``typedef`` name may appear as a type specifier, as may a class name in C++.
Therefore, to tell whether an identifier (including a qualified name) is a type
name or a declarator, a call is made to ``curr_type_symbol``; it returns a
symbol pointer when the identifier is a type name and NULL otherwise.  Since
class name can also be a constructor name under some conditions, that case is
also checked for by ``is_constructor_decl``.

The loop continues until it reaches something it does not recognize as a
declaration specifier.  Then the accumulated information is combined to create
a type entry (see ``combine_type_specifiers`` and ``add_type_qualifiers``, and
the output flags are set.  When no type is explicitly specified, a default type
is returned; ordinarily it is ``int``, but when the declarator is recognized to
be a constructor, the default is a reference to the class type, and for
destructors it is ``void``.

``decl_specifiers`` is also called to scan a set of type qualifiers only, e.g.,
as part of scanning a pointer-declarator.  For such cases
``a_type_qualifier_set`` is always returned independently of the type (even
though in the usual case it is also part of the returned type).  (See also
``collect_type_qualifiers`` in ``declarator.c``.)

The details of an ``enum`` declaration are handled by ``enum_specifier``.
First it calls ``scan_tag_name`` (which is also called from
``class_specifier``) to look up the (optional) tag name and return a symbol.
When a brace-enclosed declaration follows the tag name, ``enum_specifier``
scans the list of enumeration constant declarations, calling
``scan_integral_constant_expression`` when a value is specified.  Each
declaration is turned into a constant entry with the proper value, and all of
the constant entries are linked together under the type entry for the
enumerated type.  The size of the enumerated type is also determined from the
value.  [#f2]_ Note that in C++, unlike C, the types of the enumeration and its
constants are always the same.

Declarators
-----------

Most of the code for scanning declarators is found in ``declarator.c``.

``declarator`` scans a declarator.  It can scan either a real declarator or an
abstract declarator.  It receives as input (via the ``a_decl_parse_state``
object tracking that declaration) the type from an associated specifiers list,
and combines that type with the type information in the declarator.  It also
returns a symbol locator for the name declared in the middle of the declarator.
For an abstract declarator, it returns an error locator.  It takes as input a
vector of flags to direct its processing, and it returns a vector of output
flags to report results to the caller (again via the ``a_decl_parse_state``
object).

``declarator`` is the top-level interface and calls ``r_declarator``, which is
recursive.  Declarator processing has three parts:

* | ``pointer_declarator`` is called to handle pointer modifiers (``*``) and in
    C++ references (``&``) and pointer-to-member syntax.  There can be more
    than one such modifier, and each can be accompanied by one or more type
    qualifiers.  These are combined with the type returned by
    ``decl_specifiers``; the routines invoked to construct the new types are
    ``make_pointer_type``, ``make_reference_type``,
    ``make_rvalue_reference_type``, and ``pointer_to_member_type``.
* | The middle section is either an identifier (or the place where one would
    be, for an abstract declarator) or a nested declarator enclosed in
    parentheses.  The left parenthesis is ambiguous in an abstract declarator,
    so the routine looks one token past it to disambiguate the function
    declarator case from the nested declarator case.  The declarator
    identifier, scanned by ``scan_real_declarator_id``, may be a simple
    identifier, a qualified name, [#f3]_ a destructor name, or a token sequence
    representing an overloaded operator or a user-defined conversion (such as
    ``operator+`` or ``operator int``).
* | The third section consists of zero or more function or array declarators,
    for which calls are made to ``function_declarator`` or
    ``array_declarator``.

  * | ``function_declarator`` scans the parameter list (an old-style id list, a
      prototype parameter list, or ``void``) and builds a function type.  For a
      top-level function type some additional information is returned in a
      block called ``func_info``, which is returned to the caller to assist
      with later function processing; most notably, this includes a list of the
      identifier names in an old-style id list.  If the function declarator is
      a prototype, and if any tags are declared in the prototype, a prototype
      scope is created.
  * | ``array_declarator`` scans constant and nonconstant array dimensions (the
      latter for variable length arrays in C mode and type names in ``new``
      expressions in C++).

  | The processing for VLA declarations has several twists.  In the ordinary
    case in which a VLA declaration appears at function or block scope, a VLA
    dimension entry is created and an ``stmk_set_vla_size`` statment is put
    out.  If the VLA declaration appears in a function prototype, then the
    dimension expression may be unspecified (i.e., the\ ``[*]`` syntax is
    accepted); this is not permitted, however, when the function prototype
    belongs to a function definition, but this check is delayed till
    ``scan_function_body``.  If the dimension expression *is* specified in a
    function prototype scope, a fixup entry is created and processing is
    completed in ``scan_function_body``: once the routine's IL scope has been
    created, the VLA dimension entry can be allocated and any references to
    parameter variables can be properly incorporated into the VLA dimension
    expression.

The type information from the three sections is combined into a single type
tree.  The routine that combines derived types is ``add_to_derived_type_list``;
aside from the purely mechanical task of joining types, it also does error
checking, and calls ``set_type_size`` once all the parts of a derived type have
come together.

Information about the *declarator-id* is returned from ``declarator`` in a
symbol locator.

C++/CLI Type Constructs
-----------------------

C++/CLI adds several type composition mechanisms: handles, tracking references,
interior pointers, pin pointers, CLI arrays, and delegates.  It also adds new
class type and enum type variations.

The C++/CLI core library (which is always imported, and is usually
``mscorlib.dll``) defines a number of special "value class types" (e.g.,
``System::Int32``) that map onto fundamental types (like ``int``).  When the
type scanned by ``decl_specifiers`` is such a special class type, it is
immediately replaced by the corresponding fundamental type (by a call to
``fundamental_type_from_system_type``).  However, if a handle to a fundamental
type with a corresponding value class type is formed, the fundamental type is
replaced by the class type (through a call to ``boxed_type_for``, which in turn
calls ``system_type_from_fundamental_type``).  Handles to enumeration type are
handled similarly: A C++/CLI value class type is created that "boxes" the
enumeration (see ``make_boxed_enum_type``), and the handle is to that class
type.

Syntactically, handles and tracking references are like pointers and references
that use different declarator operators ("``^``" and "``%``" respectively).
They have different semantics and constraints, however.  Many of the basic
constraints on their underlying types are checked in
``f_check_cli_type_pointed_to``.

Like handles and tracking references, C++/CLI interior pointers and pin
pointers are represented using ``tk_pointer`` entries.  However, the syntax to
express interior pointers and pin pointers uses template-like angle brackets
(e.g., ``cli::pin_ptr<X>``) instead of declarator operators.  The front end
implements support for that syntax by predefining special alias templates
``cli::pin_ptr`` and ``cli::interior_ptr`` (see
``make_symbol_for_cli_interior_ptr`` and ``make_symbol_for_cli_pin_ptr``) that
rely on internal ``__declspec`` attributes to form the aliased ``tk_pointer``
type.

CLI arrays are instances of a ref class template ``cli::array<T, Rank = 1>``
(see ``make_symbol_for_cli_array``).  Instances of this template have the flag
``is_cli_array`` set (in their class type supplement); this simplifies, among
other things, the implementation of the function ``is_cli_array_type``).  CLI
arrays can mostly only be used via handles-to-CLI-arrays (see
``check_invalid_use_of_special_cli_class_type``).

A delegate is a special kind of generated reference class type that
encapsulates an "invocation list" (see :ref:`cppcli-delegates` for details).
Like CLI arrays, they can mostly only be used via handles-to-delegates.

C++/CLI function declarators can include a trailing "parameter array" (with
syntax like "``void f(double, ...cli::array<double> ^p)``", where the type must
be a handle to a one-dimensional CLI array).  They are akin to traditional C
variadic function arguments ("varargs"), but they are type-safe and limited to
accepting a variadic list of arguments that are all of the same type.  A
parameter array is represented with a single ``a_param_type`` entry whose
``is_cli_param_array`` flag is TRUE.

Completing the Declaration
--------------------------

Once the declaration specifiers and the declarator have been scanned, it
remains to put the type and other declarative information together and generate
IL and symbol representations for the objects declared.

If ``typedef`` was seen among the declaration specifiers, ``decl_typedef`` is
called.  It enters a ``tk_typeref`` type entry into the IL and an ``sk_type``
symbol in the symbol table.  In C++ it has some extra processing when the type
passed in to it is that of a tagless class or enum: the ``typedef`` name is
transferred to the class or enum type entry, and in cfront-compatibility mode
the symbol for the class is modified accordingly.

``define_static_data_member`` is called to process the definition of a static
data member.  The IL entry and symbol were created at the original point of
declaration.

Variables and routines are entered by calling ``decl_variable`` or
``decl_routine``, which are similarly organized functions made fairly
complicated by the need to check the declaration's compatibility with previous
declarations of the same entity.

* | ``id_linkage`` is used to determine whether the declared name has linkage,
    and if so, to what (see ``find_linked_symbol``, which returns a symbol if
    there is a previous use in scope).
* | ``qualified_name_redecl_sym`` is called instead of ``id_linkage`` if the
    declarator is a namespace-qualified name (e.g., a friend declaration or a
    namespace member definition that appears outside the scope of the namespace
    to which it belongs); it also calls ``find_linked_symbol``.
* | ``find_linked_symbol`` not only calls the appropriate lookup routine; it
    also deals with function overloading and function template specializations
    (see ``matching_template_function`` in ``templates.c``).
* | ``create_external_symbol_for_linked_entity`` is called to find any previous
    external variable or routine of the same name, even if the previous name
    has since gone out of scope; it also checks that the old and new types are
    compatible.  ``decl_variable`` and ``decl_routine`` use this information to
    decide whether to create a new variable or routine entry or instead to
    reuse or complete a previous declaration.

"Block-extern" variables and routines have linkage and are always entered at
the file scope or namespace scope in the intermediate language, but their
symbols belong to the current scope in the symbol table (except in ``pcc``
mode, where they are entered at the file scope in the symbol table too).  When
a ``friend`` declaration introduces a new routine, its symbol is entered in the
innermost enclosing nonclass scope, and the associated routine entry is entered
in the innermost enclosing namespace scope.

In addition to deciding whether an existing symbol and IL entry can be reused
or new ones need to be created, ``decl_variable`` and ``decl_routine`` are also
responsible for checking type compatibility, storage class, and name-linkage,
and they call the routines that record cross-reference and source-sequence
information.

Functions with bodies need additional processing.  ``function_definition``
finishes the scanning of a function definition started by ``declaration``.  If
there is an old-style parameter list, the declarations are scanned through a
series of calls to ``declaration``.  Next, it calls ``decl_routine`` (for
normal functions) or ``define_member_function`` (for member functions) to
retrieve or create the function's symbol and routine entry.

Then, ``scan_function_body`` is called.  First, the scope stack is fixed up --
if the routine is a member function defined outside the class body, the scope
of the class of which it is a member is reactivated, or, if it is a namespace
member appearing outside the scope of the namespace, a namespace extension
scope is pushed -- after which the routine's own function scope is pushed.

Function parameter processing is completed within the function scope, so that
parameter variables will be entered into the symbol table correctly.  A pass is
made over the linked list of ``a_param_type`` entries recorded in the function
type and the linked list of ``a_param_id`` entries created from scanning the
parameter declarations (the two lists should be in sync; the latter knows the
name of the parameter, the former only the type), and ``decl_parameter`` is
called to create the parameter variable entries.

When support for variable length arrays is enabled, a fixup is performed on any
VLA declarations that appeared in the function prototype.  If the VLA dimension
expression referred to a parameter, a dummy variable is replaced with the
"real" parameter variable that has just been created.  Then, the dimension
expression is copied from the file-scope memory region to the function-scope
memory region, and an entry of type ``a_vla_dimension`` is created to record it
in the IL.

If it is a constructor that is being defined, ``ctor_intializer`` is called to
scan the constructor initializer list and to enter implicit ctor-initializers;
similar processing is done for a destructor with a call to
``dtor_initializer``.  If this definition appears in the midst of statement
processing (either because it is an inline member function of a local class or
a template instantiation), it is necessary to save the structured statement
stack and start a brand new one (see ``new_struct_stmt_stack``).  Then
``compound_statement`` can finally be called to scan the body of the function;
the resulting statement tree is recorded in the ``assoc_block`` field of the
function's scope entry.  Before returning, ``scan_function_body`` restores the
structured statement stack and pops the various scopes off the scope stack.

Initialization
==============

Initialization is a broad topic in C and C++. It includes:

* | Setting the initial value of a variable as part of its declaration (the
    traditional C notion of initialization)
* | Initializing members and bases of a class as part of the execution of a
    constructor
* | Initializing a temporary variable as part of a cast or C99-style compound
    literal
* | Passing arguments in a function call (parameter variables are initialized
    with argument values)

Furthermore, "default" initialization actions may be required even in the
absence of an explicit source construct, and the binding of references, while
treated as a form of initialization, involves a number of rules of its own.
Also, C++17 structured bindings are sometimes "aliases" for expressions and
those are represented as variables with a special kind of "initializer"
(although arguably those do not correspond to actual initialization).

Source file ``decl_inits.c`` is primarily concerned with declaration contexts:
Initializing variables and initializing members of classes as part of
constructor definitions (i.e., the first two bullets).  It also includes the
structural handling of aggregate initialization (the traditional brace-enclosed
initializers), and that handling is also used in non-declaration contexts;
particularly, in C99-style compound literals and in various C++11-style list
initialization contexts.

IL Representation
-----------------

The special case of a variable with static storage duration initialized with a
true constant is represented by associating the ``a_constant`` entry directly
with the ``a_variable`` entry (via the ``an_initializer`` entry embedded in the
variable entry).  C++17 structured bindings for array elements and structure
fields are represented as variables with a special kind of "binding"
initializer pointing to the aliased expression (also via the ``an_initializer``
entry).  In all other cases, initialization involves a dynamic element and an
entry of type ``a_dynamic_init`` is used to represent it (see
:ref:`il-dynamic-init`).

In addition, even for entities that are not initialized (or whose
initialization is "trivial"), a dynamic init entry may nonetheless be generated
to represent the eventual destruction of that entity (assuming that entity has
a nontrivial destructor).

.. _init-components-and-state:

Init Components and Init States
-------------------------------

It is frequently convenient in the front end to separate the parsing of an
initializer from its actual binding to a target entity.  When this is done, the
parsing phase produces an "init component" (type ``an_init_component``, defined
in ``expr.h``).  Such a component can represent either

* | an expression, or
* | a C99-style aggregate member designator, or
* | a brace-enclosed list of init components.

(A fourth kind of "placeholder" init component exists.  It represents a
point at which the parsing of a large aggregate initializer has been
suspended for later continuation.  This is transparent to the routines
processing initializers because they don't access the ``next`` pointer of
init components directly and instead use macros -- like ``next_elem`` -- that
automatically resume parsing when reaching one of these placeholder
entries.)

An init component that represents an expression ultimately points to an object
of type ``an_operand`` (see :ref:`expr-operands`), as well as other information
needed for the second ("binding") phase of initializer processing.

``decl_inits.c`` relies on two functions from ``expr.c`` to scan init
components: ``scan_braced_init_list`` for list initializers (which in
general will return the top component of a tree representing the braced
structure), and ``scan_full_initializer_expr_as_component`` (which will
return a simple expression component).  Sometimes, initializers must be
"prescanned" (to deduce the type of a C++11-style ``auto`` declaration):
The two aforementioned functions will retrieve the prescanned init
components from a cache if needed.  Eventually,
``free_init_component_list`` must be called on the top-level component
produced for every initializer.

The binding of an initializer as represented by an init component to its target
entity is generally handled by a call to ``convert_initializer``, except for
aggregate initialization where the structure of the initializer is mapped to
that of the target type using a variety of functions (see
:ref:`decl-aggregate-init`) which eventually call the ``expr.c`` function
``convert_initializer`` for the non-aggregate "leaf" elements of the aggregate
initializer.

Initializer processing shares a fair amount of information across calls to a
large number of functions.  To facilitate this, the information is recorded in
a structure of type ``an_init_state``.  This is both an "input" and an "output"
device: Some fields are used to pass information down the initializer
processing functions, while others are used to return results.  In particular,
some input fields inhibit the generation of IL entries
(``check_validity_only``) and/or the emission of diagnostics
(``no_diagnostics``): This is needed to handle overload resolution and template
deduction (which requires "tentative" initialization in C++11).

A declaration parse state block (``a_decl_parse_state``) embeds an init state
for initialization directly associated with a declaration (variable
initializers, particularly), and in that case, the embedded init state also
contains a pointer back to the associated declaration parse state block.

Variable Initializers
---------------------

Variable initializers (including initializers for static data members) are
scanned and processed by calling ``initializer`` in ``decl_inits.c``.  After
checking for errors independently of the initializer itself, and if necessary
deducing the type of the variable from the type of the initializer
(``prescan_initializer_for_auto_type_deduction``), the function distinguishes
four syntactic cases:

#. | parenthesized initializers (a C++ feature; e.g., "``T x(3);``"),
#. | direct list initializers (a C++11 feature; e.g., "``T x{3};``"),
#. | traditional list initializers (e.g., "``T x = {3};``"), and
#. | simple initializers (e.g., "``T x = 3;``").

Parenthesized initializers for class types and for template-dependent types are
the only variations that do not use init components as an intermediate
representation of the initializer: Instead, these cases are handled by calls to
``scan_class_parenthesized_initializer`` and
``scan_dependent_type_parenthesized_initializer``, respectively.  The remaining
parenthesized cases (i.e., those that cannot involve a constructor and
therefore must allow for only a single parenthesized expression) are handled by
``expr_direct_init_object``.

Both the C++11-style direct list initialization syntax and the more traditional
list initialization syntax are handled by calls to ``brace_init_variable``,
except that the initialization of a C++/CLI array using traditional list
initialization syntax uses a dedicated routine
``aggr_init_cli_array_with_alloc``.  brace_init_variable calls
braced_initializer

Finally, initializations of the form "``T x =`` *expr*" are handled by
``expr_init_aggr_variable`` (for variables of class or array types) and
``expr_init_scalar_variable`` (for scalar variables).

Structured bindings to array elements and to fields are technically not
variables (they are aliased to components of the unnamed "container" variable
for the binding), but they are represented though ``a_variable`` entries in the
front end.  These entries have an ``initk_binding`` initializer kind and point
to the aliased expression.  The routines
``record_struct_binding_expr_for_array_element`` and
``record_struct_binding_expr_for_field`` (both in ``expr.c``) handle this
pseudo-initializers.

Auto type deduction
^^^^^^^^^^^^^^^^^^^

Some modes allow the keyword ``auto`` to be used as a type specifier.  In such
cases, the actual type must be deduced from the initializer using rules similar
to those for function template argument deduction.  This presents an ordering
challenge: The expression handling routines called by ``initializer`` must know
the type of the entity being initialized (e.g., to determine appropriate
conversions), but that type is not known until the initializer has been
determined.  To address this, the initializer for entities declared with the
``auto`` type specifier is prescanned (routine
``prescan_initializer_for_auto_type_deduction`` in ``expr.c``): That process
deduces the actual type of ``auto`` (using the same machinery as that used for
calls of function templates) and records the prescanned init component in the
``a_decl_parse_state`` entity for the current declaration.  The routines called
by ``initializer`` to scan init components are aware of this record and will
use it instead of actually scanning tokens when appropriate.

The same mechanism is also used to support the ``auto`` type specifier in
*new-expression*\ s (``scan_new_operator`` in ``expr.c``) and in
declarations of ``static const`` data members with in-class initializers
(``decl_static_data_member`` in ``class_decl.c``).

Default Initialization
^^^^^^^^^^^^^^^^^^^^^^

C++ entities of class type that are declared without an explicit initializer
are "default-initialized" -- which means calling the class's default
constructor (or, for an array, calling the default constructor for each
element):

.. code:: c++

   struct A { A(); /* ... */ } a;    // a is default-initialized by A::A()

The routine ``def_initializer`` is called for declarations without
initializers, and it generates the default initialization if appropriate.
Default initialization is done only for variables and static data members that
are defined in the current translation unit, and not all classes are subject to
default initialization in the same way:

#. | "POD" structs and unions (roughly, C-style structs: no non-public
      members, no user-defined constructor, destructor, or assignment operator,
      no base classes, no virtual functions, etc.) are *never*
      default-initialized:

   .. code:: c++

    struct A { int i; };     // POD
    A a1;                    // a1 goes uninitialized
    const A a2;              // error -- no explicit initializer

#. | Non-POD classes with an implicitly-declared trivial default constructor
     (i.e., classes for which each base class or nonstatic data member of
     class type, if any, will in turn be initialized with its own trivial
     default constructor) are default-initialized only when the object is
     non-``const``:

   .. code:: c++

     struct B { private: int i; };    // implicit trivial B::B()
     B b1;                            // b1 is default-initialized
     const B b2;                      // error -- no explicit initializer

#. | Classes with an implicitly-declared nontrivial default constructor are
     likewise default-initialized only when the object is non-``const``:

   .. code:: c++

    struct C { int i; virtual void f(); }; // implicit nontrivial C::C()
    C c1;                                  // c1 is default-initialized
    const C c2;                            // error -- no explicit initializer

#. | Classes with at least one user-declared constructor are default
     initialized whether or not the object is ``const``:

  .. code:: c++

     struct D { int i; D(); };   // user-declared D::D()
     D d1;                       // d1 is default-initialized
     const D d2;                 // d2 is default-initialized

A further distinction is made between the second and third groups (classes with
trivial vs.  nontrivial implicitly declared default constructor): ``b1`` and
``c1`` differ in how their default initialization is implemented.  A call to
the nontrivial default constructor ``C::C()`` is actually added to the IL (it
needs to deal with the virtual function info for class ``C``), but the trivial
default constructor ``B::B()`` need not actually be called, since calling it
would have no effect.  Its definition is generated in case there might be
compile-time side-effects (see ``reference_to_trivial_default_constructor`` in
``symbol_ref.c``), but the IL for a trivial default constructor does not appear
in the IL.

.. _decl-aggregate-init:

Aggregate Initializers
----------------------

An aggregate type is an array type or a class type that has no user-provided
constructors.  When an entity of such a type is initialized with a
braced-enclosed construct, the front end traverses the initializer structure
and the destination type's structure mapping one onto the other.  The result is
an aggregate constant (``ck_aggregate``), which may contain elements that are
not actually constants (i.e., ``ck_dynamic_init`` "constants").  Four cases are
distinguished:

* | aggregate class types: handled by ``aggr_init_class``, with help from
    ``aggr_init_field_designator`` to handle C99- and GNU-style designators,
    and from ``aggr_init_class_remainder_if_needed`` to represent nontrivial
    initializations (in C++) of fields that have no explicit initializer.
* | array types: handled by ``aggr_init_array``, with help from
    ``aggr_init_array_designator`` to handle designators, and from
    ``aggr_init_array_remainder_if_needed`` to represent nontrivial
    initializations (in C++) of array elements that have no explicit
    initializer.
* | template-dependent types: handled by ``aggr_init_generic_element``; since
    the structure of the destination type is not known in that case, the
    structure of the resulting constant is entirely determined by the
    brace-structure of the initializer.
* | C++/CLI array types: handled by ``aggr_init_cli_array`` with help from
    ``aggr_init_cli_array_level`` (which deals with the special rules for
    deducing CLI array dimensions from the initializer when applicable).

The cases can be composed (e.g., an aggregate struct can contain an array of
aggregate structs), which is handled through recursion: ``aggr_init_element``
handles the dispatching of the recursion based on the subaggregate object's
type.  Leaf entities (i.e., entities whose initialization doesn't fall into one
of the cases above) are handled by ``aggr_init_simple_element``.  Cleanup
actions needed in case of an exception occurring during the initialization of a
leaf entity -- including default initialization -- is handled by calls to
``record_partial_aggregate_cleanup_destruction``.

The complete handling of a top-level aggregate initialization is initiated
either by a call to ``braced_initializer`` (for the initialization of variables
and static data members, compound literals, and constructor-initializers), or
by a call to ``prep_aggr_initializer`` (for all other expression contexts).

While "aggregate initializer" mainly deals with the initialization of an entity
of aggregate type with a brace-enclosed construct, it also includes the
initialization of a character array with a string literal.  This can happen at
the top-level (e.g., "``char str[] = "text``";") or at a subaggregate level
(e.g., "``X x = { 1, 2, "three" };``").  A central routine for this aspect of
initialization is ``try_string_literal_init``.

Constructor Initializers
------------------------

Constructor initializers can be written explicitly in the source:

.. code:: c++

   struct A {
     int i, j;
     A() : i(1), j{2}  // Constructor initializers with parenthesized
       {}              // and braced syntax, respectively.
   };

They can also be generated implicitly by the front end for class members and
base classes that must be initialized and are not explicitly initialized in the
source.  (This includes, as an extreme case, constructor routines that are
front-end-generated; any needed constructor-initializers would have to be
implicit).

``ctor_initializer`` builds the proper list of constructor-initializer entries
and attaches it to a constructor routine entry.  It is called with a flag that
indicates whether or not it should attempt to scan the source construct (if
not, it just generates a default list without looking at the source).

The list generated is in the "right" order, that is, the order in which
initialization should be done, which is not necessarily the order in which the
explicit constructor-initializer clauses were written.  The list contains three
parts, tracked by the ``a_ctor_init_block`` data structure: direct base
entries, virtual base entries, and field entries.  Each initialization is
represented in the IL with an entry of type ``a_constructor_init`` (which in
turn points to a dynamic init entry).

The explicit clauses are scanned by calling ``scan_mem_initializer``, which
ultimately calls

* | ``scan_parenthesized_mem_init_args`` for parenthesized initializers (often
    in turn calling ``scan_dependent_type_parenthesized_initializer`` for
    template-dependent entities, ``scan_class_parenthesized_initializer`` for
    class type entities initialized through a constructor call, or
    ``expr_direct_init_object`` for other initializations not involving a
    constructor).
* | ``braced_initializer`` for C++11-style braced initializers.

Once any explicit clauses have been scanned, default initializers are generated
for any base classes and members that require them.  In addition, in C++11
mode, implicit constructor initializer entries may be generated for fields with
"field initializers" (see also :ref:`field-inits`): Such entries do not decribe
the initialization directly, but defer to the initializer recorded in the
``a_field`` entry by setting the ``use_field_initializer`` flag to TRUE.

Destructor-initializers
-----------------------

The destructor-initializers list attached to a destructor indicates any
destructions required for base classes and members of the destructor's class.
The list is in the order in which the destructions should be done, i.e., the
reverse of the order of the list on the corresponding constructor.

Unlike in the constructor case, there is no source form for the
destructor-initializers, and therefore all the entries on the list are
front-end generated.

Type Names in Expressions
=========================

When a type name appears in an expression, routines that are used in
declaration processing are invoked to scan the type name.

``type_name`` is called to scan a type that appears in a cast expression or as
an argument to ``sizeof``.  It calls [#f4]_ ``decl_specifiers`` and, for
abstractor declarators only, ``declarator``.  It returns a pointer to a type.

``new_type_name`` is called to scan a C++ *new-type-name* or a parenthesized
*type-name* that may appear in a ``new`` expression.  Only abstract declarators
are allowed, but a nonconstant bound expression is permitted for the first
array dimension.

``asm`` Declarations
====================

``asm_declaration`` is called when an ``asm`` keyword is encountered.  A
parenthesized string literal is scanned and recorded in an entity of kind
``an_asm_entry``.  The ``asm``-string is not parsed or validated by the front
end -- it is simply passed as-is to the back end.

In Microsoft and GNU modes, some syntax variations are accepted, and in GNU
modes, this can involve some limited validation checks.

Namespaces
==========

Namespace Declarations
----------------------

``namespace_declaration`` is called to process original namespace definitions,
namespace extensions, and declarations of namespace aliases.

When a namespace is originally declared, an entry of type ``a_namespace`` is
created and an ``sk_namespace`` symbol is entered in the symbol table.  Then,
an ``sck_namespace`` scope is pushed, and declarations inside the namespace are
represented by symbols that are entered on the active symbol list of a given
symbol header; when the scope is popped the symbols are moved from the active
list to the inactive list.  However, that namespace can be reopened again, in
which case it is an ``sck_namespace_extension`` scope that is pushed onto the
scope stack.  It points to the same IL scope as the original ``sck_namespace``
scope, but symbols that are entered within this scope are added directly to the
inactive list.  This affects how name lookup is done inside a namespace
extension definition -- see the :ref:`symbol-table` chapter.

For both original and extending namespace definitions, once the appropriate
scope has been pushed, a loop is executed to make a series of calls to
``declaration`` until the closing ``}`` of the namespace definition is
encountered.  Then the ``sck_namespace`` or ``sck_namespace_extension`` scope
is popped off the scope stack.

Unnamed namespace definitions are a special case.  For an original unnamed
namespace definition, an ``sck_namespace`` scope is immediately pushed and then
popped, a ``using``-directive is simulated, and then an
``sck_namespace_extension`` scope is opened.  [#f5]_ In other words, an
original unnamed namespace definition that is written like this

.. code:: c++

   namespace {
     int i;
   }

is represented internally as if it were

   | ``namespace`` *<unnamed>* ``{ }``
   | ``using namespace`` *<unnamed>*\ ``;``
   | ``namespace`` *<unnamed>* ``{``
   |   ``int i;``
   | ``}``

``namespace_declaration`` also handles declarations of namespace aliases, for
which a namespace IL entry and a namespace symbol are also created.  However,
in this case the namespace IL entry has its ``is_namespace_alias`` flag set to
TRUE and points not to an associated IL scope but to the namespace entry it
stands for.

Using-Directives
----------------

A *using-directive* is processed by ``using_directive``, which looks up the
namespace name and calls ``make_using_directive`` to create and enter an entry
of type ``a_using_decl`` and to "activate" the using directive in the current
scope (see ``add_active_using_directive`` in ``scope_stk.c``).  The effect of a
*using-directive* on the lookup algorithm is discussed in the Symbol Table
chapter.

Using-Declarations
------------------

A *using-declaration* that appears in a scope other than that of a class
definition is processed by ``nonmember_using_declaration``.  [#f6]_ First it
looks up and validates the namespace-qualified or globally qualified name.
Then it enters an ``sk_namespace_projection`` symbol to represent the name
pulled into the current scope by the using-declaration.

When a *using-declaration* specifies the name of an overloaded function, each
member of the overload set will have its own projection symbol.  An overload
set in the current scope may end up with a mix of ``sk_routine`` symbols
(declarations from the current scope) and ``sk_namespace_projection`` symbols
(referring to declarations from another scope).
``conflicts_with_previous_function_decl`` is called to determine if a function
specified by a *using-declaration* is not distinguishable, for purposes of
overloading, from functions actually declared in the current scope.

An IL entry is created to represent the *using-declaration* (see
``make_using_decl``); it is available to back ends from the ``using_decls``
list pointed to from the current scope.

Standard Attributes, GNU Attributes, and ``__declspec`` Attributes
==================================================================

In declarative contexts, the processing for standard attributes (delimited by
double square brackets) is very similar to that for GNU attributes
("``__attribute((...))``") and Microsoft ``__declspec(...)`` attributes.  The
main difference (other than the delimiting tokens) is that standard attributes
have strict "appertaining rules" whereas nonstandard (GNU and Microsoft
``__declspec``) attributes can be placed more loosely.  The standard rules are
basically that an attribute applies to the construct immediately preceding it,
except for "prefix attributes" which apply to all the entities denoted by the
declarators of the prefixed declaration.  With nonstandard attributes, the
entity modified by an attribute depends on the attribute.  For example:

.. code:: c++

   [[noreturn]] int [[X]] f [[ nothrow ]] () [[Y]], g();    // (1)
   #define A __attribute
   A((noreturn)) int A((nothrow)) f() A((pure)), g();       // (2)

In (1), the ``noreturn`` attribute applies to the functions ``f`` and ``g``,
and any attribute in that location would apply to those entities.  The unknown
attribute ``X`` (in (1)) applies to ``int``, ``nothrow`` applies to ``f``, and
``Y`` applies to the function type associated with the function declarator that
precedes it.  (There are currently no standard attributes that could appear in
the locations indicated by X and Y, but the language specification does
anticipate their appertainance.)

While in (2) the ``noreturn`` attribute applies as in (1), the ``nothrow``
attribute applies to the type of ``f`` (a function type) and the ``pure``
attribute applies to ``f`` itself.  Furthermore, the locations of the
``nothrow`` and ``pure`` attributes can be interchanged with no change of
meaning.

The principal routines for parsing and applying attributes are described in
detail :ref:`pragmas-and-attributes`.  For attributes that apply to
declarations, the attribute application "callback" functions can rely on the
``assoc_info`` field of the attribute entry pointing to the
``a_decl_parse_state`` structure for that declaration.

.. _ms-attributes:

Microsoft Attributes
====================

In Microsoft mode, Microsoft attributes delimited by single brackets are
accepted.  Attributes may, in general, be specified where a declaration is
accepted.  In particular, they are accepted in namespace scopes, class scopes,
and in function parameter lists.  Attributes have the form:

.. code:: c++

   [attribute_name]
   [attribute_name(args)]

When the bracket that begins an attribute is encountered,
``scan_microsoft_attributes`` is used to scan the attributes and create the IL
entries used to represent them.  A list of attributes is returned.  This list
is later passed to one of the following routines that either add the attributes
to the IL or issue the appropriate diagnostics if the attributes appear in
invalid locations:

.. code:: c++

   apply_microsoft_attributes
   verify_standalone_attributes
   dispose_of_unapplied_attributes

The attribute scanning routines use an attribute description data structure to
describe the attributes that should be recognized and to describe the argument
lists expected by those attributes.  When the
``RECOGNIZE_MICROSOFT_ATTRIBUTES`` macro is TRUE, the front end supplies
attribute descriptions for the documented Microsoft attributes.  When
``SUPPRESS_MICROSOFT_ATTRIBUTE_PROCESSING`` is TRUE, the Microsoft attributes
are recognized in the sense that no "unrecognized attribute" warning is issued,
but they are otherwise treated as unrecognized attributes (see below).

Each attribute is represented by an entry of type ``an_ms_attribute``.  The
entry contains a string representation of the attribute.  In addition, a
structured representation is also created for recognized attributes.  In the
structured representation, each attribute has a list of associated argument
values.  The argument values are checked to verify that they match the kind of
value expected for a given argument (e.g., for a boolean parameter the value
must be true or false).  When processing an unrecognized attribute, no semantic
checking of argument values is performed and no checking is done to verify that
a given attribute is used in an acceptable location.

Attribute processing results in a list of attribute IL entries, and a flag
in the source correspondence that indicates that attributes apply to a
given entity (see the :ref:`intermediate-language` chapter for more
information).  The source correspondence flag is only available when the
structured attribute representation is being used.  Beyond the error
checking described above, no additional semantic processing of attributes
is performed.  For example, no code insertion is performed for the COM
attributes.

C++/CLI makes use of the same attribute syntax.  Currently, however, no
C++/CLI-specific attributes are recognized (and as a consequence no associated
semantics are implemented).

.. [#f1] The top-level entities imported from assemblies are generic and
         nongeneric managed classes, including some expressed using delegate
         definition syntax.  A delegate whose definition has not yet been
         loaded is treated like a ref class without a definition.
.. [#f2] See ``enum_types_can_be_smaller_than_int`` in the configuration
         section of this document.
.. [#f3] As an extension it is permitted to use a qualified name as the
         declarator in a class member declaration (see
         ``simplify_curr_class_qualified_name``).  For instance,
.. [#f4] The indirect call is via the more general function ``type_name_full``.
.. [#f5] The reason for this is to correctly handle globally qualified
         references to unnamed namespace members within the original namespace
         definition the same way as within a subsequent namespace extension.
         For example:
.. [#f6] Using-declarations that appear inside a class definition are handled
         by ``member_using_declaration`` in ``class_decl.c``, which is
         described in the Class Declarations chapter.
