//type:fp
//options_all:--clang_v 180000 --c++20
//remark:[6.7] Errors or aborts on Clang enable_if attribute
// 7/3/24   [EDGcpfe/27390,EDGcpfe/27417]
//
// Errors or aborts on Clang enable_if attribute
//
// Some spurious errors and potential aborts during the evaluation of a Clang
// enable_if attribute have been fixed.
__attribute__((enable_if(true, ""))) unsigned f(int);
template<int> struct S {
  static int s;
  static constexpr int N = f(s);  // Previously triggered a spurious error.
};                                // Now okay.
