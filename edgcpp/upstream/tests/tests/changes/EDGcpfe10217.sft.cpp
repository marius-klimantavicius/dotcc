//type:fp
//remark:[4.2] Incorrect setting of result_is_not_used flag in some lowered expressions
// 11/23/09 [EDGcpfe/10217]
//
// Incorrect setting of result_is_not_used flag in some lowered expressions
//
// Top-level lvalue expressions are rewritten as rvalue expressions during
// lowering.  If, during this rewriting, the expression type is changed to
// void (because it contains an eok_question operation whose second and
// third operands have different types after being rewritten), it is possible that
// the result_is_not_used flag may be set incorrectly in some portion of the
// expression.  For users of the C generating back end, this had resulted in an
// assertion failure in check_result_not_used_flag.
typedef int T;
T x;
void f() {
  0, (0, (0 ? *new T : x));
}
