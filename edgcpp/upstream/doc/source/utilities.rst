.. _utility-programs:

================
Utility Programs
================

Several utility programs are provided with the front end.  Most of the utility
programs help with various link-time issues and are meant to be called from a
driver program or script.  One (``mk_errinfo``) is a tool program used when
modifying the front end.  The source for these programs is in the ``util``
directory of the release structure.

``edg_prelink``
===============

``edg_prelink.c`` contains the "prelinker" program.  ``edg_prelink.h`` contains
related declarations.  The prelinker is invoked at link time to manage
automatic instantiation of template entities.  It is given a complete list of
the object files and libraries that are to be linked together.  It examines the
external names defined and referenced within those files, and finds cases where
template entities are referenced but not defined.  It then examines information
in the object files that describes instantiations that could have been done
during compilation, and assigns the needed instantiations to appropriate files.
``edg_prelink`` then invokes the compiler again to compile those files, which
will do the necessary instantiations.

The prelinker uses UNIX commands like ``nm`` to extract information about
object files, so it may need adaptation to run in other environments.

The invocation of the prelinker looks like

   | ``edg_prelink`` [ *options* ] *files*

where the *files* list includes all object files and libraries, and the
*options* are:

.. list-table::

   * - | ``-a`` *n*
     - | Specify whether a definition list file should be created.  A
         definition list file is created if *n* is nonzero.  The default is
         specified by the ``PL_DEFAULT_USE_DEFINITION_LIST_FILE`` configuration
         flag.
   * - | ``-b``
     - | List object files mode.  This mode is used when the driver needs to
         get a list of object files including any instantiation object files
         created in one instantiation perobject mode.  The prelinker simply
         creates an object file list that is written to the file specified by
         the ``-o`` option.
   * - | ``-c`` *command*
     - | Specify the version of the ``nm`` command to be used.
   * - | ``-D``
     - | Do not assign instantiation to nonlocal object files.  Instantiations
         may only be assigned to object files in the current directory.
   * - | ``-f`` *fmt*
     - | Specify the ``nm`` format to be used.  *fmt* is one of ``solaris``,
         ``SGI``, ``SVR4``, ``HPUX``, ``gnu``, ``CLIX, MacOSX``, or\
         ``MacOSX64``.
   * - | ``-i``
     - | Ignore ``nm`` output lines that are not formatted properly.
   * - | ``-L`` *directory*
     - | Specify a library directory to be searched for libraries.
   * - | ``-m``
     - | Do not demangle identifier names that are displayed.
   * - | ``-n``
     - | Update the instantiation list but don't recompile the files.
   * - | ``-N``
     - | If a file from a nonlocal directory needs to be recompiled, do the
         compilation in the current directory.  An updated list of object files
         and library names is written to the file specified by the ``-o``
         option so that the driver program can tell that alternate versions of
         some of the object files should be used.
   * - | ``-o`` *file-name*
     - | Write an updated list of object files and library names is written to
         the file specified by *file-name*.  This option must be specified when
         the ``-N`` or -O option is used.
   * - | ``-O``
     - | One instantiation per object mode is being used.  A list of object
         files, including the instantiation object files associated with the
         object files specified on the prelinker command line, is written to
         the file specified by the ``-o`` option.
   * - | ``-q``
     - | Turns off verbose mode.
   * - | ``-r``
     - | Don't stop after a certain number of iterations.  (The instantiation
         process is iterative: a recompilation may bring up new template
         entities that need to be instantiated, which requires another
         recompilation, etc.  Some recursive templates can cause iteration that
         never terminates, because each iteration introduces another new entity
         that wasn't previously there.  By default, this process is stopped
         after a certain number of iterations.)
   * - | ``-R`` *number*
     - | Override the number of reserved instantiation information file lines
         to be used.
   * - | ``-s`` *number*
     - | Specifies whether the prelinker should check for entities that are
         referenced as both explicit specializations and generated
         instantiations.  If *number* is zero the check is disabled, otherwise
         the check is enabled.
   * - | ``-S``
     - | Suppress instantiation flags in the object files.  This causes the
         prelinker to recompile all of the local object files with the
         ``--suppress_instantiation_flags`` option.
   * - | ``-u``
     - | Specify whether external names have an added leading underscore.  The
         command-line option selects the opposite of the configuration flag
         ``TARG_EXTERNAL_NAMES_GET_UNDERSCORE_ADDED``.
   * - | ``-v``
     - | Verbose mode.
   * - | ``-d``\ *n*
     - | Set the debug level to *n*.

The *files* list may also have the following options intermixed with the file
names:

.. list-table::

   * - | ``-l``\ *xxx*
     - | Specify a library (e.g., ``-lstd``).
   * - | ``-B``\ *linkage*
     - | Specify the kind of linking to be done (e.g., static or dynamic).  The
         linkage kind is not checked for validity.
   * - | ``--``
     - | Marks the end of the list of file names that is passed back to the
         driver when the ``-N`` option is used.

``edg_munch``
=============

``edg_munch`` is the EDG version of the USL ``munch`` utility.  Its source
code is in ``edg_munch.c``, with associated declarations in
``edg_munch.h``.  ``edg_munch`` implements a lowest-common-denominator
method for getting global initialization and termination code executed on
systems that have no special support for that.  [#f1]_

``edg_munch`` operates by reading as input a file that is the output of ``nm``
run on a linked executable program.  It looks for names beginning with
``__sti__`` or ``__std__``, those being, respectively, initialization and
termination routines to be called at runtime.  It generates a C program that
defines a data structure containing a list of pointers to the initialization
and termination routines.  This generated program is then compiled and linked
in with the executable.  The data structure is consulted at runtime by startup
code invoked from ``_main``, and the routines on the list are invoked at the
appropriate times.

``edg_munch`` takes only a single option: ``-u``, if used, specifies whether
external names have an added leading underscore.  The command-line option
selects the opposite of the configuration flag
``TARG_EXTERNAL_NAMES_GET_UNDERSCORE_ADDED``.

``edg_decode``
==============

``edg_decode`` is a name demangler.  The demangling code is in ``decode.c``,
with associated declarations in ``decode.h``.  A main program that uses the
demangling code is provided in ``edg_decode.c``.

``edg_decode`` reads input from ``stdin`` and writes output to ``stdout``.
Things that look like mangled names in the input are demangled.  Everything
else is passed through unchanged.  This makes the program suitable, for
example, as a filter for the output of a linker: error messages in general are
passed through unaltered, but mangled names are demangled.  For example, using
the cfront-like ABI,

.. code:: text

   ld: Undefined symbol
      _f__Fif

is turned into

.. code:: text

   ld: Undefined symbol
      f(int, float)

Mangled names that are ill-formed in some way are also passed through
unchanged.

``edg_decode`` usually has only a single option: ``-u``, if used, specifies
whether external names have an added leading underscore.  The command-line
option selects the opposite of the configuration flag
``TARG_EXTERNAL_NAMES_GET_UNDERSCORE_ADDED``.  When the IA-64 ABI is used,
however, there is also a ``-g`` option, which selects the opposite of the
configuration flag DEFAULT_EMULATE_GNU_ABI_BUGS, which when TRUE extends the
demangling to cover some invalid mangled names put out by g++ 3.2 (and the EDG
front end, when configured to emulate those bugs).

The demangling is intended to work only on names of external entities.  There
is some name mangling done for internal entities, or by the C-generating back
end, that this program does not try to decode.

In the cfront-like ABI, names that contain double underscores in the source
code will probably not be demangled correctly.  This is an unfortunate
consequence of using a mangling scheme that is based on the one used by cfront.
The standard forbids use of double underscores in names in user code, reserving
such names for the implementation, so the effect of this shortcoming is
limited.  Implementors should try to avoid using such names for entities with
external linkage.  Double underscores at the beginnings of external names do,
however, work correctly.  In the IA-64 ABI, a similar problem applies to names
that begin with "``_Z``".

The name demangling is also available through a callable interface, which is
used in ``edg_prelink``.  See routine ``decode_identifier``.

.. _mk-errinfo:

``mk_errinfo``
==============

``mk_errinfo`` translates a text file containing error message codes and
associated descriptions into a pair of header files that represent this
information in a manner more readily used by the front end.

The invocation of the ``mk_errinfo`` is typically

.. code:: bash

   mk_errinfo error_msg.txt error_tag.txt err_codes.h err_data.h

The ``error_msg.txt`` file defines the diagnostic messages generated by the
front end.  This file, along with ``error_tag.txt``, is processed by
``mk_errinfo`` to generate the ``err_codes.h`` and ``err_data.h`` include
files.  ``mk_errinfo`` may also be used to automatically generate the file used
to generate the :ref:`error-messages` appendix of this manual.

When used to generate a documentation file, the invocation is typically one of
the following:

   | ``mk_errinfo -d error_msg.txt error_tag.txt`` *generated.tex*
   | ``mk_errinfo -mml error_msg.txt error_tag.txt`` *generated.mml*
   | ``mk_errinfo -rst error_msg.txt error_tag.txt`` *generated.rst*

*generated.tex* is the name of the generated output file containing the
documentation in LaTeX format.  *generated.mml* is the name of the generated
output file containing the documentation in MML (Maker Markup Language), which
can be used to create a FrameMaker document. *generated.rst* is the name of
the generated output file containing the documentation in reStructuredText
format, which can be used with Sphinx and various other tools.

``error_msg.txt`` contains lines that have the following form:

.. code:: text

   error_code;tag;"text"

The error code is the enumeration element name that is used by the front end.
The file must be specified in enumeration order.  To preserve the numbering of
error codes, errors may not be inserted in the middle of the file, nor may
lines be removed.  If an error message is no longer in use, the line on which
it is defined should be prefixed with the string ``REMOVED``.  This will retain
its sequence number but eliminate it from the generated tables.

The tag is an alias for a given error code to be used by the end-user to name
that error in the command-line options that change the severities of
diagnostics.  If no tag is specified, the error code (with the ``ec_`` removed)
is used as the tag.

Some messages are used as part of the formatting of other messages (for
example, the messages used to provide error context information).  These
messages should have a tag of ``INTERNAL``.  This prevents the message from
being entered in the tag table and also prevents the message from appearing in
the automatically generated documentation file.

The text string contains a quoted string similar to one that would be used in a
C program to define the equivalent string.  Any quote characters within the
text string must be escaped.

The text string includes markers indicating the location of fill-ins in the
message (e.g., ``%t`` for a type).  Full information on those fill-in codes can
be found in :ref:`error-reporting`.

The message text may include supplementary information that is only used to
improve the automatically generated documentation file (which is formatted
into the :ref:`error-messages` appendix of this manual).  For example, an
error text string might look like this

.. code:: text

   "a message with %s\='fill-in' included"

The string ``\='fill-in'`` is used when creating the documentation file as the
value to be substituted for the ``%s``.  This string is removed from the error
text information when the ``err_data.h`` file is created.

``error_tag.txt`` is used to supply additional tags that may be used as aliases
for diagnostic messages.  This mechanism is intended to be used by customers
who are packaging the front end as part of their product and allows them to
assign their own set of error tags without the need to modify the distributed
``error_msg.txt`` file.  Note that these tags are used in addition to (not in
place of) the default tags specified in the ``error_msg.txt`` file.

Each line should be formatted as:

.. code:: text

   error_code;tag

The error code is the enumeration element name used by the front end.  The tag
is the alias being assigned.  There may be more than one alias that references
a given enumerator.

In both ``error_msg.txt`` and ``error_tag.txt``, blank lines are ignored and
lines that begin with a "``#``" are treated as comments.

.. [#f1] The *patch* approach, by comparison, is more machine-dependent and
         more efficient, but still doesn't require special linker support.
         The code generated by the EDG front end will work with USL's
         ``patch``, but EDG does not provide its own version of ``patch``.
