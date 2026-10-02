//options_all:-r -x -tused
//options: --strict;cn

namespace N1 {
  extern "C" int x;
  extern "C" void f() {
    extern int y;
    extern void g(int);
    ++x;
    g(y);
  }
}
namespace N2 {
  extern "C" int x;                 // conflicts with N1::x
  extern "C" void f() {             // conflicts with N1::f
    extern int y;                   // conflicts with N1::y
    extern void g(int);             // conflicts with N1::g
    ++x;
    g(y);
  }
}
extern "C" int x;                   // conflicts with N1::x, N2::x
extern "C" void g() {               // conflicts with N1::g, N2::g
  extern void f();                  // conflicts with N1::f, N2::f
  extern int y;                     // conflicts with N1::y, N2::y
  extern int z;
}
namespace N3 {
  extern "C" int z;                 // conflicts with ::z
  extern "C" void f(int) {          // conflicts with N1::f, N2::f, ::f
    extern void g(int);             // conflicts with N1::g, N2::g, ::g
    g(z);
  }
}

