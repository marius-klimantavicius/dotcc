//options_all:-r -x -tused
//options: --strict;cn

class A { public: int a, b, c; };
class B : private A { public: int x; A::a; A::b; void f(); };
class C : private B { public: B::a; B::x; void f(); };
class D : private C { void f(); };
void B::f() {
  x = 0;
  a = 0;
  A::a = 0;
  b = 0;
  A::b = 0;
  c = 0;
  A::c = 0;
}
void C::f() {
  x = 0;
  B::x = 0;
  a = 0;
  B::a = 0;
  A::a = 0;
  b = 0;
  B::b = 0;
  A::b = 0;
  c = 0;     // inaccessible
  B::c = 0;  // inaccessible
  A::c = 0;  // inaccessible
}
void D::f() {
  x = 0;
  C::x = 0;
  B::x = 0;
  a = 0;
  C::a = 0;
  B::a = 0;
  A::a = 0;
  b = 0;     // inaccessible
  C::b = 0;  // inaccessible
  B::b = 0;  // inaccessible
  A::b = 0;  // inaccessible
}
  

