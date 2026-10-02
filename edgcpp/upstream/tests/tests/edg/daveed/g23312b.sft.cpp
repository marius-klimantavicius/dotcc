//remark:Nested types and current instantiation
//option:--c++14 --parse --gnu=70300;fp

template<typename T> struct S {
  struct N {
    template<typename U> static void h() {
      S<T>::n.f<U>();
    }
    template<typename> void f();
  };
  static N n;
};

