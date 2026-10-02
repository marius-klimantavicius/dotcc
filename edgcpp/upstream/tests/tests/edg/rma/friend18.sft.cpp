//options_all:-r -x -tused
//options: --strict;cn

// Proof that access to a base class depends on access to intermediate
// steps.
class A {public: int i;};
class B : private A {friend void f();};
class BB : private B {friend void f();};
class C : public BB {};
void f() {
  C *pc;
  A *pa;
  pc->i = 1;  // okay
  pa = pc;    // okay
}
void g() {
  C *pc;
  A *pa;
  pc->i = 1;  // error
  pa = pc;    // error
}

