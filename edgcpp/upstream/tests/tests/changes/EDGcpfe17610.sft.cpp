//type:fp
//remark:[4.13] Logical operators and constexpr conversion functions
// 10/17/16 [EDGcpfe/17610]
//
// Logical operators and constexpr conversion functions
//
// The front end previously issued a spurious error when a built-in logical
// operator included a short-circuited second operand convertible to bool via a
// constexpr conversion function that would not evaluate to a constant (but need
// not be evaluated at all since it is short-circuited).
//
// This is now fixed.
struct V {
  int v;
  constexpr operator bool() const { return v; }
};
V x;
constexpr bool r = 0 && x;  // Previously a spurious error.  Now okay.
