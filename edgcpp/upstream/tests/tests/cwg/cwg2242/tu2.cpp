  // tu2.cpp
  extern const int a;
  inline auto f() {
    static const int b = a;
    struct A { auto operator()() { return &b; } } a;
    return a;
  }
  int main() {
    return *decltype(f())()();
  }
