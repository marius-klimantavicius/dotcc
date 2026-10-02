//type:fp
//options_all:--clang_v 210000 --c++03
//remark:Clang compatibility: Enable C++ standard attributes in all modes
// 6/9/26   [EDGcpfe/28844]
//
// Clang compatibility: Enable C++ standard attributes in all modes
//
// When clang_version >= 170000, C++ standard attributes are now enabled in all
// C++ modes.
[[nodiscard]] int foo() { return 0; }
