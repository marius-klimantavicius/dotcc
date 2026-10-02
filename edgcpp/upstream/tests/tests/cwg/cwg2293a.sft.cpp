//type:fn
//options_all:--c++17 -tused -A -w
  template<class T, T::type n = 0> class X;
  struct S {
    using type = int;
  };
  using T1 = X<S, int, int>;  // error: too many arguments

//cwg: 2293
//title: Requirements for simple-template-id used as a class-name
//meeting: Rapperswil 6/18
//edg_status: Passes
