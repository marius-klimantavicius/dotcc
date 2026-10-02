//type:fp
//options_all:--clang --ms_extensions
//remark:[6.3] Abort on array new-expressions in Clang mode with Microsoft extensions
// 9/17/21  [EDGcpfe/23503,EDGcpfe/24030,EDGcpfe/24710]
//
// Abort on array new-expressions in Clang mode with Microsoft extensions
//
// Microsoft mode handles array new-expressions specially (such a new-expression
// might use a non-array operator new in Microsoft mode).  The front end's support
// for Clang mode with Microsoft extensions treated that behavior inconsistently,
// which resulted in internal errors.
//
// That is now fixed.
void g() {
  new char[1];  // Previously aborted with --clang --ms_extensions.
}               // Now okay.
