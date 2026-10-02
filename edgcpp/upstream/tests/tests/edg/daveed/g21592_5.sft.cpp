//remark:constinit
//options:--c++17;fn:--c++20;fp:--c++20 -DNEG;fn

void g() {
  static constinit int i = 42;
#ifdef NEG
  extern int f();
  static constinit int k = f();
#endif
}
