// Authored native-only discriminating witness, same definitions as positive.cpp.
#include "positive.cpp"
template<class A, class B> constexpr bool same_type = false;
template<class A> constexpr bool same_type<A, A> = true;
static_assert(same_type<Number, int>);
static_assert(same_type<decltype(add(1, 2)), int>);
static_assert(same_type<decltype(caller()), int>);
static_assert(same_type<decltype(1L + 1LL), long long>);
static_assert(same_type<decltype(1U + 1L), long>);
static_assert(same_type<decltype(1UL + 1LL), unsigned long long>);
