//type:fp
//options_all:--clang
//remark:[4.13] Clang compatibility: builtin signatures for concrete __sync_* builtins
// 10/20/16 [EDGcpfe/17649]
//
// Clang compatibility: builtin signatures for concrete __sync_* builtins
//
// A change has been made to the type of the first argument of signatures for the
// concrete __sync_* builtin routines.  Previously those routines took a pointer
// to an appropriately-sized volatile integer type and now these routines take
// "volatile void *".  Clang apparently allows passing pointers to signed/unsigned
// data types for these arguments, necessitating the change.
// --clang):
char f(signed char *p, char c) {
  return __sync_fetch_and_add_1(p, c);
}
