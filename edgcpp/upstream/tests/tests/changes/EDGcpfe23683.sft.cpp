//type:fp
//options_all:--clang_v 110000
//remark:[6.2] Clang compatibility: __builtin_assume
// 1/20/21  [EDGcpfe/23683]
//
// Clang compatibility: __builtin_assume
//
// Changes have been made to better emulate clang's __builtin_assume.
// Specifically, this builtin can now be used in a constexpr context and is
// now lowered like Microsoft's __assume builtin.
// --clang_version 110000:
  constexpr int f(int i) {
    __builtin_assume(i >= 0);
    return i + 1;
  }
  constexpr int j = f(2);
