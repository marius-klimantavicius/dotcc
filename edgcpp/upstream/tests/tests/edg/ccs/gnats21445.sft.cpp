//type:cp
//options::-DNEG;fn
//options_all:--c++17 -tused

template<int I = [] {
  struct A {
    int&& x = 29;
  };
  new A;
  return 0;
}()>
int f();

void g() {
  f<1>();
#if NEG
  f<>();
#endif /* NEG */
}
