//type:fp
//options_all:--g++
//remark:[5.1] Underlying types of enumerations in GNU compatibility mode
// 6/3/19   [EDGcpfe/15936,EDGcpfe/18827,EDGcpfe/18884,EDGcpfe/19887,
//           EDGcpfe/21308]
//
// Underlying types of enumerations in GNU compatibility mode
//
// GCC treats the underlying type of an unscoped enumeration as unsigned int when
// this can contain all the values of the enumeration.  The front-end was
// correctly emulating this behavior for unscoped enumerations, except for when
// the enumeration was empty.  It was also incorrectly applying this behavior to
// scoped enumerations.
//
// This is now fixed.
template<typename,typename> struct same;
template<typename T> struct same<T, T>{};
enum A {};
enum B { b };
enum class C {};
enum class D { d };
void f() {
  same<__underlying_type(A), unsigned int>(); // Failure to match GCC
  same<__underlying_type(B), unsigned int>(); // Matched GCC
  same<__underlying_type(C), int>(); // Matched GCC
  same<__underlying_type(D), int>(); // Failure to match GCC
}
