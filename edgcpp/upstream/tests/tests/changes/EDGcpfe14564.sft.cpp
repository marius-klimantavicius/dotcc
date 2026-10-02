//type:fp
//options_all:--c++11
//remark:[4.9] Spurious error on "explicit" keyword used in conversion function template
// 10/22/13 [EDGcpfe/14564,EDGcpfe/14601]
//
// Spurious error on "explicit" keyword used in conversion function template
//
// The front end had issued a spurious error when the "explicit" keyword was
// applied to a conversion function template.  Now fixed.
// (with --c++11):
template<typename T> struct A { };
struct B {
  template<typename T> explicit operator A<T>();
};
A<char> a = static_cast<A<char>>(B());
