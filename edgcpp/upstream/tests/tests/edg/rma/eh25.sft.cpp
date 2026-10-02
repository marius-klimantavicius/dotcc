//options_all:-r -x -tused
//options: --strict;cn

class X {};
class Y {};
class A {
  virtual ~A() throw(X);
};
class B {
  virtual ~B() throw(Y);
};
class C1 : public A, public B {
  virtual ~C1();                   // error: not "at least as restrictive as"
};                                 //   A::~A() or B::~B()
class C2 : public A, public B {
  virtual ~C2() throw(X);          // error: not as restrictive as B::~B()
};
class C3 : public A, public B {
  virtual ~C3() throw(Y);          // error: not as restrictive as A::~A()
};
class C4 : public A, public B {
  virtual ~C4() throw();           // okay
};
class C : public A, public B {
  // implicitly declares virtual ~C() throw(X,Y), which produces an error
};

