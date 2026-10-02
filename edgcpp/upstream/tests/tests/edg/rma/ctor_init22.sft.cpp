//options_all:-r -x -tused
//options: --strict;cn

// Class with ill-formed user-defined default constructor
struct A {
  const int i;
  A() { }             // Error
};

// Non-POD class with nontrivial implicitly-defined default constructor
struct B {
  const int i;
  virtual void f();
};
B b;                  // Error defining B::B()

// Non-POD aggregate class with trivial default constructor
struct C {
  const int i;
  C& operator=(const C&);
};
C c1;                 // Error defining C::C()
C c2 = { 0 };         // Okay because C is an aggregate
C c3 = { };           // Ditto, and c3.i is default-initialized

// POD class with trivial default constructor
struct D {
  const int i;
};
D d1;                 // Error??? Or okay with uninitialized d1.i?
D d2 = { 0 };         // Okay because D is an aggregate
D d3 = { };           // Ditto, and d3.i is default-initialized

