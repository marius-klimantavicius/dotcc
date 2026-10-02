//type:fp
//options_all:--c++11
//remark:[4.6] In C++11 mode, the front end now accepts an in-class direct braced initializer
// 10/23/12 [EDGcpfe/13271]
//
// In C++11 mode, the front end now accepts an in-class direct braced initializer
// for a const static data member of integral type.
struct S {
  static int const N{33};  // Now accepted in C++11 mode.
};
