//type:fp
//options_all:--clang
//remark:[6.0] Clang compatibility: ext_vector_type attribute
// 8/23/19  [EDGcpfe/16614,EDGcpfe/20883]
//
// Clang compatibility: ext_vector_type attribute
//
// Basic support for clang's ext_vector_type attribute has been added.
// Additionally, support has been added for an undocumented variant of clang's
// __builtin_shufflevector where there are two arguments and the second
// argument is a vector of integral types.  Note that at the present time,
// no additional support for these vector types (e.g., V.wxyz expressions) has
// been added.
typedef float float4 __attribute__((ext_vector_type(4)));
typedef unsigned int uint4 __attribute__((ext_vector_type(4)));
void f(float4* A, float4 x, uint4 mask) {
  *A = __builtin_shufflevector(x, mask);
}
