//options_all:-r -x -tused
//options: --strict;cn

class B { };
class D : public B { };
class X {
  virtual void f() throw(B);
  virtual void g() throw(D);
};
class Y : public X {
  virtual void f() throw(D);    // Okay
  virtual void g() throw(B);    // Error
};

