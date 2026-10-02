//options_all:-r -x -tused
//options: --strict;cp

struct A { A(); ~A(); };
void f() {
  static A x;
  {
    static A y;
    {
      static A z;
    }
  }
}

