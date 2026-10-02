//type:fp
//options_all:--clang_v 220100 --c++03
//remark:Enum-type name qualifiers in Clang C++03 mode
// 7/10/26  [EDGcpfe/28925]
//
// Enum-type name qualifiers in Clang C++03 mode
//
// The front end now accepts enum-type name qualifiers with a warning in Clang
// C++03 mode when clang_version >= 30700.
enum E { e, f };
auto r = E::e;  // Normally requires C++11, but
                // accepted in Clang C++03 mode
