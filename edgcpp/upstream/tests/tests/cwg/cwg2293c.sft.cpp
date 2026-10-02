//type:fn
//options_all:--c++17 -tused -A -w
  template<class T, T::type n = 0> class X;
  struct S {
    using type = int;
  };
  using T3 = X<1>;            // error: value 1 does not match type-parameter
