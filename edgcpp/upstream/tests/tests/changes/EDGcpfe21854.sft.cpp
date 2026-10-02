//type:fp
//options_all:--c++11 --clang
//remark:[6.0] GNU/Clang C++ compatibility: __final in class templates
// 10/1/19  [EDGcpfe/21854]
//
// GNU/Clang C++ compatibility: __final in class templates
//
// The changes for EDGcpfe/14942 (see entry of 10/13/16) meant to enable treating
// "__final" as a synonym for "final" in GNU C++ modes.  However, that change did
// not affect some template cases.
//
// That is now fixed.  Furthermore, this behavior now also applies to Clang C++
// modes.
template<typename> struct X __final {};  // Previously always an error.
                                         // Now accepted in GNU C++11 mode.
