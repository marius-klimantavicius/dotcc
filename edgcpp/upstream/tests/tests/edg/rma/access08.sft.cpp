//options_all:-r -x -tused
//options: --strict;cn

// C++PL, p. 214
//
//    X           X           X
//    |(publ)     |(prot)     |(priv)
//    Y1          Y2          Y3
//                |(publ)
//                Z2
//
class X { public: int a; };
class Y1 : public X {};
class Y2 : protected X {};
class Y3 : private X {
  void f(Y1* py1, Y2* py2, Y3* py3);
};
class Z2 : public Y2 {
  void f(Y1* py1, Y2* py2, Y3* py3);
};
void f(Y1* py1, Y2* py2, Y3* py3) {
  X* px = py1;
  py1->a = 7;
  px = py2;      // cast error
  py2->a = 7;    // inaccessible
  px = py3;      // cast error
  py3->a = 7;    // inaccessible
}
void Z2::f(Y1* py1, Y2* py2, Y3* py3) {
  X* px = py1;
  py1->a = 7;
  px = py2;
  py2->a = 7;
  px = py3;      // cast error
  py3->a = 7;    // inaccessible
}
void Y3::f(Y1* py1, Y2* py2, Y3* py3) {
  X* px = py1;
  py1->a = 7;
  px = py2;      // cast error
  py2->a = 7;    // inaccessible
  px = py3;
  py3->a = 7;
}

