//options_all:-r -x -tused
//options: --microsoft_version=1400 -n;cp

__declspec(dllimport) inline void f(struct S *p) { }
struct A {
  struct S *p;
} a;
struct S { static void g(); };
main() {
  a.p->g();
}

