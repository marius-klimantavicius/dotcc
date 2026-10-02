//type:fp
//options_all:--c++23
//remark:[6.3] C++23: if consteval/if not consteval
// 10/28/21 [EDGcpfe/24773]
//
// C++23: if consteval/if not consteval
//
// In C++23 mode, the front end now accepts the "if consteval" and "if not
// consteval" constructs (the latter can also be written "if !consteval").
//
// This feature was added to the working paper for the next standard by the
// standardization committee's paper P1938R3.
consteval int f(int i) { return i; }
constexpr int g(int i) {
  if consteval {
    return f(i);  // Not an error even though i is not a constant
                  // expression because this is a consteval context.
  }
  return 0;
}
static_assert(g(42) == 42);
