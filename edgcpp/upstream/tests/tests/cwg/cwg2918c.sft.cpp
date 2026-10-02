//type:fn
//options_all:--c++23 -A
  template <typename T>
  void f(T &&, void (*)(T &&));

  void g(int &);              // #1
  inline namespace A {
    void g(short &&);         // #2
  }
  inline namespace B {
    void g(short &&);         // #3
  }

  void q() {
    int x;
    f(x, g);         // ill-formed; previously well-formed, deducing T=int&
  }
