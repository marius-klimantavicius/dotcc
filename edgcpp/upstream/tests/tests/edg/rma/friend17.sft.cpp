//options_all:-r -x -tused
//options: --strict;cp

// Innaccessible members can still be accessed in some cases
class A {
public: int i;
  friend class C;
};
class B : private A {
  friend class C;
};
class C : private B {
  void f();
};
void C::f() {
  A *pa;
  pa->i = 1;  // okay
  B *pb;
  pb->i = 1;  // okay
  i = 1;      // okay -- surprisingly
}

