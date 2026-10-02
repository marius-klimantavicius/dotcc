//type:fn
//options_all:--c++20 -tused -A
int f();
struct A {
  int B,C;
  template<int> using D=void;
  using T=void;
  void f();
};
using B=A;
template<int> using C=A;
template<int> using D=A;
template<int> using X=A;

template<class T>
void g(T *p) {            // as instantiated for g<A>:
  p->template C<0>::f();  // error: A::C is not a template
  p->template D<0>::f();  // error: A::D<0> is not a class type
  p->T::f();              // error: A::T is not a class type
}
template void g(A*);
