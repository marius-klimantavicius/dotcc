//type:fn
//options_all:--c++17 -tused -A
  void f() {
    for (int i : { 1, 2, 3 })
      int i = 1;          // error: redeclaration
  }
