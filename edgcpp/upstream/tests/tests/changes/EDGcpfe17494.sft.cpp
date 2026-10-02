//type:fp
//options_all:--c++11
//remark:[4.12] Spurious error on capture of constant-valued variable used as an lvalue
// 9/2/16   [EDGcpfe/17494]
//
// Spurious error on capture of constant-valued variable used as an lvalue
//
// The front end previously issued a spurious error when attempting to capture a
// constant-valued variable used as an lvalue.
//
// This is now fixed.
int main() {
  int const OK = 0;
  return [=]{ return *&OK; }();  // Previously triggered a spurious error.
}                                // Now okay.
