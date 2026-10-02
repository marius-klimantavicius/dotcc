//type:fp
//options_all:--c11 --gn 70300
//remark:[6.1] GNU/Clang compatibility: Vector assignment
// 7/21/20  [EDGcpfe/23123]
//
// GNU/Clang compatibility: Vector assignment
//
// In GNU and Clang modes, the front end now accepts more assignments involving
// "vector" types.
typedef long long VLL1 __attribute((vector_size(8)));
typedef int VI2 __attribute((vector_size(8)));
void g(VLL1 x, VI2 y) {
  x = y;  // Previously an error.  Now okay.
}
