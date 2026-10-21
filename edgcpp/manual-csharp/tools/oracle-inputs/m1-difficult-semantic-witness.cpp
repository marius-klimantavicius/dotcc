// Native-only witness over the exact changed-stream managed difficult gate.
#include "../../fixtures/m1-difficult.cpp"
template<class A, class B> constexpr bool same_type = false;
template<class A> constexpr bool same_type<A, A> = true;
static_assert(same_type<decltype(combine(4)), long>);
static_assert(same_type<decltype(combine(5L)), long>);
static_assert(same_type<decltype(id(4)), int>);
static_assert(same_type<decltype(id(5L)), long>);
static_assert(run() == 7);
static_assert(combine(5L) == 8);
static_assert(ordinary() == 1);
