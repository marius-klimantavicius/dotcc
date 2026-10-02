//type:fp
//options_all:--c++11 --clang_v 30400
//remark:[6.2] String literal operator templates in clang mode
// 11/19/20 [EDGcpfe/23608]
//
// String literal operator templates in clang mode
//
// The conditions under which string literal operator templates (see
// EDGcpfe/15168,EDGcpfe/20235) were enabled in clang mode were not correct.
// As noted in the original description, the feature was supported only in
// C++14 and newer modes and when gnu_version was at least 40900.  In clang
// mode, the restriction to C++14 mode was incorrect, and the dependency on
// gnu_version was unobvious.  The front end has now been changed to enable
// the feature in clang mode for C++11 and newer modes when clang_version is
// at least 30400, regardless of the value of gnu_version.
// --c++11 --clang_version=30400:
template<typename T, T ...chars> int operator"" _x();  // Now accepted
