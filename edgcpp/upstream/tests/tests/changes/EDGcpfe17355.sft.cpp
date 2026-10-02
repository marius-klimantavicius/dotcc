//type:fp
//options_all:--c++14
//remark:[4.12] Abort on value-initialized pointer-to-member in C++14 constexpr evaluation
// 6/24/16  [EDGcpfe/17355]
//
// Abort on value-initialized pointer-to-member in C++14 constexpr evaluation
//
// In C++14 mode, the front end aborted in init_subobject_to_zero when evaluating
// the value-initialization of a pointer-to-member in a mem-initializer.
//
// This is now fixed.
class C {};
typedef void (C::*PMF)();
struct P {
  PMF v;
  constexpr P(): v() {}  // The evaluation of "v()" previously triggered
};                       // an internal error.
P g() {
  return P();
}
