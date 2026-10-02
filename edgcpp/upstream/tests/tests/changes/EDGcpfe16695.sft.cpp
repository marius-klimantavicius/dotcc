//type:fp
//options_all:--c++11
//remark:[4.11] Single-element multi-dimensional arrays and constexpr constructors
// 12/2/15  [EDGcpfe/16695]
//
// Single-element multi-dimensional arrays and constexpr constructors
//
// The lowered IL generated for a default-initialized multi-dimensional array
// having only one element and with a folded constexpr constructor sometimes
// resulted in incorrect generated C code by the C-generating back end.  That
// IL has been made more idiomatic.
//
// Previously, the C code generated for the initialization of y was incorrect.
// This is now fixed.
struct S {
  int s;
  constexpr S() : s(21) { }
};
int main() {
  int result;
  S x[1];
  result = x[0].s;
  S y[1][1];
  result += y[0][0].s;
  return result != 42;
}
