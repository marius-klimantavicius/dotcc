//options_all:--c++23 -tused -A
  template <typename T = int>
  struct S {
    constexpr void f();                      // #1
    constexpr void f(this S&) requires true; // #2
  };

  void test() {
    S<> s;
    s.f();                 // calls #2
  }

//cwg: 2789
//title: Overload resolution with implicit and explicit object member function
//meeting: Kona 11/23
//edg_status: EDGcpfe/26791
