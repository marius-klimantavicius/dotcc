// Authored native controls for independent review findings R3/R5.
const int source();
int cv_call() { return source(); }
bool decay() { return source; }
bool negate_designator() { return !source; }
int alternate_bitwise() { return 1 bitand 2; }
int alternate_logical() { return 1 and 2; }
template<class A, class B> constexpr bool same_type = false;
template<class A> constexpr bool same_type<A, A> = true;
static_assert(same_type<decltype(source()), int>);
