//type:fp
//options_all:--clang --gnu=40800 --no_c++11 --c++11
//remark:[4.9] clang compatibility: __has_feature(cxx_decltype) in non-C++11 mode
// 4/13/14  [EDGcpfe/14871]
//
// clang compatibility: __has_feature(cxx_decltype) in non-C++11 mode
//
// The front end previously incorrectly gave the value 1 for the clang
// feature-test macro __has_feature(cxx_decltype) in modes in which the
// __decltype construct is supported (see the entry for EDGcpfe/11338) but the
// "decltype" keyword is not recognized (i.e., when gnu_version is at least
// 40300 but C++11 mode is not enabled).  This is now fixed.
// with --clang --gnu_version=40800 --no_c++11:
int i;
#if __has_feature(cxx_decltype)
typedef decltype(i) I;  // Previously selected, causing an error
#else
typedef int I;
#endif
