//options_all:-r -x -tused
//options: --strict;cp

class X {};
class Y {};
class A {
  virtual A& operator=(A&) throw(X);
};
class B {
  virtual B& operator=(B&) throw(X,Y);
};
class C {
  virtual C& operator=(C&) throw();
};
class D {
  virtual D& operator=(D&);
};
class Q1 : public A { };            // impl: Q1& operator=(Q1&) throw(X)
class Q2 : public B { };            // impl: Q2& operator=(Q2&) throw(X,Y)
class Q3 : public A, public B { };  // impl: Q3& operator=(Q3&) throw(X,Y)
class Q4 : public C { };            // impl: Q4& operator=(Q4&) throw()
class Q5 : public D { };            // impl: Q5& operator=(Q5&)
class Q6 : public B, public D { };  // impl: Q6& operator=(Q6&)
class Q7 : public A, public C { };  // impl: Q7& operator=(Q7&) throw(X)
class Q8 : public C, public D { };  // impl: Q8& operator=(Q8&)
class Q9 : public A {
  B b;
};                                  // impl: Q9& operator=(Q9&) throw(X,Y)
class Q10 : public A {
  C c;
};                                  // impl: Q10& operator=(Q10&) throw(X)
class Q11 : public A {
  D d;
};                                  // impl: Q11& operator=(Q11&)
class Q12 : public B {
  A a;
};                                  // impl: Q12& operator=(Q12&) throw(X,Y)
class Q13 : public B {
  D d;
};                                  // impl: Q13& operator=(QX13)

