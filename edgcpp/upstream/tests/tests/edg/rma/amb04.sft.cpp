//options_all:-r -x -tused
//options: --strict;cn

// From ARM pp. 202-3
class A {
  public:
    int a;
    int (*b) ();
    int f();
//  int f(int);
    int g();
};

class B {
    int a;
    int b();
  public:
    int f();
    int g;
    int h();
//  int h(int);
};

class C : public A, public B {};

void g(C* pc)
{
    pc->a = 1;   // ambiguous
    pc->b();     // ambiguous
    pc->f();     // ambiguous
//  pc->f(1);    // ambiguous
    pc->g();     // ambiguous
    pc->g = 1;   // ambiguous
    pc->h();     // okay
//  pc->h(1);    // okay
}

