//type:fn
//options_all:--c++17 -tused -A
//
  struct N {
  protected:
    static int m;
  };
  int R() { return N::m; }

//cwg: 1873
//title: Protected member access from derived class friends
//meeting: Lenexa 5/15
//edg_status: Passes
