//type:fp
//options_all:--c++17 -A --exceptions -tused
//remark:[6.1] Spurious error on use of local reference in constant-expression context
// 2/3/20   [EDGcpfe/21029]
//
// Spurious error on use of local reference in constant-expression context
//
// In strict mode, the front end previously sometimes failed to evaluate valid
// constant-expressions that refer to local references.
//
// That problem is now fixed.
int arr[2] = { 1,2 };
void g() {
  auto &[ x, y ] = arr;
  static_assert (&x == &arr[0]);  // Previously an error.  Now okay.
}
