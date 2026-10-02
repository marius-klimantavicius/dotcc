//type:fp
//options_all:--clang
//remark:[6.2] Clang compatibility: attribute "overloadable" (IL CHANGE)
// 7/27/20  [EDGcpfe/22600]
//
// Clang compatibility: attribute "overloadable" (IL CHANGE)
//
// Version 5.0 of the front end added support for the Clang attribute
// "overloadable", which enables overloading function declarations in C mode (see
// the entry for EDGcpfe/14779, etc.).  However, the attribute was essentially
// ignored in C++ mode.  Now, it also allows overloading extern-C functions in
// Clang C++ mode.
//
// Previously, the type of a function declared using this attribute was marked as
// "C++ external" (see the routine_name_linkage field of the associated routine
// type supplement).  That is no longer the case, and instead the routine entry
// itself is marked as having "C++ name linkage" if appropriate (via the
// source_corresp.name_linkage field).  This is a subtle IL CHANGE.
extern "C" {
  int f(int) { return 1; }
  int f(unsigned) __attribute((overloadable)) { return 2; }
    // Previously an error in all C++ modes.  Now okay in Clang C++ mode.
}
