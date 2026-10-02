//type:fp
//options_all:--c++17 --gnu_version=100100
//remark:[6.2] GNU/Clang compatibility: Inline static data members of class templates
// 12/4/20  [EDGcpfe/23502]
//
// GNU/Clang compatibility: Inline static data members of class templates
//
// In GNU C++ mode, in-class initializers for constant static data members of
// class template instantiations are only instantiated when the associated value
// is needed or the associated definition is instantiated (see the entry for
// EDGcpfe/16403,EDGcpfe/16644).  Now that behavior has been extended to inline
// static data members of class template instantiations, in both GNU and Clang
// modes.
//
// Ordinarily, the example above produces an error because the initializer for
// S<X>::value is parsed while S<X> is instantiated, prior to parsing the "body"
// of X (i.e., before X::f() is declared).  Now, the example is accepted in GNU
// and Clang modes because the initializer "T::f()" is not instantiated until it
// is needed.
template<typename T> struct S {
  static inline int value = T::f();
};
struct X: public S<X> {         // Previously an error.  Now okay in GNU and
  static int f() { return 0; }  // Clang C++17 modes.  
};
