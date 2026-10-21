// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
// Exact unchanged managed gate, plus native-only type identity witnesses.
#include "m1-unicode-storage-gate.cpp"
template<class A,class B> constexpr bool same_unicode_storage_type=false;
template<class A> constexpr bool same_unicode_storage_type<A,A> = true;
static_assert(same_unicode_storage_type<decltype(αντίγραφο(u'é')),char16_t>);
static_assert(same_unicode_storage_type<decltype(αντίγραφο(2L)),long>);
static_assert(same_unicode_storage_type<decltype(ηχώ(u'é')),const char16_t&>);
static_assert(same_unicode_storage_type<decltype(ηχώ(2L)),const long&>);
static_assert(same_unicode_storage_type<decltype(ηχώ(κόσμος)),void>);
