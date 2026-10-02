//type:fn
//options_all:--c++17 -tused -A
  void f() {
    for (int i = 0; i < 10; ++i)
      int i = 0;          // error: redeclaration
  }

//cwg: 2220
//title: Hiding index variable in range-based for
//meeting: Kona 2/17
//edg_status: Passes
