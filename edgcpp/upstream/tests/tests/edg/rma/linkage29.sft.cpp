//options_all:-r -x -tused
//options: --strict;cn:;rp

  void f() { extern main(); }
  extern main();
  main() {}
  void g() { extern main(); }
  extern main();


