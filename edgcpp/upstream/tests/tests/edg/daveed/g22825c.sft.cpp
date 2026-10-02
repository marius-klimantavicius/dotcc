//remark:tpck_expression management
//options:--c++14 --gnu=70500;fp

  template<typename T> T f(T*);
  template<int I> void g(int *x) {
    f<int>(&x[I+1]);  // Triggered an abort in version 6.1.
  }
