//type:fp
//options_all:--c++20
//remark:[6.6] Incorrectly generated copy/move constructor for lambda with init capture pack
// 8/21/23  [EDGcpfe/26601]
//
// Incorrectly generated copy/move constructor for lambda with init capture pack
//
// Here, the lambda inside "f" contains an init capture pack that is expanded in
// its body.  However, when the definition of the move constructor for the
// lambda's closure type was generated, the first member of the pack wasn't
// copied, leading to uninitialized reads later on, either at run time or constant
// evaluation time.  That is now fixed.
struct C {
  constexpr C() = default;
  constexpr C(const C &o) : i(o.i) { }
  int i{1};
};
struct X {
  C c{};
};
constexpr auto f(auto ... x) {
  auto l = [... cx = x]() { return (cx.c.i + ...); };
  return l;
};
static_assert(f(X(), X())() == 2);  // Previously an error.  Now okay.
