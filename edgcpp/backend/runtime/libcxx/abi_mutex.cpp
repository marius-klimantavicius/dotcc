// Adapted from LLVM libc++18.1.8 src/mutex_destructor.cpp and
// src/condition_variable_destructor.cpp, Apache-2.0 WITH LLVM-exception.
// See LICENSE.TXT. The external-thread managed ABI requires nontrivial
// destruction. Use the actual public class definitions; the upstream private
// redeclarations serve historical native trivial-destructor ABI compatibility.
#include <mutex>
#include <condition_variable>

#if defined(_LIBCPP_HAS_TRIVIAL_MUTEX_DESTRUCTION) || defined(_LIBCPP_HAS_TRIVIAL_CONDVAR_DESTRUCTION)
#error The managed thread provider requires nontrivial synchronization destruction
#endif
_LIBCPP_BEGIN_NAMESPACE_STD
mutex::~mutex() noexcept { __libcpp_mutex_destroy(&__m_); }
condition_variable::~condition_variable() { __libcpp_condvar_destroy(&__cv_); }
_LIBCPP_END_NAMESPACE_STD
