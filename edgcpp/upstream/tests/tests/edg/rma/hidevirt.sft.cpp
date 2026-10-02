//options_all:-r -x -tused
//options: --strict;cp

class A {
  virtual void f(int);
  virtual void f();
};
class AA {
  virtual void f(int);
  virtual void f(int,int);
};
class B : public A, public AA {
  virtual void f(int);
};
class BB : public A, public AA { };
class C : public BB {
  virtual void f(int);
};

//class B : public A { };
//class C : public B {
//  int f;
//};

