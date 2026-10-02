//options_all:-r -x -tused
//options: --strict;cn:;cn

static void xxx()
{
  struct B { int bi; };
  class D : private B {
    int di;
    friend void yyy(D &d) {};
  };
  D d;
  yyy(d);
}

