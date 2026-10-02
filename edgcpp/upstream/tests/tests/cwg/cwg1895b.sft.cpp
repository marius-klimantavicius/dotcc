//type:fn
//options_all:--c++17 -A -tused
//

  struct S {
    int i:3;
    const int j:4;
  } s;
  int k = true ? s.i : s.j;
