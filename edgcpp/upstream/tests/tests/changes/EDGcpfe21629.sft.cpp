//type:fp
//options_all:--g++
//remark:[6.0] Assertion failure folding unary operation on vector operand
// 9/9/19   [EDGcpfe/21629]
//
// Assertion failure folding unary operation on vector operand
//
// When the type of a GNU-compatible vector constant is specified via a typedef,
// a unary operation applied to that constant resulted in an assertion failure
// (in unary_operation).  That is now fixed.
typedef int VECT __attribute__((vector_size(16)));
VECT x = -((VECT){0,0,0,0});  // Previously resulted in assertion failure
