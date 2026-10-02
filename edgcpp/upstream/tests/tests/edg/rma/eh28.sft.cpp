//options_all:-r -x -tused
//options: --strict;cp

class X {};
class Y {};
class A {
  A(A&) throw(X);
};
class B {
  B(B&) throw(X,Y);
};
class C {
  C(C&) throw();
};
class D {
  D(D&);
};
class Q1 : public A { };            // implicit decl: Q1(Q1&) throw(X)
class Q2 : public B { };            // implicit decl: Q2(Q2&) throw(X,Y)
class Q3 : public A, public B { };  // implicit decl: Q3(Q3&) throw(X,Y)
class Q4 : public C { };            // implicit decl: Q4(Q4&) throw()
class Q5 : public D { };            // implicit decl: Q5(Q5&)
class Q6 : public B, public D { };  // implicit decl: Q6(Q6&)
class Q7 : public A, public C { };  // implicit decl: Q7(Q7&) throw(X)
class Q8 : public C, public D { };  // implicit decl: Q8(Q8&)
class Q9 : public A {
  B b;
};                                  // implicit decl: Q9(Q9&) throw(X,Y)
class Q10 : public A {
  C c;
};                                  // implicit decl: Q10(Q10&) throw(X)
class Q11 : public A {
  D d;
};                                  // implicit decl: Q11(Q11&)
class Q12 : public B {
  A a;
};                                  // implicit decl: Q12(Q12&) throw(X,Y)
class Q13 : public B {
  D d;
};                                  // implicit decl: Q13(QX13)

