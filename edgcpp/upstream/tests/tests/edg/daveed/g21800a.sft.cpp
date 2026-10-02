//remark:Braced GNU vector init
//options:--c++14 --gnu=70300;fp

  typedef float V __attribute__((__vector_size__(16)));
  struct S { operator V(); };
  V av[1] = { S() };  // (1) Previously an error.  Now okay.
  V v = { av[0] };    // (2) Previously an error.  Now okay.
