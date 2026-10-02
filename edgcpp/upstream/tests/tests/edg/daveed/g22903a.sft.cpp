//remark:Ref binding (Core issue 2352)
//options:--c++20;fp:--microsoft_v=1927;fn:--microsoft_v=1928;fp
//options_all:--diag_error=430


  int *ptr;
  const int *const &f() {
    return ptr;
  }
