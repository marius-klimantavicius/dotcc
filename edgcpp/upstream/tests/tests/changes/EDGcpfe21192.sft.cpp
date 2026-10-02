//type:fp
//options_all:--c++17 -A --exceptions -tused
//remark:[6.1] Unused inline variables
// 1/10/20  [EDGcpfe/21192]
//
// Unused inline variables
//
// The front end previously issued a spurious error for an inline variable that
// is declared but not defined, and referenced but not odr-used.
//
// That is now fixed.
extern inline int i;  // Not a definition.
auto r = sizeof(i);   // i is referenced, but not "used".  Previously, this
                      // triggered an error.  Now okay.
