//options_all:-r -x -tused
//options: --strict;cn

static union {
  int i, j;
  template <class T> class X { };
};
class A {
  union {
    int i, j;
    template <class T> class X { };
  };
};


