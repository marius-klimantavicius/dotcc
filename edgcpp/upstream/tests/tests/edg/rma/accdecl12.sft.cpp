//options_all:-r -x -tused
//options: --cfront_3.0;cn:;cn

class A {
  public:
    int i, j;
};
class B :  private A {};
class C : private B {
  public:
    A::i;         // Access adjustment on a member of an indirect base class
                  // which would otherwise be inaccessible -- error to cfront
    void f() 
    {
      i = 1;      // cfront: no error
      A::i = 2;   // cfront: no error
      B::i = 3;   // cfront: no error
      j = 1;      // cfront: base class access error
      A::j = 2;   // cfront: base class access error
      B::j = 3;   // cfront: base class access error
    }
};
main () {
  C c;
  c.i = 0;        // cfront: no error
  c.A::i = 0;     // cfront: no error
  c.B::i = 0;     // cfront: no error
  c.j = 0;        // cfront: base class access error
  c.A::j = 0;     // cfront: base class access error
  c.B::j = 0;     // cfront: base class access error
}

