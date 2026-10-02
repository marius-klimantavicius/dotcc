//options_all:-r -x -tused
//options: --strict;cn

// Taken from section 11-3 of the draft standard
class A {
  public:
    int z;
};
class B : private A {
  public:
    int a, x;
  private:
    int b;
  protected:
    int c;
};
class D : private B {
  public:
    B::a;  // make "a" a public member of D
    B::b;  // error
    A::z;  // error
  protected:
    B::c;  // make "c" a protected member of D
    B::x;  // error
};
class E : protected B {
  public:
    B::a;  // make "a" a public member of E
};

