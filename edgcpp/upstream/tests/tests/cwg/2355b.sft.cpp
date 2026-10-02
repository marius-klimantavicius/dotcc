//type:fn
//options_all:--c++20 -tused
  template <bool E> void f1(void (*)() noexcept(E));
  template<bool> struct A { };
  template<bool B> void f2(void (*)(A<B>) noexcept(B));

  void g1();
  void g2() noexcept;
  void g3(A<true>);

  void h() {
    f1(g1);    // OK: E is false
    f1(g2);    // OK: E is true
    f2(g3);    // error: B deduced as both true and false
  }
