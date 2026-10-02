//type:fn
//options_all:--c++20 -tused -A
  template<typename T> struct end { bool f(); };
  int f();
  template<typename T> bool Foo(T it) {
    return it.end<int()>::f();
  }
  struct X { int end; void f(); };
  struct Y : end<int()> { void f(); };
  bool x = Foo(X());
  bool y = Foo(Y());
