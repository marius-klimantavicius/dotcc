//type:fn
//options_all:--c++17 -tused -A -w
  template<class T, T::type n = 0> class X;
  struct S {
    using type = int;
  };
  using T4 = X<int>;          // error: substitution failure for second template parameter
