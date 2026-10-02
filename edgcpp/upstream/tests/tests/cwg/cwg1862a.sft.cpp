//type:fp
//options_all:--c++17 -tused -A
  template<class T> struct A {
    struct B { };
    void f();
    struct D {
      void g();
    };
    T h();
    template<T U> T i();
  };
  template<> struct A<int> {
    struct B { };
    int f();
    struct D {
      void g();
    };
    template<int U> int i();
  };
  template<> struct A<float*> {
    int *h();
  };
  class C {
    template<class T> friend struct A<T>::B;      // grants friendship to A<int>::B even though
                                                  // it is not a specialization of A<T>::B
    template<class T> friend int *A<T*>::h();     // grants friendship to A<int*>::h() and A<float*>::h()
    template<class T> template<T U>               // grants friendship to instantiations of A<T>::i() and to A<int>::i()
      friend T A<T>::i();                         // and thereby to all specializations of those function templates
  };

//cwg: 1862
//title: Determining “corresponding members” for friendship
//meeting: Albuquerque 11/17
//edg_status: EDGcpfe/20149
