//remark:inline template static data members
//options:--c++17;cp

  struct S {
    template<int N> static inline int m = N;  // Previously triggered a
  };                                          // spurious error when
  int i = S::m<42>;                           // instantiated.

