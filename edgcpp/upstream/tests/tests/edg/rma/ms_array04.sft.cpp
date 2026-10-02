//options_all:-r -x -tused
//options: --microsoft -n;cp

  struct B { int i; };
  struct S : B {
    S();
  private:
    int a, b, c[];
    virtual void f();
  };

