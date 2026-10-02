//type:fp
//options_all:--c++20 -tused -A -w
  template<class T, T::type n = 0> class X;
  struct S {
    using type = int;
  };
  using T5 = X<S>;            // OK
