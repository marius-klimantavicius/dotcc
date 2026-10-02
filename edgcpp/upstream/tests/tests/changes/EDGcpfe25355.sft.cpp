//type:fp
//options_all:--g++
//remark:[6.4] GNU C++ compatibility: Compound assignment with pointer to void or function
// 6/13/22  [EDGcpfe/25355]
//
// GNU C++ compatibility: Compound assignment with pointer to void or function
//
// The front end already permitted ordinary addition and subtraction operations
// on void* and pointer-to-function values in GNU C++ modes with gnu_version >=
// 40400 (see the entry for EDGcpfe/11204).  That now also applies to the
// corresponding compound assignment operators.
void g(void *v, int (*f)(), int x) {
  v += x;  // Previously an error.  Now accepted when gnu_version >= 40400.
  f += x;  // Ditto.
}
