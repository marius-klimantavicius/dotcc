//options_all:--c++23 -A
  template<bool B> struct X {
    static void f(short) requires B;   // #1
    static void f(short);              // #2
  };
  void test() {
    auto x = &X<true>::f;       // OK, deduces void(*)(short), selects #1
    auto y = &X<false>::f;       // OK, deduces void(*)(short), selects #2
  }
