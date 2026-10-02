//type:fp
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
  p->template X<0>::f();  // OK: ::X found in definition context
  p->B::f();              // OK: non-type A::B ignored
}
template void g(A*);

//cwg: 2065
//title: Current instantiation of a partial specialization
//meeting: Virtual 11/20*
//edg_status: EDGcpfe/23835
