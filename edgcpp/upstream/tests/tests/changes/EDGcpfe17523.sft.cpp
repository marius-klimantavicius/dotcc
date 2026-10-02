//type:fp
//options_all:--c++14 -tused
//remark:[4.13] Constexpr interpreter and addresses of local variables
// 12/2/16  [EDGcpfe/17523]
//
// Constexpr interpreter and addresses of local variables
//
// In some cases where a constexpr function returns a value that includes the
// address of a local variable, a constant representing the result of evaluating
// that function could violate memory region constraints (a file-scope constant
// entry pointed to a function-scope IL entry).  This was likely to result in an
// internal error during IL traversal.
//
// This is now fixed.
struct S { int const &r; };
constexpr S f(int const &p) {
  return { p };
}
int main() {
  const int x = 0;
  f(x);  // Previously produced IL violating memory region constraints.
}
