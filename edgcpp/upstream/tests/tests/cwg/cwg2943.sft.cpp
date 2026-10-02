//options_all:--c++20
  [[nodiscard]] void f();
  template<class T> [[nodiscard]] T g();

  void h() {
    f();                // suggested change: warning no longer recommended
    (void)f();          // warning not recommended
    g<int>();           // warning recommended
    g<void>();          // suggested change: warning no longer recommended
    (void)g<void>();    // warning not recommended
  }

//cwg: 2943
//title: Discarding a void return value
//meeting: Hagenberg 2/25
//edg_status: Passes
