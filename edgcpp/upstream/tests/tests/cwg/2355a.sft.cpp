//options_all:--c++20 -tused -A
  template <bool E> void f1(void (*)() noexcept(E));

  void g1();
  void g2() noexcept;

  void h() {
    f1(g1);    // OK: E is false
    f1(g2);    // OK: E is true
  }
