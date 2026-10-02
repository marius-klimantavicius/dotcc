//options_all:--c++23 -A

  void f() {}

  void(&g())(int) {
    return reinterpret_cast<void(&)(int)>(f);
  }

//cwg: 2780
//title: reinterpret_cast to reference to function types
//meeting: Kona 11/23
//edg_status: Passes
