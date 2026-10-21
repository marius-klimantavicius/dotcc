// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
// Native-only fact inspector. Includes the exact managed fixtures unchanged.
// These assertions are proposed supervisor controls, not executed evidence.
#include "m3-reference-positive.cpp"
#include "m3-reference-ranking.cpp"
template<class A, class B> constexpr bool same_reference_type = false;
template<class A> constexpr bool same_reference_type<A, A> = true;

static_assert(same_reference_type<decltype(select(object)), int>);
static_assert(same_reference_type<decltype(select(fixed)), long>);
static_assert(same_reference_type<decltype(select(1)), bool>);
static_assert(same_reference_type<decltype(select(expiring())), bool>);
static_assert(same_reference_type<decltype(select(movable)), int>);
static_assert(same_reference_type<decltype(left(object)), int&>);
static_assert(same_reference_type<decltype(right(fixed)), const int&>);
static_assert(same_reference_type<decltype(expiring()), int&&>);
static_assert(same_reference_type<decltype((movable)), int&>);
static_assert(same_reference_type<decltype(movable), int&&>);
static_assert(same_reference_type<decltype(&left), int& (*)(int&)>);
using collapsed_lvalue = Movable&;
using collapsed_rvalue = Mutable&&;
using retained_rvalue = Movable&&;
static_assert(same_reference_type<collapsed_lvalue, Mutable>);
static_assert(same_reference_type<collapsed_rvalue, Mutable>);
static_assert(same_reference_type<retained_rvalue, Movable>);

short short_argument = 1;
long long_argument = 2;
static_assert(same_reference_type<decltype(reference_promote(short_argument)), int>);
static_assert(same_reference_type<decltype(reference_cv(1)), int>);
static_assert(same_reference_type<decltype(reference_kind(1)), int>);
static_assert(same_reference_type<decltype(reference_converted(long_argument)), int>);
static_assert(same_reference_type<decltype(reference_exact(long_argument)), long>);

// Native constexpr values are a future interpreter control only: managed
// reference execution remains explicitly Unsupported, with no folded value.
constexpr int reference_constant = 17;
constexpr const int& reference_constant_alias = reference_constant;
constexpr int native_read_reference(const int& value) { return value; }
static_assert(reference_constant_alias == 17);
static_assert(native_read_reference(19) == 19);
