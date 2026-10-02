//options_all:-r -x -tused
//options: --strict;cn:;ln

extern "C" void a();
namespace N {
  extern "C" void a();
  extern "C" void b();
  extern "C" {
    void f() {
      extern void a();
      extern void b();
      extern void c();
      extern void d();
      extern void e();
    }
  }
  extern "C" void c();
}
extern "C" void d();
namespace M {
  extern "C" void e();
}
main() {
  a();
  N::b();
  N::c();
  d();
  M::e();
}

