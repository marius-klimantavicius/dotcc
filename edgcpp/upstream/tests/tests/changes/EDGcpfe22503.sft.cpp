//type:fp
//options_all:--g++
//remark:[6.1] GCC compatibility: Vector shift-assign operations
// 7/15/20  [EDGcpfe/22503,EDGcpfe/23082]
//
// GCC compatibility: Vector shift-assign operations
//
// In GNU modes with gnu_version >= 40800 and Clang modes with clang_version >=
// 40000, the front end now accepts shift-assign (>>= and <<=) operations on
// vector types.
typedef int __attribute((vector_size(16))) V4;
void g(V4 *p) {
  *p <<= 2;  // Now accepted in some GNU and Clang modes.
}
