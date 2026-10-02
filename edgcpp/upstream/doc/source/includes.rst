============================
ANSI C and C++ Include Files
============================

Standard C++ Include Files
==========================

The following include files are supplied with the front end:

* | ``compare.stdh``
* | ``coroutine.stdh``
* | ``cxxabi.h``
* | ``exception.h``
* | ``exception.stdh``
* | ``initializer_list.stdh``
* | ``new.h``
* | ``new.stdh``
* | ``source_location.stdh``
* | ``stdexcept.h``
* | ``stdexcept.stdh``
* | ``stdfloat.stdh``
* | ``typeinfo.h``
* | ``typeinfo.stdh``

The ".  ``stdh``" files are basically the same as the definitions in the C++
standard.  The standard specifies header names without a suffix (e.g.,
``<new>``).  The front end, by default, adds the ``stdh`` suffix when searching
for the header file.  The "``.h``" versions are supplied so that older code
that uses header names like ``<new.h>`` will continue to work.

Not all of the header files for the libraries required by the C++ standard are
provided.  In general, header files are only supplied for components of the
library for which an implementation is supplied with the front end.

The ``typeinfo.h`` header file contains the definition of the ``type_info``
data structure used by the front end and the runtime library.  The
``type_info`` structure must not be changed unless corresponding changes are
also made in the front end and runtime.

The ``cxxabi.h`` header includes declarations of routines and data structures
specified by the IA-64 ABI.

Namespaces and the Standard Header Files
----------------------------------------

When front end configuration flag ``RUNTIME_USES_NAMESPACES`` is TRUE the front
end defines the preprocessing macro ``__EDG_RUNTIME_USES_NAMESPACES``.  This
flag is tested by the C++ header files to determine whether the runtime
declarations should appear in the global scope or in the ``std`` namespace.
The C++ header files also test the ``__EDG_IMPLICIT_USING_STD`` flag, which is
defined by the front end when the front end is configured to use namespace in
the runtime and when the ``--using_std`` option is specified when the front end
is invoked.  This flag causes the header files to do a ``using namespace std``
in any header file that contains declarations for entities in the ``std``
namespace.
