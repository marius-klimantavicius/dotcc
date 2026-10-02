.. _templates:

=========
Templates
=========

Support for template declarations and the instantiation of classes and objects
based on templates is found principally in ``templates.c``; ``templates.h``
contains the associated declarations.

Overview
========

Template support is almost entirely a front-end issue.  By default, templates
themselves are not represented in the IL, and classes and functions that are
generated on the basis of templates look just like any other class or function.
The exceptions to this rule are:

* | Classes, functions, variables, and aliases generated from templates
    identify the associated template arguments, because those arguments may
    be needed in generating the name of the entity.
* | In automatic instantiation mode additional flags are provided in the
    variable and routine entries so that the information provided by the flags
    may be represented in the resulting object file in some form.
* | A template IL entry is created for each template declaration.  When
    ``PROTOTYPE_INSTANTIATIONS_IN_IL`` is TRUE, the entry points to a
    "prototype instantiation", which is an IL entry that represents the entity
    declared by the template.  When ``RECORD_TEMPLATE_STRINGS`` is TRUE, the
    entry contains a null-terminated string that represents the template
    declaration.  The IL template entry is also used to represent the value of
    a template template argument.
* | The template instantiation mechanism may require support in the back end.
    For example, if each instantiation is placed in its own object file, the
    back end must recognize the data structures provided to accomplish this.

Generally speaking, when a template definition is scanned, the definition of
the template is saved and then used later to generate instances of the
template.  There are two common techniques used by compilers to save the
definition of a template: in the first technique, the template is parsed
(without semantic analysis) and the parse tree is saved; in the second, some
unparsed (more or less textual) representation of the template is saved.  A
parse tree may be a reasonable choice for a multi-pass front-end featuring a
separate parser.  However, in the EDG C++ front end (which uses a single pass
approach in which parsing and semantic analysis are done together), a textual
representation, which we call a "token cache", is more appropriate.  Correctly
implemented, the two techniques are semantically equivalent.

When using exported templates (see :ref:`exported-templates`) the front end
reloads the translation unit(s) that define the exported templates that require
instantiation so that the information from all of the translation units is
available to the front end.  Even though there is a great deal of special
processing required to make exported templates work properly, the underlying
mechanism of rescanning tokens from a token cache is still used to produce the
actual instantiation.

Since the method one chooses to record the definition of the template affects
the data structures used in the implementation and also influences the
terminology used to describe them, it will be helpful to introduce some terms
that are used in the EDG C++ front end:

* | A *template* denotes a sequence of tokens that, along with one or more
    template parameters and a declarative context, is used as the basis for
    generating an *instance* of the template.
  |
  | A template entity may be

  * | a class template (e.g., class ``A``),
  * | a function template (e.g., function ``f``),
  * | a member function of a class template (e.g., ``A<T>::f``),
  * | a static data member of a class template (e.g., ``A<T>::t``),
  * | a nested class of a class template (e.g., ``A<T>::B``),
  * | a member class template (e.g., ``A<T>::C``),
  * | a member function template (e.g., ``A<T>::g``),
  * | a variable template (e.g., ``x<T>``),
  * | an alias template, or
  * | a partial specialization of a class template, member class template,
      or variable template.

  | For example,

  .. code:: c++

   template <class T> void f(T);       // function template
   template <class T> class A {        // class template
     T f() { return t; }               // member function of class template
     static T t;                       // static data member of class template
     struct B {};                      // nested class of class template
     template <class U> void g(U);     // member function template
     template <class U> struct C {};   // member class template
     template <class U> struct C<U*> {}; // partial specialization
     template <class U> static U vt;   // member variable template
   };
   template <class T> T x = 1;         // variable template
   template <class T> using B = A<T*>; // alias template

* | A *template parameter* refers to a template formal argument, and a
    *template argument* is a template actual argument.
* | A *template instance* is a class (sometimes called a *template class*)
    generated on the basis of a class template, an alias (i.e., typedef,
    sometimes called a *template alias*) generated on the basis of an alias
    template, a function (sometimes called a *template function*) generated on
    the basis of a function template, or a variable (sometimes called a
    *template variable*) generated on the basis of a variable template.  In
    addition, a member function or static data member of a template class is
    itself an instance of the corresponding member function or static data
    member of the class template.  For example,

  .. code:: c++

     template <class T> class A {
       static T t;
       T f() { return t; }
     };
     A<int> ai;
     template <class T> T g(T t) { return t; }
     int i = g(0);

  | With the declaration of ``ai`` an instance of class template ``A``, namely,
    ``A<int>``, is generated, and in the process instances ``A<int>::f`` and
    ``A<int>::t`` are also generated.  Similarly, the reference to ``g(int)``
    causes an instance of function template ``g`` to be generated.
* | In many cases instances of variable templates and static data members of
    template classes are treated similarly.  The term "template variable" is
    used to refer to both cases, unless otherwise specified.
* | The generation of an instance of a template is called *instantiation*.
    (Sometimes the result of instantiation, namely an instance, is also called
    an instantiation.)
* | An instantiation may be either a *partial instantiation* or a *full
    instantiation*.  A partial instantiation of a class template is like a
    declaration of an ordinary class when no body is supplied; similarly, a
    partial instantiation of a function template is like a declaration of a
    function without a function body.  On the other hand, a full instantiation
    is the generation of a class or function definition -- i.e., it includes
    the body.  For instance,

  .. code:: c++

     template <class T> class A {};
     A<long> *pal;
     A<int> ai;

  | In this case the declaration of ``pal`` causes the partial instantiation of
    ``A<long>`` -- that is, no body needs to be generated -- whereas the
    declaration of ``ai`` causes the full instantiation of ``A<int>``.  The
    instantiation of an alias template is always a full instantiation.
* | The first time a class template is scanned -- partly to record its member
    functions, static data members, nested classes, and friend declarations,
    and partly to check for errors -- the result is referred to as a *prototype
    instantiation* of the class template, and the class produced is called a
    *"nonreal" class*; in general, a template class based on template arguments
    that include at least one template parameter is called "nonreal".  In
    addition, a template class based on a template that is a member of a
    nonreal class is also called "nonreal".  Consider this example:

  .. code:: c++

     template <class T, int I> struct A {
       template <class T2> struct X {};
     };
     template <class T> struct B {
       A<T, 0> a;
       typename T::X<int> x;
     };
     B<A<int,1> > b;

  | In this example, when class template ``A`` is scanned, the prototype
    instantiation produces nonreal class ``A<T,I>``, and during the scanning of
    class template ``B`` nonreal classes ``A<T,0>`` and ``T::X<int>`` are
    generated.  Subsequently, when ``b`` is declared and ``B<A<int,1> >`` is
    instantiated, ``A<int,0>``, a real class, is instantiated to serve as the
    type for member ``B<int>::a``, and ``A<int,1>::X<int>`` is instantiated to
    serve as the type for member ``B<int>::x``.
* | A prototype instantiation is also performed for alias templates resulting
    in a nonreal alias.  Nonreal aliases are also created for instantiations of
    alias templates if the template argument list depends on a template
    parameter.  Unlike nonreal classes, nonreal aliases are actually
    instantiated by rescanning the tokens of the alias.
* | Nested classes of class templates are not themselves referred to as
    templates, but their member functions and static data members are,
    respectively, member functions of class templates and static data members
    of class templates, and a member function or static data member of a class
    nested within a template class is handled as an instance of the
    corresponding member function or static data member from the class
    template.  A class nested within a nonreal class is considered a nonreal
    class.
* | The definition of a template instance may be supplied explicitly by the
    user instead of being generated based on the template.  Such instances are
    referred to as *explicit specializations* of the template.
* | A class template or variable template may be *partially specialized*.  A
    partial specialization is an alternate version of the template that is used
    to generate instances whose template argument lists match a specified
    pattern.  For example, a partial specialization could be provided that will
    be used to generate all instances for which a specified template argument
    is a pointer type.

  .. code:: c++

     template <class T> struct A {};      // primary template
     template <class T> struct A<T*> {};  // partial specialization

  | The template that is specialized by a partial specialization is called
    the *primary template*.
* | Function templates may be overloaded with normal functions and with other
    function templates.  Overload resolution is used to select the function or
    template to be used for a given call.  If, as a result of overload
    resolution, there are two or more templates that could generate equivalent
    functions, *partial ordering* rules are used to select the template to be
    used.  Partial ordering is different from partial specialization in that
    there is no relationship between the function templates being ordered (i.e,
    one is not considered to be a specialization of the other).
* | In some modes, prototype instantiations are performed for entities other
    than classes and aliases.  When semantic analysis of nonclass templates has
    been requested, prototype instantiations are done for the bodies and
    default arguments of function templates and member functions of class
    templates, the initializers of template static data members, and the
    default values of template arguments.
* | Template template parameters are class templates for which the template
    definition to be used for a given reference is supplied by a template
    template argument.

By default, prototype instantiations and entries associated with nonreal
classes and aliases do not appear in the IL.  When
``PROTOTYPE_INSTANTIATIONS_IN_IL`` is TRUE, IL entries for prototype
instantiations are generated.  See :ref:`templates-in-the-il` for more
information.

Template definitions are typically placed in header files that are included by
the programs that make use of the templates.  Alternatively, the definitions of
template static data members and non-inline template functions may be provided
using the "export" facility of the C++ language (see
:ref:`exported-templates`).

The major part of template support falls into several phases.  The first is
processing the declaration of the template, which for a class template also
involves doing a prototype instantiation.  The second phase is recognizing a
reference to an instance of a template and producing a partial template
instantiation, which can be thought of as an on-the-fly declaration of the
instance.  The third phase is the full instantiation of the template instance;
typically, a template class is fully instantiated at the first point of
reference in a given translation unit, but a template function presents special
complications, since the full instantiation of a noninline template function
may occur only once in a program, no matter how many translation units it may
appear in.

Each of these phases of template processing will be examined in detail, but
first it is necessary to describe the data structures that are built to
represent templates and their instances.

Data Structures
===============

Each collection of tokens that is stored so that the tokens may be rescanned
later is represented by an entry of type ``a_template_cache``.  The fields of
the template cache entry are:

* | ``tokens``, where the tokens are kept on which an instantiation of the
    template is to be based.

  * | For classes the tokens comprise the base specifiers list as well as the
      body of the class.
  * | For functions, including member functions, they extend from the opening
      left brace to the closing right brace of the function body, except that
      for templates of constructors the *ctor-initializer* list, if present, is
      also included.
  * | For static data members the tokens identify the initializer, if any.

  | The last token in the token cache is always a ``tok_end_of_source`` token.
* | ``decl_info``, a pointer to an entry of type ``a_template_decl_info``,
    which describes the context in which the tokens stored in the token cache
    appeared.

The template declaration information entry (``a_template_decl_info``) in turn
includes the following fields:

* | ``parameters``, a pointer to list of entries of type ``a_template_param``;
    they record the template parameters declared in the template declaration.

  * | For type parameters a template param entry points to an ``sk_type``
      symbol (which in turn refers to a ``tk_template_param`` type entry).
  * | For nontype parameters a template param entry points to an
      ``sk_constant`` symbol (which refers to a ``ck_template_param`` constant
      entry).
  * | For template template parameters a template param entry pointed to an
      ``sk_class_template`` symbol.  The template symbol supplement for which
      the ``template_template_param`` field is TRUE.
* | ``declaration_scope``, identifying the ``sck_template_declaration`` scope
    in which the template parameter symbols were declared.
* | ``enclosing_scope``, a pointer to the scope containing the template
    declaration.  This is important, for example, when a function template is
    defined in a friend declaration within a class, in which case the class
    must be reactivated when instantiations of that function template are
    generated.
* | ``enclosing_template_decl``, a pointer to the template declaration
    information for an enclosing template declaration.  This is used for member
    class templates and member function templates when one needs to know, for
    example, not only the template parameters of the member template, but also
    the template parameter lists of any enclosing class templates.
* | ``decl_seq``, the declaration sequence number at the point of the template
    declaration.  This is used when doing dependent name lookup to exclude
    names not visible at the point of the template definition.
* | ``nondependent_calls``, used when doing dependent name processing to record
    the result of overload resolution for nondependent calls that were
    evaluated during a prototype instantiation.

A template (of whatever flavor) is represented by the combination of a symbol
and an associated entry of type ``a_template_symbol_supplement`` (defined in
``symbol_tbl.h``).  An instance of the template is then represented by an entry
that appears on a list pointed to from the template symbol supplement.  That is
the basic pattern for representing a template and its instances -- a template
symbol, a supplement, and a list of instances -- but each sort of template
presents a different version of the pattern.

The template symbol supplement contains a template cache entry for the template
body and also has fields that are specific to the kind of template it is
associated with; these are discussed below.

In most cases the instantiation of a template creates one or more classes,
functions, and/or static data members.  However, when a class template contains
members that are themselves templates (i.e, member function templates or member
class templates) the instantiation of the enclosing class template results in
the creation of entities that are templates.  When this happens, it is
necessary to record the template from which the new template was generated.
The template symbol supplement field named ``prototype_template`` contains a
pointer to the symbol associated with the template from which a given member
template was generated.  Similarly the prototype template contains a field
named ``subordinate_templates`` that points to a list of templates that were
generated from the prototype template.

Class Templates
---------------

An ``sk_class_template`` symbol represents a class template; it contains a
pointer to a template symbol supplement.  The supplement for a class template
includes the variant field ``instantiations``, a pointer to a list of the
symbols (of kind ``sk_class_or_struct_tag`` or ``sk_union_tag``) that represent
instances of the template.  The symbol for the prototype instantiation is
pointed to by the field ``prototype_instantiation``.

For each template class represented by a symbol on the instantiations list the
class symbol supplement has several important fields:

* | ``class_template`` is a pointer back to the ``sk_class_template`` symbol
    that represents the template upon which its instantiation is based.
    However, for a nested class of a template class the ``class_template``
    pointer will always be NULL.
* | ``corresp_prototype_sym`` is a pointer to the template class symbol for the
    prototype instantiation and is present for template class instances and
    classes nested within template classes.
* | ``is_nonreal_class`` is TRUE for template classes produced by prototype
    instantiations (and in general for template classes for which the template
    argument list contains at least one template parameter reference).  It is
    also TRUE for a class that is a nested class of a nonreal template class.

A class symbol for a template class (i.e., for an instance of a class template)
does not otherwise appear in the symbol table.  It cannot be looked up directly
but rather must be looked up via the class template symbol.

The type entry associated with a template class is like that of any other class
except that it contains the following additional information about template
classes:

* | ``template_arg_list`` points to the template argument used to generate the
    type (the template arguments are incorporated in the type so that back ends
    can use them to generate names for template classes).
* | ``is_template_class`` TRUE for template class instances and classes nested
    within template class instances.
* | ``is_nonreal_class`` is TRUE for nonreal instantiations and for local
    classes of prototype instantiations.
* | ``is_prototype_instantiation`` is TRUE for prototype instantiations of
    class templates and nested classes of class templates.
* | ``is_specialized`` is TRUE for template classes declared or defined in an
    explicit specialization declaration.
* | ``specialized_with_old_syntax`` was specialized, but not with the
    "``template <>``" syntax that is now required by the standard.
* | ``partial_spec_template_arg_list`` is used when a partial specialization is
    used to generate the definition of the template class.  It points to the
    template argument list that is specified with respect to the partial
    specialization's template parameter list.

A class template may have associated with it a set of partial specializations
that may be used instead of the primary template when generating full
instantiations of instances of the class template.  The template symbol
supplement contains a field named ``partial_specializations`` that points to a
list of ``sk_class_template`` symbols for any partial specializations that may
exist.  Instances created from partial specializations go on the instantiations
list of the primary template.  Each partial specialization has its own
prototype instantiation, which also goes on the instantiations list of the
primary template.  The class type supplement field
``partial_spec_template_arg_list`` is used in class types generated from class
template partial specializations (including the prototype instantiation).  It
contains the template argument list with respect to the partial specialization,
while the ``template_arg_list`` field contains the template argument list with
respect to the primary template.  In the following example, the template
argument list with respect to the primary template is "``<int*, int>``", while
the template argument list with respect to the partial specialization is
"``<int>``".

.. code:: c++

   template <class T1, class T2> struct A {};
   template <class T> struct A<T*, int> {};
   A<int*, int> a;

As mentioned previously, it is here that the type entries of real and nonreal
template classes differ -- the template arguments of a nonreal template class
involve at least one ``tk_template_param`` type or ``ck_template_param``
constant.  In addition, the type entry for a nonreal class does not appear in
the IL that is passed on to the back end for further processing.

When an instance of a class template is fully instantiated, partial
instantiations for all of its member functions, static data members, and nested
classes are generated.  Because these entities are not themselves templates,
they do not have their own template argument lists.  The template argument list
for their enclosing class is used when a full instantiation is performed.

Alias Templates
---------------

Alias templates are represented in much the same way as class templates.  An
``sk_class_template`` symbol is used to represent the alias template, however
the instantiation list and prototype instantiation fields of the template
symbol supplement point to ``sk_type`` symbols.  The ``is_alias_template``
field of the template symbol supplement is used to identify alias templates.

The typeref type created for instantiations of alias templates (with typeref
kind trk_is_template_alias) is like that of normal aliases except that it
contains additional information, including the template argument list and flags
such as ``is_template_alias``, ``is_nonreal``, and
``is_prototype_instantiation``.

Function Templates
------------------

An ``sk_function_template`` symbol is used to represent a function template,
including member function templates, but not member functions of class
templates.  It contains a pointer to a template symbol supplement whose variant
field ``instantiations`` points to a list of entries of type
``a_template_instance``: each time a partial instantiation is performed on the
function template, the resulting instance is recorded in this list.

Entries of type ``a_template_instance`` are also utilized to record instances
of member function of class templates and static data members of class
templates, but they are used somewhat differently in those cases (as described
later).  When a template instance is used to record an instance of a function
template, the following use is made of its fields:

* | ``instance_sym`` points to the ``sk_routine`` symbol that represents the
    instance.  That symbol will in turn point to the routine entry created to
    represent the instantiated function.  A full instantiation need not have
    been done.  The routine entry contains a pointer to the template arguments
    on which the instance is based.
* | ``template_sym`` points to the ``sk_function_template`` symbol that
    represents the template.
* | ``instantiation_required`` is TRUE if the routine instance was actually
    called or had its address taken.  It means that a *full* instantiation
    needs to be performed -- i.e., that the function body needs to be
    generated.  When this flag is set, the instance is placed on the
    ``instantiations_required`` list, to assure that it is checked for
    additional processing once the entire translation unit has been
    scanned.
* | ``is_guiding_decl`` is TRUE if this routine was explicitly declared by the
    user (whether or not a body was supplied).  When this flag is set, the
    symbol pointed to by ``instance_sym`` will have been added to the overload
    list for this name.  Note that a "specific declaration" is different than
    an "explicit specialization".  A specifically declared function
    participates in overload resolution, but need not be explicitly
    specialized.
  |
  | Here's an example:

  .. code:: c++

     template <class T> void f(T) {}
     void f(int);

  | The second declaration assures that ``f(int)`` is represented both in the
    instantiations list for function template ``f`` and on the overload list
    associated with an ``sk_overloaded_function`` symbol with an identifier
    ``f``.  Such a declaration affects how the function is found during
    overload resolution, but it does not prevent a body from being generated
    for it based on the function template.
* | ``explicit_instantiation`` is TRUE if the user has explicitly requested
    full instantiation by means of an explicit instantiation directive or
    ``#pragma instantiate`` directive.
* | ``referencing_namespace`` points to the namespace in which the use that
    first required the instantiation of the template was encountered, or NULL
    if the first reference was in the global namespace.  This field is set when
    the ``instantiation_required`` flag is set to TRUE.

In addition to the pointer to the instantiations list, the template symbol
supplement for a function template makes use of some other variant fields:

* | ``routine`` is a pointer to a routine entry.  This entry is generated when
    the function template declaration is scanned, but it doesn't actually map
    to a real routine (and does not appear in the IL proper).  Rather, it
    serves as a convenient place to store the routine type and other
    information about the function (e.g., the storage class, the special
    function kind, and whether inlining was specified in the original
    declaration).
* | ``func_info`` is a field of type ``a_func_info_block`` and stores
    information picked up during declaration processing that needs to be reused
    during instantiation (e.g., the names of the function parameters).
* | ``def_arg_expr_list`` is a list of entries containing information about
    default arguments to be applied to the function parameters when the
    instantiation is done.

Whereas, as noted previously, an instance of a class template cannot be looked
up independently of its template, the same is not necessarily the case for an
instance of a function template.  When there has been a guiding declaration,
the template function's symbol may be found during ordinary overload
resolution.  However, when there is no guiding declaration, the symbol can only
be found via the function template symbol and its list of instantiations.
Consider this example:

.. code:: c++

   template <class T> T f(T t) { return t; }
   char f(char);           // Guiding declaration
   int i = f(0);           // Instantiation of f(int)
   void f(int, int);       // Unrelated to the template

Symbols are arrayed under the symbol header for "``f``" as follows:

* | overloaded function symbol with overload list:
* | template function symbol ``f(T)`` with instantiation list:
* | instance entry pointing to routine symbol ``f(char)``
* | instance entry pointing to routine symbol ``f(int)``
* | routine symbol ``f(char)``
* | routine symbol ``f(int,int)``

Notice that the symbol for ``f(char)`` appears both on the overload list and on
the instantiation list (there are not two different symbols), whereas
``f(int)`` appears only on the instantiation list.  The former, consequently,
is subject to the ordinary lookup rules, whereas the latter must be looked up
using the stricter rules for template functions.  Guiding declarations have
been removed from the standard language, but are still supported by the front
end depending on the command line options and configuration flags being used.
When guiding declarations are disabled, declarations such as ``f(char)`` are
simply declarations of normal functions that are unrelated to the template.

Member Function of Class Templates
----------------------------------

A member function of a class template is represented by an
``sk_member_function`` symbol created during the prototype instantiation of the
class template.  These symbols look like ordinary member function symbols,
which among other things means they do not have their own pointer to a template
symbol supplement; rather they point to a template instance entry, and it has a
pointer to a template symbol supplement.

The problem being addressed with this somewhat roundabout arrangement is that a
member function of a class template is really a "quasi-template" whose partial
instantiation is in fact always part of any full instantiation of the class
template to which it belongs.  A consequence of this approach is that the
instantiations list from the associated template symbol supplement includes
both

#. ordinary instance entries that point to ordinary member function symbols
   (members of *real* instantiations of the class template) and

#. one special instance entry that represents the quasi-template by
   pointing to a member function symbol of the *nonreal* prototype
   instantiation of the class template.

The template instance entries for member functions are similar to those for
ordinary template functions:

* | ``instance_sym`` points to the ``sk_member_function`` symbol that
    represents the instance.
* | ``template_sym`` points to the ``sk_member_function`` symbol that
    represents the template.
* | When ``instance_sym`` and ``template_sym`` are identical, the
    ``template_info`` field is non-NULL and points to the template symbol
    supplement belonging to this member function of a class template.
* | The flags are used as for nonmember function instances, except that
    ``specific_decl`` is always TRUE.

Variable Templates
------------------

A variable template is represented by an ``sk_variable_template`` symbol.
Instantiations of the template result in variable entries and associated
``sk_variable`` symbols.  Variable templates declared in class scopes are, from
a language point of view, static data members, but they and their
instantiations are represented in the same way as non-member variable
templates.  When a variable template is instantiated using a dependent template
argument list, the ``is_nonreal`` flag is set in the variable entry.  The
``variable`` variant of the template symbol supplement contains information
used for both variable templates and static data members of class templates
(although some fields are only used for one or the other).  The
``instantiations`` field points to a list of symbols for the real
instantiations of the variable template.  Template instance entries are created
for the real instantiations of a variable template.

Static Data Members of Class Templates
--------------------------------------

Static data members of class templates are represented much like member
functions of class templates.  The template symbol, an
``sk_static_data_member`` symbol from the prototype instantiation of the class
template, points to an instance entry whose ``template_sym`` field points back
to it and whose ``template_info`` field points on to a template symbol
supplement.  The latter has a ``definitions`` field that points to the instance
list.  The fields in the instance entries are used just like those for member
functions.

Template Declaration Information
--------------------------------

As described earlier, each token cache that is used in the template
instantiation process is associated with a pointer to an entry of type
``a_template_decl_info``, which contains information about the template
parameters, the scope in which the template was declared, and a pointer to an
enclosing template declaration information entry, if applicable.  An enclosing
template declaration information pointer is used when a member function
template or member class template is declared within another class template.  A
function template, or member function of a class template, contains a token
cache that contains the function declaration and a separate token cache
containing the function body.  Each token cache has associated with it a
separate template declaration information pointer, because the two caches may
have come from different declarations with different template parameter lists,
etc.

There is one template declaration information entry for each ``template <...>``
clause that is scanned (except for those with empty parameter lists, which are
used in explicit specializations).  When there is more than one template clause
in a given declaration, the inner template clauses contain a pointer to the
enclosing template declaration clause (the one to the left of a given template
clause).

When a template is defined within another template the enclosing template
declaration information pointer is NULL indicating that the template
declaration information for the enclosing template should be obtained by
consulting its template symbol supplement.

Template Declarations
=====================

Overview
--------

``template_directive_or_declaration`` is the top-level routine for handling
template directives and template declarations.  It is called when the
``template`` or ``export`` keyword is encountered during declaration
processing.  This occurs for template declarations, explicit specialization
declarations, and explicit instantiation directives.  For example:

.. code:: c++

   template <class T> struct A {};       // template declaration
   export template <class T> void f(T){} // exported template declaration
   template <> class A<int> {};          // explicit specialization
   template class A<int>;                // explicit instantiation

When no template parameter list is present ``explicit_instantiation`` is
called.  See :ref:`explicit-instantiation` for further information.  When a
template parameter list is present, including an empty one (e.g., ``<>``),
``template_or_specialization_declaration`` is called.

It first calls ``cache_template_declaration`` to create a token cache
containing the entire template declaration, including both the template
parameter list(s) and the declaration that follows.  The definition of the
template (e.g., the class or function body) is not included in this cache.  An
initial scan of the tokens in the cache is done to determine whether the
declaration is a full specialization, and whether it is a friend declaration.

Template Parameter Clauses
^^^^^^^^^^^^^^^^^^^^^^^^^^

A template declaration or template specialization declaration contains one or
more "template parameter clauses".  A template parameter clause consists of the
``template`` keyword followed by a possibly empty template parameter list.  The
initial declaration of a template always has a single template parameter clause
(although a template declaration may be nested within another template
declaration).  Multiple template parameter clauses are used only when a member
template of a class template is defined or specialized outside of its class.

Each template parameter list has associated with it a template "nesting depth".
The nesting depth of the parameter list and the position within the template
parameter list are the attributes used when comparing two template parameters
to see if they represent the same parameter.  The names of the template
parameters are not significant.  The nesting depth and position are
collectively called the template parameter's "coordinates".

For example:

.. code:: c++

   template <class T1> struct A {
     template <class T2, class T3> void f(T1, T2, T3);
   };
   template <class X> template <class Y, class Z> void A<X>::f(X,Y,Z){}

where

* | ``T1`` and ``X`` both have a nesting depth of 1 and a position of 1
* | ``T2`` and ``Y`` both have a nesting depth of 2 and a position of 1
* | ``T3`` and ``Z`` both have a nesting depth of 2 and a position of 2

The nesting depth of a template parameter clause in a namespace scope is 1, a
template parameter clause declared within one enclosing class template is 2,
etc.  When scanning a template friend declaration any enclosing class templates
are ignored and the template parameter clauses are numbered beginning with 1.
This is necessary so that any types declared in the friend declaration match up
correctly with the template that is being made a friend.  For example, to make
the template in the example above a friend of another template, you would do
the following:

.. code:: c++

   template <class T> struct B {
     template <class X>
     template <class Y, class Z> friend void A<X>::f(X,Y,Z);
   };

The declaration of a template template parmeter also includes a template
parameter clause.  The template parameters in the template parameter clause for
a template template parameter have a nesting depth of zero.

For each nonempty template parameter list, the template parameters from the
parameter list are scanned.  This process includes the following steps:

* | An ``sck_template_declaration`` scope is pushed.  This will be the
    declaration scope to which the template parameter names belong.
* | ``scan_template_param_list`` is called to scan the template parameters and
    build a linked list of entries of type ``a_template_param`` to represent
    them.  Each template parameter has an associated symbol.  These symbols
    initially point to special "prototype argument" values that, for class
    templates, are used during the prototype instantiation.  During real
    instantiation these symbols are modified to point to the actual argument
    values to be used.

  * | A type parameter points to an ``sk_type`` symbol that initially points to
      a ``tk_template_param`` type.  The type symbol points to the actual
      argument type during an instantiation.
  * | A nontype parameter points to an ``sk_constant`` symbol that initially
      points to a ``ck_template_param`` constant.  The constant symbol points
      to the actual argument constant during an instantiation.
  * | A template template parameter points to an ``sk_class_template`` symbol.
      Unlike the type and nontype cases, the template template parameter symbol
      always points to the template symbol supplement for that template
      template parameter.  The ``argument_template`` field of the template
      symbol supplement points to the actual argument to be used during an
      instantiation.  Template template parameters differ from other template
      parameters in that they have certain properties that must persist through
      real instantiations.  For example, the default template argument values
      used for a template template parameter are always the ones specified in
      the template template parameter declaration, never those associated with
      the actual template template argument.
* | If the template parameter has a default argument, the type or expression is
    scanned on the spot if possible.  However, if the default argument depends
    on another template parameter, the default argument must be rescanned when
    its value is needed using the actual argument values for the template
    parameters on which it depends.  Therefore, its tokens are cached so that
    they may be scanned later (see ``prescan_default_arg_expr``).  There are a
    number of ways in which a default argument can depend on another template
    parameter:

  * | The type of a nontype parameter may make use of another template type
      parameter.
  * | The value of a default nontype argument may depend on a template type
      parameter or nontype parameter.
  * | The type used as the default argument for a type parameter may make use
      of another template parameter.

  | For example:

  .. code:: c++

     template <class T = int, class T2 = T, int I = 0, T J = I+1> class A {}

  | The default type of ``T`` and the default constant value for ``I`` can
    be known when the template parameters are scanned, but the default type
    of ``T2`` and the value for ``J`` are dependent upon whatever type
    ``T`` takes on, and the value of ``J`` depends on the value used for
    ``I``.

Classifying the Template Declaration
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

When processing specializations a distinction is made between a "full
specialization" and a "template specialization".  A full specialization is one
in which the entity being declared is a class, function, variable, or static
data member, while a template specialization is one in which the entity being
declared is still a template.  For example:

.. code:: c++

   template <class T> struct A {
     void f();
     template <class T2> void g(T2);
     template <class T2> struct B {};
     struct C {};
   };
   // Full specializations
   template <> void A<int>::f();
   template <> struct A<int>::C {};
   // Template specializations
   template <> template <class T2> void A<int>::g(T2);
   template <> template <class T2> struct A<int>::B {};

A partial specialization of a template is treated as a normal template
declaration, not as a template specialization.  A template specialization is a
template with one or more empty template parameter clauses.

Full specializations are handled by calling ``full_specialization``.  See
:ref:`explicit-specializations` for more information.  The rest of this
section describes ``template_declaration``, which is responsible for
processing template declarations and template specializations.

Earlier, a token cache was built that contains the template parameter clauses
and the declaration that follows them.  We are now at the point in the token
stream that marks the division between the two components.  Because the
declaration component must be prescanned to determine the kind of template
being declared, and because the declaration may be needed later for the purpose
of generating partial instantiations of functions, the existing cache is split
into a template parameter cache and a template declaration cache.  In certain
error cases, such as the presence of a syntax error in the template parameter
list, the template declaration may not have been cached correctly when the
initial cache was built.  When this occurs, the template declaration is
recached now that the true position of the beginning of the declaration is
known.

Next, ``is_class_template_decl`` is called.  If the declaration is in fact of a
class template, TRUE is returned.  If it is not a class template declaration,
FALSE is returned to signal that processing for other kinds of template
declarations should be done.

The template parameters are checked to ensure that they are compatible with any
previous declaration of the template, and for class members, that the template
parameter lists that correspond to enclosing class templates are compatible
with the template parameter lists of those class templates.

Class Templates
---------------

``class_template_declaration`` is called to process class template
declarations.  This includes normal class templates, partial specializations,
and template specializations.  It is also called to process declarations of
nested classes of class templates that are defined outside of the class.
``class_template_declaration`` performs the following functions:

* | Looks up and verifies the template name.
* | Creates the ``sk_class_template`` symbol and enters it into the symbol
    table, if necessary.  The symbols for partial specializations are not
    entered into the symbol table.
* | Calls ``create_prototype_type`` to create the symbol and type for the
    prototype instantiation, and to create the template argument list(s) for
    the prototype instantiation.
* | If a definition is provided, collects the tokens for the base class
    specifiers and the class body and caches them for use later.
* | For partial specializations, the template parameters of the partial
    specialization must appear in the template argument list associated with
    the partial specialization in such a way that their values can be deduced.
    In addition, nontype arguments cannot have types that depend on other
    template parameters, nor may they be used in expressions.  These error
    tests are accomplished by calling
    ``check_partial_spec_nontype_param_usage``.
* | If the template is a partial specialization,
    ``check_for_prior_use_of_partial_spec`` is called to determine whether the
    newly declared partial specialization would have been used to generate any
    existing full instantiations of the primary template, had it been declared
    when the full instantiation had been done.  If any such full instantiations
    are found, an error is issued.

Then the template declaration scopes that were pushed when the template
parameter clauses were scanned are popped, and ``instantiate_class_template``
is called to do a prototype instantiation -- i.e., to scan the template
definition even though the template parameters have not yet been given "real"
values.  Because declaration/expression disambiguation often cannot be done
based on dummy types, only declarative information is scanned; inline function
bodies and default argument expressions are cached and, in some modes, have
their prototype instantiations done later.  Most syntax errors and some
semantic errors are detected, but some errors cannot be reported until a real
instantiation is done.

The main benefit of prototype instantiation is to record the names and types of
member functions and static data members -- in effect, to create symbols to
represent member functions of class templates and static data members of class
templates.  ``instantiate_class_template`` accomplishes its work in the
following steps:

* | An ``sck_template_instantiation`` scope is pushed onto the scope stack.
* | The token cache for the class template body is activated.
* | ``scan_class_definition`` is called to process the tokens.  Special
    provisions are made for prototype instantiations:

  * | Diagnostics are suppressed where appropriate.
  * | ``decl_member_function`` and ``decl_static_data_member`` create template
      instance entries, template symbol supplements, and whatever else is
      needed to make the member symbols serve as templates.
  * | ``delayed_scan_fixup_for_class`` moves the tokens cached for member
      function bodies into the appropriate template symbol supplement; it also
      moves the entries describing default argument expressions onto a list in
      the appropriate template symbol supplement.  The cached tokens for
      inline-defined friend functions are discarded -- they will be picked up
      during real instantiation.
* | The starting and ending token positions of any nested classes, member
    functions whose definitions were provided in the class, and member
    templates (functions or classes) whose definitions were provided in the
    class are recorded.  These token positions will be used to remove the
    definitions from the token cache for the class template body.
* | The template instantiation scope is popped.
* | The bodies of nested classes, etc.  are removed from the class template
    body token cache.  Nested classes, member functions, and member templates
    may all be specialized.  In other words, the definitions provided in the
    class template definition may not be the ones actually used for a given
    instance of the class template, so there is no sense in scanning them while
    instantiating the class.  As with class templates and function templates,
    nested classes of class templates, member functions of class templates, and
    member class and function templates are only instantiated when needed, and
    may be specialized any time before the point at which they are first used
    in a way that requires their instantiation.

Alias templates
---------------

``alias_template_declaration`` is called to process alias template declarations
and performs the following functions:

* | Verifies the template name.
* | Creates the ``sk_class_template`` symbol and enters it into the symbol
    table.
* | Calls ``create_prototype_type`` to create the symbol and type for the
    prototype instantiation, and to create the template argument list(s) for
    the prototype instantiation.
* | Collects the tokens for the type defined by the alias and caches them for
    use later.

After calling ``alias_template_declaration``, ``template_declaration`` calls
``alias_prototype_instantiation`` to create the prototype instantiation of the
alias.

Function Templates
------------------

The above is what happens when ``is_class_template_decl`` does in fact
determine that the declaration is a class declaration.  However, if it returns
FALSE, ``template_declaration`` proceeds to examine what must be a template
declaration for something other than a class template.

If the template declaration appears within the definition of a class it must be
a member function template declaration or a template friend declaration.  If it
is a member function template declaration ``class_member_template_declaration``
(in ``class_decl.c``) is called to scan its declaration, then
``complete_function_template_decl`` is called to create the necessary template
data structures and to scan the template body, if present.

For all other cases, ``scan_template_declaration`` is called to scan the
declaration, which it does by calling ``decl_specifiers`` and ``declarator``.
If that produces a function type, the declaration is processed as a function
template or member function of a class template declaration; otherwise, it is
processed as a variable template or static data member of a class template
declaration or reported as an error.

``function_template_declaration`` is called for function templates.  It, in
turn, calls either ``decl_function_template`` (in ``decls.c``) or
``template_specialization``.

``decl_function_template`` does for member and nonmember template declarations
more or less what ``decl_routine`` does for ordinary function declarations.
The processing for nonmember function templates includes the following:

* | ``id_linkage`` is called to determine whether this is a redeclaration of a
    function template that is already in the symbol table and whether the name
    of the function template is overloaded.
* | If this is not a redeclaration, then:

  * | An ``sk_function_template`` symbol is entered into the symbol table
      (either directly or into an overload set).
  * | A routine entry is allocated and its fields are set to record the type,
      storage class, and so forth specified in the declaration.  It is not
      added to the IL, however, because it does not designate a "real" routine.

Member functions, for which a qualified name must be present, will already have
been declared in the class template definition.  Their processing includes a
call to ``member_function_redecl_sym``, which returns the member function
symbol with a matching type signature.

``decl_function_template`` returns to ``function_template_declaration`` an
already existing or newly created symbol that represents the function template.
Then, as with member function template declarations,
``complete_function_template_decl`` is called.

Completing the Function Template
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

The next step is to collect the tokens of the function body (including, for
a constructor, the *ctor-initializer*\ s) and store them in a cache in the
template symbol supplement.  For both member and nonmember function
templates the new template declaration information (parameter list, etc.)
is recorded in the associated template symbol supplement.

When doing nonclass prototype instantiations,
``function_prototype_instantiation`` is called to perform a prototype
instantiation on the body of a function template or member function of a class
template, and ``default_arg_prototype_instantiation`` is called to perform a
prototype instantiation on the default arguments of such functions.

Variable Templates and Static Data Member Templates
---------------------------------------------------

Variable template declarations can be either ``extern`` declarations, or are
definitions.  Static data member declarations are always definitions.  For
definitions, the template parameter list is recorded in the template symbol
supplement, and the tokens comprising the initializer, if any, are cached.  In
some cases, some or all of the initializer may have already been cached as part
of the template declaration.  In such cases, ``split_token_cache`` is used to
move the initializer tokens from the declaration cache to the initializer
cache; the remaining initializer tokens are then appended to the initializer
cache.

When doing nonclass prototype instantiations,
``variable_template_prototype_instantiation`` is called to perform a prototype
instantiation on the initializer of the variable template or template static
data member.

Variadic Templates
------------------

The C++11 standard adds "variadic templates," which are templates that take a
variable number of arguments.  For example:

.. code:: c++

   template<class ...T> void f(T ...args) {
     int a[] = {0, args..., 5};
   }
   int main() {
     f(1, 2, 3, 4);
   }

The template ``f`` can be called with zero or more arguments.  For a given
call, the parameter pack ``T`` is deduced to be a list of types, matching the
types of the arguments supplied, and ``args`` is a function parameter pack that
is a series of parameters of the types given by ``T``.

The front end implements variadic templates by storing, along with the tokens
of the template, information on the location and details of any pack references
and expansions.  When the template is instantiated, the cached tokens of its
declaration or definition are scanned, and in places where there are pack
expansions, the processing loops over a sequence of tokens, substituting a
different value for the pack name on each iteration, thus producing an expanded
version of the source text.

The language definition of variadic templates uses a syntax-based trick: in
contexts that allow a list of items (usually, a comma-separated list), an item
in the list can be suffixed by "``...``" to indicate that it is a pack
expansion.  In an instantiation of the template, that pack expansion is
replaced by, in effect, a list of entities generated by the expansion of the
pack.  So, in the above example, the aggregate initialization expands to the
equivalent of

.. code:: c++

     int a[] = {0, 1, 2, 3, 4, 5};

with the elements of the pack expansion inserted into the surrounding list.

Such expansions are allowed in parameter lists, call argument lists,
brace-enclosed aggregate initializers, base class specifiers,
*mem-initializer*\ s, template argument lists, exception specifications,
attributes, and capture lists.

The front end uses its own trick to expand these patterns.  In each of those
contexts, the code that handles the normal, non-variadic, case is enclosed in a
loop with the following general structure:

.. code:: c++

   a_pack_expansion_stack_entry_ptr pesep;
   a_boolean                        any_more;

   any_more = begin_potential_pack_expansion_context(&pesep);
   while (any_more) {
     a_pack_expansion_descr_ptr pedep;

     /* Normal processing for one item is done here. */
     ...
     pedep = end_potential_pack_expansion_context(pesep,
                                                  /*is_declarator=*/FALSE);
     if (pedep != NULL) {
       /* Mark the IL created as being for a pack expansion. */
     }  /* if */
     any_more = advance_to_next_pack_element(pesep);
   }  /* while */

When no pack expansions are present, the "``begin...``" call returns TRUE, the
"``end...``" call returns NULL, and the "``advance...``" call returns FALSE, so
the loop goes around exactly once and handles one item on the list.

When a pack expansion is present, in the prototype instantiation the
"``begin...``" call returns TRUE, the "``end...``" call swallows the "``...``"
indicating the pack expansion and returns a pointer to a structure that
describes the pack expansion, and the "``advance...``" call returns FALSE.
Again, the loop goes around exactly once, but information about the pack
expansion is recorded in the IL and in data structures attached to the
template.

In a real instantiation, the loop goes around as many times as the pack being
expanded has values.  Each time through, the source token position is reset to
the start of the expansion, the pack identifier is given an appropriate value
for that iteration, and the tokens are scanned again.  After the right number
of iterations, the "``advance...``" call returns FALSE and the loop exits.  If
the pack has zero elements, the "``begin...``" call advances the source token
position to after the "``...``" at the end of the expansion, and returns FALSE,
and the loop goes around zero times, thus producing no items on the list for
the pack expansion.

The pattern above appears in every place where a pack expansion could occur.
In a call argument list, for example, it must be done around the scanning of
every argument, which means in a lot of places.  (Or, at least, in a lot of
places when inside a template that has variadic parameters.) Almost all the
time, the variadic processing notes a starting position and starts a potential
expansion context, but then finds out later that there is no trailing "``...``"
and therefore no pack expansion, and discards the information saved.  This has
to be done, however, so that we can recall the starting position of a pack
expansion in the cases where one does occur.

The information saved for a pack expansion in the prototype scope (and,
incidentally, prototype instantiations are forced for variadic templates, so
that such information can be recorded) includes the starting position of the
pack expansion, so that, in a real instantiation, the "``begin...``" call can
look up the current token position and see whether a pack expansion starts at
the current token, and what the details of the expansion are.  The saved
information includes the list of pack names that were referenced and are to be
iterated over in the loop.  With that information, the "``begin...``" routine
can set up the proper looping over the proper sequence of tokens.

There are variant versions of these variadic-loop-management routines that are
used for expression rescan contexts (no source tokens are consumed; pack
identifiers are updated and the right number of iterations of the loop are
done) and prescan and disambiguation contexts (where we don't know yet the
context of the code, and therefore can't be certain about what pack expansions
mean, so we don't want to record anything permanent).

Template argument lists need to contain some extra information when variadic
templates are used.  For example, for a case like

.. code:: c++

   template<class U, class ...T> void f(U p, T ...args) {
   }
   int main() {
     f(1, 2.0, 3.0f);
   }

the complete deduced template argument list for ``f`` has ``int`` for ``U``,
and then a list of ``double`` and ``float`` for T.  In diagnostics, this is
displayed as ``<int, <double, float>>``.  Internally, it requires a way to
indicate that part of the list is a pack expansion.  That's the function of the
``tak_start_of_pack_expansion`` kind of template argument: it marks the start
of a sequence of template arguments associated with a pack expansion (and each
member of that sequence is marked with the ``is_pack_element`` flag).

That causes problems for lots of code in the front end that traverses template
argument lists.  In most cases, that code does not want to worry about
start-of-pack-expansion entries.  It just wants the "real" entries on the list.
That's what the traversal routines ``begin_template_arg_list_traversal``,
``advance_to_next_template_arg``, and the "``..._simple``" versions of those
routines do.  They allow easy set up of a traversal of a template argument list
that ignores the start-of-pack-expansion entries.  They should be used in
general to process template argument lists, to avoid problems.

Deduction does special processing to handle parameter packs, matching up
template parameters with multiple types, or nontype values, or template
template parameters.  Zero-length packs are a particular problem, in that they
are deduced from an absense of information (e.g., no arguments corresponding to
that parameter pack).

Overload resolution does special processing for parameter packs, and in
particular has to be careful because the normal one-to-one correspondence
between arguments and parameters does not always hold.

Initializer processing deals with pack expansions by using expresison caches.
The initializer routines call to fetch one expression.  If the expression
scanned is a pack expansion, the first expresison of the expansion is returned,
and the rest are pushed into an expression cache.  They are then returned on
subsequent calls at the same or a higher or lower level.  If the pack expansion
produces zero expressions, another expression is scanned, unless we hit the end
of the initializer (e.g., a closing brace).  In some cases an expression is
prescanned so we can decide whether the expression-list is over before going
down a path in the code that will expects to be able to fetch an expression.

Partial ordering and partial specialization processing also deal specially with
variadic templates.

Templates in the IL
===================

An entry of type ``a_template`` is created for each template declaration (see
``make_il_template_entry``).  By default, no information about the template
itself is preserved in the IL.  There are two different optional mechanisms
that can be used to record information about templates:

* | If ``PROTOTYPE_INSTANTIATIONS_IN_IL`` is TRUE, any prototype instantiation
    is recorded in the IL.  If nonclass prototype instantiations have been
    requested, prototype instantiations will be generated for each template
    entity declared; otherwise, prototype instantiations will be recorded only
    for class templates.  A prototype instantiation is an IL entry whose kind
    corresponds to the kind of entry being declared, but that represents the
    template itself and not any particular instantiation thereof.  In other
    words, it still contains references to the template parameters.  A type
    entry is used to represent a class template, a routine entry is used to
    represent a function template, and a variable entry is used to represent a
    variable template or template static data member.
* | If ``RECORD_TEMPLATE_STRINGS`` to TRUE, the IL template entry will point to
    a null-terminated string that reproduces the text (minus comments, after
    preprocessing) of the template declaration in the source program (see
    ``make_template_string``).  If a template has more than one declaration
    (e.g., a nondefining declaration followed by a definition), an IL template
    entry is produced for each declaration.

Both of these mechanisms can be used at the same time.  Note that while all
templates can be represented as template strings, only those templates that
make use of the standard-mandated ``typename`` and ``template`` keywords
can have prototype instantiation IL entries generated (because without such
keywords it is not possible to parse the template definitions in the
absence of a set of actual template argument values).  See
:ref:`templates-in-the-il` for more information.

Partial Instantiation
=====================

Partial instantiation occurs automatically for nested classes, member
functions, and static data members when the class of which they are members is
instantiated.  That is to say, the full instantiation of a class necessarily
entails the implicit declaration of all its members: symbols are introduced
into the symbol table, type entries are created as required, IL entries are
created and added to the IL, and so forth.  The partial instantiation of class
templates and function templates, on the other hand, is triggered by a
reference.

Class Templates
---------------

When a class template reference -- *class-template-name*\ ``<``\
*template-arg-list*\ ``>`` -- is encountered, a call is made to
``coalesce_template_class_reference`` to scan the template argument list and
look up the reference.  Having verified that types in the template argument
list correspond to type template parameters and constants correspond to nontype
template parameters, it builds a linked-list representation of the template
arguments.  If there are more template parameters than template arguments and
if the template parameters have default values, entries are supplied for the
default values as well, whether from a saved constant value or after rescanning
the cached tokens comprising the default argument expression.

The template argument list is then passed to ``find_template_class``, which
searches among the instantiations that already exist for the class template,
calling ``equiv_template_arg_lists`` to compare the template arguments scanned
for the current reference against the template arguments associated with
existing instantiations.  If an instance with matching template arguments is
located, the symbol for it is returned.  If no match is found, a new symbol is
created (see ``make_template_class_sym`` in ``symbol_tbl.c``) and added to the
instantiations list for the class template.  A new class type is also created,
but, since this is a partial instantiation, no body is generated for it yet.

A reference to an instance of a class template *always* causes a partial
instantiation, but not all references cause full instantiation.  Partial
instantiation occurs during the *lexical* scan of a template class reference,
whereas the decision whether to do full instantiation occurs during *semantic*
processing.  Partial and full instantiation are always separate steps in
template processing.

Note that a partial instantiation can be generated on the basis of an
incomplete class template.  For example:

.. code:: c++

   template <class T> class A;
   A<int> *x;

Even though no body has been supplied for class template ``A``, a partial
instantiation of ``A<int>`` can be produced.  On the other hand, ``A<int>`` is
an incomplete (and incompletable) type, and so an error would be issued if it
were used in a context that required a complete type.  Similarly, partial
specializations are not relevant when a partial instantiation is generated.
They are only considered when a full instantiation is done.

Function Templates
------------------

A function template may be referenced in several different ways:

* | as a function call,
* | by taking the address of a function,
* | and by declaring a function in an explicit specialization, explicit
    instantiation, or friend instance declaration.

In each case, determining which function template is to be used, and which
instance of the template is being referenced, requires the evaluation of an
explicitly specified template argument list, deducing template arguments from
the function template's parameters, or some combination of the two.  This
process involves the following steps for each of the candidate function
templates:

* | If an explicit template argument list was specified,
    ``substitute_template_arguments`` is called.  Note that an explicit
    template argument list is completely scanned before
    ``substitute_template_arguments`` is called.  When it is scanned, the
    number of parameters, and their types are not known.  Consequently, the
    template argument list needs to be converted into one that matches the
    template parameter list of the candidate template.  This is done by calling
    ``create_initial_template_arg_list``.  If the specified template arguments
    cannot be converted to the types of the corresponding parameters, or if
    there are more template arguments than template parameters, no template
    argument list is created and the candidate template is disqualified.  Note
    that an error is not issued because there may be another candidate template
    that is actually the one intended to be used.
* | Once an appropriate template argument list has been produced,
    ``substitute_template_arguments`` calls ``copy_type_with_substitution`` to
    create a function type that represents the function type of the candidate
    template with the template parameters that correspond to the explicitly
    specified template arguments replaced with the specified template argument
    values.  If the substitution process would result in an invalid type (e.g.,
    a pointer to a reference type, or an array of void) the candidate template
    is disqualified.
* | Template argument deduction is done using the substituted function type
    created above in an attempt to deduce the template arguments that were not
    explicitly specified.  Template argument deduction is done by calling
    ``matches_template_type``, which is described below.
* | If template argument deduction succeeds,
    ``wrapup_function_template_argument_deduction`` is called to verify that
    values have been deduced for all of the template parameters, and that the
    types of any deduced nontype parameters match the types of the
    corresponding template parameters.  ``copy_type_with_substitution`` is
    called once again to create a new function type in which all of the
    template parameters have been replaced with the corresponding template
    argument values.

When the reference is from a function call, the routines described above are
called from the overload resolution routines.  When the reference is from the
address of a function or from a declaration, these routines are called by
``is_match_for_function_template``, which in turn is called from either
``matching_template_function`` or ``has_matching_template_function``.

There are several routines in ``templates.c`` that directly and indirectly
support the process described above:

* | ``matches_template_type`` is designed to be called successively to build a
    template argument list while checking for a match between a type from the
    function template routine type and a type from a new routine type; it also
    calls itself recursively to apply the processing to type trees.  When a
    template parameter type is found, the "real" type with which it is paired
    is recorded in the corresponding template argument entry -- unless it
    already has some other type, in which case an inconsistency has been found.
    As long as there are no inconsistencies or type incongruities, this process
    of augmenting the template argument list continues; otherwise,
    ``matches_template_type`` returns FALSE to halt further processing and
    invalidate the template argument list built thus far.
  |
  | After ``matches_template_type`` has indicated that each of the arguments
    match the template, ``verify_template_nontype_args`` must be called to
    verify that nontype parameters whose types depend on a template parameter
    are consistent with the deduced types.  It also supplies types for nontype
    parameters that are deduced entirely from array bounds.
* | ``compare_function_templates`` is used to determine whether one function
    template is *more specialized* than another based on the partial ordering
    rules for function templates.  It is called by the overload resolution
    routines to compare two function templates that are otherwise identical
    from the point of view of overload resolution.
* | ``add_to_partial_order_candidates_list`` is used in contexts other than
    overload resolution in which a function template must be selected from a
    set of function templates using the partial ordering rules.  This is done
    when taking the address of a function that is determined to be an instance
    of a function template, and when the lookup of a conversion operator name
    results in the partial instantiation of a template (e.g., when ``operator
    int*`` is a reference to an instance of a function template such as
    ``operator T*``).  After this routine has been called for each candidate
    template, ``select_best_partial_order_candidate`` is called to return the
    best matching template.
* | ``find_template_function`` is passed a function template symbol and a
    template argument list.  After checking for the use of a local type among
    the template arguments (an error), it goes through the instances that
    already exist for the template, calling ``equiv_template_arg_lists`` until
    a match is found.  If none of the existing instances match the new template
    argument list, it creates a new instance by calling
    ``make_template_function``, to which the new template argument list is
    passed.
* | ``is_match_for_function_template`` is passed a function template symbol and
    a new routine type.  It returns TRUE if the type fits the type signature of
    the template and FALSE if not.  First, obvious mismatches are detected,
    such as mismatches in the number of function parameters.  Next, if an
    explicit template argument list was specified, the template arguments are
    compared with the corresponding template parameters.  Type arguments must
    be associated with type parameters, nontype arguments must be associated
    with nontype parametersm, and the specific nontype value specified must
    match the type of the nontype parameter or must be capable of being
    converted to the type of the nontype parameter.  It then tries to find
    among the instances of the template one whose type signature exactly
    matches the new routine type.  If it finds such an instance, it returns
    TRUE, along with the symbol it found.  Otherwise, it makes successive calls
    to ``matches_template_type`` in an attempt to construct a template argument
    list based on any explicitly specified template arguments, the template
    parameter types, the template routine type, and the new routine type; if
    this is successful, it returns TRUE, along with the new template argument
    list, but otherwise it returns FALSE.
* | ``matching_template_function`` is passed a function template symbol and a
    routine type.  It calls ``is_match_for_function_template`` which checks for
    a match between the routine type and the template.  If there is a match but
    no instance already exists, it creates a new instance by calling
    ``make_template_function``, to which the routine type is passed.
* | ``has_matching_template_function`` is similar to
    ``matching_template_function`` in that it determines whether a given
    routine type matches a potential instance of the specified template, but it
    does not create an instance of the template.  It is primarily used when
    issuing diagnostics, to choose the most appropriate of several possible
    messages.  In the following example, the error "specializing overloaded
    function ``"A::f"`` requires ``template <>`` syntax" is used because the
    function type matches the template declared in the class.

  .. code:: c++

     struct A {
       template <class T> void f(T);
     };
     void A::f(int) {}

* | ``make_template_function`` is the routine that actually does the partial
    instantiation.  When it is passed a non-NULL routine type, it simply uses
    that type; otherwise, it activates the declaration token cache (see the
    ``decl_token_cache`` field in the template symbol supplement) and calls
    either ``scan_member_declaration`` or ``scan_template_declaration``, for
    member and nonmember function templates, respectively, to assure that
    template arguments are correctly substituted for template parameters in the
    new routine type; and then, if there are default arguments, it calls
    ``delayed_scan_for_function_template_default_args``.  In addition, it
    creates the ``sk_member_funciont`` or ``sk_routine`` symbol entry, the
    routine entry, and the template instance, setting all their pointers and
    other fields appropriately.  The symbol is not entered directly into the
    symbol table -- it is accessible to lookup via the instantiation list only.

Variable Templates
------------------

A reference to a variable template results in a full instantiation of the
instance if a definition is available.  The only exception is when the variable
template is used in a declarative context (i.e., to declare an explicit
specialization of the variable template).  In a partial instantiation the
initializer is not scanned.

Full Instantiation
==================

Full instantiation of a template means providing an instance of the template
with a defining body (or, in the case of a static data member, a definition
with an implicit or explicit initializer).  Typically, this occurs
automatically, on the basis of the template provided.  However, it can also
occur when the user provides an explicit specialization definition.  Both cases
are discussed in this section.

.. _explicit-specializations:

Explicit Specializations
------------------------

Two forms of explicit specialization are supported by the front end: the new
explicit specialization syntax (e.g., ``template <> ...``), and the old form
that used the normal declaration syntax.  The new form is accepted in all
modes, while acceptance of the old form is controlled by a command line option.
Member function templates and member class templates may only be explicitly
specialized using the new syntax.

For example:

.. code:: c++

   template <class T> void f(T);
   template <class T> class A {};
   template <> class A<int>;       // new specialization declaration
   template <> class A<int> {};    // new specialization definition
   class A<char>;                  // old specialization declaration
   class A<short> {};              // old specialization definition
   template <> void f(int);        // new specialization declaration
   template <> void f(int){}       // new specialization definition
   void f(char);                   // guiding declaration
   void f(double){}                // old specialization

Explicit specializations that use the new syntax begin with one or more
template parameter clauses in which the parameter list is empty.  An explicit
specialization can be a declaration or a definition.  The language requires
that an explicit specialization be declared before its first use in a
translation unit.

When using the old syntax, only classes can be declared as specializations.
For all other kinds of entities, specializations can only be supplied as
definitions.  A function declaration that looks like an old-style
specialization declaration is actually what is known as a "guiding
declaration", whose only purpose is to cause a template instance to be treated
like a normal function for overload resolution purposes.  There are no member
guiding declarations.

Old-style explicit specializations look like normal class, function, and static
data member declarations and are handled by the same routines that handle
normal declarations.

``full_specialization`` processes new-style explicit specializations and is
called after the template parameter clauses have been scanned.
``decl_specifiers`` and ``declarator`` are called to scan the declaration that
follows the template parameter clauses.  If the entity being specialized is a
class, the call to ``declarator`` is bypassed.  ``full_specialization`` then
identifies the template instance that is being specialized.

Class Templates
^^^^^^^^^^^^^^^

An explicit specialization of a class template occurs when the explicit
specialization syntax is used, or when old specializations are enabled, when an
ordinary class definition is provided in which the class name is a template
class reference, for example:

.. code:: c++

   template <class T> class A {};
   template <> class A<short> {};
   class A<int> {};
   A<char> x;

In this example the bodies for ``A<int>`` and ``A<short>`` are the ones
provided by the user's explicit definitions.  The bodies for all other
instances of template class ``A``, such as ``A<char>``, are based on the
template definition.

The processing of class template explicit specializations is done within
``class_specifier``, even when the new specialization syntax is used.  This is
necessary because only when ``class_specifier`` is called is it possible to
determine whether the class is being specialized or whether the class specifier
is part of an elaborated type specifier in some other kind of declaration.

When the template class reference is encountered as a tag name (see
``scan_tag_name``), a partial instantiation is done automatically and the
symbol is entered on the instantiations list of the class template.  Then
``class_specifier`` marks the class as being explicitly specialized by setting
the ``is_specialized`` flag in the type entry, and adds the definition, if one
has been supplied.  Because the ``is_specialized`` flag is set, no subsequent
automatic instantiation will be attempted.

The same processing applies to both class templates and member class templates.
The processing for nested classes of class templates is basically the same
except that, as with other members of class templates, the partial
instantiation of the nested class is done when the enclosing class is
instantiated.

Function Templates and Member Functions of Class Templates
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

An instance of a function template may be explicitly specialized using the
explicit specialization syntax, or when old specializations are enabled, by
providing a function definition that happens to match the type signature of a
function template.  For example:

.. code:: c++

   template <class T> void f(T) {}
   template <> void f(char);         // explicit specialization
   void f(int) {}                    // old explicit specialization
   void f(int,int} {}                // Unrelated to the template

In both the old and new style specializations, the ``is_specialized`` flag in
the routine entry is set, which suppresses the full instantiation of the
template.  The old-style specialization of a function also acts as a guiding
declaration for overload resolution purposes.

When the new specialization syntax is used, ``find_matching_template_instance``
is called to find the template instance that matches the routine type returned
by ``declarator``.  This is done not only for instances of function templates
and member function templates, but also for member functions of class
templates.

Processing for old-style explicit specializations follows the track for
ordinary function definitions.  The primary difference appears in
``find_linked_symbol`` (called from ``decl_routine`` via ``id_linkage``), which
searches for previous declarations of the entity being declared.  If the
ordinary search fails to turn up a matching function but does find a function
template with the given name, a call is made to ``matching_template_function``,
which extends the search to instances of the template function and, as
described above, creates a new one when appropriate.  In other words, this
search in ``id_linkage`` may have the effect of doing a partial instantiation
of a new template function.  Once the instance symbol has been found (or
created by means of a partial instantiation), attaching the function definition
to the routine involves no special processing.

It can happen that an old-style explicit specialization of a function template
precedes the declaration of the template itself.  For example:

.. code:: c++

   void f(int) {}                 // Specific definition
   template <class T> void f(T) {}

When the function template declaration is encountered,
``decl_function_template`` goes back through existing functions of the same
name and calls ``record_predeclared_template_function`` for each -- if the
function turns out to match the new template, it is retroactively entered as an
instance.

Old-style explicit specializations of member functions or static data members
of instances of a class templates involve little special handling beyond what
is normally done for definitions of member functions and static data members of
nontemplate classes.  Here's an example:

.. code:: c++

   template <class T> class A {
     void f(T);
     static T s;
   };
   void A<int>::f(int) {}
   int A<int>::s = 0;

The reason no special processing is required is that the full instantiation of
``A<int>`` entails the partial instantiation of its members ``f(int)`` and
``s``.  After that it's just a matter of marking the symbols as ``defined`` and
adding the function body or initializer.

Variable Templates
^^^^^^^^^^^^^^^^^^

The use of a variable template in an expression context results in a full
instantiation if a definition is available.  If a definition is not available,
the normal instantiation processing is done as for template functions and
static data members of template classes (e.g., an instantiation could be done
later in the translation unit if a definition was provided, depending on the
instantiation mode).  Even though a full instantiation may be done, an external
definition of the instance may or may not be emitted depending on the
instantiation mode.

Automatically Generated Definitions
-----------------------------------

Class Templates
^^^^^^^^^^^^^^^

The full instantiation of a template class occurs at the first point in the
translation unit where a reference to the instance requires it to be a complete
type.  In other words, it is done as soon as necessary, but no sooner.  For
example:

.. code:: c++

   template <class T> class A {};
   A<int> *p;                   // Partial instantiation of A<int>
   A<int> a;                    // Full instantiation of A<int>

The first declaration involving ``A<int>`` does not require that it be a
complete type, but the second one does, and so that is the point at which the
full instantiation occurs.  At points in the front end where a complete type is
required, the macro ``complete_class_type_is_needed`` is invoked, resulting, if
appropriate, in a call to ``instantiate_template_class`` to generate the full
instantiation.

Partial Specializations
~~~~~~~~~~~~~~~~~~~~~~~

A class or variable template may be partially specialized.  A partial
specialization is a version of the template that should be used for certain
instantiations.  The template to be used is selected based on the template
argument list on which the instantiation is to be based.

When a full instantiation is required, ``check_partial_specializations`` is
called to match the template argument list of the class being instantiated with
each of the partial specializations that have been declared for the template.
``matches_partial_specialization`` is called to perform the actual matching.

The matching is done using the same process and routines that are used to do
function template argument deduction (see ``matches_template_type`` above).  If
the template parameters for a given partial specialization can be deduced from
the actual template arguments, then the template class matches the partial
specialization.  For example:

.. code:: c++

   template <class T1, class T2> struct A {};              // #1
   template <class U1, class U2> struct A<U1*, U2> {};     // #2
   template <class V1>           struct A<V1, V1> {};      // #3
   template <class X1>           struct A<X1*, short> {};  // #4
   A<int, char> a1;  // uses primary template (#1)
   A<int*, int> a2;  // uses #2
   A<int, int> a3;   // uses #3

The declaration of ``a1`` does not match any of the partial specializations, so
the primary template is used.  The declaration of ``a2`` matches the partial
specialization labeled #2 because ``U1`` and ``U2`` can be deduced from
``<int*, int>`` (as ``int`` and ``int``, respectively).  Partial specialization
#3 is used when the two template arguments are the same, as in the declaration
of ``a3``.

Each partial specialization that matches the argument list is added to a list
of candidate partial specializations using the routine
``add_to_partial_order_candidates_list``, which compares the new entry with any
entries already on the candidates list.  The comparison is done by
``is_more_specialized``, which determines whether a given partial
specialization is "more specialized" than another.  If the entry being added is
not as specialized as an entry already on the list, the new entry is not added.
If the new entry is more specialized than an entry already on the list, that
entry is removed from the list.  After all of the partial specializations have
been evaluated, the candidates list contains a set of partial specializations
that match the template argument list of the class being instantiated and which
are unordered relative to one another.  If there are no entries on the list,
the primary template is used to generate the class.  If there is exactly one
entry on the list, that partial specialization is used to generate the class or
variable.  If there is more than one entry on the list, the instantiation is
ambiguous and an error is issued.

.. code:: c++

   A<int*, int*> a4;  // ambiguous - could be #2 or #3
   A<int*, short> a5; // uses #4

The declaration of ``a5`` uses partial specialization #4, even though both #2
and #4 match.  #4 is preferred because it is "more specialized" than #2.
Whether a given partial specialization is more specialized than another is
determined in much the same way in which a partial specialization is determined
to match a given actual template argument list, i.e., though use of the
argument deduction routines.  In the example above, if the argument list of #4
was considered to be the actual argument list and the argument list of #2 was
considered to be the parameter list, the parameters of #2 could be deduced as
``U1=X1`` and ``U2=short``.  If the roles of the two partial specializations
are reversed, making #2 the actual argument list and #4 the formal parameter
list, the type deduction fails.  Therefore, #4 is considered more specialized
than #2.

Generating the Template Class
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

Once the appropriate template has been selected (i.e., either the primary
template or a partial specialization), the body of the template class is
generated.  These are the steps:

* | The ``pending_instantiations`` value in the template symbol supplement for
    the class template is incremented.  This allows the compiler to catch and
    report "runaway recursive instantiation," as the following example would
    produce:

  .. code:: c++

     template <int I> class A { A<I+1> a; };
     A<0> x;

  | Once the pending instantiations counter reaches
    ``max_pending_instantiation`` (a global variable that can be set by a
    command-line option, and whose default is defined in ``lang_feat.h``), an
    error is issued and the recursion is halted.
* | ``push_template_instantiation_scope`` is called to push a template
    instantiation scope onto the scope stack and to do some special processing
    for template instantiations:

  * | Pointers to the template symbol, the instance symbol, and the template
      argument list are recorded in the scope stack entry.
  * | ``depth_innermost_instantiation_scope`` is updated with the current scope
      stack depth.
  * | ``update_template_param_symbols`` is called to bind the symbols for
      template parameters to the values in the corresponding template
      arguments.  For example:

    .. code:: c++

       template <class T, int I> class A {};
       A<int,0> x;

  | By default the ``sk_type`` symbol for template parameter ``T`` refers to a
    ``tk_template_param`` type entry and the ``sk_constant`` symbol for ``I``
    refers to a ``ck_template_param`` constant.  But during the instantiation
    of ``A<int,0>`` -- that is, while the template instantiation scope is
    active -- any reference to ``T`` or ``I`` should be translated into a
    reference to the value of the corresponding template argument, namely, to
    ``int`` or ``0``, respectively.  This is done by modifying the pointers in
    the symbols; the default pointers are restored when the template
    instantiation scope is popped.
* | The cache containing the tokens for class template definition is activated.
* | ``scan_class_definition`` is called.  In most respects processing is
    identical to the processing for any class definition.  The differences have
    mainly to do with recording member functions and static data members as
    instances:

  * | ``find_corresp_prototype_tag_sym`` is called to find the symbol for the
      prototype instantiation of the class.  (When a nested class within a
      template class is scanned, the symbol returned is the corresponding
      nested class of the prototype instantiation.) That symbol is required for
      calling the two routines described next.
  * | ``find_member_function_template`` is called for member functions of the
      new template class.  It locates the template symbol (i.e., the symbol for
      the corresponding member function in prototype instantiation), creates a
      template instance entry for the new member function, and adds it to the
      instantiation list of the template.  The template argument list for the
      new template class serves for the instance, too.
  * | ``find_variable_member_template`` is called for variable templates and
      static data members of the new template class.  The processing is similar
      to what is done for member functions.
  * | ``find_function_template_member`` is called for member function templates
      of the new template class.  The processing is similar to what is done for
      member functions.
  * | ``find_class_template_member`` is called for member class templates of
      the new template class.  The processing is similar to what is done for
      member functions.
  * | ``set_nested_template_class_symbol_info`` is called for nested classes of
      the new template class.  This call is made by ``class_specifier`` when
      the nested class is first encountered.  The processing is otherwise
      similar to what is done for member function.
  * | ``delayed_scan_fixup_for_class``, called at the end of scanning the class
      definition, provides somewhat special handling for template classes.  For
      inline-defined member functions the function bodies are not scanned at
      this time; such functions are only instantiated if they are called, so
      that errors are not issued on functions that are not actually used.
      Also, ``delayed_scan_for_function_template_default_args`` is called to
      process the default arguments on the function parameters: the token cache
      to be used is accessed through the template symbol supplement.
* | ``set_instantiation_required_for_template_class_members`` is invoked; for
    each member function and static data member, both of the template class and
    of any classes nested within the template class, it calls
    ``update_instantiation_required_flag``.
* | ``pop_template_instantiation_scope`` is called to pop the template
    instantiation scope.  The template parameter symbols and
    ``depth_innermost_instantiation_scope`` are restored to the values they had
    before the scope was pushed.  (Note that the possibility of recursive
    instantiation prohibits restoring these values to default values.)
* | The ``pending_instantiations`` value in the template symbol supplement for
    the class template is decremented.

Alias Templates
^^^^^^^^^^^^^^^

When an instance of an alias template is referenced, ``find_template_class`` is
called to look for a previously created template alias or instantiate a new
alias if no previously created instance exists.  Template aliases are
instantiated by ``instantiate_template_alias``.  Note that unlike class
templates, there are no partial instantiations of alias templates

Function Templates
^^^^^^^^^^^^^^^^^^

The timing for the instantiation of member and nonmember template functions is
partly controlled by the user (as discussed in the next section).  A function
may end up being instantiated

* | at the point of first reference in a given translation unit (e.g.,
    functions declared ``inline``);
* | at the end of the translation unit; or
* | externally to the current translation unit.

Wherever it occurs, it is ``instantiate_template_function`` that is called to
generate a function body based on a template.  These are the steps in the
process:

* | The function's storage class and name linkage are set, along with its
    ``is_inline`` flag if required.
* | The ``pending_instantiations`` value in the template symbol supplement is
    incremented.  As for class templates, this field is used to catch runaway
    recursive instantiation.
* | ``push_template_instantiation_scope`` is called to push a template
    instantiation scope onto the scope stack.
* | The token cache is activated.
* | ``push_scope`` is called again to push a function scope onto the scope
    stack.
* | On the basis of the ``param_id`` list saved in the template symbol
    supplement the function parameters are entered into the symbol table (in
    the function scope) by ``decl_parameter``.
* | For constructors a call is made to ``ctor_initializer`` to process the
    *ctor-initializer* list, if any, as well as to do default processing,
    and for destructors a call is made to ``dtor_initializer``.
* | In case a function is being instantiated "on the fly" in the midst of
    executable code, the structured statement stack is suspended and a new one
    begun by a call to ``new_struct_stmt_stack``.
* | ``compound_statement`` is called to process the tokens comprising the
    function body.  The IL produced is recorded in the function scope.
* | If appropriate, ``restore_struct_stmt_stack`` is called.
* | ``pop_scope`` is called for the function scope.
* | ``pop_template_instantiation_scope`` is called for the template
    instantiation scope.
* | The ``pending_instantiations`` value in the template symbol supplement for
    the class template is decremented.

Variable Templates and Static Data Members of Class Templates
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

``instantiate_template_variable`` is invoked to handle full instantiation of
static data members and for partial and full instantiations of variable
templates.  It performs the following:

* | Static data members are marked as defined.  Variable templates are marked
    as defined if the declaration is a definition.
* | If an explicit initializer was specified in the static data member template
    declaration, then:

  * | ``push_template_instantiation_scope`` is called to push a template
      instantiation scope onto the scope stack.
  * | The token cache is activated.
  * | The type is scanned to determine the type of a template variable, or to
      make sure the type agrees with the declaration in the class for static
      data members.
  * | For a full instantiation, if a definition is available, ``initializer``
      is called to process the tokens comprising the initializer.
  * | ``pop_template_instantiation_scope`` is called for the template
      instantiation scope.
* | If no initializer was specified for a defined variable template or static
    data member, ``def_initializer`` is called.

Constrained Templates
=====================

C++20 introduces the notion of "constrained templates", which is often referred
to as "concepts" (although concepts are just one feature in support of
constrained templates).  For example:

.. code:: c++

   template<typename T> requires(sizeof(T)<4*sizeof(int*))
     int hash(T *p);

Constraints are boolean predicates.  In the example above, it's just a normal
expression.  Whenever template instance is considered (e.g., for overload
resolution or for a partial instantiation), its constraint predicates are
substituted and evaluated: If the predicates evaluates to ``false``, the
template is discarded.  For functions and partial specializations, that may not
be an error (assuming other candidates satisfy their constraint or are
unconstrained), but other cases will then elicit an error that will indicate
which constraint failed to be satisfied.

Concepts
--------

A constraint expression can be abstracted through a *concept template*.  For
example:

.. code:: c++

   template<typename T> concept SmallHashable = sizeof(T)<4*sizeof(int*);

After that definition, the declaration of hash above can instead be rewritten
as:

.. code:: c++

   template<typename T> requires SmallHashable(T)
     int hash(T *p);

or

.. code:: c++

   template<typename T>
     int hash(T *p) requires SmallHashable(T);

or

.. code:: c++

   template<SmallHashable T> int hash(T *p);

or even

.. code:: c++

   int hash(SmallHashable auto *p);

Although concept template are a kind of template, they are not (partially or
fully) instantiated.  Instead, they are "substituted" somewhat like function
templates are substituted during the deduction process.

Concept templates are represented in the IL using ``a_template`` entries of
``templk_concept`` kind, and in the symbol table using ``sk_concept_template``
symbols (that point to ``a_template_symbol_supplement`` entries with no variant
fields).  A ``templk_concept`` template points to the predicate, which is
represented as an ordinary expression tree (``an_expr_node`` structure).

Concept templates can also be "invoked" directly to produce a boolean value.
For example:

.. code:: c++

   auto cond = SmallHashable<X>;

Such expressions are represented using ``enk_concept_id`` nodes: They point to
a ``templk_concept``\ ``a_template`` entry and, optionally, to a list of
template arguments.

Representation of constraints
-----------------------------

As hinted above, there are four possible syntactic locations for constraints:

* | A concept name used to introduce a template type parameter.  Such
    constraints -- called *type constraints* -- are represented as
    ``enk_concept_id`` nodes pointed to by the
    ``a_template_param_type_supplement`` associated with the constrained
    template parameter.  The first template argument for such
    ``enk_concept_id`` nodes are implied (it's the template parameter being
    constrained) and that is indicated by the ``is_type_constraint`` flag in
    the ``enk_concept_id`` node.
* | A *requires clause* following a ``template<...>`` header, as in our
    original example.  Such constrains are represented using
    ``a_requires_clause`` entries pointed to from ``a_template_decl``,
    itself pointed to from ``a_template``.  ``a_requires_clause`` is itself
    mostly a structure pointing to an expression tree.
* | An implicit template type parameter introduces by a so-called "abbreviated
    function template declaration", as in the example ``int hash(SmallHashable
    auto *p);`` above.  This case is represented very much as the first case
    (concept name used to introduce a template type parameter): By having the
    associated ``a_template_param_type_supplement`` entry point to an
    ``enk_concept_id`` node representing the constraint.  However, in this
    case, the ``a_param_type`` entry corresponding to the parameter that
    introduced the constrained also has the flag ``is_auto_param`` set to TRUE.

* | A trailing requires clause following the function declaration itself, as we
    showed in the second variation of our example:

  .. code:: c++

     template<typename T>
       int hash(T *p) requires SmallHashable(T);

  | This is represented by having the ``a_routine`` entry for the prototype
    instantiation of the function template point to an entry of type
    ``a_requires_clause``.  It's worth noting that this variation of a
    constraint can also be applied to ordinary member functions of class
    templates (hence the need to point to it from an ``a_routine`` entry rather
    than a template-specific entry).

  | To ease the description of constraints, C++20 also introduces
    *requires-expression*\ s (not to be confused with *requires-clause*\ s,
    which were described above).  For example:

.. code:: c++

   template<typename T> void f() {
     bool b = requires { typename T::node; };
   }

Such expressions are represented by an ``enk_requires`` node, which points to a
list of expression nodes: Entries on that list that are of kind
``enk_type_operand``, ``enk_compound_req``, and ``enk_nested_req`` are of
special significance.  See ``il_def.h`` and ``expr.c`` for details.

Important Constrained-Template Functions
----------------------------------------

The following are some functions that are central to the handling of
constrained templates:

* | ``scan_requires_clause`` and ``scan_concept_expression`` (both in
    ``expr.c``).  These are the functions that parse constraint expressions,
    which requires special treatment of the logical operators (``&&`` and
    ``||``).
* | ``scan_requires_expr`` (``expr.c``), which parses *requires-expression*\ s.
* | ``check_template_constraints`` (``templates.c``), which determines if a
    given template's constraints are satisfied by a given set of template
    arguments.
* | ``requires_clause_satisfied`` (``exprutil.c``).  This function performs
    substitution of constraint expression, which is subject to various subtle
    rules.  It leans heavily on the expression rescanning facilities (see
    :ref:`rescanning-exprs`).
* | ``compare_constraints`` (``exprutil.c``).  When determining the partial
    order of function templates or partial specializations, constraint can play
    a special role.  This function, is the main function for comparing the
    constraints on templates (to determine whether a template is more or less
    constrained than another, or maybe neither is more constrained than the
    other).  The algorithm for this comparison is relatively complex and
    described in great detail in ``exprutil.c``.

Managing Instantiation of Functions, Variables, and Static Data Members
=======================================================================

As noted previously, the full instantiation of template classes occurs as soon
as necessary, but no sooner.  The timing for the automatic instantiation of
template functions, variables and members of template classes is more
complicated.

For example, if a member function of a template class is not referenced,
there is no reason to instantiate it.  But even if it is referenced, its
instantiation may occur elsewhere.  The EDG C++ front end supports several
methods whereby the user can manage the instantiation of templates:
automatic instantiation, instantiation modes that are specified on the
command line, and explicit instantiation directives included in the source
code of the program.  Explicit instantiation directives are supported in
both the form specified by the C++ standard and as ``#pragma`` directives.
For a detailed description of how the instantiation modes and pragmas are
used, see :ref:`template-instantiation`.


The ``instantiation_required`` flag in template instance entries is set to
indicate that a function, variable, or static data member has been referenced
in such a way as to require its definition (i.e., full instantiation) somewhere
in the program, even if not in the current translation unit.  For example,
calling a template function causes the flag to be set.

``update_instantiation_required_flag`` is called to set the flag and to place
the instance on a linked list (see ``add_to_instantiations_required_list``) so
that it can be readily found later, when the actual instantiation is done.
Sometimes the routine to do full instantiation is called immediately, however
(e.g., for calls to template functions that are declared as ``inline`` or when
in the midst of instantiation wrapup processing); in such cases the instance is
not placed on the list.

It is ``do_any_needed_instantiations``, called by
``template_and_inline_function_wrapup``, as part of the ``fe_wrapup`` process,
that goes through the ``instantiations_required`` list.  It determines for each
instance whether full instantiation is actually required in the current
translation unit as a result of the instantiation mode being used, explicit
instantiation and ``#pragma`` directives found in the program (see
``should_be_instantiated``, which checks instantiation modes and directives),
and whether the instantiation has been assigned to the current translation unit
by the automatic instantiation mechanism.  It then will call
``instantiate_entity``, if appropriate.

Instantiations are normally generated as part of the object file of the
translation unit in which the instantiations are performed.  But when "one
instantiation per object" mode is specified, each instantiation is placed in
its own object file, and the names of those object files are written to the
template information file (which is described below).  One instantiation per
object mode is useful when generating libraries that need to include copies of
the instances referenced from the library.  If each instance is not placed in
its own object file it may be impossible to link the library with another
library containing some of the same instances.  Without this feature it is
necessary to create each individual instantiation object file using the manual
instantiation mechanism.

.. _template-auto-instantiation:

Automatic Instantiation
-----------------------

The EDG C++ front end provides a complete prototype implementation of an
automatic instantiation mechanism.  The EDG automatic instantiation mechanism
is a "linker feedback" mechanism.  It works by providing additional information
in the object file that is used by a "prelinker" to determine which template
entities require instantiation so that the program can be linked successfully.
Unlike most aspects of the front end the automatic instantiation mechanism is,
by its nature, dependent on certain operating system and object file format
properties.  In particular the prelinker is a separate program that makes use
of the UNIX ``nm`` command to access information about the symbols defined in
object files.  Furthermore it relies on features implemented in the ``eccp``
driver script.  Consequently, this code should be viewed as a sample
implementation that may need adaptation to work on other systems.

This section describes the steps involved in automatic instantiation.  The
steps are described in the sequence in which they would occur while compiling
and linking an application and not necessarily the sequence in which they occur
in a given execution of the front end.  While one might expect the two
sequences to be the same the automatic instantiation mechanism may, in fact,
compile a given file a number of times while determining the set of
instantiations that are needed to link the application.

Initial Compilation
^^^^^^^^^^^^^^^^^^^

When a program is compiled, the front end generates a set of flags that are
provided to the back end and are ultimately either output as a separate file by
the front end or are encoded in some form in the generated object file.  The
flags are stored in the IL variable and routine entries.

* | The ``can_be_instantiated`` flag indicates that this translation unit is
    capable of generating an instantiation for a given function, variable, or
    static data member (hereafter referred to as a "template entity").  This is
    used by the prelinker to determine which source files are possible
    instantiation sites for the template entity.  This flag is set even if the
    translation unit provides an instantiation of the template entity.
* | The ``instance_required`` flag indicates that a definition of the template
    entity must be supplied by some translation unit (but not necessarily this
    one).  Both the ``instance_required`` and the ``can_instantiate`` flags
    indicate that the entity is one that can potentially be defined by a
    generated template instantiation.  This information allows the prelinker to
    distinguish between an unresolved template reference and a plain undefined
    symbol.  These flags are also used by the prelinker to distinguish class
    specializations, whose members cannot be defined by generated
    instantiations, from regular template classes whose members may be defined
    by generated instantiations.
* | The ``do_not_instantiate`` flag indicates that instantiation of the
    template entity has been explicitly prohibited by use of the
    ``do_not_instantiate``\ ``#pragma`` directive.  When automatic
    instantiation is used, the ``do_not_instantiate`` pragma prohibits
    instantiation not only in the file in which the pragma was included but
    anywhere within the application with which the resulting object file is
    being linked.

In the default configuration, these flags are written to the template
information file, which is descibed below.  In earlier versions of the front
end these flags were translated by IL lowering into tentative definitions of
specially named symbols.  The old behavior can be selected using the
``INSTANTIATION_FLAGS_IN_TEMPLATE_INFO_FILE`` configuration flag.  The flags
were removed from the object file because the amount of space consumed by the
names of the flags could be excessive, because the mangled names of template
entities can be very long.  When the flags are generated as part of the object
file, the symbol names include a code that indicates the flag kind and also
includes the name of the symbol being described.  The special symbol includes a
prefix of ``__CBI__`` (can be instantiated), ``__TIR__`` (template instance
required), or ``__DNI__`` (do not instantiate) followed by the mangled name of
the entity.  For example, a template function of type ``void f(int)`` could
have the following flags generated.

.. code:: text

   __CBI__f__fi__TIR__f__fi__DNI__f__fi``

Because they are generated as tentative definitions, they may each appear in
any or all of the object files being linked into an application while only
appearing once in the generated executable file

When a template class contains noninline virtual functions, the instantiation
mechanism must ensure that the virtual function table is defined.  IL lowering
does this by generating a "template instance required" flag for the first
noninline virtual function of any template class, because it's the presence or
absence of a definition for that function that controls whether or not IL
lowering puts out a definition for the virtual function table.  By requiring
the first noninline virtual function to be defined we ensure that the virtual
function table will also be defined

The Template Information and Instantiation Request Files
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

Information is exchanged between the front end and prelinker using the
"template information" and "instantiation request" files.  These files are also
known respectively as the ``.ti`` and ``.ii`` files [#f1]_.  The template
information file is used for infomation produced by the front end for use by
the prelinker.  The instantiation request file is produced by the prelinker for
use by the front end.  The template information file was added in version 2.37
of the front end to support features such as one-instantiation-per-object mode,
and generation of instantiation flags in a separate file instead of the object
file.  The front end can be configured to not use a template information file
in order to be compatible with earlier versions of the front end, but this
requires that the features that use the template information file be disabled.

The template information file provides several diferent kinds of information to
the prelinker.  Each line of the file contains a type code followed by a
string.  The type code indicates the type of information being provided.  The
following types of lines are generated by either the front end or driver:

* | ``cmd:`` command line (supplied by the driver)
* | ``dir:`` compilation directory (supplied by the driver)
* | ``fnm:`` compilation file name (supplied by the driver)
* | ``idn:`` instantiation directory (in one-instantiation-per-object mode,
    supplied by the driver)
* | ``flg:`` instantiation flags (when not encoded in the object file)
* | ``ifn:`` instantiation file names (in one-instantiation-per-object mode)
* | ``stu:`` secondary translation units (supplied by the driver)
* | ``tnm:`` export templates defined in this file
* | ``dep:`` dependency information when exported instantiations have been
    generated

The order in which these are specified is not, in general, significant except
that the prelinker currently requires that the instantiation directory entry
precede any instantiation file name entries.

The template information and instantiation request files have the same base
name as the source file, but use special suffixes specified by configuration
parameters in ``host_envir.h``, with the default values indicated above.

At the end of each compilation the front end determines whether any template
entities were referenced in the translation unit.  If so a template information
file is created (or an instantiation request file, if template information
files are not being used).  If no template entities were referenced in the
translation unit the file will not be created and any existing file will be
removed.  When template information files are being used, the instantiation
request file is also removed if the template information file is removed.

After invoking the front end the driver checks for the existence of either the
template information or instantiation request file, depending on the
configuration.  If one exists, the driver updates it with the command line used
to compile the source file, the name of the current directory when the
compilation was done, and the name of the file that was compiled.  This
information is used later if the prelinker needs to recompile the file.

Prelinker
^^^^^^^^^

Once a complete set of object files has been generated, including the
appropriate flags, the prelinker is invoked to determine whether any new
instantiations are required or if any existing instantiations are no longer
required.  The command line arguments to the prelinker include a list of input
files to be analyzed.  The input files are the object files and libraries that
constitute the application.  The prelinker begins by looking for template
information files for each of the object files (or instantiation request files,
if template information files are not being used).  If no template information
(or instantiation request) files are present, the prelinker concludes that no
further action is required.

If there are template information (or instantiation request) files, the
prelinker reads the template information file and the current instantiation
list from the instantiation request file.  The instantiation list is the list
of instantiations assigned to a given source file by a previous invocation of
the prelinker.  The prelinker uses the UNIX ``nm`` command to produce a list of
the global symbols that are referenced or defined by each of the input files.
The prelinker then simulates a link operation to determine which symbols must
be defined for the application to link successfully.

When the link simulation has been completed, the prelinker processes each input
file to determine whether any new instantiations should be assigned to the
input file or if any existing instantiations should be removed.  The prelinker
goes through the current instantiation list from the instantiation request file
to determine whether any of the existing instantiations are no longer needed.
An instantiation may be no longer needed because the template entity is no
longer referenced by the program or because a user supplied specialization has
been provided.  If the instantiation is no longer needed, it is removed from
the list (internally; the file will be updated later) and the file is flagged
as requiring recompilation.

The prelinker then examines any symbols referenced by the input file.  The
responsibility for generating an instantiation of a given entity that has not
already been defined is assigned to the first file that is capable of
generating that instantiation.

If a given file contains instantiations of exported templates, the template
information file contains dependency information.  If a source file that is
part of an exported template translation unit changes, the dependency
information is used to detect such a change so that the prelinker can recompile
the file and regenerate the instantiations of the exported templates.

Recompilation
^^^^^^^^^^^^^

Once all of the assignments have been updated, the prelinker once again goes
through the list of object files.  For each, if the corresponding instantiation
request file must be updated, the new file is written.  Only source files whose
corresponding instantiation request file has been modified will be recompiled.

At this point the combination of the template information and instantiation
request files contains the information needed to recompile the source file, and
a list of instantiations assigned to the source file (in the form of mangled
function, variable and static data member names).

Then, during recompilation the front end reads the entries from the
instantiation request file produced by the prelinker and checks them against
entries on its own "instantiations required" list.  Whenever a match is found,
based on comparing the mangled name from the instantiation request file and the
name returned by either ``get_mangled_function_name`` or
``get_mangled_variable_name`` (in ``lower_il.c``), the
``automatically_instantiated`` flag is set in the template instance entry.

When the prelinker invokes the front end, it provides a "definition list file",
which contains a list of all the external definitions found in the object files
and libraries specified on the prelinker command line.  The front end reads the
definition list file and determines whether each of the entries on its
instantiations required list has already been defined elsewhere.  If an entity
is found that is not defined elsewhere, its ``automatically_instantiated`` flag
is set.

Later, in ``instantiation_wrapup``, instantiations will be generated for all
instances for which the ``automatically_instantiated`` flag is set, whether as
a result of being included in the instantiation request file or "adopted" by
the front end when it was determined that the instance was not defined
elsewhere.  A list of the entities adopted by the front end is written to the
definition list file.  This list is used by the prelinker to add the adopted
entities to the adopting translation unit's instantiation request file.

The definition list file permits the front end to perform instantiations that
have become necessary as a result of other instantiations that were assigned by
the prelinker, without requiring that the prelinker invoke the front end an
additional time to perform those instantiations.  This reduces the number of
iterations of the prelinker and front end that are required to generate a
complete set of instantiations for a program.

If an error occurs during a recompilation, the prelinker exits without updating
the remaining information files and without attempting any additional
compilations.

Iteration and Termination
^^^^^^^^^^^^^^^^^^^^^^^^^

If all recompilations complete without error, the prelink process is repeated,
since an instantiation can produce the demand for another instantiation.  This
prelink cycle (finding uninstantiated templates, updating the appropriate
instantiation request files, and dispatching recompilations) continues until no
further recompilations are required.  [#f2]_

When the prelinker is finished, the linker is invoked.  Note that simply
because the prelinker completes successfully does not assure that the linker
will not detect errors.  Unresolvable template references and other linker
errors will not be diagnosed by the prelinker.

In many cases, once the application has been successfully linked, the
recompilation overhead will have become a one-time-only expense, because the
next time the source file is compiled, the instantiation request file will
provide precisely the right list of instantiations to be done.  Recompilation
will be required again only if the instantiation request file is invalidated,
that is:

* | if additional instantiations come to be needed (in this or another file);
* | if existing instantiations are not longer referenced;
* | if a different set of command-line options is used to compile the file; or
* | if the instantiation request file is deleted.

Except in such cases, once the complete set of instantiation request files has
been generated, the prelinker should not need to force recompilation for any
source files in the application.

Implementing Alternative Automatic Instantiation Mechanisms
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

The default automatic instantiation implementation uses the instantiation
request file as described above.  The routines in ``templates.c`` are designed
to permit an alternative mechanism to be used (e.g., querying a database) to
determine whether a given entity should be instantiated.  This may be done by
modifying the following routines:

* | ``init_auto_instantiation_information`` is called at the beginning of the
    automatic instantiation processing.  The default version calls
    ``read_auto_instantiation_information``, which opens the instantiation
    request file and reads its contents.
* | ``check_if_entity_should_be_automatically_instantiated`` is called for each
    instantiatable entity (external function, variable, or static data member).
    It sets the ``automatically_instantiated`` flag if the entity is to be
    instantiated in this translation unit, and the ``add_to_request_file`` flag
    if the instantiation was "adopted" by this translation unit and must be
    added to the instantiation request file by the prelinker.  The default
    version calls ``check_if_present_in_request_file`` to determine whether the
    specified entity was named in the instantiation request file and/or the
    definition list file.
* | ``wrapup_auto_instantiation_information`` is called by ``fe_wrapup``.  The
    default version calls ``create_or_remove_instantiation_information_file``
    and ``close_or_remove_template_info`` file.

Instantiation Modes
-------------------

The instantiation mode is specified by a command line option and determines
whether the compiler should automatically generate instances of template
entities (function templates, variable templates, member function of template
classes, and static data members of template classes.) The command line option
is used to set the global variable ``instantiation_mode`` to one of the
following values:

.. list-table::

   * - | ``tim_none``
     - | Do not automatically create instantiations of any template entities.
         This is the default.
   * - | ``tim_used``
     - | Instantiate those template entities that were used in the compilation.
         This will include all static data members for which there are template
         definitions.
   * - | ``tim_all``
     - | Instantiate all template entities declared or referenced in the
         compilation unit.  For each fully instantiated template class, all of
         its member functions and static data members will be instantiated
         whether or not they were used.  Nonmember template functions will be
         instantiated even if the only reference was a declaration.
   * - | ``tim_local``
     - | Similar to ``tim_used`` except that the functions are given internal
         linkage.  This is intended to provide a very simple mechanism for
         those getting started with templates.  The compiler will instantiate
         the functions, variables and static data members used in the
         compilation as local (static) functions and local (static) variables.

Inline functions are usually instantiated when they are first referenced, and
so unreferenced inline functions are usually not instantiated.  However, in
``tim_all`` mode they are always instantiated -- either during
``instantiation_wrapup`` or, for member functions for which the body is
supplied in the class template definition, when the class is instantiated.

``#pragma`` Directives
----------------------

There are three instantiation pragmas, ``instantiate``, which causes the
specified entity to be instantiated, ``do_not_instantiate``, which causes
instantiation of the specified entity to be suppressed, and
``can_instantiate``, which indicates that the specified entity can be
instantiated by an automatic instantiation mechanism, but need not be.  [#f3]_
The ``do_not_instantiate`` pragma is typically used with the manual
instantiation mechanisms when a specific definition will be supplied elsewhere.

The argument to the instantiation pragmas may be a template class name, a
member function name, a variable name, a static data member name, a member
function declaration, a function declaration, an elaborated type specifier for
a class, or a static data member declaration.  When a class name or elaborated
type specifier is specified, the directive is applied to all member functions
and static data members of the class.

Processing the pragmas results in setting some flags in the template instance.
They are later checked by ``should_be_instantiated`` (called from
``instantiation_wrapup``) to determine whether the entity requires full
instantiation.  The ``instantiate`` pragma sets the ``instantiation_required``
and ``explicit_instantiation`` flags and records the current source position
(for reporting errors later).  The ``do_not_instantiate`` pragma clears
``instantiation_required`` and sets ``specific_def`` to block instantiation
based on the instantiation mode.

Explicit instantiations are processed by ``instantiation_wrapup`` in much the
same way as instantiations that may be generated depending on the instantiation
mode.  The only difference is that certain conditions that are ignored for
nonexplicit instantiations cause errors to be issued for explicit
instantiation.  If no template definition has been supplied from which to
generate an instantiation, an error is issued for explicit instantiation
requests.  Likewise, if a specific definition has been supplied and an explicit
instantiation was requested, an error is also issued.

.. _explicit-instantiation:

Explicit Instantiation Directives
---------------------------------

Explicit instantiation directives are processed in nearly the same way as
instantiation pragmas.  The differences between the handling of the
instantiation pragma and an explicit instantiation directive are as follows:

* | Inline functions may be specified in explicit instantiation directives,
    even though doing so is meaningless.
* | An entity may not appear in more than one explicit instantiation directive.
* | Explicit instantiation directives require that a declaration follow the
    ``template`` keyword, while the instantiation pragmas permit a simple name
    to be specified.

.. _implicit-inclusion:

Implicit Inclusion
------------------

In the *cfront* model of programming with templates the template
declarations are placed in a header (e.g., "``.h``") file while the
definitions of noninline functions and static data members are placed in a
source (e.g., "``.c``") file.  To support this style of programming the
front end provides an optional "implicit inclusion" feature.  When this
feature is enabled, the front end will implicitly include a source file
when needed to provide definitions of template entities declared in a given
header file.

When a header file is included, information about the kind of include statement
used is stored in the IL source file structure.  This specifies whether the
include statement was a "normal" include (e.g., ``#include "file.h"``) or a
system include (e.g., ``#include <file.h>``).

At the end of compilation, during instantiation wrapup processing, the source
file associated with a given header file will be included if needed.  A file is
"needed" if an instantiation of an entity is required as a result of a command
line option, pragma directive, or instantiation list file.  In automatic
instantiation mode implicit inclusions of template definition files will
essentially always be done so that accurate "can be instantiated" flags may be
generated.

The implicit inclusion is done by searching through each of the directories in
the appropriate search path list (based on the kind of include statement used
to include the header file).  A configuration parameter specifies a list of
file suffixes to be used during the search for a file to be implicitly
included.  In each directory on the search path each of the possible suffixes
will be tried in sequence.  If no matching file is found, the next directory in
the list is used.  If no matching file is found in any directory, no error is
reported (unless the instantiation was explicitly requested using an
``instantiate`` pragma in which case an error will be issued indicating that no
template definition was supplied).

Instantiating Non-Template Inline Functions
-------------------------------------------

The front end provides two implementations of C++``extern inline`` functions,
referred to as "lower extern inline" and "instantiate extern inline".  The
"lower" version is the default, and causes extern inline functions to be
translated into static functions with special care taken to ensure that
entities such as local static variables have the proper semantics.  (See the
comments on the definition of ``INSTANTIATE_EXTERN_INLINE`` in ``targ_def.h``
for the advantages and disadvantages of each implementation.)

The "instantiate" version, which is enabled by setting the
``INSTANTIATE_EXTERN_INLINE`` configuration flag, ensures that only a single
external out-of-line copy of the inline function is generated, and then only if
one is required by the program.  The automatic template instantiation mechanism
is used to implement this mechanism.

When an inline function is defined, ``add_to_inline_function_list`` is called
to add the function to a list of all of the inline functions in the translation
unit.  During the wrapup processing at the end of the compilation,
``inline_function_wrapup`` uses the inline function list to determine which
inline functions should have their bodies emitted as part of this compilation.
The ``suppress_inline_body`` flag in the routine entry is cleared if a back end
should emit an out-of-line body of an inline function.
``update_inline_function_flags`` emits instantiation flags as is done for
template entities.  The prelinker will then assign the "instantiation" of
inline functions for which out-of-line bodies are required.

.. _exported-templates:

Exported Templates
==================

The C++ standard specifies that the definition of certain templates (static
data members and non-inline functions) can be separately compiled.  This
feature is generally referred to as "export" as such templates are declared
using the ``export`` keyword.  The definition of an exported template is
provided in a single translation unit.  Other translation units that use the
template require only a declaration of the template.

Exported templates require special processing not needed for normal
(non-exported) templates:

* | When the definition of an exported template is encountered, a record of the
    translation unit containing the exported template must be made so that the
    definition can be found later when an instantiation is needed.
* | When an instantiation of an exported template is generated, the front end
    must be able to search for a definition of the template.
* | Generating an instantiation of an exported template requires that the front
    end have access to information about the translation unit containing the
    template definition and the translation unit containing the reference.  One
    exported template may call another, which may call another, and so on.  As
    a result, the front end must be able to simultaneously process an arbitrary
    number of translation units at the same time (see :ref:`multi-tu`).
* | Whenever a partial or full instantiation of an entity must be done, the
    front end must update its context information so that the translation unit
    containing the template is the current translation unit
    (see :ref:`multi-tu-dispatch`).

When an exported template is defined, information about the definition must be
recorded.  ``add_to_exported_templates_list`` is called when an exported
definition is encountered.  This information is used at the end of the
compilation to create a file that provides a list of the exported templates
defined in the translation unit.  This file is known as the "exported template"
file and, by default, has a suffix of ``.et``.  The following is an example of
an exported template file:

.. code:: text

   fnm:t.c
   tnm:f__tm__4_Z1Z__FZ1Z_v
   tnm:f2__tm__4_Z1Z__FZ1Z_v
   mid:t_c_ee131b0b
   def:XXX=3
   und:YYY
   end:

The following types of lines appear in such a file:

* | ``fnm:`` The name of the primary source file to be used when reloading the
    translation unit.
* | ``tnm:`` The mangled name of a template defined in the translation unit.
* | ``mid:`` The module ID to be used when instantiating templates from the
    file.
* | ``def:`` A command-line macro definition to be used when reloading the
    translation unit.
* | ``und:`` A command-line macro un-definition to be used when reloading the
    translation unit.
* | ``end:`` The end of the entries associated with the previous ``fnm:``
    entry.

The information for a given file always begins with "``fnm:``" and ends with
"``end:``".  This makes it possible to concatenate a group of exported template
files together, which is useful when creating a library that defines exported
templates.

When the front end compiles a file that makes use of exported templates, a
search path of exported template directories is used to locate the file that
defines a given exported template.  The entries on the search path are
specified with the ``--template_directory`` command-line option.  The front end
searches each of the directories in the search path to find any exported
template files that exist there.  Each of the exported template files is read
to determine which exported templates are defined, and the name of the file
that defines each template.  An exported template directory can also contain a
file named ``export_info``, which provides information used when reloading
exported template translation units.  The export info file contains the include
search path to be used when reloading a translation unit.  If no export info
file is present, the include search path associated with the primary
translation unit is used.

A translation unit may contain entities that must be given unique names (e.g.,
unnamed namespace members).  A "module ID" is used as part of the name in such
cases to ensure that the name is unique.  The instantiation of an exported
template must be able to refer to, and generate additional, entities with
appropriate unique names.  To accomplish this, the module ID is saved in the
exported template file and that module ID is re-used when the translation unit
containing the exported template definitions is reloaded.

When an exported template translation unit is reloaded, it undergoes what is
basically the normal compilation process, except that none of the external
entities defined in the translation unit are emitted.  Only the instantiations
of exported templates, and other templates or inline functions referenced as a
result of such instantiations (directly or indirectly) are emitted.  The
entities that are emitted are copied into the IL of the primary file being
compiled (see :ref:`multi-tu-copy`).  The command-line options used when the
exported template translation unit is loaded are the same options used to
compile the primary translation unit.  The only exceptions are command-line
macro definitions/undefinitions, and the include search path to be used.  The
command-line macro definitions/undefinitions are saved in the exported template
file, and those values are used when the exported template translation unit is
loaded (include search path handling is described above).

A mangled name is generated for each exported template.  It is this mangled
name that is used to match a reference to an exported template with the
definition found elsewhere.  A hash table of exported template definitions is
built as the exported template files are read.  When an instantiation is
needed, the mangled name is looked up in the hash table to see if a definition
is available.  If a definition is available, ``process_translation_unit`` is
called to load the translation unit that defines the exported template.

A template instance entry is created for each function, variable, or static
data member instance that is partially or fully instantiated.  If a given
instance is declared in more than one translation unit, there will be multiple
template instance entries created.  In addition, a "master instance" entry is
created to represent the information that is shared among translation units.
For template instances in the primary translation unit, the master instance is
created when the template instance is added to the instantiation required list.
For template instances in secondary translation units, the correspondence
information is used to find a master instance entry that may have been already
created for an earlier translation unit.  Because the correspondence
information is not set until a translation unit has been completely scanned,
the master instance information cannot be established until after scanning has
completed.  The master instance entry points to the template instance entry
associated with the canonical copy of the instance (see
:ref:`canonical-entries`).  Each template instance contains a pointer to the
associated master instance.

Instantiations of exported templates are generated during the "instantiation
wrapup" process, which is performed by ``template_and_inline_function_wrapup``.
The instantiations required list for each translation unit is examined, and
instantiations are generated if appropriate.  An instantiation in one
translation unit may result in the need for an instantiation in another
translation unit.  Because the other translation unit may already have been
processed, the wrapup process must iterate over the set of translation units
until no additional instantiations are added for during an iteration.

The instantiation of a function, variable, or static data member is initiated
by calling ``instantiate_entity`` with the template instance entry of the
instance to be generated.  For exported templates defined in other translation
units, the translation unit is loaded and ``find_corresponding_instance`` is
called to find or create the template instance entry in the exported template
translation unit that corresponds to the one for which the instantiation is
being generated.  The corresponding instance is then used for the remainder of
the instantiation process.

A "translation unit stack" is maintained when instantiating an entities defined
in other translation units.  This stack serves two purposes: it provides a
sequence of lookup contexts for resolving dependent function names during the
instantiation process, and it provides a record of the translation unit
switches that have taken place so that the prior translation unit state can be
restored when an instantiation has completed.

C++/CLI Generics
================

C++/CLI generics are sort of like templates, and sort of not.  They have
arguments, but the arguments are more constrained than for templates, being
effectively only managed class types or handles to them, value classes,
fundamental types, or scoped enum types.  (And types only: No nontype
arguments, no template template arguments.) In addition, generics can have
user-written constraint clauses that further restrict the valid argument values
to certain kinds of class types.  Generic classes do get instantiated for each
set of template arguments, much like template classes, but generic functions
effectively have just one instantiation, which corresponds to a single routine
body that can operate on all types.  That's possible because the function can
only perform those operations that are valid based on the declared constraints.
That allows the instantiation of generic functions to be delayed until runtime
and for most references to share a single instantiation (for example, all
instantiations based on handles to ref classes share a single instantiation at
runtime).  That in turn forces some restrictions on the code that can be
written in generic functions.

Generics are represented as templates marked with the ``is_generic_definition``
flag in the class type or routine entry.  If they have constraints, those are
represented by entries of type ``a_generic_constraint`` and
``a_generic_constraint_clause``.

Generic classes and generic functions don't get the standard prototype
instantiations.  Instead, they undergo a process similar to prototype
instantiation (and kicked off by the same routine that does prototype
instantiations) using invented types based on the constraints.  The result is
marked as a "generic definition" instead of a prototype instantiation.  The
same process applies to static data members as well.

During the generic definition instantiation, a "constraint type" is created for
each generic parameter.  It is a class type that reflects the constraints,
e.g., if the generic parameter is required to be derived from a class X, the
constraint type has X as a base class.  The constraint type is a real type (not
a nonreal type), and lookups can be done in it.  Such lookups in fact
significantly define what can be done with an object of that generic parameter
type, in the sense, for example, that a "``+``" operation can be done only if a
lookup finds an appropriate "``operator+``" function in the constraint type.

The actual type that is used for a given generic parameter type during the
generic defintition instantiation is known as the "generic definition argument
type." It is a handle to the constraint type except in cases when the generic
parameter is constrained to be a value class type, in which case it is the same
as the constraint type.

During the generic definition instantiation, the template parameter symbols for
the generic are set to the generic definition argument type.  That makes a lot
of processing, in declarations and expressions, work out fine without special
handling: uses of the template parameter name produce the generic definition
argument type, and for the most part that that can be processed without
difficulty.  In particular, the generic definition argument type is not a
dependent type, and therefore real types can be built (instead of unknown
template parameter types), real results of overload resolution can be
determined, etc.  However, there are also cases where one wants to think of the
type as being the generic parameter type.  There's a duality here: the template
parameter type and the generic definition argument type are really equivalent
types, and sometimes we want one and sometimes the other.  We chose to make the
default value be the generic definition argument type, because that seemed to
work out better in terms of requiring less special-case code, but whichever way
we had decided that there would be cases that wanted to look at the type the
other way.  Fortunately, because the types point to each other, it's easy to
flip to the other representation if one has the wrong one.  See, for example,
``generic_param_if_generic_definition_argument``.

Because the rules for valid generic template arguments are different than those
for normal template arguments, a special routine is used to scan generic
template argument lists: ``scan_generic_argument_list``.  It calls
``is_valid_generic_argumen``\ t to check that each argument is valid for any
kind of generic.  Later, when one has a full set of generic type arguments
(whether from an explicit list or through type deduction),
``verify_generic_arg_list_satisfies_constraints`` is called to check that the
arguments satisfy their constraints.

Generic classes can overload on arity, meaning that there can be multiple
generic classes with the same name, but differing numbers of type arguments,
declared in the same scope.  That is not valid in source code, but it does come
up in libraries written in other languages and imported via metadata.  When
such overloading occurs, the first generic that is declared is what is entered
into the symbol table and what is returned by lookup.  The template symbol
supplement for that symbol has a pointer to a list of any other generic classes
with the same name and different arities in the same scope.  If there is also a
non-generic class in the same scope, it is pointed to by another pointer there.
With this information, references to the name can be resolved to the proper
instance of the generic or to the non-generic.

Template deduction for generics is done in the usual way; there are no special
tricks to speak of.

.. [#f1] The instantiation request file was formerly known as the
         "instantiation information" file, before the template information file
         came into existence.  The name of the instantiation information file
         was changed to avoid confusion, but the ``.ii`` suffix was retained
         for compatibility reasons.
.. [#f2] The number of iterations is limited by a parameter defined in
         ``edg_prelink.h``.  The default is 30 iterations.  This prevents the
         prelinker from getting in an infinite loop if a recursive
         instantiation is encountered.
.. [#f3] At the moment, the ``can_instantiate`` pragma ends up forcing the
         instantiation of the template instance even if it isn't referenced
         somewhere else in the program; that's a weakness of the initial
         implementation which we expect to address.
