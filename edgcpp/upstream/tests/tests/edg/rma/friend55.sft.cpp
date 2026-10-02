//options_all:-r -x -tused
//options: --strict;cn:;cn

void f() {
  extern void a();
  int b;
  struct S {
    friend void a();
    friend void a(int); // error
    friend void b();    // error
    friend void c();    // error
  };
  {
    struct SS {
      friend void a();  // error
    };
  }
}
int x;
struct S {
  friend void x();      // error
};

