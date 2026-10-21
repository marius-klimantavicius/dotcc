// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
#include "m1-string-storage-gate.cpp"
template<class A,class B> constexpr bool same_string_type=false;
template<class A> constexpr bool same_string_type<A,A> = true;
static_assert(same_string_type<decltype(ΚΕΙΜΕΝΟ),const char16_t(&)[3]>);
static_assert(same_string_type<decltype(ΚΕΙΜΕΝΟ[0]),const char16_t&>);
char16_t native_mutable[2]=u"a";
static_assert(same_string_type<decltype(βοήθησε(native_mutable)),void>);
