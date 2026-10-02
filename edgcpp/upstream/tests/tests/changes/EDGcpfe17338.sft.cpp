//type:fp
//options_all:--c++17
//remark:[4.12] C++17: Relaxed range-based-for loop requirements
// 7/25/16  [EDGcpfe/17338]
//
// C++17: Relaxed range-based-for loop requirements
//
// In C++17 mode and in Microsoft mode with microsoft_version >= 1903, the front
// end now permits the "begin" and "end" iterators implied by the definition of
// the range-based-for statement not to have compatible types.
//
// This example prints 43, 44, and 45 on separate lines.  It works despite the
// "begin" and "end" iterators having different types (I and S, respectively)
// because user-defined conversion operators allow those two types to be compared.
// This relaxation was introduced into the working paper for C++17 by the
// standardization committee's paper P0184R0.
struct S { operator int const*() { return nullptr; } };
struct I {
  int i;
  I(): i(42) {}
  I const &begin() const { return *this; }
  S end() const { return S(); }
  int operator*() const { return i; }
  void operator++() const {}
  operator int const*() { if (i++ != 45) return &i; else return nullptr; }
};
extern "C" int printf(char const*, ...);
int main() {
  for (int x: I()) {
    printf("%d\n", x);
  }
}
