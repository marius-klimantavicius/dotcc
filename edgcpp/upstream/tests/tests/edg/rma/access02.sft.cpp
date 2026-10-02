//options_all:-r -x -tused
//options: --strict;cn

// From ARM p. 243

class B {
  public:
    static void f();
    void g();
};
class D : private B {};
class DD : public D {
  void h();
};
void DD::h() {
  B::f();          // okay
  this->f();       // no access
  this->B::f();    // no access
  f();             // no access
  g();             // no access
}

