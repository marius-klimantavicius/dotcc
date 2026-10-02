//type:fp
//options_all:--microsoft
//remark:[6.7] Unbounded loop in Microsoft mode
// 4/6/24   [EDGcpfe/27132]
//
// Unbounded loop in Microsoft mode
//
// In some cases involving a cast applied to a comma expression, the front end
// created an expression node using itself as an operand, which in turn was likely
// to trigger an unbounded loop during IL traversal.
//
// This is now fixed.
char* g(unsigned long &x, char *str) {
  return str ? str : (char*)(x = 0, 0);  // Previously triggered an unbounded
}                                        // loop in Microsoft mode.
