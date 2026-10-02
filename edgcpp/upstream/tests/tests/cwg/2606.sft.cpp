//options_all:--c++20 -tused -A
  struct S {
    int a[5];
  } s;
  int (*p)[] = reinterpret_cast<int(*)[]>(&s);
  int n = (*p)[0];
