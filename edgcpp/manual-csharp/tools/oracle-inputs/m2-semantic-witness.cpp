// Authored native-only witness over the exact managed acceptance fixture bytes.
#include "../../fixtures/m2-positive.cpp"
template<class A, class B> constexpr bool same_type = false;
template<class A> constexpr bool same_type<A, A> = true;
static_assert(same_type<Count, long>);
static_assert(same_type<decltype(widen(0)), long>);
static_assert(same_type<decltype(entry()), long>);
static_assert(same_type<decltype(&widen), long (*)(int)>);
