//type:fp
//options_all:--clang_v 150100
//remark:Clang compatibility: Integer overflow in enumerator constant values
// 2/7/26   [EDGcpfe/28603]
//
// Clang compatibility: Integer overflow in enumerator constant values
//
// The constant expression for the enumerator constant exceeds the range of type
// int, which is ordinarily an error.  However, Clang does not diagnose this
// prior to version 19.  The front end now emulates that behavior (with a
// warning) in Clang C++ mode when clang_version < 190000.  This is a regression
// because version 6.6 of the front end also accidentally accepted such code.
enum E { e = 2147483647 + 100 };
