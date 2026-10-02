//remark:Abbreviated member templates
//options:--c++20;fp

template<typename T> struct S {
  template<typename U> struct N {
    void f1(auto);
    template<typename> void f2(auto);
  };
};
template<typename T> template<typename U> void S<T>::N<U>::f1(auto) {}
template<typename T> template<typename U>
   template<typename> void S<T>::N<U>::f2(auto) {}
