//options_all:-r -x -tused
//options: --strict;cn

class A { static int i; friend void f(); };
class B { static int j; };
void f() {
  A::i = 0;
  B::j = 0;
  B::k = 0;
  C::k = 0;
}
class C { friend void A::f(); };


