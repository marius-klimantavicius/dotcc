//type:fp
//options_all:--gnu_version=100300 --c++20 -tused -w
//remark:[6.7] Evaluation of complex multiplication and division
// 1/3/25   [EDGcpfe/26231,EDGcpfe/27027,EDGcpfe/27828]
//
// Evaluation of complex multiplication and division
//
// The functions cx_multiply and cx_divide in float_pt.c yielded incorrect
// results when the result address aliases one of the operand addresses.  This
// situation arose with compound assignments.
//
// This is now fixed.
constexpr double g() {
  __complex double z{0, 7};
  z *= z;             // Results in a call to cx_multiply where the result
  return __imag__ z;  // address is also the address of the first operand.
}
static_assert(g() == 0);  // Previously failed.  Now okay.
