//type:fn
//option_all:--c++23 -tused
  struct B {
    static void f();
  } y;
  void (*q)() = y.f;   // error
