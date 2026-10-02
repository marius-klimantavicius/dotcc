//type:fp
//options_all:--gcc
//remark:[4.10] Lowering of __builtin_complex
// 6/3/14   [EDGcpfe/15202]
//
// Lowering of __builtin_complex
//
// The changes for EDGcpfe/12780,EDGcpfe/14747 introduced __builtin_complex,
// but any attempt to use it in configurations that use lowering had resulted in
// an assertion failure in lower_builtin_operation.  That is now fixed.
_Complex double f(double a, double b) {
  return __builtin_complex(a, b);
}
