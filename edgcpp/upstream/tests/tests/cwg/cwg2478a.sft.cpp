//options_all:--c++20 -tused -A

  template<typename T> struct S {
    int f();
    constexpr int g();
  };
  template<> constexpr int S<int>::f() {  // OK, constexpr?
    return 0;
  }
  template<> int S<int>::g() {            // OK, not constexpr?
    return 0;
  }
