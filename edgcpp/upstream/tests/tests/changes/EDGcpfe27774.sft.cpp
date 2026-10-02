//type:fp
//options_all:--ms_extensions --clang_v 170000 --ms_extensions
//remark:[6.7] Clang compatibility: changing the encoding of a function name operator string
// 12/3/24  [EDGcpfe/27774]
//
// Clang compatibility: changing the encoding of a function name operator string
//
// With the -fms-extensions command-line option, the clang compiler emulates
// MSVC in allowing character string encoding prefixes like 'L' to be applied
// to the function name operators to change the encoding of the function name
// string.  However, the front end only enabled this functionality in
// Microsoft mode, not in clang mode with --ms_extensions.  This is now fixed.
//
// (As described in the entry for EDGcpfe/24385, some features enabled by
// --ms_extensions are implemented in the front end using keywords, even
// though gcc and clang do not reserve those identifiers.  This change adds
// __LPREFIX, __UPREFIX, __lPREFIX, and __uPREFIX to the list of such keywords
// in clang mode.)
#define M(X) L ## X
#define N(X) M(X)
void f() {
  N(__FUNCTION__);   // Previously resulted in an error that __LPREFIX is
                     // undefined; now equivalent to L"f"
}
