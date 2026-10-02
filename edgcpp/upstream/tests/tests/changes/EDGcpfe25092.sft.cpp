//type:fp
//options_all:--gn 90100
//remark:[6.7] GNU/Clang compatibility: explicit(bool) enabled in pre-C++20 modes
// 3/8/24   [EDGcpfe/25092,EDGcpfe/27070,EDGcpfe/27081]
//
// GNU/Clang compatibility: explicit(bool) enabled in pre-C++20 modes
//
// The explicit(bool) feature (see the Changes entry for EDGcpfe/20042) is now
// enabled (with a warning) when emulating GNU 9.0.0 or Clang 10.0.0 or later
// versions.
struct A { explicit(true) A() {} };  // Now accepted with a warning in
                                     // later GNU and Clang modes.
