//type:fp
//options_all:--c++20
//remark:[6.4] Abort in num_template_levels_of
// 1/10/22  [EDGcpfe/24923,EDGcpfe/24968]
//
// Abort in num_template_levels_of
//
// The changes for EDGcpfe/24623 in version 6.3 introduced a regression causing
// the front end to trigger an abort in num_template_levels_of on certain
// abbreviated out-of-class member template definitions.
//
// That is now fixed.
struct S {
  template<int> void f(auto x);
};
template<int> void S::f(auto x) {}  // Previously aborted in C++20 mode.
                                    // Now okay.
