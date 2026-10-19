/* Actual upstream core layouts with supplied target headers. No conversion
 * implementation, native wchar service substitution or product execution. */
#define Py_BUILD_CORE
#include <Python.h>
#include "pycore_runtime.h"

#ifndef R42_EXPECT_UTF8
#error "Select baseline or candidate explicitly"
#endif
#if R42_EXPECT_UTF8
#if !defined(_Py_FORCE_UTF8_LOCALE) || _Py_FORCE_UTF8_LOCALE != 1
#error "Whole locale feature missing or disabled"
#endif
#ifndef _Py_FORCE_UTF8_FS_ENCODING
#error "Upstream filesystem derivation missing"
#endif
#else
#if defined(_Py_FORCE_UTF8_LOCALE) || defined(_Py_FORCE_UTF8_FS_ENCODING)
#error "Baseline must retain its original locale selection"
#endif
#endif

_Static_assert(sizeof(wchar_t) == 2, "actual target wchar width");
#ifdef R42_NATIVE_HEADER_CONTROL
/* V22's pre-existing _Static_assert evaluator does not narrow (wchar_t)-1;
 * the retained baseline RED records that limitation. Runtime target unsigned
 * semantics are independently exercised by B41's unchanged-C counterexample. */
_Static_assert((wchar_t)-1 > 0, "actual target unsigned wchar");
#endif
_Static_assert(sizeof(void *) == 8 && sizeof(long) == 8, "target LP64");

unsigned long r42_interpreter_size(void) { return sizeof(PyInterpreterState); }
unsigned long r42_runtime_size(void) { return sizeof(_PyRuntimeState); }
unsigned long r42_main_offset(void) { return offsetof(_PyRuntimeState, _main_interpreter); }
unsigned long r42_warnings_offset(void) { return offsetof(PyInterpreterState, warnings); }

#ifdef R42_NATIVE_HEADER_CONTROL
/* Only the eight ASCII layout observations cross the real native stdio ABI. */
int main(void)
{
    printf("%lu %lu %lu %lu %lu %lu %lu %lu\n", r42_interpreter_size(), r42_runtime_size(),
        r42_main_offset(), r42_warnings_offset(), (unsigned long)sizeof(wchar_t),
#ifdef HAVE_FORK
        (unsigned long)offsetof(PyInterpreterState, before_forkers),
        (unsigned long)offsetof(PyInterpreterState, after_forkers_parent),
        (unsigned long)offsetof(PyInterpreterState, after_forkers_child)
#else
        0UL, 0UL, 0UL
#endif
    );
    return 0;
}
#endif
