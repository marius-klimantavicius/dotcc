//type:fp
//options_all:--clang_version=120000
//remark:[6.3] Clang C++ compatibility: Flexible array members in otherwise empty classes
// 3/9/21   [EDGcpfe/24028]
//
// Clang C++ compatibility: Flexible array members in otherwise empty classes
//
// The changes for EDGcpfe/22387 in version 6.2 of the front end introduced a
// regression: Those changes caused flexible array members in Clang C++ mode to
// be treated like standard C99 flexible array members.  However, C99 requires
// such a member not to be the sole named member of its parent type, but Clang
// does not enforce that constraint in C++ modes (it does in C modes).  The
// front end now emulates that behavior in Clang C++ modes, thereby fixing the
// regression.
struct S {
  int fam[];  // An error in Clang C++ mode with version 6.2 of the front
};            // end.  Now accepted.
