=====================
C-Generating Back End
=====================

``c_gen_be.c`` contains code to translate the intermediate language to C;
``c_gen_be.h`` contains the associated declarations.  This is a fake "back end"
that is useful as a way of testing the front end in the absence of a real back
end, as a portability aid, and as an example of how to traverse the
intermediate code (which may be helpful to those writing back ends).

Function
========

Generated C Code
----------------

The C code generated can be either ANSI C or old-style K&R C, depending on the
setting of the configuration flag ``C_GEN_BE_GENERATES_ANSI_C``.

The output contains ``#line`` directives that relate the generated code back to
the original source lines that generated it.  The ``#line`` directives do not
ascend monotonically through the line numbers of the source file; they jump
around as the code generation jumps around.  However, since the C-generating
back end tends to process all declarations of entities of one kind in a sweep
through those entities in source order, the output tends to have all the types
in source order, then all the variables in source order, etc.

Long output lines are broken (between tokens) to avoid unduly stressing C
compilers that must process the generated code.

Since the generated code is intended to be fed into a C compiler, rather than
read by a human, it is not very readable.  The readability can be improved
(slightly) by turning on debugging (e.g., ``-d0`` on the command line); that
will cause the C-generating back end to generate annotation comments and
indentation.  Unreferenced entities, which ordinarily are not put out at all,
are generated inside ``#if 0``/``#endif`` delimiters in this mode.

Transformations
---------------

For the most part, what the C-generating back end does is just writing out the
C code that corresponds directly to the IL.  Since the IL has the same concepts
and constructs as the C language, the processing is pretty straightforward.
However, there are a few complicating transformations.

The following transformations on names are done:

* | Names that match C reserved words, or words that are likely to mean
    something to the underlying C compiler (e.g., ``unix``), are given a prefix
    of "``__x``".
* | Names with no linkage (e.g., local variables) are given a prefix that is
    the source sequence (line) number and column number of the name's
    declaration, to make sure they do not conflict with similar names from
    elsewhere in the program (e.g., "``i``" is output as "``__28_7_i``").
* | Unnamed entities are given generated names of the form "``__T``\ *nnnnnn*",
    with *nnnnnn* generated from the IL entry address.
* | The "``this``" parameter, passed through from C++, is given the name
    "``this``".
* | Function-local and prototype-scope types are given names formed by
    concatenating the original name, "``__``", the (mangled) function name,
    "``__``", and "``L``\ *nn*", where *nn* is a number identifying the
    function, prototype, or block scope scope of which the type is a
    member.  (Renaming is needed because such types are promoted out of
    functions; more on this below.)
* | When generating K&R C, file-scope static variables are given a suffix
    formed from the name of the module and the date and time of compilation.
    This suffix is needed because initialized file-scope static variables must
    be made external to avoid some pcc limitations on declaring static
    variables more than once.  The suffix helps to ensure that the now-external
    name does not conflict with like-named static and external variables from
    other compilation units.

Another set of transformations is necessitated by the fact that IL lowering
generates "C" IL that actually contains some minor extensions.  These
"extensions" are things that a real back end would probably take in its stride
(i.e., it wouldn't see these cases as being different than normal cases), but
which must be rewritten if standard C code must be generated:

* | Dynamic initializations for declarations can appear in mid-block, after
    executable statements have appeared.  (These are generated as executable
    code.  Because of this, "``const``" type qualifiers are suppressed on
    variables initialized dynamically and on all struct members.)
* | Structs and unions can have no members at all, and aggregate constants used
    in initializers can contain empty lists of constants ("``{}``").  (The
    struct/union definition will include a dummy field, and the initialization
    puts a zero into that field.)
* | Enumerations having no enumerator constants are put out as the
    corresponding integral type.
* | The ``eok_bassign`` operator is used to do assignment of arrays (e.g., in
    generated copy constructors).  (A ``memcpy`` is generated.)
* | Prototyped parameter variables may be unnamed.  (A name is generated.)
* | Variables can be indicated to be initialized to zero (the ``initk_zero``
    initialization kind).  This is used to differentiate real definitions of
    external variables from tentative definitions (a distinction that is hazy
    in C), and for local aggregate variables that are partially initialized.
    (For static variables, the initialization to zero is preserved in the
    generated C; for automatic variables, a ``memset`` is generated.)
* | String literals in C++ are ``const``.  IL lowering leaves them ``const``,
    even though string literals in C are not ``const``.  (This does not require
    a transformation in the generated C code: the string literals are put out
    as such, and the C compiler sees them as non-``const``, which works out
    okay.)

When generating K&R C, some transformations are needed to erase ANSI C features
that do not exist in K&R C:

* | Prototyped functions are put out as old-style unprototyped functions.
* | Type qualifiers ("``const``" and "``volatile``") are not put out.
* | Enumerations are put out as integral types, and enumeration constants are
    put out as integral constants.
* | "``signed``" is never put out.  "``signed char``" is put out as "``char``",
    and "``signed int``" is not used for the base type of a bit field.
* | "``long double``" is put out as "``double``".
* | The suffixes "``U``" on integral constants and "``F``" and "``L``" on
    floating constants are not put out; a cast is used instead.
* | Union initializations are rendered as executable code.
* | Initializations of automatic aggregates are rendered as executable code.
* | Bit fields in unions are put out as normal fields.
* | Signed bit fields are simulated by adding code to sign-extend and truncate
    the bit fields on each access.
* | Second and third operands of "``?``" of type ``void`` are rewritten.
* | ``extern void`` variables are put out with type ``char``.
* | Labels are given a prefix of "``__L_``" to accommodate the fact that pcc
    does not maintain a separate name space for labels.

The following transformations also deal with features missing in K&R C, but
these transformations are done even when generating ANSI C to avoid some
features that might not be implemented (well) in C compilers:

* | Rvalue field selections (e.g., "``f(x).i``") are rewritten as lvalue field
    selections by using a temporary.

  .. code:: c++

     struct A { int i; };
     extern struct A f();
     void m() {
       j = f().i;  /* Rewritten as "j = (Tnnnn = f(), Tnnnn.i)" */
     }

* | Wide string constants are stored in generated static variables and the
    variables are used in place of the constants at each reference.
* | Character arrays initialized with string constants not containing a
    trailing null character are rendered in "exploded" form, e.g., something
    like

  .. code:: c++

     int a[4] = "abcd";

  | is rendered as

  .. code:: c++

     int a[4] = {'a', 'b', 'c', 'd'};

The following transformations are necessary to deal with ordering problems
(i.e., they're necessary because of the order in which the C-generating back
end outputs entities):

* | Local types of functions are generated at file scope.  This avoids problems
    with extern declarations written inside functions (the extern entity is on
    the file-scope list, but it may refer to local types of the function).

  .. code:: c++

     void f() {
       struct A { int i; };
       extern void g(struct A);
     }

  | is rewritten as

  .. code:: c++

     struct A { int i; };
     extern void g(struct A);
     void f() {}

* | Types in the prototype scope of a function are promoted out of the
    prototype scope.  This is because each function and variable is put out
    twice, once as a declaration and once as a definition.  Without this
    promotion, the declaration and definition would be incompatible because the
    two copies of the prototype scope types would be considered to be distinct
    types.

  .. code:: c++

     void f(struct A { int i; } p) {}

  | is rewritten as

  .. code:: c++

     struct A { int i; };
     void f(struct A);
     void f(struct A p) {}

  | Prototype scopes that are not part of a function or variable definition are
    not processed in this way.  When generating K&R C, such unprocessed
    function types are put out unprototyped, which eliminates the types defined
    in the prototype scope.  When generating ANSI C, such unprocessed function
    types are put out prototyped, and if any structs, unions, or enums are
    defined in the prototype scope they are put out inline.
  |
  | When a pointer type containing a prototype scope needs to be put out (i.e.,
    as the type of a constant or in a cast), the type (and cast) is suppressed,
    because the conversion must have been implicit in the source program --
    it's not possible to name the types defined in a prototype scope outside of
    that scope.

When virtual functions with covariant return types appear in the source
program, IL lowering rewrites them in a way that uses wrapper functions.  That
is, when an overriding virtual function returns a pointer to a derived class,
and the overridden virtual function returns a pointer to a base, a wrapper
function is generated that calls the overriding function but returns a pointer
appropriate for the overridden function.  Such functions have a body that is
simply a return of an ``enk_result_of_overriding_function`` node cast to the
proper pointer-to-base-class type.  The C-generating back end expands these
wrapper functions by generating the body of the overriding function again, with
the added cast inserted over the expression at each return statement.  Note
that the "obvious" implementation technique of replacing the
``enk_result_of_overriding_function`` node by a call of the overriding function
(i.e., implementing these as actual wrapper functions) will not work if the
functions have variable-length argument lists.  The same technique is used for
alternate entry points for constructors in the IA64-ABI, but only if the
routine has a variable-length argument list.

Configuration
-------------

Although the generated C code is generally portable, there are some
configuration issues that must be attended to.

The most obvious is to make sure that the sizes and alignments of basic types
defined in ``targ_def.h`` match those assumed by the target C compiler.

Block operations (e.g., ``memcpy``) are generated in either the BSD form or the
System V/ANSI form under control of the ``__BSD__`` configuration option, so
make sure its setting matches the library of the target C compiler.

If the target C compiler has some way of indicating that a function is inline,
the code in ``dump_routine_decl`` should probably be changed to output that
construct.

Long output lines are wrapped to avoid overly long lines.  You can alter the
maximum size by adjusting ``MAX_OUTPUT_LINE_SIZE``.  The default is fairly
modest (a few hundred characters).

If the target C compiler has particular bugs or shortcomings, you may want to
add code to work around them.  There is existing code for the Sun, Microsoft,
and gcc C compilers.

The C-generating back end can emit ``#pragma`` directives for IL pragma
entries, but only in a somewhat limited way.  For pragmas bound to an entity,
the pragma is emitted right before the declaration of the entity.  However,
pragmas not bound to an entity are emitted at the beginning of the scope
containing them, and therefore they may not be where expected.  That makes that
kind of pragma suitable only in cases where the exact position of the pragma is
not very significant, e.g., for global-effect pragmas and those that associate
attributes with entities by naming the entities.

Operation
=========

The code can be compiled either as a subroutine ``back_end`` that is called in
the same program as the front end, or as a main program.  In either case,
``c_gen_be`` is called to do the C generation.  It begins at ``il_header`` and
walks through the intermediate language tree, generating C and writing it to a
file with a ``.int.c`` suffix.  First, it dumps the source correspondence
information (``dump_source_file_correspondence_info``) and some other header
code (``dump_header_code``).  Then, for the file scope level, it

* | calls ``dump_scope_constants`` to dump code for constants on the file-scope
    list.  This has marginal utility, but dumps class member constants (an
    extension) promoted to the file scope by IL lowering.  They are put out as
    ``enum`` declarations.
* | calls ``dump_scope_types`` to dump declarations for all named types --
    i.e., ``typedef``\ s and tags.  The tags are output twice: as incomplete
    declarations on a first pass, and as complete declarations on the second,
    to avoid forward-reference problems.  Local and prototype-scope types are
    also output here (at the file scope) to avoid some ordering problems.
* | calls ``dump_scope_routines`` to dump routine headers (no bodies).
* | calls ``dump_scope_variables`` twice.  The first time dumps variables
    without initializers, and tentative declarations for those with
    initializers.  The second time dumps only the initialized variables, this
    time with initializers.  This is to avoid forward-reference problems.
* | calls ``dump_scope_routines`` to dump routines with bodies.

When ``ONE_INSTANTIATION_PER_OBJECT`` is enabled, each instantiation is put
out as a separate object file.  There is still only one IL tree, but it is
marked so that the C-generating back end can sweep through it several times
and put out a different C file (a "slice") each time, each C file
containing one instantiated entity (function or static data member) plus
exactly the set of other entities needed by that entity.  The information
on which entities to include on each sweep is provided by the
per-instantiation "needed" flags, which are a bit vector attached to the
source correspondence entry (see :ref:`needed-flags`).  Each slice is
assigned one bit in that bit vector, and if the bit is 1 the associated
entity is needed in the slice.  On beginning the generation of the C file
for a slice, the C-generating back end sets the global variable
``needed_flag_bit_number`` to the bit number for the slice, and then goes
through the normal processing to generate C for everything in the IL tree.
The normal test of the "needed" flag, used to exclude things that aren't
needed, in this mode tests the proper bit in the bit vector, and therefore
excludes all entities that are not in the slice.  In addition, when a
variable or function with a definition is encountered, if the variable or
function is not assigned to the current slice (as indicated by the
``instantiation_needed_bit_number`` field in the variable or routine
entry), the variable or function is put out as a declaration instead of a
definition.

The lower-level routines are as follows:

``dump_type_decl`` generates a type declaration. It calls

* | ``dump_enum_definition``,
* | ``dump_struct_union_definition``, or
* | ``dump_typedef_decl``.

Types are written out by ``dump_type`` and
``dump_general_declaration_using_type``.  They use routines from
``il_to_str.c`` to produce the output for the type.  ``dump_enum_definition``
puts out enum definitions, and ``dump_struct_union_definition`` puts out
struct/union definitions.

``dump_variable_decl`` generates a variable declaration.  For a variable with
an initializer, it calls ``dump_initializer``.  There's some fairly complicated
processing under that routine to turn constant aggregate initialization into
executable code.  Among other things, it involves writing initialization
assignments to an auxiliary file while processing declarations, then copying
the initialization code to the right place after all the declarations have been
processed.  Because initialized static variables are put out twice (on two
calls of ``dump_variable_decl``), they are made external when generating K&R C,
so their names are modified (by ``dump_variable_name``) to avoid name conflicts
with like-named static and external variables in other compilation units.

Code generated for file-scope initializations (of unions when generating K&R C)
is placed in a separate routine (with a name beginning with "``__cgi__``") that
needs to be called at program start-up time.  If the compilation contains a
main program, or an initialization routine generated by IL lowering (with a
prefix of "``__sti__``"), a call to the C-generating back end initialization
routine is inserted there.  Otherwise, if the compilation is of C++ source, the
C-generating back end generates an initialization routine of the same form as
that generated by IL lowering ("``__sti__``" prefix), along with the variables
needed to alert the link phase to the presence of the initialization routine.
If the compilation is of C source, a message is issued to ask the user to
include an appropriate "``-i``" option elsewhere to request invocation of the
initialization code.

``dump_routine_decl`` generates a routine declaration.  For a routine with a
definition, it calls ``dump_routine_definition``.  That in turn calls
``dump_func_definition_type`` to dump the parameters (prototyped or old-style).

``dump_statement`` is called to generate code for statements.

The statements and expressions of functions are pre-scanned before code for the
routine is generated, to generate declarations for any temporaries that will be
needed in the generated code.  Specifically, temporaries are generated for
rvalue field selections and wide string constants.

Expressions are dumped by calling ``dump_expr``.  It outputs the expression by
walking the tree.  It puts in explicit parentheses where necessary to guarantee
the grouping of operators.  Taking the address of a function or array must also
be handled carefully, and is done by ``dump_ampersand``.

Since ``pcc`` doesn't allow signed bit fields, operations on such fields are
surrounded by references to macros to sign-extend them or truncate them for
storing.

``dump_constant`` dumps out a literal constant or address constant using the
IL-to-string routines in ``il_to_str.c``.

Actual output of generated code is handled by the following routines:

* | ``m_write_ch``,
* | ``write_ch``,
* | ``m_write_str``,
* | ``write_str``,
* | ``m_write_tok_ch``,
* | ``write_tok_ch``,
* | ``m_write_tok_str``,
* | ``write_tok_str``,
* | ``write_unsigned_num``, and
* | ``write_num``.

The "``m_``" versions are macros; the others are not.  One can choose between
them based on the need for speed and/or a single evaluation of the argument.
The "``tok_``" routines always write a full token (or several); that allows the
output line to be broken before or after the output if the line is too long.

``set_output_position`` is called to establish the source position of an entity
about to be generated.  If necessary, it generates a ``#line`` directive or the
right number of empty lines to adjust the output position (see
``write_line_directive``).
