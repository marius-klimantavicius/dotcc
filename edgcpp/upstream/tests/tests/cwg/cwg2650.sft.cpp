//type:fn
//options_all:--c++20 -tused
  template <class T, T> struct S {};
  template <class T> int f(S<T, T{}>*);   // #1
  class X {
    int m;
  };
  int i0 = f<X>(0);   // #1 uses a value of a non-structural type X as a non-type template argument

//cwg: 2650
//title: Incorrect example for ill-formed non-type template arguments
//meeting: Kona 11/22
//edg_status: Passes
