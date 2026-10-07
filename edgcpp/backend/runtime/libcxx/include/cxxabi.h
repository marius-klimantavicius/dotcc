/* Optional Itanium demangler declaration for portable C++ consumers.
   No native RTTI layouts are exposed. A referenced demangler requires a real
   translated implementation; the backend rejects an unresolved call. */
#ifndef EDG_LIBCXX_CXXABI_H
#define EDG_LIBCXX_CXXABI_H
#include <stddef.h>
namespace __cxxabiv1 {
extern "C" char *__cxa_demangle(const char *, char *, size_t *, int *);
}
namespace abi = __cxxabiv1;
#endif
