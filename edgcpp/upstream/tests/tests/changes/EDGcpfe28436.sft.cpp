//type:fp
//options_all:--c++17 --clang_v 190100
//remark:[6.8] GNU/clang compatibility: large integer user-defined literals
// 10/27/25 [EDGcpfe/28436]
//
// GNU/clang compatibility: large integer user-defined literals
//
// In clang and g++ modes, the front end previously issued spurious
// diagnostics (a warning in g++ mode, a discretionary error in clang mode)
// for very large integer user-defined literals that invoke a raw user-defined
// literal operator or a specialization of a user-defined literal template.
// (Such user-defined literals were accepted without a diagnostic in other
// emulation modes.)  This is now fixed.
// --clang_version=190100:
template <char... Cs> __int128 operator""_i128() {
  return 0;
}
__int128 i128_max() {
  return 170141183460469231731687303715884105727_i128;  // Previously an
                                                        // error, now okay
}
