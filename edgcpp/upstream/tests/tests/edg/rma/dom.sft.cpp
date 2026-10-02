//options_all:-r -x -tused
//options: --strict;cp

class A { public: int i,j; };
class X : virtual public A { public: float i; };
class Y : virtual public A { public: float j; };
class B : public X, public Y { void f(); };
void B::f() {
  A::i = 0;  // int
  X::i = 0;  // float
  Y::i = 0;  // int
  B::i = 0;  // float
  i = 0;     // float
  A::j = 0;  // int
  X::j = 0;  // int
  Y::j = 0;  // float
  B::j = 0;  // float
  j = 0;     // float
}

