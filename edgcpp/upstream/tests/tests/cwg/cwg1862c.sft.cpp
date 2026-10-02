//type:fn
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
    template<class T> friend void A<T>::D::g();   // does not grant friendship to A<int>::D::g()
                                                  // because A<int>::D is not a specialization of A<T>::D ill-formed, A<T>::D does not end with a simple-template-id
  };
