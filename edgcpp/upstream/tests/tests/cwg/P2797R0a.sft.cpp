//options_all:--c++23 -tused -A
    struct S {
      int m;
    };
    int i = sizeof(S::m);           // OK
    int j = sizeof(S::m + 42);      // OK
