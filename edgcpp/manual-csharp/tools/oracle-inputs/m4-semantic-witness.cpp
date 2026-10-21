// Native-only semantic inspector over the exact combined managed gate bytes.
#include "../../fixtures/m4-combined-gate.cpp"
template<class A, class B> constexpr bool same_type = false;
template<class A> constexpr bool same_type<A, A> = true;
static_assert(same_type<decltype(identity(static_cast<short>(1))), short>);
static_assert(same_type<decltype(identity(1)), int>);
static_assert(same_type<decltype(identity(1L)), long>);
template<class T, class U> int pattern(T, U) { return 1; }
template<class V> long pattern(V, V) { return 2; }
static_assert(same_type<decltype(pattern(1, 2)), long>);
static_assert(same_type<decltype(pattern(1, 2L)), int>);
int preferred(int);
template<class T> long preferred(T) { return 1; }
static_assert(same_type<decltype(preferred(1)), int>);
static_assert(same_type<decltype(preferred(1L)), long>);
static_assert(same_type<decltype(preferred(static_cast<short>(1))), long>);
