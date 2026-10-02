========================
Administrative Functions
========================




The source file ``cfe.c`` contains the main program. It calls
``proc_command_line`` (in ``cmd_line.c``) to parse the command line;
``fe_init`` (in ``fe_init.c``) to do global initialization;
``translation_unit`` (in ``decls.c``) to compile or ``cpp_driver`` (in
``preproc.c``) to do preprocessing; and ``fe_wrapup`` (in ``fe_wrapup.c``)
to do global termination. If a back end should be called, and there were no
errors, it also calls ``back_end``. Then, it calls ``fe_wrapup_part_2``
(also in ``fe_wrapup.c``) to do post-back-end wrapup. Finally, it calls
``exit_compilation`` to exit with the exit status appropriate to the
severity of the errors detected.

If multiple source files can be compiled, the main program includes a loop
around the steps from ``fe_init`` to ``back_end``. It calls
``get_next_source_file`` to see if there are any more source file names in
the command line.

Usually, the front end has its own main program, but it can instead be used
as part of some other main program. This is done by setting the
``MAKE_FRONT_END_CALLABLE`` configuration macro. The name of the main entry
point into the front end is specified by the ``EDG_MAIN`` macro, and is
``edg_main`` by default. When the front end is callable,
``exit_compilation`` does not actually exit the program, but instead
returns control to the routine that called the front end.

Initialization and Wrapup
=========================

``fe_init.c`` contains one-time initialization. Declarations of external
variables in ``.h`` files, which must begin with ``EXTERN``, are compiled
here with ``EXTERN`` defined as an empty string, so storage is actually
allocated. Likewise, ``VAR_INITIALIZERS`` is defined as 1, so that
initializers on those variables are put out. The main routine is
``fe_init``; it calls other routines to initialize predefined macros,
keywords, and assorted state variables. In particular, it calls some
initialization routines in other files (e.g., ``lexical_init``,
``sym_tbl_init``). There are several different kinds of initialization
routines:

* | "early" initialization routines (e.g., ``cmd_line_early_init``), which
    are called before command-line processing is done.

* | "one time" initialization routines (e.g., ``lexical_one_time_init``),
    which are called only once per invocation of the front end, even if
    multiple source files have been specified on the command-line (when
    ``COMPILE_MULTIPLE_SOURCE_FILES`` is enabled).

* | "per-compilation" initialization routines (e.g., ``lexical_init``),
    which are called for each source file specified on the command-line
    (when ``COMPILE_MULTIPLE_SOURCE_FILES`` is enabled).

* | "trans-unit" initialization routines (e.g.,
    ``lexical_trans_unit_init``), which are called once for each
    translation unit processed when processing exported template definition
    files or when ``COMPILE_MULTIPLE_TRANSLATION_UNITS`` is enabled.

Because the front end can be reinitialized (when
``MAKE_FRONT_END_CALLABLE`` is used), most variables must be initialized
dynamically, not statically. Variables that are modified by command-line
processing must be initialized in early initialization routines.

Command-line ``-D`` and ``-U`` options are also handled here (see
``preproc_init``).

If multiple source files are accepted, ``fe_init`` is called at the start
of compilation of each file.

``fe_wrapup.c`` contains one-time wrapup code. This involves popping the
file name scope, checking for errors on files written, and dumping debug
statistics, such as information on symbol table efficiency and on total
memory used. ``fe_wrapup`` is called after the front end is done but before
the back end (if any) is executed, and ``fe_wrapup_part_2`` is called after
the back end (if any) is executed.

.. _cppcli-init:

C++/CLI Initialization
----------------------

The C++/CLI language depends in some essential ways on its core library,
which is loaded from assembly metadata (usually a file called
``mscorlib.dll``). The initialization process therefore preloads that
library and any others specified via the ``--preusing`` command-line option
with a call to ``process_preusings``.

Immediately after loading the top-level declarations of the core library,
``process_preusings`` also initializes a number of global variables (and
global array elements) to point to the symbols for special entities from
the core library (for example, the ref class ``System::Object``). This is
done in ``init_cli_symbols``.

``init_cli_symbols`` also creates templates for ``cli::array``,
``cli::interior_ptr``, and ``cli::pin_ptr``. This is achieved by running
ordinary front end processing on string literals (via calls to
``scan_top_level_metadata_declarations``) containing the declarations for
those templates. The symbols representing these templates are recorded at
this time (for ease of reference in later contexts).

The symbol for namespace ``::cli`` itself is created even earlier by a call
to ``make_symbol_for_namespace_cli``.

Debug Routines
==============

The source file ``debug.c`` contains the routine that processes the
command-line debug request option (``-d``), and also the routines
``debug_enter`` and ``debug_exit`` which are called for flow tracing. When
the command-line debug request includes routine names, ``debug_enter``
checks the name of the routine entered against the names on the request
list. If the name is found, the associated modification of ``debug_level``
is done. An internal stack is used to preserve information from
``debug_enter`` time to ``debug_exit`` time for a function. One trick: A
checksum of the stop tokens set is computed and stored on entry, and
recomputed and checked on exit. This helps catch mismatched
``add_stop_token`` and ``remove_stop_token`` calls in recursive descent
scanning routines.

``debug_enter`` and ``debug_exit`` are not called if there is no
command-line debug request (i.e., if ``db_active`` is FALSE).

Generating Strings from IL Types and Constants
==============================================

``il_to_str.c`` contains code that converts IL types and constants into
C/C++-form strings. Associated declarations are in ``il_to_str.h``.

To use these routines, one needs to declare a control block of type
``an_il_to_str_output_control_block``, call
``clear_il_to_str_output_control_block`` to initialize it, and then set
applicable fields to describe how the string output should be done. The
control block includes, among other things, a pointer to a routine to
output a string, so one can establish a control block that provides an
output routine that writes to a file, or a control block that provides a
routine that puts the output string into a buffer, etc. Other fields in the
control block control the style of output and provide other (optional)
callback routines to do specialized output (instead of the default version)
for certain constructs.

``form_type`` produces a string for a type. It does this by calling

* | ``form_type_first_part``, which outputs the type specifiers and the
    part of the declarator that precedes the name, and then

* | ``form_type_second_part``, which outputs the part of the declarator
    following the name.

So, for example, for the type

  | *array* ``[10]`` *of pointer to* ``const int``

the "first part" would be

.. code:: c++

   const int *

and the second part would be

.. code:: c++

   [10]

If one wants to put out a declaration of an identifier, one can do so by

#. calling ``form_type_first_part``, then

#. outputting the identifier, then

#. calling ``form_type_second_part``.


producing an overall output string like "``const int *a[10]``".

``form_type_first_part`` descends through any derived types that are at the
top of the type by calling itself recursively. When it reaches a
non-derived type, it calls ``form_type_specifier`` to output the specifiers
part of the type (e.g., "``const int``" for the above example). Then, while
reascending, it puts out the parts of the declarator that appear on the
left-hand side of the declarator (pointers, references,
pointers-to-members, and any required parentheses).

``form_type_second_part`` also descends through any derived types by
calling itself recursively. While descending, it puts out the parts of the
declarator that appear on the right-hand side of the declarator (arrays,
functions, and any required parentheses). ``form_array_declarator`` puts
out array declarators. ``form_function_declarator`` puts out function
declarators; it can be replaced by a user-provided routine if desired. One
reason to replace it: It does not handle putting out function declarators
for function definitions.

``form_type_specifier`` puts out non-derived types. Most cases are simple
(see, for example, ``form_int_kind_name`` and
``form_float_kind_name``). Typedef names are put out as such. ``class``,
``struct``, ``union``, and ``enum`` types are always put out as references
to those types, i.e., a definition is never put out.

``form_name`` puts out names; it can be replaced by a user-provided routine
if desired. It handles qualified names (see
``form_class_or_namespace_qualifier``), unnamed entities, and template
argument lists on classes (see ``form_template_args``).

``form_constant`` produces a string for a constant. It does some processing
itself, and has several important helper routines:

* | ``form_integer_constant`` puts out integer constants, including sign,
    suffix, and implicit casts. It also handles integer constants cast to
    pointer types (e.g., null pointers).

* | ``form_address_constant`` puts out address constants, including the
    "``&``" and optimizations for arrays and for adding a byte offset to
    the base address.

* | ``form_pm_constant`` puts out pointer-to-member constants, including
    null constants and any required implicit casts.

* | ``form_char`` puts out a single character of a string or character
    constant, dealing with unprintable characters and the like.

.. _error-reporting:

Error Reporting
===============

``error.c`` contains code that handles reporting of errors. ``error.h``
contains the declarations needed to use the error
routines. ``error_msg.txt`` defines the error codes and associated error
message text for all of the error messages, and, along with
``error_tag.h``, is used by ``mk_errinfo`` to generate the ``err_codes.h``
and ``err_data.h`` files. It is the generated files that are actually used
by the front end to define the ``an_error_code`` enumeration and the array
of error message text. See :ref:`mk-errinfo` for a description.

``an_error_code`` defines enumeration constants that represent each error
message (for example, ``ec_unclosed_string`` represents the error message
"missing closing quote"). ``ec_no_error`` is the first member of the
enumeration, and therefore has the value ``0``, which has the effect of
allowing flags with the value ``0`` for no error, and a non-zero value to
indicate a specific error.

Within the error message texts, fill-in codes beginning with "``%``"
indicate points at which information is to be inserted:

.. list-table::

   * - | ``%%``
     - | A "``%``" is inserted into the message.
   * - | ``%d``
     - | A number is inserted into the message.
   * - | ``%s``
     - | A character string is inserted into the message. ``%sq`` indicates
         that the inserted string should be placed in quotes, e.g.,

           ``could not open source file "abc.h"``

   * - | ``%t``
     - | A description of a type is inserted into the message. It is always
         placed in quotes, e.g.,

           ``cast to array type is nonstandard (treated as cast to
           "int *")``

   * - | ``%n``
     - | A description of an entity in the program (identified by a symbol)
         is inserted into the message. The default ``%n`` ("name") can be
         modified by appending an "``o``" (``%no``, "name only") to get just
         the name of the entity, a "``p``" (``%np``, "name with function
         parameters") to force parameter information on a function, an
         "``f``" (``%nf``, "name full") to get full type information on the
         name, an "``a``" (``%na``, "name with template arguments") to get
         arguments on a template, "``t``" to force the "``A<T> with T=int``"
         form of template name to be used, or a "T" to get the translation
         unit in which the symbol was declared.

       .. list-table::

          * - | ``%n``
            - | ``function "A::f"``
          * - | ``%no``
            - | ``"A::f"``
          * - | ``%np``
            - | ``"A::f(int, float)"``
          * - | ``%na``
            - | ``"A<int>"``
          * - | ``%nt``
            - | ``"A<T> with T=int"``
          * - | ``%nT``
            - | ``function "f(int)" (from translation unit "x.c")``

.. list-table::

   * - | ``%p``
     - | A source position is inserted into the message.
   * - | ``%[``\ *label*\ ``]``
     - | The specified label is looked up in the ``label_fill_ins`` array
         and one of two error messages is substituted depending on the
         value of a boolean variable associated with the label.  The
         substituted error messages cannot themselves have fill-ins. See
         the definition of ``label_fill_ins`` to determine the list of
         valid label fill-ins (or to add a new one).
   * - | ``%T``
     - | A template argument list is inserted into the message.

In the default mode, the entity description is preceded by a keyword that
identifies the type of the entity (e.g., ``function`` above). That is also
done in the "full" mode for entities that are not variable-like or
function-like. The normal "``%n``" displays function parameter types only
for overloaded functions. Any of the ``%n`` codes can be modified by
appending a "``d``" to request information on the point of declaration of
the symbol:

.. code:: text

   declaration is incompatible with "int f(char)" (declared at line 17)

Following the optional "``d``" modifier, the "``T``" modifier may be used
to display the translation unit associated with the symbol ("``from
translation unit "x.c"``"). When compiling a primary translation unit, this
will cause the translation unit to be displayed for symbols from secondary
translation units. When compiling a secondary translation unit, this will
cause the translation unit to be displayed for all symbols.

There can be more than one fill-in code in a message. When there is, each
fill-in code can be numbered (e.g., ``%t1``, ``%t2``) to tie it to a
corresponding fill-in argument. Note that this technique allows one to
reorder the fill-ins in a message (say, when changing the error messages to
a language other than English) without changing the order of the fill-in
arguments on error calls throughout the front end.

Each normal diagnostic includes four kinds of information:

* | the error code, which is a value from the enumeration just mentioned,
    and indicates the error message text;

* | a severity (remark, warning, discretionary error, error, or
    catastrophic error);

* | the error position (a sequence number and column); and

* | optionally, some information that goes into fill-in spots in the error
    message text.

The severity is usually indicated by the routine called (e.g., ``warning``
instead of ``error``). There are also routines that accept the severity as
an argument. A given message can be (and is, in many cases) output with
different severities in different modes or different places.

The severity of a given error code may be overridden through the use of a
command line option or pragma. For example, the option
``--diag_error=extra_semicolon`` may be used to cause the extra semicolon
diagnostic to be issued as an error, even if the ``warning`` function was
actually called to report the error. Only diagnostics that would by default
be issued as remarks, warnings, or discretionary errors may have their
severities overridden (i.e., nondiscretionary errors and catastrophic
errors may not have their severities overridden). When a severity is
overridden by a pragma, the default severity can be restored by use of the
``diag_default`` pragma. The default severity is the severity as would
normally be used for the message as adjusted by any command-line options.

The error severity may be overridden by specifying either an error "tag" or
an error number. The error tag is a name assigned to each error code. The
error number is the numeric value of the error code enumerator that
represents the message. By default, the error tag is the same as the error
code enumerator name with the ``ec_`` prefix removed. The error tag may be
changed by specifying an alternate tag in the ``error_msg.txt``
file. Additional tags may be provided by creating entries in the
``error_tag.txt`` file. ``set_severity_for_error_tag`` and
``set_severity_for_error_number`` are used to override the severity for a
given diagnostic. These routines are called to process the error severity
options during command line processing.

The error position is usually given by the global variable
``error_position``, but it can also be explicitly
supplied. ``error_position`` is set to the start of each token as it is
scanned, and is set on exit from recursive descent routines to the start of
the entity just scanned, unless that entity is extremely large (like
several lines). This usually results in the correct position in
``error_position``, at least for syntax errors.

When the sequence number of ``a_source_position`` is ``0``, the column has
special meaning (for example, it can indicate that the error occurred in
the command line). See ``basics.h``.

The error routines themselves have names that encode the severity (one of
``remark``, ``warning``, ``error``, ``catastrophe`` or the generic
``diagnostic``), and the particular combination of fill-ins and a source
position that they accept. That is, an error routine name may be as simple
as ``error`` or as complicated as ``pos_ty2_warning``. ``pos`` indicates
that a source position is provided; ``str`` or ``st`` indicates that a
string fill-in is provided; ``sym`` or ``sy`` indicates that a symbol
fill-in is provided; ``type`` or ``ty`` indicates that a type fill-in is
provided. These can be followed by a number if more than one fill-in is
needed, e.g., ``ty2`` for two types. So, for example, ``pos_ty2_warning``
issues a warning, and the caller is expected to provide the error code, the
source position, and two fill-in types. See ``error.c`` for the full list
of routines. Not all combinations exist, but it is easy to add any routine
that does not exist.

For a syntax error, one can instead call

.. code:: c++

   syntax_error(error_code);

which will call ``error`` and then ``flush_tokens`` (see ``lexical.c``).

For complicated errors that may have to list an arbitrary number of items,
for example diagnostics for overloading ambiguity, one can make an initial
call to issue the initial line of the error (calling ``start_error`` or one
of its variants such as ``type_start_error``), then any number of calls to
add individual lines of additional information (calling ``add_diag_info``
or one of its variants such as ``sym_add_diag_info``), and then a call to
``end_error`` to finish the error processing (including writing the source
line in error). The end result is illustrated by

.. code:: text

   "test.C", line 7: more than one constructor applies to convert from
             "double" to "A":
               function "A::A(int)"
               function "A::A(float)"
       f(2.0);
         ^

The usual fill-in processing is available on both the initial line and any
lines of additional information. The additional lines are described by an
error code, so they can also contain arbitrary text.

The error formatting and processing is done by ``construct_message``. It
fetches the error message text, builds a linked list of message segments
describing the message and its fill-in codes, expands each fill-in
appropriately, and writes the full message.

If an error is detected while processing a template instantiation or during
implicit generation of a routine (e.g., a constructor), it is helpful to
provide additional information on the context in which the error was
detected:

.. code:: text

   "test.c", line 7: error: "A::A()" is inaccessible
     B x;
        ^
             detected during implicit generation of "B::B()" at line 7

The context is deduced from the entries in the scope
stack. ``include_in_context_output`` identifies the scope stack entries
that merit extra context information. If there are any such entries,
``diag_message`` outputs an extra context line for each, in order from
innermost context to outermost.

``format_output_line`` is the routine that actually prints the diagnostic
messages; it is the routine to change to modify the error output. To output
the source line associated with the diagnostic, ``write_source_line`` is
called. If the error is in the current line, ``write_orig_source_line``
reconstructs and writes out the original source line, with a caret under
the column position of the error. If the error is not in the current line,
``write_error_source_line`` reads the line from the source file, if
possible, and writes out the source line. Long diagnostic messages are
wrapped across multiple lines. If raw listing information is requested,
similar information is written to ``f_raw_listing`` (in encoded form). The
diagnostic routines count the number of diagnostics of each severity so
that the compilation can be ended with an appropriate exit status. They
suppress diagnostics that are below the error display threshold. If an
error is catastrophic or if the error limit is reached, the compilation is
terminated.

The severity of diagnostics for strict ANSI violations and C++ anachronisms
can be specified by command line options. To simplify handling of these
diagnostics the global variables ``strict_ansi_error_severity`` and
``strict_ansi_discretionary_severity`` contain the error severity to be
used for strict ANSI violations. The former is either ``es_warning`` or
``es_error``, while the latter is either ``es_warning`` or
``es_discretionary_error``, depending on which strict mode was selected on
the command line. ``anachronism_error_severity`` contains the error
severity to be used when anachronistic features are used. These global
variables can be used as arguments to the ``diagnostic`` reporting
routines.

``command_line_error`` and ``str_command_line_error`` are the routines
called for command-line errors (without or with fill-in text,
respectively). They do not return.

The routine ``internal_error`` should be called for internal errors, such
as failures of consistency checks. It should not be called for anything
that is due to a source program error or some environmental error (like a
missing file). The call should look like

   ``internal_error("``\ *routine-name*: *problem found*\ ``");``

Several routines are provided as convenient ways to do consistency
checking. Calls of these routines need not be surrounded by ``#if
CHECKING``/``#endif``; they are defined as macros that expand to nothing
when ``CHECKING`` is disabled. ``check_assertion`` can be used to check an
expression to make sure it is true; ``check_assertion_str`` is similar, but
includes a string to be printed on error
termination. ``unexpected_condition`` is used when a piece of code is not
expected to be reached (for example, the default clause of a switch);
``unexpected_condition_str`` is similar, but includes a string to be
printed on error termination.

Memory Management
=================

``mem_manage.c`` contains memory management routines. ``mem_manage.h``
contains the associated declarations.

Memory is allocated in *memory regions*. File-scope intermediate language
information is put in memory region 1, and the functions in the source
program will each have one of the memory regions 2, 3, and so forth. Memory
region 0 is used for storage that need not survive the front end, like the
symbol table.

Before using a memory region, one calls either ``init_memory_region`` (for
regions 0 and 1) or ``new_memory_region`` (for regions for functions) to
initialize the new region.

``alloc_in_region`` is then called when needed to allocate space in a given
region. (``alloc_fe`` can be called for the special case of allocation in
region 0, the front-end-only region.) ``alloc_in_region`` allocates a large
block of memory for each memory region by calling ``malloc``, then parcels
out that storage as it is needed. When the block is exhausted, a new block
is allocated and put on a linked list of blocks for that memory region.

When one is done with an IL memory region,
``check_for_done_with_memory_region`` is called; it will either write the
information to a file and free the block, or trim the end of the memory
region to reclaim any unused space (depending on whether an IL file is
being generated), or defer this processing until later if the memory is
potentially needed for operations such as inlining or generic lambda
instantiation. "Trimming" means making a separate available block from the
remaining space at the end of the last block in the region. Since the
blocks are large (typically, 64K) and a typical function will use less than
10K of the block, this is an important feature. The combination of using a
large block size and reusing the remaining space improves efficiency (in
reducing the number of calls to ``malloc``) and avoids the need to guess at
the amount of storage a particular memory region is likely to
need. Eventually, when the partial blocks created in this way are released,
they are recombined into the original large blocks allocated via
``malloc``, and freed using the system ``free``. Thus storage used is
freed, but only on a large-block basis, so fragmentation is not a problem.

``alloc_general`` is a more-or-less direct interface to ``malloc`` to be
used in the infrequent cases where storage must be allocated that will
survive into the back end if the back end is called in the same program. An
example of such storage is the debug option control list, which holds
information about the command-line debug option.

``alloc_resizable_buffer`` and ``realloc_buffer`` are used to allocate
tables that can be enlarged (e.g., ``input_stack``).

When ``MAKE_FRONT_END_CALLABLE`` is used, the front end keeps a record of
all memory allocations so that all of the memory allocated can be freed
before returning control to the caller.

When precompiled headers are used, the memory management routines work to
allocate the IL memory region blocks at reproducible addresses, so that the
information in a precompiled header file can be read back in without the
need to walk the IL and remap pointer references. See
:ref:`memory-management-issues`.

File Variables
==============

Normally, any open files are closed when the front end exits, but when
``MAKE_FRONT_END_CALLABLE`` is used, any open files must be closed if the
front-end were to terminate abnormally. This is accomplished by having a
special cleanup routine (e.g., ``templates_cleanup``) in any file that
opens and closes files. The cleanup routine is present only when
``MAKE_FRONT_END_CALLABLE`` is used. File variables must be set to ``NULL``
when they do not refer to an open file. The cleanup routine generally calls
``close_file_if_open``, to close any files that may be open. The cleanup
routines are called by ``fe_cleanup`` (in ``fe_wrapup.c``).

Command-line Parsing
====================

``cmd_line.c`` contains code to parse the command line; ``cmd_line.h``
contains the associated declarations. The main routine is
``proc_command_line``. Aside from setting switches, ``proc_command_line``
establishes the primary source file, the preprocessing output file if there
is one, the raw listing output file if there is one, the cross-reference
output file if there is one, and the include file search list.

If multiple source files can be compiled, ``get_next_source_file`` is
called after each source file is compiled to see if there is another file
name on the command line. It also does some minor initialization.

``proc_command_line`` is called only once, even if multiple source files
are specified on the command line. Since the IL memory for each source file
is separate and self-contained, ``proc_command_line`` cannot allocate
anything in the IL memory: For one thing, it does not exist yet, and for
another, there will be a distinct copy for each source file
compiled. Therefore, file names and other information saved by
``proc_command_line`` must be saved in general memory, and copied into IL
memory later (e.g., in ``fe_init``) for each source file.

Host-dependent Routines
=======================

``host_envir.c`` contains host-dependent routines; ``host_envir.h``
contains the associated declarations.

The following routines are used to maintain the list of directory names to
be searched for include files:

* | ``add_default_include_search_path``
* | ``add_to_include_search_path``
* | ``add_to_front_of_include_search_path``
* | ``push_primary_include_search_dir``
* | ``pop_primary_include_search_dir``
* | ``change_primary_include_search_dir``

The following routines are used to tear apart and build up file names:

* | ``directory_of``
* | ``gs_directory_of``
* | ``derived_name``
* | ``replace_file_name_suffix``
* | ``combine_dir_and_file_name``

The following routines are used to open files in various ways:

* | ``open_source_file``
* | ``reopen_source_file``
* | ``okay_as_output_file``
* | ``open_output_file``
* | ``reopen_error_output_file``

``delete_file`` is used to delete files (specifically, files that were
partially generated and are to be canceled because of detection of an
error).

``open_temp_file`` and ``close_temp_file`` provide support for creating
temporary files. (Note: At the present time, the front end only uses
temporary files when generating an intermediate language file for immediate
use by a back end called in the same program, and in the C-generating back
end.)

``normal_termination`` and ``term_compilation`` are used for termination of
the compilation. The low-level functions of ``term_compilation`` are
available individually through ``write_signoff`` and ``exit_compilation``.

``identify_source_file`` is called from ``fe_init`` at the start of
compilation of each source file when there are multiple source files on the
command line. It outputs a message identifying the start of the compilation
of the file.

``set_signal_handlers`` is called during initialization to set up handling
for signals that should abort the compilation.

This file is likely to be modified for different hosts. See the section on
host configuration for more information.

Intermediate Language Traversal
===============================

``il_walk.c`` contains the routines that walk the intermediate language
tree, and ``il_walk.h`` contains the associated
declarations. ``walk_entry.h`` contains the detailed code to process the
fields of each IL entry kind.

"Walking" the tree means traversing the in-memory form of the IL and
calling a supplied routine for each node in the tree. This is used in
displaying the IL in human-readable form, in writing the IL to a file, and
in reading the IL from a file.

The walking is done on a memory region basis, i.e., for the file-scope
memory region or the memory region for a function. The routines for those
cases are ``walk_file_scope_il`` and ``walk_routine_scope_il``. They call
``walk_entry_and_subtree`` or ``walk_string_entry`` for each entry, and
those in turn call the user-supplied entry processing functions. There are
two separate entry processing functions supplied for each traversal: one
for strings, and one for non-strings. The main reason for the difference is
that string entries do not have fixed sizes, so the string routine has an
extra parameter for the size.

Since the tree is actually a graph and has cycles, there needs to be a way
to tell when the traversal returns to an entry that has already been
visited. This is provided by the ``il_walk_flag`` in the prefix that
precedes each IL entry. This flag starts out as 0 in all entries. On the
first traversal, it is changed to 1 in all entries, and it keeps
alternating between 0 and 1 on each subsequent traversal. Therefore, on a
given traversal, the entries already seen have one value, and those not yet
seen have the other value.

While the tree is traversed, one can also request that all the pointers in
the tree be remapped in some way. For example, when writing to an IL file
in the alternate form, each pointer is replaced by an entry number. Such
remapping can be requested by providing a pointer remap function for the IL
walk, or explicitly by calling ``remap_pointers_in_il_entry`` or
``remap_il_header_pointers``.

Special handling is required for "orphan" entries. These are entries
allocated in the file scope memory region that are referenced only from
function scope memory regions. When the parents of such entries are written
out and then removed from memory, the entries are orphaned because they are
not attached to the rest of the file-scope IL tree.

The orphan mechanism makes lists of such entries so that they can be found
during traversal of the file-scope IL.

The array ``orphaned_file_scope_il_entries`` heads lists of orphaned
entries linked together by a hidden orphan-list pointer that precedes the
storage for each file-scope IL entry. (It also precedes the
``an_il_entry_prefix`` structure.)

The orphan lists are built up while the function scope memory regions are
walked. Each time an entry in a function scope memory region contains a
pointer into the file scope memory region, the entry pointed to is recorded
as a potential orphan. Later, at the end of the walk of the file scope IL,
the entries on the orphan lists are visited. If the entries are not in fact
orphans, they will have been previously visited, and the IL walk flag will
indicate that fact.

A separate mechanism exists for orphaned lists, specifically the lists of
local types and local static variables in function and block scopes. Those
are unusual in that the list is completely in the file scope memory region
but the pointer to the list is in a function scope memory region
entry. Each of the entries on those lists is placed on the orphaned entries
list, but there needs in addition to be a way to remember the head-of-list
pointers so that the ``next`` pointers in the entries can be
remapped. That's done with a list of entries of type
``a_scope_orphaned_list_header`` pointed to by the
``scope_orphaned_list_headers`` field of ``il_header``. Each entry on the
list points to a list of types and a list of static variables preserved
from a function or block scope.

Obviously, the code in ``walk_entry.h`` must be kept up-to-date whenever
the IL definition in ``il_def.h`` is changed.

``il_walk.c`` also contains some special-purpose walking routines:
``walk_declarative_entities_in_scope`` can be used to walk entities like
variables in a given scope; ``traverse_expr`` can be used to walk an
expression tree; and ``traverse_statement`` can be used to walk a statement
tree. In each case, user-provided callback routines can be called for each
node of the tree.

.. _needed-flags:

Needed Flags
============

When the configuration switch ``MAINTAIN_NEEDED_FLAGS`` is TRUE, the front
end will maintain the ``needed`` flag in the source-correspondence entry,
and the ``definition_needed`` flag in class types and routines. The code to
do this is in ``il_walk.c``, ``walk_entry.h``, ``il.c``, and
``scope_stk.c``. The needed flags are a more sophisticated version of the
``referenced`` flag. They are set by beginning with all externally-defined
variables, static data members, and functions in the compilation unit, and
marking those as needed. Then the entities referenced by those are marked
as needed, and so on. The resulting ``needed`` flag setting indicates
whether something is "really" needed, as opposed to referenced from
something that is itself unneeded. For example:

.. code:: c++

   static int i;         // i is referenced, not needed
   static void f() {     // f is not referenced, not needed
     i = 1;
   }
   int main () {}        // main is needed, not referenced

The ``definition_needed`` flag indicates whether the definition of a class
is needed, as opposed to merely a declaration. For example:

.. code:: c++

   struct A { };
   struct B { };
   A *p;      // forces needed on A, but not definition_needed
   B b;       // forces needed and definition_needed on B

There is a similar flag for functions as well.

If ``DEFAULT_REMOVE_UNNEEDED_ENTITIES`` is TRUE, and that setting is not
overridden via a command-line option, unneeded entities are removed from
the IL. Classes that have ``needed`` TRUE and ``definition_needed`` FALSE
are modified to eliminate their definitions. Function-local entities are
never affected by this process (they are not removed, and local classes are
never turned into declarations). The IL tree after alteration is
self-consistent, in that a back end that does not use the "needed" flags
will find all the IL entries it expects. A back end that does use the
"needed" flags should be sure to use both of them, i.e.,
``definition_needed`` as well as ``needed``.

The "needed" flags processing and removal of unneeded entries works with
and without IL lowering, and for both C and C++. When IL lowering is used,
the "needed" flag information is determined on the basis of the lowered
code. When the C++-generating back end is used, and the source program has
templates, elimination of unneeded entities is disabled, because the
template bodies may contain references to apparently unneeded
entities. This can be changed, carefully, if one has a situation where all
template instantiation will be done by the EDG front end rather than by
some compiler operating on the source code generated by the C++-generating
back end.

The setting of the "needed" flags is done as follows:

* | At the end of scanning a function definition, if the function is
    externally defined, ``pop_scope`` calls ``mark_as_needed`` on the
    routine, which in turn calls ``set_routine_definition_needed``. These
    use a specialized version of the ``walk_entry.h`` IL-walking code to
    sweep through the IL tree and set the ``needed`` flag on all
    transitively-referenced entities. If the type of reference requires a
    complete type, and the type is a class, ``set_class_definition_needed``
    is called to set the ``definition_needed`` flag on the class. If a
    reference to a routine requires that the routine have a definition,
    ``set_routine_definition_needed`` is called for the routine.
  |
  | When a routine's definition is needed, the definition is also walked,
    and then the memory region for the function can be disposed
    of. (``mark_to_keep_in_il`` is also called for the function memory
    region; more on this below.) That means the memory region can be
    written to a file and then freed, if an IL file is being
    used. Conversely, as long as a function is not known to be needed, its
    body must be retained. If the function is still marked as unneeded at
    the end of the compilation, it is then clear that the function truly is
    unneeded. It is deleted and its memory region is freed (without writing
    it to the IL file, if there is one).
* | When the file scope is being
    popped from the scope stack, several things are done. First,
    ``set_needed_flags_at_end_of_file_scope`` is called. It visits all
    variables (including static data members), and calls ``mark_as_needed``
    for those that are externally-defined. This causes the variables and
    everything they reference to be marked as "needed."
* | Then,
    ``mark_to_keep_in_il`` is called for the file scope. This uses another
    variant of the ``walk_entry.h`` IL-walking code to sweep through the IL
    tree and set the ``keep_in_il`` flags in the IL entry prefix and the
    ``keep_definition_in_il`` flag in classes and routines. These flags
    have meanings similar to the ``needed`` and ``definition_needed``
    flags, but they are set in more cases, to deal with the fact that some
    entities in the IL tree must be retained (for consistency) even if they
    are "unneeded." In this sweep, entities that can and will be removed
    (based on the ``needed`` and ``definition_needed`` flags and the
    context where the entities appear) will not be visited. The
    ``keep_in_il`` sweep also notes whether complete class types are
    needed, and calls ``set_class_keep_definition_in_il`` when appropriate,
    and whether routine definitions are needed, and calls
    ``set_routine_keep_definition_in_il``.
  |
  | All decisions about
    removing IL entries from the IL tree are based on the ``keep_in_il``
    and ``keep_definition_in_il`` flags, rather than on ``needed`` and
    ``definition_needed``.
* | Then,
    ``eliminate_bodies_of_unneeded_functions`` is called. It eliminate the
    bodies (but not the declarations) of functions that are not needed.
* | Then, ``eliminate_unneeded_il_entries`` is called. It removes types,
    variables, and routines in the file scope that are unneeded, and
    reduces classes to declarations where appropriate (by calling
    ``turn_class_definition_into_declaration``). Auxiliary parts of the IL
    data structure, such as source sequence entries and the hidden-name
    table, are updated as necessary.

The IL-walk process used to set these "needed" flags stops when it
encounters an entry with the flag already set. This is desirable for
compilation speed reasons, and necessary to avoid loops for parts of the IL
structure that are graphs instead of trees, but it also makes certain kinds
of problems possible. Variables, routines, and types that are not
function-local have the potential to be redeclared after the "needed" flag
is initially set. For example, a class ``A`` might be declared, then marked
as needed, then defined. Or, a function might be defined, then declared to
add an additional default argument. In all such cases, The ``needed`` or
``keep_in_il`` flag would be set already on the entry, and therefore an
attempt to set it again would result in an immediate return, rather than a
sweep through the updated subtree. To avoid this problem, the subtrees of
these redeclarable entities are not swept ordinarily; the appropriate flag
is set on the entry itself, but its subtree is not visited. At the end of
the file scope, when no more redeclarations are possible, the global flag
``end_of_file_scope_needed_flags_phase`` is set to TRUE, and that enables
the subtree walks. ``set_needed_flags_at_end_of_file_scope`` and
``mark_to_keep_in_il``, as they are doing the ``needed`` and ``keep_in_il``
walks respectively, look for these delayed-subtree-processing entries, and
handle them by clearing the ``needed`` or ``keep_in_il`` flag and then
calling the subroutine to set them again, which this time will visit the
subtree. For the ``needed`` flag walk, ``remark_as_needed`` provides a
convenient way of doing this.

When ``ONE_INSTANTIATION_PER_OBJECT`` is TRUE, a more complicated set of
"needed" flags is maintained. In that mode, each instantiation is put out
as a separate object file. There is still only one IL tree, but it is
marked so that a back end can sweep through it several times and put out a
different "slice" each time, each slice containing one instantiated entity
(function, variable, or static data member) plus exactly the set of other
entities needed by that entity. This is done via a set of per-instantiation
"needed" flags, represented as a bit vector implemented as a linked list of
entries of type ``a_per_instantiation_needed_flags_entry`` attached to the
``per_instantiation_needed_flags`` field of the source correspondence. Each
slice is assigned one bit in that bit vector, and if the bit is 1 the
associated entity is needed in the slice.

The per-instantiation "needed" flags are tested and set by the macros
``needed_flag_is_set`` and ``set_needed_flag``. The global variable
``needed_flag_bit_number`` indicates the bit number to be tested or set.
If it is zero, the ``needed`` flag is tested or set instead of the bit
vector.

Each instantiation is actually assigned two bits in the bit vector, the
second one used for the per-instantiation versions of the class and
function ``definition_needed`` flags. These are tested and set by
``class_definition_needed_flag_is_set``,
``set_class_definition_needed_flag``,
``routine_definition_needed_flag_is_set``, and
``set_routine_definition_needed_flag``. As with the needed-flag macros,
these macros depend on the value of ``needed_flag_bit_number`` and use the
``definition_needed`` flags if that variable is zero.

The bit number assigned to a given instantiation entity is recorded in the
``instantiation_needed_bit_number`` field in the variable or routine. When
``mark_as_needed`` is called for the entity, after the standard "needed"
flag processing has been done, another sweep is done to set the proper bit
in the per-instantiation "needed" flag bit vectors of all entities
referenced from the instantiation. The class and function per-instantiation
definition-needed bits are also set in this sweep.

All externally-defined entities that are not instantiations are assigned to
the slice numbered 1 (though their ``instantiation_needed_bit_number``
fields are left as zero), and a similar sweep is done for them. By the end
of the compilation, therefore, every externally-defined entity has been
assigned to a slice, and the per-instantiation bits show what must be
included in each slice.  The "1" slice includes everything that isn't in an
instantiation slice.

Some entities are present in multiple slices.  Constants, types, and class
definitions appear in each slice as needed. Inline functions are also
present in a slice if they are referenced in that slice. In general,
however, functions and variables can only appear in one slice, or more
precisely, their definitions can only appear in one slice. If other slices
reference them, they appear only as declarations in those slices. This
causes a problem for static functions and variables, which by definition
cannot be referenced from other object files. This is handled by making
those entities external with generated names. The sweeping routines will
set the ``static_used_by_instantiation`` flag in static entities that are
used by an instantiation, and IL lowering will turn such routines and
variables into external entities.

The simple ``needed`` and ``definition_needed`` flags end up being in
effect the unions of the corresponding per-instantiation sets.  That is, if
any per-instantiation "needed" flag is set on an entity, then the
``needed`` flag will be set, and likewise for the definition-needed
flags. Therefore, when the normal processing to remove unneeded entities is
done, it will remove only entities that are unneeded in all slices.

Intermediate Language Display
=============================

``il_display.c`` contains routines to display the IL in human-readable
form. ``il_display.h`` contains the associated declarations. The code is
currently set up to be compiled as a standalone utility program: It reads
the IL from a file (by calling the IL read routines), then traverses the
in-memory form using the IL walk routines, producing output on
``stdout``. ``il_display`` could be set up as a subroutine to be called
from within the front end.

The processing is very simple: Using the IL walk routines, it walks the
file scope and displays it, then walks each routine scope and displays
that. The IL walk routines are told to call ``disp_entry`` for each
non-string entry, and to call nothing for string entries (string entries
are displayed when pointers pointing to them are displayed). Most of the
display routines are very straightforward. When pointers are displayed, an
attempt is made to show what is pointed to. That is, if the entity pointed
to is named, or if it is a string, a constant, or a type, a brief summary
of the entity is given so one does not have to look at, say, a type entry
to find out that it is the entry for ``int``.

If there are errors in the IL, they are highlighted (e.g.,
"**BAD-TYPE-SPECIFIER-KIND**") and the display continues as well as it
can. An internal error is not generated.

Obviously, the code in ``il_display.c`` must be kept up-to-date whenever
the IL definition in ``il_def.h`` is changed.

Writing the Intermediate Language to a File
===========================================

``il_write.c`` contains routines that write the intermediate language to a
file, and ``il_write.h`` contains associated declarations.

IL writing takes the in-memory IL and writes it to a file for later
reading.

``start_il_file`` is called to open the IL file, and ``finish_il_file`` to
close it. Alternatively, ``cancel_il_file`` can be called to delete the IL
file and suppress further IL file generation if an error is detected during
the compilation.

``write_memory_region`` is called to write out each memory region, i.e.,
the file-scope memory region or a memory region for a function. It writes
the memory region at the end of the intermediate language file.

There are two IL file formats. The switch ``ALTERNATE_IL_FILE_FORMAT`` in
``host_envir.h`` indicates which is to be used. The file header is the same
in both formats:

* | A special string identifying the file as an IL file, and including the
    version number of the IL (see ``il_file.h``).

* | The number of memory regions.
* | The number of function definitions.
* | The file position of the file index (at the end of the file).
* | The file position of the file-scope memory region.
* | The ``il_header`` struct, which is the root of the IL tree.
* | The array of orphan-list pointers.

In terms of processing, the space for the header information is reserved at
``start_il_file`` time, but the full information is not known until all
regions have been written. At ``finish_il_file`` time, the file pointer is
repositioned and the final information is overwritten on the reserved
space.

The file index is also the same in both formats: It is an array of file
positions, such that file-index[*i*] gives the file position of the start of
the information for memory region *i*. The index is the last thing written in
the file. No entry 0 is written; the first entry is entry 1 (i.e., for the
file scope), which is at the position indicated by the file index position
in the header. The other entries follow. There are as many entries as the
"number of memory regions" given in the header.

Between the header and the file index is a sequence of one or more memory
regions. Since the file-scope region is not complete until the end of the
compilation, the file contains all the function regions first, in order of
definition, and then the file-scope region at the end.

In both formats, the information for a memory region begins with the memory
region number, and the overall sequence of regions is followed by a zero
which serves as an end marker if the regions are read sequentially.

Beyond that, the information for a memory region is quite different in the
two file formats.

The default file format involves writing out the memory blocks for each
region exactly as they appear in memory -- i.e., large blocks, in memory image
form. On the reading side, these blocks are read back in, and since they
are probably not read in at the same address they had when written out, the
IL tree is walked to find and remap all the pointers. A tree walk is also
required on the writing side, at least for function scope memory regions,
to get the data structure for orphaned entries built. The process is fairly
fast on both the writing side and the reading side; although the tree walk
takes time, at least the I/O for reading the blocks can be done
efficiently, in large blocks.

The information for a region in the default format is as follows:

* | The memory region number.
* | The original memory address for the first block header.
* | The original memory address for the primary scope entry for the region.

* | The total size in bytes of the information following (i.e., the total
    size of all blocks and their associated headers).

* | For each memory block, the block header and block data. The header
    contains pointers that point to the next block and to the data (all as
    original memory addresses).

In the alternate file format, each entry is assigned an entry number (which
is recorded in the prefix preceding the entry), all pointers are changed to
the corresponding entry numbers, and each entry is written individually to
the file, preceded by an indication of the entry kind and by the entry
number. On the reading side, the entries are read individually and the
pointers in them are changed to the proper new pointer values. This format
is less efficient than the default format, because the actual I/O is done
one entry at a time for both writing and reading (there is of course
buffering under this, but it is still less efficient to make thousands of
calls to ``fwrite`` and ``fread`` than to make dozens of calls). The
advantage of the alternate file format is that it allows resizing or other
alteration of the entries on the reading side. If one wants to add
back-end-specific information, for example, one can do so.

The information for a region in the alternate file format is as follows:

* | The memory region number.

* | An array giving, for all the IL entry kinds (see ``il_walk.h``), the
    number of entries of each kind in this memory region.

* | A sequence of entries, giving for each entry the entry kind, the entry
    number, the entry length (only for string entries), the orphan-list
    pointer (only for non-string file-scope memory region entries), and the
    entry itself. After the last entry, a zero entry kind marks the end of
    the sequence.

Entry numbers are assigned from 1 for each kind of table; there can be, for
example, a type entry 27 and a constant entry 27. 0 is used to represent a
NULL pointer. The entry numbers in each function scope start again from 1,
but in addition they have the top bit on, i.e., they effectively start with
``LONG_MAX+2``. Thus there can be identical entry numbers in two function
scopes (and that is not a problem), but a file-scope entry number can
always be distinguished from a function-scope number.

For string entries, the "entry number" used is an offset into a conceptual
block formed by concatenating all the strings of that kind, including the
terminating null if any, and with space reserved between the strings for
the entry prefix and for any required alignment. The offsets start from 1
to preserve 0 as a NULL pointer. In the array of entry counts, the values
for string entries indicate the total size of the conceptual block, in
bytes, rather than an entry count. Thus, if there were three strings "``abc``",
"``de``", and "``fghi``" in a region, the prefix takes 4 bytes, and the host
machine requires 4-byte alignment, the strings' entry numbers would be 5,
13, and 21, respectively, and the entry in the count array would be 25.

Reading the Intermediate Language from a File
=============================================

``il_read.c`` contains routines that read the intermediate language from a
file, and ``il_read.h`` contains associated declarations.

File reading takes the IL in file form and reads it in to re-create the
in-memory form.

The routine ``il_read`` reads the file header and the file index, then
calls ``read_memory_region`` to read the file-scope memory region. Other
(function-scope) regions must be read explicitly by calling
``read_memory_region`` directly. This allows the back end to read each
function, process it, and then free its IL by calling
``free_memory_region``, to reduce overall memory requirements.

With the default file format, the information for each memory region is
read with one large read, and the pointers in the region are then remapped
to the proper values by calling ``walk_file_scope_il`` or
``walk_routine_scope_il`` with a pointer remap function. If all the memory
blocks are read back in at their original addresses (an attempt is made to
do this), the IL walk is not needed.

With the alternate file format, the array of entry counts is read at the
beginning of processing the region. For each entry kind, an area is
allocated that is large enough to contain all the entries of that kind (its
size is the number of entries times the size of each entry). This area is
then considered an array of entries of that kind, and the entry number is
an index into the area. (For string entries, the "entry count" is the
actual total size, and the "entry number" is still an index -- with the area
treated as an array of characters.) Each entry is then read, placed at the
right place in the allocated space, and its pointers changed from entry
numbers back to real pointers. Note that no overall tree walk is done, and
no special orphan processing is required: Since each entry is read
individually, one can do all the processing for each entry when it is read,
and be sure no entries were missed.

As each region is read, the associated entry of ``region_scope_entry`` in
``il_header`` is set to point to the primary scope entry for the region.

Precompiled Headers
===================

The precompiled headers ("PCH") facility is intended to speed up
compilations that include relatively stable header files. It does this by
writing a precompiled header file that is a snapshot of the front end's
internal state at a point in the compilation process immediately following
the initial sequence of ``#include``\ s. On a recompilation of that file
(or another that begins with the same sequence of ``#includes``), the front
end checks to see if all the header files are unchanged since the
precompiled header file was generated, and if so reads in the precompiled
header file to re-establish the front end state as it would be after
reading and compiling the header files. Reading in the PCH file takes less
time than compiling the header files from source, so there is a net savings
in compilation time.

Since the point of this feature is saving time, it is important that the
process of writing and reading the PCH files be as fast as possible. One
would like to see savings of 80% or the like (comparing the time to restore
a PCH file to the time to compile the original source code); an improvement
of only 30% or so would barely be worth the extra complexity. In addition,
one should not have to pay a large price to write out the precompiled
header file; it's okay to slow compilations a little to get the benefits of
precompiled headers on later compilations, but if it takes too long to
write the file, the net gain is marginal or negative.

These speed issues suggest some basic design decisions:

* | The information written to the precompiled header file must not require
    a tree walk, because visiting every entry takes a lot of time. (It also
    requires code or data structures that describe how to do the tree walk,
    which exist for IL entries but not for the front-end-only data
    structures.) Therefore, the information must be written out and read
    back in as large chunks (rather than one entry at a time), and it must
    not be necessary to remap all pointers in the information to new values
    (i.e., it must be possible to bring the information back in at the same
    address). These requirements also make it possible to exploit memory
    mapping on systems that support it.

* | Precompiled header files should contain snapshots of compilation
    states, and not information about individual header files. While it's
    tempting to imagine writing precompiled header files for individual
    header files, the work required in carving out the right information
    and the description of dependencies so that the information can be
    written to a file, and the work to bring it back in and merge it with
    the front end state, will make substantial time savings unlikely.

An unfortunate consequence of this latter design decision is that users are
probably forced to change their source code to get any meaningful benefit
from the PCH facility. In average source code, the order of inclusion of
header files is essentially random. The net effect of the various orders of
inclusion is often the same, because of the way header files tend to be
written, but it is difficult to deduce that fact, and time-consuming. So we
have a dilemma: If we strive for maximum save/restore speed, we will find
few "natural" common sequences, and therefore generate many PCH files for
little benefit. On the other hand, if we try for maximum ability to reuse
precompiled headers from unaltered source files, we lose something
significant on save/restore speed.

We've chosen to go in the direction of maximum save/restore speed. We
believe that users are willing to make small changes in their source code
to get large reductions in compilation time, and that given a choice
between

#. | You don't have to change your source, and precompiled headers will
     give you a 15% savings, or

#. | You may have to reorder your initial ``#include``\ s or add the
     equivalent of

       ``#include <everything.h>``

   | to the beginning of your source files, and precompiled headers will
     give you a 50% savings,

users will choose option 2. This, of course, is a matter of opinion, and
the percentages are fictitious. But for better or worse, this is what we
decided.

Because PCH processing is not likely to be helpful on arbitrary files, it
is disabled by default.

The code for precompiled header processing is mostly in ``pch.c``, with
associated declarations in ``pch.h``.

Prefix and Header Stop Point
----------------------------

The initial sequence of the source code of the primary source file that is
subject to the PCH optimization is referred to as the *prefix*. It consists
of the preprocessing directives preceding the *header stop point*. The header
stop point is determined as follows:

* | Start at the first token of the primary source file that is not part of
    a preprocessing directive (or the end of the file if there is no such
    token).

* | If there is a ``#pragma hdrstop`` preceding that token, back up to the
    pragma position.

* | If the position determined by the first two steps is inside a
    preprocessing directive like an ``#if``, back up to (just after) the
    previous top-level preprocessing directive.

For example:

.. code:: c++

   /* Comments. */
   #define flag 1
   #if flag
   #include "abc.h"
   #else
   #include "def.h"
   #endif
   #include <stdio.h>
   /* header-stop point. */
   int i = 1;
   #include "abc.h"
   /* header-stop point. */
   #if xyz
   #include "def.h"
   #pragma hdrstop
   #endif

Note that, while determining the prefix, ``#if``\ s and the like are not
evaluated, so code that will be skipped in the actual compilation can
nevertheless serve as a header stop point. This is particularly important
with ``#pragma hdrstop``.

At the beginning of compilation, if precompiled headers are enabled, the
prefix for the primary source file is accumulated. This is done by opening
the primary source file, reading the beginning of it in a special mode
while building an in-memory representation of the preprocessing directives
read, and closing the source file. This is a separate process from the
normal compilation of the file, and is a necessary first step whether the
front end ends up using an existing PCH file or creating a new one. See
``build_prefix_information``.

The prefix scan is done by setting ``building_pch_prefix`` and calling
``get_token`` once. That causes the scanning of all of the preprocessing
directives preceding the first "real" token of the primary source file. The
preprocessing directives are not evaluated in the usual way; they are just
placed on the prefix event list. (In particular, ``#include`` directives
are just accumulated like the others; the included files are not read.)
Each such event contains an enumeration code for the preprocessing
directive and a null-terminated string giving the text of the directive
following the directive keyword.

After the initial prefix list is built up, the list is trimmed down by
discarding anything after a ``#pragma hdrstop`` and then, if the end is
inside an ``#if`` or the like, discarding preprocessing directives back to
the last top-level directive.

If the primary source for the compilation is coming from ``stdin`` instead
of a file, the prefix construction cannot be done (and therefore PCH files
cannot be created or used).

A list of the command-line options for the current compilation is also
built. It is also a linked list, and each entry on the list indicates one
command-line option. Options that have no effect on the state preserved by
PCH processing are not put on the list, but the rest (most of them)
are. The option code is represented by an enumeration, so the difference
between the single-letter form of the option and the keyword form is
erased, but in all other respects the list built up will have to match the
corresponding list for a PCH file exactly -- same options, same order.

Using a PCH file
----------------

Once the prefix has been created, the front end will look to see if any of
the existing PCH files can be used to avoid some part of the compilation
for the current file. A host-dependent routine is used to fetch the names
of all PCH files in the appropriate directory (the directory can be
specified via the ``--pch_dir`` command-line option). Each file is checked
to make sure it is a regular file (not, say, a directory), that it has the
right header string identifying it as a PCH file, that the compiler version
number and generation date/time are the same, and that the flag indicating
that the file is completely written has been set. If any of those checks
fails, the file is closed and not considered further; in effect, it's
considered not to be a PCH file.

The next round of checks seeks to determine whether the PCH file is
applicable to the current compilation. The front end checks that the
current directory and the one recorded in the PCH file are the same; that
the command-line event lists are the same; and that the preprocessing
directive event lists are the same (or the current file's prefix list is
longer than the prefix list in the PCH file). If those are okay, the front
end checks that the header files included in the PCH file have not changed
since the PCH file was built (this is done by checking the timestamps of
the files against the timestamps recorded in the PCH file). If the
timestamps indicate that the PCH file is out of date, it is deleted.

A PCH file that makes it this far can be used, but it might not be the best
choice: There might be another PCH file that also matches but includes more
of the prefix preprocessing directives. Therefore, if the match is not
perfect, other PCH files are considered and the best-matching PCH file is
chosen. Each PCH file is closed after it is considered and must be reopened
later if selected as the best file.

If the user has used the ``--use_pch`` command-line option, the indicated
PCH file is chosen directly. No other PCH files are considered, and the
applicability test is not done (at this point; see below).

Once the best-matching PCH file is selected for a file called (say)
``x.C``, if the PCH file selected is not ``x.pch``, and there exists an
``x.pch``, ``x.pch`` is deleted. This is a simple way of getting rid of PCH
files that are no longer needed.

Once a PCH file is selected, the front end state is restored from the
file. The PCH file is opened again and the prefix check is done again to
make sure the file has not changed since the first check. For a PCH file
specified via a ``--use_pch`` command-line option, this is the only
applicability test done (and if the file fails, a warning is issued and the
compilation continues without using the PCH file); for other cases, this is
a repetition of the applicability test already done. Next, the global
variables registered via ``register_pch_saved_variables`` are read and
restored. Finally, the contents of the various memory regions (IL and front
end) are restored. These are restored at the original addresses, so it is
not necessary to walk through the restored data to adjust pointers. (See
the section on memory management issues below for more information on this
aspect.)

The restoration process occurs after most initialization of the front end,
so some variables are initialized then overwritten by the restoration from
the PCH file.

Once the front end state has been restored from the PCH file, the lexical
routines are reset and the primary source file is opened again. The prefix
is skipped by doing a ``get_token`` in a special lexical mode. This is very
similar to the scan done to accumulate the prefix, in that initial
preprocessing directives are passed over uninterpreted. However, once the
preprocessing directive that marks the end of the matched part of the
prefix is processed, normal processing of preprocessing directives is
re-enabled. Shortly thereafter, the initial ``get_token`` finishes, and
normal compilation continues from that point (we know we are between
top-level declarations, so we can continue simply by entering the main
compilation-unit loop).

The PCH restoration process restores the front end state after compilation
of the precompiled headers, but it doesn't duplicate the output that would
have been done on auxiliary files during the skipped part of the
compilation. Diagnostics that would have been written, and output lines on
the raw listing and cross-reference output files that would have been
generated, are not put out when a PCH file is used. If the front end is
configured to use an IL file, that file also is not generated as this
point. However, the IL file does get generated by a catch-up process done
after the point where a PCH file is generated or a decision is made not to
generate one; see below.

Because the source sequence numbers of the original compilation may not
exactly match the sequence numbers in the current compilation (e.g.,
because of whitespace differences), the source file mapping information in
the IL in the PCH file is restored as, in effect, the first include file on
the list under the primary source file of the current compilation.

Creating a PCH File
-------------------

After the front end has read in a PCH file or chosen not to use one, the
"normal" compilation process begins. When the compilation reaches the
header stop point, we have the option of creating a PCH file that
encapsulates the front end's state at that point for later restoration.

Note that it is possible to both use and create a PCH file in the same
compilation, because the best-matching PCH file might contain only the head
of the full prefix. In that case, the internal state is restored from the
PCH file, then the rest of the prefix is compiled in the normal way, and at
the header stop point the compiled form of the full prefix is saved to a
new PCH file.

The position of the header stop point was determined by the prefix scan at
the very beginning of the compilation. Its position is defined in terms of
a preprocessing directive, more specifically as the end of the processing
for that directive. Compilation starts up or continues in a mostly normal
way, with the header stop position set as a sort of breakpoint. When the
end of the indicated preprocessing directive is reached, the front end will
check to see whether a PCH file can be generated.

On the way to the header stop point, some things may happen to preclude
generation of a PCH file:

* | A reference to one of the macros ``__DATE__`` or ``__TIME__``. Since
    these would change from compilation to compilation, there is no point
    to saving the state of a compilation that uses them.

* | A ``#pragma no_pch``. This is an explicit request not to generate a PCH
    file.

* | A ``#line`` directive. (Such directives could probably be allowed but
    are not accommodated at present.)

When the header stop point is reached, ``generate_precompiled_header`` is
called. It starts by making several checks that the current position is
acceptable:

* | The current position is not inside a declaration, i.e., it is between
    two file-scope declarations, or before the first such declaration, or
    after the last. ``next_token_is_top_level_decl_start``, maintained by
    the declaration-scanning routines, gives this information.

* | The preprocessing ``#if`` stack is empty, i.e., the current position is
    not inside a conditionally-compiled section.

* | There have been no errors.

* | The global variable ``cannot_create_pch_file`` is not set. It is set
    for things that preclude generation of a PCH file (e.g., referencing
    ``__DATE__``).

If the current position is acceptable, a precompiled header file can be
written, and the front end will write out a file *xxx*\ ``.pch``, where "*xxx*"
is the base file name of the file being compiled. The user can specify a
different file name via the ``--create_pch`` command-line option. If a file
with the chosen name already exists, it is deleted. Precompiled header
files are written in the current directory by default, but another
directory can be specified via the ``--pch_dir`` command-line option.

The following information is written to the PCH file, in the order given:

* | An string that identifies the file as a PCH file, and gives the front
    end version number and generation date/time.

* | The current directory name (this may affect the include file search
    order, etc.).

* | The list of command-line events, i.e., all the command-line options
    that could have an effect on PCH files.

* | The list of preprocessing directive events, i.e., the sequence of
    preprocessing directives whose compiled form is encapsulated in the PCH
    file.

* | Timestamps for all the header files referenced by the compilation (this
    includes all files, not just those included directly from the primary
    source file).

* | The values of all front end variables registered with
    ``register_pch_saved_variables``.

* | The contents of all memory regions, i.e., the front end memory region
    and all IL memory regions.

* | Once the rest of the information has been written, the front end goes
    back to the header and writes a flag indicating that the PCH file is
    complete. This flag prevents other compilations from using a PCH file
    that is not yet completely written.

After writing out the PCH file, the front end continues with
compilation. Because the header-stop point has been passed, no further
precompiled-header checking or processing is done in the rest of the
compilation.

If an IL file is being generated, function scope memory regions that are
completed before the header stop point are held in memory and neither
written to the IL file nor freed. Once the header stop point has been
reached, and a PCH file has been written or a decision made not to write
one, those memory regions are written to the IL file and freed.

.. _memory-management-issues:

Memory Management Issues
------------------------

As was mentioned earlier, one of the prerequisites for fast PCH processing
is the ability to bring memory regions back into memory at the same
addresses they had when written out. Clearly, that requires that (a)
nothing else happens to have been allocated at the desired addresses, and
(b) there is some way to request allocation or assignment of those
addresses so that no later allocation uses them.

The front end provides two schemes that accomplish this goal. The first and
better scheme requires memory mapping support from the underlying operating
system (UNIX and Windows NT versions are provided). A fixed set of
addresses is chosen for all memory regions (the exact addresses are either
left to the discretion of the operating system or chosen in the
configuration of the front end; see
``USE_FIXED_ADDRESS_FOR_MMAP``). Initially, these memory addresses are
mapped onto a temporary file. When a PCH file is created, the memory is
written to the PCH file (using normal file I/O). On restoration of a PCH
file, the default mapping to the temporary file is released, and the
reserved memory addresses are mapped onto the PCH file being restored. This
brings the saved information into the front end's address space without any
immediate I/O. (In the actual implementation, the memory addresses are
tentatively mapped once to verify that the mapping can be done, and then,
once it is known that all the mappings will be successful, they are done
again with the official mappings.) All dynamically allocated memory that is
not part of a memory region is just allocated in the normal way through
``malloc``, since it need not be at any particular addresses. This means,
in effect, that there are two completely separate pools of memory -- memory
region allocations are done at the fixed mappable addresses, and everything
else is done more casually in the traditional way.

The other implementation does not require memory mapping, but it is
slightly less friendly to users. Very early in the execution of the front
end a large amount of memory is preallocated in large blocks and saved off
to the side for use exclusively for memory regions. This is done early so
that the allocated addresses are reproducible (i.e., one can get the same
addresses on all compilations). If the allocations were done later, their
addresses would be affected by minor variations in the amount of memory
used during the command-line processing, and perhaps by memory allocated by
runtime routines of the host C compiler (e.g., ``printf`` formatting). As
the compilation proceeds, requests for memory for memory region blocks are
satisfied by taking blocks from this preallocated list. All other
allocations (for things not in memory regions) are done directly through
``malloc``. When a PCH file is created, the memory addresses of blocks are
written to the PCH file along with the block contents. Also written is an
array of the preallocated memory block addresses used, serving as a sort of
allocation history (this array is written to the file also in the
memory-mapped mode). Once the header stop point is passed, and either a PCH
file was written or a decision was made not to write one, the remaining
preallocated memory is freed, and subsequent allocation for all purposes is
done directly through ``malloc``.

If the preallocated space is too small to hold the memory regions created
up to the header stop point, the precompiled header cannot be created
(since the remaining required space cannot be allocated at predictable
addresses). A warning is issued, telling the programmer that the
``--pch_mem`` option can be used (on a subsequent invocation of the front
end) to request a larger preallocation to avoid the problem. The
compilation continues normally -- the PCH file cannot be created, but the
compilation can be completed regardless.

Since the preallocated blocks have a fixed (though large) size, any
requirement to allocate a single entity larger than that fixed size cannot
be satisfied from the preallocated pool, and therefore precludes generation
of a PCH file. This could happen, for example, if a user program contains a
single literal string containing 200,000 characters. Again, the compilation
continues without difficulty, but a warning is issued that a PCH file could
not be generated.

When a PCH file is read back in the non-memory-mapped mode, the allocation
history information history is read in from the PCH file and used to check that
the preallocated blocks are indeed at the same addresses (on some systems, in
spite of the care taken to allocate the blocks early, the addresses still
change; for example, under MS-DOS, the addresses can be affected by placing a
new TSR program in memory).  If the addresses are not right, it is still
possible to give up on using the PCH file and continue with normal compilation.
If the history information matches, the memory regions are read in: The address
for each block is read from the file, the preallocated block at the right
address is assigned, and the memory region contents are read from the PCH file
into the proper addresses.

It should be noted that if a given environment provides a (nonstandard) way
of allocating memory from a pool distinct from the normal ``malloc``
allocation pool, the non-memory-mapped approach can be made to work without
preallocation and without the ``--pch_mem`` command-line option.

When multiple source files are specified in one invocation of the front
end, the preallocation scheme breaks down: The first file can be processed
okay, but on subsequent files the preallocated memory has been used and
reclaimed and is no longer in quite the right predictable state. This can
probably be solved in some way, but the current implementation takes the
easy way out and disallows PCH file use on compilations where more than one
source file is specified on the command line.

.. _predefined-macro-defs:

Predefined Macro Definition File
================================

Macros may be predefined either by explicit calls of
``enter_predef_macro``, and/or through use of a predefined macro definition
file. Whether or not the front end reads a predefined macro definition file
is controlled by the ``DEFAULT_USE_PREDEFINED_MACRO_FILE`` macro. The file
``EDG_BASE/EDG_AUXILIARY_INFO_DIR_NAME/PREDEFINED_MACRO_FILE_NAME`` is read
to provide the macro definitions. The ``EDG_BASE`` directory can be
specified by an environment variable (``EDG_BASE``), build-time macro
(``DEFAULT_EDG_BASE``), or command-line option
(``--edg_base_dir``). ``EDG_AUXILIARY_INFO_DIR_NAME`` and
``PREDEFINED_MACRO_FILE_NAME`` are configuration macros with default values
of "``lib``" and "``predefined_macros.txt``". If
``EDG_AUXILIARY_INFO_DIR_NAME`` is null, the ``EDG_BASE`` directory is used
directly. The format of the entries in the file is:

.. code:: text

   mode,!mode,mode   cannot_redefine   macro_name   macro_value

* | ``mode`` is a label from the predefined macro modes table. The macro is
    defined if the mode is set, or if the mode is not set when ``!mode`` is
    used. The macro is defined if any of the mode tests is TRUE. The set of
    mode values is specified in ``host_envir.h``.

* | ``cannot_redefine`` indicates whether the predefined macro may later be
    redefined. The value must be "yes" or "no".

* | ``macro_name`` is the name of the macro to be defined.

* | ``macro_value`` is the value to which the macro should be defined. All
    of the characters until the end of the line are used as the macro
    value.

For example:

.. code:: text

   gcc no  __HAVE_BUILTIN_SETJMP__ 1
   gpp no  __CHAR_BIT__ 8
   gnu no  __GNUC_PATCHLEVEL__ 0
   gnu no  __WINT_TYPE__ unsigned int
   gnu,microsoft __IS_GNU_OR_MICROSOFT_MODE 1

A script (``make_predefined_macro_file``) is provided that automatically
generates the ``predefined_macros.txt`` file for a given version of gcc/g++
or clang. It does this by invoking gcc and g++ or clang and capturing the
predefined macro information.
