//options_all:-r -x -tused
//options: --strict;cp

template <class T> struct A {
  struct N {
    N(T = 0) { }
  };
};
A<int>::N a;

