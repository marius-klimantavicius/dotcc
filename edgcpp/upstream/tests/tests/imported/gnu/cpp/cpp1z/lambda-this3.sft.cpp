//type: fp
//options: --c++17
// { dg-do compile { target c++17 } }

struct S {
  int i;
  constexpr S() : i(5) { 
    ([*this] () { return i + 10; }());
  }
  constexpr operator int() const { return i; }
};
constexpr int x = S();
