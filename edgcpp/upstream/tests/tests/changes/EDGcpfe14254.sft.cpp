//type:fp
//remark:[4.8] Capture by reference of a variable captured by value in an enclosing lambda
// 6/27/13  [EDGcpfe/14254]
//
// Capture by reference of a variable captured by value in an enclosing lambda
//
// When a nested lambda captures a variable by reference that is captured by
// value in a non-mutable enclosing lambda, the reference capture should behave
// as a reference-to-const.
//
// The front end, however, failed to propagate this principle to more deeply
// nested reference captures, which resulted in spurious errors about attempting
// to bind a reference to a non-const type to a const initializer value.
//
// This is now fixed.
void g1(int x) {
  auto lambda = [x] {
                  [&x] {  // Implies "reference to const int".
                  };
                };
}

void g2(int x) {
  auto lambda = [x] {
                  [&x] {
                    [&x] {};  // Triggered a spurious error because the
                  };          // capture was handled as a "reference to
                };            // (non-const) int".
}
