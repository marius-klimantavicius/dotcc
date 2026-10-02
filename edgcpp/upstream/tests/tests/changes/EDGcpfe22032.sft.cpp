//type:fp
//options_all:--clang_version 60000
//remark:[6.4] Clang and GNU C++ mode compatibility: Designated field initializers
// 9/20/22  [EDGcpfe/22032,EDGcpfe/24268,EDGcpfe/25205,EDGcpfe/25617]
//
// Clang and GNU C++ mode compatibility: Designated field initializers
//
// The front end previously disallowed designated field initializers for non-POD
// class types in Clang and GNU C++ modes that do not enable C++20 (in C++20 such
// initializers can be valid).  Now, the C++20 rules apply in GNU C++ modes with
// gnu_version >= 80100 and in all Clang C++ modes.
struct S { S(char const*); };
struct V {
  int i;
  S s;
};
V v{ .s = ""};  // Previously an error in Clang and GNU modes that don't
                // enable C++20.  Now okay (except for earlier GNU modes).
