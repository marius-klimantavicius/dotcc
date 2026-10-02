//type:fp
//options_all:--c++17 -tused -A
template<class T> struct A {
    typedef int M;
    struct B {
      typedef void M;
      struct C;
    };
  };

  template<class T> struct A<T>::B::C : A<T> {
    M m; // OK, A<T>::M
  };

//cwg: 591
//title: When a dependent base class is the current instantiation
//meeting: Urbana-Champaign 11/14
//edg_status: EDGcpfe/19871
