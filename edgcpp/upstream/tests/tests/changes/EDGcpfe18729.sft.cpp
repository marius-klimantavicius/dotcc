//type:fp
//options_all:--c++14 --g++
//remark:[5.0] Interpreter and ++/-- on run-time addresses
// 10/10/17 [EDGcpfe/18729]
//
// Interpreter and ++/-- on run-time addresses
//
// The interpreter previously did not correctly handle increment (++) and
// decrement (--) operations applied to the addresses of run-time entities.
// Both the pre-increment/decrement and the post-increment/decrement varieties
// were affected.
//
// This is now fixed.
constexpr int* pred(int *p) {
  p--;
  return p;
}
int x[3];  // Run-time array.
static_assert(pred(x+2) == &x[1], "Unexpected");
    // Previously a spurious error.  Now okay.
