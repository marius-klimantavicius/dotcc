//options_all:-r -x -tused
//options: --strict;cn:;cp

struct A {
  A() { }
  virtual ~A() { }
  virtual void f() { }
};
struct B : public A {
  virtual void f() { }
};
B *bp = new B;
struct C {
  struct CN : public A {
    virtual void f() { }
  };
};
C::CN *cp = new C::CN;
void g() {
  struct BB : public A {
    virtual void f() { }
  };
  struct CC {
    struct CCN : public A {
      virtual void f() { }
    };
  };
  BB *bp = new BB;
  CC::CCN *cp = new CC::CCN;
};

