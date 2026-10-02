//type:fp
//options_all:--c++17 -A --exceptions -tused
//remark:[6.1] Spurious error on by-reference captures in constant-expression contexts
// 2/3/20   [EDGcpfe/21030]
//
// Spurious error on by-reference captures in constant-expression contexts
//
// The front end previously emitted spurious errors when a lambda in a constant-
// expression context captured a variable outside that context by reference.
//
// That is now fixed.
bool f() {
  int i;
  constexpr bool r = [&]{ return &i; }() == &i;  // Previously an error.
  return r;                                      // Now okay.
}
