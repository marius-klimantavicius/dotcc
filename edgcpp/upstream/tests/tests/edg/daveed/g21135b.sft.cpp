//remark:noexcept and calls through folded ptr-to-member expression
//options:--c++17;fp:--gnu_version=80100 --c++17;fp

  struct S {
    void f() noexcept {}
    void g() {}
  } s;
  static_assert(!noexcept((s.*(true ? &S::f : &S::g))()));


