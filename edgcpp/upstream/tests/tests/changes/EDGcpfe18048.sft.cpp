//type:fp
//options_all:--gn 50200 --c++11
//remark:[4.14] noexcept and constant-expressions
// 3/8/17   [EDGcpfe/18048]
//
// noexcept and constant-expressions
//
// The C++11 standard specifies that "noexcept(<expr>)" is true if <expr> is a
// "core constant expression".  Previously, the front end only applied this rule
// to a subset of core constant expressions that are also "constant expressions".
// In particular, this excluded calls to constexpr functions that attempt to
// return the address of a local variable.
//
// Here "f(&i)" is a "core constant expression" but not a "constant expression".
// Previously, the noexcept operator produced "false" for this case (leading to
// an error).  Now, it produces "true" and the case is accepted.
constexpr int const* f(int const *p) { return p; }
int main() {
  constexpr int i = 42;
  static_assert(noexcept(f(&i)), "Unexpected!");  // Previously an error.
}                                                 // Now accepted.
