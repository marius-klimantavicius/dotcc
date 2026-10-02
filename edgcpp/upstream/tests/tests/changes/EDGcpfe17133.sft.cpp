//type:fp
//options_all:--c++11
//remark:[4.12] Abort on nonstandard anonymous union with default initializer in template
// 5/13/16  [EDGcpfe/17133]
//
// Abort on nonstandard anonymous union with default initializer in template
//
// In configurations that accept nonstandard anonymous unions in C++11 mode, the
// front end triggered an internal error during IL lowering (in lower_constant)
// after attempting to instantiate a class template containing a nonstandard
// anonymous union with a default member initializer.
//
// This is now fixed.
template<typename T> struct S {
  struct {
    float f = 0;
  };  // Nonstandard anonymous union.
};
S<int> si;
