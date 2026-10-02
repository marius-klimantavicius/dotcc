//options_all:-r -x -tused --set_flag=no_checking_pragmas
//options: --strict;cn:;rp

void f();
void g();
main() {
  extern void f();
  extern void g();
  class A {
    void ff() { extern void g(); g(); }
    friend void f();
  } a;
}

