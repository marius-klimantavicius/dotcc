//options_all:-r -x -tused
//options: --microsoft_version=1200 -n;cn

void f() {
  extern class X *p;
  class Y { int i; };
  extern class Y *q;
}
void g() {
  extern void x(class X *p);
  class Y { int i; };
  extern void y(class Y *q);
  x(0);
  y(0);
}

