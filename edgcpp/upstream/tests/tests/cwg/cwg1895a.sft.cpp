//type:fn
//options_all:--c++17 -tused -A
//

  struct S {
    int i:3;
    const int j:4;
  } s;
  int k = true ? s.i : s.j;
