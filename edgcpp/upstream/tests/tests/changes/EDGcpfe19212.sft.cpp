//type:fp
//options_all:--microsoft
//remark:[5.0] Microsoft/GNU compatibility: Cast of zero address in constant-expression
// 1/30/18  [EDGcpfe/19212]
//
// Microsoft/GNU compatibility: Cast of zero address in constant-expression
//
// GCC and Microsoft Visual C++ appear to accept in a constant-expression a cast
// of a constant zero address (not just a null pointer constant) to an integer
// type.
constexpr int f(void *p) {
  return (long)p;
}
constexpr int x = f(3-3);  // Now accepted in GNU and Microsoft C++11 modes.
