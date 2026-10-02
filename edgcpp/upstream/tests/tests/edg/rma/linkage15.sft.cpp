//options_all:-r -x -tused
//options: --strict;cn

extern "C" void f() { }
extern "C" void f(int);
namespace N {
  extern "C" void f(int) { }
  extern "C" void g();
  extern "C" void h();
}
extern "C" void g();
namespace M {
  extern "C" void f();
  extern "C" void g();
  extern "C" void h();
}
extern "C" void h();


