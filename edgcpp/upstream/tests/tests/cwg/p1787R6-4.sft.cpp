//type:fp
//options_all:--c++20 -tused -A
struct B { };
namespace N {
  typedef void V;
  template<class T> struct A : B {
    typedef void C;
    void f();
    template<class U> void g(U);
  };
}

template<class V> void N::A<V>::f() {  // N::V not considered here
  V v;              // V is still the template parameter, not N::V
}

template<class B> template<class C> void N::A<B>::g(C) {
  B b;              // the base class, not the template parameter
  C c;              // the template parameter C, not A's C
}
