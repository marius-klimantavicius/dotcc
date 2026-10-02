//type:fp
//options_all:--microsoft_v 1938 --ms_c++20
//remark:[6.7] Template arguments for dependent destructor name
// 6/11/24  [EDGcpfe/27356]
//
// Template arguments for dependent destructor name
//
// Previously, the front end did not interpret a "<" in a dependent destructor
// name as the delimiter of a template argument list.
namespace ns {
  template<typename T>
  struct C {};
}
template<typename T>
void f(ns::C<T> c) {
  c.~C<T>();  // Previously a spurious error.  Now okay.
}
