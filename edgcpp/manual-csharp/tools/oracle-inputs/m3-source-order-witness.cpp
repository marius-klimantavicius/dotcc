// Authored native-only witness for point-of-reference overload visibility.
// decltype is a native inspector here; managed M3 tests inspect call graph facts.
template<class A, class B> constexpr bool same_type = false;
template<class A> constexpr bool same_type<A, A> = true;
long staged(long);
using Before = decltype(staged(1));
int staged(int);
using After = decltype(staged(1));
static_assert(same_type<Before, long>);
static_assert(same_type<After, int>);
