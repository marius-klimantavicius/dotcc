//options_all:-r -x -tused
//options: --strict;cn

class A {
public:
  const int i;
  int& j;
  A() {}               // Warnings on user-defined default constructor
  A(const A&) {}       // No error on user-defined copy constructor
} a;
A x = a;

class B {
public:
  const int i;
  int& j;
  virtual void f();    // Force generation of B::B()
} b;                   // Warnings on compiler-generated default constructor
B y = b;               // No error on compiler-generated copy constructor

