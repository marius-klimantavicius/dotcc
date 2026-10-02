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

// Using non-qualified name may be required in Microsoft mode if x is a
// private member of C?
void A::B::C::f() {
  ::x = 0;
  A::x = 0;
  A::B::x = 0;
  x = 0;                 // = A::B::C::x
  A::B::C::D::x = 0;
}

