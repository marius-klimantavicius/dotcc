//type:fn
//options::-A
//options_all:--c++11 -tused

template<class T> struct B1 {
  template<class U> B1(U);
};
template<class T> struct B2 {
  template<class U> B2(U);
};

template<class T> struct D: B1<T>, B2<T> {
  using B1<T>::B1;
  using B2<T>::B2;
};

D<int> d(2.0);
