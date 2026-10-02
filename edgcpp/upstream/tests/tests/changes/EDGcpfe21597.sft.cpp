//type:fp
//options_all:--c++20
//remark:[6.0] C++20: Support for constexpr destructors and dynamic allocations
// 7/25/19  [EDGcpfe/21597]
//
// C++20: Support for constexpr destructors and dynamic allocations
//
// In C++20 mode, the front end now accepts constexpr destructors and can
// perform dynamic allocations and deallocations during the evaluation of
// constant expressions.  For such allocations to be valid, they must all be
// deallocated before the completion of the evaluation they are part of.
//
// This example is now accepted in C++20 mode, unless compiled with the macro NEG
// defined: In the latter case, the allocation is not deallocated by the time the
// call to forty_two() completes, and that call is therefore not considered a
// valid constant expression.
//
// Allocations can be performed using global non-placement new-expressions or
// using the standard allocator (std::allocator<X>, provided the appropriate
// members of that allocator are declared "constexpr").  The interpreter also
// implements evaluation of the placement-new expression that occurs in a normal
// std::construct_at implementation (other placement-new expressions are not
// permitted, though).
//
// These features were added to the draft working paper for C++20 through the
// standardization committee's paper P0784R7.
consteval int forty_two() {
  int *p = new int(42);
  int r = *p;
#ifndef NEG
  delete p;
#endif
  return r;
}
static_assert(forty_two() == 42);
