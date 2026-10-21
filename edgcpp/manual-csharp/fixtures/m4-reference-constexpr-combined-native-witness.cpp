// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
// Native fact witness over the unchanged managed reference/storage gate.
#include "m4-reference-constexpr-combined.cpp"
template<class A,class B> constexpr bool same_reference_constant_type=false;
template<class A> constexpr bool same_reference_constant_type<A,A> = true;
static_assert(same_reference_constant_type<decltype(copy(u'A')),char16_t>);
static_assert(same_reference_constant_type<decltype(copy(2L)),long>);
static_assert(same_reference_constant_type<decltype(echo(u'A')),const char16_t&>);
static_assert(same_reference_constant_type<decltype(echo(2L)),const long&>);
char16_t native_mutable_argument=u'A';
static_assert(same_reference_constant_type<decltype(echo(native_mutable_argument)),void>);
