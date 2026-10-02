//remark:Static data member initializer instantiation
//options:--c++17 --clang;fp:--c++17 --gnu=70300;fp:--c++17;fn

  template<typename T> struct S {
    static inline int value = T::f();
  };
  struct X: public S<X> {         // Previously an error.  Now okay in GNU and
    static int f() { return 0; }  // Clang C++17 modes.  
  };
