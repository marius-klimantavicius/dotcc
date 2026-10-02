//type:fp
//options_all:--c++11
//remark:[4.10.1] Trailing return types in handler parameter declarations
// 1/7/15   [EDGcpfe/15593]
//
// Trailing return types in handler parameter declarations
//
// Previously, the front end failed to accept C++11 trailing return types in
// handler parameters.
//
// This is now fixed.
int main() {
  try {
  } catch (auto (*pf)()->int) {  // Previously a spurious error in C++11.
  }                              // Now okay.
}
