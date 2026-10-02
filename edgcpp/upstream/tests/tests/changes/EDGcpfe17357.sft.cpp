//type:fp
//options_all:--c++14
//remark:[4.12] C++14 constexpr: Spurious failure on address equality comparison
// 6/28/16  [EDGcpfe/17357]
//
// C++14 constexpr: Spurious failure on address equality comparison
//
// Given a complete object X, comparing for equality a pointer to that complete
// object and a pointer "one past the end" of that object previously triggered a
// spurious constexpr evaluation failure.
//
// This is now fixed.
constexpr bool g() {
  int x = 1;
  return &x+1 == &x;  // Previously triggered a constexpr evaluation
}                     // failure.  Now evaluated to false.
static_assert(!g(), "Unexpected");  // Previously a compilation error.
                                    // Now okay.
