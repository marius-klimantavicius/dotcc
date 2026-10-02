//options_all:-r -x -tused
//options: --strict;cn

class B {
public:
  int mi;
  static int si;
};
class D : private B {
};
class DD : public D {
  void f();
};
void DD::f() {
  mi = 3;            // error -- inaccessible
  si = 3;            // error -- inaccessible
  B b;
  b.mi = 3;          // okay
  b.si = 3;          // okay
  B::si = 3;         // okay
  B* bp1 = this;     // error -- this cannot be implicitly cast to B
  B* bp2 = (B*)this; // okay -- explicit cast to B is allowed
  bp2->mi = 3;       // okay, even though b2->mi is same as this->mi
}

