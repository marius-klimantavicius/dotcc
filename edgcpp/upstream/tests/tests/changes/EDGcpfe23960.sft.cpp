//type:fp
//options_all:--gn 70300 --c++14
//remark:[6.4] Explicit template argument list in overloaded nontype template argument to an
// 3/21/22  [EDGcpfe/23960]
//
// Explicit template argument list in overloaded nontype template argument to an
// rvalue reference parameter pack
//
// The front end previously issued a spurious error when an explicit template
// argument list is used to select one member of an overload set as a nontype
// template argument for a template parameter pack that is an rvalue
// reference.  This is now fixed.
template<typename... _Ts> void f(_Ts&& ...);

int g(int);
template<int> int g(int);

void h() {
  f(g<0>);  // selects template g from overload set; previously an error
}
