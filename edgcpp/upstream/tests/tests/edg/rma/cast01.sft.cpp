//options_all:-r -x -tused
//options: --strict;cn

class A { int i; };
class B : private A { void f(); void g(A*); };
class C : private B { void f(); void g(A*); };
void B::f() {
  B b;
  g((A*)&b);
}
void C::f() {
  C c;
  g((A*)&c);
}

