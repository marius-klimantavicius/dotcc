//type:fp
//options_all:--c++14
//remark:[6.1] Interpreter error for floating-point compound assignments
// 4/30/20  [EDGcpfe/22546]
//
// Interpreter error for floating-point compound assignments
//
// The constexpr interpreter previously did not correctly compute compound
// assignments involving two different floating-point types.
//
// This is now fixed.
constexpr float g(float x) {
   return x += 2.0;  // "float += double", previously produced incorrect
}                    // result.
static_assert(g(2.0) == 4.0, "");  // Previously failed.  Now okay.
