//type:fn
//options_all:--c++20 -tused -A -w
template <class T> struct A { using X = typename T::X; };
template <class T> auto g(typename A<T>::X) -> typename T::X;
template <class T> void g(...) { }

  void x() {
  
        g<int>(0);    // error, substituting parameter type instantiates A<int>
  }
