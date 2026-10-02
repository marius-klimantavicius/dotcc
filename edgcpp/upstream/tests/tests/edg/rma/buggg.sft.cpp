//options_all:-r -x -tused
//options: --strict;cn:;rp

main() {
  struct B {
    int i;
    ~B() { }
  };
  struct D : B {
    int i;
  };
  D d;
}

