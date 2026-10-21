// Authored native-only type witness over the exact managed M3 positive fixture.
#include "../../fixtures/m3-positive.cpp"
template<class A, class B> constexpr bool same_type = false;
template<class A> constexpr bool same_type<A, A> = true;
static_assert(same_type<decltype(pick(1)), int>);
static_assert(same_type<decltype(pick(1L)), long>);
static_assert(same_type<decltype(pick(static_cast<short>(0))), int>);
static_assert(same_type<decltype(pick(true)), int>);
static_assert(same_type<decltype(pick(static_cast<unsigned short>(0))), int>);
