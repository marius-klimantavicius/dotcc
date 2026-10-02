//type:fp
//options_all:--clang --c++17
//remark:[6.3] Spurious failure to match a dependent nontype template parameter
// 2/24/21  [EDGcpfe/23975]
//
// Spurious failure to match a dependent nontype template parameter
//
// The changes for EDGcpfe/20943,EDGcpfe/23454 in version 6.2 introduced a
// regression that caused the front end to sometimes spuriously fail to match a
// template-dependent nontype template parameter that turns out to be const-
// qualified after substitution.
//
// In this example, the dependent template parameter type "T::X" ends up being
// "char const" after substitution.  The default argument 0 is a valid argument
// for that parameter, but the front end previously failed to establish that.
// That problem is now fixed.
struct S{
  using X = char const;
};
template <typename T, typename T::X = 0> void f();
void g() {
  f<S>();  // Previously an error.  Now okay.
}
