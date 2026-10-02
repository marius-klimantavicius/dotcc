//options_all:-r -x -tused --set_flag=no_checking_pragmas
//options: --strict;cn:;rp

main() {
  struct B {
    int i;
    B(int ii) : i(ii) { }
    int operator ++() { return i *= i; }
    operator unsigned int() { return '0' + i; }
    ~B() { --i; }
  };
  struct C : B {
    C(int ii) : B(ii) { }
    ~C() { --i; }
  };
  struct D : C {
    D(int ii) : C(ii + 1) { }
    ~D() { --i; }
  };
  D d (2);
  D &dr = d;
  C *pc = &d;
}
