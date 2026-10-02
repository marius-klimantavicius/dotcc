//options_all:-r -x -tused
//options: --strict;cp

class A { void f (); void g(); };
class B { static int i; friend class A; };
void A::f() {
  B::i = 0;
}


