//options_all:-r -x -tused
//options: --strict;cp

class C {
  C() { }
  C(int) { }
  ~C() {}
public:
  static C *pc;  // (1a)
  static C c1;   // (2a)
  static C c2;   // (3a)
};

C *C::pc = new C;  // (1b)
C C::c1;           // (2b)
C C::c2(1);        // (3b)


