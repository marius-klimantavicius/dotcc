//remark:Empty pack expansions and overload resolution
//options:--microsoft_v=1920;fn:--clang_v=50000;fn

  template<typename A, typename ...T> void f(T..., A);

  void test_f() { 
    f<int, int, int>(0, 0); // no matching overload
  }

