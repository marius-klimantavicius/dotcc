//type: fp
//options: 
//options_all: --gnu_version=80200 -tused -e 200 --no_wrap
/* { dg-do compile { target i?86-*-* x86_64-*-* } } */
/* { dg-require-ifunc "" } */

struct a
{
  __attribute__((target_clones("sse", "default"))) void operator^=(a) {}
} * b;

class c {
public:
  a *d();
};

class f {
  void g();
  c e;
};

void f::g() { *e.d() ^= b[0]; }
