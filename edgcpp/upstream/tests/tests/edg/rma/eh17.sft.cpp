//options_all:-r -x -tused
//options: --strict;cn

class X {
  virtual void f1();
  virtual void f2() throw(int,char);
  virtual void f3() throw(int);
  virtual void f4() throw();
};
class A : public X {
  void f1();
  void f2();                            // Error
  void f3();                            // Error
  void f4(int);
  void f4();                            // Error
};
class B : public X {
  void f1() throw(int,char,short);
  void f2() throw(int,char,short);      // Error
  void f3() throw(int,char,short);      // Error
  void f4(int);
  void f4() throw(int,char,short);      // Error
};
class C : public X {
  void f1() throw(int,char);
  void f2() throw(int,char);
  void f3() throw(int,char);            // Error
  void f4(int);
  void f4() throw(int,char);            // Error
};
class D : public X {
  void f1() throw(int);
  void f2() throw(int);
  void f3() throw(int);
  void f4(int);
  void f4() throw(int);                 // Error
};
class E : public X {
  void f1() throw();
  void f2() throw();
  void f3() throw();
  void f4(int);
  void f4() throw();
};

