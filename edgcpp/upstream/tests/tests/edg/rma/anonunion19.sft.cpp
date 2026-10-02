//options_all:-r -x -tused
//options: --strict;cn

static union {
  int i, j;
  template <class T> void f(T);
};
class A {
  union {
    int i, j;
    template <class T> void f(T);
  };
};


