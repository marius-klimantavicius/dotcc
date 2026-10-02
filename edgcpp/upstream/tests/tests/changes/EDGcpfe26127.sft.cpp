//type:fp
//options_all:--c++17
//remark:Exception specification of a defaulted default constructor with a folded
// 7/10/26  [EDGcpfe/26127,EDGcpfe/28803,EDGcpfe/28868,EDGcpfe/28930]
//
// Exception specification of a defaulted default constructor with a folded
// default member initializer
//
// Since C++17 (P0003R5), a call to a function that does not have a non-throwing
// exception specification is potentially throwing even when the call is a
// constant expression.  When a default member initializer was a constant
// expression, the front end folded it to a constant and then failed to account
// for the (notional) constructor call that it performs, so the generated default
// constructor could be incorrectly treated as non-throwing.
//
// A related case, in which the default member initializer belongs to a member of
// an anonymous union, was also corrected:
struct S { int x; constexpr S() : x(0) {} };  // constexpr, not noexcept
struct A { S m = S(); };
static_assert(!noexcept(A()));  // now holds (previously failed)
