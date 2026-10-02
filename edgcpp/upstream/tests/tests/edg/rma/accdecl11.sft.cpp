//options_all:-r -x -tused --set_flag=no_checking_pragmas
//options: --cfront_3.0;cp

// Access adjustment on hidden member of an indirect base class.  Is it
// legal?  What does it mean?
class A {
  public:
    int i;
};
class B : public A {
  public:
    int i;
};
class C : private B {
  public:
    A::i;       // Maybe this should be illegal.
    void f() 
    {
      A::i = 1;
      B::i = 2;
    }
};
main () {
  C c;
  c.f();
  int n = c.i;  // Does n == 1 (==> access adjustment is "stronger" than
                // hiding) or 2 (==> hiding is "stronger")?  cfront 2.1 says
                // the latter, but the ARM doesn't give us any help.
}

