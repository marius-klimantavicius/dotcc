// Managed exception ABI for the pinned libc++ provider. Public class/function
// definitions come from libc++18, never from the separate EDG bootstrap.
#include <exception>

extern "C" {
void* __cxa_current_primary_exception() noexcept;
void __cxa_increment_exception_refcount(void*) noexcept;
void __cxa_decrement_exception_refcount(void*) noexcept;
void __cxa_rethrow_primary_exception(void*);
void* __edg_cxx_current_exception(void*) noexcept;
void __edg_cxx_increment_exception_refcount(void*,void*) noexcept;
void __edg_cxx_decrement_exception_refcount(void*,void*) noexcept;

std::terminate_handler __edg_cxx_set_terminate(std::terminate_handler) noexcept;
std::terminate_handler __edg_cxx_get_terminate() noexcept;
[[noreturn]] void __edg_cxx_terminate() noexcept;
[[noreturn]] void __edg_cxx_terminate_default() noexcept;
int __edg_cxx_uncaught_exceptions() noexcept;
}

// Adapted from LLVM 18.1.8 src/support/runtime/exception_pointer_cxxabi.ipp.
// Part of the LLVM Project, under the Apache License v2.0 with LLVM Exceptions.
// See LICENSE.TXT. SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
// Only reference hooks gain the holder address. Public headers/layout and
// retain-before-release ordering are unchanged.
namespace std {

exception_ptr::~exception_ptr() noexcept { __edg_cxx_decrement_exception_refcount(__ptr_,this); }

exception_ptr::exception_ptr(const exception_ptr& other) noexcept : __ptr_(other.__ptr_) {
  __edg_cxx_increment_exception_refcount(__ptr_,this);
}

exception_ptr& exception_ptr::operator=(const exception_ptr& other) noexcept {
  if (__ptr_ != other.__ptr_) {
    __edg_cxx_increment_exception_refcount(other.__ptr_,this);
    __edg_cxx_decrement_exception_refcount(__ptr_,this);
    __ptr_ = other.__ptr_;
  }
  return *this;
}

exception_ptr exception_ptr::__from_native_exception_pointer(void* __e) noexcept {
  exception_ptr ptr;
  ptr.__ptr_ = __e;
  __edg_cxx_increment_exception_refcount(ptr.__ptr_,&ptr);

  return ptr;
}

nested_exception::nested_exception() noexcept : __ptr_(current_exception()) {}

nested_exception::~nested_exception() noexcept {}

_LIBCPP_NORETURN void nested_exception::rethrow_nested() const {
  if (__ptr_ == nullptr)
    terminate();
  rethrow_exception(__ptr_);
}

exception_ptr current_exception() noexcept {
  // be nicer if there was a constructor that took a ptr, then
  // this whole function would be just:
  //    return exception_ptr(__cxa_current_primary_exception());
  exception_ptr ptr;
  ptr.__ptr_ = __edg_cxx_current_exception(&ptr);
  return ptr;
}

_LIBCPP_NORETURN void rethrow_exception(exception_ptr p) {
  __cxa_rethrow_primary_exception(p.__ptr_);
  // if p.__ptr_ is NULL, above returns so we terminate
  terminate();
}

} // namespace std


namespace {
void managed_default_terminate() {__edg_cxx_terminate_default();}
}
namespace std {
terminate_handler set_terminate(terminate_handler handler) noexcept {
  auto previous=__edg_cxx_set_terminate(handler);
  return previous?previous:managed_default_terminate;
}
terminate_handler get_terminate() noexcept {
  auto handler=__edg_cxx_get_terminate();
  return handler?handler:managed_default_terminate;
}
void terminate() noexcept {__edg_cxx_terminate();}
bool uncaught_exception() noexcept {return __edg_cxx_uncaught_exceptions()!=0;}
int uncaught_exceptions() noexcept {return __edg_cxx_uncaught_exceptions();}
}

#if _LIBCPP_STD_VER <= 14 || defined(_LIBCPP_ENABLE_CXX17_REMOVED_UNEXPECTED_FUNCTIONS) || defined(_LIBCPP_BUILDING_LIBRARY)
extern "C" {
std::unexpected_handler __edg_cxx_set_unexpected(std::unexpected_handler) noexcept;
std::unexpected_handler __edg_cxx_get_unexpected() noexcept;
[[noreturn]] void __edg_cxx_unexpected();
[[noreturn]] void __edg_cxx_unexpected_default();
// The unused typed parameter gives the backend the exact public class identity
// without guessing it from source names or manufacturing a lowered class.
[[noreturn]] void __edg_cxx_throw_bad_exception(std::bad_exception*) {throw std::bad_exception();}
}
namespace {
void managed_default_unexpected() {__edg_cxx_unexpected_default();}
}
namespace std {
unexpected_handler set_unexpected(unexpected_handler handler) noexcept {
  auto previous=__edg_cxx_set_unexpected(handler);
  return previous?previous:managed_default_unexpected;
}
unexpected_handler get_unexpected() noexcept {
  auto handler=__edg_cxx_get_unexpected();
  return handler?handler:managed_default_unexpected;
}
void unexpected() {__edg_cxx_unexpected();}
}
#endif
