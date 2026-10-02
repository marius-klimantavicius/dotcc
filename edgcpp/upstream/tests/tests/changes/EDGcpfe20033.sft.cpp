//type:fp
//options_all:--c++20
//remark:[6.2] C++20: Nontype template parameters of class type or floating-point type
// 7/29/20  [EDGcpfe/20033,EDGcpfe/20276,EDGcpfe/22245]
//
// C++20: Nontype template parameters of class type or floating-point type
//
// In C++20 mode, the front end now accepts nontype template parameters (and
// corresponding arguments) of floating-point types and of certain class types
// (such class types must be "structural types").
//
// This implements the changes to the standard made by the standardization
// committee's paper P1907R1, which itself modified an earlier proposal adopted
// via paper P0732R2.
struct S {
  constexpr int f() const { return i; }
  int i;
};
template<S s> struct X {
  static constexpr int f() { return s.f(); }
};
X<S{42}> xs;  // Now accepted in C++20 modes.
constexpr int r = xs.f();  // Initializes r to 42.
