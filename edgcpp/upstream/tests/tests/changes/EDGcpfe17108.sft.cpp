//type:fp
//options_all:--c++11 --g++
//remark:[4.11] Spurious error in const static data member initializer in GNU C++ mode
// 5/5/16   [EDGcpfe/17108]
//
// Spurious error in const static data member initializer in GNU C++ mode
//
// In GNU C++11 modes, the front end previously issued a spurious error when a
// const static data member of non-dependent type is initialized with a dependent
// expression containing a potentially-constant call-like construct.
//
// This is now fixed.
template<typename> struct S {
  constexpr operator int() const { return 42; }
};
template<typename T> struct X {
  static const bool B = S<T>() == 42;  // Previously triggered a spurious
};                                     // error.
