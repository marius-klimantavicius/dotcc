//options_all:-r -x -tused
//options: --strict;cp

int x;
namespace A {
  int x;
  namespace B {
    int x;
    struct C {
      static int x;
      struct D {
        static int x;
        void f();
      };
      void f();
    };
    void f();
  }
  void f();
}

// Using non-qualified name may be required when D is not public or
// (as above) in Microsoft mode
void A::B::C::D::f() {
  ::x = 0;
  A::x = 0;
  A::B::x = 0;
  A::B::C::x = 0;
  x = 0;                 // = A::B::C::D::x
}

