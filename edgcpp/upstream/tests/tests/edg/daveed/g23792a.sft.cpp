//remark:GNU C++17 non-constant SDM initializer
//options:--c++17 --gnu=50100;fp

  int f();
  template<typename T> struct S {
    static inline int const x = f();
  };
  int r = S<int>{}.x;  // Previously an error in GNU C++17 mode.  Now okay.
