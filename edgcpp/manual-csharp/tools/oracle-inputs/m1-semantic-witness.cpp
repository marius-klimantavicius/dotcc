// Native-only semantic witness over the exact managed M1 fixture and header.
#include "../../fixtures/m1-integrated.cpp"
template<class A, class B> constexpr bool same_type = false;
template<class A> constexpr bool same_type<A, A> = true;
static_assert(same_type<decltype(run()), int>);
static_assert(same_type<decltype(id(3)), int>);
static_assert(same_type<decltype(id(3L)), long>);
static_assert(id(3) == 3);
static_assert(id(3L) == 3L);
static_assert(run() == 7);
