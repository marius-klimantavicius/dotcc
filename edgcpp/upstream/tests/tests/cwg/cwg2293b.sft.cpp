//type:fn
//options_all:--c++17 -tused -A -w
  template<class T, T::type n = 0> class X;
  struct S {
    using type = int;
  };
  using T2 = X<>;             // error: no default argument for first template parameter
