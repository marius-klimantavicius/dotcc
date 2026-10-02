//options_all:-r -x -tused
//options: --strict;cp

extern "C" {
  class A {
    void f();
    inline void g();
    static void h();
  };
  typedef void FUNC_c();
  class B {
    FUNC_c f;
    inline FUNC_c g;
    static FUNC_c h;
  };
}

