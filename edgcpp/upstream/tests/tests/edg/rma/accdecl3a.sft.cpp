//options_all:-r -x -tused
//options: --strict;cn

class A { public: int a; };
class B : private A { public: A::a; };
class C : private B { public: void f(); };
void C::f() {
  a = 0;
  B::a = 0;
  A::a = 0;
}

