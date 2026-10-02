//type: fp
//options: 
# 0 "./compat/init/byval1_y.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./compat/init/byval1_y.C"
# 1 "./compat/init/byval1.h" 1
struct C
{
  int m;
  C();
  ~C();
};
# 2 "./compat/init/byval1_y.C" 2

void *p[2];

int i;
int r;

C::C() { p[i++] = this; }
C::~C() { if (p[--i] != this) r = 1; }

void Foo (C c)
{
  p[i++] = &c;
}
