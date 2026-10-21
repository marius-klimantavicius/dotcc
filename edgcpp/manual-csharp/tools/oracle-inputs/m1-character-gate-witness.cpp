// Native-only semantic witness for the exact included managed difficult path.
#include "../../fixtures/m1-character-gate.cpp"
template<class A, class B> constexpr bool same_type = false;
template<class A> constexpr bool same_type<A, A> = true;
static_assert(same_type<decltype(id(CHAR_VALUE)), char>);
static_assert(same_type<decltype(id(TARGET_CHAR)), char16_t>);
static_assert(run() == 66);
static_assert(id(CHAR_VALUE) == 65);
static_assert(id(TARGET_CHAR) == 65);
static_assert(id('B') == 66);
