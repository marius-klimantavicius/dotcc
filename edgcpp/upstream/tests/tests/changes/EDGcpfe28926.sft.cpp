//type:fp
//options_all:--clang_v 220100 --c++03
//remark:Lambda expressions in Clang C++03 mode
// 7/10/26  [EDGcpfe/28926]
//
// Lambda expressions in Clang C++03 mode
//
// Lambda expressions are now accepted with a warning in Clang C++03 mode
// when clang_version >= 190000.
void f() { []() {}; }
