//type:fp
//options_all:--c --gcc
//remark:[4.4] Abort on GNU C-mode void return expression in presence of VLAs
// 9/14/11  [EDGcpfe/9815]
//
// Abort on GNU C-mode void return expression in presence of VLAs
//
// In GNU C mode, the front end accepts a return expression of type void in a
// function with void return type (this is a standard feature in C++ mode, but
// not in C mode).  Previously, in configurations with VLA_DEALLOCATIONS_IN_IL
// set to TRUE, the front end produced an invalid expression statement entry
// (one with a NULL expr field) in such a function if a variable-length array
// (VLA) also had to be deallocated as part of returning from the function.
// This invalid statement entry usually triggered an internal error later on
// (e.g., in lowering).
//
// This is now fixed.
void func(int n) {
  int a[n];        // VLA must be deallocated later.
  return (void)0;  // Void-expression return resulted in an invalid
}                  // statement in the IL.
