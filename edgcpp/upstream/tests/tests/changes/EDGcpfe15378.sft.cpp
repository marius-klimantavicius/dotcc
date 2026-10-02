//type:fp
//options_all:--c++11
//remark:[4.10] Spurious "not constant" error with dependent constructor call
// 9/8/14   [EDGcpfe/15378]
//
// Spurious "not constant" error with dependent constructor call
//
// In a template definition, a dependent constructor call initializing a
// variable that later appears in a context requiring a constant expression
// previously resulted in an error.  This is now fixed.
// --c++11):
template<class T, T N> struct S {
  static constexpr T x = N;
};

template<typename T> int func() {
  const T v(16);  // Initialization by dependent constructor call
  S<T, v + 1> s;  // Use of "v" was previously incorrectly diagnosed as
                  // not a constant, now accepted
  return s.x;
}
