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

// Using non-qualified name is not really necessary
void A::f() {
  ::x = 0;
  x = 0;                 // = A::x
  A::B::x = 0;
  A::B::C::x = 0;
  A::B::C::D::x = 0;
}

