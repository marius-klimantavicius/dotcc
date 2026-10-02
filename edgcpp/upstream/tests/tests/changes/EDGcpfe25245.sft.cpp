//type:fp
//options_all:--clang_v 100000
//remark:[6.4] Clang compatibility: Nullability qualifiers on array parameters
// 5/6/22   [EDGcpfe/25245]
//
// Clang compatibility: Nullability qualifiers on array parameters
//
// The changes for EDGcpfe/16527,EDGcpfe/16916 added support for "nullability
// qualifiers" on pointer types in Clang modes.  However, Clang also permits these
// in array parameter declarators.
//
// That is now also emulated by the front end in Clang modes.
void f(int [_Nonnull]) {}  // Previously an error.  Now okay in Clang modes.
