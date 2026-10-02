//type:fp
//options_all:--c++11 --g++
//remark:[6.8] GCC compatibility: Treatment of alias templates as opaque
// 4/14/25  [EDGcpfe/27060,EDGcpfe/28032,EDGcpfe/28071]
//
// GCC compatibility: Treatment of alias templates as opaque
//
// The changes for EDGcpfe/26857 (in version 6.7) primarily treated some alias
// templates in base class specifiers as opaque.  Now, this special handling is no
// longer specific to base class specifiers, and to more accurately emulate GCC,
// only alias templates referring to non-dependent class types are treated as
// opaque.
struct B;
template<typename T> using A = B;
template<typename T>
struct D : A<T> {  // Previously accepted in GNU C++ modes.
  using A<T>::A;   // Now also accepted in GNU C++ modes.
};
struct B { };
D<void> d;
