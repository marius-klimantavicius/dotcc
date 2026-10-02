//type:fp
//options_all:--c++17 --gnu=120100
//remark:[6.5] Conditional operation on vector defined with typedef element type
// 4/18/23  [EDGcpfe/26258]
//
// Conditional operation on vector defined with typedef element type
//
// The front end previously issued a spurious "incompatible types" error for a
// conditional operator in which the condition is a vector and the operands
// are vectors defined with a typedef as the element type.  This is now fixed.
typedef float xxx;
typedef xxx vf __attribute__ ((vector_size (64)));
vf f(vf a, vf b) {
  return (a < b) ? a : b;   // Previously a spurious error, now okay
}
