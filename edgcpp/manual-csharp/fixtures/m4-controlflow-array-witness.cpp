// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
#include "m4-controlflow-array-gate.cpp"
template<class A,class B> constexpr bool same_array_control_type=false;
template<class A> constexpr bool same_array_control_type<A,A> = true;
static_assert(same_array_control_type<decltype(DATA),const char16_t(&)[4]>);
char16_t mutable_element=0;
static_assert(same_array_control_type<decltype(helper(mutable_element)),void>);
