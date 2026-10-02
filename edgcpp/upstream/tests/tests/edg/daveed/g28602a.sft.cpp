//remark:Deducibility of parameters
//options:--c++17;fp:--c++14;fp

  template<typename> struct W {};
  template<int> struct X {
    enum { x };
    template<typename T> X(T);
  };
  template<typename T> void f(X<T::x>, W<T>);
  void g(W<X<1>> w) {
    f(1, w);
  }
