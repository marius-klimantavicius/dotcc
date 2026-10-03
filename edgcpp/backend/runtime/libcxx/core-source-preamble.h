// Match libc++18 CMakeLists.txt:559/564 for library implementation compilation.
#define _LIBCPP_BUILDING_LIBRARY
#define _LIBCPP_REMOVE_TRANSITIVE_INCLUDES
#include <__config>
// This managed ABI has no old native ABI-v1 inline export consumers.
#undef _LIBCPP_DEPRECATED_ABI_LEGACY_LIBRARY_DEFINITIONS_FOR_INLINE_FUNCTIONS
