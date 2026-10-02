//remark:const class-type members
//options:--c++20;fp:--c++20 --clang;fn

  struct I { int i = 42; };
  struct S {
    const I ci;
  };
  S s;  // Previously an error.  Now okay (except in Clang modes).
