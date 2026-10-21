// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
// Native-only discriminating facts over the unchanged managed source inputs.
#include "m4-controlflow-positive.cpp"
#include "m4-controlflow-combined.cpp"
template<class A,class B> constexpr bool same_control_type=false;
template<class A> constexpr bool same_control_type<A,A> = true;
int native_destination=0;
static_assert(same_control_type<decltype(native_destination=1),int&>);
static_assert(same_control_type<decltype(native_destination+=1L),int&>);
static_assert(same_control_type<decltype((native_destination=1)=2),int&>);
static_assert(same_control_type<decltype(accumulate(3)),int>);
static_assert(same_control_type<decltype(accumulate(3L)),long>);
static_assert(same_control_type<decltype(select(native_destination)),void>);
