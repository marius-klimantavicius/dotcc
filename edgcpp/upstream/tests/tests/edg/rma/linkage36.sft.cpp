//options_all:-r -x -tused
//options: --strict;cn:;cp

namespace N {
  class A {
    class X {
      void p();
      void q() {
        extern void a();  // N::a
      }
    };
    class Y;
    class Z;
  };
  class A::Y {
    void p();
    void q() {
      extern void b();   // N::b
    }
  };
  void A::X::p() {
    extern void c();     // N::c
  }
}
void N::A::Y::p() {
  extern void d();       // ::d or N::d?  (we say ::d)
}
class N::A::Z {
  void p();
  void q() {
    extern void e();     // ::e or N::e?  (we used to say N::e)
  }
};
void N::A::Z::p() {
  extern void f();       // ::f or N::f?  (we say ::f)
};


