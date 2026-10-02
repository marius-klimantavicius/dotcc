//type: fp
//options: 
# 0 "./torture/pr60438-2.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./torture/pr60438-2.C"


# 1 "./torture/pr60438-1.C" 1



struct A { int a; };
struct B { A foo (); };
struct C { B *foo (); };
int foo (struct C *, float);
void bar (struct C *);
void baz (struct A *);
int a, b, c;

int
foo (struct C *y, float x)
{
  struct A d;
  if (c)
    bar (y);
  else
    {
      C g;
      g.foo ()->foo ();
      a = b;
      d.a = (int) (b * x);
    }
  baz (&d);

  return 0;
}
# 4 "./torture/pr60438-2.C" 2
