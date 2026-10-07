// One pinned whole-program libc++ ABI; .NET chooses inlining.
#ifndef DOTGEN_LIBCXX_UNIFORM_CONFIGURATION
#define DOTGEN_LIBCXX_UNIFORM_CONFIGURATION
#define _LIBCPP_NO_ABI_TAG
#include <__config>
#undef _LIBCPP_ALWAYS_INLINE
#define _LIBCPP_ALWAYS_INLINE
#endif
