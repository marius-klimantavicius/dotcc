//options_all:-r -x -tused
//options: --strict;cn

class A {
public:
  A();
private:
  A(const A&);
};

class B;

class C {
public:
  C();
  B f();
};

class B : A {
public:
  B();
  B(const C&);
#ifdef CCTOR
private:
  B(const B&);
#endif
};

B C::f() {
  return B(*this);
}

