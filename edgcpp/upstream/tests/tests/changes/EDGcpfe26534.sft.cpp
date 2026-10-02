//type:fp
//options_all:--c++20
//remark:Abbreviated friend template definitions in class templates
// 4/8/26   [EDGcpfe/26534,EDGcpfe/28787]
//
// Abbreviated friend template definitions in class templates
//
// Previously, during the instantiation of S<int>, the front end ignored all the
// members following the abbreviated friend template definition.  In this case,
// that means that it treated S<int> as having no member v, and thus issuing an
// error message about s having too many initializer values.  That is now fixed.
template<typename T> struct S {
  friend constexpr void f(auto n) noexcept {}
  T v;
};
void g() {
  S<int> s{ 0 };  // Previously an error about too many initializers.
}                 // Now okay.
