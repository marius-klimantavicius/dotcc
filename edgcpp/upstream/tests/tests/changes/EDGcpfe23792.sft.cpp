//type:fp
//options_all:--gnu_version=80999 --c++17 -tused -w
//remark:[6.2] Spurious GNU C++17 mode error on nonconstant static data member initializer
// 1/28/21  [EDGcpfe/23792,EDGcpfe/23823]
//
// Spurious GNU C++17 mode error on nonconstant static data member initializer
//
// Prior to C++17, in-class static data member initializers always had to be
// constant initializers.  With C++17, such members can be "inline" and those can
// have non-constant initializers.  In GNU C++17 mode, however, the front end
// handles such members slightly differently when they appear in class template
// instances to emulate GCC's on-demand instantiation of the initializer.
// Unfortunately, that handling previously still required a constant initializer
// and that triggered spurious errors in GNU C++17 mode.
//
// That problem is now fixed.
int f();
template<typename T> struct S {
  static inline int const x = f();
};
int r = S<int>{}.x;  // Previously an error in GNU C++17 mode.  Now okay.
