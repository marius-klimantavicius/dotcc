//options_all:-r -x -tused
//options: --strict;ln

  // file c.c
  class A *pa;
  extern int f(A *);
  int main() {
    return f(pa);
  }

