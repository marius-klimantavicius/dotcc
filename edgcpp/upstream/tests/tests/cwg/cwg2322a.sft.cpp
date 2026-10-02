//type:fp
//options_all:--c++17 -tused -w -A
  template <class T> struct A { using X = typename T::X; };
  template <class T> typename T::X f(typename A<T>::X);
  template <class T> void f(...) { }


  void x() {
    f<int>(0);    // OK, substituting return type causes deduction to fail
  }

//cwg: 2322
//title: Substitution failure and lexical order
//meeting: Rapperswil 6/18
//edg_status: Passes
