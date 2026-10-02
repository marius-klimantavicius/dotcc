//remark:extern/static conflict
//options:--clang;fn:--clang --ms_ext;fp:--gcc --gnu=30400;fp:--gcc --gnu=40000;fn:--sun;fp:;fn:--microsoft;fp

  extern void g(void); 
  static void g(void) {}
