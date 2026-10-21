// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
// Native-only facts over the exact managed character/reference fixture.
#include "m3-reference-character.cpp"
template<class A, class B> constexpr bool same_character_reference_type=false;
template<class A> constexpr bool same_character_reference_type<A,A> = true;
static_assert(same_character_reference_type<decltype(promotion(U'A')),int>);
static_assert(same_character_reference_type<decltype(exact(U'A')),int>);
static_assert(same_character_reference_type<decltype(left(unit)),char16_t&>);
static_assert(same_character_reference_type<decltype(relayed()),char16_t&>);
static_assert(same_character_reference_type<decltype((bound)),char16_t&>);
static_assert(same_character_reference_type<decltype(expiring()),char16_t&&>);
static_assert(same_character_reference_type<decltype(materialized),const char8_t&>);
static_assert(same_character_reference_type<decltype(choose(wide)),long>);
static_assert(same_character_reference_type<decltype(choose(unit)),int>);
