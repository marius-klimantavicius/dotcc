//options_all:-r -x -tused
//options: --strict;cn:;rp

// All functions should be extern.
extern void a();
main() {
  extern void b();
  class A {
    friend void a();
    friend void b();
    friend void c();
    friend void d();
    friend void e();
  };
  extern void c();
}
extern void d();

