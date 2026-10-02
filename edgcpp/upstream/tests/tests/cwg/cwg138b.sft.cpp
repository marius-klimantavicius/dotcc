//type:fn
//options_all:--c++20 -tused -A
using I=int;
using D=double;
namespace A {
  inline namespace N {using C=char;}
  using F=float;
  void f(I);
  void f(D);
  void f(C);
  void f(F);
}
struct X0 {using F=float;};
struct W {
  using D=void;
  struct X : X0 {
    void g(I);
    void g(::D);
    void g(F);
  };
};
namespace B {
  typedef short I,F;
  class Y {
    friend void A::f(I);  // error: no void A::f(short)
  };
}
