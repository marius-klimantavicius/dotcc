//type:fp
//options_all:--g++ --c++14
//remark:[4.12] GNU C++-mode abort on C++14 constexpr use of member constant
// 6/14/16  [EDGcpfe/17313]
//
// GNU C++-mode abort on C++14 constexpr use of member constant
//
// In GNU C++ mode, the in-class initializer of a static data member of a class
// template is not instantiated unless it is needed (see the entry for
// EDGcpfe/16403,EDGcpfe/16644).  Previously, if the first need of such a static
// data member was during the evaluation of a C++14 constexpr function call, the
// front end aborted with an assertion failure in extract_value_from_constant.
//
// This is now fixed.
template<typename T> constexpr int f(const T &p) { return p; }
template <class T> struct S {
  static constexpr int N = 0;
};
void g() {
  f(S<int>::N);  // Previously aborted in GNU C++14 mode.  Now okay.
}
