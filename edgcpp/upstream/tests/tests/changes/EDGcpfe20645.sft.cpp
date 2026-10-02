//type:fp
//options_all:--c++20
//remark:[5.1] C++20: "try"/"catch" in constexpr functions
// 1/2/19   [EDGcpfe/20645]
//
// C++20: "try"/"catch" in constexpr functions
//
// In C++20 mode, the front end now accepts "try"/"catch" constructs in constexpr
// functions (throwing an exception is still not permitted during the evaluation
// of a constant expression, however).
//
// This feature was added to the working paper for the next standard by the
// standardization committee's paper P1002R1.
constexpr int f() {
  try {                    // Now accepted in C++20 mode.
    return 42;
  } catch (...) {
    return 0;
  }
}
static_assert(f() == 42);  // Okay in C++20 mode.
