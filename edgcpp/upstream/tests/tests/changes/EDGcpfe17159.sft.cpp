//type:fp
//options_all:--clang
//remark:[4.13] Clang compatibility: Vector type conversions
// 12/7/16  [EDGcpfe/17159,EDGcpfe/17662]
//
// Clang compatibility: Vector type conversions
//
// In Clang mode, the front end now supports implicit conversions between vectors
// of identical lengths if the implicit conversion for the underlying types is
// permitted.
typedef long V2L __attribute((vector_size(2*sizeof(long long))));
typedef double V2D __attribute((vector_size(2*sizeof(double))));
V2L vl;
V2D vd = vl;  // Now accepted in Clang mode.
