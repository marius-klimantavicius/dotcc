//type:fp
//options_all:--c++14 --gn 79999
//remark:[6.3] Constant-evaluation of template arguments
// 2/26/21  [EDGcpfe/22267]
//
// Constant-evaluation of template arguments
//
// Several improvements have been made to the handling of template arguments and
// the constant-evaluation in particular.
//
// This example (which is not standard C++) was previously rejected in all modes,
// but now it is accepted in GNU C++ mode.
struct S {
  constexpr S();
  int x;
};
template <typename, int, int i> class C {
  static constexpr S s = S(i);
  template<typename = C<double, s.x, 42>> int f();
     // Previously an error about s.x not being constant.
     // Now accepted (in GNU mode).
};
