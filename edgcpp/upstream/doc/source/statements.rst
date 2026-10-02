==================
Statement Scanning
==================

``statements.c`` contains the code to scan statements, and ``statements.h``
contains the associated declarations.

The top-level routine here is ``compound_statement``, which scans a
brace-enclosed list of statements.  This is used for the bodies of functions
and for blocks (with and without local declarations) that appear within
functions.  In C mode, it first calls ``opt_declaration_list`` to scan the
optional list of declarations; then, in C or C++ mode, it calls ``statement``
(in a loop) to scan the statements.

``statement`` scans one statement, with an optional preceding label or labels.
Its job is mostly to call the appropriate statement-scanning routine based on
the first token of the statement.  For most statements, the first token is a
keyword that identifies the statement.  Otherwise, the statement is an
expression statement (in C or C++) or a declaration (in C++ only;
``is_decl_not_expr`` is called to decide whether the construct is an expression
or a declaration).

A stack of active statements is maintained in ``struct_stmt_stack``.  For each
active statement, it contains the kind of statement and some information about
what has been scanned so far in the statement.  ``push_stmt_stack`` and
``pop_stmt_stack`` manage the stack.  ``find_enclosing_struct_stmt`` can be
used to find the nearest enclosing structured statement of a given type; this
is used in finding the construct associated with a ``break`` or ``continue``.

One important function of the statement stack is that it points to the
intermediate language for the associated statement.  The top-most entry
contains enough information to enable one to find the "current" location in the
intermediate language being generated, the point at which one would like to add
a statement.  This is, in fact, exactly what ``add_statement_list`` does: it
links the given list of statements (possibly just a single statement) into the
growing tree of statements for the function.  Usually, this is called through\
``add_statement_at_stmt_pos`` , which allocates a statement of a given kind and
also maintains the ``code_reachable`` flag, indicating whether or not the
current location in the code is reachable.  This flag is used to check for
unreachable code and issue appropriate warnings.  Code is attached to the
statement tree even if it is unreachable.  As statements are pushed onto and
popped off the stack, the ``code_reachable`` flag is maintained accordingly,
based on whether or not the beginning of the statement can be reached, whether
the end can be reached from the beginning, and whether the end can be reached
from the inside.  The ``lint_notreached_flag`` is also consulted.  For
statements that have several clauses (``if`` and ``switch``)
``start_stmt_clause`` and ``term_stmt_clause`` are called at the appropriate
locations in processing to keep the ``code_reachable`` flag up to date.

The statement stack contains pointers to ``break`` and ``continue`` labels for
the statement.  These are initialized to ``NULL``, and later made to point to a
label if a ``goto`` is actually generated for a ``break`` or ``continue``.  At
``pop_stmt_stack`` time, the definition for the ``break`` label is generated if
one is needed.  The statement scanning routine must handle the ``continue``
label is there is one.

If a statement is labeled with a simple label, ``statement`` calls
``scan_label`` to look up or enter the label, then ``define_label`` to add a
label definition statement to the intermediate language.

Nothing tricky is involved in statement processing routines ``if_statement``,
``while_statement``, ``do_statement``, ``goto_statement``, ``break_statement``,
and ``continue_statement``.  ``asm_statement`` is called for asm statements; it
in turn calls ``asm_declaration``.

``dependent_statement`` is called to scan statements that are dependent
statements of ``if``, ``switch``, ``while``, ``do while``, and ``for``
statements.  In C mode, it just scans a statement.  In C++ mode, it creates a
block statement and places the dependent statement under it, unless the
dependent statement is a block statement.  That ensures that if anything is
declared in the dependent statement, its scope will be limited to the dependent
statement (this is a post-ARM C++ language rule).

``for_statement`` scans a ``for`` or range-based ``for`` statement (or, in UPC
mode, a ``upc_forall`` statement).  Since it is unknown at the beginning of
scanning whether a ``for`` or range-based ``for`` statement is being scanned,
two block scopes (used to contain the range-based ``for`` temporary variables)
are pushed in preparation for the range-based ``for`` case (they're discarded
if scanning reveals a ``for`` statement).  The first entity inside the
parentheses (initialization) is scanned by calling ``for_init_statement``,
which allows an expression (in C or C++), a declaration (in C++ only), or a
``for-range-declaration`` (in C++11).  ``for_init_statement`` also determines
(through a call to ``decl_statement_full``) which type of statement is being
scanned.  If the ``for-init`` is a declaration (and a ``for`` statement is
being scanned), ``start_for_init_block`` is called to push an ``sck_block``
scope (with ``is_for_init_block`` set to TRUE) onto the scope stack, where
it remains till after the dependent statement of the loop is scanned (see
``finish_for_init_block``).  If the second expression (test) is present, it is
scanned by calling ``can_boolean_controlling_expression``.  The third
expression (increment) is scanned by calling ``scan_void_expression``.  For any
of those expressions/statements that is omitted, a ``NULL`` pointer is used in
the ``stmk_for`` statement.  In the range-based ``for`` case, the statement
kind is set to ``stmk_range_based_for`` (this is somewhat unusual since the
statement kind is typically known earlier) and
``scan_range_based_for_expression`` is called to scan the range expression.
Semantic checks, determination of the type of range-based ``for`` (see header
comments), and IL generation are handled by
``check_range_based_for_statement``.

The Microsoft for-each statement (available in C++/CLI emulation mode, as well
as when ``microsoft_version`` >= 1400) is scanned by ``for_each_statement``.
If the for-each statement contains a previously declared iterator variable, it
is scanned by ``scan_previously_decl_iterator_name``, otherwise the iterator
declaration is scanned by ``for_each_iterator_declaration``.  Two block scopes
are pushed (an outer block scope for the loop temporary variables, and an inner
scope for the iterator variable).  The collection expression is scanned by
``scan_for_each_expression`` and semantic checks (and IL generated) by
``check_for_each_statement``.  ``check_for_each_statement`` handles the various
types of for-each statements (see the header comment for details).  Much of the
underlying code is shared with the range-based ``for`` statement.

``return_statement`` scans the ``return`` statement.  It examines the type of
the current function.  A ``void`` function should have ``return``\ s that
return no value; other functions should return a value.
``check_void_return_okay`` checks whether or not a value-less ``return`` is
okay in the current function.  It is also used at the final closing brace of
the function if that position is reachable (since there is an implicit
``return`` there).

``switch`` statements cause the most problems.  ``switch_statement`` itself is
not too complicated: it just pushes an entry on the statement stack, scans the
selector expression, and scans the dependent statement.  The complex processing
comes with ``case_label`` (which also handles the GNU extension of case ranges;
e.g., "``case 1 ...  3:``") and ``default_label``, or more precisely, with the
subroutine those two cases call, ``add_switch_clause``.  That routine
determines whether or not the label can be added to the list of labels for the
currently-active clause, or whether instead a new clause must be begun.  It
also determines where the case value (or the default) belongs in the list of
constants for the clause.  The configuration macro
``RECORD_SWITCH_CASE_ENTRIES`` determines how individual case labels are
represented.  When TRUE (the default), all the labels (including the default
case and any GNU case ranges) are represented using type
``a_switch_case_entry``.  When FALSE (an option maintained primarily for
backward compatibility purposes), the labels within a clause are usually
represented as a list of ``a_constant`` entries, but in a clause labeled with
``default``, any specific values from ``case`` labels are superfluous and are
removed.  In the latter approach, GNU case ranges are represented as a list
containing all the constants in the range (which doesn't scale well for large
ranges).  Because the intermediate language idiom for ``switch`` statements is
slightly different from the C language idiom, a bit of translation from one
idiom to the other is necessary:

* | A ``break`` in a ``switch`` usually just ends the current clause and makes
    the code following it unreachable.  Since the intermediate language
    ``switch`` clauses end with a ``break`` by default, no ``goto`` to a
    ``break`` label is usually needed.
* | Fall-through from one clause into the next is translated as a ``goto`` to a
    generated label at the start of the next clause.
* | When ``case`` or ``default`` labels are not at the top level in the
    ``switch`` body (i.e., they are defined within structured statements inside
    the ``switch``), the ``switch`` clause is just a ``goto`` to a generated
    label at the right location, and the unusual statement(s) inside the
    ``switch`` body go into the body statement attached to the intermediate
    language ``switch``.
* | Likewise, when there are local declarations in the compound statement that
    is the body of the ``switch``, those declarations go into the body
    statement.

There is also processing to detect and report cases in which a declaration that
involves implicit or explicit initialization is bypassed by a transfer of
control (by means of a ``goto`` or ``switch``).  A separate data structure is
maintained to assist in this checking: variable ``control_flow_descr_list``
identifies a doubly linked list of entries of type ``a_control_flow_descr``
that represents a view of the static control flow pattern of a given function.
Blocks, ``goto``\ s, labels, ``case`` labels, and initializing declarations are
all represented by entries on the list.  Some of these entries point to IL
entries, but the IL does not point back.

The list is dynamically pruned -- it has only enough information on it for the
checking that is required.  For instance, once a forward goto has been checked,
it is no longer interesting and is removed, and entire blocks may be removed
once they become irrelevant to subsequent analysis.  (See
``remove_control_flow_descr`` and ``remove_list_of_flow_control_descrs``, which
place entries onto an available list, and ``alloc_control_flow_descr``, which
reuses freed entries in preference to allocating new ones.)

``add_to_control_flow_descr_list`` is called (from ``add_stmt_at_stmt_pos`` for
initializations, from ``push_stmt_stack`` for blocks, etc.) to add a new entry
to the end of the list, though in some cases the entry will not be
added -- because it is not required after all -- and sometimes it will even
cause other entries to be removed -- for instance, if a completed block turns
out to have no labels or gotos, the entire block can be eliminated.

At the end of a block within a ``switch`` statement,
``add_to_control_flow_descr_list`` may call ``report_switch_past_init``, which
examines the entries on the list for initializing declarations that may be
bypassed by a transfer of control to a ``case`` label.  An error is issued in
C++ mode, a warning otherwise.

``check_for_jump_over_initialization`` is called from ``goto_statement`` and
from ``statement`` when a label definition is seen.  For a forwards ``goto`` it
calls ``add_to_control_flow_descr_list`` to record information that will be
used for checking when the label is processed.  For a label it calls
``check_goto_and_label`` to report any previously recorded ``goto``\ s that may
now be recognized to involve bypassing an initializing declaration (see
``report_goto_past_init``, which issues an error in C++ and a warning
otherwise); and an entry representing the label is added to the list.  For a
backwards ``goto`` it again calls ``check_goto_and_label``, which in turn calls
``report_goto_past_init``, if appropriate.

The control flow descriptor list is also used by
``check_for_branch_into_handler``, which reports attempts to branch into an
exception handler from code outside the handler.

An important part of statement processing is the management of object
lifetimes.  For example, each time a compound statement starts, a new object
lifetime begins; and it ends when the compound statement terminates.  In other
words, objects (e.g., automatic variables and long-lifetime expression
temporaries) created inside the compound statement have lifetimes that
terminate when it does.  The destructor (if any) for such an object must be
called in the event of the normal termination of the scope, a branch out of it,
or unwinding through it as the result of an exception.  The information
enabling back ends to support these implicit destructor calls, in the right
place and in the right order, is provided by entries of type
``an_object_lifetime``, discussed in detail in the chapter on the intermediate
language.

Each time a new function or block scope is created, a new object lifetime is
pushed on the object lifetime stack -- ``push_object_lifetime`` is called in
``push_scope``, and the object lifetime is bound to the scope entry for block
scopes in ``ensure_il_scope_exists`` (in ``il.c``).  In addition, an object
lifetime is pushed for try blocks in ``try_block_statement`` and for cfront
dependent statements in ``start_block_statement``; in the latter case, since
there is no scope, the lifetime entry is bound to the block entry associated
with the block statement.

When a label is defined, a block-after-label object lifetime is pushed.  Such
lifetimes are effectively subblocks of the current lifetime that run from the
label to the end of the block in which the label appears.  When a label is
defined inside an inner block, a block-after-label lifetime is also pushed in
the containing block when the inner block terminates.  (See
``terminate_curr_block_object_lifetime`` and
``reset_curr_block_object_lifetime``.) Block-after-label lifetimes are also
pushed for case labels.

The object lifetime that is current at the point of the label's definition is
recorded in the label statement.  A ``goto`` statement that references that
label will point to the innermost object lifetime shared by the label and the
``goto``; this is computed by ``common_object_lifetime``.

The general strategy is always to create an object lifetime at standard
transitions in the source code (e.g., upon entering a block or when a label is
seen) and then, when the construct it belongs to terminates, to decide whether
there is justification for retaining that lifetime entry in the IL.  If not (as
determined by ``is_useless_object_lifetime``), it is removed from the IL tree.

Consequently, a label or ``goto`` statement may be assigned an object lifetime
that subsequently proves "useless".  In that case the object lifetime pointers
of the statement entries are promoted (see
``fixup_curr_block_labels_and_gotos``, which calls
``promote_label_and_goto_lifetimes``; this is also where
``pop_object_lifetime`` is called for block-after-label lifetimes).  This
processing is driven by management of the control flow description list (see
``add_to_control_flow_descr_list``).

If all the labels and ``goto``\ s in a function end up pointing to the function
scope lifetime but the function scope lifetime itself turns out to be useless,
then the lifetime pointers in the label and ``goto`` statements are cleared
(see ``wrapup_control_flow_processing``).
