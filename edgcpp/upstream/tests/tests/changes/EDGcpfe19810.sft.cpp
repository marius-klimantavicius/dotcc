//type:fn
//options_all:--c++11 --g++ --no_defer_parse_function_templates
//remark:[6.6] GNU/Clang compatibility: spurious "call through incomplete class" warning
// 6/23/23  [EDGcpfe/19810,EDGcpfe/20667,EDGcpfe/25807,EDGcpfe/26254]
//
// GNU/Clang compatibility: spurious "call through incomplete class" warning
//
// The warning added for EDGcpfe/15754 (see entry of 12/4/14) to improve
// compatibility with GCC/Clang did not accurately match the behavior of GCC and
// Clang, as the example provided was not actually accepted by GCC or Clang.
// Instead, GCC and Clang accept some cases because they consider some expressions
// involving "this" to be template-dependent (see EDGcpfe/16428).  The warning
// ec_call_through_incomplete_class_type has therefore been removed.
// with --c++11 --g++ --no_defer_parse_function_templates:
struct C {
  int operator () () const;
  template<int I>
  static auto f(const C &c) -> decltype(c());  // Previously a spurious
                                               // warning.  Now okay.
};
int i = C::f<0>(C());
struct D;
template<typename> void f(D const &d) {
  d();  // Previously a warning.  Now an error.
}
