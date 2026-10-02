.. _pragmas-and-attributes:

======================
Pragmas and Attributes
======================

Support for the processing of ``#pragma`` directives appears in several files
(including ``preproc.c``, ``lexical.c``, and ``il.c``), but the principal
routines are in ``pragma.c``.  ``pragma.h`` and ``il_def.h`` contain
definitions of the data structures that are used.

Standard attributes, GNU attributes, and Microsoft ``__declspec`` attributes
are like pragmas in their semantic scope (i.e., a generic mechanism for small
language extensions), but unlike pragmas they fit in ordinary grammar-based
based front end processing.  The principal routines for attribute processing
are in ``attribute.c`` (and ``attribute.h``); the main data structures are in
``il_def.h``.

Introduction to Pragma Processing
=================================

Pragma processing differs from other front end processing in several ways.
Pragmas are scanned as part of the inter-token processing of the lexical
routines, yet the actual interpretation of the pragmas must be performed at a
suitable point in the syntactic and semantic processing of the front end.  In
fact tokens may need to be scanned far in advance of when they are actually
used, and so pragmas may be encountered at the lexical level when it is
impossible for them to be fully processed.  This occurs in scanning the bodies
of template definitions, in doing the lookahead to disambiguate declarations
and expressions, and so forth.

Moreover, it is clear that the "suitable point in syntactic and semantic
processing" where the pragma is interpreted will be different for different
pragmas.  And some pragmas simply carry information for use by the back end and
do not affect front-end processing at all.  Each pragma has special
requirements.

Pragmas are also unusual because, by definition, they are specific to a given
implementation.  The mechanism that the EDG front end provides is intended to
make it relatively easy for customers to add pragmas and to customize how they
are processed, from how they are tokenized to how IL pragma entries are
generated.  The handful of pragmas that are supplied by default with the EDG
front end can serve as prototypes for such customization.

A pivotal role in pragma processing is played by an entry of type
``a_pending_pragma``, a front-end-only repository of information about a
specific ``#pragma`` directive in the source.  It is "pending" in that it is
awaiting interpretation, after which it is disposed of.  Interpreting a pragma
may mean setting a flag in a symbol, causing a diagnostic to be suppressed,
creating an IL pragma entry to pass information on to the back end, and so
forth.

Pragma processing can be summarized as consisting of three steps:

#. The ``#pragma`` directive that appears in the source code is identified and
   recorded in a pending pragma entry.  Although most pragmas are simply
   recorded for later interpretation, "preprocessing immediate" pragmas are
   dispatched immediately upon being recognized by the preprocessing
   routines.

#. The pending pragma entry is "dispatched" -- i.e., made available for
   additional processing by being placed on a list where it can be found
   later.

#. The pending pragma entry is interpreted, with its message transferred to
   another form (e.g., a flag is set somewhere), and then it is disposed
   of.

Pragmas must be recognized to be processed, and once they have been recognized
much of the processing is table-driven.  The key pieces of information about
every pragma are:

* | The pragma kind -- an element of the enumeration ``a_pragma_kind`` (defined
    in ``il_def.h``).  Every pragma that is recognized corresponds to one
    pragma kind.
* | The pragma-id -- the identifier string that follows the ``#pragma`` keyword
    and that may be looked up in the array ``pragma_ids`` (defined in
    ``il_def.h``) to determine the pragma kind.
* | The pragma kind description -- a data structure of type
    ``a_pragma_kind_description`` (defined in ``pragma.h`` and available to the
    front end only) that summarizes the attributes and processing requirements
    of a given pragma.

When the pragma has been recognized, much of the processing is governed by the
corresponding pragma kind description, which contains information such as how
(and whether) the pragma binds to an IL construct, whether the pragma is
intended to be processed by the front end or back end, and how tokens that
comprise the pragma should be passed from the point at which the pragma is
initially scanned to the point at which the pragma is semantically processed.
The pragma kind description also contains an optional processing function
pointer, which, if present, points to a function to be called to process this
kind of pragma.

``pragma_description_for_pragma_kind`` is an array of pointers to pragma kind
descriptions that may be indexed by pragma kind.  It is initialized in
``pragma_init`` by a series of calls to ``add_pragma_kind_description``.

Phase One: Recording Pragmas
============================

Pragma directives are initially scanned by the routine ``proc_pragma``, which
is one of the preprocessing routines in ``preproc.c``.  As mentioned earlier,
the initial scanning of the pragma may occur far too early for the pragma to be
processed in any meaningful way.  The most extreme example of this is a
template definition containing a pragma that must be reevaluated for each
instantiation generated from the template.

To accommodate this kind of use, the initial scanning of a pragma typically
consists simply of identifying the kind of pragma being scanned, and recording
the body of the pragma in a form in which it may easily be processed later.
Preprocessing immediate pragmas are the exception to this rule, and are
dispatched immediately upon being identified by the preprocessing routines.

Pragmas are expected to have the form

 ``#pragma identifier`` *token1 token2 token3 ...*

The pragma kind is determined by comparing the identifier with each the
elements of the array ``pragma_ids``.  Once the pragma kind has been
determined, the associated pragma kind description determines how the body of
the pragma is to be recorded.  The body may be recorded as either a token cache
or a character string.  Most pragmas will be recorded as token caches since
this is usually the easiest representation for the front end to use later.
Pragmas that are to be reemitted by the C or C++ generating back end must be
recorded as character strings.

When a pragma is recorded as a token cache, the pragma kind description
specifies how the lexical routines should tokenize the pragma body.
Specifically, it specifies whether macros should be expanded and whether the
recognition of certain preprocessing tokens (e.g., ``#`` and ``##``) should be
disabled.  Typically both of these flags would be FALSE, but when scanning a
pragma containing what looks like a source language construct (as is the case
with the instantiation pragmas) these fields would be TRUE.

If the configuration parameter ``INCLUDE_UNRECOGNIZED_PRAGMAS_IN_IL`` is TRUE,
any pragma identifier that is not recognized is processed using the pragma kind
description associated with the ``pk_unrecognized`` pragma kind.

To record the pragma a pending pragma entry is allocated (see
``alloc_pending_pragma`` in ``pragma.c``), and the pragma representation (token
cache or text) is attached to it.  A pragma is associated with a token that
immediately follows it in the source, so the newly created pending pragma entry
is appended to ``curr_token_pragmas``, a list of pending pragma entries that
are associated with the current token (see ``add_to_curr_token_pragma_list``).

Since the current token may be cached, the list of pending pragma entries
associated with it is cacheable, too (see ``add_pragma_entry_to_cache`` in
``lexical.c``).  When a token is cached, the pragmas associated with it are
recorded in the cache as well, and when the token is retrieved from the cache
the associated pragmas are restored as the ``current_token_pragmas`` list.  If
the cache from which the token was retrieved is a reusable cache, a copy of the
pragma list is made (instead of using the original list).

Pending pragma entries are also created to represent "pseudo pragmas",
constructs that are processed by the front end like pragmas even though they
are not actually represented in the source code by a pragma directive.  Thus,
lint comments are implemented as pseudo pragmas; e.g., when a ``/*ARGSUSED*/``
comment is encountered, the front end generates a ``pk_lint_argsused`` pragma
entry, and all subsequent processing is handled just like a pragma specified
using a ``#pragma`` directive.

Phase Two: Dispatching Pragmas
==============================

The result of the initial scanning process is ``curr_token_pragmas``, a list of
pending pragma entries that represent the pragmas immediately preceding the
current token.  However, the pragma is not subject to further processing simply
as a consequence of being associated with the current token.  For example, with

.. code:: c++

   #pragma xyz
   int a;

the ``xyz`` pragma is on the current token pragma list for ``int`` but is not
made available for additional processing until processing begins on the
construct that ``int`` introduces or else until the next token, ``a``, is about
to fetched.  This convention is followed to avoid processing the pending pragma
entry prematurely -- we wouldn't want to process pragma ``xyz`` if there is a
chance token ``int`` will end up in a token cache.

The pending pragma entries on the ``curr_token_pragmas`` list are dispatched
for additional processing either by ``select_curr_construct_pragmas`` or by
``process_curr_token_pragmas``.  These routines do most of the middle phase
processing of pending pragma entries.  How they differ and how they interact
can be explained in terms of pragma binding kinds.

Each pragma kind specifies one of three pragma binding kinds [#f1]_; each
binding kind represents a different processing pathway; and each form of
processing implies a different form of dispatching.

* | Pending pragma entries with a binding kind of ``pbk_immediate`` and
    ``pbk_next_token`` are processed immediately at the point the current token
    is consumed.  Such pragmas may appear anywhere in the program, and though
    they may be recorded in the IL, they are not bound to any particular IL
    entity (the difference between ``pbk_immediate`` and ``pbk_next_token`` is
    described below).
* | Pending pragma entries with a binding kind of ``pbk_next_construct`` are
    subject to processing during the processing of the given statement or
    declaration.  They must appear immediately before the first token of the
    construct to which they will be bound.  If they appear in the IL, they will
    be bound to a particular IL entity.
* | Pending pragma entries with a binding kind of ``pbk_other`` are not subject
    to processing at any particular time -- that is up to the implementation.
    They may appear anywhere in the source program.  They are dispatched onto a
    list to wait for a processing function to check for them; if they are not
    explicitly handled and removed, they simply remain there until the scope
    they belong to is terminated.

Bound Pragmas
-------------

``select_curr_construct_pragmas`` is called at the start of each new
declaration or statement.  It is always called before advancing past the first
token of the construct; this is to ensure that it is called before
``process_curr_token_pragmas`` is called.  Its principal job is go through the
``curr_token_pragmas`` list, removing each ``pbk_next_construct`` entry and
dispatching it to a list specifically reserved for pragmas bound to the current
construct, the ``curr_construct_pragmas`` list in the current scope stack
entry.  The pragmas wait on the list to be processed sometime during the
processing of the current construct.

The pending pragma entries associated with the current construct will remain on
the list to which they are dispatched only for the life of the declaration or
statement.  If the current construct cannot be bound to (this is the case, for
example, with the declaration of an unnamed enumeration or a ``break``
statement), ``cannot_bind_to_curr_construct`` is called to issue diagnostics,
if appropriate, and clear the list.  If, usually as a result of an error, the
processing of the pending pragma entries cannot be completed,
``discard_curr_construct_pragmas`` is called to clear the list.

Normally, therefore, when ``select_curr_construct_pragmas`` is called, the list
it updates will have been cleared, since entries on the list for the previous
construct will already have been disposed of by the time the current construct
is dealt with.  An exception is when a pragma precedes a label: such a pragma
is construed as applying not to the label but to the construct following, so it
must be preserved on the ``curr_construct_pragmas`` list as processing advances
past the label.  In such situations pending pragma entries may accumulate,
e.g., for this case

.. code:: c++

   #pragma some_pragma
   l:
   #pragma another_pragma
   f();

both pragmas can end up bound to the call of function ``f``.

Another job of ``select_curr_construct_pragmas``, when source sequence entries
are being generated, is to add a source sequence entry for each pending pragma
entry on the ``curr_token_pragmas`` list.  If an IL entry is subsequently
created for the pragma, it uses the source sequence entry; otherwise, the
source sequence entry is returned to an available list.

Unbound Pragmas
---------------

``process_curr_token_pragmas`` is called each time the current token is about
to be consumed and a new token is about to become the current token.  Its job
is to handle pending pragma entries that remain on the ``curr_token_pragmas``
list and then to clear that list before the next token is taken.

If it finds a ``pbk_immediate`` or ``pbk_next_token`` pragma on the list, it
initiates the completion of its processing.  There is no dispatching of these
pragmas.  They are processed immediately and automatically.  In non-cached
contexts, ``pbk_immediate`` pragmas will have already been procesed when
``process_curr_token_pragmas`` is called and will be ignored.

``pbk_other`` pragmas are dispatched to a list of the pragmas that are
currently available for further processing in a given scope -- see
``pending_pragmas`` in ``a_scope_stack_entry``.  The pending pragma entry is
placed on the list of the current scope and is discarded when the current scope
terminates.  Such a pragma simply waits on the list until some event removes it
to complete its processing; there is no default mechanism to trigger such
processing automatically.

Since ``select_curr_construct_pragmas`` will already have been called, there
should not be any ``pbk_next_construct`` entries remaining on the
``curr_token_pragmas`` list.  If there are, they must have appeared in the
source somewhere other than immediately before a declaration or statement.
``process_curr_token_pragmas`` removes them from the list, optionally issuing a
diagnostic of the severity specified in the corresponding pragma kind
description.

If source sequence entries are being generated, ``process_curr_token_pragmas``
will also add source sequence entries for each pending pragma entry that needs
one.  If an IL entry is subsequently created for the pragma, it uses that
source sequence entry; otherwise, the source sequence entry is returned to an
available list.

Phase Three: Interpreting Pragmas
=================================

The pragma representations that result from the scanning of pragmas are called
"pending pragmas" because, having been created as repositories of uninterpreted
information about a specific pragma directive, they wait around for the right
moment for their processing to be completed.  The completion can be described
as the "interpretation" of the pragma -- its translation into another form so
that it will have the intended effect on subsequent compilation.

The different pragma binding kinds indicate, as noted above, different
processing pathways:

* | Pending pragma entries with a binding kind of ``pbk_next_token`` are
    processed automatically by ``process_curr_token_pragmas``, at the point the
    current token is consumed.
* | Pending pragma entries with a binding kind of ``pbk_immediate`` are
    processed immediately after they have been scanned.  In cached contexts
    they are also processed by ``process_curr_token_pragmas`` (in the same way
    as ``pbk_next_token`` pragmas) each time the cache is rescanned.
* | Pending pragma entries with a binding kind of ``pbk_next_construct`` are
    subject to processing during the processing of the given statement or
    declaration.  Either they are processed as the result of an explicit action
    involving a call to ``extract_specific_pragmas``, or else the processing is
    initiated automatically when (at an appropriate point, depending on the
    construct they are associated with) ``process_curr_construct_pragmas`` is
    called.
* | Pending pragma entries with a binding kind of ``pbk_other`` are subject to
    processing only as the result of an explicit action involving a call to
    ``extract_specific_pragmas``; otherwise they simply remain on the
    ``pending_pragmas`` list of the current scope stack entry and their
    processing is never completed.

``process_curr_construct_pragmas`` is the routine that initiates automatic
processing for the ``pbk_next_construct`` pragmas associated with a given
declaration or statement.  It traverses the list of them on the current scope
stack entry, making sure that each pragma binds to the sort of construct
(statement or declaration) that is currently being processed; it (optionally)
issues a diagnostic otherwise.  If ``automatically_include_in_il`` (in the
corresponding pragma kind description) is TRUE, it calls
``create_il_entry_for_pragma``.  If ``next_construct_processing_function`` is
non-NULL, it calls it do any additional processing that is required.  It also
returns all the pending pragma entries to the available list.

If a pragma should be included in the IL, ``create_il_entry_for_pragma`` is
called; it calls ``add_pragma_to_il``, which in turn calls ``alloc_pragma`` to
create the entry of type ``a_pragma`` and ``add_to_pragmas_list`` to add it to
the end of the ``pragmas`` list of the proper scope.

``create_il_entry_for_pragma`` is called automatically -- by
``process_curr_token_pragmas`` for ``pbk_immediate`` pragmas and by
``process_curr_construct_pragmas`` for ``pbk_next_construct`` pragmas -- when
the ``automatically_include_in_il`` flag is set.  It is also called
automatically by ``extract_specific_pragmas`` when that flag is set.  It can be
called directly from pragma-processing code when automatic creation of IL
entries is undesirable.

The scope to which the IL pragma entry is attached is determined based on
several considerations.  If a ``pbk_next_construct`` pragma is bound to a class
member, it belongs to the scope of the class; otherwise, if it is bound to an
IL entry allocated in the file-scope memory region, it belongs to the file
scope; otherwise, it belongs to the current (block or function) scope.  A
``pbk_immediate``, ``pbk_next_token``, or ``pbk_other`` pragma belongs to the
file scope if the ``global`` flag in the corresponding pragma kind description
is TRUE; otherwise it belongs to the current scope.

``pbk_next_construct`` pragmas that are added to the IL have a pointer to the
IL entry with which they are associated (a statement, a variable, etc.), but
the latter has no pointer back to the pragma.  Instead, the
``has_associated_pragma`` flag (in ``a_source_correspondence`` and in
``a_statement``) is set, indicating that one or more pragmas is bound to it.
``find_assoc_pragma`` may be called to locate the pragma entry (or entries)
associated with a given IL entry.

The automatic processing of pending pragma entries may also involve calling a
routine to do whatever special processing is required for the particular pragma
kind.  The address of this routine, which must be declared with the appropriate
type (see ``a_next_construct_pragma_function``,
``an_immediate_pragma_function``, and ``an_other_pragma_function``) is recorded
in the pragma kind description.  Here are some examples from pragmas supplied
by default by the EDG front end:

* | ``instantiate_pragma`` is specified as the processing function for the
    ``pbk_next_token`` pragma of kind ``pk_instantiate``; it is called by
    ``process_curr_token_pragmas`` when such pending pragma entries are
    encountered; no IL entry is required.
* | ``record_arg_pragma`` is the processing function for pragmas of kind
    ``pk_printf_args`` and ``pk_scanf_args``; it is called from
    ``process_curr_construct_pragmas``.  Note that a symbol pointer is included
    in the interface of the processing functions for ``pbk_next_construct``
    pragmas, and so ``record_arg_pragma`` is able to record the event for the
    current routine.

The timing of the call to ``process_curr_construct_pragmas`` and therefore to
the corresponding processing function may be critical.  The rules, in general,
are:

* | for declarations, ``process_curr_construct_pragma`` is called at the point
    the symbol is bound to its corresponding IL entry;
* | for statements, it is called immediately after the statement entry is
    created (but before other processing is done).

For reasons of timing and other considerations it is sometimes inappropriate to
use the automatic mechanisms that are provided.  An alternative is to call
``extract_specific_pragmas``, which returns a pending pragma entry of a given
pragma kind, [#f2]_ and to do the required processing directly.  For an example
of this style of pragma use see ``record_lint_argsused_and_varargs_state``,
which is called *only* when a function is being defined: if
``extract_specific_pragmas`` returns a pending pragma entry of the specified
kind, it sets a flag in the routine type supplement and, since the entry will
have been removed from the current construct pragma list, calls
``free_pending_pragma_list`` to dispose of it.

``extract_specific_pragmas`` is also called for ``pbk_other`` pragmas.  In
fact, it is essential for completing the processing of such pragmas and should
be included in custom code to implement a specific pragma.  Once this routine
has been called for a given pragma kind, the pending pragma entry returned is
no longer on the ``pending_pragmas`` list, so it cannot be found a second time.
Note also that the calling routine is responsible for freeing the pending
pragma entry.

How to Add A New Pragma
=======================

The mechanisms provided to support ``#pragma`` directives have been designed
with extensibility in mind.  This section is intended to provide some
additional guidance in adding new pragma kinds.  In general, this is what you
will need to do to:

* | Add a new entry to the enumeration ``a_pragma_kind`` in ``il_def.h``, and
    add an identifier string to the array ``pragma_ids``.
* | Decide on the binding kind for the pragma.

  * | ``pbk_immediate`` is appropriate when the pragma's effect is quite
      independent of the context in which it appears in the source.  It is
      processed unconditionally immediately after the pragma is scanned.  If
      the pragma appears in a cached context, the pragma is also processed each
      time the cache is rescanned.
  * | ``pbk_next_token`` is similar to ``pbk_immediate`` except that the pragma
      is not processed until the next regular (non-preprocessing) token is
      fetched and if the pragma appears in a cached context it is not processed
      when it is first encountered but only each time the cache is rescanned.
  * | ``pbk_next_construct`` is appropriate when the pragma is meant to affect
      the construct that immediately follows it in the source.  Set one or both
      of ``may_bind_to_decl`` or ``may_bind_to_statement`` depending on what
      sort of construct it applies to.  These pragmas are discarded (optionally
      with a diagnostic) if they appear in front of the wrong kind of
      construct.
  * | ``pbk_other`` is appropriate for maximum flexibility, though it requires
      additional intervention by you, since the automated mechanisms are not
      available.
  * | ``pbk_preproc_immediate`` is appropriate when the pragma must be
      processed when encountered by the preprocessing routines and cannot be
      deferred until the next nonpreprocessing token of the program is scanned.
      No pending pragma entry is created for these pragmas and they cannot be
      entered into the IL.
* | Create the pragma kind description by adding a call to
    ``add_pragma_kind_description`` in ``pragma_init``; the call should be made
    using the interface function that is specific to the binding kind.
* | If you want to store information -- say, the value of a pragma argument
    that will be used when the pragma is processed -- you can add one or more
    variant fields to ``a_pending_pragma``.  The switch statement in
    ``alloc_pending_pragma`` (where the variant field, if any, is initialized)
    should be updated to recognize the new pragma kind.
* | If the pragma is to be entered in the IL and additional information is
    needed in the IL entry, you should add variant fields to ``a_pragma``
    (defined in ``il_def.h``) and initialize them in ``alloc_pragma`` (in
    ``il_alloc.c``).
* | If the pragma is to be entered in the IL automatically, set
    ``automatically_include_in_il`` when creating the pragma kind description
    entry.
* | If the pragma binding kind is ``pbk_immediate``, pbk_next_token, or
    ``pbk_next_construct``, decide whether you want a processing function to be
    called automatically.  If so, be sure to include a pointer to it when
    creating the pragma kind description entry.
  |
  | Even if you have specified a processing function, be sure that
    ``automatically_include_in_il`` is FALSE if you want control over whether
    the IL pragma entry should be created; if you do this, you can call
    ``create_il_entry_for_pragma``.  If ``automatically_include_in_il`` is
    TRUE, the IL pragma entry will already have been created before your
    routine is called, but you will still be able to set any variant fields
    that are specific to this pragma kind.
* | If the pragma binding kind is ``pbk_other`` -- or if it is
    ``pbk_next_construct`` but you don't want to use the automatic facility of
    ``process_curr_construct_pragma`` -- you will have to write code to call
    ``extract_specific_pragmas`` and to do the required processing.  Note that,
    if ``automatically_include_in_il`` is set to TRUE,
    ``extract_specific_pragmas`` will cause an IL pragma entry to be created;
    if you want more control, be sure the flag is FALSE when you create the
    pragma kind description entry.
* | If your code uses ``extract_specific_pragmas``, be sure to call
    ``free_pending_pragma_list`` to dispose of the pending pragma entry (or
    entries) returned to you.
* | If your pragma processing function needs to deal with more than the pragma
    identifier, decide whether your code requires the pragma to be represented
    as a text string or as a token cache.  If the former, set
    ``record_pragma_text`` to TRUE when you create the pragma kind description
    entry.  However, if you choose to deal with tokens, the following
    convention should be observed:

  #. Call ``begin_rescan_of_pragma_tokens`` to find the appropriate token
     cache for rescanning the tokens that comprise the pragma body; it also
     pushes a ``sck_pragma`` scope. The first token you will see is the one
     following the pragma identifier.

  #. When you have finished scanning the tokens, call
     ``wrapup_rescan_of_pragma_tokens`` to pop the ``sck_pragma`` scope and
     check that all of the tokens were scanned from the cache; if the current
     token is not the ``tok_end_of_source`` at the end of the pragma
     directive, an "extra text in preprocessing directive" error will be
     issued.

* | You should set ``expand_macros`` to FALSE if you want to suppress macro
    expansion while the pragma tokens are scanned.  Furthermore, setting
    ``processing_C_code`` when generating a token cache means keywords will be
    recognized and adjacent string literals will be concatenated together.
* | If you are simply passing the pragma on to the back end without processing
    it in the front end, be sure that ``make_text_not_tokens`` is TRUE when you
    create the pragma kind description entry -- a pragma cannot be passed to
    the back end in the form of a token cache.
* | If you want a diagnostic issued when a ``pbk_next_construct`` pragma
    appears in the wrong place in the source program or an unprocessed
    ``pbk_other`` pragma is encountered when a scope is popped, set the
    ``error_severity`` appropriately when you create the pragma kind
    description entry; use ``es_none`` to suppress the diagnostic.

Introduction to Attributes
==========================

Unlike pragmas, attributes are parsed like other ordinary language features.
This parsing is usually achieved with a call to ``scan_attributes``, which
returns a list of attributes (``an_attribute`` entries) in the order that they
were encountered.  Each attribute entry represent a standard attribute, a GNU
attribute, or a Microsoft ``__declspec`` attribute [#f3]_: The specific variant
is recorded in the ``family`` field (which can be ``af_std``, ``af_gnu``, or
``af_ms_declspec``).  Whether a specific attribute family syntax is accepted is
determined by the global variables ``std_attributes_enabled``,
``gnu_attributes_enabled``, and ``ms_declspec_attributes_enabled``.  The
parsing process also classifies attributes according to their kind
(``ak_align``, ``ak_nothrow``, etc.) and their syntactic location
(``al_prefix``, ``al_declarator_id``, etc.).  As with pragmas, the front end
accepts attributes that it doesn't recognize (these are assigned attribute kind
``ak_unrecognized``).

Attributes appear in "groups". For example:

.. code:: c++

   __attribute((align(4))) __attribute((pure,nothrow)) void f();

The ``an_attribute`` entry corresponding to the ``align`` attribute will point
to a first entry of type ``an_attribute_group``, whereas the ``an_attribute``
entries for ``pure`` and ``nothrow`` will both point to a second
``an_attribute_group`` entry.  Attribute groups can be empty (typically as the
result of macro expansion).  To represent empty groups, an ``ak_empty``
attribute is created (which then points to an ordinary entry of type
``an_attribute_group``).  Attribute group entries do not carry information of
their own other than the position of delimiting tokens: Their principal
function is to indicate how attributes were grouped in the source code.  The
"upside-down" arrangement of attributes vs.  groups keeps all the attributes
associated with a given entity on a "flat" list, which simplifies traversing
them (the alternative would consist in having to traverse groups and within
each group traversing a list of attributes).

After attributes have been parsed, they are "attached" and "applied" to an
appropriate IL entity.  This is usually done by calling ``attach_attributes``.

Attaching an attribute means recording its entry in the IL by placing it on a
list pointed to by the IL entry of the entity to which the attribute applies.
Usually, the list is pointed to by the ``attributes`` field in
``a_source_correspondence``.  Unrecognized attributes are recorded in the IL
only when the global variable ``record_unrecognized_attributes`` (initialized
with the configuration macro ``RECORD_UNRECOGNIZED_ATTRIBUTES``) is TRUE.  If
unrecognized attributes are not recorded in the IL, they elicit a warning.

Applying an attribute means that the validity of the attribute for its target
entity is checked (e.g., it is an error for a standard ``[[nothrow]]``
attribute to be applied to a variable) and, possibly, that the IL entry for the
target entity is updated (e.g., the ``noreturn`` attribute causes the
``does_not_return`` flag in the corresponding routine type supplement to be set
to TRUE).  Applying an unrecognized or an empty attribute has no effect.

When simultaneously compiling multiple translation units, the attributes
attached to corresponding declarations are checked.  By default, attributes
must match exactly, but alternative policies are possible for specific
attributes and contexts.

The parsing/classification of attributes, their application to IL entities, and
the checking of corresponding attributes across translation units (when
applicable) is at least in part table-driven.

Parsing Attributes
==================

As mentioned above, attributes are usually parsed by calling
``scan_attributes``.  However, if only GNU attributes should be considered,
``scan_gnu_attribute_groups`` is called instead.  Both functions take an
argument of type ``an_attribute_location``, which specifies the syntactic
context in which the call is made.  For example, in the declaration

.. code:: c++

   [[nothrow]] int __attribute((pure))
               f [[final]] () __attribute((deprecated));

the ``nothrow`` attribute is parsed as ``al_prefix``, the ``pure`` attribute is
parsed as ``al_specifier``, the ``final`` attribute is parsed as
``al_declarator_id``, and the ``deprecated`` attribute is parsed as
``al_post_func``.  (I.e., the front end contains calls to ``scan_attributes``
or ``scan_gnu_attribute_groups`` in various places, each call passing an
argument reflecting the current parsing context.)

In the case of standard attributes, strict *appertaining rules* allow the
syntactic context to unambiguously determine what entity the attribute applies
to.  For example, standard ``al_prefix`` attributes apply to the variables,
functions, or typedefs being declared, while standard ``al_specifier``
attributes apply to the type specified by the declaration specifiers.

In contrast, the syntactic location of GNU attributes does not unambiguously
determine the entity they apply to; instead, the affected entity depends on the
attribute kind.  For example, in

.. code:: c++

   __attribute((align(16))) int f1();
   __attribute((vector_size(16))) int f2();

the ``align`` attribute modifies function ``f1``, but the ``vector_size``
attribute modifies type ``int``.  To deal with this, the front end reclassifies
some GNU attributes after they are parsed based on whether the attribute
transforms a type or not.  This ensures that the attribute is applied to the
correct entity.  For example, the ``vector_size`` attribute in the example
above is parsed as ``al_prefix``, but then reclassified as ``al_specifier``
(this is done in ``attach_specifier_attributes``).  A related example is

.. code:: c++

   void f3() __attribute((warn_unused_result));
   void f4() __attribute((pure));

These attributes are originally parsed with context ``al_post_func``
(indicating they appear after a function declarator), which for a C++11
attribute would imply that the attribute applies to the associated function
type.  However, GNU attributes in this syntactic context can apply to the
function type (e.g., ``warn_unused_result``) or to the function itself (e.g.,
``pure``).  In cases like these, the attributes are reclassified to
``al_postfix`` (in ``scan_declarator_attributes``) and the attribute
application function modifies the function or its type depending on the
attribute (and the same process applies to attributes after array declarators).

Microsoft ``__declspec`` attributes also lack strict appertaining rules, but
their treatment is simpler because there are no type-transforming
``__declspec`` attributes and ``__declspec`` attributes can appear in fewer
syntactic locations (only ``al_prefix``, ``al_specifier``, and ``al_tag_name``
contexts are valid, and the former two are usually equivalent).  There are some
special cases, however.  For example, the ``align`` attribute in

.. code:: c++

   __declspec(align(16)) struct S { ... } x;

is equivalent to

.. code:: c++

   struct __declspec(align(16)) S { ... } x;

and the front end handles this by recording the ``align`` attribute as an
``al_tag_name`` attribute in such cases.

Unscanning Attributes
---------------------

Sometimes, it is not clear at the time of parsing the attribute which context
the attributes will end up in.  E.g., GNU attributes can appear after the left
parenthesis introducing function parameters and after the left parenthesis
introducing a nested declarator (see ``r_declarator`` in ``declarator.c``).  In
such circumstances, the attributes can be scanned (by calling
``scan_attributes``) with an assumed context, then "unscanned" (with
``unscan_attributes``), and then "rescanned" by calling ``scan_attributes``
with the correct syntactic location.  Note that the call to
``unscan_attributes`` does not restore the tokens that made up the attributes;
it just stores the given attribute entries to be returned by the next call to
``scan_attributes``.

Recognizing Attributes
======================

For an attribute to be recognized in the front end, it must have a description
in the array ``known_attr_table`` defined in ``attribute.c``.  The description
(type ``an_attr_descr``) contains four fields:

* | A string with the source form of the attribute name (e.g., "``section``").
    The optional leading and trailing underscores in the case of GNU attributes
    should not be included in this string.
* | A string encoding what kind of arguments (if any) the attribute takes.
    This the "signature" of the attribute.  The details of this encoding are
    described in the definition of ``an_attr_descr`` in ``attribute.c``.  For
    example, the code ``"(sn+)"`` would indicate that an argument list is
    required and that it should contain one or more (narrow) string literals.
* | A string encoding the general conditions (primarily, the mode) in which an
    attribute should be accepted, and, in the case of standard attributes, the
    attribute namespace for the attribute.  This encoding too is described in
    the definition of ``an_attr_descr`` in ``attribute.c``.  For example,
    ``"g+(-39999)"`` indicates that the attribute should be accepted as a GNU
    attribute in GNU C++ modes with ``gnu_version`` less than 40000, while
    ``"c+[EDG]"`` specifies that the attribute should be accepted with standard
    syntax and namespace-prefix ``EDG::`` in modes that accept standard C++11
    attribute syntax.
* | The attribute kind (e.g., ``ak_section``) that should be assigned to the
    attribute.

Note that there can be multiple entries for the same attribute kind.  For
example, the ``nothrow`` attribute is accepted as a GNU attribute, as a
Microsoft ``__declspec`` attribute, and as a standard attribute, and therefore
(at the time of this writing) it has three description entries as follows:

.. code:: c++

     ...
     { "nothrow", "", "1c+", ak_nothrow },
     ...
     { "nothrow", "", "gx", ak_nothrow },
     ...
     { "nothrow", "", "m+", ak_nothrow },
     ...

(the prefix "``1``" in ``"1c+"`` indicates that the attribute cannot be
repeated in an attribute group).

If an attribute name is recognized, but it does not meet other constraints, a
diagnostic is issued and the attribute is reclassified as ``ak_unrecognized``
(by calling the macro ``make_attr_unrecognized``).  This recovery technique is
used throughout attribute processing: Rather than dropping an attribute
altogether, it is generally just reclassified by calling
``make_attr_unrecognized``.

Applying Attributes
===================

The application of attributes is highly dependent on the specific attribute
being applied, but some of the processing is also table-driven.  Specifically,
the array ``known_attr_appl_table`` contains an entry of type
``an_attr_appl_descr`` for every attribute kind.  Each entry contains the
following fields:

* | The attribute kind, included only for readability and consistency checking
    (the array contents must match the enumeration ``an_attribute_kind``).
* | A string encoding which entity kinds an attribute applies to, and,
    optionally, simple constraints for matching entities.  The details of the
    encoding are described in the definition of ``an_attr_appl_descr`` in
    ``attribute.c``.  For example, ``"r|v"`` means the attribute applies to
    routines and variables only, while ``"r|v:-a"`` means the same thing, but
    requires the variable not to have automatic storage duration.  (A failure
    to meet these constraints causes the attribute to be reclassified as
    ``ak_unrecognized``.)
* | A pointer to a callback function (of type ``an_attr_application_fn``).  If
    this pointer is non-NULL and if the constraints in the previous field are
    fullfilled, the indicated function is called with the attribute and the IL
    entry it applies to (the "target entity") passed in as arguments.  The
    callback function can check additional constraints on the target entity and
    update the entry (and any other structures) as appropriate.

A few Microsoft ``__declspec`` attributes require special handling in the front
end.  For example, the presence of a ``property(...)`` attribute is detected
soon after being scanned because it affects certain semantic checks performed
by the front end before the complete declaration has been parsed (and,
therefore, before the application callback function can be called).

Type-Transforming Attributes
----------------------------

Most attributes only modify relatively minor properties of the entities to
which they apply, and this modification is to the entity itself.  For example,
the standard attribute ``[[final]]`` when applied to a class type ``X``
prevents other classes from deriving from ``X``; it does not create a new IL
entry.  Some GNU attributes, however, can transform a type into a new type
requiring a distinct IL entry [#f4]_.  For example, the GNU attribute in:

.. code:: c++

   float __attribute((vector_size(16))) vec;

transforms type ``float`` to a type "vector of ``float``".  Such
type-transforming attributes are applied (and attached) using function
``attach_type_attributes``.  It uses the array ``known_attr_appl_table`` much
like ``attach_attributes``, but instead of making the resulting type entry
point directly to the attributes list, it adds a ``tk_typeref`` entry on top of
the resulting type for that purpose.

Because of the possibility of entity transformation, the callback functions
recorded in ``known_attr_appl_table`` must return an IL entry pointer that
represents the entity resulting from the application of the attribute to a
given entity.  Except for type-transforming attributes, the returned IL entry
is the same entry as the one passed to the callback function.

Attributes in Back Ends
=======================

Most known attributes have an effect on IL fields that provides sufficient
information to a traditional code-generating back end.  For example, the
``nothrow`` attribute causes the ``never_throws`` flag in ``a_routine`` to be
set to TRUE.  However, since attribute entries are also pointed to by affected
entities, it is not strictly necessary to add separate fields to IL entries.  A
specific attribute can be found using the function ``find_attribute``.  For
example, the standard attribute ``carries_dependency`` is currently only
represented by its ``an_attribute`` entry.

The C-generating back end ignores most ``an_attribute`` entries.  Instead, it
usually renders GNU attributes (when the target compiler is known to be a
version of GCC) based on other fields in the IL.  However, for some attributes
that are only represented in the IL through their ``an_attribute`` entry, the
C-generating back end may emit a GNU attribute construct based on the presence
of the corresponding entry.

The C++-generating back end, on the other hand, renders attributes based *only*
on applicable ``an_attribute`` entries.  To ensure that the rendered code is
close to the original form, attributes specified on a secondary declaration are
recorded twice in the IL (i.e., the entries are duplicated): Once on the list
pointed to by the source correspondence of the target entity, and once on a
list pointed by the associated ``a_src_seq_secondary_decl``.  Attributes on
primary declarations are recorded only once, but are marked as having appeared
on the primary declaration (flag ``on_primary_declaration`` in
``an_attribute``).

How to Add a New Attribute
==========================

The framework described above make the addition of many common kinds of
attributes relatively simple:

* | Add an ``ak_``... attribute kind in ``il_def.h``.
* | Add one or more entries to the ``known_attr_table`` in ``attribute.c``.
* | Add an entry in the ``known_attr_appl_table`` in ``attribute.c``.
* | Write the callback function specified in the new ``known_attr_appl_table``
    entry (if such a callback is needed at all).
* | If the attribute is not required to match exactly across translation units,
    add one or more entries describing the correspondence handling to the
    ``attr_corresp_table`` defined in ``attribute.c``.  (In some cases this may
    involve adding additional functions or updating the code in
    ``trans_corresp.c``.)

(The attribute may require new IL entry fields or even new IL entry kinds.  If
so, see :ref:`il-extension-guide`.)

If attributes must be recognized in contexts that do not currently allow them,
additional calls to ``scan_attributes`` and ``attach_attributes`` must be added
to the front end (and, most likely, additional enumeration constants for
``an_attribute_location``).  If the C++-generating back end is used, it will
also require a matching call to ``gen_attributes``.

.. [#f1] Pragmas of a fourth kind, ``pbk_preproc_immediate``, are dispatched
         when scanned by the preprocessing routines.  A pending pragma entry is
         never created.
.. [#f2] Actually, it returns a list, though this would seldom be significant.
.. [#f3] Microsoft attributes delimited by single brackets are not handled by
         this mechanism (see :ref:`ms-attributes`).
.. [#f4] Currently, this is limited to the GNU attributes ``vector_size`` and
         ``mode``.
