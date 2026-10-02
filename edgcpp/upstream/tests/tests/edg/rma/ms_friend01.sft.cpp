//options_all:-r -x -tused
//options: --microsoft_version=1400 -n;cn

struct S {
  friend static void a();
  friend extern void b();
  friend auto void c();
  friend inline static void d();
  friend inline extern void e();
  friend inline auto void f();
};
struct X {
  void a();
};
struct SS {
  friend static void X::a();
};

