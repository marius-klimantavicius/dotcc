//type:fp
//options_all:--c++17 --gn 80100
//remark:[6.2] Internal error in get_rescan_info with class template argument deduction
// 10/23/20 [EDGcpfe/22589,EDGcpfe/23473]
//
// Internal error in get_rescan_info with class template argument deduction
//
// In some situations with constructors involving parameter types that include
// expressions, the front end aborted with an internal error in get_rescan_info
// (exprutil.c, "missing default rescan info") while performing class template
// argument deduction.
//
// This is now fixed.
template<int> struct E {};
struct I { static constexpr int one = 1; };
template<typename T> struct S {
  static constexpr int N = T::one;
  S() {}
  S(E<N> x) {}
};
void g() {
  S<I> x;
  S y(x);  // Previously triggered an internal error.
}
