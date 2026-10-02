//remark:Deleted virtual overriders and noexcept
//options:--c++14;fn:--c++17;fp

  struct B { virtual void h() noexcept = delete; };
  struct D : B {
    void h() = delete;  // Now accepted in C++17 mode.
  };
