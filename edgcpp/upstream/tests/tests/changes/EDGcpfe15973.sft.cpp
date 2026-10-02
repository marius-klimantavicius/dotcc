//type:fp
//options_all:--c++11
//remark:[4.10.1] Spurious error on in-class initializer for static array member in template
// 2/4/15   [EDGcpfe/15973]
//
// Spurious error on in-class initializer for static array member in template
//
// In C++11 mode, a static array data member can have an in-class initializer,
// but the front end issued a spurious error in such cases if the underlying
// array element type is a template parameter.
//
// This is now fixed.
template<typename T> struct S {
  static constexpr T x[2] = { 1, 2 };  // Previously a spurious error in
};                                     // C++11 mode -- now okay.
