//options_all:-r -x -tused
//options: --strict;cn

struct X {
  static X x;
  static int f(X);
  X(const X&, int = f(x));
};


