//type: fp
//options: 
# 0 "./lto/20101020-1_0.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./lto/20101020-1_0.C"


# 1 "./lto/20101020-1_0.h" 1
struct A;
typedef void (A::*Am1) (void *);
typedef void (A::*Am2) ();

struct B
{
  Am2 am2;
};

struct A
{
  A ();
  struct B b;
  struct C *c;
  struct D *d;
  void foo (Am1);
  void bar (void *);
};

struct C
{
};
# 4 "./lto/20101020-1_0.C" 2
A::A ()
{
  foo (&A::bar);
}
int main() { return 0; }
