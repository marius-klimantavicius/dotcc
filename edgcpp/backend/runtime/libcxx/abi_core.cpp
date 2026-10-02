//===----------------------------------------------------------------------===//
//
// Part of the LLVM Project, under the Apache License v2.0 with LLVM Exceptions.
// See LICENSE.TXT for license information.
// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
//
//===----------------------------------------------------------------------===//
// Class definitions selected from libc++ 18.1.8
// src/support/runtime/exception_fallback.ipp. The other fallback definitions
// (native termination and unimplemented uncaught counts) are deliberately not
// used: managed EH supplies those operations in a separate ABI provider.
// Compile with libc++ headers, never the EDG bootstrap std declarations.

#include <exception>
#include <new>
#include <typeinfo>

namespace std {
exception::~exception() noexcept {}
const char* exception::what() const noexcept { return "std::exception"; }
bad_exception::~bad_exception() noexcept {}
const char* bad_exception::what() const noexcept { return "std::bad_exception"; }
bad_alloc::bad_alloc() noexcept {}
bad_alloc::~bad_alloc() noexcept {}
const char* bad_alloc::what() const noexcept { return "std::bad_alloc"; }
bad_array_new_length::bad_array_new_length() noexcept {}
bad_array_new_length::~bad_array_new_length() noexcept {}
const char* bad_array_new_length::what() const noexcept { return "bad_array_new_length"; }
bad_cast::bad_cast() noexcept {}
bad_cast::~bad_cast() noexcept {}
const char* bad_cast::what() const noexcept { return "std::bad_cast"; }
bad_typeid::bad_typeid() noexcept {}
bad_typeid::~bad_typeid() noexcept {}
const char* bad_typeid::what() const noexcept { return "std::bad_typeid"; }
}

// EDG emits these ABI entry points for failed reference casts, null polymorphic
// typeid and invalid array lengths. Actual translated C++ throws preserve the
// same class identity and cleanup path as an explicit source throw.
extern "C" [[noreturn]] void __cxa_bad_cast() { throw std::bad_cast(); }
extern "C" [[noreturn]] void __cxa_bad_typeid() { throw std::bad_typeid(); }
extern "C" [[noreturn]] void __cxa_throw_bad_array_new_length() {
  throw std::bad_array_new_length();
}
