//options_all:-r -x -tused
//options: --microsoft_version=1400 -n;cn

class A {
  static friend void a();
  friend static void b();
  extern friend void c();
  friend extern void d();
  friend typedef void e();
  typedef friend void f();
  friend mutable void g();
  mutable friend void h();
  friend auto void i();
  auto friend void j();
  friend register void k();
  register friend void l();
};



