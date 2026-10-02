//options_all:-r -x -tused
//options: --strict;cn:;cp

class X {};
class Y {};
class A {
  virtual ~A() throw(X);
};
class B {
  virtual ~B() throw(X,Y);
};
class C {
  virtual ~C() throw();
};
class D {
  virtual ~D();
};
class Q1 : public A { };            // implicit decl: virtual ~Q1() throw(X)
class Q2 : public B { };            // implicit decl: virtual ~Q2() throw(X,Y)
class Q3 : public A, public B { };  // implicit decl: virtual ~Q3() throw(X,Y) Error
class Q4 : public C { };            // implicit decl: virtual ~Q4() throw()
class Q5 : public D { };            // implicit decl: virtual ~Q5()
class Q6 : public B, public D { };  // implicit decl: virtual ~Q6()
class Q7 : public A, public C { };  // implicit decl: virtual ~Q7() throw(X)
class Q8 : public C, public D { };  // implicit decl: virtual ~Q8()
class Q9 : public A {
  B b;
};                                  // implicit decl: virtual ~Q9() throw(X,Y)
class Q10 : public A {
  C c;
};                                  // implicit decl: virtual ~Q10() throw(X)
class Q11 : public A {
  D d;
};                                  // implicit decl: virtual ~Q11()
class Q12 : public B {
  A a;
};                                  // implicit decl: virtual ~Q12() throw(X,Y)
class Q13 : public B {
  D d;
};                                  // implicit decl: virtual ~Q13()

