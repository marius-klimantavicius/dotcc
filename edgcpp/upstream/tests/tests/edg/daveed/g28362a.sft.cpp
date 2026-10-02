//remark:Partial specialization matching
//options:--c++11;fp

  template<typename> struct S {
    template<typename> struct N;
  };
  template<typename T> template<typename U> struct S<T>::N<U*> {};
  template<> template<typename W> struct S<int>::N<W*> {};

