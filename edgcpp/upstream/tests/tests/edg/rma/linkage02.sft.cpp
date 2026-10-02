//options_all:-r -x -tused --set_flag=no_checking_pragmas
//options: --strict;cn:;ln

extern "C" {
  struct S {
    int a,b,c;
    friend void h();
    friend void f() { extern void g(); g(); h(); };
  };
}
main() { f(); }

