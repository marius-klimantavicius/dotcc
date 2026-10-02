//type:fp
//options_all:--clang_v 170000
//remark:Abort in prep_special_selector_operand
// 2/19/26  [EDGcpfe/28682]
//
// Abort in prep_special_selector_operand
//
// Previously, this triggered an internal error in prep_special_selector_operand
// (in overload.c) while the front end attempted to evaluate the enable_if operand
// (a dummy operand created to evaluate the __is_assignable intrinsic).  That is
// now fixed.
template<int> struct V {};
template<typename T> struct X: V<__is_assignable(T, T)> {};
struct S {
  operator int();
  S operator=(int p) __attribute__((enable_if(p, "")));
};
int g() { X<S> v; }
