//options_all:-r -x -tused
//options: --strict;cp

  struct S {
    const int i, j;
  };
  S s = { 1 };        // spurious error is no longer issued -- s.j is
                      //   now initialized to zero

