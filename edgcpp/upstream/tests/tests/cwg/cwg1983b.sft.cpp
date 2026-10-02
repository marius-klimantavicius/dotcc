//type:fn
//options_all:--c++17 -tused -A
  template<typename T> struct C { virtual void f() {} };
  template void C<int>::f() final;
  template<> void C<char>::f() final;
