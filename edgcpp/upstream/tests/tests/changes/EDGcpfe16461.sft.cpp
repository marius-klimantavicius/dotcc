//type:fp
//options_all:--c++11
//remark:[4.11] Constant folding of cast to void in left operand of comma operator
// 9/13/15  [EDGcpfe/16461]
//
// Constant folding of cast to void in left operand of comma operator
//
// The front end previously incorrectly rejected as non-constant a comma
// expression whose left operand is an explicit cast to void, even when the
// operands could otherwise have been folded to constants.  This is now fixed.
struct X {
  constexpr X(const char *s, int l): str(s), len(l) { }
  constexpr const char &f() const {
    return ((void)0), str[len-1];  // Previously incorrectly non-constant
  }
private:
  const char *str;
  int len;
};
void g() {
  constexpr X x("ABC", 2);
  static_assert(x.f() == 'B', "error");  // Previously a spurious error
}
