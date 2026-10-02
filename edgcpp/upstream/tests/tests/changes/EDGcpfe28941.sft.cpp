//type:fp
//options_all:--clang_version=220100 --c++03
//remark:GNU and Clang C++ compatibility: unrestricted unions in pre-C++11 modes
// 7/10/26  [EDGcpfe/28941]
//
// GNU and Clang C++ compatibility: unrestricted unions in pre-C++11 modes
//
// GCC (since version 4.6) and Clang (since version 3.1) accept "unrestricted
// unions" (i.e., unions with members whose types have nontrivial special member
// functions, a C++11 feature) in their pre-C++11 modes, with a warning.  The
// front end now matches that behavior in the corresponding GNU and Clang C++
// modes.
struct S { S(); };
union U { S s; };  // Previously an error; now accepted with a warning.
