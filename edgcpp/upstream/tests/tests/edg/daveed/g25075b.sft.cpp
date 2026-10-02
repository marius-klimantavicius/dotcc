//remark:auto(x)/auto{x}
//options:--c++23;fp:--c++23 -DNEG;fn

template<typename T>
  struct A {};
template<typename T>
  void f(A<T>&);  // #1
template<typename T>
  void f(A<T>&&); // #2
template<typename T>
  A<T>& g();
template<typename T>
  void h() {
    f(auto(g<T>()));
#ifdef NEG
    f(auto());
    f(auto(1, 1));
#endif
  }
